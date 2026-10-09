"use client";

import { plural } from "@/lib/admin/text";
import { topicLabel } from "@/lib/feedback/builder";
import { cn } from "@/lib/cn";
import type { NpsResult, QuestionResult, SessionResults } from "@/types/admin";
import { LockIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { Kicker, Rule } from "@/components/ui/typography";

const percent = (count: number, total: number) => (total === 0 ? 0 : Math.round((count / total) * 100));

/** Barra horizontal de cor sólida, com o número escrito ao lado (a cor nunca informa sozinha). */
function Bar({ label, count, total, tone = "brand" }: { label: string; count: number; total: number; tone?: "brand" | "accent" }) {
  const value = percent(count, total);

  return (
    <div className="grid grid-cols-[minmax(4rem,9rem)_1fr_4.5rem] items-center gap-3 font-sans text-sm">
      <span className="truncate" title={label}>
        {label}
      </span>
      <span className="h-3 border border-rule-strong bg-paper" aria-hidden>
        <span className={cn("block h-full", tone === "brand" ? "bg-brand" : "bg-accent")} style={{ width: `${value}%` }} />
      </span>
      <span className="text-right font-mono text-xs">
        {count} · {value}%
      </span>
    </div>
  );
}

export function NpsBlock({ nps, compact = false }: { nps: NpsResult; compact?: boolean }) {
  const parts = [
    { label: "Detratores (0–6)", count: nps.detractors, className: "bg-danger" },
    { label: "Neutros (7–8)", count: nps.passives, className: "bg-warning" },
    { label: "Promotores (9–10)", count: nps.promoters, className: "bg-success" },
  ];

  return (
    <div className={cn("grid gap-4", !compact && "sm:grid-cols-[10rem_1fr] sm:items-center sm:gap-8")}>
      <div>
        <Kicker>NPS</Kicker>
        <p className="text-6xl leading-none font-medium tracking-[-0.02em]">{nps.score}</p>
        <p className="mt-1 font-sans text-sm text-ink-soft">{plural(nps.total, "nota", "notas")}, de −100 a 100</p>
      </div>
      <div>
        <div className="flex h-4 border border-ink" aria-hidden>
          {parts.map((part) => (
            <span key={part.label} className={part.className} style={{ width: `${percent(part.count, nps.total)}%` }} />
          ))}
        </div>
        <ul className="mt-3 grid gap-1 font-sans text-sm sm:grid-cols-3">
          {parts.map((part) => (
            <li key={part.label} className="flex items-center gap-2">
              <span aria-hidden className={cn("size-3 shrink-0 border border-ink", part.className)} />
              {part.label}: <span className="font-mono text-xs">{part.count} · {percent(part.count, nps.total)}%</span>
            </li>
          ))}
        </ul>
      </div>
    </div>
  );
}

function QuestionResultView({ question, number }: { question: QuestionResult; number: number }) {
  return (
    <li className="border-t border-rule py-6">
      <div className="flex items-baseline gap-3.5">
        <span className="font-mono text-sm text-ink-soft">{String(number).padStart(2, "0")}</span>
        <div className="min-w-0 flex-1">
          <p className="text-xl leading-snug">{question.text}</p>
          <p className="mt-1 font-sans text-sm text-ink-soft">{plural(question.answerCount, "resposta", "respostas")}</p>
        </div>
      </div>

      <div className="mt-4 sm:ml-[2.4rem]">
        {question.isHidden ? (
          <p className="flex items-center gap-2 font-sans text-sm text-ink-soft">
            <LockIcon /> Menos de 3 respostas nesta pergunta: só a contagem aparece, para proteger quem respondeu.
          </p>
        ) : (
          <>
            {question.type === "Nps" && question.nps && <NpsBlock nps={question.nps} compact />}
            {question.type === "Scale" && (
              <div className="grid gap-2">
                {question.average != null && (
                  <p className="mb-2 font-sans text-sm">
                    Média <span className="text-3xl font-medium">{question.average.toFixed(1).replace(".", ",")}</span>
                  </p>
                )}
                {question.distribution?.map((item) => (
                  <Bar key={item.value} label={String(item.value)} count={item.count} total={question.answerCount} />
                ))}
              </div>
            )}
            {question.type === "Nps" && (
              <div className="mt-4 grid gap-1.5">
                {question.distribution?.map((item) => (
                  <Bar key={item.value} label={String(item.value)} count={item.count} total={question.answerCount} />
                ))}
              </div>
            )}
            {question.options && (
              <div className="grid gap-2">
                {question.options.map((option) => (
                  <Bar key={option.id} label={option.label} count={option.count} total={question.answerCount} />
                ))}
                {question.type === "MultipleChoice" && (
                  <p className="font-sans text-xs text-ink-soft">Múltipla escolha: a soma passa de 100%.</p>
                )}
              </div>
            )}
            {question.texts && (
              <ul className="grid gap-3">
                {question.texts.map((text, index) => (
                  <li key={index} className="border-l-2 border-ink pl-4 text-lg leading-relaxed italic">
                    {text}
                  </li>
                ))}
              </ul>
            )}
          </>
        )}
      </div>
    </li>
  );
}

export function SessionResultsView({ results }: { results: SessionResults }) {
  if (!results.hasEnoughResponses) {
    return (
      <Notice tone="privacy" title={`Resultados a partir de ${results.minimumResponses} respostas`}>
        Para ninguém ser identificado, nada aparece antes disso. Agora: {plural(results.responseCount, "resposta", "respostas")}.
      </Notice>
    );
  }

  const numbers = results.sections.map((_, s) => results.sections.slice(0, s).reduce((total, section) => total + section.questions.length, 1));

  return (
    <div>
      <Notice tone="privacy" className="mb-8">
        Respostas anônimas: sem nome, aparelho ou horário. Os textos aparecem em ordem aleatória. Ao compartilhar com a empresa
        cliente, não tente adivinhar quem escreveu.
      </Notice>
      {results.nps && (
        <div className="mb-10 border-2 border-ink p-6">
          <NpsBlock nps={results.nps} />
        </div>
      )}
      {results.sections.map((section, s) => (
        <section key={section.id} className="mb-10">
          <Rule variant="strong" />
          <Kicker className="mt-4">{topicLabel[section.topic]}</Kicker>
          <h3 className="mt-1 text-3xl">{section.title}</h3>
          <ol className="mt-4">
            {section.questions.map((question, q) => (
              <QuestionResultView key={question.id} question={question} number={numbers[s] + q} />
            ))}
          </ol>
        </section>
      ))}
    </div>
  );
}
