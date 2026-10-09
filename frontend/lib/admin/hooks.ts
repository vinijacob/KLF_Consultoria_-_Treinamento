"use client";

import { useCallback, useEffect, useState } from "react";
import { ApiError } from "@/lib/api/client";
import { adminFetch } from "@/lib/auth/session";

type Result<T> = { key: string; data?: T; error?: ApiError };

function asApiError(error: unknown) {
  return error instanceof ApiError ? error : new ApiError(0, "Não foi possível falar com o servidor. Confira a conexão.");
}

/**
 * Carrega um recurso da API. Enquanto recarrega, mantém o dado anterior na tela.
 * `path` nulo não carrega nada (ex.: formulário de criação).
 */
export function useApi<T>(path: string | null) {
  const [version, setVersion] = useState(0);
  const [result, setResult] = useState<Result<T> | null>(null);
  const key = `${path}#${version}`;

  useEffect(() => {
    if (!path) return;
    let active = true;

    adminFetch<T>(path).then(
      (data) => active && setResult({ key, data }),
      (error: unknown) => active && setResult({ key, error: asApiError(error) }),
    );

    return () => {
      active = false;
    };
  }, [path, key]);

  const samePath = result?.key.startsWith(`${path}#`) ?? false;

  return {
    data: samePath ? result?.data : undefined,
    error: samePath && result?.key === key ? result?.error : undefined,
    loading: path !== null && result?.key !== key,
    reload: useCallback(() => setVersion((current) => current + 1), []),
    replace: useCallback((data: T) => setResult({ key: `${path}#${version}`, data }), [path, version]),
  };
}

/** Envio de formulário: guarda erros por campo da API e a mensagem geral. */
export function useSubmit() {
  const [pending, setPending] = useState(false);
  const [errors, setErrors] = useState<Record<string, string[]> | undefined>();
  const [message, setMessage] = useState<string | null>(null);

  const run = useCallback(async <T,>(action: () => Promise<T>): Promise<T | undefined> => {
    setPending(true);
    setErrors(undefined);
    setMessage(null);

    try {
      return await action();
    } catch (error) {
      const apiError = asApiError(error);
      setErrors(apiError.errors);
      setMessage(apiError.errors ? "Confira os campos destacados." : apiError.message);
      return undefined;
    } finally {
      setPending(false);
    }
  }, []);

  return { pending, errors, message, run, setMessage };
}

/*
 * Recado de uma tela para a próxima (ex.: "Serviço criado." depois de redirecionar para a edição).
 * Vive só na memória: some ao recarregar a página.
 */
let pendingFlash: string | null = null;

export function setFlash(message: string) {
  pendingFlash = message;
}

/** Lê o recado deixado pela tela anterior (uma vez só). */
export function useFlash() {
  const [message] = useState(() => pendingFlash);

  useEffect(() => {
    pendingFlash = null;
  }, []);

  return message;
}
