import type { Metadata } from "next";
import Link from "next/link";
import { Container, EmptyNote, PageHeader } from "@/components/site/blocks";
import { PostIndex } from "@/components/site/lists";
import { PostSearch } from "@/components/site/post-search";
import { copy } from "@/content/site-copy";
import { getPosts } from "@/lib/api/public";
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

function href(slug: string | undefined, search: string, page = 1) {
  const query = new URLSearchParams();
  if (slug) query.set("tipo", slug);
  if (search) query.set("busca", search);
  if (page > 1) query.set("pagina", String(page));
  const text = query.toString();
  return text ? `/conteudo?${text}` : "/conteudo";
}

export default async function PostsPage({ searchParams }: PageProps<"/conteudo">) {
  const params = await searchParams;
  const active = filters.find((filter) => filter.slug && filter.slug === params.tipo) ?? filters[0];
  const page = Math.max(1, Number(params.pagina) || 1);
  const search = typeof params.busca === "string" ? params.busca.trim().slice(0, 100) : "";
  const posts = await getPosts({ type: active.type, search: search || undefined, page, pageSize: 10 });
  const status = search
    ? `${posts.totalItems} ${posts.totalItems === 1 ? "publicação encontrada" : "publicações encontradas"} para “${search}”.`
    : null;

  return (
    <>
      <PageHeader kicker={copy.posts.kicker} title={copy.posts.title} intro={copy.posts.intro}>
        <div className="mt-10">
          <PostSearch key={search} initial={search} type={active.slug} clearHref={href(active.slug, "")} status={status} />
        </div>
        <nav aria-label="Filtrar por tipo" className="mt-4">
          <ul className="flex flex-wrap gap-x-6 gap-y-2 font-sans text-[0.9375rem]">
            {filters.map((filter) => (
              <li key={filter.label}>
                <Link
                  href={href(filter.slug, search)}
                  aria-current={filter === active ? "page" : undefined}
                  className="py-1 underline-offset-[6px] hover:underline aria-[current=page]:font-semibold aria-[current=page]:underline aria-[current=page]:decoration-2"
                >
                  {filter.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>
      </PageHeader>

      <Container className="mt-12">
        {posts.items.length > 0 ? <PostIndex posts={posts.items} /> : <EmptyNote>{search ? `Nenhuma publicação encontrada para “${search}”. Tente outra palavra.` : copy.posts.empty}</EmptyNote>}

        {posts.totalPages > 1 && (
          <nav aria-label="Páginas" className="mt-10 flex items-center justify-between font-sans text-[0.9375rem]">
            {posts.hasPreviousPage ? (
              <Link href={href(active.slug, search, page - 1)} className="text-brand underline underline-offset-4">
                Mais recentes
              </Link>
            ) : (
              <span />
            )}
            <span className="font-mono text-xs uppercase tracking-[0.12em] text-ink-soft">
              Página {posts.page} de {posts.totalPages}
            </span>
            {posts.hasNextPage ? (
              <Link href={href(active.slug, search, page + 1)} className="text-brand underline underline-offset-4">
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
