import { describe, expect, it } from "vitest";
import { matchesQuery, normalizeText, pickRandom } from "./search";

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


  it("sorteia sem repetir e respeita o limite", () => {
    const items = Array.from({ length: 30 }, (_, i) => i);
    const picked = pickRandom(items, 20);

    expect(picked).toHaveLength(20);
    expect(new Set(picked).size).toBe(20);
    expect(pickRandom([1, 2, 3], 20)).toHaveLength(3);
    expect(items).toHaveLength(30);
  });
});
