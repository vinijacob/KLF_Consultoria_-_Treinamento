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

/** Até `count` itens sorteados, sem repetir (Fisher–Yates). `random` pode ser trocado nos testes. */
export function pickRandom<T>(items: readonly T[], count: number, random: () => number = Math.random) {
  const pool = [...items];
  for (let i = pool.length - 1; i > 0; i--) {
    const j = Math.floor(random() * (i + 1));
    [pool[i], pool[j]] = [pool[j], pool[i]];
  }
  return pool.slice(0, count);
}
