"use client";

import { useState } from "react";
import { optional, toInt, useEntityEditor } from "@/lib/admin/editor";
import { useApi } from "@/lib/admin/hooks";
import { slugify } from "@/lib/admin/text";
import { fieldError } from "@/lib/api/client";
import { formatWorkload, serviceFormatLabel } from "@/lib/format";
import type { AdminService, AdminServiceListItem } from "@/types/admin";
import type { ServiceFormat } from "@/types/site";
import { Checkbox, Input, Select, Textarea } from "@/components/ui/field";
import { EntityLoader, FlashNotice } from "../entity";
import { MediaField } from "../media";
import { RichTextEditor } from "../rich-text";
import { visibilityTag } from "../tags";
import { AdminForm, AdminHeader, ConfirmButton, EmptyState, FieldRow, IndexList, IndexRow, LoadError, Loading, NewButton, Section } from "../ui";

const LIST = "/painel/servicos";

export function ServicesList() {
  const { data, error, reload } = useApi<AdminServiceListItem[]>("/admin/services");

  return (
    <div>
      <AdminHeader
        kicker="Conteúdo do site"
        title="Serviços"
        description="Treinamentos e palestras mostrados em /servicos. A ordem de exibição define a posição no site."
        actions={<NewButton href={`${LIST}/novo`}>Novo serviço</NewButton>}
      />
      <FlashNotice />
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.length === 0 && (
        <EmptyState title="Nenhum serviço cadastrado" action={<NewButton href={`${LIST}/novo`}>Cadastrar o primeiro</NewButton>}>
          Cadastre os treinamentos e palestras que a KLF oferece.
        </EmptyState>
      )}
      {data && data.length > 0 && (
        <IndexList>
          {data.map((service) => (
            <IndexRow
              key={service.id}
              href={`${LIST}/${service.id}`}
              number={service.displayOrder}
              title={service.title}
              meta={`${serviceFormatLabel[service.format]} · ${formatWorkload(service.workloadHours)} · /servicos/${service.slug}`}
              tags={visibilityTag(service.isActive)}
            />
          ))}
        </IndexList>
      )}
    </div>
  );
}

type Form = {
  title: string;
  slug: string;
  summary: string;
  contentHtml: string;
  audience: string;
  workloadHours: string;
  format: ServiceFormat;
  coverId: string | null;
  displayOrder: string;
  isActive: boolean;
};

function toForm(service?: AdminService): Form {
  return {
    title: service?.title ?? "",
    slug: service?.slug ?? "",
    summary: service?.summary ?? "",
    contentHtml: service?.contentHtml ?? "",
    audience: service?.audience ?? "",
    workloadHours: String(service?.workloadHours ?? 8),
    format: service?.format ?? "InCompany",
    coverId: service?.coverId ?? null,
    displayOrder: String(service?.displayOrder ?? 0),
    isActive: service?.isActive ?? true,
  };
}

