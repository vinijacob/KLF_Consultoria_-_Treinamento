"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { formatDateTime, formatShortDate } from "@/lib/admin/datetime";
import { setFlash, useApi } from "@/lib/admin/hooks";
import { notifyMessagesChanged } from "@/lib/admin/unread";
import { adminFetch } from "@/lib/auth/session";
import { ApiError } from "@/lib/api/client";
import { phoneDigits, phoneHref, whatsappLink } from "@/lib/format";
import type { ContactMessage, ContactMessageListItem, ContactMessageStatus, Paged } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Input, Select } from "@/components/ui/field";
import { Notice } from "@/components/ui/notice";
import { Kicker } from "@/components/ui/typography";
import { FlashNotice } from "../entity";
import { AdminHeader, ConfirmButton, EmptyState, IndexList, IndexRow, LoadError, Loading, Pagination, Tag } from "../ui";

const LIST = "/painel/mensagens";

export const contactStatusLabel: Record<ContactMessageStatus, string> = {
  New: "Nova",
  Read: "Lida",
  Answered: "Respondida",
};

function statusTag(status: ContactMessageStatus) {
  const tone = status === "New" ? "accent" : status === "Answered" ? "success" : "neutral";
  return <Tag tone={tone}>{contactStatusLabel[status]}</Tag>;
}

export function MessagesList({ initialStatus }: { initialStatus?: ContactMessageStatus }) {
  const [status, setStatus] = useState<ContactMessageStatus | "">(initialStatus ?? "");
  const [search, setSearch] = useState("");
  const [query, setQuery] = useState("");
  const [page, setPage] = useState(1);

  const params = new URLSearchParams({ page: String(page), pageSize: "20" });
  if (status) params.set("status", status);
  if (query) params.set("search", query);

  const { data, error, reload } = useApi<Paged<ContactMessageListItem>>(`/admin/contact-messages?${params}`);
  const filtered = Boolean(status || query);

  return (
    <div>
      <AdminHeader
        kicker="Atendimento"
        title="Mensagens"
        description="Pedidos de proposta e dúvidas enviados pelo formulário do site. Cada mensagem é excluída sozinha 2 anos depois do último contato (LGPD)."
      />
      <FlashNotice />
      <form
        role="search"
        className="mb-8 grid gap-4 sm:grid-cols-[1fr_2fr_auto] sm:items-end"
        onSubmit={(event) => {
          event.preventDefault();
          setPage(1);
          setQuery(search.trim());
        }}
      >
        <Select
          label="Situação"
          value={status}
          onChange={(event) => {
            setPage(1);
            setStatus(event.target.value as ContactMessageStatus | "");
          }}
        >
          <option value="">Todas</option>
          {(Object.keys(contactStatusLabel) as ContactMessageStatus[]).map((value) => (
            <option key={value} value={value}>
              {contactStatusLabel[value]}
            </option>
          ))}
        </Select>
        <Input label="Buscar por nome, e-mail ou empresa" type="search" value={search} maxLength={100} onChange={(event) => setSearch(event.target.value)} />
        <button type="submit" className="min-h-[3.125rem] border border-ink px-5 font-sans text-[0.9375rem] font-medium hover:bg-ink hover:text-paper">
          Buscar
        </button>
      </form>

      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.items.length === 0 &&
        (filtered ? (
          <EmptyState title="Nada encontrado">Tente outra situação ou outra palavra.</EmptyState>
        ) : (
          <EmptyState title="Nenhuma mensagem ainda">As mensagens enviadas pela página Contato aparecem aqui.</EmptyState>
        ))}
      {data && data.items.length > 0 && (
        <>
          <IndexList>
            {data.items.map((message) => (
              <IndexRow
                key={message.id}
                href={`${LIST}/${message.id}`}
                title={message.name}
                meta={
                  <>
                    {[message.company, message.serviceTitle, formatDateTime(message.receivedAt)].filter(Boolean).join(" · ")}
                    <span className="mt-1 block truncate">{message.excerpt}</span>
                  </>
                }
                tags={statusTag(message.status)}
              />
            ))}
          </IndexList>
          <Pagination page={page} totalPages={data.totalPages} onChange={setPage} />
        </>
      )}
    </div>
  );
}

