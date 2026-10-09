import type {
  FeedbackTopic,
  FormDefinition,
  FormOption,
  FormQuestion,
  FormSection,
  QuestionType,
} from "../../types/feedback";
import { numericRange } from "./logic";

/*
 * Regras do construtor de formulários. Espelham `FormDefinition.FindProblems` da API (a API valida de novo e
 * devolve erros com caminho, ex.: `Definition.Sections[0].Questions[2].Options`).
 */
export const MAX_SECTIONS = 20;
export const MAX_QUESTIONS = 100;
export const MAX_OPTIONS = 30;

export const questionTypeLabel: Record<QuestionType, string> = {
  ShortText: "Texto curto",
  LongText: "Texto longo",
  Scale: "Escala",
  Nps: "Recomendação (0 a 10)",
  SingleChoice: "Escolha única",
  MultipleChoice: "Múltipla escolha",
};

export const topicLabel: Record<FeedbackTopic, string> = {
  Company: "Sobre a empresa do participante",
  Training: "Sobre a KLF e o treinamento",
};

export function newId(prefix: string) {
  const random = Math.random().toString(36).slice(2, 8).padEnd(6, "0");
  return `${prefix}-${random}`;
}

const isChoice = (type: QuestionType) => type === "SingleChoice" || type === "MultipleChoice";

export function blankOptions(): FormOption[] {
  return [
    { id: newId("o"), label: "" },
    { id: newId("o"), label: "" },
  ];
}

export function blankQuestion(type: QuestionType = "LongText"): FormQuestion {
  return changeQuestionType({ id: newId("q"), type: "ShortText", text: "", required: false }, type);
}

export function blankSection(topic: FeedbackTopic): FormSection {
  return { id: newId("s"), title: "", description: null, topic, questions: [blankQuestion()] };
}

/** Troca o tipo e limpa o que não vale para o tipo novo (a API rejeita opções em texto, escala em escolha etc.). */
export function changeQuestionType(question: FormQuestion, type: QuestionType): FormQuestion {
  const next: FormQuestion = {
    ...question,
    type,
    options: null,
    scaleMin: null,
    scaleMax: null,
    minLabel: null,
    maxLabel: null,
  };

  if (isChoice(type)) next.options = question.options?.length ? question.options : blankOptions();
  if (type === "Scale") {
    next.scaleMin = question.scaleMin ?? 1;
    next.scaleMax = question.scaleMax ?? 5;
  }
  if (type === "Scale" || type === "Nps") {
    next.minLabel = question.minLabel ?? null;
    next.maxLabel = question.maxLabel ?? null;
  }

  return next;
}

/** Perguntas que podem servir de condição para a pergunta na posição dada: vêm antes e têm valores fixos. */
export function conditionSources(definition: FormDefinition, sectionIndex: number, questionIndex: number) {
  const sources: FormQuestion[] = [];

  definition.sections.forEach((section, s) =>
    section.questions.forEach((question, q) => {
      const before = s < sectionIndex || (s === sectionIndex && q < questionIndex);
      if (before && question.type !== "ShortText" && question.type !== "LongText") sources.push(question);
    }),
  );

  return sources;
}

/** Valores que uma pergunta de origem pode ter, para montar a condição "mostrar se a resposta for...". */
export function conditionValues(source: FormQuestion): FormOption[] {
  const range = numericRange(source);
  if (range) {
    return Array.from({ length: range[1] - range[0] + 1 }, (_, i) => {
      const value = String(range[0] + i);
      return { id: value, label: value };
    });
  }

  return (source.options ?? []).map((option) => ({ id: option.id, label: option.label || "(opção sem texto)" }));
}

/**
 * Remove condições que deixaram de fazer sentido (a origem foi apagada, mudou de tipo, foi para depois ou perdeu o valor
 * escolhido). Roda antes de salvar e depois de mover perguntas.
 */
