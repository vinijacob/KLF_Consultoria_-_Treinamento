"use client";

import { useId } from "react";
import { ChoiceList } from "@/components/ui/choice";
import { controlClasses, FieldError } from "@/components/ui/field";
import { NumberScale } from "@/components/ui/number-scale";
import { LONG_TEXT_MAX, SHORT_TEXT_MAX, numericRange } from "@/lib/feedback/logic";
import type { AnswerValue, FormQuestion } from "@/types/feedback";

type QuestionProps = {
  question: FormQuestion;
  number: number;
  value: AnswerValue | undefined;
  error?: string;
  onChange: (value: AnswerValue) => void;
};

function Title({ question, number }: { question: FormQuestion; number: number }) {
  return (
    <span className="flex items-baseline gap-3.5">
      <span className="font-mono text-sm font-medium tracking-wider text-ink-soft">
        {String(number).padStart(2, "0")}
      </span>
      <span className="text-[1.375rem] leading-snug font-medium text-ink">{question.text}</span>
    </span>
  );
}

function Meta({ question, helpId }: { question: FormQuestion; helpId: string }) {
  return (
    <div className="mt-1.5 ml-[2.4rem] flex flex-wrap items-center gap-x-3 gap-y-1 font-sans text-sm text-ink-soft">
      <span className="font-mono text-xs uppercase tracking-[0.12em]">
        {question.required ? "obrigatória" : "opcional"}
      </span>
      {question.helpText && <span id={helpId}>{question.helpText}</span>}
    </div>
  );
}

/** Uma pergunta do formulário. Texto usa <label>; escala e escolha usam <fieldset>/<legend> (leitores de tela leem o grupo). */
export function Question({ question, number, value, error, onChange }: QuestionProps) {
  const uid = useId();
  const helpId = `${uid}-help`;
  const errorId = `${uid}-error`;
  const describedBy = [question.helpText && helpId, error && errorId].filter(Boolean).join(" ") || undefined;
  const isText = question.type === "ShortText" || question.type === "LongText";

  if (isText) {
    const max = question.type === "ShortText" ? SHORT_TEXT_MAX : LONG_TEXT_MAX;
    const text = value?.text ?? "";
    const common = {
      id: uid,
      value: text,
      maxLength: max + 200,
      "aria-invalid": error ? true : undefined,
      "aria-describedby": describedBy,
      className: controlClasses,
      onChange: (event: { target: { value: string } }) => onChange({ text: event.target.value }),
    };

    return (
      <div id={`q-${question.id}`} className="scroll-mt-6">
        <label htmlFor={uid} className="block">
          <Title question={question} number={number} />
        </label>
        <Meta question={question} helpId={helpId} />
        <div className="mt-3.5">
          {question.type === "ShortText" ? (
            <input type="text" autoComplete="off" {...common} />
          ) : (
            <textarea rows={5} {...common} className={`${controlClasses} min-h-36 resize-y leading-relaxed`} />
          )}
          {text.length > max * 0.8 && (
            <p
              className={`mt-1.5 text-right font-mono text-xs ${text.length > max ? "font-medium text-danger" : "text-ink-soft"}`}
            >
              {text.length.toLocaleString("pt-BR")} / {max.toLocaleString("pt-BR")}
            </p>
          )}
          <FieldError id={errorId}>{error}</FieldError>
        </div>
      </div>
    );
  }

  const range = numericRange(question);

  return (
    <fieldset id={`q-${question.id}`} className="scroll-mt-6 min-w-0">
      <legend className="w-full p-0">
        <Title question={question} number={number} />
      </legend>
      <Meta question={question} helpId={helpId} />
      <div className="mt-3.5">
        {range ? (
          <NumberScale
            name={question.id}
            min={range[0]}
            max={range[1]}
            value={value?.number ?? null}
            onChange={(next) => onChange({ number: next })}
            minLabel={question.minLabel ?? (question.type === "Nps" ? "Nada provável" : null)}
            maxLabel={question.maxLabel ?? (question.type === "Nps" ? "Muito provável" : null)}
            describedBy={describedBy}
            invalid={!!error}
          />
        ) : (
          <ChoiceList
            name={question.id}
            type={question.type === "SingleChoice" ? "radio" : "checkbox"}
            options={question.options ?? []}
            value={value?.choices ?? []}
            onChange={(choices) => onChange({ choices })}
            describedBy={describedBy}
            invalid={!!error}
          />
        )}
        <FieldError id={errorId}>{error}</FieldError>
      </div>
    </fieldset>
  );
}
