"use client";

import Link from "next/link";
import { useState } from "react";
import { ArrowRightIcon } from "@/components/ui/icons";

const replies = [
  { text: "Já vou! Enquanto isso, dê uma olhada nos treinamentos.", href: "/servicos", label: "Ver os treinamentos" },
  { text: "Um minutinho, estou no estoque. Cliente bem atendido não espera à toa.", href: "/conteudo", label: "Ler enquanto espera" },
  { text: "Três toques? Isso é cliente decidido. Vamos conversar.", href: "/contato", label: "Falar com a gente" },
  { text: "Achei! Quer dizer… não achei. Mas a vitrine está cheia.", href: "/", label: "Ir para a vitrine" },
];

/**
 * Cena da página 404, de loja: a placa "Volto já" pendurada num prego (balança devagar; no hover ou foco vira para
 * "Aberto" e leva à página inicial) e o sininho do balcão (cada toque balança o sino, sobe um "plim!" e o balão traz
 * uma resposta diferente, sempre com uma saída). Com "reduzir movimento", nada balança sozinho.
 */
export function StoreBreak() {
  const [rings, setRings] = useState(0);
  const reply = rings > 0 ? replies[(rings - 1) % replies.length] : null;

  return (
    <div className="rounded-3xl border border-rule bg-paper-deep px-6 pt-8 pb-6 sm:px-10">
      <div className="flex justify-center">
        <div className="origin-[50%_6px] motion-safe:animate-swing">
          <svg aria-hidden viewBox="0 0 240 64" className="mx-auto block h-16 w-60 text-ink">
            <path d="M120 6 L28 62 M120 6 L212 62" fill="none" stroke="currentColor" strokeWidth="1.5" />
            <circle cx="120" cy="6" r="5" fill="currentColor" />
          </svg>
          <Link href="/" aria-label="Voltar para a página inicial" className="group block [perspective:900px]">
            <span className="relative block h-44 w-60 transition-transform duration-700 ease-[cubic-bezier(0.22,1,0.36,1)] [transform-style:preserve-3d] group-hover:[transform:rotateY(180deg)] group-focus-visible:[transform:rotateY(180deg)] sm:w-72">
              <SignFace kicker="Erro 404" title="Volto já" note="Fui buscar esta página no estoque." />
              <SignFace back kicker="Pode entrar" title="Aberto" note="Página inicial" arrow />
            </span>
          </Link>
        </div>
      </div>

      <div className="relative mt-14 flex flex-col items-center">
        <div aria-live="polite" className="flex h-44 w-full max-w-sm flex-col justify-end sm:h-36">
          {!reply && <p className="text-center font-mono text-xs tracking-[0.16em] text-ink-soft uppercase">Toque o sininho</p>}
          {reply && (
            <div key={rings} className="relative rounded-2xl border border-rule-strong bg-card px-5 py-4 motion-safe:animate-bubble">
              <p className="font-sans text-[0.9375rem] leading-relaxed">{reply.text}</p>
              <Link href={reply.href} className="mt-2 inline-flex min-h-11 items-center gap-2 font-sans text-sm font-medium text-brand underline underline-offset-4">
                {reply.label}
                <ArrowRightIcon />
              </Link>
              <span aria-hidden className="absolute -bottom-[7px] left-1/2 size-3 -translate-x-1/2 rotate-45 border-r border-b border-rule-strong bg-card" />
            </div>
          )}
        </div>

        <button
          type="button"
          onClick={() => setRings((count) => count + 1)}
          aria-label="Tocar o sininho do balcão"
          className="relative mt-4 grid min-h-16 min-w-20 place-items-center rounded-2xl text-ink"
        >
          {rings > 0 && (
            <span key={rings} aria-hidden className="pointer-events-none absolute -top-5 font-mono text-xs tracking-[0.14em] text-accent uppercase motion-safe:animate-plim">
              plim!
            </span>
          )}
          <svg aria-hidden viewBox="0 0 64 48" className="h-12 w-16">
            <g key={rings} className={rings > 0 ? "origin-[32px_40px] motion-safe:animate-ring" : undefined}>
              <rect x="30" y="9" width="4" height="8" fill="currentColor" />
              <circle cx="32" cy="8" r="3.5" fill="currentColor" />
              <path d="M10 39 A22 22 0 0 1 54 39 Z" className="fill-accent" />
              <path d="M18 31 A15 15 0 0 1 28 22" fill="none" className="stroke-card" strokeWidth="2" strokeLinecap="round" />
            </g>
            <rect x="5" y="39" width="54" height="6" rx="2" fill="currentColor" />
          </svg>
        </button>
        <span aria-hidden className="mt-1 block h-3 w-full rounded-full bg-rule-strong" />
      </div>
    </div>
  );
}

function SignFace({ kicker, title, note, back, arrow }: { kicker: string; title: string; note: string; back?: boolean; arrow?: boolean }) {
  return (
    <span
      className={`absolute inset-0 flex flex-col items-center justify-center rounded-2xl bg-brand px-5 text-center text-brand-contrast [backface-visibility:hidden] ${back ? "[transform:rotateY(180deg)]" : ""}`}
    >
      <span aria-hidden className="absolute inset-2 rounded-xl border border-brand-contrast" />
      <span className="font-mono text-[0.6875rem] tracking-[0.2em] uppercase">{kicker}</span>
      <span className="mt-1 font-serif text-5xl leading-none italic">{title}</span>
      <span className="mt-3 inline-flex items-center gap-1.5 font-sans text-sm">
        {note}
        {arrow && <ArrowRightIcon />}
      </span>
    </span>
  );
}
