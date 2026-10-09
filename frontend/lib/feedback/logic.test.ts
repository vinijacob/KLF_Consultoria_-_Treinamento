import { describe, expect, it } from "vitest";
import type { FormDefinition } from "../../types/feedback";
import { buildPayload, evaluate, splitServerErrors, validate } from "./logic";

const definition: FormDefinition = {
  sections: [
    {
      id: "empresa",
      title: "Sobre a sua empresa",
      topic: "Company",
      questions: [
        { id: "bom", type: "LongText", text: "O que faz bem?", required: true },
        { id: "melhorar", type: "LongText", text: "O que melhorar?", required: false },
      ],
    },
    {
      id: "klf",
      title: "Sobre o treinamento",
      topic: "Training",
      questions: [
        { id: "nota", type: "Scale", text: "Nota", required: true, scaleMin: 1, scaleMax: 5 },
        { id: "nps", type: "Nps", text: "Recomendaria?", required: true },
        {
          id: "porque",
          type: "LongText",
          text: "O que faltou?",
          required: true,
          showIf: { questionId: "nps", anyOf: ["0", "1", "2", "3", "4", "5", "6"] },
        },
        {
          id: "detalhe",
          type: "ShortText",
          text: "Algo mais?",
          required: true,
          showIf: { questionId: "porque", anyOf: ["x"] },
        },
        {
          id: "formato",
          type: "SingleChoice",
          text: "Formato",
          required: false,
          options: [
            { id: "presencial", label: "Presencial" },
            { id: "online", label: "Online" },
          ],
        },
      ],
    },
  ],
};

const base = { bom: { text: "Equipe unida" }, nota: { number: 4 }, nps: { number: 10 } };

describe("evaluate", () => {
  it("esconde a pergunta condicional quando a condição não é atendida", () => {
    const { visible } = evaluate(definition, base);

    expect(visible.has("porque")).toBe(false);
  });

  it("mostra a pergunta condicional quando a nota está na lista", () => {
    const { visible } = evaluate(definition, { ...base, nps: { number: 3 } });

    expect(visible.has("porque")).toBe(true);
  });

  it("esconde perguntas que dependem de uma pergunta oculta", () => {
    const { visible } = evaluate(definition, { ...base, nps: { number: 10 }, porque: { text: "x" } });

    expect(visible.has("detalhe")).toBe(false);
  });

  it("não considera resposta vazia como resposta para a condição", () => {
    const { visible } = evaluate(definition, { ...base, nps: { number: 3 }, porque: { text: "   " } });

    expect(visible.has("detalhe")).toBe(false);
  });
});

describe("validate", () => {
  it("não acusa erro quando as obrigatórias visíveis estão respondidas", () => {
    expect(validate(definition, base)).toEqual({});
  });

  it("exige as obrigatórias visíveis e ignora as ocultas", () => {
    const errors = validate(definition, { nps: { number: 10 } });

    expect(Object.keys(errors).sort()).toEqual(["bom", "nota"]);
    expect(errors.bom).toBe("Responda esta pergunta.");
  });

  it("exige a condicional quando ela aparece", () => {
    const errors = validate(definition, { ...base, nps: { number: 2 } });

    expect(errors).toEqual({ porque: "Responda esta pergunta." });
  });

  it("recusa texto acima do limite e valor fora da escala", () => {
    const errors = validate(definition, {
      ...base,
      bom: { text: "a".repeat(3001) },
      nota: { number: 9 },
    });

    expect(errors.bom).toBe("Use no máximo 3000 caracteres.");
    expect(errors.nota).toBe("Escolha um valor de 1 a 5.");
  });

  it("aceita zero como resposta válida de NPS", () => {
    const errors = validate(definition, { ...base, nps: { number: 0 }, porque: { text: "tudo" } });

    expect(errors).toEqual({});
  });
});

describe("buildPayload", () => {
  it("envia só o que está visível e respondido, com texto aparado", () => {
    const payload = buildPayload(definition, {
      ...base,
      bom: { text: "  Equipe unida  " },
      melhorar: { text: "   " },
      porque: { text: "não deveria ir" },
      formato: { choices: ["online"] },
    });

    expect(payload).toEqual([
      { questionId: "bom", text: "Equipe unida" },
      { questionId: "nota", number: 4 },
      { questionId: "nps", number: 10 },
      { questionId: "formato", choices: ["online"] },
    ]);
  });

  it("mantém o zero do NPS no envio", () => {
    const payload = buildPayload(definition, { ...base, nps: { number: 0 }, porque: { text: "ruim" } });

    expect(payload).toContainEqual({ questionId: "nps", number: 0 });
    expect(payload).toContainEqual({ questionId: "porque", text: "ruim" });
  });
});

describe("splitServerErrors", () => {
  it("separa erros por pergunta dos erros gerais", () => {
    const result = splitServerErrors({
      "Answers.nota": ["Escolha um valor de 1 a 5."],
      Answers: ["Responda pelo menos uma pergunta."],
    });

    expect(result.byQuestion).toEqual({ nota: "Escolha um valor de 1 a 5." });
    expect(result.general).toEqual(["Responda pelo menos uma pergunta."]);
  });
});
