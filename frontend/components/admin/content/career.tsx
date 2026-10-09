"use client";

import { useState } from "react";
import { optional, toInt, useEntityEditor } from "@/lib/admin/editor";
import { useApi } from "@/lib/admin/hooks";
import { fieldError } from "@/lib/api/client";
import { formatMonthYear } from "@/lib/format";
import type { AdminCareerEntry } from "@/types/admin";
import type { CareerEntryType } from "@/types/site";
import { Checkbox, Input, Select, Textarea } from "@/components/ui/field";
import { Kicker } from "@/components/ui/typography";
import { EntityLoader, FlashNotice } from "../entity";
import { AdminForm, AdminHeader, ConfirmButton, EmptyState, FieldRow, IndexList, IndexRow, LoadError, Loading, NewButton, Section, Tag } from "../ui";

const LIST = "/painel/trajetoria";

const typeLabel: Record<CareerEntryType, string> = {
  Experience: "Experiência",
  Education: "Formação",
  Certification: "Certificação",
};

const groupOrder: CareerEntryType[] = ["Experience", "Education", "Certification"];
const groupTitle: Record<CareerEntryType, string> = {
  Experience: "Experiência profissional",
  Education: "Formação",
  Certification: "Certificações",
};

function period(entry: AdminCareerEntry) {
  return `${formatMonthYear(entry.startDate)} – ${entry.endDate ? formatMonthYear(entry.endDate) : "atual"}`;
}

export function CareerList() {
  const { data, error, reload } = useApi<AdminCareerEntry[]>("/admin/career");

  return (
    <div>
      <AdminHeader
        kicker="Conteúdo do site"
        title="Trajetória"
        description="Experiência, formação e certificações da página Sobre."
        actions={<NewButton href={`${LIST}/novo`}>Novo item</NewButton>}
      />
      <FlashNotice />
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.length === 0 && (
        <EmptyState title="Trajetória vazia" action={<NewButton href={`${LIST}/novo`}>Adicionar o primeiro item</NewButton>}>
          Cadastre empregos, cursos e certificações.
        </EmptyState>
      )}
      {data &&
        data.length > 0 &&
        groupOrder.map((type) => {
          const entries = data.filter((entry) => entry.entryType === type);
          if (entries.length === 0) return null;

          return (
            <section key={type} className="mb-10">
              <Kicker className="mb-3">{groupTitle[type]}</Kicker>
              <IndexList>
                {entries.map((entry) => (
                  <IndexRow
                    key={entry.id}
                    href={`${LIST}/${entry.id}`}
                    number={entry.displayOrder}
                    title={entry.title}
                    meta={[entry.institution, period(entry)].filter(Boolean).join(" · ")}
                    tags={entry.isOngoing ? <Tag tone="brand">Atual</Tag> : undefined}
                  />
                ))}
              </IndexList>
            </section>
          );
        })}
    </div>
  );
}

type Form = {
  entryType: CareerEntryType;
  title: string;
  institution: string;
  description: string;
  startDate: string;
  endDate: string;
  ongoing: boolean;
  displayOrder: string;
};

function toForm(entry?: AdminCareerEntry): Form {
  return {
    entryType: entry?.entryType ?? "Experience",
    title: entry?.title ?? "",
    institution: entry?.institution ?? "",
    description: entry?.description ?? "",
    startDate: entry?.startDate ?? "",
    endDate: entry?.endDate ?? "",
    ongoing: entry ? !entry.endDate : false,
    displayOrder: String(entry?.displayOrder ?? 0),
  };
}

function CareerForm({ entry }: { entry?: AdminCareerEntry }) {
  const [form, setForm] = useState(() => toForm(entry));
  const editor = useEntityEditor<AdminCareerEntry>({
    apiPath: "/admin/career",
    listHref: LIST,
    id: entry?.id,
    created: "Item criado.",
    deleted: "Item excluído.",
  });
  const error = (field: string) => fieldError(editor.errors, field);
  const set = <K extends keyof Form>(key: K, value: Form[K]) => setForm((current) => ({ ...current, [key]: value }));
  const institutionLabel = form.entryType === "Experience" ? "Empresa" : "Instituição";

  return (
    <>
      <AdminHeader kicker="Trajetória" title={entry ? entry.title : "Novo item"} back={{ href: LIST, label: "Toda a trajetória" }} />
      <AdminForm
        pending={editor.pending}
        message={editor.message}
        savedNote={editor.savedNote}
        submitLabel={entry ? "Salvar alterações" : "Criar item"}
        onSubmit={() =>
          void editor.save({
            entryType: form.entryType,
            title: form.title,
            institution: optional(form.institution),
            description: optional(form.description),
            startDate: form.startDate || null,
            endDate: form.ongoing ? null : form.endDate || null,
            displayOrder: toInt(form.displayOrder),
          })
        }
        extraActions={
          entry && (
            <ConfirmButton
              label="Excluir item"
              title="Excluir este item da trajetória?"
              description={<p>“{entry.title}” sai da página Sobre.</p>}
              confirmLabel="Excluir"
              onConfirm={editor.remove}
            />
          )
        }
      >
        <Section title="O quê">
          <Select label="Tipo" value={form.entryType} onChange={(event) => set("entryType", event.target.value as CareerEntryType)} error={error("entryType")}>
            {groupOrder.map((type) => (
              <option key={type} value={type}>
                {typeLabel[type]}
              </option>
            ))}
          </Select>
          <Input
            label={form.entryType === "Experience" ? "Cargo" : "Curso ou certificação"}
            value={form.title}
            maxLength={150}
            onChange={(event) => set("title", event.target.value)}
            error={error("title")}
          />
          <Input
            label={institutionLabel}
            optional
            value={form.institution}
            maxLength={150}
            onChange={(event) => set("institution", event.target.value)}
            error={error("institution")}
          />
          <Textarea
            label="Descrição"
            optional
            value={form.description}
            maxLength={1000}
            onChange={(event) => set("description", event.target.value)}
            error={error("description")}
          />
        </Section>
        <Section title="Quando" description="Só o mês e o ano aparecem no site.">
          <FieldRow>
            <Input label="Início" type="date" value={form.startDate} onChange={(event) => set("startDate", event.target.value)} error={error("startDate")} />
            {!form.ongoing && (
              <Input label="Término" type="date" value={form.endDate} onChange={(event) => set("endDate", event.target.value)} error={error("endDate")} />
            )}
          </FieldRow>
          <Checkbox label="Ainda em andamento" checked={form.ongoing} onChange={(event) => set("ongoing", event.target.checked)} />
          <FieldRow>
            <Input
              label="Ordem de exibição"
              hint="Menor número aparece primeiro dentro do grupo."
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

export function CareerEditor({ id }: { id?: string }) {
  return (
    <EntityLoader<AdminCareerEntry> path={id ? `/admin/career/${id}` : null}>
      {(entry) => <CareerForm entry={entry} />}
    </EntityLoader>
  );
}
