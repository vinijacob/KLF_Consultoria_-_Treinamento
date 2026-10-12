/*
 * Consentimento de cookies opcionais (LGPD). Os essenciais não dependem de escolha. A escolha fica no cookie
 * `klf_consent` por 6 meses; mudar `CONSENT_VERSION` faz todo mundo escolher de novo (ex.: quando entra uma categoria).
 */
export const CONSENT_COOKIE = "klf_consent";
export const CONSENT_VERSION = 1;
export const CONSENT_MAX_AGE_DAYS = 180;

export type ConsentCategory = "thirdParty";

export type ConsentChoice = { version: number; decidedOn: string } & Record<ConsentCategory, boolean>;

export type ConsentCategoryInfo = { id: ConsentCategory; label: string; description: string; inUse: boolean };

/** Categorias opcionais. Com `inUse: false` ainda não há nada delas no site, então ninguém precisa escolher. */
export const consentCategories: ConsentCategoryInfo[] = [
  {
    id: "thirdParty",
    label: "Conteúdo de terceiros",
    description:
      "Vídeos do YouTube e publicações do Instagram mostrados dentro do site. Ao carregar, esses serviços podem gravar cookies próprios e saber que você visitou a página.",
    inUse: false,
  },
];

export function activeCategories(categories: ConsentCategoryInfo[] = consentCategories) {
  return categories.filter((category) => category.inUse);
}

export function parseConsent(raw: string | null | undefined): ConsentChoice | null {
  if (!raw) return null;
  try {
    const value = JSON.parse(decodeURIComponent(raw)) as Partial<ConsentChoice>;
    if (value.version !== CONSENT_VERSION || typeof value.decidedOn !== "string") return null;
    if (consentCategories.some((category) => typeof value[category.id] !== "boolean")) return null;
    return value as ConsentChoice;
  } catch {
    return null;
  }
}

export function serializeConsent(choice: ConsentChoice) {
  return encodeURIComponent(JSON.stringify(choice));
}

export function makeChoice(granted: Partial<Record<ConsentCategory, boolean>>, today: string): ConsentChoice {
  const categories = Object.fromEntries(consentCategories.map((category) => [category.id, granted[category.id] === true]));
  return { version: CONSENT_VERSION, decidedOn: today, ...categories } as ConsentChoice;
}

/** O banner só aparece se alguma categoria opcional estiver em uso e a pessoa ainda não escolheu (ou a escolha é de outra versão). */
export function needsDecision(choice: ConsentChoice | null, categories: ConsentCategoryInfo[] = consentCategories) {
  return choice === null && activeCategories(categories).length > 0;
}

export function readCookie(cookies: string, name: string) {
  const prefix = `${name}=`;
  return cookies
    .split(";")
    .map((part) => part.trim())
    .find((part) => part.startsWith(prefix))
    ?.slice(prefix.length);
}
