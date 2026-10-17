/** Pedidos do titular sobre os próprios dados (LGPD, art. 18): textos usados no site e no painel. */
export type DataRequestKind = "Access" | "Correction" | "Deletion" | "ConsentWithdrawal" | "Other";
export type DataRequestStatus = "Open" | "Completed" | "Declined";

export const dataRequestKinds: { id: DataRequestKind; label: string; hint: string }[] = [
  { id: "Access", label: "Saber quais dados a KLF tem sobre mim", hint: "Enviamos uma cópia do que encontrarmos." },
  { id: "Correction", label: "Corrigir dados", hint: "Diga o que está errado e como deve ficar." },
  { id: "Deletion", label: "Apagar os meus dados", hint: "Apagamos de verdade o que não precisarmos guardar por lei." },
  { id: "ConsentWithdrawal", label: "Retirar um consentimento", hint: "Por exemplo, tirar do site um depoimento seu." },
  { id: "Other", label: "Outro pedido", hint: "Portabilidade, com quem compartilhamos, oposição…" },
];

export const dataRequestKindLabel = Object.fromEntries(dataRequestKinds.map((kind) => [kind.id, kind.label])) as Record<DataRequestKind, string>;

export const dataRequestStatusLabel: Record<DataRequestStatus, string> = {
  Open: "Em aberto",
  Completed: "Concluído",
  Declined: "Recusado",
};

/** Correção e "outro" precisam de explicação (a API confere igual). */
export const needsDetails = (kind: DataRequestKind) => kind === "Correction" || kind === "Other";

const dayNumber = (day: string) => {
  const [year, month, date] = day.split("-").map(Number);
  return Date.UTC(year, month - 1, date) / 86_400_000;
};

/** Situação do prazo de 15 dias de um pedido em aberto, a partir de "hoje" em Manaus (datas "AAAA-MM-DD"). */
export function deadlineState(dueOn: string, today: string): { text: string; tone: "danger" | "warning" | "neutral" } {
  const left = dayNumber(dueOn) - dayNumber(today);
  if (left < 0) return { text: `prazo vencido há ${-left} dia${left === -1 ? "" : "s"}`, tone: "danger" };
  if (left === 0) return { text: "prazo vence hoje", tone: "danger" };
  if (left <= 3) return { text: `prazo vence em ${left} dia${left === 1 ? "" : "s"}`, tone: "warning" };
  return { text: `prazo em ${left} dias`, tone: "neutral" };
}
