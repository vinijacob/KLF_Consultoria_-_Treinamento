"use client";

import Image from "next/image";
import { useState } from "react";
import { cn } from "@/lib/cn";
import { mediaUrl } from "@/lib/media";
import { usePrefersReducedMotion } from "@/lib/motion";

const INTERVAL_MS = 6000;
const pad = (value: number) => String(value).padStart(2, "0");

/**
 * Retrato da Kilciene em carrossel: a foto nova entra como uma página virada (da direita para a esquerda) e se assenta
 * devagar; embaixo, o número da foto e um traço por foto, que enche até a troca. Pausa com o mouse ou o foco em cima e
 * para de vez quando alguém escolhe uma foto. Quem pediu menos movimento troca só clicando, sem animação.
 * Com uma foto só, é um retrato comum; sem nenhuma, a moldura com o aviso.
 */
export function PortraitCarousel({
  ids,
  name,
  priority,
  sizes = "(min-width: 1024px) 40vw, 100vw",
  placeholder = "Retrato em breve",
}: {
  ids: string[];
  name: string;
  priority?: boolean;
  sizes?: string;
  placeholder?: string;
}) {
  const reducedMotion = usePrefersReducedMotion();
  const [index, setIndex] = useState(0);
  const [previous, setPrevious] = useState<number | null>(null);
  const [chosen, setChosen] = useState(false);
  const [paused, setPaused] = useState(false);
  const count = ids.length;
  const auto = count > 1 && !chosen && !reducedMotion;

  function show(next: number) {
    if (next === index) return;
    setPrevious(index);
    setIndex(next);
  }

  return (
    <div
      onMouseEnter={() => setPaused(true)}
      onMouseLeave={() => setPaused(false)}
      onFocusCapture={() => setPaused(true)}
      onBlurCapture={() => setPaused(false)}
    >
      <div className="relative">
        <div aria-hidden className="absolute inset-0 translate-x-4 translate-y-4 rounded-2xl bg-brand-soft" />
        <div
          role={count > 1 ? "group" : undefined}
          aria-roledescription={count > 1 ? "carrossel" : undefined}
          aria-label={count > 1 ? `Fotos de ${name}` : undefined}
          className="relative aspect-[4/5] overflow-hidden rounded-2xl bg-paper-deep"
        >
          {count === 0 && (
            <div className="absolute inset-0 grid place-items-center p-6 text-center">
              <span className="font-mono text-xs uppercase tracking-[0.16em] text-ink-soft">{placeholder}</span>
            </div>
          )}
          {ids.map((id, position) => {
            const current = position === index;
            const under = position === previous;

            return (
              <div
                key={id}
                aria-hidden={!current}
                onAnimationEnd={(event) => {
                  if (current && event.target === event.currentTarget) setPrevious(null);
                }}
                className={cn("absolute inset-0 overflow-hidden", current ? "z-20" : under ? "z-10" : "invisible", current && previous !== null && "motion-safe:animate-unveil")}
              >
                <Image
                  src={mediaUrl(id)}
                  alt={count > 1 ? `${name}, foto ${position + 1} de ${count}` : name}
                  fill
                  unoptimized
                  priority={priority && position === 0}
                  sizes={sizes}
                  className={cn("object-cover", (current || under) && "motion-safe:animate-drift")}
                />
              </div>
            );
          })}
        </div>
      </div>

      {count > 1 && (
        <div className="mt-8 flex items-center gap-5">
          <span aria-hidden className="font-mono text-xs tracking-[0.12em] text-ink-soft tabular-nums">
            {pad(index + 1)} / {pad(count)}
          </span>
          <div role="group" aria-label="Escolher a foto" className="flex flex-1 gap-1.5">
            {ids.map((id, position) => {
              const current = position === index;

              return (
                <button
                  key={id}
                  type="button"
                  aria-label={`Mostrar a foto ${position + 1} de ${count}`}
                  aria-current={current ? "true" : undefined}
                  onClick={() => {
                    setChosen(true);
                    show(position);
                  }}
                  className="group flex min-h-11 flex-1 items-center"
                >
                  <span className="relative block h-0.5 w-full overflow-hidden bg-rule transition-colors group-hover:bg-rule-strong">
                    {current &&
                      (auto ? (
                        <span
                          key={index}
                          aria-hidden
                          onAnimationEnd={() => show((index + 1) % count)}
                          className={cn("absolute inset-0 origin-left bg-accent motion-safe:animate-progress", paused && "[animation-play-state:paused]")}
                          style={{ animationDuration: `${INTERVAL_MS}ms` }}
                        />
                      ) : (
                        <span aria-hidden className="absolute inset-0 bg-ink" />
                      ))}
                  </span>
                </button>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
