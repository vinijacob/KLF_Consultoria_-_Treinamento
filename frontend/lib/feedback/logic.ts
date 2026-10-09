import type {
  AnswerPayload,
  AnswerState,
  AnswerValue,
  FormDefinition,
  FormQuestion,
} from "../../types/feedback";

export const SHORT_TEXT_MAX = 300;
export const LONG_TEXT_MAX = 3000;

export function numericRange(question: FormQuestion): [number, number] | null {
  if (question.type === "Nps") return [0, 10];
  if (question.type === "Scale") return [question.scaleMin ?? 1, question.scaleMax ?? 5];
  return null;
}

export function isAnswered(answer: AnswerValue | undefined): boolean {
  if (!answer) return false;
  return (
    (answer.text?.trim().length ?? 0) > 0 ||
    (answer.number !== undefined && answer.number !== null) ||
    (answer.choices?.length ?? 0) > 0
  );
}

function conditionMet(question: FormQuestion, kept: AnswerState): boolean {
  const condition = question.showIf;
  if (!condition) return true;

  const answer = kept[condition.questionId];
  if (!answer) return false;

  if (answer.number !== undefined && answer.number !== null) {
    return condition.anyOf.includes(String(answer.number));
  }

  return answer.choices?.some((choice) => condition.anyOf.includes(choice)) ?? false;
}

/**
 * Mesma regra do servidor: percorre as perguntas em ordem; uma condição só olha respostas de perguntas
 * anteriores que também estão visíveis. Respostas de perguntas ocultas não são enviadas.
 */
export function evaluate(definition: FormDefinition, answers: AnswerState) {
  const visible = new Set<string>();
  const kept: AnswerState = {};

  for (const section of definition.sections) {
    for (const question of section.questions) {
      if (!conditionMet(question, kept)) continue;

      visible.add(question.id);
      if (isAnswered(answers[question.id])) kept[question.id] = answers[question.id];
    }
  }

  return { visible, kept };
}

export function validate(definition: FormDefinition, answers: AnswerState): Record<string, string> {
  const errors: Record<string, string> = {};
  const { visible } = evaluate(definition, answers);

  for (const section of definition.sections) {
    for (const question of section.questions) {
      if (!visible.has(question.id)) continue;

      const answer = answers[question.id];

      if (!isAnswered(answer)) {
        if (question.required) errors[question.id] = "Responda esta pergunta.";
        continue;
      }

      const problem = checkAnswer(question, answer);
      if (problem) errors[question.id] = problem;
    }
  }

  return errors;
}

function checkAnswer(question: FormQuestion, answer: AnswerValue): string | null {
  switch (question.type) {
    case "ShortText":
    case "LongText": {
      const max = question.type === "ShortText" ? SHORT_TEXT_MAX : LONG_TEXT_MAX;
      return (answer.text?.trim().length ?? 0) > max ? `Use no máximo ${max} caracteres.` : null;
    }
    case "Scale":
    case "Nps": {
      const [min, max] = numericRange(question)!;
      const value = answer.number;
      return value === undefined || value === null || value < min || value > max
        ? `Escolha um valor de ${min} a ${max}.`
        : null;
    }
    case "SingleChoice":
      return answer.choices?.length === 1 ? null : "Escolha apenas uma opção.";
    case "MultipleChoice":
      return null;
  }
}

/** Monta o corpo do envio: só perguntas visíveis e respondidas, com texto sem espaços sobrando. */
export function buildPayload(definition: FormDefinition, answers: AnswerState): AnswerPayload[] {
  const { kept } = evaluate(definition, answers);
  const payload: AnswerPayload[] = [];

  for (const section of definition.sections) {
    for (const question of section.questions) {
      const answer = kept[question.id];
      if (!answer) continue;

      if (question.type === "ShortText" || question.type === "LongText") {
        payload.push({ questionId: question.id, text: answer.text!.trim() });
      } else if (question.type === "Scale" || question.type === "Nps") {
        payload.push({ questionId: question.id, number: answer.number! });
      } else {
        payload.push({ questionId: question.id, choices: answer.choices! });
      }
    }
  }

  return payload;
}

/** Traduz os erros do servidor ("Answers.{id}") para erros por pergunta; o resto vira mensagem geral. */
export function splitServerErrors(errors: Record<string, string[]> | undefined) {
  const byQuestion: Record<string, string> = {};
  const general: string[] = [];

  for (const [key, messages] of Object.entries(errors ?? {})) {
    if (key.startsWith("Answers.")) byQuestion[key.slice("Answers.".length)] = messages.join(" ");
    else general.push(...messages);
  }

  return { byQuestion, general };
}
