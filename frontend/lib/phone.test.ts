import { describe, expect, it } from "vitest";
import { formatPhone, nextPhoneValue, phoneFieldValue } from "./phone";

function type(text: string, start = "") {
  return [...text].reduce((value, char) => nextPhoneValue(value, value + char), start);
}

describe("máscara de telefone", () => {
  it("formata celular e fixo", () => {
    expect(formatPhone("5592988887777")).toBe("+55 (92) 98888-7777");
    expect(formatPhone("559233334444")).toBe("+55 (92) 3333-4444");
  });

  it("vai formatando enquanto a pessoa digita só o número, com o 55 automático", () => {
    expect(type("9")).toBe("+55 (9");
    expect(type("92")).toBe("+55 (92)");
    expect(type("929888")).toBe("+55 (92) 9888");
    expect(type("92988887777")).toBe("+55 (92) 98888-7777");
  });

  it("respeita o código do país quando a pessoa começa com +", () => {
    expect(type("+5592988887777")).toBe("+55 (92) 98888-7777");
  });

  it("não passa de 13 dígitos", () => {
    expect(type("929888877771234")).toBe("+55 (92) 98888-7777");
  });

  it("apagar a pontuação apaga o dígito anterior", () => {
    expect(nextPhoneValue("+55 (92) 98888-7777", "+55 (92) 98888-777")).toBe("+55 (92) 9888-8777");
    expect(nextPhoneValue("+55 (92)", "+55 (92")).toBe("+55 (9");
    expect(nextPhoneValue("+55 (9", "+55 (")).toBe("+55");
  });

  it("colar um número com ou sem 55 dá o mesmo resultado", () => {
    expect(nextPhoneValue("", "(92) 98888-7777")).toBe("+55 (92) 98888-7777");
    expect(nextPhoneValue("", "5592988887777")).toBe("+55 (92) 98888-7777");
    expect(nextPhoneValue("+55 (11) 1111-1111", "(92) 98888-7777")).toBe("+55 (92) 98888-7777");
  });

  it("mostra no campo o que estava salvo, com ou sem pontuação", () => {
    expect(phoneFieldValue("5592981289495")).toBe("+55 (92) 98128-9495");
    expect(phoneFieldValue("+55 (92) 3333-4444")).toBe("+55 (92) 3333-4444");
    expect(phoneFieldValue(null)).toBe("");
  });
});
