"use client";

import { useRouter } from "next/navigation";
import { useEffect, useId, useState } from "react";
import { addDays, formatDateTime, formatShortDate, fromLocalInput, toLocalInput, todayLocal } from "@/lib/admin/datetime";
import { optional } from "@/lib/admin/editor";
import { setFlash, useApi, useFlash, useSubmit } from "@/lib/admin/hooks";
import { plural } from "@/lib/admin/text";
import { adminDownload, adminFetch } from "@/lib/auth/session";
import { ApiError, fieldError } from "@/lib/api/client";
import { toPayload } from "@/lib/feedback/builder";
import { cn } from "@/lib/cn";
import type {
  AdminClient,
  AdminServiceListItem,
  FeedbackFormListItem,
  FeedbackSessionDetail,
  FeedbackSessionListItem,
  FeedbackSummary,
  Paged,
  SessionResults,
} from "@/types/admin";
import type { FormDefinition } from "@/types/feedback";
import { Button, ButtonLink } from "@/components/ui/button";
import { Input, Select, Textarea } from "@/components/ui/field";
import { DownloadIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { Kicker } from "@/components/ui/typography";
import { FlashNotice } from "../entity";
import { sessionStatusTag } from "../tags";
import {
  AdminForm,
  AdminHeader,
  ConfirmButton,
  EmptyState,
  FieldRow,
  IndexList,
  IndexRow,
  LoadError,
  Loading,
  NewButton,
  Pagination,
  Section,
  Stat,
} from "../ui";
import { FormBuilder, PreviewButton } from "./builder";
import { NpsBlock, SessionResultsView } from "./results";

const LIST = "/painel/avaliacoes/turmas";

function useLookups() {
  const clients = useApi<AdminClient[]>("/admin/clients");
  const services = useApi<AdminServiceListItem[]>("/admin/services");
  return { clients: clients.data ?? [], services: services.data ?? [] };
}

function LinkSelects({
  clientId,
  serviceId,
  onClient,
  onService,
  errors,
  allLabel,
}: {
  clientId: string;
  serviceId: string;
  onClient: (id: string) => void;
  onService: (id: string) => void;
  errors?: Record<string, string[]>;
  allLabel?: string;
}) {
  const { clients, services } = useLookups();

  return (
    <FieldRow>
      <Select label="Empresa ou loja" optional={!allLabel} value={clientId} onChange={(event) => onClient(event.target.value)} error={fieldError(errors, "clientId")}>
        <option value="">{allLabel ?? "Nenhuma"}</option>
        {clients.map((client) => (
          <option key={client.id} value={client.id}>
            {client.name}
          </option>
        ))}
      </Select>
      <Select label="Treinamento" optional={!allLabel} value={serviceId} onChange={(event) => onService(event.target.value)} error={fieldError(errors, "serviceId")}>
        <option value="">{allLabel ?? "Nenhum"}</option>
        {services.map((service) => (
          <option key={service.id} value={service.id}>
            {service.title}
          </option>
        ))}
      </Select>
    </FieldRow>
  );
}

export function SessionsList() {
  const [page, setPage] = useState(1);
  const [search, setSearch] = useState("");
  const [query, setQuery] = useState("");
  const [clientId, setClientId] = useState("");
  const [serviceId, setServiceId] = useState("");
  const params = new URLSearchParams({ page: String(page), pageSize: "20" });
  if (query) params.set("search", query);
  if (clientId) params.set("clientId", clientId);
  if (serviceId) params.set("serviceId", serviceId);
  const { data, error, reload } = useApi<Paged<FeedbackSessionListItem>>(`/admin/feedback-sessions?${params}`);

  return (
    <div>
      <AdminHeader
        kicker="Avaliações"
        title="Turmas"
        description="Cada turma tem um QR Code próprio. Os participantes respondem anonimamente enquanto ela estiver aberta."
        actions={<NewButton href={`${LIST}/nova`}>Nova turma</NewButton>}
      />
      <FlashNotice />
      <form
        role="search"
        className="mb-8 grid gap-4"
        onSubmit={(event) => {
          event.preventDefault();
          setPage(1);
          setQuery(search.trim());
        }}
      >
        <LinkSelects
          clientId={clientId}
          serviceId={serviceId}
          allLabel="Todos"
          onClient={(id) => {
            setPage(1);
            setClientId(id);
          }}
          onService={(id) => {
            setPage(1);
            setServiceId(id);
          }}
        />
        <div className="flex items-end gap-3">
          <div className="flex-1">
            <Input label="Buscar pelo nome da turma" type="search" value={search} maxLength={100} onChange={(event) => setSearch(event.target.value)} />
          </div>
          <Button type="submit" variant="outline" className="min-h-[3.125rem]">
            Buscar
          </Button>
        </div>
      </form>
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.items.length === 0 && (
        <EmptyState title="Nenhuma turma encontrada" action={<NewButton href={`${LIST}/nova`}>Criar turma</NewButton>}>
          Crie a turma antes do treinamento e projete ou imprima o QR Code no fim.
        </EmptyState>
      )}
      {data && data.items.length > 0 && (
        <>
          <IndexList>
            {data.items.map((session) => (
              <IndexRow
                key={session.id}
                href={`${LIST}/${session.id}`}
                title={session.title}
                meta={`${formatDateTime(session.opensAt)} até ${formatDateTime(session.closesAt)} · ${plural(session.responseCount, "resposta", "respostas")}${session.maxResponses ? ` de ${session.maxResponses}` : ""}`}
                tags={sessionStatusTag(session.status)}
              />
            ))}
          </IndexList>
          <Pagination page={page} totalPages={data.totalPages} onChange={setPage} />
        </>
      )}
    </div>
  );
}

