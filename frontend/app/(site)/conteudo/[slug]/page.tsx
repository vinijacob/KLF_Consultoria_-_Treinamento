import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { ArrowLink, Container, FramedImage, Prose } from "@/components/site/blocks";
import { ClosingCta } from "@/components/site/contact";
import { Kicker, Rule } from "@/components/ui/typography";
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
            <Kicker>
              {postTypeLabel[post.type]}
              {post.publishedAt && (
                <>
                  {" · "}
                  <time dateTime={post.publishedAt}>{formatDate(post.publishedAt)}</time>
                </>
              )}
            </Kicker>
            <h1 className="mt-4 text-[clamp(2.25rem,6vw,4rem)] leading-[1.05]">{post.title}</h1>
            {post.summary && <p className="mt-6 text-[1.375rem] leading-relaxed text-ink-soft">{post.summary}</p>}
            <p className="mt-6 font-sans text-[0.9375rem]">
              Por <span className="font-semibold">{copy.person.name}</span>
            </p>
            <Rule variant="double" className="mt-8" />
          </div>
        </Container>

        {post.coverId && (
          <Container className="mt-10">
            <div className="mx-auto max-w-[42rem]">
              <FramedImage src={mediaUrl(post.coverId)} alt="" ratio="aspect-[16/9]" sizes="(min-width: 1024px) 896px, 100vw" />
            </div>
          </Container>
        )}

        <Container className="mt-12">
          <Prose html={post.contentHtml} className="mx-auto" />
          <div className="mx-auto mt-16 max-w-[42rem]">
            <Rule />
            <ArrowLink href="/conteudo" className="mt-6">
              Ver todas as publicações
            </ArrowLink>
          </div>
        </Container>
      </article>
      <ClosingCta contact={settings.contact} />
    </>
  );
}
