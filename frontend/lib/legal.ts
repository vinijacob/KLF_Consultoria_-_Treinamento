import type { LegalSettings } from "@/types/site";

/** Última revisão do texto das páginas legais. Mude junto com o texto de `/privacidade`, `/termos` ou `/cookies`. */
export const LEGAL_TEXT_REVISED_ON = "2026-10-15";

const requiredFields: [keyof LegalSettings, string][] = [
  ["companyName", "razão social"],
  ["cnpj", "CNPJ"],
  ["address", "endereço da sede"],
  ["forumCity", "cidade do foro"],
  ["dpoName", "nome do encarregado pelos dados"],
  ["dpoEmail", "e-mail do encarregado pelos dados"],
];

/** O que falta preencher em Dados do site → Dados legais para as páginas legais ficarem completas. */
export function missingLegalData(legal?: LegalSettings | null): string[] {
  return requiredFields.filter(([key]) => !legal?.[key]?.trim()).map(([, label]) => label);
}

/** A data mais recente entre a revisão do texto e a última vez que os dados da empresa foram salvos. */
export function legalUpdatedOn(legal?: LegalSettings | null): string {
  const saved = legal?.updatedOn;
  return saved && saved > LEGAL_TEXT_REVISED_ON ? saved : LEGAL_TEXT_REVISED_ON;
}
