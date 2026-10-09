"use client";

import { useState } from "react";
import { formatDateTime, fromLocalInput, toLocalInput } from "@/lib/admin/datetime";
import { optional, useEntityEditor } from "@/lib/admin/editor";
import { useApi } from "@/lib/admin/hooks";
import { slugify } from "@/lib/admin/text";
import { fieldError } from "@/lib/api/client";
import { postTypeLabel } from "@/lib/format";
import type { AdminPost, AdminPostListItem, Paged, PostStatus } from "@/types/admin";
import type { PostType } from "@/types/site";
import { ChoiceList } from "@/components/ui/choice";
import { Input, Select, Textarea } from "@/components/ui/field";
import { EntityLoader, FlashNotice } from "../entity";
import { MediaField } from "../media";
import { RichTextEditor } from "../rich-text";
import { effectiveStatus, postStatusLabel, postStatusTag } from "../tags";
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
} from "../ui";

const LIST = "/painel/publicacoes";
const postTypes = Object.keys(postTypeLabel) as PostType[];
const statuses = Object.keys(postStatusLabel) as PostStatus[];

export function PostsList({ initialStatus }: { initialStatus?: PostStatus }) {
  const [type, setType] = useState<PostType | "">("");
  const [status, setStatus] = useState<PostStatus | "">(initialStatus ?? "");
  const [search, setSearch] = useState("");
  const [query, setQuery] = useState("");
  const [page, setPage] = useState(1);

  const params = new URLSearchParams({ page: String(page), pageSize: "20" });
  if (type) params.set("type", type);
  if (status) params.set("status", status);
  if (query) params.set("search", query);

  const { data, error, reload } = useApi<Paged<AdminPostListItem>>(`/admin/posts?${params}`);
  const filtered = Boolean(type || status || query);

  return (
    <div>
      <AdminHeader
        kicker="Conteúdo do site"
        title="Publicações"
        description="Artigos, projetos e notícias da seção Conteúdo. Rascunhos e agendadas não aparecem no site."
        actions={<NewButton href={`${LIST}/nova`}>Nova publicação</NewButton>}
      />
      <FlashNotice />
      <form
        role="search"
        className="mb-8 grid gap-4 sm:grid-cols-[1fr_1fr_2fr_auto] sm:items-end"
        onSubmit={(event) => {
          event.preventDefault();
          setPage(1);
          setQuery(search.trim());
        }}
      >
        <Select
          label="Tipo"
          value={type}
          onChange={(event) => {
            setPage(1);
            setType(event.target.value as PostType | "");
          }}
        >
          <option value="">Todos</option>
          {postTypes.map((value) => (
            <option key={value} value={value}>
              {postTypeLabel[value]}
            </option>
          ))}
        </Select>
        <Select
          label="Situação"
          value={status}
          onChange={(event) => {
            setPage(1);
            setStatus(event.target.value as PostStatus | "");
          }}
        >
          <option value="">Todas</option>
          {statuses.map((value) => (
            <option key={value} value={value}>
              {postStatusLabel[value]}
            </option>
          ))}
        </Select>
        <Input label="Buscar no título" type="search" value={search} maxLength={100} onChange={(event) => setSearch(event.target.value)} />
        <button type="submit" className="min-h-[3.125rem] border border-ink px-5 font-sans text-[0.9375rem] font-medium hover:bg-ink hover:text-paper">
          Buscar
        </button>
      </form>

      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.items.length === 0 &&
        (filtered ? (
          <EmptyState title="Nada encontrado">Tente outro filtro ou outra palavra.</EmptyState>
        ) : (
          <EmptyState title="Nenhuma publicação ainda" action={<NewButton href={`${LIST}/nova`}>Escrever a primeira</NewButton>}>
            Conte um projeto, escreva um artigo ou divulgue uma notícia.
          </EmptyState>
        ))}
      {data && data.items.length > 0 && (
        <>
          <IndexList>
            {data.items.map((post) => (
              <IndexRow
                key={post.id}
                href={`${LIST}/${post.id}`}
                title={post.title}
                meta={
                  <>
                    {postTypeLabel[post.type]}
                    {post.publishedAt &&
                      ` · ${effectiveStatus(post.status, post.publishedAt) === "Scheduled" ? "sai em" : "publicada em"} ${formatDateTime(post.publishedAt)}`}
                  </>
                }
                tags={postStatusTag(effectiveStatus(post.status, post.publishedAt))}
              />
            ))}
          </IndexList>
          <Pagination page={page} totalPages={data.totalPages} onChange={setPage} />
        </>
      )}
    </div>
  );
}

type Form = {
  type: PostType;
  title: string;
  slug: string;
  summary: string;
  contentJson: string;
  contentHtml: string;
  coverId: string | null;
  status: PostStatus;
  scheduledFor: string;
  seoTitle: string;
  seoDescription: string;
};

function toForm(post?: AdminPost): Form {
  return {
    type: post?.type ?? "Article",
    title: post?.title ?? "",
    slug: post?.slug ?? "",
    summary: post?.summary ?? "",
    contentJson: post?.contentJson ?? "",
    contentHtml: post?.contentHtml ?? "",
    coverId: post?.coverId ?? null,
    status: post?.status ?? "Draft",
    scheduledFor: post?.status === "Scheduled" ? toLocalInput(post.publishedAt) : "",
    seoTitle: post?.seoTitle ?? "",
    seoDescription: post?.seoDescription ?? "",
  };
}