function ServiceForm({ service }: { service?: AdminService }) {
  const [form, setForm] = useState(() => toForm(service));
  const [slugTouched, setSlugTouched] = useState(Boolean(service));
  const editor = useEntityEditor<AdminService>({
    apiPath: "/admin/services",
    listHref: LIST,
    id: service?.id,
    created: "Serviço criado.",
    deleted: "Serviço excluído.",
  });
  const error = (field: string) => fieldError(editor.errors, field);
  const set = <K extends keyof Form>(key: K, value: Form[K]) => setForm((current) => ({ ...current, [key]: value }));

  return (
    <>
      <AdminHeader
        kicker="Serviços"
        title={service ? service.title : "Novo serviço"}
        back={{ href: LIST, label: "Todos os serviços" }}
        description={service && <>No site: /servicos/{service.slug}</>}
      />
      <AdminForm
        pending={editor.pending}
        message={editor.message}
        savedNote={editor.savedNote}
        submitLabel={service ? "Salvar alterações" : "Criar serviço"}
        onSubmit={() =>
          void editor.save({
            ...form,
            summary: optional(form.summary),
            workloadHours: toInt(form.workloadHours),
            displayOrder: toInt(form.displayOrder),
          })
        }
        extraActions={
          service && (
            <ConfirmButton
              label="Excluir serviço"
              title="Excluir este serviço?"
              description={
                <p>
                  “{service.title}” sai do site e do painel. Se quiser só tirar do site por um tempo, desmarque “Mostrar no site”.
                </p>
              }
              confirmLabel="Excluir"
              onConfirm={editor.remove}
            />
          )
        }
      >
        <Section title="Identificação" description="Como o serviço aparece na lista e no endereço da página.">
          <Input
            label="Título"
            value={form.title}
            maxLength={200}
            onChange={(event) => {
              const title = event.target.value;
              setForm((current) => ({ ...current, title, slug: slugTouched ? current.slug : slugify(title) }));
            }}
            error={error("title")}
          />
          <Input
            label="Endereço (slug)"
            hint="Parte final do link: só letras minúsculas, números e hífens."
            value={form.slug}
            maxLength={200}
            onChange={(event) => {
              setSlugTouched(true);
              set("slug", event.target.value);
            }}
            error={error("slug")}
          />
          <Textarea
            label="Resumo"
            optional
            hint="Uma ou duas frases para a lista de serviços."
            value={form.summary}
            maxLength={500}
            onChange={(event) => set("summary", event.target.value)}
            error={error("summary")}
            className="min-h-24"
          />
        </Section>

        <Section title="Ficha técnica" description="Aparece ao lado do texto, na página do serviço.">
          <FieldRow>
            <Select label="Formato" value={form.format} onChange={(event) => set("format", event.target.value as ServiceFormat)} error={error("format")}>
              {(Object.keys(serviceFormatLabel) as ServiceFormat[]).map((format) => (
                <option key={format} value={format}>
                  {serviceFormatLabel[format]}
                </option>
              ))}
            </Select>
            <Input
              label="Carga horária (horas)"
              type="number"
              min={1}
              max={2000}
              inputMode="numeric"
              value={form.workloadHours}
              onChange={(event) => set("workloadHours", event.target.value)}
              error={error("workloadHours")}
            />
          </FieldRow>
          <Input
            label="Público"
            hint="Ex.: equipes de vendas e atendimento de lojas."
            value={form.audience}
            maxLength={200}
            onChange={(event) => set("audience", event.target.value)}
            error={error("audience")}
          />
        </Section>

        <Section title="Página do serviço">
          <RichTextEditor
            label="Texto"
            hint="O que é, para quem é, o que a equipe aprende e como funciona."
            initialHtml={service?.contentHtml}
            onChange={(value) => set("contentHtml", value.html)}
            error={error("contentHtml")}
          />
          <MediaField label="Imagem de capa" value={form.coverId} onChange={(id) => set("coverId", id)} error={error("coverId")} />
        </Section>

        <Section title="Publicação">
          <FieldRow>
            <Input
              label="Ordem de exibição"
              hint="Menor número aparece primeiro."
              type="number"
              min={0}
              inputMode="numeric"
              value={form.displayOrder}
              onChange={(event) => set("displayOrder", event.target.value)}
              error={error("displayOrder")}
            />
          </FieldRow>
          <Checkbox
            label="Mostrar no site"
            hint="Desmarque para esconder sem excluir."
            checked={form.isActive}
            onChange={(event) => set("isActive", event.target.checked)}
          />
        </Section>
      </AdminForm>
    </>
  );
}

export function ServiceEditor({ id }: { id?: string }) {
  return (
    <EntityLoader<AdminService> path={id ? `/admin/services/${id}` : null}>
      {(service) => <ServiceForm service={service} />}
    </EntityLoader>
  );
}
