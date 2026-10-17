/*
 * Consentimento de cookies opcionais (LGPD). Os essenciais não dependem de escolha. A escolha fica no cookie
 * `klf_consent` por 6 meses, com as categorias que foram perguntadas: quando uma categoria nova entra em uso, quem já
 * tinha escolhido vê o banner de novo. Mudar `CONSENT_VERSION` faz todo mundo escolher de novo.
 */
export const CONSENT_COOKIE = "klf_consent";
export const CONSENT_VERSION = 1;
export const CONSENT_MAX_AGE_DAYS = 180;

export type ConsentCategory = "thirdParty";

/** `asked`: categorias em uso quando a pessoa escolheu (sem nenhuma, ela só viu o aviso dos essenciais). */
export type ConsentChoice = { version: number; decidedOn: string; asked?: ConsentCategory[] } & Record<ConsentCategory, boolean>;

export type ConsentCategoryInfo = { id: ConsentCategory; label: string; description: string; inUse: boolean };

/** Categorias opcionais. Com `inUse: false` ainda não há nada delas no site, então ninguém precisa escolher. */
export const consentCategories: ConsentCategoryInfo[] = [
  {
    id: "thirdParty",
    label: "Conteúdo de terceiros",
    description:
      "Vídeos do YouTube mostrados dentro do site. Ao carregar, esse serviço pode gravar cookies próprios e saber que você visitou a página.",
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
    if (value.asked !== undefined && !Array.isArray(value.asked)) return null;
    if (consentCategories.some((category) => typeof value[category.id] !== "boolean")) return null;
    return value as ConsentChoice;
  } catch {
    return null;
  }
}

export function serializeConsent(choice: ConsentChoice) {
  return encodeURIComponent(JSON.stringify(choice));
}

export function makeChoice(
  granted: Partial<Record<ConsentCategory, boolean>>,
  today: string,
  asked: ConsentCategory[] = activeCategories().map((category) => category.id),
): ConsentChoice {
  const categories = Object.fromEntries(consentCategories.map((category) => [category.id, granted[category.id] === true]));
  return { version: CONSENT_VERSION, decidedOn: today, asked, ...categories } as ConsentChoice;
}

/**
 * O banner aparece na primeira visita (ou com escolha de outra versão) e quando entrou em uso uma categoria que não
 * foi perguntada. Sem categoria opcional em uso, ele só avisa que o site usa cookies essenciais.
 */
export function needsDecision(choice: ConsentChoice | null, categories: ConsentCategoryInfo[] = consentCategories) {
  if (choice === null) return true;
  return activeCategories(categories).some((category) => !(choice.asked ?? []).includes(category.id));
}

export function readCookie(cookies: string, name: string) {
  const prefix = `${name}=`;
  return cookies
    .split(";")
    .map((part) => part.trim())
    .find((part) => part.startsWith(prefix))
    ?.slice(prefix.length);
}
