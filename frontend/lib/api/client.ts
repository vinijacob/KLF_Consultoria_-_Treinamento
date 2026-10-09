import { env } from "@/lib/env";

export type ProblemDetails = {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
};

export class ApiError extends Error {
  constructor(
    public readonly status: number,
    message: string,
    public readonly errors?: Record<string, string[]>,
  ) {
    super(message);
    this.name = "ApiError";
  }
}

export function apiBaseUrl() {
  return `${typeof window === "undefined" ? env.apiUrl : env.publicApiUrl}/api/v1`;
}

export async function readResponse<T>(response: Response): Promise<T> {
  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null;

    throw new ApiError(
      response.status,
      response.status === 429
        ? "Muitas tentativas em pouco tempo. Aguarde um minuto e tente de novo."
        : (problem?.detail ?? problem?.title ?? `Falha na requisição (${response.status}).`),
      problem?.errors,
    );
  }

  if (response.status === 204 || response.status === 202) return undefined as T;

  return response.json() as Promise<T>;
}

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const response = await fetch(`${apiBaseUrl()}${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...init?.headers },
  });

  return readResponse<T>(response);
}

/** Primeira mensagem de erro de um campo, sem diferenciar maiúsculas (a API devolve `Title`, o formulário usa `title`). */
export function fieldError(errors: Record<string, string[]> | undefined, field: string) {
  if (!errors) return undefined;
  const key = Object.keys(errors).find((name) => name.toLowerCase() === field.toLowerCase());

  return key ? errors[key][0] : undefined;
}
