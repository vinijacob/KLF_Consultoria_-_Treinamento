import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { AlbumPhotos } from "@/components/site/album-photos";
import { ArrowLink, Container, EmptyNote, PageHeader } from "@/components/site/blocks";
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
          <AlbumPhotos items={album.items} />
        )}
        <ArrowLink href="/galeria" className="mt-16">
          Voltar para a galeria
        </ArrowLink>
      </Container>
    </>
  );
}