type Schedule = { title: string; opensAt: string; closesAt: string; maxResponses: string; clientId: string; serviceId: string };

function schedulePayload(form: Schedule) {
  return {
    title: form.title,
    opensAt: fromLocalInput(form.opensAt),
    closesAt: fromLocalInput(form.closesAt),
    maxResponses: form.maxResponses.trim() ? Number.parseInt(form.maxResponses, 10) : null,
    clientId: form.clientId || null,
    serviceId: form.serviceId || null,
  };
}

function ScheduleFields({ form, onChange, errors }: { form: Schedule; onChange: (form: Schedule) => void; errors?: Record<string, string[]> }) {
  return (
    <>
      <Input
        label="Nome da turma"
        hint="Aparece para os participantes. Ex.: “Loja Centro — Atendimento (manhã)”."
        value={form.title}
        maxLength={200}
        onChange={(event) => onChange({ ...form, title: event.target.value })}
        error={fieldError(errors, "title")}
      />
      <FieldRow>
        <Input
          label="Abre em"
          type="datetime-local"
          value={form.opensAt}
          onChange={(event) => onChange({ ...form, opensAt: event.target.value })}
          error={fieldError(errors, "opensAt")}
        />
        <Input
          label="Fecha em"
          type="datetime-local"
          value={form.closesAt}
          onChange={(event) => onChange({ ...form, closesAt: event.target.value })}
          error={fieldError(errors, "closesAt")}
        />
      </FieldRow>
      <FieldRow>
        <Input
          label="Limite de respostas"
          optional
          hint="Normalmente o número de participantes."
          type="number"
          min={1}
          value={form.maxResponses}
          onChange={(event) => onChange({ ...form, maxResponses: event.target.value })}
          error={fieldError(errors, "maxResponses")}
        />
      </FieldRow>
      <LinkSelects
        clientId={form.clientId}
        serviceId={form.serviceId}
        errors={errors}
        onClient={(clientId) => onChange({ ...form, clientId })}
        onService={(serviceId) => onChange({ ...form, serviceId })}
      />
    </>
  );
}

