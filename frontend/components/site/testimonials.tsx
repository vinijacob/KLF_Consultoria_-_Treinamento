"use client";

import { useEffect, useId, useState, useSyncExternalStore } from "react";
import { Button } from "@/components/ui/button";
import { ArrowLeftIcon, ArrowRightIcon, PauseIcon, PlayIcon } from "@/components/ui/icons";
import { cn } from "@/lib/cn";
import { twoDigits } from "@/lib/format";
import type { Testimonial } from "@/types/site";
import { TestimonialQuote } from "./lists";

const INTERVAL_MS = 9000;

function subscribeReducedMotion(onChange: () => void) {
  const query = window.matchMedia("(prefers-reduced-motion: reduce)");
  query.addEventListener("change", onChange);
  return () => query.removeEventListener("change", onChange);
}

function usePrefersReducedMotion() {
  return useSyncExternalStore(
    subscribeReducedMotion,
    () => window.matchMedia("(prefers-reduced-motion: reduce)").matches,
    () => true,
  );
}

/**
 * Depoimentos em rodízio: um por vez, em letra grande. Gira sozinho a cada 9 s, para com o mouse ou o foco em cima,
 * tem botão de pausa e setas, e não gira para quem pediu menos movimento no sistema (WCAG 2.2.2).
 * Todos os depoimentos ficam empilhados no mesmo lugar, então a altura não pula na troca.
 */
export function TestimonialCarousel({ testimonials }: { testimonials: Testimonial[] }) {
  const id = useId();
  const reducedMotion = usePrefersReducedMotion();
  const [index, setIndex] = useState(0);
  const [stopped, setStopped] = useState(false);
  const [hovering, setHovering] = useState(false);
  const [focused, setFocused] = useState(false);
  const count = testimonials.length;
  const playing = count > 1 && !stopped && !reducedMotion;
  const running = playing && !hovering && !focused;

  useEffect(() => {
    if (!running) return;
    const timer = window.setTimeout(() => setIndex((current) => (current + 1) % count), INTERVAL_MS);
    return () => window.clearTimeout(timer);
  }, [running, index, count]);

  if (count === 0) return null;
  if (count === 1) return <TestimonialQuote testimonial={testimonials[0]} large />;

  const go = (next: number) => setIndex((next + count) % count);

  return (
    <section
      aria-roledescription="carrossel"
      aria-label="Depoimentos"
      onMouseEnter={() => setHovering(true)}
      onMouseLeave={() => setHovering(false)}
      onFocus={() => setFocused(true)}
      onBlur={(event) => {
        if (!event.currentTarget.contains(event.relatedTarget)) setFocused(false);
      }}
    >
      <div className="grid" aria-live={playing ? "off" : "polite"}>
        {testimonials.map((testimonial, position) => {
          const current = position === index;
          return (
            <div
              key={testimonial.id}
              id={`${id}-${position}`}
              role="group"
              aria-roledescription="depoimento"
              aria-label={`${position + 1} de ${count}`}
              aria-hidden={!current}
              className={cn("[grid-area:1/1]", current ? "motion-safe:animate-fade" : "invisible")}
            >
              <TestimonialQuote testimonial={testimonial} large />
            </div>
          );
        })}
      </div>

      <div className="mt-10 flex flex-wrap items-center gap-x-6 gap-y-4">
        <p className="font-mono text-sm tracking-[0.12em] text-ink-soft">
          <span className="text-ink">{twoDigits(index + 1)}</span> / {twoDigits(count)}
        </p>
        <div className="relative h-0.5 min-w-24 flex-1 bg-rule" aria-hidden>
          {playing && (
            <span
              key={index}
              className={cn("absolute inset-0 origin-left bg-accent motion-safe:animate-progress", !running && "[animation-play-state:paused]")}
              style={{ animationDuration: `${INTERVAL_MS}ms` }}
            />
          )}
        </div>
        <div className="flex gap-2">
          <Button variant="outline" className="min-w-11 px-0" aria-label="Depoimento anterior" aria-controls={`${id}-${index}`} onClick={() => go(index - 1)}>
            <ArrowLeftIcon />
          </Button>
          <Button variant="outline" className="min-w-11 px-0" aria-label="Próximo depoimento" aria-controls={`${id}-${index}`} onClick={() => go(index + 1)}>
            <ArrowRightIcon />
          </Button>
          {!reducedMotion && (
            <Button
              variant="outline"
              className="min-w-11 px-0"
              aria-label={stopped ? "Voltar a girar os depoimentos" : "Pausar os depoimentos"}
              onClick={() => setStopped((value) => !value)}
            >
              {stopped ? <PlayIcon /> : <PauseIcon />}
            </Button>
          )}
        </div>
      </div>
    </section>
  );
}

const STEP = 6;

/** Todos os depoimentos em colunas, como cartas de leitores, mostrados de 6 em 6. */
export function TestimonialWall({ testimonials }: { testimonials: Testimonial[] }) {
  const [visible, setVisible] = useState(STEP);
  const shown = testimonials.slice(0, visible);
  const rest = testimonials.length - shown.length;

  return (
    <div>
      <ul className="gap-x-12 md:columns-2 lg:columns-3">
        {shown.map((testimonial) => (
          <li key={testimonial.id} className="mb-10 break-inside-avoid border-t border-rule pt-6 motion-safe:animate-rise">
            <TestimonialQuote testimonial={testimonial} />
          </li>
        ))}
      </ul>
      {rest > 0 && (
        <div className="flex justify-center">
          <Button variant="outline" size="lg" onClick={() => setVisible((current) => current + STEP)}>
            Mostrar mais {Math.min(STEP, rest)} de {rest}
          </Button>
        </div>
      )}
    </div>
  );
}
