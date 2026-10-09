import type { Role } from "@/types/admin";

/** Slug como a API aceita: minúsculas, números e hífens (`^[a-z0-9]+(?:-[a-z0-9]+)*$`). */
export function slugify(text: string) {
  return text
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, "-")
    .replace(/^-+|-+$/g, "")
    .slice(0, 200)
    .replace(/-+$/, "");
}

/** Perfil principal da pessoa: Admin vence Editor. Sem nenhum dos dois, não há acesso ao painel. */
export function primaryRole(roles: readonly string[]): Role | null {
  if (roles.includes("Admin")) return "Admin";
  if (roles.includes("Editor")) return "Editor";
  return null;
}

export function firstName(fullName: string) {
  return fullName.trim().split(/\s+/)[0] ?? "";
}

export function greetingFor(hour: number) {
  if (hour < 5) return "Boa noite";
  if (hour < 12) return "Bom dia";
  if (hour < 18) return "Boa tarde";
  return "Boa noite";
}

export function formatBytes(bytes: number) {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${Math.round(bytes / 1024)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1).replace(".", ",")} MB`;
}

/** "1 resposta", "3 respostas". */
export function plural(count: number, singular: string, pluralForm: string) {
  return `${count} ${count === 1 ? singular : pluralForm}`;
}
