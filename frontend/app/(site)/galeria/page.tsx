import type { Metadata } from "next";
import Link from "next/link";
import { Container, EmptyNote, FramedImage, PageHeader } from "@/components/site/blocks";
import { Kicker } from "@/components/ui/typography";
import { copy } from "@/content/site-copy";
import { getAlbums } from "@/lib/api/public";

export const metadata: Metadata = {
  title: "Galeria",
  description: copy.gallery.intro,
};

export default async function GalleryPage() {
  const albums = await getAlbums();

  return (
    <>
      <PageHeader kicker={copy.gallery.kicker} title={copy.gallery.title} intro={copy.gallery.intro} />
      <Container className="mt-12">
        {albums.length === 0 ? (
          <EmptyNote>{copy.gallery.empty}</EmptyNote>
        ) : (
          <ul className="grid gap-x-10 gap-y-14 sm:grid-cols-2 lg:grid-cols-3">
            {albums.map((album) => (
              <li key={album.id}>
                <Link href={`/galeria/${album.slug}`} className="group block">
                  <FramedImage src={album.coverUrl} alt="" ratio="aspect-[4/3]" placeholder="Sem capa" sizes="(min-width: 1024px) 30vw, (min-width: 640px) 45vw, 100vw" />
                  <Kicker className="mt-4">{album.itemCount === 1 ? "1 foto" : `${album.itemCount} fotos`}</Kicker>
                  <p className="mt-1 font-serif text-2xl leading-snug group-hover:underline group-hover:decoration-1 group-hover:underline-offset-[6px]">
                    {album.title}
                  </p>
                  {album.description && <p className="mt-2 text-base leading-relaxed text-ink-soft">{album.description}</p>}
                </Link>
              </li>
            ))}
          </ul>
        )}
      </Container>
    </>
  );
}
