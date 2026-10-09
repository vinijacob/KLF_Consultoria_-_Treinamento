"use client";

import { useState } from "react";
import { PauseIcon, PlayIcon } from "@/components/ui/icons";
import { cn } from "@/lib/cn";
import type { Client } from "@/types/site";

const MIN_FOR_MOTION = 4;
const MIN_PER_LOOP = 10;

/** Repete a lista até encher a faixa: com poucos nomes, o laço do letreiro deixaria um buraco no fim. */
function fill(names: string[]) {
  const repeats = Math.ceil(MIN_PER_LOOP / names.length);
  return Array.from({ length: repeats }, () => names).flat();
}

function Row({ names, reverse, paused, still }: { names: string[]; reverse?: boolean; paused: boolean; still: boolean }) {
  const items = still ? names : fill(names);
  const seconds = Math.max(35, items.length * 5);

  return (
    <div
      className={cn(
        "group overflow-hidden border-b border-rule py-5 first:border-t-2 first:border-t-ink last:border-b-2 last:border-b-ink",
        !still && "motion-reduce:overflow-x-auto",
      )}
    >
      <ul
        className={cn(
          still
            ? "flex flex-wrap items-center gap-y-3"
            : "flex w-max items-center motion-safe:animate-marquee group-hover:[animation-play-state:paused]",
          paused && "[animation-play-state:paused]",
        )}
        style={{ animationDuration: `${seconds}s`, animationDirection: reverse ? "reverse" : "normal" }}
      >
        {(still ? [0] : [0, 1]).map((copy) =>
          items.map((name, index) => (
            <li key={`${copy}-${index}`} className={cn("flex items-center", copy === 1 && "motion-reduce:hidden")}>
              <span className={cn("px-7 font-serif text-[clamp(1.5rem,3vw,2.25rem)] leading-none whitespace-nowrap", index % 2 === 1 && "italic")}>
                {name}
              </span>
              <span className="size-1.5 shrink-0 rotate-45 bg-ink-soft" />
            </li>
          )),
        )}
      </ul>
    </div>
  );
}

/**
 * Letreiro de empresas atendidas, como a faixa de notícias de um jornal: corre devagar, para com o mouse em cima,
 * tem botão de pausa (WCAG 2.2.2) e fica parado para quem pediu menos movimento no sistema.
 * O letreiro é decorativo para leitores de tela; a lista real vem em texto logo junto.
 */
export function ClientMarquee({ clients }: { clients: Client[] }) {
  const [paused, setPaused] = useState(false);
  const names = clients.map((client) => client.name);
  const rows = names.length >= 12 ? [names.filter((_, i) => i % 2 === 0), names.filter((_, i) => i % 2 === 1)] : [names];

  return (
    <div>
      <ul className="sr-only">
        {names.map((name, index) => (
          <li key={index}>{name}</li>
        ))}
      </ul>
      <div aria-hidden>
        {rows.map((row, index) => (
          <Row key={index} names={row} reverse={index === 1} paused={paused} still={names.length < MIN_FOR_MOTION} />
        ))}
      </div>
      {names.length >= MIN_FOR_MOTION && (
        <div className="mx-auto w-full max-w-6xl px-5 sm:px-8">
          <button
            type="button"
            onClick={() => setPaused((value) => !value)}
            className="mt-3 inline-flex min-h-11 items-center gap-2 font-sans text-sm text-ink-soft hover:text-ink motion-reduce:hidden"
          >
            {paused ? <PlayIcon /> : <PauseIcon />}
            {paused ? "Movimentar o letreiro" : "Pausar o letreiro"}
          </button>
        </div>
      )}
    </div>
  );
}
