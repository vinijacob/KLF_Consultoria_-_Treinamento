/** Texto para comparação: sem acentos, minúsculo e sem espaços nas pontas ("Gestão" → "gestao"). */
export function normalizeText(text: string) {
  return text
    .normalize("NFD")
    .replace(/[̀-ͯ]/g, "")
    .toLowerCase()
    .trim();
}

/** Todas as palavras da busca precisam aparecer em algum dos campos, em qualquer ordem. */
export function matchesQuery(query: string, ...fields: (string | null | undefined)[]) {
  const terms = normalizeText(query).split(/\s+/).filter(Boolean);
  if (terms.length === 0) return true;

  const haystack = normalizeText(fields.filter(Boolean).join(" "));
  return terms.every((term) => haystack.includes(term));
}

/** Letra do índice alfabético; nomes que começam com número ou símbolo vão para "#". */
export function initialOf(name: string) {
  const first = normalizeText(name).charAt(0);
  return /[a-z]/.test(first) ? first.toUpperCase() : "#";
}

const collator = new Intl.Collator("pt-BR", { sensitivity: "base", numeric: true });

/** Agrupa por letra inicial, em ordem alfabética (com "#" no fim), e ordena os itens de cada grupo. */
export function groupByInitial<T>(items: readonly T[], nameOf: (item: T) => string) {
  const groups = new Map<string, T[]>();

  for (const item of [...items].sort((a, b) => collator.compare(nameOf(a), nameOf(b)))) {
    const letter = initialOf(nameOf(item));
    groups.set(letter, [...(groups.get(letter) ?? []), item]);
  }

  return [...groups.entries()].sort(([a], [b]) => (a === "#" ? 1 : b === "#" ? -1 : a.localeCompare(b)));
}

export const ALPHABET = [..."ABCDEFGHIJKLMNOPQRSTUVWXYZ", "#"];
