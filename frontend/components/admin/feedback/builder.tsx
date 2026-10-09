"use client";

import { useState } from "react";
import {
  MAX_OPTIONS,
  MAX_QUESTIONS,
  MAX_SECTIONS,
  blankQuestion,
  blankSection,
  changeQuestionType,
  conditionSources,
  conditionValues,
  countQuestions,
  definitionError,
  generalDefinitionErrors,
  move,
  newId,
  pruneConditions,
  questionTypeLabel,
  topicLabel,
} from "@/lib/feedback/builder";
import { evaluate } from "@/lib/feedback/logic";
import { cn } from "@/lib/cn";
import type { AnswerState, FeedbackTopic, FormDefinition, FormQuestion, FormSection, QuestionType } from "@/types/feedback";
import { Question } from "@/components/feedback/question";
import { Button } from "@/components/ui/button";
import { Checkbox, Input, Select, Textarea } from "@/components/ui/field";
import { ArrowDownIcon, ArrowUpIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { Kicker, Rule } from "@/components/ui/typography";
import { Dialog } from "../ui";

type Errors = Record<string, string[]> | undefined;
type Change = (definition: FormDefinition) => void;

const questionTypes = Object.keys(questionTypeLabel) as QuestionType[];
const topics = Object.keys(topicLabel) as FeedbackTopic[];

function MoveButtons({
  label,
  index,
  count,
  onMove,
}: {
  label: string;
  index: number;
  count: number;
  onMove: (to: number) => void;
}) {
  return (
    <>
      <Button variant="outline" className="min-w-11 px-0" aria-label={`Subir ${label}`} disabled={index === 0} onClick={() => onMove(index - 1)}>
        <ArrowUpIcon />
      </Button>
      <Button
        variant="outline"
        className="min-w-11 px-0"
        aria-label={`Descer ${label}`}
        disabled={index === count - 1}
        onClick={() => onMove(index + 1)}
      >
        <ArrowDownIcon />
      </Button>
    </>
  );
}

function OptionsEditor({
  question,
  path,
  errors,
  onChange,
}: {
  question: FormQuestion;
  path: string;
  errors: Errors;
  onChange: (question: FormQuestion) => void;
}) {
  const options = question.options ?? [];
  const setOptions = (next: typeof options) => onChange({ ...question, options: next });

  return (
    <fieldset className="grid gap-3">
      <legend className="mb-2 font-sans text-sm font-medium text-ink">Opções</legend>
      {options.map((option, index) => (
        <div key={option.id} className="flex items-start gap-2">
          <span className="w-6 shrink-0 pt-3.5 font-mono text-xs text-ink-soft">{String.fromCharCode(97 + (index % 26))})</span>
          <div className="min-w-0 flex-1">
            <Input
              label={`Opção ${index + 1}`}
              className="py-2.5"
              value={option.label}
              maxLength={200}
              onChange={(event) => setOptions(options.map((item, i) => (i === index ? { ...item, label: event.target.value } : item)))}
              error={definitionError(errors, `${path}.Options[${index}].Label`)}
            />
          </div>
          <div className="flex gap-1 pt-7">
            <MoveButtons label={`opção ${index + 1}`} index={index} count={options.length} onMove={(to) => setOptions(move(options, index, to))} />
            <Button
              variant="quiet"
              className="text-danger"
              disabled={options.length <= 2}
              onClick={() => setOptions(options.filter((_, i) => i !== index))}
            >
              Tirar
            </Button>
          </div>
        </div>
      ))}
      <p className="font-sans text-sm text-danger empty:hidden">{definitionError(errors, `${path}.Options`)}</p>
      <div>
        <Button variant="outline" disabled={options.length >= MAX_OPTIONS} onClick={() => setOptions([...options, { id: newId("o"), label: "" }])}>
          Adicionar opção
        </Button>
      </div>
    </fieldset>
  );
}

