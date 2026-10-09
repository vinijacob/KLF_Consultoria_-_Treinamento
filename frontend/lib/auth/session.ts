import { ApiError, apiBaseUrl, readResponse } from "@/lib/api/client";
import type { LoginResponse } from "@/types/admin";

/*
 * Sessão do painel. O access token (15 min) fica só na memória da aba; o refresh token mora no cookie HttpOnly
 * `klf_refresh`, que o JavaScript nunca lê. Ao recarregar a página, a sessão volta pelo `/auth/refresh`.
 *
 * O refresh nunca roda em paralelo: um refresh token usado duas vezes derruba todas as sessões (detecção de roubo).
 * Dentro da aba, chamadas simultâneas reaproveitam a mesma promessa; entre abas, o Web Locks API enfileira
 * (o cookie é compartilhado, então a segunda aba já envia o token novo).
 */

type Session = { accessToken: string; expiresAt: number };

const EXPIRY_MARGIN_MS = 30_000;

let session: Session | null = null;
let refreshing: Promise<Session | null> | null = null;
const expiredListeners = new Set<() => void>();

export function startSession(accessToken: string, expiresAt: string) {
  session = { accessToken, expiresAt: Date.parse(expiresAt) };
}

export function clearSession() {
  session = null;
}

export function onSessionExpired(listener: () => void) {
  expiredListeners.add(listener);
  return () => {
    expiredListeners.delete(listener);
  };
}

function expire() {
  session = null;
  expiredListeners.forEach((listener) => listener());
}

async function withLock<T>(task: () => Promise<T>): Promise<T> {
  if (typeof navigator !== "undefined" && "locks" in navigator) {
    return navigator.locks.request("klf-auth-refresh", task);
  }

  return task();
}

async function requestRefresh(): Promise<Session | null> {
  const response = await fetch(`${apiBaseUrl()}/auth/refresh`, { method: "POST", credentials: "include" });

  if (!response.ok) return null;

  const body = (await response.json()) as LoginResponse;
  if (!body.accessToken || !body.expiresAt) return null;

  startSession(body.accessToken, body.expiresAt);
  return session;
}

export function refreshSession(): Promise<Session | null> {
  refreshing ??= withLock(requestRefresh)
    .catch(() => null)
    .finally(() => {
      refreshing = null;
    });

  return refreshing;
}

async function accessToken(): Promise<string> {
  if (session && session.expiresAt - EXPIRY_MARGIN_MS > Date.now()) return session.accessToken;

  const renewed = await refreshSession();
  if (!renewed) {
    expire();
    throw new ApiError(401, "Sua sessão expirou. Entre novamente.");
  }

  return renewed.accessToken;
}

type AdminInit = Omit<RequestInit, "body"> & { json?: unknown; body?: BodyInit };

async function send(path: string, init: AdminInit, token: string) {
  const { json, headers, ...rest } = init;

  return fetch(`${apiBaseUrl()}${path}`, {
    ...rest,
    body: json === undefined ? rest.body : JSON.stringify(json),
    headers: {
      ...(json === undefined ? {} : { "Content-Type": "application/json" }),
      ...headers,
      Authorization: `Bearer ${token}`,
    },
  });
}

async function authorizedResponse(path: string, init: AdminInit): Promise<Response> {
  let response = await send(path, init, await accessToken());

  if (response.status === 401) {
    const renewed = await refreshSession();
    if (!renewed) {
      expire();
      throw new ApiError(401, "Sua sessão expirou. Entre novamente.");
    }
    response = await send(path, init, renewed.accessToken);
  }

  return response;
}

/** Chamada autenticada à API (`path` a partir de `/api/v1`). Renova o token sozinha quando preciso. */
export async function adminFetch<T>(path: string, init: AdminInit = {}): Promise<T> {
  return readResponse<T>(await authorizedResponse(path, init));
}

/** Baixa um arquivo autenticado (QR Code, cartaz em PDF) e devolve o conteúdo e o nome sugerido pela API. */
export async function adminDownload(path: string): Promise<{ blob: Blob; fileName: string | null }> {
  const response = await authorizedResponse(path, {});
  if (!response.ok) await readResponse(response);

  const disposition = response.headers.get("Content-Disposition") ?? "";
  const fileName = /filename\*?=(?:UTF-8'')?"?([^";]+)"?/i.exec(disposition)?.[1] ?? null;

  return { blob: await response.blob(), fileName: fileName && decodeURIComponent(fileName) };
}

/** Rotas de `/auth/*` que criam ou encerram a sessão: mandam e recebem o cookie. */
export async function authRequest<T>(path: string, body?: unknown): Promise<T> {
  const response = await fetch(`${apiBaseUrl()}/auth${path}`, {
    method: "POST",
    credentials: "include",
    headers: body === undefined ? undefined : { "Content-Type": "application/json" },
    body: body === undefined ? undefined : JSON.stringify(body),
  });

  return readResponse<T>(response);
}

export async function signOut() {
  clearSession();
  await authRequest("/logout").catch(() => undefined);
}
