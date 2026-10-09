import type { PostStatus } from "@/types/admin";

/** Só volta para dentro do painel (evita redirecionamento aberto para outro site). */
export function safeReturnPath(value: unknown) {
  return typeof value === "string" && value.startsWith("/painel") && !value.startsWith("//") && !value.includes("\\")
    ? value
    : "/painel";
}

const postStatuses: readonly string[] = ["Draft", "Scheduled", "Published"] satisfies PostStatus[];

export function isPostStatus(value: unknown): value is PostStatus {
  return typeof value === "string" && postStatuses.includes(value);
}
