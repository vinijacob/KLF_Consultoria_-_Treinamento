"use client";

import Image from "next/image";
import { useState } from "react";
import { Lightbox } from "./bubble";

type AlbumPhoto = { url: string; altText?: string | null; caption?: string | null };

/** Fotos do álbum em grade; clicar abre a foto ampliada na bolha, com anterior e próxima dentro do álbum. */
export function AlbumPhotos({ items }: { items: AlbumPhoto[] }) {
  const [zoomed, setZoomed] = useState<number | null>(null);

  return (
    <>
      <ul className="grid gap-x-8 gap-y-12 sm:grid-cols-2 lg:grid-cols-3">
        {items.map((item, index) => (
          <li key={`${item.url}-${index}`} className="reveal">
            <figure>
              <button
                type="button"
                aria-haspopup="dialog"
                aria-label={item.altText ? `Ampliar a foto: ${item.altText}` : `Ampliar a foto ${index + 1}`}
                onClick={() => setZoomed(index)}
                className="lift block w-full cursor-zoom-in overflow-hidden rounded-2xl"
              >
                <span className="relative block aspect-[4/3] overflow-hidden rounded-2xl bg-paper-deep">
                  <Image
                    src={item.url}
                    alt={item.altText ?? ""}
                    fill
                    unoptimized
                    sizes="(min-width: 1024px) 30vw, (min-width: 640px) 45vw, 100vw"
                    className="object-cover"
                  />
                </span>
              </button>
              {item.caption && <figcaption className="mt-3 font-sans text-sm text-ink-soft">{item.caption}</figcaption>}
            </figure>
          </li>
        ))}
      </ul>
      <Lightbox
        photos={items.map((item) => ({ src: item.url, alt: item.altText ?? "", caption: item.caption }))}
        index={zoomed}
        onIndexChange={setZoomed}
        onClose={() => setZoomed(null)}
      />
    </>
  );
}
