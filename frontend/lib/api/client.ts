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

export async function apiFetch<T>(path: string, init?: RequestInit): Promise<T> {
  const baseUrl = typeof window === "undefined" ? env.apiUrl : env.publicApiUrl;
  const response = await fetch(`${baseUrl}/api/v1${path}`, {
    ...init,
    headers: { "Content-Type": "application/json", ...init?.headers },
  });

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null;

    throw new ApiError(
      response.status,
      problem?.detail ?? problem?.title ?? `Falha na requisição (${response.status}).`,
      problem?.errors,
    );
  }

  if (response.status === 204) return undefined as T;

  return response.json() as Promise<T>;
}
