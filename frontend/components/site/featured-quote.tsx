import { Fragment, type CSSProperties } from "react";
import type { FeaturedQuote } from "@/types/site";

const FIRST_WORD_MS = 600;
const WORD_STEP_MS = 45;
const MAX_STEPS = 40;

/**
 * Citação da página inicial como uma placa impressa: moldura dupla com um losango no meio do fio, aspas grandes em
 * tijolo e a assinatura com fio. A entrada (moldura que se desenha, aspas carimbadas, palavras como tinta fresca) e o
 * hover (os fios da moldura se afastam) ficam no `globals.css` (`.quote-plate`) e só rodam na página inicial.
 */
export function FeaturedQuoteCard({ quote }: { quote: FeaturedQuote }) {
  const words = quote.text.trim().split(/\s+/);
  const signDelay = FIRST_WORD_MS + Math.min(words.length, MAX_STEPS) * WORD_STEP_MS + 400;

  return (
    <figure
      className="reveal quote-plate group relative overflow-hidden rounded-3xl bg-brand px-10 pt-24 pb-16 text-brand-contrast sm:px-20 sm:pt-28 sm:pb-20"
      style={{ "--sign-delay": `${signDelay}ms` } as CSSProperties}
    >
      <span
        aria-hidden
        className="quote-frame pointer-events-none absolute inset-3 rounded-2xl border border-brand-contrast transition-[inset] duration-700 ease-[cubic-bezier(0.22,1,0.36,1)] group-hover:inset-2"
      />
      <span
        aria-hidden
        className="quote-frame quote-frame-inner pointer-events-none absolute inset-5 rounded-xl border border-brand-contrast transition-[inset] duration-700 ease-[cubic-bezier(0.22,1,0.36,1)] group-hover:inset-6"
      >
        <span className="absolute -top-[5px] left-1/2 size-2.5 -translate-x-1/2 rotate-45 bg-brand-contrast ring-4 ring-brand" />
        <span className="absolute -bottom-[5px] left-1/2 size-2.5 -translate-x-1/2 rotate-45 bg-brand-contrast ring-4 ring-brand" />
      </span>

      <span aria-hidden className="quote-mark absolute top-9 left-9 font-serif text-[8rem] leading-none text-accent select-none sm:top-10 sm:left-16 sm:text-[10rem]">
        “
      </span>

      <blockquote className="relative max-w-4xl">
        <p className="font-serif text-[clamp(1.75rem,3.6vw,2.75rem)] leading-[1.25] italic">
          {words.map((word, index) => (
            <Fragment key={index}>
              <span className="quote-word inline-block" style={{ "--word": index } as CSSProperties}>
                {word}
              </span>
              {index < words.length - 1 && " "}
            </Fragment>
          ))}
        </p>
      </blockquote>

      <figcaption className="relative mt-10 flex flex-wrap items-center gap-x-4 gap-y-1 font-mono text-xs uppercase tracking-[0.16em]">
        <span aria-hidden className="quote-rule h-px w-14 origin-left bg-brand-contrast" />
        <span className="quote-author">{quote.author}</span>
        {quote.source && <span className="quote-author font-sans text-sm normal-case tracking-normal">{quote.source}</span>}
      </figcaption>
    </figure>
  );
}