export function SessionCreate() {
  const router = useRouter();
  const forms = useApi<FeedbackFormListItem[]>("/admin/feedback-forms");
  const today = todayLocal();
  const [formId, setFormId] = useState("");
  const [schedule, setSchedule] = useState<Schedule>({
    title: "",
    opensAt: `${today}T08:00`,
    closesAt: `${addDays(today, 1)}T23:00`,
    maxResponses: "",
    clientId: "",
    serviceId: "",
  });
  const { pending, errors, message, run } = useSubmit();
  const chosenForm = formId || forms.data?.[0]?.id || "";

  async function create() {
    const created = await run(() =>
      adminFetch<FeedbackSessionDetail>("/admin/feedback-sessions", { method: "POST", json: { formId: chosenForm, ...schedulePayload(schedule) } }),
    );
    if (created) {
      setFlash("Turma criada. Baixe o QR Code ou o cartaz na aba Divulgar.");
      router.replace(`${LIST}/${created.id}`);
    }
  }

  return (
    <div>
      <AdminHeader kicker="Turmas" title="Nova turma" back={{ href: LIST, label: "Todas as turmas" }} />
      {forms.error && <LoadError error={forms.error} onRetry={forms.reload} />}
      {forms.data?.length === 0 ? (
        <EmptyState title="Crie um formulário primeiro" action={<NewButton href="/painel/avaliacoes/formularios/novo">Criar formulário</NewButton>}>
          A turma copia as perguntas de um formulário.
        </EmptyState>
      ) : (
        <AdminForm pending={pending} message={message} submitLabel="Criar turma" onSubmit={() => void create()}>
          <Section title="Perguntas" description="A turma guarda uma cópia: editar o formulário depois não muda esta turma.">
            <Select label="Formulário" value={chosenForm} onChange={(event) => setFormId(event.target.value)} error={fieldError(errors, "formId")}>
              {forms.data?.map((form) => (
                <option key={form.id} value={form.id}>
                  {form.title} ({plural(form.questionCount, "pergunta", "perguntas")})
                </option>
              ))}
            </Select>
          </Section>
          <Section title="Agenda" description="Horário de Manaus. Uma turma fica aberta por no máximo 90 dias.">
            <ScheduleFields form={schedule} onChange={setSchedule} errors={errors} />
          </Section>
        </AdminForm>
      )}
    </div>
  );
}

function useObjectUrl(path: string) {
  const [result, setResult] = useState<{ path: string; url?: string; error?: string } | null>(null);

  useEffect(() => {
    let url: string | null = null;
    let active = true;
    adminDownload(path).then(
      ({ blob }) => {
        url = URL.createObjectURL(blob);
        if (active) setResult({ path, url });
      },
      () => active && setResult({ path, error: "Não foi possível gerar o QR Code." }),
    );
    return () => {
      active = false;
      if (url) URL.revokeObjectURL(url);
    };
  }, [path]);

  return result?.path === path ? result : null;
}

async function download(path: string, fallbackName: string) {
  const { blob, fileName } = await adminDownload(path);
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName ?? fallbackName;
  link.click();
  URL.revokeObjectURL(url);
}

function Share({ session }: { session: FeedbackSessionDetail }) {
  const qr = useObjectUrl(`/admin/feedback-sessions/${session.id}/qrcode`);
  const [copied, setCopied] = useState(false);
  const [problem, setProblem] = useState<string | null>(null);
  const linkId = useId();

  async function get(path: string, name: string) {
    setProblem(null);
    try {
      await download(path, name);
    } catch (error) {
      setProblem(error instanceof ApiError ? error.message : "Não foi possível baixar o arquivo.");
    }
  }

  return (
    <div className="grid gap-10 md:grid-cols-[18rem_1fr]">
      <div className="border border-ink p-2">
        <div className="grid aspect-square place-items-center bg-paper-deep">
          {qr?.url ? (
            // eslint-disable-next-line @next/next/no-img-element -- blob local, sem otimização possível
            <img src={qr.url} alt={`QR Code da turma ${session.title}`} className="size-full" />
          ) : (
            <Kicker role="status">{qr?.error ?? "Gerando…"}</Kicker>
          )}
        </div>
      </div>
      <div className="grid content-start gap-6">
        <div>
          <label htmlFor={linkId} className="mb-1.5 block font-sans text-sm font-medium">
            Endereço do formulário
          </label>
          <div className="flex gap-2">
            <input id={linkId} readOnly value={session.publicUrl} className="min-w-0 flex-1 rounded-xs border border-rule-strong bg-card px-3 py-2.5 font-mono text-sm" />
            <Button
              variant="outline"
              onClick={() => void navigator.clipboard.writeText(session.publicUrl).then(() => setCopied(true))}
            >
              {copied ? "Copiado" : "Copiar"}
            </Button>
          </div>
        </div>
        <div className="flex flex-wrap gap-3">
          <Button onClick={() => void get(`/admin/feedback-sessions/${session.id}/poster`, "cartaz.pdf")}>
            <DownloadIcon /> Cartaz para imprimir (PDF)
          </Button>
          <Button variant="outline" onClick={() => void get(`/admin/feedback-sessions/${session.id}/qrcode`, "qrcode.png")}>
            <DownloadIcon /> QR Code (PNG)
          </Button>
          <ButtonLink href={session.publicUrl} variant="quiet" target="_blank" rel="noopener">
            Abrir o formulário
          </ButtonLink>
        </div>
        {problem && <Notice tone="error">{problem}</Notice>}
        <Notice tone="info">
          O cartaz traz o QR Code, o período e o aviso de anonimato. Projete o QR no fim do treinamento e dê alguns minutos para a
          turma responder.
        </Notice>
      </div>
    </div>
  );
}

