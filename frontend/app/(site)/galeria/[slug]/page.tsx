import type { Metadata } from "next";
import Image from "next/image";
import { notFound } from "next/navigation";
import { ArrowLink, Container, EmptyNote, PageHeader } from "@/components/site/blocks";
import { Rule } from "@/components/ui/typography";
import { getAlbum } from "@/lib/api/public";

export async function generateMetadata({ params }: PageProps<"/galeria/[slug]">): Promise<Metadata> {
  const { slug } = await params;
  const album = await getAlbum(slug);

  if (!album) return { title: "Álbum não encontrado" };

  return {
    title: album.title,
    description: album.description ?? undefined,
    openGraph: album.coverUrl ? { images: [album.coverUrl] } : undefined,
  };
}

export default async function AlbumPage({ params }: PageProps<"/galeria/[slug]">) {
  const { slug } = await params;
  const album = await getAlbum(slug);

  if (!album) notFound();

  return (
    <>
      <PageHeader kicker="Galeria" title={album.title} intro={album.description ?? undefined} />
      <Container className="mt-12">
        {album.items.length === 0 ? (
          <EmptyNote>As fotos deste álbum serão publicadas em breve.</EmptyNote>
        ) : (
          <ul className="grid gap-x-8 gap-y-12 sm:grid-cols-2 lg:grid-cols-3">
            {album.items.map((item, index) => (
              <li key={`${item.url}-${index}`}>
                <figure>
                  <a href={item.url} target="_blank" rel="noopener" className="block border border-ink p-2">
                    <span className="relative block aspect-[4/3] overflow-hidden bg-paper-deep">
                      <Image
                        src={item.url}
                        alt={item.altText ?? ""}
                        fill
                        unoptimized
                        sizes="(min-width: 1024px) 30vw, (min-width: 640px) 45vw, 100vw"
                        className="object-cover"
                      />
                    </span>
                    <span className="sr-only">Abrir a foto em tamanho original (nova aba)</span>
                  </a>
                  {item.caption && <figcaption className="mt-3 font-sans text-sm text-ink-soft">{item.caption}</figcaption>}
                </figure>
              </li>
            ))}
          </ul>
        )}
        <Rule className="mt-16" />
        <ArrowLink href="/galeria" className="mt-6">
          Voltar para a galeria
        </ArrowLink>
      </Container>
    </>
  );
}