function MessageView({ message, onChange }: { message: ContactMessage; onChange: (message: ContactMessage) => void }) {
  const router = useRouter();
  const [pending, setPending] = useState(false);
  const [problem, setProblem] = useState<string | null>(null);
  const firstName = message.name.split(" ")[0];
  const subject = encodeURIComponent("Seu contato com a KLF Consultoria");

  async function changeStatus(status: ContactMessageStatus) {
    setPending(true);
    setProblem(null);
    try {
      onChange(await adminFetch<ContactMessage>(`/admin/contact-messages/${message.id}/status`, { method: "PUT", json: { status } }));
      notifyMessagesChanged();
    } catch (failure) {
      setProblem(failure instanceof ApiError ? failure.message : "Não foi possível atualizar. Tente de novo.");
    } finally {
      setPending(false);
    }
  }

  async function remove() {
    try {
      await adminFetch(`/admin/contact-messages/${message.id}`, { method: "DELETE" });
      notifyMessagesChanged();
      setFlash(`Mensagem de ${message.name} excluída definitivamente.`);
      router.push(LIST);
      return null;
    } catch (failure) {
      return failure instanceof ApiError ? failure.message : "Não foi possível excluir. Tente de novo.";
    }
  }

  return (
    <>
      <AdminHeader
        kicker="Mensagens"
        title={message.name}
        back={{ href: LIST, label: "Todas as mensagens" }}
        description={
          <span className="flex flex-wrap items-center gap-3">
            {statusTag(message.status)} Recebida em {formatDateTime(message.receivedAt)}
          </span>
        }
      />

      {problem && (
        <Notice tone="error" role="alert" className="mb-8">
          {problem}
        </Notice>
      )}

      <div className="grid gap-10 lg:grid-cols-[1fr_20rem]">
        <article>
          {message.serviceTitle && (
            <p className="font-sans text-sm text-ink-soft">
              Interesse: <span className="font-medium text-ink">{message.serviceTitle}</span>
            </p>
          )}
          <div className="mt-4 border border-rule bg-card p-6 text-lg leading-relaxed whitespace-pre-line">{message.message}</div>

          <div className="mt-8 flex flex-wrap items-center gap-3">
            {message.status !== "Answered" ? (
              <Button disabled={pending} onClick={() => void changeStatus("Answered")}>
                Marcar como respondida
              </Button>
            ) : (
              <Button variant="outline" disabled={pending} onClick={() => void changeStatus("Read")}>
                Desfazer “respondida”
              </Button>
            )}
            {message.status !== "New" && (
              <Button variant="quiet" disabled={pending} onClick={() => void changeStatus("New")}>
                Marcar como não lida
              </Button>
            )}
          </div>
        </article>

        <aside className="grid content-start gap-8">
          <section aria-labelledby="responder">
            <Kicker id="responder">Responder</Kicker>
            <ul className="mt-3 grid gap-1 font-sans text-[0.9375rem]">
              <li>
                <a
                  href={`mailto:${message.email}?subject=${subject}`}
                  className="inline-flex min-h-11 items-center text-brand underline underline-offset-4 break-all"
                >
                  {message.email}
                </a>
              </li>
              {message.phone && (
                <>
                  <li>
                    <a href={phoneHref(message.phone)} className="inline-flex min-h-11 items-center text-brand underline underline-offset-4">
                      Ligar: {message.phone}
                    </a>
                  </li>
                  <li>
                    <a
                      href={whatsappLink(phoneDigits(message.phone), `Olá, ${firstName}! Aqui é a Kilciene, da KLF. Recebi sua mensagem pelo site.`)}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="inline-flex min-h-11 items-center text-brand underline underline-offset-4"
                    >
                      Abrir no WhatsApp<span className="sr-only"> (abre em nova aba)</span>
                    </a>
                  </li>
                </>
              )}
            </ul>
            {message.company && <p className="mt-2 font-sans text-sm text-ink-soft">Empresa: {message.company}</p>}
          </section>

          <section aria-labelledby="dados" className="border-t border-rule pt-6">
            <Kicker id="dados">Dados pessoais (LGPD)</Kicker>
            <p className="mt-3 font-sans text-sm leading-relaxed text-ink-soft">
              A pessoa confirmou ter lido a política de privacidade de {formatShortDate(`${message.privacyPolicyVersion}T12:00:00Z`)} ao enviar.
              Esta mensagem será excluída sozinha em {formatShortDate(message.deleteAfter)}.
            </p>
            <div className="mt-4">
              <ConfirmButton
                label="Excluir definitivamente"
                title="Excluir esta mensagem?"
                description={
                  <p>
                    A mensagem e os dados de {message.name} saem do sistema de vez, sem como recuperar. Use quando a pessoa pedir a
                    exclusão dos dados ou quando o contato não for mais necessário.
                  </p>
                }
                confirmLabel="Excluir definitivamente"
                onConfirm={remove}
              />
            </div>
          </section>
        </aside>
      </div>
    </>
  );
}

/** Abre a mensagem e, se ainda não tinha sido lida, marca como lida. */
export function MessageDetail({ id }: { id: string }) {
  const { data, error, reload } = useApi<ContactMessage>(`/admin/contact-messages/${id}`);
  const [message, setMessage] = useState<ContactMessage | null>(null);
  const markedRef = useRef(false);
  const current = message ?? data;

  useEffect(() => {
    if (!data || data.status !== "New" || markedRef.current) return;
    markedRef.current = true;
    adminFetch<ContactMessage>(`/admin/contact-messages/${id}/status`, { method: "PUT", json: { status: "Read" } }).then((read) => {
      setMessage(read);
      notifyMessagesChanged();
    }, () => undefined);
  }, [data, id]);

  if (error) {
    return (
      <>
        <AdminHeader kicker="Mensagens" title="Mensagem" back={{ href: LIST, label: "Todas as mensagens" }} />
        <LoadError error={error} onRetry={reload} />
        <p className="mt-6 font-sans text-sm">
          <Link href={LIST} className="text-brand underline underline-offset-4">
            Voltar para as mensagens
          </Link>
        </p>
      </>
    );
  }

  if (!current) return <Loading />;

  return <MessageView message={current} onChange={setMessage} />;
}