function ScaleEditor({ question, path, errors, onChange }: { question: FormQuestion; path: string; errors: Errors; onChange: (question: FormQuestion) => void }) {
  const isScale = question.type === "Scale";

  return (
    <div className="grid gap-4">
      {isScale && (
        <div className="grid grid-cols-2 gap-4 sm:max-w-sm">
          <Select
            label="De"
            value={String(question.scaleMin ?? 1)}
            onChange={(event) => onChange({ ...question, scaleMin: Number(event.target.value) })}
            error={definitionError(errors, `${path}.ScaleMin`)}
          >
            <option value="0">0</option>
            <option value="1">1</option>
          </Select>
          <Select
            label="Até"
            value={String(question.scaleMax ?? 5)}
            onChange={(event) => onChange({ ...question, scaleMax: Number(event.target.value) })}
            error={definitionError(errors, `${path}.ScaleMax`)}
          >
            {[2, 3, 4, 5, 6, 7, 8, 9, 10].map((value) => (
              <option key={value} value={value}>
                {value}
              </option>
            ))}
          </Select>
        </div>
      )}
      <div className="grid gap-4 sm:grid-cols-2">
        <Input
          label="Legenda do menor valor"
          optional
          placeholder={isScale ? "Ex.: Fraco" : "Ex.: Nada provável"}
          value={question.minLabel ?? ""}
          maxLength={60}
          onChange={(event) => onChange({ ...question, minLabel: event.target.value })}
          error={definitionError(errors, `${path}.MinLabel`)}
        />
        <Input
          label="Legenda do maior valor"
          optional
          placeholder={isScale ? "Ex.: Excelente" : "Ex.: Muito provável"}
          value={question.maxLabel ?? ""}
          maxLength={60}
          onChange={(event) => onChange({ ...question, maxLabel: event.target.value })}
          error={definitionError(errors, `${path}.MaxLabel`)}
        />
      </div>
    </div>
  );
}

function ConditionEditor({
  definition,
  sectionIndex,
  questionIndex,
  question,
  path,
  errors,
  onChange,
}: {
  definition: FormDefinition;
  sectionIndex: number;
  questionIndex: number;
  question: FormQuestion;
  path: string;
  errors: Errors;
  onChange: (question: FormQuestion) => void;
}) {
  const sources = conditionSources(definition, sectionIndex, questionIndex);
  const condition = question.showIf;
  const source = sources.find((item) => item.id === condition?.questionId);

  if (sources.length === 0) {
    return (
      <p className="font-sans text-sm text-ink-soft">
        Para mostrar esta pergunta só em alguns casos, coloque antes dela uma pergunta de escala, recomendação ou escolha.
      </p>
    );
  }

  return (
    <div className="grid gap-4">
      <Checkbox
        label="Mostrar só dependendo de uma resposta anterior"
        checked={Boolean(condition)}
        onChange={(event) =>
          onChange({ ...question, showIf: event.target.checked ? { questionId: sources[sources.length - 1].id, anyOf: [] } : null })
        }
      />
      {condition && (
        <div className="grid gap-4 border-l-2 border-brand pl-4">
          <Select
            label="Quando a pergunta"
            value={condition.questionId}
            onChange={(event) => onChange({ ...question, showIf: { questionId: event.target.value, anyOf: [] } })}
            error={definitionError(errors, `${path}.ShowIf.QuestionId`)}
          >
            {sources.map((item) => (
              <option key={item.id} value={item.id}>
                {item.text || "(pergunta sem texto)"}
              </option>
            ))}
          </Select>
          {source && (
            <fieldset>
              <legend className="mb-2 font-sans text-sm font-medium text-ink">tiver como resposta qualquer um destes:</legend>
              <div className="flex flex-wrap gap-2">
                {conditionValues(source).map((value) => {
                  const checked = condition.anyOf.includes(value.id);
                  return (
                    <label
                      key={value.id}
                      className={cn(
                        "flex min-h-11 cursor-pointer items-center border px-3 font-sans text-sm",
                        "has-focus-visible:outline-2 has-focus-visible:outline-offset-2 has-focus-visible:outline-ring",
                        checked ? "border-brand bg-brand text-brand-contrast" : "border-rule-strong bg-card hover:border-ink",
                      )}
                    >
                      <input
                        type="checkbox"
                        className="sr-only"
                        checked={checked}
                        onChange={() =>
                          onChange({
                            ...question,
                            showIf: {
                              ...condition,
                              anyOf: checked ? condition.anyOf.filter((item) => item !== value.id) : [...condition.anyOf, value.id],
                            },
                          })
                        }
                      />
                      {value.label}
                    </label>
                  );
                })}
              </div>
              <p className="mt-2 font-sans text-sm text-danger empty:hidden">{definitionError(errors, `${path}.ShowIf.AnyOf`)}</p>
            </fieldset>
          )}
        </div>
      )}
    </div>
  );
}