const statusOptions = [
  { id: "Draft", label: "Rascunho: só aparece aqui no painel" },
  { id: "Published", label: "Publicar agora" },
  { id: "Scheduled", label: "Agendar para uma data" },
];

function Counter({ value, max }: { value: string; max: number }) {
  const over = value.length > max;
  return (
    <span className={over ? "text-danger" : "text-ink-soft"}>
      {value.length}/{max}
    </span>
  );
}

function PostForm({ post }: { post?: AdminPost }) {
  const [form, setForm] = useState(() => toForm(post));
  const [slugTouched, setSlugTouched] = useState(Boolean(post));
  const editor = useEntityEditor<AdminPost>({
    apiPath: "/admin/posts",
    listHref: LIST,
    id: post?.id,
    created: "Publicação criada.",
    deleted: "Publicação excluída.",
  });
  const error = (field: string) => fieldError(editor.errors, field);
  const set = <K extends keyof Form>(key: K, value: Form[K]) => setForm((current) => ({ ...current, [key]: value }));

  function submit() {
    void editor.save({
      ...form,
      summary: optional(form.summary),
      seoTitle: optional(form.seoTitle),
      seoDescription: optional(form.seoDescription),
      contentJson: form.contentJson || JSON.stringify({ type: "doc", content: [] }),
      scheduledFor: form.status === "Scheduled" ? fromLocalInput(form.scheduledFor) : null,
    });
  }

  return (
    <>
      <AdminHeader
        kicker="Publicações"
        title={post ? post.title : "Nova publicação"}
        back={{ href: LIST, label: "Todas as publicações" }}
        description={
          post && (
            <span className="flex flex-wrap items-center gap-3">
              {postStatusTag(effectiveStatus(post.status, post.publishedAt))}
              {effectiveStatus(post.status, post.publishedAt) === "Published" && <span>No site: /conteudo/{post.slug}</span>}
            </span>
          )
        }
      />
      <AdminForm
        pending={editor.pending}
        message={editor.message}
        savedNote={editor.savedNote}
        submitLabel={post ? "Salvar alterações" : "Criar publicação"}
        onSubmit={submit}
        extraActions={
          post && (
            <ConfirmButton
              label="Excluir publicação"
              title="Excluir esta publicação?"
              description={<p>“{post.title}” sai do site e do painel. Para tirar só do site, mude para rascunho.</p>}
              confirmLabel="Excluir"
              onConfirm={editor.remove}
            />
          )
        }
      >
        <Section title="Texto">
          <Select label="Tipo" value={form.type} onChange={(event) => set("type", event.target.value as PostType)} error={error("type")}>
            {postTypes.map((value) => (
              <option key={value} value={value}>
                {postTypeLabel[value]}
              </option>
            ))}
          </Select>
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
            hint="Aparece na lista de publicações."
            value={form.summary}
            maxLength={500}
            onChange={(event) => set("summary", event.target.value)}
            error={error("summary")}
            className="min-h-24"
          />
          <RichTextEditor
            label="Conteúdo"
            initialHtml={post?.contentHtml}
            initialJson={post?.contentJson}
            onChange={(value) => setForm((current) => ({ ...current, contentHtml: value.html, contentJson: value.json }))}
            error={error("contentHtml") ?? error("contentJson")}
          />
          <MediaField label="Imagem de capa" value={form.coverId} onChange={(id) => set("coverId", id)} error={error("coverId")} />
        </Section>

        <Section title="Publicação" description="Agendadas entram no site sozinhas na data marcada (horário de Manaus).">
          <fieldset>
            <legend className="mb-2 font-sans text-sm font-medium text-ink">Situação</legend>
            <ChoiceList
              name="status"
              type="radio"
              options={statusOptions}
              value={[form.status]}
              onChange={([value]) => set("status", value as PostStatus)}
            />
          </fieldset>
          {form.status === "Scheduled" && (
            <Input
              label="Data e hora da publicação"
              type="datetime-local"
              value={form.scheduledFor}
              onChange={(event) => set("scheduledFor", event.target.value)}
              error={error("scheduledFor")}
            />
          )}
        </Section>

        <Section title="Busca e compartilhamento" description="Opcional. Sem preencher, o site usa o título e o resumo.">
          <FieldRow>
            <Input
              label="Título para o Google"
              optional
              value={form.seoTitle}
              maxLength={60}
              onChange={(event) => set("seoTitle", event.target.value)}
              error={error("seoTitle")}
            />
            <div className="hidden sm:block" />
          </FieldRow>
          <Textarea
            label="Descrição para o Google"
            optional
            value={form.seoDescription}
            maxLength={160}
            onChange={(event) => set("seoDescription", event.target.value)}
            error={error("seoDescription")}
            className="min-h-20"
          />
          <p className="-mt-4 font-sans text-xs">
            <Counter value={form.seoDescription} max={160} />
          </p>
        </Section>
      </AdminForm>
    </>
  );
}

export function PostEditor({ id }: { id?: string }) {
  return (
    <EntityLoader<AdminPost> path={id ? `/admin/posts/${id}` : null}>
      {(post) => <PostForm post={post} />}
    </EntityLoader>
  );
}
