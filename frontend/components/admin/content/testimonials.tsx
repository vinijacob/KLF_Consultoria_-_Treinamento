"use client";

import { useId, useRef, useState } from "react";
import { formatShortDate, fromLocalInput, toLocalInput } from "@/lib/admin/datetime";
import { optional, toInt, useEntityEditor } from "@/lib/admin/editor";
import { useApi } from "@/lib/admin/hooks";
import { formatBytes } from "@/lib/admin/text";
import { adminDownload, adminFetch } from "@/lib/auth/session";
import { ApiError, fieldError } from "@/lib/api/client";
import type { AdminTestimonial } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Checkbox, FieldError, Input, Textarea } from "@/components/ui/field";
import { DownloadIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { EntityLoader, FlashNotice } from "../entity";
import { MediaField } from "../media";
import { AdminForm, AdminHeader, ConfirmButton, EmptyState, FieldRow, IndexList, IndexRow, LoadError, Loading, NewButton, Section, Tag } from "../ui";

const LIST = "/painel/depoimentos";

function stateTag(item: AdminTestimonial) {
  if (item.consentRevokedAt) return <Tag tone="danger">Consentimento revogado</Tag>;
  if (!item.consentGivenAt) return <Tag tone="warning">Sem consentimento</Tag>;
  return item.isPublished ? <Tag tone="success">No site</Tag> : <Tag>Oculto</Tag>;
}

function listTags(item: AdminTestimonial) {
  return (
    <>
      {stateTag(item)}
      {item.consentGivenAt && !item.consentDocument && <Tag tone="warning">Sem termo em PDF</Tag>}
    </>
  );
}

const PDF_MAX_BYTES = 10 * 1024 * 1024;

/**
 * Termo de consentimento assinado (PDF). Vai na hora para o armazenamento privado da API (não depende do botão
 * Salvar) e só sai de lá baixado por quem está no painel.
 */
function ConsentDocumentField({ item, onReplace }: { item: AdminTestimonial; onReplace: (item: AdminTestimonial) => void }) {
  const inputRef = useRef<HTMLInputElement>(null);
  const hintId = useId();
  const [pending, setPending] = useState(false);
  const [problem, setProblem] = useState<string | null>(null);
  const path = `/admin/testimonials/${item.id}/consent-document`;
  const attached = item.consentDocument;

  async function upload(file: File) {
    setProblem(null);
    if (file.type && file.type !== "application/pdf") return setProblem("Envie o termo em PDF.");
    if (file.size > PDF_MAX_BYTES) return setProblem(`O arquivo tem ${formatBytes(file.size)}; o limite é 10 MB.`);

    const body = new FormData();
    body.append("file", file);
    setPending(true);
    try {
      onReplace(await adminFetch<AdminTestimonial>(path, { method: "PUT", body }));
    } catch (failure) {
      setProblem(
        failure instanceof ApiError ? (fieldError(failure.errors, "file") ?? failure.message) : "Não foi possível enviar o termo. Tente de novo.",
      );
    } finally {
      setPending(false);
    }
  }

  async function download() {
    setProblem(null);
    try {
      const { blob, fileName } = await adminDownload(path);
      const url = URL.createObjectURL(blob);
      const link = document.createElement("a");
      link.href = url;
      link.download = fileName ?? "termo.pdf";
      link.click();
      URL.revokeObjectURL(url);
    } catch (failure) {
      setProblem(failure instanceof ApiError ? failure.message : "Não foi possível baixar o termo. Tente de novo.");
    }
  }

  async function remove() {
    try {
      onReplace(await adminFetch<AdminTestimonial>(path, { method: "DELETE" }));
      setProblem(null);
      return null;
    } catch (failure) {
      return failure instanceof ApiError ? failure.message : "Não foi possível remover o termo. Tente de novo.";
    }
  }

  return (
    <div className="rounded-2xl border border-rule bg-card p-5">
      <p className="font-sans font-medium">Termo assinado (PDF)</p>
      <p id={hintId} className="mt-1 font-sans text-sm leading-relaxed text-ink-soft">
        Fica guardado em área privada: não aparece no site e só quem entra no painel consegue baixar. O envio é na hora,
        sem precisar salvar. Até 10 MB.
      </p>

      {attached && (
        <p className="mt-4 font-sans text-[0.9375rem]">
          <span className="font-medium break-all">{attached.fileName}</span>
          <span className="text-ink-soft">
            {" "}
            · {formatBytes(attached.sizeBytes)} · enviado em {formatShortDate(attached.uploadedAt)}
          </span>
        </p>
      )}

      <input
        ref={inputRef}
        type="file"
        accept="application/pdf,.pdf"
        className="sr-only"
        tabIndex={-1}
        aria-hidden
        onChange={(event) => {
          const file = event.target.files?.[0];
          event.target.value = "";
          if (file) void upload(file);
        }}
      />

      <div className="mt-4 flex flex-wrap items-center gap-3">
        {attached && (
          <Button variant="outline" onClick={() => void download()}>
            <DownloadIcon /> Baixar termo
          </Button>
        )}
        <Button variant={attached ? "quiet" : "outline"} disabled={pending} aria-describedby={hintId} onClick={() => inputRef.current?.click()}>
          {pending ? "Enviando…" : attached ? "Trocar arquivo" : "Anexar termo em PDF"}
        </Button>
        {attached && (
          <ConfirmButton
            label="Remover"
            title="Remover o termo?"
            description={<p>O arquivo {attached.fileName} é apagado do armazenamento e não pode ser recuperado. O depoimento continua como está.</p>}
            confirmLabel="Remover termo"
            variant="quiet"
            onConfirm={remove}
          />
        )}
      </div>
      <div aria-live="polite">{problem && <FieldError>{problem}</FieldError>}</div>
    </div>
  );
}

export function TestimonialsList() {
  const { data, error, reload } = useApi<AdminTestimonial[]>("/admin/testimonials");

  return (
    <div>
      <AdminHeader
        kicker="Conteúdo do site"
        title="Depoimentos"
        description="Só vão para o site com o consentimento do autor registrado. Revogar o consentimento tira o depoimento do ar para sempre."
        actions={<NewButton href={`${LIST}/novo`}>Novo depoimento</NewButton>}
      />
      <FlashNotice />
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.length === 0 && (
        <EmptyState title="Nenhum depoimento ainda" action={<NewButton href={`${LIST}/novo`}>Cadastrar depoimento</NewButton>}>
          Peça autorização por escrito antes de publicar.
        </EmptyState>
      )}
      {data && data.length > 0 && (
        <IndexList>
          {data.map((item) => (
            <IndexRow
              key={item.id}
              href={`${LIST}/${item.id}`}
              number={item.displayOrder}
              title={item.authorName}
              meta={
                <>
                  {[item.authorRole, item.companyName].filter(Boolean).join(", ")}
                  <span className="mt-1 block truncate italic">“{item.quote}”</span>
                </>
              }
              tags={listTags(item)}
            />
          ))}
        </IndexList>
      )}
    </div>
  );
}