function ScheduleEditor({ session, onSaved }: { session: FeedbackSessionDetail; onSaved: (session: FeedbackSessionDetail) => void }) {
  const [form, setForm] = useState<Schedule>({
    title: session.title,
    opensAt: toLocalInput(session.opensAt),
    closesAt: toLocalInput(session.closesAt),
    maxResponses: session.maxResponses ? String(session.maxResponses) : "",
    clientId: session.clientId ?? "",
    serviceId: session.serviceId ?? "",
  });
  const [saved, setSaved] = useState(false);
  const { pending, errors, message, run } = useSubmit();

  return (
    <AdminForm
      pending={pending}
      message={message}
      savedNote={saved ? "Agenda salva." : null}
      submitLabel="Salvar agenda"
      onSubmit={() =>
        void run(() => adminFetch<FeedbackSessionDetail>(`/admin/feedback-sessions/${session.id}`, { method: "PUT", json: schedulePayload(form) })).then(
          (updated) => {
            if (updated) {
              setSaved(true);
              onSaved(updated);
            }
          },
        )
      }
    >
      <Section title="Agenda e vínculos" description="Horário de Manaus.">
        <ScheduleFields
          form={form}
          onChange={(next) => {
            setSaved(false);
            setForm(next);
          }}
          errors={errors}
        />
      </Section>
    </AdminForm>
  );
}

function QuestionsEditor({ session, onSaved }: { session: FeedbackSessionDetail; onSaved: (session: FeedbackSessionDetail) => void }) {
  const [title, setTitle] = useState(session.formTitle);
  const [description, setDescription] = useState(session.formDescription ?? "");
  const [definition, setDefinition] = useState<FormDefinition>(session.definition);
  const [saved, setSaved] = useState(false);
  const { pending, errors, message, run } = useSubmit();

  if (session.responseCount > 0) {
    return (
      <div className="grid gap-6">
        <Notice tone="privacy" title="Perguntas travadas">
          Esta turma já tem respostas, então as perguntas não mudam mais (respostas antigas perderiam o sentido).
        </Notice>
        <div>
          <PreviewButton title={session.formTitle} description={session.formDescription} definition={session.definition} />
        </div>
      </div>
    );
  }

  return (
    <AdminForm
      pending={pending}
      message={message}
      savedNote={saved ? "Perguntas salvas." : null}
      submitLabel="Salvar perguntas desta turma"
      extraActions={<PreviewButton title={title} description={description} definition={definition} />}
      onSubmit={() =>
        void run(() =>
          adminFetch<FeedbackSessionDetail>(`/admin/feedback-sessions/${session.id}/form`, {
            method: "PUT",
            json: { formTitle: title, formDescription: optional(description), definition: toPayload(definition) },
          }),
        ).then((updated) => {
          if (updated) {
            setSaved(true);
            onSaved(updated);
          }
        })
      }
    >
      <Notice tone="info" className="mb-8">
        Mudanças aqui valem só para esta turma, e só até a primeira resposta.
      </Notice>
      <Section title="Apresentação">
        <Input label="Título" value={title} maxLength={200} onChange={(event) => setTitle(event.target.value)} error={fieldError(errors, "formTitle")} />
        <Textarea
          label="Texto de abertura"
          optional
          value={description}
          onChange={(event) => setDescription(event.target.value)}
          error={fieldError(errors, "formDescription")}
          className="min-h-24"
        />
      </Section>
      <Section title="Perguntas">
        <FormBuilder value={definition} onChange={setDefinition} errors={errors} />
      </Section>
    </AdminForm>
  );
}

