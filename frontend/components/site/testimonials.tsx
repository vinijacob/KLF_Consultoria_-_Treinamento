"use client";

import { useId, useState } from "react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/cn";
import { mediaUrl } from "@/lib/media";
import { usePrefersReducedMotion } from "@/lib/motion";
import type { Testimonial } from "@/types/site";
import { Avatar } from "./blocks";
import { TestimonialQuote } from "./lists";

const INTERVAL_MS = 8000;

/**
 * Depoimento em destaque com a lista de quem falou logo abaixo (fotos ou iniciais). A troca é automática a cada 8 s,
 * marcada por uma linha que enche sob o nome atual; ela pausa com o mouse em cima e para de vez quando a pessoa
 * escolhe um depoimento. Quem pediu menos movimento no sistema troca só clicando.
 * O tempo vem do fim da própria animação da linha, então o texto troca exatamente quando a linha termina.
 */
export function TestimonialSpotlight({ testimonials }: { testimonials: Testimonial[] }) {
  const id = useId();
  const reducedMotion = usePrefersReducedMotion();
  const [index, setIndex] = useState(0);
  const [chosen, setChosen] = useState(false);
  const [hovering, setHovering] = useState(false);
  const count = testimonials.length;
  const auto = count > 1 && !chosen && !reducedMotion;

  if (count === 0) return null;

  return (
    <div onMouseEnter={() => setHovering(true)} onMouseLeave={() => setHovering(false)}>
      <div className="grid items-center rounded-3xl border border-rule bg-card px-7 py-10 sm:px-12 sm:py-14">
        {testimonials.map((testimonial, position) => (
          <div
            key={testimonial.id}
            id={`${id}-panel-${position}`}
            role={count > 1 ? "tabpanel" : undefined}
            aria-labelledby={count > 1 ? `${id}-tab-${position}` : undefined}
            aria-hidden={position !== index}
            className={cn("[grid-area:1/1]", position === index ? "motion-safe:animate-fade" : "invisible")}
          >
            <TestimonialQuote testimonial={testimonial} large />
          </div>
        ))}
      </div>

      {count > 1 && (
        <div role="tablist" aria-label="Escolha um depoimento" className="-mx-1 mt-5 flex snap-x gap-3 overflow-x-auto px-1 pb-2">
          {testimonials.map((testimonial, position) => {
            const current = position === index;

            return (
              <button
                key={testimonial.id}
                id={`${id}-tab-${position}`}
                type="button"
                role="tab"
                aria-selected={current}
                aria-controls={`${id}-panel-${position}`}
                onClick={() => {
                  setChosen(true);
                  setIndex(position);
                }}
                className={cn(
                  "relative flex min-h-16 shrink-0 snap-start items-center gap-3 overflow-hidden rounded-2xl border px-3 py-2.5 text-left transition-colors",
                  current ? "border-brand bg-brand-soft" : "border-rule bg-card hover:border-rule-strong",
                )}
              >
                <Avatar name={testimonial.authorName} src={testimonial.photoId ? mediaUrl(testimonial.photoId) : null} size="sm" />
                <span className="max-w-44 pr-1">
                  <span className="block truncate font-sans text-sm font-semibold text-ink">{testimonial.authorName}</span>
                  {testimonial.companyName && <span className="block truncate font-sans text-xs text-ink-soft">{testimonial.companyName}</span>}
                </span>
                {current && auto && (
                  <span
                    key={index}
                    aria-hidden
                    onAnimationEnd={() => setIndex((position + 1) % count)}
                    className={cn("absolute inset-x-0 bottom-0 h-0.5 origin-left bg-accent motion-safe:animate-progress", hovering && "[animation-play-state:paused]")}
                    style={{ animationDuration: `${INTERVAL_MS}ms` }}
                  />
                )}
              </button>
            );
          })}
        </div>
      )}
    </div>
  );
}

const STEP = 6;

/** Todos os depoimentos em caixas, em colunas, mostrados de 6 em 6. */
export function TestimonialWall({ testimonials }: { testimonials: Testimonial[] }) {
  const [visible, setVisible] = useState(STEP);
  const shown = testimonials.slice(0, visible);
  const rest = testimonials.length - shown.length;

  return (
    <div>
      <ul className="gap-5 md:columns-2 lg:columns-3">
        {shown.map((testimonial) => (
          <li key={testimonial.id} className="mb-5 break-inside-avoid rounded-2xl border border-rule bg-card p-7 motion-safe:animate-rise">
            <TestimonialQuote testimonial={testimonial} />
          </li>
        ))}
      </ul>
      {rest > 0 && (
        <div className="mt-5 flex justify-center">
          <Button variant="outline" size="lg" onClick={() => setVisible((current) => current + STEP)}>
            Mostrar mais depoimentos ({rest})
          </Button>
        </div>
      )}
    </div>
  );
}
