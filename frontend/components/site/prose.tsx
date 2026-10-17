"use client";

import { useEffect, useRef, useState } from "react";
import { cn } from "@/lib/cn";
import { Lightbox, type LightboxPhoto } from "./bubble";

const zoomable = (target: EventTarget) => (target instanceof HTMLImageElement && !target.closest("a") ? target : null);

/**
 * Conteúdo vindo do editor do painel. O HTML já chega sanitizado pela API (IHtmlContentSanitizer). As imagens do texto
 * (fora de links) abrem ampliadas na bolha, pelo clique ou pelo teclado.
 */
export function Prose({ html, className }: { html: string; className?: string }) {
  const ref = useRef<HTMLDivElement>(null);
  const [photo, setPhoto] = useState<LightboxPhoto | null>(null);

  useEffect(() => {
    ref.current?.querySelectorAll("img").forEach((image) => {
      if (image.closest("a")) return;
      image.tabIndex = 0;
      image.setAttribute("role", "button");
      image.setAttribute("aria-haspopup", "dialog");
      image.setAttribute("aria-label", image.alt ? `Ampliar a imagem: ${image.alt}` : "Ampliar a imagem");
    });
  }, [html]);

  const open = (image: HTMLImageElement | null) => image && setPhoto({ src: image.currentSrc || image.src, alt: image.alt });

  return (
    <>
      <div
        ref={ref}
        className={cn("prose-klf", className)}
        onClick={(event) => open(zoomable(event.target))}
        onKeyDown={(event) => {
          if (event.key !== "Enter" && event.key !== " ") return;
          const image = zoomable(event.target);
          if (!image) return;
          event.preventDefault();
          open(image);
        }}
        dangerouslySetInnerHTML={{ __html: html }}
      />
      <Lightbox photos={photo ? [photo] : []} index={photo ? 0 : null} onClose={() => setPhoto(null)} />
    </>
  );
}
