import type { SessionStatus } from "@/types/feedback";
import type { PostStatus } from "@/types/admin";
import { Tag } from "./ui";

export const sessionStatusLabel: Record<SessionStatus, string> = {
  Scheduled: "Agendada",
  Open: "Aberta",
  Closed: "Encerrada",
};

export function sessionStatusTag(status: SessionStatus) {
  const tone = status === "Open" ? "success" : status === "Scheduled" ? "brand" : "neutral";
  return <Tag tone={tone}>{sessionStatusLabel[status]}</Tag>;
}

export const postStatusLabel: Record<PostStatus, string> = {
  Draft: "Rascunho",
  Scheduled: "Agendada",
  Published: "Publicada",
};

/** Agendada com data já passada já está no site (regra `Post.IsVisible` da API). */
export function effectiveStatus(status: PostStatus, publishedAt?: string | null): PostStatus {
  return status === "Scheduled" && publishedAt && Date.parse(publishedAt) <= Date.now() ? "Published" : status;
}

export function postStatusTag(status: PostStatus) {
  const tone = status === "Published" ? "success" : status === "Scheduled" ? "brand" : "neutral";
  return <Tag tone={tone}>{postStatusLabel[status]}</Tag>;
}

export function visibilityTag(isActive: boolean) {
  return isActive ? <Tag tone="success">No site</Tag> : <Tag>Oculto</Tag>;
}
