import { describe, expect, it } from "vitest";
import { LEGAL_TEXT_REVISED_ON, legalUpdatedOn, missingLegalData } from "./legal";
import { formatDay } from "./format";

describe("missingLegalData", () => {
  it("lista tudo quando nada foi preenchido", () => {
    expect(missingLegalData(undefined)).toHaveLength(6);
  });

  it("ignora campos só com espaços como se estivessem vazios", () => {
    const missing = missingLegalData({ companyName: "KLF Ltda", cnpj: "  ", address: "Rua A", forumCity: "Manaus/AM", dpoName: "Kil", dpoEmail: "dados@klf.com.br" });
    expect(missing).toEqual(["CNPJ"]);
  });
});

describe("legalUpdatedOn", () => {
  it("usa a revisão do texto quando os dados são mais antigos ou não existem", () => {
    expect(legalUpdatedOn(undefined)).toBe(LEGAL_TEXT_REVISED_ON);
    expect(legalUpdatedOn({ updatedOn: "2026-01-01" })).toBe(LEGAL_TEXT_REVISED_ON);
  });

  it("usa a data dos dados quando foram salvos depois da revisão do texto", () => {
    expect(legalUpdatedOn({ updatedOn: "2027-02-03" })).toBe("2027-02-03");
  });
});

describe("formatDay", () => {
  it("não volta um dia por causa do fuso", () => {
    expect(formatDay("2026-10-12")).toBe("12 de outubro de 2026");
  });
});
