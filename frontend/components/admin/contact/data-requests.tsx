"use client";

import Link from "next/link";
import { useState, type FormEvent } from "react";
import { formatDateTime, formatDay, formatShortDate, todayLocal } from "@/lib/admin/datetime";
import { useApi } from "@/lib/admin/hooks";
import { plural } from "@/lib/admin/text";
import { notifyMessagesChanged } from "@/lib/admin/unread";
import { adminFetch } from "@/lib/auth/session";
import { ApiError, fieldError } from "@/lib/api/client";
import { dataRequestKindLabel, dataRequestStatusLabel, deadlineState, type DataRequestKind, type DataRequestStatus } from "@/lib/data-requests";
import type { Paged } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Input, Select, Textarea } from "@/components/ui/field";
import { Notice } from "@/components/ui/notice";
import { Kicker } from "@/components/ui/typography";
import { AdminHeader, ConfirmButton, EmptyState, IndexList, IndexRow, LoadError, Loading, Pagination, Section, Tag } from "../ui";

const LIST = "/painel/pedidos-lgpd";

type ListItem = {
  id: string;
  protocol: string;
  name: string;
  email: string;
  kind: DataRequestKind;
  status: DataRequestStatus;
  receivedAt: string;
  dueOn: string;
  isOverdue: boolean;
};

type DataRequest = ListItem & {
  details?: string | null;
  closedAt?: string | null;
  resolution?: string | null;
  history: { at: string; text: string }[];
  deleteAfter?: string | null;
};

type Findings = {
  searchedName: string;
  contactMessages: { id: string; receivedAt: string; excerpt: string }[];
  testimonials: { id: string; authorName: string; companyName?: string | null; isPublished: boolean; isDeleted: boolean; hasPhoto: boolean; hasConsentDocument: boolean }[];
  hasPanelAccount: boolean;
};

function statusTag(item: Pick<ListItem, "status" | "dueOn">) {
  if (item.status !== "Open") return <Tag tone={item.status === "Completed" ? "success" : "neutral"}>{dataRequestStatusLabel[item.status]}</Tag>;
  const deadline = deadlineState(item.dueOn, todayLocal());
  return <Tag tone={deadline.tone === "neutral" ? "accent" : deadline.tone}>{deadline.text}</Tag>;
}

const failureText = (failure: unknown, fallback: string) =>
  failure instanceof ApiError ? (fieldError(failure.errors, "resolution") ?? failure.message) : fallback;

export function DataRequestsList() {
  const [status, setStatus] = useState<DataRequestStatus | "">("Open");
  const [page, setPage] = useState(1);
  const params = new URLSearchParams({ page: String(page), pageSize: "20" });
  if (status) params.set("status", status);
  const { data, error, reload } = useApi<Paged<ListItem>>(`/admin/data-requests?${params}`);

  return (
    <div>
      <AdminHeader
        kicker="Atendimento"
        title="Pedidos LGPD"
        description="Pedidos de acesso, correção ou exclusão de dados pessoais feitos pelo site (página Seus dados). A lei dá 15 dias para responder."
      />
      <div className="mb-8 max-w-xs">
        <Select
          label="Situação"
          value={status}
          onChange={(event) => {
            setPage(1);
            setStatus(event.target.value as DataRequestStatus | "");
          }}
        >
          <option value="">Todos</option>
          {(Object.keys(dataRequestStatusLabel) as DataRequestStatus[]).map((value) => (
            <option key={value} value={value}>
              {dataRequestStatusLabel[value]}
            </option>
          ))}
        </Select>
      </div>

      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.items.length === 0 && (
        <EmptyState title={status === "Open" ? "Nenhum pedido em aberto" : "Nenhum pedido"}>
          Os pedidos feitos na página Seus dados (/meus-dados) aparecem aqui, e a KLF recebe um aviso por e-mail.
        </EmptyState>
      )}
      {data && data.items.length > 0 && (
        <>
          <IndexList>
            {data.items.map((item) => (
              <IndexRow
                key={item.id}
                href={`${LIST}/${item.id}`}
                title={item.name}
                meta={`${dataRequestKindLabel[item.kind]} · protocolo ${item.protocol} · recebido em ${formatDateTime(item.receivedAt)}`}
                tags={statusTag(item)}
              />
            ))}
          </IndexList>
          <Pagination page={page} totalPages={data.totalPages} onChange={setPage} />
        </>
      )}
    </div>
  );
}

