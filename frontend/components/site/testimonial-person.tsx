"use client";

import Image from "next/image";
import { useState } from "react";
import { Kicker } from "@/components/ui/typography";
import { mediaUrl } from "@/lib/media";
import type { Testimonial } from "@/types/site";
import { Avatar, initials } from "./blocks";
import { Bubble } from "./bubble";

/**
 * Foto (ou iniciais) de quem deu o depoimento, que abre na bolha um cartão com a foto grande, o nome, o cargo, a empresa
 * e o depoimento inteiro. A área clicável do botão se estica (`after:inset-0`) até o primeiro ancestral com `relative`:
 * o cartão do depoimento inteiro abre o cartão da pessoa, e continua sendo um botão só (teclado e leitor de tela).
 */
export function TestimonialPerson({
  testimonial,
  size,
  onOpenChange,
}: {
  testimonial: Testimonial;
  size: "sm" | "md";
  onOpenChange?: (open: boolean) => void;
}) {
  const [open, setOpenState] = useState(false);
  const setOpen = (value: boolean) => {
    setOpenState(value);
    onOpenChange?.(value);
  };
  const photo = testimonial.photoId ? mediaUrl(testimonial.photoId) : null;
  const byline = [testimonial.authorRole, testimonial.companyName].filter(Boolean);

  return (
    <>
      <button
        type="button"
        aria-haspopup="dialog"
        aria-label={`Ver o depoimento de ${testimonial.authorName}`}
        onClick={() => setOpen(true)}
        className="shrink-0 cursor-pointer rounded-full ring-2 ring-transparent ring-offset-2 ring-offset-card transition-[box-shadow] duration-500 after:absolute after:inset-0 hover:ring-accent"
      >
        <Avatar name={testimonial.authorName} src={photo} size={size} />
      </button>
      <Bubble open={open} onClose={() => setOpen(false)} label={`Depoimento de ${testimonial.authorName}`} className="w-[min(44rem,calc(100vw-2rem))]">
        <figure className="grid gap-6 sm:grid-cols-[13rem_1fr] sm:gap-8">
          <div className="relative aspect-[4/5] w-40 overflow-hidden rounded-2xl bg-brand-soft sm:w-full">
            {photo ? (
              <Image src={photo} alt={testimonial.authorName} fill unoptimized sizes="(min-width: 640px) 13rem, 10rem" className="object-cover" />
            ) : (
              <span aria-hidden className="absolute inset-0 grid place-items-center font-serif text-5xl text-brand">
                {initials(testimonial.authorName)}
              </span>
            )}
          </div>
          <div className="pr-10 pb-2 sm:pt-2">
            <Kicker>Depoimento</Kicker>
            <figcaption className="mt-3">
              <span className="block font-serif text-[1.75rem] leading-tight">{testimonial.authorName}</span>
              {byline.map((line) => (
                <span key={line} className="mt-1 block font-sans text-[0.9375rem] text-ink-soft">
                  {line}
                </span>
              ))}
            </figcaption>
            <blockquote className="mt-6 border-t border-rule pt-6 font-serif text-[1.1875rem] leading-snug italic">
              <span aria-hidden className="mr-1 text-accent not-italic">
                “
              </span>
              {testimonial.quote}
              <span aria-hidden className="ml-0.5 text-accent not-italic">
                ”
              </span>
            </blockquote>
          </div>
        </figure>
      </Bubble>
    </>
  );
}