type Form = {
  authorName: string;
  authorRole: string;
  companyName: string;
  quote: string;
  photoId: string | null;
  consentDate: string;
  consentCoversImage: boolean;
  isPublished: boolean;
  displayOrder: string;
};

function toForm(item?: AdminTestimonial): Form {
  return {
    authorName: item?.authorName ?? "",
    authorRole: item?.authorRole ?? "",
    companyName: item?.companyName ?? "",
    quote: item?.quote ?? "",
    photoId: item?.photoId ?? null,
    consentDate: toLocalInput(item?.consentGivenAt).slice(0, 10),
    consentCoversImage: item?.consentCoversImage ?? false,
    isPublished: item?.isPublished ?? false,
    displayOrder: String(item?.displayOrder ?? 0),
  };
}

function TestimonialForm({
  item,
  onReplace,
  onReset,
}: {
  item?: AdminTestimonial;
  onReplace: (item: AdminTestimonial) => void;
  onReset: () => void;
}) {
  const [form, setForm] = useState(() => toForm(item));
  const editor = useEntityEditor<AdminTestimonial>({
    apiPath: "/admin/testimonials",
    listHref: LIST,
    id: item?.id,
    created: "Depoimento criado.",
    onReset,
    deleted: "Depoimento excluído.",
  });
  const error = (field: string) => fieldError(editor.errors, field);
  const set = <K extends keyof Form>(key: K, value: Form[K]) => setForm((current) => ({ ...current, [key]: value }));
  const revoked = Boolean(item?.consentRevokedAt);

  async function revoke() {
    try {
      onReplace(await adminFetch<AdminTestimonial>(`/admin/testimonials/${item!.id}/revoke-consent`, { method: "POST" }));
      return null;
    } catch (problem) {
      return problem instanceof ApiError ? problem.message : "Não foi possível revogar. Tente de novo.";
    }
  }

  return (
    <>
      <AdminHeader
        kicker="Depoimentos"
        title={item ? item.authorName : "Novo depoimento"}
        back={{ href: LIST, label: "Todos os depoimentos" }}
        description={item && stateTag(item)}
      />
      {revoked && (
        <Notice tone="error" title={`Consentimento revogado em ${formatShortDate(item!.consentRevokedAt!)}`} className="mb-8">
          Este depoimento não volta ao site. Se a pessoa autorizar de novo, cadastre um depoimento novo com o novo termo.
        </Notice>
      )}
      <AdminForm
        pending={editor.pending}
        message={editor.message}
        savedNote={editor.savedNote}
        submitLabel={item ? "Salvar alterações" : "Criar depoimento"}
        onSubmit={() =>
          void editor.save({
            authorName: form.authorName,
            authorRole: optional(form.authorRole),
            companyName: optional(form.companyName),
            quote: form.quote,
            photoId: form.photoId,
            consentGivenAt: form.consentDate ? fromLocalInput(`${form.consentDate}T12:00`) : null,
            consentCoversImage: form.consentCoversImage,
            isPublished: form.isPublished,
            displayOrder: toInt(form.displayOrder),
          })
        }
        extraActions={
          item && (
            <>
              {item.consentGivenAt && !revoked && (
                <ConfirmButton
                  label="Revogar consentimento"
                  title="Revogar o consentimento?"
                  description={
                    <p>
                      O depoimento de {item.authorName} sai do site agora e não pode ser publicado de novo. Use quando a pessoa
                      pedir para retirar a autorização (LGPD).
                    </p>
                  }
                  confirmLabel="Revogar"
                  variant="quiet"
                  onConfirm={revoke}
                />
              )}
              <ConfirmButton
                label="Excluir"
                title="Excluir este depoimento?"
                description={<p>O depoimento de {item.authorName} sai do site e do painel.</p>}
                confirmLabel="Excluir"
                onConfirm={editor.remove}
              />
            </>
          )
        }
      >
        <Section title="Depoimento">
          <Textarea
            label="Texto do depoimento"
            hint="Exatamente como a pessoa autorizou."
            value={form.quote}
            maxLength={1000}
            onChange={(event) => set("quote", event.target.value)}
            error={error("quote")}
          />
          <Input label="Nome" value={form.authorName} maxLength={120} onChange={(event) => set("authorName", event.target.value)} error={error("authorName")} />
          <FieldRow>
            <Input label="Cargo" optional value={form.authorRole} maxLength={120} onChange={(event) => set("authorRole", event.target.value)} error={error("authorRole")} />
            <Input label="Empresa" optional value={form.companyName} maxLength={200} onChange={(event) => set("companyName", event.target.value)} error={error("companyName")} />
          </FieldRow>
          <MediaField
            label="Foto da pessoa"
            hint="Só com autorização de uso de imagem no termo."
            value={form.photoId}
            onChange={(id) => set("photoId", id)}
            error={error("photoId")}
          />
        </Section>

        <Section
          title="Consentimento"
          description="Registre a data em que a pessoa assinou a autorização. Sem ela, o depoimento não pode ir ao site."
        >
          <FieldRow>
            <Input
              label="Data da autorização"
              type="date"
              value={form.consentDate}
              disabled={revoked}
              onChange={(event) => set("consentDate", event.target.value)}
              error={error("consentGivenAt")}
            />
          </FieldRow>
          <Checkbox
            label="A autorização inclui o uso da foto"
            checked={form.consentCoversImage}
            disabled={revoked}
            onChange={(event) => set("consentCoversImage", event.target.checked)}
            error={error("consentCoversImage")}
          />
          {item ? (
            <ConsentDocumentField item={item} onReplace={onReplace} />
          ) : (
            <Notice tone="privacy">Depois de criar o depoimento, abra-o de novo para anexar o termo assinado em PDF.</Notice>
          )}
        </Section>

        <Section title="Publicação">
          <Checkbox
            label="Mostrar no site"
            hint={revoked ? "Indisponível: consentimento revogado." : "Precisa da data da autorização."}
            checked={form.isPublished}
            disabled={revoked}
            onChange={(event) => set("isPublished", event.target.checked)}
            error={error("isPublished")}
          />
          <FieldRow>
            <Input
              label="Ordem de exibição"
              type="number"
              min={0}
              value={form.displayOrder}
              onChange={(event) => set("displayOrder", event.target.value)}
              error={error("displayOrder")}
            />
          </FieldRow>
        </Section>
      </AdminForm>
    </>
  );
}

export function TestimonialEditor({ id }: { id?: string }) {
  return (
    <EntityLoader<AdminTestimonial> path={id ? `/admin/testimonials/${id}` : null}>
      {(item, replace, reset) => <TestimonialForm key={item?.consentRevokedAt ?? "active"} item={item} onReplace={replace} onReset={reset} />}
    </EntityLoader>
  );
}