function FindingsSection({ request, onChanged }: { request: DataRequest; onChanged: (request: DataRequest) => void }) {
  const [nameInput, setNameInput] = useState("");
  const [name, setName] = useState("");
  const query = name ? `?name=${encodeURIComponent(name)}` : "";
  const { data, error, reload } = useApi<Findings>(`/admin/data-requests/${request.id}/findings${query}`);

  async function erase(path: string) {
    try {
      onChanged(await adminFetch<DataRequest>(`/admin/data-requests/${request.id}/${path}`, { method: "POST" }));
      notifyMessagesChanged();
      reload();
      return null;
    } catch (failure) {
      return failureText(failure, "Não foi possível apagar. Tente de novo.");
    }
  }

  return (
    <Section
      title="Onde estão os dados"
      description="O que o sistema guarda com o e-mail e o nome do pedido. Fora daqui, confira também e-mails, WhatsApp e planilhas da KLF."
    >
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data && (
        <>
          <div>
            <Kicker>Mensagens de contato com {request.email}</Kicker>
            {data.contactMessages.length === 0 ? (
              <p className="mt-2 font-sans text-[0.9375rem] text-ink-soft">Nenhuma.</p>
            ) : (
              <>
                <ul className="mt-3 border-t border-rule">
                  {data.contactMessages.map((message) => (
                    <li key={message.id} className="border-b border-rule py-2 font-sans text-[0.9375rem]">
                      <Link href={`/painel/mensagens/${message.id}`} className="inline-flex min-h-11 items-center text-brand underline underline-offset-4">
                        {formatDateTime(message.receivedAt)}
                      </Link>
                      <span className="block text-ink-soft">{message.excerpt}</span>
                    </li>
                  ))}
                </ul>
                <div className="mt-4">
                  <ConfirmButton
                    label={`Apagar de vez ${plural(data.contactMessages.length, "mensagem", "mensagens")}`}
                    title="Apagar as mensagens deste e-mail?"
                    description={<p>Todas as mensagens de contato enviadas com {request.email} saem do sistema de vez, sem como recuperar.</p>}
                    confirmLabel="Apagar de vez"
                    onConfirm={() => erase("erase-contact-messages")}
                  />
                </div>
              </>
            )}
          </div>

          <div>
            <Kicker>Depoimentos com o nome “{data.searchedName}”</Kicker>
            <form
              role="search"
              className="mt-3 grid gap-3 sm:grid-cols-[1fr_auto] sm:items-end"
              onSubmit={(event: FormEvent) => {
                event.preventDefault();
                setName(nameInput.trim());
              }}
            >
              <Input label="Procurar por outro nome" optional value={nameInput} maxLength={120} onChange={(event) => setNameInput(event.target.value)} />
              <Button type="submit" variant="outline">
                Procurar
              </Button>
            </form>
            {data.testimonials.length === 0 ? (
              <p className="mt-3 font-sans text-[0.9375rem] text-ink-soft">Nenhum depoimento com esse nome (os excluídos do painel também entram na busca).</p>
            ) : (
              <ul className="mt-3 border-t border-rule">
                {data.testimonials.map((testimonial) => (
                  <li key={testimonial.id} className="flex flex-wrap items-center gap-x-6 gap-y-2 border-b border-rule py-3">
                    <span className="min-w-0 flex-1 font-sans text-[0.9375rem]">
                      {testimonial.isDeleted ? (
                        <span className="font-medium">{testimonial.authorName}</span>
                      ) : (
                        <Link href={`/painel/depoimentos/${testimonial.id}`} className="font-medium text-brand underline underline-offset-4">
                          {testimonial.authorName}
                        </Link>
                      )}
                      {testimonial.companyName && <span className="text-ink-soft"> · {testimonial.companyName}</span>}
                      <span className="mt-1 flex flex-wrap gap-1.5">
                        {testimonial.isPublished && <Tag tone="success">No site</Tag>}
                        {testimonial.isDeleted && <Tag>Excluído do painel, dados ainda guardados</Tag>}
                        {testimonial.hasPhoto && <Tag>Foto</Tag>}
                        {testimonial.hasConsentDocument && <Tag>Termo em PDF</Tag>}
                      </span>
                    </span>
                    <ConfirmButton
                      label="Apagar de vez"
                      variant="quiet"
                      title={`Apagar de vez o depoimento de ${testimonial.authorName}?`}
                      description={
                        <p>
                          O depoimento sai do site e do sistema, junto com o termo assinado e a foto (a foto fica só se outro conteúdo
                          a usar). Não há como recuperar. Se o pedido for de exclusão do termo também, lembre que ele prova a
                          autorização dada antes.
                        </p>
                      }
                      confirmLabel="Apagar de vez"
                      onConfirm={() => erase(`erase-testimonials/${testimonial.id}`)}
                    />
                  </li>
                ))}
              </ul>
            )}
          </div>

          {data.hasPanelAccount && (
            <Notice tone="info" title="Este e-mail tem conta no painel">
              Contas da equipe são tratadas em <Link href="/painel/equipe" className="text-brand underline underline-offset-4">Equipe</Link>{" "}
              (desativar a conta encerra o acesso).
            </Notice>
          )}
        </>
      )}
    </Section>
  );
}