function QuestionEditor({
  definition,
  sectionIndex,
  questionIndex,
  number,
  count,
  errors,
  onChange,
  onMove,
  onRemove,
}: {
  definition: FormDefinition;
  sectionIndex: number;
  questionIndex: number;
  number: number;
  count: number;
  errors: Errors;
  onChange: (question: FormQuestion) => void;
  onMove: (to: number) => void;
  onRemove: () => void;
}) {
  const question = definition.sections[sectionIndex].questions[questionIndex];
  const path = `Sections[${sectionIndex}].Questions[${questionIndex}]`;
  const [showHelp, setShowHelp] = useState(Boolean(question.helpText));
  const isChoice = question.type === "SingleChoice" || question.type === "MultipleChoice";
  const isScale = question.type === "Scale" || question.type === "Nps";

  return (
    <li className="grid gap-5 border-t border-rule py-6 sm:grid-cols-[3rem_1fr]">
      <span className="font-mono text-sm font-medium text-ink-soft">{String(number).padStart(2, "0")}</span>
      <div className="grid min-w-0 gap-5">
        <div className="grid gap-4 md:grid-cols-[14rem_1fr]">
          <Select
            label="Tipo"
            value={question.type}
            onChange={(event) => onChange(changeQuestionType(question, event.target.value as QuestionType))}
            error={definitionError(errors, `${path}.Type`)}
          >
            {questionTypes.map((type) => (
              <option key={type} value={type}>
                {questionTypeLabel[type]}
              </option>
            ))}
          </Select>
          <Input
            label="Pergunta"
            value={question.text}
            maxLength={500}
            onChange={(event) => onChange({ ...question, text: event.target.value })}
            error={definitionError(errors, `${path}.Text`)}
          />
        </div>

        {showHelp ? (
          <Input
            label="Texto de ajuda"
            optional
            hint="Aparece abaixo da pergunta."
            value={question.helpText ?? ""}
            maxLength={500}
            onChange={(event) => onChange({ ...question, helpText: event.target.value })}
            error={definitionError(errors, `${path}.HelpText`)}
          />
        ) : (
          <div>
            <Button variant="quiet" className="px-0" onClick={() => setShowHelp(true)}>
              Adicionar texto de ajuda
            </Button>
          </div>
        )}

        {isChoice && <OptionsEditor question={question} path={path} errors={errors} onChange={onChange} />}
        {isScale && <ScaleEditor question={question} path={path} errors={errors} onChange={onChange} />}

        <Checkbox label="Resposta obrigatória" checked={question.required} onChange={(event) => onChange({ ...question, required: event.target.checked })} />

        <ConditionEditor
          definition={definition}
          sectionIndex={sectionIndex}
          questionIndex={questionIndex}
          question={question}
          path={path}
          errors={errors}
          onChange={onChange}
        />

        <div className="flex flex-wrap gap-2">
          <MoveButtons label={`pergunta ${number}`} index={questionIndex} count={count} onMove={onMove} />
          <Button variant="quiet" className="text-danger" onClick={onRemove}>
            Excluir pergunta
          </Button>
        </div>
      </div>
    </li>
  );
}

