import { describe, expect, it } from "vitest";
import type { FormDefinition } from "../../types/feedback";
import {
  blankQuestion,
  changeQuestionType,
  conditionSources,
  conditionValues,
  definitionError,
  generalDefinitionErrors,
  move,
  pruneConditions,
  starterDefinition,
  toPayload,
} from "./builder";

function definition(): FormDefinition {
  return {
    sections: [
      {
        id: "s1",
        title: "Treinamento",
        topic: "Training",
        questions: [
          { id: "nota", type: "Scale", text: "Nota", required: true, scaleMin: 1, scaleMax: 5 },
          { id: "canal", type: "SingleChoice", text: "Canal", required: false, options: [{ id: "loja", label: "Loja" }, { id: "site", label: "Site" }] },
          { id: "porque", type: "LongText", text: "Por quê?", required: false, showIf: { questionId: "nota", anyOf: ["1", "2"] } },
        ],
      },
    ],
  };
}

describe("builder", () => {
  it("limpa campos do tipo anterior ao trocar o tipo da pergunta", () => {
    const choice = changeQuestionType(blankQuestion("ShortText"), "SingleChoice");
    expect(choice.options).toHaveLength(2);

    const scale = changeQuestionType(choice, "Scale");
    expect(scale.options).toBeNull();
    expect([scale.scaleMin, scale.scaleMax]).toEqual([1, 5]);

    const text = changeQuestionType(scale, "LongText");
    expect([text.scaleMin, text.scaleMax, text.minLabel]).toEqual([null, null, null]);
  });

  it("só oferece como condição perguntas anteriores com valores fixos", () => {
    const sources = conditionSources(definition(), 0, 2).map((question) => question.id);
    expect(sources).toEqual(["nota", "canal"]);
    expect(conditionSources(definition(), 0, 0)).toEqual([]);
  });

  it("lista os valores possíveis da origem", () => {
    const [nota, canal] = definition().sections[0].questions;
    expect(conditionValues(nota).map((value) => value.id)).toEqual(["1", "2", "3", "4", "5"]);
    expect(conditionValues(canal).map((value) => value.id)).toEqual(["loja", "site"]);
  });

  it("remove a condição quando a origem passa a vir depois", () => {
    const current = definition();
    const moved: FormDefinition = { sections: [{ ...current.sections[0], questions: move(current.sections[0].questions, 2, 0) }] };

    expect(pruneConditions(moved).sections[0].questions[0].showIf).toBeNull();
    expect(pruneConditions(current).sections[0].questions[2].showIf?.anyOf).toEqual(["1", "2"]);
  });

  it("descarta valores da condição que a origem não tem mais", () => {
    const current = definition();
    current.sections[0].questions[0] = { ...current.sections[0].questions[0], scaleMax: 3 };
    current.sections[0].questions[2].showIf = { questionId: "nota", anyOf: ["2", "5"] };

    expect(pruneConditions(current).sections[0].questions[2].showIf?.anyOf).toEqual(["2"]);
  });

  it("monta o payload com textos aparados e opcionais nulos", () => {
    const current = definition();
    current.sections[0].title = "  Treinamento  ";
    current.sections[0].questions[1].helpText = "   ";

    const payload = toPayload(current);
    expect(payload.sections[0].title).toBe("Treinamento");
    expect(payload.sections[0].questions[1].helpText).toBeNull();
  });

  it("move itens sem sair dos limites", () => {
    expect(move([1, 2, 3], 0, 2)).toEqual([2, 3, 1]);
    expect(move([1, 2, 3], 0, -1)).toEqual([1, 2, 3]);
  });

  it("começa com as duas partes da avaliação e ids únicos", () => {
    const starter = starterDefinition();
    const ids = starter.sections.flatMap((section) => [section.id, ...section.questions.map((question) => question.id)]);

    expect(starter.sections.map((section) => section.topic)).toEqual(["Company", "Training"]);
    expect(new Set(ids).size).toBe(ids.length);
  });

  it("encontra erros da API pelo caminho da definição", () => {
    const errors = {
      "Definition.Sections[0].Questions[1].Options": ["Perguntas de escolha precisam de 2 a 30 opções."],
      "Definition.Sections": ["Adicione pelo menos uma pergunta."],
    };

    expect(definitionError(errors, "Sections[0].Questions[1].Options")).toMatch(/2 a 30/);
    expect(generalDefinitionErrors(errors)).toEqual(["Adicione pelo menos uma pergunta."]);
  });
});