export function pruneConditions(definition: FormDefinition): FormDefinition {
  return {
    sections: definition.sections.map((section, s) => ({
      ...section,
      questions: section.questions.map((question, q) => {
        if (!question.showIf) return question;

        const source = conditionSources(definition, s, q).find((item) => item.id === question.showIf!.questionId);
        const allowed = source ? new Set(conditionValues(source).map((value) => value.id)) : null;
        const anyOf = allowed ? question.showIf.anyOf.filter((value) => allowed.has(value)) : [];

        return anyOf.length > 0 ? { ...question, showIf: { ...question.showIf, anyOf } } : { ...question, showIf: null };
      }),
    })),
  };
}

export function move<T>(items: readonly T[], from: number, to: number): T[] {
  if (to < 0 || to >= items.length || from === to) return [...items];
  const next = [...items];
  const [item] = next.splice(from, 1);
  next.splice(to, 0, item);
  return next;
}

export function countQuestions(definition: FormDefinition) {
  return definition.sections.reduce((total, section) => total + section.questions.length, 0);
}

/** Texto aparado e campos opcionais vazios como `null`, no formato que a API espera. */
export function toPayload(definition: FormDefinition): FormDefinition {
  const optional = (text?: string | null) => (text?.trim() ? text.trim() : null);

  return {
    sections: pruneConditions(definition).sections.map((section) => ({
      ...section,
      title: section.title.trim(),
      description: optional(section.description),
      questions: section.questions.map((question) => ({
        ...question,
        text: question.text.trim(),
        helpText: optional(question.helpText),
        minLabel: optional(question.minLabel),
        maxLabel: optional(question.maxLabel),
        options: question.options?.map((option) => ({ ...option, label: option.label.trim() })) ?? null,
      })),
    })),
  };
}

/** Modelo inicial com as duas partes da avaliação (empresa do participante e treinamento). */
export function starterDefinition(): FormDefinition {
  return {
    sections: [
      {
        id: newId("s"),
        title: "Sobre a sua empresa",
        description: "Suas respostas são anônimas: ninguém da empresa vê quem respondeu.",
        topic: "Company",
        questions: [
          { ...blankQuestion("LongText"), text: "O que a sua empresa faz bem no atendimento?" },
          { ...blankQuestion("LongText"), text: "O que precisa melhorar?" },
          { ...blankQuestion("LongText"), text: "Que sugestão você daria para melhorar?" },
        ],
      },
      {
        id: newId("s"),
        title: "Sobre o treinamento",
        description: null,
        topic: "Training",
        questions: [
          {
            ...blankQuestion("Scale"),
            text: "Que nota você dá para o treinamento?",
            required: true,
            minLabel: "Fraco",
            maxLabel: "Excelente",
          },
          {
            ...blankQuestion("Nps"),
            text: "De 0 a 10, quanto você recomendaria este treinamento a um colega?",
            required: true,
            minLabel: "Nada provável",
            maxLabel: "Muito provável",
          },
          { ...blankQuestion("LongText"), text: "O que pode melhorar no treinamento?" },
        ],
      },
    ],
  };
}

/** Erros da API para um caminho exato da definição (ex.: `Sections[0].Questions[1].Text`). */
export function definitionError(errors: Record<string, string[]> | undefined, path: string) {
  if (!errors) return undefined;
  const wanted = `definition.${path}`.toLowerCase();
  const key = Object.keys(errors).find((name) => name.toLowerCase() === wanted);
  return key ? errors[key][0] : undefined;
}

/** Erros que não pertencem a nenhum campo mostrado (ex.: "Adicione pelo menos uma pergunta."). */
export function generalDefinitionErrors(errors: Record<string, string[]> | undefined) {
  if (!errors) return [];
  return Object.entries(errors)
    .filter(([key]) => /^definition(\.sections)?$/i.test(key))
    .flatMap(([, messages]) => messages);
}
