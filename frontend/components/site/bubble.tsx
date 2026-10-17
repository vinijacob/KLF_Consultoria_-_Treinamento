"use client";

import Image from "next/image";
import { useEffect, useRef, useState, type ReactNode } from "react";
import {
  ArrowLeftIcon,
  ArrowRightIcon,
  CloseIcon,
} from "@/components/ui/icons";
import { cn } from "@/lib/cn";
import { useBackdropClose } from "@/lib/dialog";

/**
 * Janela pequena no centro da tela, sem sair da página: `<dialog>` nativo (foco preso, Esc fecha), fecha também no X e
 * clicando fora. Atrás, o próprio site borrado e esmaecido (exceção pedida às regras de estilo, só aqui); a caixa entra
 * subindo de leve.
 */
export function Bubble({
  open,
  onClose,
  label,
  className,
  onKeyDown,
  children,
}: {
  open: boolean;
  onClose: () => void;
  label: string;
  className?: string;
  onKeyDown?: (event: React.KeyboardEvent<HTMLDialogElement>) => void;
  children: ReactNode;
}) {
  const ref = useRef<HTMLDialogElement>(null);
  const backdropClose = useBackdropClose(onClose);

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    if (!open && dialog.open) dialog.close();
  }, [open]);

  return (
    <dialog
      ref={ref}
      aria-label={label}
      onClose={onClose}
      onKeyDown={onKeyDown}
      {...backdropClose}
      className="m-auto max-h-[calc(100dvh-2rem)] max-w-[calc(100vw-2rem)] overflow-visible bg-transparent p-0 text-ink backdrop:bg-paper/65 backdrop:backdrop-blur-md motion-safe:backdrop:animate-veil"
    >
      {open && (
        <div
          className={cn(
            "relative max-h-[calc(100dvh-2rem)] overflow-y-auto rounded-3xl border border-rule-strong bg-card p-3 motion-safe:animate-bubble sm:p-4",
            className,
          )}
        >
          <button
            type="button"
            onClick={onClose}
            aria-label="Fechar"
            className="absolute top-3 right-3 z-10 grid size-11 place-items-center rounded-full border border-rule-strong bg-card text-xl text-ink transition-colors hover:bg-paper-deep sm:top-4 sm:right-4"
          >
            <CloseIcon />
          </button>
          {children}
        </div>
      )}
    </dialog>
  );
}

export type LightboxPhoto = {
  src: string;
  alt: string;
  caption?: string | null;
};

const pad = (value: number) => String(value).padStart(2, "0");

/** Foto ampliada na bolha; com mais de uma, anterior e próxima (também pelas setas do teclado). */
export function Lightbox({
  photos,
  index,
  onIndexChange,
  onClose,
}: {
  photos: LightboxPhoto[];
  index: number | null;
  onIndexChange?: (index: number) => void;
  onClose: () => void;
}) {
  const photo = index === null ? undefined : photos[index];
  const count = photos.length;
  const go = (step: number) =>
    index !== null && onIndexChange?.((index + step + count) % count);
  const browsable = count > 1 && onIndexChange !== undefined;

  return (
    <Bubble
      open={photo !== undefined}
      onClose={onClose}
      label={photo?.alt ? `Foto ampliada: ${photo.alt}` : "Foto ampliada"}
      onKeyDown={(event) => {
        if (!browsable) return;
        if (event.key === "ArrowLeft") go(-1);
        if (event.key === "ArrowRight") go(1);
      }}
    >
      {photo && index !== null && (
        <figure>
          <Image
            key={photo.src}
            src={photo.src}
            alt={photo.alt}
            width={0}
            height={0}
            sizes="90vw"
            unoptimized
            className="h-auto max-h-[min(76dvh,56rem)] w-auto max-w-[min(calc(100vw-4rem),72rem)] rounded-2xl bg-paper-deep motion-safe:animate-fade"
          />
          {(photo.caption || browsable) && (
            <figcaption className="flex flex-wrap items-center justify-between gap-x-6 gap-y-3 px-1 pt-3 sm:px-2">
              <span className="font-sans text-sm text-ink-soft">
                {photo.caption}
              </span>
              {browsable && (
                <span className="flex items-center gap-2">
                  <span className="mr-2 font-mono text-xs tracking-[0.12em] text-ink-soft tabular-nums">
                    {pad(index + 1)} / {pad(count)}
                  </span>
                  <button
                    type="button"
                    onClick={() => go(-1)}
                    aria-label="Foto anterior"
                    className="grid size-11 place-items-center rounded-full border border-rule-strong text-lg transition-colors hover:bg-paper-deep"
                  >
                    <ArrowLeftIcon />
                  </button>
                  <button
                    type="button"
                    onClick={() => go(1)}
                    aria-label="Próxima foto"
                    className="grid size-11 place-items-center rounded-full border border-rule-strong text-lg transition-colors hover:bg-paper-deep"
                  >
                    <ArrowRightIcon />
                  </button>
                </span>
              )}
            </figcaption>
          )}
        </figure>
      )}
    </Bubble>
  );
}

/**
 * Deixa uma foto clicável: abre a mesma foto ampliada na bolha, sem sair da página nem abrir outra aba. O tamanho e o
 * formato do botão vêm de `className`.
 */
export function ZoomablePhoto({
  photo,
  className,
  children,
}: {
  photo: LightboxPhoto;
  className?: string;
  children: ReactNode;
}) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <button
        type="button"
        onClick={() => setOpen(true)}
        aria-haspopup="dialog"
        aria-label={
          photo.alt ? `Ampliar a foto: ${photo.alt}` : "Ampliar a foto"
        }
        className={cn("cursor-zoom-in text-left", className)}
      >
        {children}
      </button>
      <Lightbox
        photos={[photo]}
        index={open ? 0 : null}
        onClose={() => setOpen(false)}
      />
    </>
  );
}
