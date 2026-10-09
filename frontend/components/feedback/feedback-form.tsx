"use client";

import { useRef, useState, type FormEvent } from "react";
import { Question } from "@/components/feedback/question";
import { Button } from "@/components/ui/button";
import { Notice } from "@/components/ui/notice";
import { Kicker, Rule } from "@/components/ui/typography";
import { ApiError, apiFetch } from "@/lib/api/client";
import { buildPayload, evaluate, splitServerErrors, validate } from "@/lib/feedback/logic";
import type { AnswerState, AnswerValue, PublicFeedbackForm } from "@/types/feedback";

type Props = {
  code: string;
  form: PublicFeedbackForm & { definition: NonNullable<PublicFeedbackForm["definition"]> };
  onSent: () => void;
  onStateChanged: () => void;
};

export function FeedbackForm({ code, form, onSent, onStateChanged }: Props) {
  const { definition } = form;
  const [answers, setAnswers] = useState<AnswerState>({});
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [message, setMessage] = useState<string | null>(null);
  const [sending, setSending] = useState(false);
  const summaryRef = useRef<HTMLDivElement>(null);

  const { visible } = evaluate(definition, answers);
  const numbering = new Map<string, number>();
  for (const section of definition.sections) {
    for (const question of section.questions) {
      if (visible.has(question.id)) numbering.set(question.id, numbering.size + 1);
    }
  }

  const errorEntries = Object.entries(errors).filter(([id]) => visible.has(id));
  const hasProblems = errorEntries.length > 0 || message !== null;

  function setAnswer(id: string, value: AnswerValue) {
    setAnswers((current) => ({ ...current, [id]: value }));
    setErrors((current) => {
      const next = { ...current };
      delete next[id];
      return next;
    });
  }

  function showProblems() {
    requestAnimationFrame(() => summaryRef.current?.focus());
  }

  function focusQuestion(id: string) {
    document.querySelector<HTMLElement>(`#q-${CSS.escape(id)} input, #q-${CSS.escape(id)} textarea`)?.focus();
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    setMessage(null);

    const found = validate(definition, answers);
    setErrors(found);

    if (Object.keys(found).length > 0) {
      showProblems();
      return;
    }

    setSending(true);

    try {
      await apiFetch(`/public/feedback/${encodeURIComponent(code)}/responses`, {
        method: "POST",
        credentials: "include",
        body: JSON.stringify({ answers: buildPayload(definition, answers) }),
      });
      onSent();
    } catch (error) {
      if (error instanceof ApiError && error.status === 400) {
        const { byQuestion, general } = splitServerErrors(error.errors);
        setErrors(byQuestion);
        setMessage(general.join(" ") || (Object.keys(byQuestion).length ? null : error.message));
      } else if (error instanceof ApiError && error.status === 409) {
        onStateChanged();
        return;
      } else if (error instanceof ApiError && error.status === 429) {
        setMessage("Muitos envios em pouco tempo. Aguarde um minuto e tente de novo.");
      } else {
        setMessage("Não foi possível enviar agora. Confira sua conexão e tente novamente.");
      }

      showProblems();
    } finally {
      setSending(false);
    }
  }

  const shownSections = definition.sections.filter((section) =>
    section.questions.some((question) => visible.has(question.id)),
  );

  return (
    <form onSubmit={submit} noValidate>
      <header>
        <Kicker>Avaliação anônima</Kicker>
        <h1 className="mt-3 text-[clamp(2.25rem,7vw,3.75rem)] leading-[1.05] text-ink">{form.formTitle}</h1>
        <p className="mt-3 font-sans text-base font-medium text-ink-soft">{form.sessionTitle}</p>
        {form.formDescription && <p className="mt-5 max-w-prose text-xl leading-relaxed">{form.formDescription}</p>}
      </header>

      <Notice tone="privacy" title="Sua resposta é anônima" className="mt-8">
        Não pedimos nome, e-mail nem telefone, e não guardamos dados do seu aparelho nem o horário exato do envio. Os
        resultados só aparecem em grupo, a partir de 3 respostas.
      </Notice>

      <div
        ref={summaryRef}
        tabIndex={-1}
        role={hasProblems ? "alert" : undefined}
        className="outline-offset-4 empty:hidden"
      >
        {hasProblems && (
          <Notice tone="error" title="Falta pouco para enviar" className="mt-6">
            {message && <p>{message}</p>}
            {errorEntries.length > 0 && (
              <ul className="mt-1 list-disc space-y-0.5 pl-5">
                {errorEntries.map(([id, text]) => (
                  <li key={id}>
                    <a
                      href={`#q-${id}`}
                      onClick={(event) => {
                        event.preventDefault();
                        focusQuestion(id);
                      }}
                      className="underline underline-offset-2"
                    >
                      Pergunta {String(numbering.get(id) ?? 0).padStart(2, "0")}
                    </a>
                    : {text}
                  </li>
                ))}
              </ul>
            )}
          </Notice>
        )}
      </div>

      {shownSections.map((section, index) => {
        const shown = section.questions.filter((question) => visible.has(question.id));

        return (
          <section key={section.id} aria-labelledby={`s-${section.id}`} className="mt-14">
            <Rule variant="strong" />
            <div className="mt-4 flex items-baseline justify-between gap-4">
              <Kicker>
                Parte {index + 1} de {shownSections.length}
              </Kicker>
            </div>
            <h2 id={`s-${section.id}`} className="mt-2 text-3xl leading-tight text-ink sm:text-4xl">
              {section.title}
            </h2>
            {section.description && <p className="mt-3 max-w-prose text-lg text-ink-soft">{section.description}</p>}

            <div className="mt-9 space-y-10">
              {shown.map((question) => (
                <Question
                  key={question.id}
                  question={question}
                  number={numbering.get(question.id)!}
                  value={answers[question.id]}
                  error={errors[question.id]}
                  onChange={(value) => setAnswer(question.id, value)}
                />
              ))}
            </div>
          </section>
        );
      })}

      <div className="mt-16">
        <Rule variant="double" />
        <p className="mt-6 max-w-prose font-sans text-sm text-ink-soft">
          Ao enviar, sua resposta é registrada sem qualquer identificação e não poderá ser alterada depois.
        </p>
        <Button type="submit" size="lg" className="mt-5 w-full sm:w-auto" disabled={sending} aria-disabled={sending}>
          {sending ? "Enviando…" : "Enviar avaliação"}
        </Button>
      </div>
    </form>
  );
}
