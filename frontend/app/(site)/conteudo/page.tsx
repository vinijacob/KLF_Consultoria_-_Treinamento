import type { Metadata } from "next";
import Link from "next/link";
import { Container, EmptyNote, PageHeader } from "@/components/site/blocks";
import { PostGrid } from "@/components/site/lists";
import { PostSearch } from "@/components/site/post-search";
import { CloseIcon } from "@/components/ui/icons";
import { copy } from "@/content/site-copy";
import { getPostCategories, getPosts, getPostTags } from "@/lib/api/public";
import { postTypePlural } from "@/lib/format";
import type { PostType } from "@/types/site";

export const metadata: Metadata = {
  title: "Conteúdo",
  description: copy.posts.intro,
};

const filters: { slug?: string; type?: PostType; label: string }[] = [
  { label: "Tudo" },
  { slug: "artigos", type: "Article", label: postTypePlural.Article },
  { slug: "projetos", type: "Project", label: postTypePlural.Project },
  { slug: "noticias", type: "News", label: postTypePlural.News },
];

type State = { tipo?: string; categoria?: string; tag?: string; busca?: string; pagina?: number };

function href(state: State) {
  const query = new URLSearchParams();
  if (state.tipo) query.set("tipo", state.tipo);
  if (state.categoria) query.set("categoria", state.categoria);
  if (state.tag) query.set("tag", state.tag);
  if (state.busca) query.set("busca", state.busca);
  if (state.pagina && state.pagina > 1) query.set("pagina", String(state.pagina));
  const text = query.toString();
  return text ? `/conteudo?${text}` : "/conteudo";
}

const text = (value: string | string[] | undefined) => (typeof value === "string" ? value.trim().slice(0, 100) : "");

const chip =
  "inline-flex min-h-11 items-center rounded-full border border-rule-strong px-4 transition-colors hover:border-ink aria-[current=page]:border-brand aria-[current=page]:bg-brand aria-[current=page]:text-brand-contrast";

export default async function PostsPage({ searchParams }: PageProps<"/conteudo">) {
  const params = await searchParams;
  const active = filters.find((filter) => filter.slug && filter.slug === params.tipo) ?? filters[0];
  const page = Math.max(1, Number(params.pagina) || 1);
  const search = text(params.busca);
  const [categories, tags] = await Promise.all([getPostCategories(), getPostTags()]);
  const category = categories.find((item) => item.slug === text(params.categoria));
  const tagSlug = text(params.tag);
  const tag = tagSlug ? (tags.find((item) => item.slug === tagSlug) ?? { name: tagSlug, slug: tagSlug }) : undefined;
  const state: State = { tipo: active.slug, categoria: category?.slug, tag: tag?.slug, busca: search || undefined };
  const posts = await getPosts({ type: active.type, search: search || undefined, category: category?.slug, tag: tag?.slug, page, pageSize: 10 });
  const status = search
    ? `${posts.totalItems} ${posts.totalItems === 1 ? "publicação encontrada" : "publicações encontradas"} para “${search}”.`
    : null;

  return (
    <>
      <PageHeader kicker={copy.posts.kicker} title={copy.posts.title} intro={copy.posts.intro}>
        <div className="mt-10">
          <PostSearch
            key={search}
            initial={search}
            keep={{ tipo: state.tipo, categoria: state.categoria, tag: state.tag }}
            clearHref={href({ ...state, busca: undefined })}
            status={status}
          />
        </div>
        <nav aria-label="Filtrar por tipo" className="mt-4">
          <ul className="flex flex-wrap gap-2 font-sans text-[0.9375rem]">
            {filters.map((filter) => (
              <li key={filter.label}>
                <Link href={href({ ...state, tipo: filter.slug })} aria-current={filter === active ? "page" : undefined} className={chip}>
                  {filter.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>
        {categories.length > 0 && (
          <nav aria-labelledby="filtro-categoria" className="mt-5">
            <p id="filtro-categoria" className="font-mono text-xs uppercase tracking-[0.12em] text-ink-soft">
              Categorias
            </p>
            <ul className="mt-2 flex flex-wrap gap-2 font-sans text-[0.9375rem]">
              <li>
                <Link href={href({ ...state, categoria: undefined })} aria-current={!category ? "page" : undefined} className={chip}>
                  Todas
                </Link>
              </li>
              {categories.map((item) => (
                <li key={item.slug}>
                  <Link href={href({ ...state, categoria: item.slug })} aria-current={item === category ? "page" : undefined} className={chip}>
                    {item.name}
                  </Link>
                </li>
              ))}
            </ul>
          </nav>
        )}
        {tag && (
          <p className="mt-5 flex flex-wrap items-center gap-3 font-sans text-[0.9375rem]">
            <span>
              Publicações com a tag <span className="font-semibold">#{tag.name}</span>
            </span>
            <Link
              href={href({ ...state, tag: undefined })}
              className="inline-flex min-h-11 items-center gap-1.5 text-brand underline underline-offset-4"
            >
              <CloseIcon aria-hidden /> Tirar a tag
            </Link>
          </p>
        )}
      </PageHeader>

      <Container className="mt-12">
        {posts.items.length > 0 ? (
          <PostGrid posts={posts.items} />
        ) : (
          <EmptyNote>
            {search
              ? `Nenhuma publicação encontrada para “${search}”. Tente outra palavra.`
              : category || tag
                ? "Nenhuma publicação com esse filtro por enquanto."
                : copy.posts.empty}
          </EmptyNote>
        )}

        {posts.totalPages > 1 && (
          <nav aria-label="Páginas" className="mt-10 flex items-center justify-between font-sans text-[0.9375rem]">
            {posts.hasPreviousPage ? (
              <Link href={href({ ...state, pagina: page - 1 })} className="text-brand underline underline-offset-4">
                Mais recentes
              </Link>
            ) : (
              <span />
            )}
            <span className="font-mono text-xs uppercase tracking-[0.12em] text-ink-soft">
              Página {posts.page} de {posts.totalPages}
            </span>
            {posts.hasNextPage ? (
              <Link href={href({ ...state, pagina: page + 1 })} className="text-brand underline underline-offset-4">
                Mais antigas
              </Link>
            ) : (
              <span />
            )}
          </nav>
        )}
      </Container>
    </>
  );
}