function ResolutionSection({ request, onChanged }: { request: DataRequest; onChanged: (request: DataRequest) => void }) {
  const [resolution, setResolution] = useState(request.resolution ?? "");
  const [pending, setPending] = useState(false);
  const [problem, setProblem] = useState<string | null>(null);
  const subject = encodeURIComponent(`Seu pedido sobre dados pessoais — protocolo ${request.protocol}`);

  async function change(status: DataRequestStatus) {
    setPending(true);
    setProblem(null);
    try {
      onChanged(await adminFetch<DataRequest>(`/admin/data-requests/${request.id}/status`, { method: "PUT", json: { status, resolution: resolution.trim() || null } }));
      notifyMessagesChanged();
    } catch (failure) {
      setProblem(failureText(failure, "Não foi possível salvar. Tente de novo."));
    } finally {
      setPending(false);
    }
  }

  return (
    <Section
      title="Resposta"
      description="Responda à pessoa por e-mail e registre aqui o que foi feito. Recusar exige o motivo (por exemplo, um dado que a lei manda guardar)."
    >
      <p className="font-sans text-[0.9375rem]">
        <a href={`mailto:${request.email}?subject=${subject}`} className="inline-flex min-h-11 items-center text-brand underline underline-offset-4 break-all">
          Escrever para {request.email}
        </a>
      </p>
      <Textarea
        label="O que foi respondido ou feito"
        optional={request.status !== "Open"}
        value={resolution}
        maxLength={2000}
        disabled={request.status !== "Open"}
        onChange={(event) => setResolution(event.target.value)}
        error={problem ?? undefined}
        className="min-h-28"
      />
      <div className="flex flex-wrap gap-3">
        {request.status === "Open" ? (
          <>
            <Button disabled={pending} onClick={() => void change("Completed")}>
              Concluir pedido
            </Button>
            <Button variant="outline" disabled={pending} onClick={() => void change("Declined")}>
              Recusar com motivo
            </Button>
          </>
        ) : (
          <Button variant="outline" disabled={pending} onClick={() => void change("Open")}>
            Reabrir pedido
          </Button>
        )}
      </div>
    </Section>
  );
}

function DataRequestView({ request, onChanged }: { request: DataRequest; onChanged: (request: DataRequest) => void }) {
  return (
    <>
      <AdminHeader
        kicker="Pedidos LGPD"
        title={request.name}
        back={{ href: LIST, label: "Todos os pedidos" }}
        description={
          <span className="flex flex-wrap items-center gap-3">
            {statusTag(request)} Protocolo {request.protocol} · recebido em {formatDateTime(request.receivedAt)} · responder até{" "}
            {formatDay(request.dueOn)}
          </span>
        }
      />

      {request.status === "Open" && (
        <Notice tone="privacy" title="Confirme que é a pessoa" className="mb-8">
          Antes de entregar, corrigir ou apagar dados, confirme pelo e-mail {request.email} que o pedido é mesmo dela. Assim
          ninguém consegue os dados de outra pessoa só sabendo o nome.
        </Notice>
      )}

      <Section title="Pedido">
        <p className="text-xl">{dataRequestKindLabel[request.kind]}</p>
        <p className="font-sans text-[0.9375rem]">
          {request.name} · <span className="break-all">{request.email}</span>
        </p>
        {request.details && <div className="rounded-2xl border border-rule bg-card p-5 text-lg leading-relaxed whitespace-pre-line">{request.details}</div>}
        {request.resolution && request.status !== "Open" && (
          <div>
            <Kicker>{request.status === "Declined" ? "Motivo da recusa" : "Resposta registrada"}</Kicker>
            <p className="mt-2 font-sans text-[0.9375rem] whitespace-pre-line">{request.resolution}</p>
          </div>
        )}
      </Section>

      <FindingsSection request={request} onChanged={onChanged} />
      <ResolutionSection key={request.status} request={request} onChanged={onChanged} />

      <Section title="Histórico" description="Fica guardado com o pedido, como prova do atendimento.">
        <ol className="border-t border-rule font-sans text-[0.9375rem]">
          {request.history.map((item, index) => (
            <li key={index} className="flex flex-wrap gap-x-4 border-b border-rule py-2">
              <span className="w-36 shrink-0 font-mono text-xs leading-6 text-ink-soft">{formatDateTime(item.at)}</span>
              <span className="min-w-0 flex-1">{item.text}</span>
            </li>
          ))}
        </ol>
        {request.deleteAfter && (
          <p className="font-sans text-sm text-ink-soft">Este pedido é apagado sozinho em {formatShortDate(request.deleteAfter)} (5 anos depois de fechado).</p>
        )}
      </Section>
    </>
  );
}

export function DataRequestDetail({ id }: { id: string }) {
  const { data, error, reload } = useApi<DataRequest>(`/admin/data-requests/${id}`);
  const [current, setCurrent] = useState<DataRequest | null>(null);
  const request = current ?? data;

  if (error) {
    return (
      <>
        <AdminHeader kicker="Pedidos LGPD" title="Pedido" back={{ href: LIST, label: "Todos os pedidos" }} />
        <LoadError error={error} onRetry={reload} />
      </>
    );
  }

  if (!request) return <Loading />;

  return <DataRequestView request={request} onChanged={setCurrent} />;
}
