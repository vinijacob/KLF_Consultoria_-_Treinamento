"use client";

import { useState } from "react";
import { formatShortDate, fromLocalInput, toLocalInput } from "@/lib/admin/datetime";
import { optional, toInt, useEntityEditor } from "@/lib/admin/editor";
import { useApi } from "@/lib/admin/hooks";
import { adminFetch } from "@/lib/auth/session";
import { ApiError, fieldError } from "@/lib/api/client";
import type { AdminTestimonial } from "@/types/admin";
import { Checkbox, Input, Textarea } from "@/components/ui/field";
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
              tags={stateTag(item)}
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

function TestimonialForm({ item, onReplace }: { item?: AdminTestimonial; onReplace: (item: AdminTestimonial) => void }) {
  const [form, setForm] = useState(() => toForm(item));
  const editor = useEntityEditor<AdminTestimonial>({
    apiPath: "/admin/testimonials",
    listHref: LIST,
    id: item?.id,
    created: "Depoimento criado.",
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
          <Notice tone="privacy">
            Guarde o termo assinado (PDF) em local seguro. O envio do arquivo para o painel, em armazenamento privado, ainda
            está em desenvolvimento.
          </Notice>
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
      {(item, replace) => <TestimonialForm key={item?.consentRevokedAt ?? "active"} item={item} onReplace={replace} />}
    </EntityLoader>
  );
}
