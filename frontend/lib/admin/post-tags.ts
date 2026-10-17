import { normalizeText } from "../search";

/** Limites iguais aos da API (`Post.MaxTags` e `TaxonomyText.TagNameMaxLength`). */
export const MAX_TAGS = 10;
export const TAG_MAX_LENGTH = 40;

export const cleanTagName = (name: string) => name.trim().replace(/\s+/g, " ");

const sameTag = (a: string, b: string) => normalizeText(a).replace(/[^a-z0-9]+/g, "") === normalizeText(b).replace(/[^a-z0-9]+/g, "");

export type AddTagResult = { tags: string[]; problem: string | null };

/**
 * Acrescenta as tags digitadas (uma ou várias separadas por vírgula). Ignora repetidas, mesmo com outro acento ou
 * maiúsculas; usa o nome de uma tag já existente quando ela bate (para não criar "pos venda" ao lado de "Pós-venda").
 */
export function addTags(current: string[], typed: string, existing: readonly string[] = []): AddTagResult {
  const tags = [...current];
  let problem: string | null = null;

  for (const raw of typed.split(",")) {
    const name = cleanTagName(raw);
    if (!name || !/[\p{L}\p{N}]/u.test(name)) continue;
    if (name.length > TAG_MAX_LENGTH) {
      problem = `Cada tag pode ter no máximo ${TAG_MAX_LENGTH} caracteres.`;
      continue;
    }
    if (tags.some((tag) => sameTag(tag, name))) continue;
    if (tags.length >= MAX_TAGS) {
      problem = `Use no máximo ${MAX_TAGS} tags por publicação.`;
      break;
    }
    tags.push(existing.find((tag) => sameTag(tag, name)) ?? name);
  }

  return { tags, problem };
}