function SectionEditor({
  definition,
  sectionIndex,
  firstNumber,
  errors,
  onChange,
  onMove,
  onRemove,
}: {
  definition: FormDefinition;
  sectionIndex: number;
  firstNumber: number;
  errors: Errors;
  onChange: (section: FormSection) => void;
  onMove: (to: number) => void;
  onRemove: () => void;
}) {
  const section = definition.sections[sectionIndex];
  const path = `Sections[${sectionIndex}]`;
  const [newType, setNewType] = useState<QuestionType>("LongText");
  const total = countQuestions(definition);

  function setQuestions(questions: FormQuestion[]) {
    onChange({ ...section, questions });
  }

  return (
    <section aria-label={`Parte ${sectionIndex + 1}`} className="border-2 border-ink bg-card">
      <div className="flex flex-wrap items-center justify-between gap-3 border-b border-ink bg-paper-deep px-5 py-3">
        <Kicker className="text-ink">Parte {sectionIndex + 1}</Kicker>
        <div className="flex flex-wrap gap-2">
          <MoveButtons label={`parte ${sectionIndex + 1}`} index={sectionIndex} count={definition.sections.length} onMove={onMove} />
          <Button variant="quiet" className="text-danger" disabled={definition.sections.length <= 1} onClick={onRemove}>
            Excluir parte
          </Button>
        </div>
      </div>
      <div className="grid gap-5 px-5 pt-5 pb-2">
        <Select
          label="Assunto"
          hint="Separa os resultados: o que é sobre a empresa do participante e o que é sobre a KLF."
          value={section.topic}
          onChange={(event) => onChange({ ...section, topic: event.target.value as FeedbackTopic })}
          error={definitionError(errors, `${path}.Topic`)}
        >
          {topics.map((topic) => (
            <option key={topic} value={topic}>
              {topicLabel[topic]}
            </option>
          ))}
        </Select>
        <Input
          label="Título da parte"
          value={section.title}
          maxLength={200}
          onChange={(event) => onChange({ ...section, title: event.target.value })}
          error={definitionError(errors, `${path}.Title`)}
        />
        <Textarea
          label="Texto de apoio"
          optional
          value={section.description ?? ""}
          maxLength={1000}
          onChange={(event) => onChange({ ...section, description: event.target.value })}
          error={definitionError(errors, `${path}.Description`)}
          className="min-h-20"
        />
        <p className="font-sans text-sm text-danger empty:hidden">{definitionError(errors, `${path}.Questions`)}</p>
      </div>
      <ol className="px-5">
        {section.questions.map((question, questionIndex) => (
          <QuestionEditor
            key={question.id}
            definition={definition}
            sectionIndex={sectionIndex}
            questionIndex={questionIndex}
            number={firstNumber + questionIndex}
            count={section.questions.length}
            errors={errors}
            onChange={(next) => setQuestions(section.questions.map((item, i) => (i === questionIndex ? next : item)))}
            onMove={(to) => setQuestions(move(section.questions, questionIndex, to))}
            onRemove={() => setQuestions(section.questions.filter((_, i) => i !== questionIndex))}
          />
        ))}
      </ol>
      <div className="flex flex-wrap items-end gap-3 border-t border-rule px-5 py-5">
        <div className="w-full sm:w-64">
          <Select label="Nova pergunta do tipo" value={newType} onChange={(event) => setNewType(event.target.value as QuestionType)}>
            {questionTypes.map((type) => (
              <option key={type} value={type}>
                {questionTypeLabel[type]}
              </option>
            ))}
          </Select>
        </div>
        <Button variant="outline" disabled={total >= MAX_QUESTIONS} onClick={() => setQuestions([...section.questions, blankQuestion(newType)])}>
          Adicionar pergunta
        </Button>
      </div>
    </section>
  );
}

