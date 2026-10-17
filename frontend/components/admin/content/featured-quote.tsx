"use client";

import { useState } from "react";
import { copy } from "@/content/site-copy";
import { formatDateTime } from "@/lib/admin/datetime";
import { optional } from "@/lib/admin/editor";
import { useApi, useSubmit } from "@/lib/admin/hooks";
import { adminFetch } from "@/lib/auth/session";
import { fieldError } from "@/lib/api/client";
import { Checkbox, Input, Textarea } from "@/components/ui/field";
import { Kicker } from "@/components/ui/typography";
import { AdminForm, AdminHeader, FieldRow, LoadError, Loading, Section } from "../ui";

type Quote = { text: string; author: string; source?: string | null; isPublished: boolean; updatedAt: string };

const TEXT_MAX = 400;

function QuoteForm({ initial }: { initial: Quote | null }) {
  const [form, setForm] = useState({
    text: initial?.text ?? "",
    author: initial?.author ?? copy.person.name,
    source: initial?.source ?? "",
    isPublished: initial?.isPublished ?? true,
  });
  const [savedAt, setSavedAt] = useState<string | null>(initial?.updatedAt ?? null);
  const submit = useSubmit();
  const error = (field: string) => fieldError(submit.errors, field);

  async function save() {
    const saved = await submit.run(() =>
      adminFetch<Quote>("/admin/featured-quote", {
        method: "PUT",
        json: { text: form.text, author: form.author, source: optional(form.source), isPublished: form.isPublished },
      }),
    );
    if (saved) setSavedAt(saved.updatedAt);
  }

  return (
    <AdminForm
      onSubmit={() => void save()}
      pending={submit.pending}
      message={submit.message}
      savedNote={savedAt ? `Última alteração em ${formatDateTime(savedAt)}.` : null}
    >
      <Section title="Citação" description="Uma frase só. Aparece numa faixa azul da página inicial, logo depois dos três pilares.">
        <Textarea
          label="Citação"
          hint={`Sem aspas: o site coloca. ${form.text.trim().length}/${TEXT_MAX} caracteres.`}
          value={form.text}
          maxLength={TEXT_MAX}
          onChange={(event) => setForm({ ...form, text: event.target.value })}
          error={error("text")}
          className="min-h-28"
        />
        <FieldRow>
          <Input label="Autor" value={form.author} maxLength={120} onChange={(event) => setForm({ ...form, author: event.target.value })} error={error("author")} />
          <Input
            label="Origem"
            optional
            placeholder="Ex.: palestra no Sebrae, 2025"
            value={form.source}
            maxLength={150}
            onChange={(event) => setForm({ ...form, source: event.target.value })}
            error={error("source")}
          />
        </FieldRow>
        <Checkbox
          label="Mostrar na página inicial"
          hint="Desmarque para tirar a faixa do site sem perder o texto."
          checked={form.isPublished}
          onChange={(event) => setForm({ ...form, isPublished: event.target.checked })}
        />
      </Section>

      {form.text.trim() && (
        <Section title="Como fica">
          <figure className="relative overflow-hidden rounded-3xl bg-brand px-8 pt-14 pb-10 text-brand-contrast">
            <span aria-hidden className="absolute top-1 left-6 font-serif text-[7rem] leading-none select-none">
              “
            </span>
            <blockquote className="relative">
              <p className="font-serif text-[1.75rem] leading-[1.2] italic">{form.text.trim()}</p>
            </blockquote>
            <figcaption className="relative mt-6 flex flex-wrap items-center gap-x-4 gap-y-1 font-mono text-xs uppercase tracking-[0.16em]">
              <span aria-hidden className="h-px w-10 bg-brand-contrast" />
              <span>{form.author.trim()}</span>
              {form.source.trim() && <span className="font-sans text-sm tracking-normal normal-case">{form.source.trim()}</span>}
            </figcaption>
          </figure>
          {!form.isPublished && <Kicker>Oculta: o site não mostra a faixa</Kicker>}
        </Section>
      )}
    </AdminForm>
  );
}

/** A citação da Kilciene na página inicial: uma só, criada aqui na primeira vez e editada depois. */
export function FeaturedQuotePage() {
  const { data, error, reload } = useApi<Quote>("/admin/featured-quote");
  const empty = error?.status === 404;

  return (
    <div>
      <AdminHeader
        kicker="Conteúdo do site"
        title="Citação"
        description="Uma frase da Kilciene em destaque, só na página inicial."
      />
      {error && !empty && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {(data || empty) && <QuoteForm initial={data ?? null} />}
    </div>
  );
}
