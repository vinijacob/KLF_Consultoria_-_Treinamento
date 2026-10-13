import { describe, expect, it } from "vitest";
import { formatCnpj, nextCnpjValue } from "./cnpj";

const type = (text: string) => [...text].reduce((value, char) => nextCnpjValue(value, value + char), "");

describe("máscara de CNPJ", () => {
  it("formata o CNPJ numérico e o alfanumérico", () => {
    expect(formatCnpj("11222333000181")).toBe("11.222.333/0001-81");
    expect(formatCnpj("12abc34501de35")).toBe("12.ABC.345/01DE-35");
  });

  it("vai pontuando enquanto a pessoa digita", () => {
    expect(type("112")).toBe("11.2");
    expect(type("11222333")).toBe("11.222.333");
    expect(type("112223330001")).toBe("11.222.333/0001");
    expect(type("11222333000181")).toBe("11.222.333/0001-81");
  });

  it("não aceita letra nos dígitos verificadores nem passa de 14 caracteres", () => {
    expect(type("12ABC34501DEX35")).toBe("12.ABC.345/01DE-35");
    expect(type("1122233300018199")).toBe("11.222.333/0001-81");
  });

  it("apagar a pontuação apaga o caractere anterior", () => {
    expect(nextCnpjValue("11.222.333/0001-81", "11.222.333/0001-8")).toBe("11.222.333/0001-8");
    expect(nextCnpjValue("11.222.333/0001-8", "11.222.333/0001-")).toBe("11.222.333/0001");
    expect(nextCnpjValue("11.222", "11.22")).toBe("11.22");
    expect(nextCnpjValue("11.2", "11.")).toBe("11");
  });
});
