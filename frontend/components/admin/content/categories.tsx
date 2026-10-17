"use client";

import { useState, type FormEvent } from "react";
import { useApi } from "@/lib/admin/hooks";
import { plural } from "@/lib/admin/text";
import { adminFetch } from "@/lib/auth/session";
import { ApiError, fieldError } from "@/lib/api/client";
import type { AdminTaxonomyItem } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/field";
import { Notice } from "@/components/ui/notice";
import { AdminHeader, ConfirmButton, Dialog, LoadError, Loading, Section } from "../ui";

type Kind = "category" | "tag";

const api: Record<Kind, string> = { category: "/admin/post-categories", tag: "/admin/post-tags" };
const maxLength: Record<Kind, number> = { category: 60, tag: 40 };

function problemOf(failure: unknown, fallback: string) {
  return failure instanceof ApiError ? (fieldError(failure.errors, "name") ?? failure.message) : fallback;
}

function RenameDialog({ kind, item, onClose, onSaved }: { kind: Kind; item: AdminTaxonomyItem; onClose: () => void; onSaved: () => void }) {
  const [name, setName] = useState(item.name);
  const [pending, setPending] = useState(false);
  const [problem, setProblem] = useState<string | null>(null);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setPending(true);
    setProblem(null);
    try {
      await adminFetch(`${api[kind]}/${item.id}`, { method: "PUT", json: { name } });
      onSaved();
      onClose();
    } catch (failure) {
      setProblem(problemOf(failure, "Não foi possível renomear. Tente de novo."));
    } finally {
      setPending(false);
    }
  }

  return (
    <Dialog open onClose={onClose} title={kind === "category" ? "Renomear categoria" : "Renomear tag"}>
      <form onSubmit={submit} className="grid gap-5">
        <Input label="Nome" value={name} maxLength={maxLength[kind]} onChange={(event) => setName(event.target.value)} error={problem ?? undefined} autoFocus />
        <p className="font-sans text-sm text-ink-soft">
          O endereço muda junto com o nome. {item.postCount > 0 && `Vale para as ${item.postCount} publicações que usam ${kind === "category" ? "a categoria" : "a tag"}.`}
        </p>
        <div className="flex flex-wrap gap-3">
          <Button type="submit" disabled={pending}>
            {pending ? "Salvando…" : "Salvar"}
          </Button>
          <Button variant="quiet" onClick={onClose}>
            Cancelar
          </Button>
        </div>
      </form>
    </Dialog>
  );
}

function TaxonomyList({ kind, items, onChanged }: { kind: Kind; items: AdminTaxonomyItem[]; onChanged: () => void }) {
  const [renaming, setRenaming] = useState<AdminTaxonomyItem | null>(null);

  async function remove(item: AdminTaxonomyItem) {
    try {
      await adminFetch(`${api[kind]}/${item.id}`, { method: "DELETE" });
      onChanged();
      return null;
    } catch (failure) {
      return problemOf(failure, "Não foi possível excluir. Tente de novo.");
    }
  }

  return (
    <>
      <ul className="border-t-2 border-ink">
        {items.map((item) => (
          <li key={item.id} className="flex flex-wrap items-center gap-x-6 gap-y-2 border-b border-rule py-3">
            <span className="min-w-0 flex-1">
              <span className="block text-xl leading-snug">{kind === "tag" ? `#${item.name}` : item.name}</span>
              <span className="block font-sans text-sm text-ink-soft">
                {plural(item.postCount, "publicação", "publicações")} · /conteudo?{kind === "category" ? "categoria" : "tag"}=
                {item.slug}
              </span>
            </span>
            <span className="flex flex-wrap items-center gap-2">
              <Button variant="quiet" onClick={() => setRenaming(item)}>
                Renomear
              </Button>
              <ConfirmButton
                label="Excluir"
                variant="quiet"
                title={kind === "category" ? `Excluir a categoria “${item.name}”?` : `Excluir a tag “${item.name}”?`}
                description={
                  <p>
                    {item.postCount === 0
                      ? "Nenhuma publicação usa este nome."
                      : kind === "category"
                        ? item.postCount === 1
                          ? "A publicação que usa esta categoria fica sem categoria. Nada é apagado dela."
                          : `As ${item.postCount} publicações que usam esta categoria ficam sem categoria. Nada é apagado delas.`
                        : item.postCount === 1
                          ? "A tag sai da publicação que a usa. Nada mais muda nela."
                          : `A tag sai das ${item.postCount} publicações que a usam. Nada mais muda nelas.`}
                  </p>
                }
                confirmLabel="Excluir"
                onConfirm={() => remove(item)}
              />
            </span>
          </li>
        ))}
      </ul>
      {renaming && <RenameDialog kind={kind} item={renaming} onClose={() => setRenaming(null)} onSaved={onChanged} />}
    </>
  );
}

function NewCategoryForm({ onCreated }: { onCreated: () => void }) {
  const [name, setName] = useState("");
  const [pending, setPending] = useState(false);
  const [problem, setProblem] = useState<string | null>(null);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setPending(true);
    setProblem(null);
    try {
      await adminFetch(api.category, { method: "POST", json: { name } });
      setName("");
      onCreated();
    } catch (failure) {
      setProblem(problemOf(failure, "Não foi possível criar. Tente de novo."));
    } finally {
      setPending(false);
    }
  }

  return (
    <form onSubmit={submit} className="grid gap-3 sm:grid-cols-[1fr_auto] sm:items-start">
      <Input
        label="Nova categoria"
        placeholder="Ex.: Atendimento, Liderança, Vendas"
        value={name}
        maxLength={maxLength.category}
        onChange={(event) => setName(event.target.value)}
        error={problem ?? undefined}
      />
      <Button type="submit" variant="outline" disabled={pending || !name.trim()} className="sm:mt-7">
        {pending ? "Criando…" : "Criar"}
      </Button>
    </form>
  );
}

/** Categorias (temas) e tags das publicações: criar categorias, renomear e excluir as duas. */
export function TaxonomyPage() {
  const categories = useApi<AdminTaxonomyItem[]>(api.category);
  const tags = useApi<AdminTaxonomyItem[]>(api.tag);

  return (
    <div>
      <AdminHeader
        kicker="Conteúdo do site"
        title="Categorias e tags"
        description="A categoria é o tema da publicação (uma por publicação); as tags são palavras-chave (várias). No site, as duas viram filtros na página Conteúdo."
      />

      <Section title="Categorias" description="Só aparecem no site as que têm alguma publicação no ar.">
        <NewCategoryForm onCreated={categories.reload} />
        {categories.error && <LoadError error={categories.error} onRetry={categories.reload} />}
        {!categories.data && !categories.error && <Loading />}
        {categories.data?.length === 0 && <p className="font-sans text-[0.9375rem] text-ink-soft">Nenhuma categoria ainda.</p>}
        {categories.data && categories.data.length > 0 && <TaxonomyList kind="category" items={categories.data} onChanged={categories.reload} />}
      </Section>

      <Section title="Tags" description="Tags novas nascem no editor da publicação: basta digitar. Aqui dá para corrigir o nome ou apagar.">
        {tags.error && <LoadError error={tags.error} onRetry={tags.reload} />}
        {!tags.data && !tags.error && <Loading />}
        {tags.data?.length === 0 && (
          <Notice tone="info">Nenhuma tag ainda. Elas aparecem aqui depois de usadas numa publicação.</Notice>
        )}
        {tags.data && tags.data.length > 0 && <TaxonomyList kind="tag" items={tags.data} onChanged={tags.reload} />}
      </Section>
    </div>
  );
}
