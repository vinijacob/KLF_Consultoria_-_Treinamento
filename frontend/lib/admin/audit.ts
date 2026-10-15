/** Uma linha do registro de atividades, como vem da API. */
export type AuditEntry = {
  id: string;
  occurredAt: string;
  actorId?: string | null;
  actorName?: string | null;
  action: string;
  targetId?: string | null;
  targetLabel?: string | null;
};

export const auditAreas = [
  { value: "content", label: "Conteúdo do site" },
  { value: "contact", label: "Atendimento" },
  { value: "feedback", label: "Avaliações" },
  { value: "team", label: "Equipe" },
  { value: "access", label: "Acessos" },
] as const;

/** O que cada tipo de registro é, no texto e no painel (`null` = sem página para abrir). */
const subjects: Record<string, { noun: string; href: ((id: string) => string) | null }> = {
  service: { noun: "o serviço", href: (id) => `/painel/servicos/${id}` },
  post: { noun: "a publicação", href: (id) => `/painel/publicacoes/${id}` },
  career: { noun: "o item da trajetória", href: (id) => `/painel/trajetoria/${id}` },
  testimonial: { noun: "um depoimento", href: (id) => `/painel/depoimentos/${id}` },
  client: { noun: "o cliente", href: (id) => `/painel/clientes/${id}` },
  media: { noun: "a imagem", href: () => "/painel/imagens" },
  album: { noun: "o álbum", href: (id) => `/painel/galeria/${id}` },
  settings: { noun: "os dados do site", href: () => "/painel/dados-do-site" },
  "feedback-form": { noun: "o formulário", href: (id) => `/painel/avaliacoes/formularios/${id}` },
  "feedback-session": { noun: "a turma", href: (id) => `/painel/avaliacoes/turmas/${id}` },
  contact: { noun: "uma mensagem de contato", href: (id) => `/painel/mensagens/${id}` },
  user: { noun: "", href: (id) => `/painel/equipe/${id}` },
};

const verbs: Record<string, string> = {
  created: "criou",
  updated: "editou",
  deleted: "excluiu",
  published: "publicou",
  uploaded: "enviou",
  "consent-revoked": "revogou o consentimento de",
  closed: "encerrou",
  reopened: "reabriu",
  answered: "marcou como respondida",
};

/** Frases que não seguem o padrão "verbo + o quê". `{label}` vira o rótulo entre aspas. */
const phrases: Record<string, string> = {
  "login.succeeded": "entrou no painel",
  "auth.recovery-code-used": "entrou com um código de recuperação",
  "auth.two-factor-enabled": "ativou a verificação em duas etapas",
  "auth.recovery-codes-regenerated": "gerou novos códigos de recuperação",
  "auth.password-reset": "redefiniu a senha",
  "user.invited": "convidou {label} para a equipe",
  "user.invite-resent": "gerou um novo convite para {label}",
  "user.invite-accepted": "aceitou o convite e criou a senha",
  "user.updated": "editou os dados de {label}",
  "user.role-changed": "mudou o perfil de {label}",
  "user.deactivated": "desativou a conta de {label}",
  "user.reactivated": "reativou a conta de {label}",
  "user.two-factor-reset": "redefiniu a verificação em duas etapas de {label}",
  "contact.received": "enviou uma mensagem pelo formulário de contato",
  "contact.purged": "excluiu {label} com mais de 2 anos (limpeza automática)",
  "post.published": "publicou {label}",
  "media.uploaded": "enviou a imagem {label}",
  "settings.updated": "editou os dados do site: {label}",
};

const settingsLabels: Record<string, string> = {
  about: "Sobre",
  contact: "Contato",
  social: "Redes sociais",
  seo: "Busca e compartilhamento",
  legal: "Dados legais",
};

export type AuditLine = { actor: string; text: string; href: string | null };

/** "Kilciene" + "publicou “Atendimento que encanta”" + link para abrir, a partir de uma linha do registro. */
export function describeAudit(entry: AuditEntry): AuditLine {
  const [area, verb] = entry.action.split(".");
  const rawLabel = area === "settings" ? (settingsLabels[entry.targetLabel ?? ""] ?? entry.targetLabel) : entry.targetLabel;
  const label = rawLabel ? (area === "user" || area === "settings" || area === "contact" ? rawLabel : `“${rawLabel}”`) : null;
  const actor = entry.actorName ?? (entry.action === "contact.received" ? "Visitante do site" : "Sistema");
  const subject = subjects[area];
  const href = verb !== "deleted" && entry.targetId && subject?.href ? subject.href(entry.targetId) : null;

  const phrase = phrases[entry.action];
  if (phrase) return { actor, text: phrase.replace("{label}", label ?? "alguém").trim(), href };

  if (subject && verbs[verb]) {
    return { actor, text: [verbs[verb], subject.noun, label].filter(Boolean).join(" "), href };
  }

  return { actor, text: entry.action, href };
}