/**
 * Construtor livre de formulário: partes (empresa do participante / treinamento) com perguntas de vários tipos.
 * Os erros da API chegam com caminho (`Definition.Sections[0].Questions[2].Text`) e aparecem no campo certo.
 */
export function FormBuilder({ value, onChange, errors }: { value: FormDefinition; onChange: Change; errors: Errors }) {
  const firstNumbers = value.sections.map((_, s) => value.sections.slice(0, s).reduce((total, section) => total + section.questions.length, 1));
  const general = generalDefinitionErrors(errors);

  function update(next: FormDefinition) {
    onChange(pruneConditions(next));
  }

  return (
    <div className="grid gap-8">
      {general.length > 0 && (
        <Notice tone="error" role="alert">
          {general.join(" ")}
        </Notice>
      )}
      {value.sections.map((section, sectionIndex) => (
        <SectionEditor
          key={section.id}
          definition={value}
          sectionIndex={sectionIndex}
          firstNumber={firstNumbers[sectionIndex]}
          errors={errors}
          onChange={(next) => update({ sections: value.sections.map((item, i) => (i === sectionIndex ? next : item)) })}
          onMove={(to) => update({ sections: move(value.sections, sectionIndex, to) })}
          onRemove={() => update({ sections: value.sections.filter((_, i) => i !== sectionIndex) })}
        />
      ))}
      <div className="flex flex-wrap gap-3">
        {topics.map((topic) => (
          <Button
            key={topic}
            variant="outline"
            disabled={value.sections.length >= MAX_SECTIONS}
            onClick={() => update({ sections: [...value.sections, blankSection(topic)] })}
          >
            Nova parte: {topic === "Company" ? "empresa" : "treinamento"}
          </Button>
        ))}
      </div>
    </div>
  );
}

/** Pré-visualização interativa: mesmas perguntas e condições que o participante vê, sem enviar nada. */
export function FormPreview({ title, description, definition }: { title: string; description?: string | null; definition: FormDefinition }) {
  const [answers, setAnswers] = useState<AnswerState>({});
  const { visible } = evaluate(definition, answers);
  const visibleIds = definition.sections.flatMap((section) =>
    section.questions.filter((question) => visible.has(question.id)).map((question) => question.id),
  );
  const numbering = new Map(visibleIds.map((id, index) => [id, index + 1]));

  return (
    <div>
      <Kicker>Avaliação anônima · pré-visualização</Kicker>
      <h3 className="mt-3 text-4xl leading-tight">{title || "(sem título)"}</h3>
      {description && <p className="mt-4 max-w-prose text-lg">{description}</p>}
      {definition.sections.map((section, index) => (
        <div key={section.id} className="mt-10">
          <Rule variant="strong" />
          <Kicker className="mt-4">Parte {index + 1}</Kicker>
          <h4 className="mt-2 text-3xl">{section.title || "(parte sem título)"}</h4>
          {section.description && <p className="mt-2 text-lg text-ink-soft">{section.description}</p>}
          <div className="mt-8 space-y-10">
            {section.questions
              .filter((question) => visible.has(question.id))
              .map((question) => (
                <Question
                  key={question.id}
                  question={question}
                  number={numbering.get(question.id)!}
                  value={answers[question.id]}
                  onChange={(value) => setAnswers((current) => ({ ...current, [question.id]: value }))}
                />
              ))}
          </div>
        </div>
      ))}
    </div>
  );
}

export function PreviewButton({ title, description, definition }: { title: string; description?: string | null; definition: FormDefinition }) {
  const [open, setOpen] = useState(false);

  return (
    <>
      <Button variant="outline" onClick={() => setOpen(true)}>
        Pré-visualizar
      </Button>
      <Dialog open={open} onClose={() => setOpen(false)} title="Como o participante vê" wide>
        {open && <FormPreview title={title} description={description} definition={definition} />}
      </Dialog>
    </>
  );
}
