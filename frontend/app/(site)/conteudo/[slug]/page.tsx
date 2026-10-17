import type { Metadata } from "next";
import Link from "next/link";
import { notFound } from "next/navigation";
import { ArrowLink, Badge, Container, FramedImage, Prose } from "@/components/site/blocks";
import { ClosingCta } from "@/components/site/contact";
import { copy } from "@/content/site-copy";
import { getPost, getSettings, mediaUrl } from "@/lib/api/public";
import { formatDate, postTypeLabel } from "@/lib/format";

export async function generateMetadata({ params }: PageProps<"/conteudo/[slug]">): Promise<Metadata> {
  const { slug } = await params;
  const post = await getPost(slug);

  if (!post) return { title: "Publicação não encontrada" };

  return {
    title: post.seoTitle ?? post.title,
    description: post.seoDescription ?? post.summary ?? undefined,
    openGraph: {
      type: "article",
      publishedTime: post.publishedAt ?? undefined,
      images: post.coverId ? [mediaUrl(post.coverId)] : undefined,
    },
  };
}

export default async function PostPage({ params }: PageProps<"/conteudo/[slug]">) {
  const { slug } = await params;
  const [post, settings] = await Promise.all([getPost(slug), getSettings()]);

  if (!post) notFound();

  return (
    <>
      <article>
        <Container className="pt-12 sm:pt-16">
          <div className="mx-auto max-w-[42rem]">
            <p className="flex flex-wrap items-center gap-3">
              <Badge tone="brand">{postTypeLabel[post.type]}</Badge>
              {post.category && (
                <Link
                  href={`/conteudo?categoria=${encodeURIComponent(post.category.slug)}`}
                  className="inline-flex min-h-11 items-center font-mono text-[0.6875rem] font-medium uppercase tracking-[0.12em] text-ink-soft underline underline-offset-4 hover:text-ink"
                >
                  {post.category.name}
                </Link>
              )}
              {post.publishedAt && (
                <time dateTime={post.publishedAt} className="font-sans text-sm text-ink-soft">
                  {formatDate(post.publishedAt)}
                </time>
              )}
            </p>
            <h1 className="mt-4 text-[clamp(2.25rem,6vw,4rem)] leading-[1.05]">{post.title}</h1>
            {post.summary && <p className="mt-6 text-[1.375rem] leading-relaxed text-ink-soft">{post.summary}</p>}
            <p className="mt-6 font-sans text-[0.9375rem]">
              Por <span className="font-semibold">{copy.person.name}</span>
            </p>
          </div>
        </Container>

        {post.coverId && (
          <Container className="mt-10">
            <div className="mx-auto max-w-[42rem]">
              <FramedImage src={mediaUrl(post.coverId)} alt="" ratio="aspect-video" sizes="(min-width: 1024px) 896px, 100vw" />
            </div>
          </Container>
        )}

        <Container className="mt-12">
          <Prose html={post.contentHtml} className="mx-auto" />
          {post.tags.length > 0 && (
            <nav aria-labelledby="tags-da-publicacao" className="mx-auto mt-12 max-w-[42rem]">
              <p id="tags-da-publicacao" className="font-mono text-xs uppercase tracking-[0.12em] text-ink-soft">
                Tags
              </p>
              <ul className="mt-3 flex flex-wrap gap-2 font-sans text-[0.9375rem]">
                {post.tags.map((tag) => (
                  <li key={tag.slug}>
                    <Link
                      href={`/conteudo?tag=${encodeURIComponent(tag.slug)}`}
                      className="inline-flex min-h-11 items-center rounded-full border border-rule-strong px-4 transition-colors hover:border-ink"
                    >
                      #{tag.name}
                    </Link>
                  </li>
                ))}
              </ul>
            </nav>
          )}
          <div className="mx-auto mt-16 max-w-[42rem]">
            <ArrowLink href="/conteudo">
              Ver todas as publicações
            </ArrowLink>
          </div>
        </Container>
      </article>
      <ClosingCta contact={settings.contact} />
    </>
  );
}
