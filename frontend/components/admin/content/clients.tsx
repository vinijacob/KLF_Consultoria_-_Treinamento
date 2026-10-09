"use client";

import { useState } from "react";
import { optional, toInt, useEntityEditor } from "@/lib/admin/editor";
import { useApi } from "@/lib/admin/hooks";
import { fieldError } from "@/lib/api/client";
import { mediaUrl } from "@/lib/media";
import type { AdminClient } from "@/types/admin";
import { Checkbox, Input } from "@/components/ui/field";
import { EntityLoader, FlashNotice } from "../entity";
import { MediaField, Thumb } from "../media";
import { visibilityTag } from "../tags";
import { AdminForm, AdminHeader, ConfirmButton, EmptyState, FieldRow, IndexList, IndexRow, LoadError, Loading, NewButton, Section } from "../ui";

const LIST = "/painel/clientes";

export function ClientsList() {
  const { data, error, reload } = useApi<AdminClient[]>("/admin/clients");

  return (
    <div>
      <AdminHeader
        kicker="Conteúdo do site"
        title="Clientes"
        description="Empresas e lojas treinadas pela KLF, na página Clientes. Cadastre só quem autorizou aparecer."
        actions={<NewButton href={`${LIST}/novo`}>Novo cliente</NewButton>}
      />
      <FlashNotice />
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.length === 0 && (
        <EmptyState title="Nenhum cliente cadastrado" action={<NewButton href={`${LIST}/novo`}>Cadastrar cliente</NewButton>} />
      )}
      {data && data.length > 0 && (
        <IndexList>
          {data.map((client) => (
            <IndexRow
              key={client.id}
              href={`${LIST}/${client.id}`}
              number={client.displayOrder}
              title={client.name}
              meta={client.websiteUrl ?? "Sem site"}
              aside={client.logoId ? <Thumb src={mediaUrl(client.logoId)} alt="" className="hidden w-20 sm:block" /> : undefined}
              tags={visibilityTag(client.isActive)}
            />
          ))}
        </IndexList>
      )}
    </div>
  );
}

type Form = { name: string; websiteUrl: string; logoId: string | null; displayOrder: string; isActive: boolean };

function ClientForm({ client }: { client?: AdminClient }) {
  const [form, setForm] = useState<Form>(() => ({
    name: client?.name ?? "",
    websiteUrl: client?.websiteUrl ?? "",
    logoId: client?.logoId ?? null,
    displayOrder: String(client?.displayOrder ?? 0),
    isActive: client?.isActive ?? true,
  }));
  const editor = useEntityEditor<AdminClient>({
    apiPath: "/admin/clients",
    listHref: LIST,
    id: client?.id,
    created: "Cliente criado.",
    deleted: "Cliente excluído.",
  });
  const error = (field: string) => fieldError(editor.errors, field);
  const set = <K extends keyof Form>(key: K, value: Form[K]) => setForm((current) => ({ ...current, [key]: value }));

  return (
    <>
      <AdminHeader kicker="Clientes" title={client ? client.name : "Novo cliente"} back={{ href: LIST, label: "Todos os clientes" }} />
      <AdminForm
        pending={editor.pending}
        message={editor.message}
        savedNote={editor.savedNote}
        submitLabel={client ? "Salvar alterações" : "Criar cliente"}
        onSubmit={() =>
          void editor.save({ ...form, websiteUrl: optional(form.websiteUrl), displayOrder: toInt(form.displayOrder) })
        }
        extraActions={
          client && (
            <ConfirmButton
              label="Excluir cliente"
              title="Excluir este cliente?"
              description={<p>“{client.name}” sai do site e do painel. Para tirar só do site, desmarque “Mostrar no site”.</p>}
              confirmLabel="Excluir"
              onConfirm={editor.remove}
            />
          )
        }
      >
        <Section title="Cliente">
          <Input label="Nome" value={form.name} maxLength={200} onChange={(event) => set("name", event.target.value)} error={error("name")} />
          <Input
            label="Site"
            optional
            type="url"
            placeholder="https://"
            value={form.websiteUrl}
            maxLength={300}
            onChange={(event) => set("websiteUrl", event.target.value)}
            error={error("websiteUrl")}
          />
          <MediaField
            label="Logo"
            hint="Use o logo só com autorização da empresa."
            value={form.logoId}
            onChange={(id) => set("logoId", id)}
            error={error("logoId")}
          />
        </Section>
        <Section title="Publicação">
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
          <Checkbox label="Mostrar no site" checked={form.isActive} onChange={(event) => set("isActive", event.target.checked)} />
        </Section>
      </AdminForm>
    </>
  );
}

export function ClientEditor({ id }: { id?: string }) {
  return (
    <EntityLoader<AdminClient> path={id ? `/admin/clients/${id}` : null}>
      {(client) => <ClientForm client={client} />}
    </EntityLoader>
  );
}