const tabs = [
  { id: "divulgar", label: "Divulgar" },
  { id: "resultados", label: "Resultados" },
  { id: "agenda", label: "Agenda" },
  { id: "perguntas", label: "Perguntas" },
] as const;

type Tab = (typeof tabs)[number]["id"];

function SessionView({ initial }: { initial: FeedbackSessionDetail }) {
  const router = useRouter();
  const flash = useFlash();
  const [session, setSession] = useState(initial);
  const [tab, setTab] = useState<Tab>(initial.responseCount > 0 ? "resultados" : "divulgar");
  const results = useApi<SessionResults>(tab === "resultados" ? `/admin/feedback-sessions/${session.id}/results` : null);
  const toggle = useSubmit();
  const tabsId = useId();
  const manuallyClosed = Boolean(session.closedAt);

  async function setOpen(open: boolean) {
    const updated = await toggle.run(() =>
      adminFetch<FeedbackSessionDetail>(`/admin/feedback-sessions/${session.id}/${open ? "reopen" : "close"}`, { method: "POST" }),
    );
    if (updated) setSession(updated);
  }

  async function remove() {
    try {
      await adminFetch(`/admin/feedback-sessions/${session.id}`, { method: "DELETE" });
      setFlash("Turma excluída.");
      router.push(LIST);
      return null;
    } catch (error) {
      return error instanceof ApiError ? error.message : "Não foi possível excluir.";
    }
  }

  return (
    <div>
      <AdminHeader
        kicker="Turma"
        title={session.title}
        back={{ href: LIST, label: "Todas as turmas" }}
        description={
          <span className="flex flex-wrap items-center gap-x-4 gap-y-2">
            {sessionStatusTag(session.status)}
            <span>
              {formatDateTime(session.opensAt)} até {formatDateTime(session.closesAt)}
            </span>
            <span>
              {plural(session.responseCount, "resposta", "respostas")}
              {session.maxResponses ? ` de ${session.maxResponses}` : ""}
            </span>
          </span>
        }
        actions={
          <>
            {session.status !== "Closed" && (
              <Button variant="outline" disabled={toggle.pending} onClick={() => void setOpen(false)}>
                Encerrar agora
              </Button>
            )}
            {manuallyClosed && (
              <Button variant="outline" disabled={toggle.pending} onClick={() => void setOpen(true)}>
                Reabrir
              </Button>
            )}
          </>
        }
      />
      {flash && (
        <Notice tone="success" role="status" className="mb-8">
          {flash}
        </Notice>
      )}
      {toggle.message && (
        <Notice tone="error" role="alert" className="mb-8">
          {toggle.message}
        </Notice>
      )}

      <div role="tablist" aria-label="Seções da turma" className="mb-10 flex flex-wrap gap-x-1 border-b-2 border-ink">
        {tabs.map((item) => (
          <button
            key={item.id}
            id={`${tabsId}-${item.id}`}
            type="button"
            role="tab"
            aria-selected={tab === item.id}
            aria-controls={`${tabsId}-${item.id}-panel`}
            onClick={() => setTab(item.id)}
            className={cn(
              "-mb-0.5 min-h-11 border-2 px-4 font-sans text-[0.9375rem] font-medium",
              tab === item.id ? "border-ink border-b-paper bg-paper text-ink" : "border-transparent text-ink-soft hover:text-ink",
            )}
          >
            {item.label}
          </button>
        ))}
      </div>

      <div role="tabpanel" id={`${tabsId}-${tab}-panel`} aria-labelledby={`${tabsId}-${tab}`}>
        {tab === "divulgar" && <Share session={session} />}
        {tab === "resultados" && (
          <>
            {results.error && <LoadError error={results.error} onRetry={results.reload} />}
            {results.loading && !results.data && <Loading />}
            {results.data && <SessionResultsView results={results.data} />}
          </>
        )}
        {tab === "agenda" && <ScheduleEditor session={session} onSaved={setSession} />}
        {tab === "perguntas" && <QuestionsEditor session={session} onSaved={setSession} />}
      </div>

      <div className="mt-16 border-t border-rule pt-6">
        <ConfirmButton
          label="Excluir turma"
          title="Excluir esta turma?"
          description={<p>A turma e os resultados saem do painel. O QR Code para de funcionar.</p>}
          confirmLabel="Excluir"
          onConfirm={remove}
        />
      </div>
    </div>
  );
}

