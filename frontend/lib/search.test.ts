import { describe, expect, it } from "vitest";
import { groupByInitial, initialOf, matchesQuery, normalizeText } from "./search";

describe("search", () => {
  it("ignora acentos e maiúsculas", () => {
    expect(normalizeText("  Gestão de LOJA  ")).toBe("gestao de loja");
    expect(matchesQuery("gestao", "Gestão de loja")).toBe(true);
    expect(matchesQuery("ATENÇÃO", "Atendimento com atenção")).toBe(true);
  });

  it("exige todas as palavras, em qualquer campo e ordem", () => {
    expect(matchesQuery("vendas online", "Técnicas de vendas", "Formato: Online")).toBe(true);
    expect(matchesQuery("vendas presencial", "Técnicas de vendas", "Online")).toBe(false);
  });

  it("busca vazia encontra tudo e campos nulos são ignorados", () => {
    expect(matchesQuery("   ", "qualquer")).toBe(true);
    expect(matchesQuery("loja", null, undefined, "Loja Centro")).toBe(true);
  });

  it("agrupa por letra inicial em ordem alfabética, com números no fim", () => {
    const groups = groupByInitial(["Óptica Sol", "3M", "Atacadão", "Ótica Lua", "Bazar"], (name) => name);

    expect(groups.map(([letter]) => letter)).toEqual(["A", "B", "O", "#"]);
    expect(groups.find(([letter]) => letter === "O")?.[1]).toEqual(["Óptica Sol", "Ótica Lua"]);
    expect(initialOf("Ébano")).toBe("E");
  });
});