export function SessionDetail({ id }: { id: string }) {
  const { data, error, reload } = useApi<FeedbackSessionDetail>(`/admin/feedback-sessions/${id}`);

  if (error) return <LoadError error={error} onRetry={reload} />;
  if (!data) return <Loading />;

  return <SessionView initial={data} />;
}

export function FeedbackSummaryPage() {
  const today = todayLocal();
  const [from, setFrom] = useState(addDays(today, -90));
  const [to, setTo] = useState(today);
  const [clientId, setClientId] = useState("");
  const [serviceId, setServiceId] = useState("");
  const params = new URLSearchParams();
  if (from) params.set("from", from);
  if (to) params.set("to", to);
  if (clientId) params.set("clientId", clientId);
  if (serviceId) params.set("serviceId", serviceId);
  const { data, error, reload } = useApi<FeedbackSummary>(`/admin/feedback-sessions/summary?${params}`);

  return (
    <div>
      <AdminHeader
        kicker="Avaliações"
        title="Resultados"
        description="Visão das turmas de um período. Turmas com menos de 3 respostas entram nos totais, mas não no NPS."
        actions={<NewButton href={`${LIST}/nova`}>Nova turma</NewButton>}
      />
      <div className="mb-10 grid gap-4">
        <FieldRow>
          <Input label="De" type="date" value={from} onChange={(event) => setFrom(event.target.value)} error={fieldError(error?.errors, "from")} />
          <Input label="Até" type="date" value={to} onChange={(event) => setTo(event.target.value)} error={fieldError(error?.errors, "to")} />
        </FieldRow>
        <LinkSelects clientId={clientId} serviceId={serviceId} allLabel="Todos" onClient={setClientId} onService={setServiceId} />
      </div>
      {error && !error.errors && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data && (
        <>
          <div className="grid gap-8 sm:grid-cols-3">
            <Stat label="Turmas" value={data.sessionCount} />
            <Stat label="Respostas" value={data.responseCount} />
            <Stat label="Média por turma" value={data.sessionCount ? Math.round(data.responseCount / data.sessionCount) : "—"} />
          </div>
          <div className="mt-10 border-2 border-ink p-6">
            {data.nps ? (
              <NpsBlock nps={data.nps} />
            ) : (
              <p className="font-sans text-[0.9375rem] text-ink-soft">O NPS do período aparece quando houver pelo menos 3 notas em turmas com 3 respostas ou mais.</p>
            )}
          </div>
          <h2 className="mt-14 mb-4 text-3xl">Turmas do período</h2>
          {data.sessions.length === 0 ? (
            <EmptyState title="Nenhuma turma no período" />
          ) : (
            <IndexList>
              {data.sessions.map((session) => (
                <IndexRow
                  key={session.id}
                  href={`${LIST}/${session.id}`}
                  title={session.title}
                  meta={`${formatShortDate(session.opensAt)} · ${plural(session.responseCount, "resposta", "respostas")}`}
                  tags={
                    <span className="font-mono text-sm">
                      NPS {session.nps ? <strong className="font-medium">{session.nps.score}</strong> : "—"}
                    </span>
                  }
                />
              ))}
            </IndexList>
          )}
        </>
      )}
    </div>
  );
}
