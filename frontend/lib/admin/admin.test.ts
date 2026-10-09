import { describe, expect, it } from "vitest";
import { addDays, datelineFor, formatDay, fromLocalInput, toLocalInput, todayLocal } from "./datetime";
import { isPostStatus, safeReturnPath } from "./params";
import { firstName, formatBytes, greetingFor, primaryRole, slugify } from "./text";

describe("datetime", () => {
  it("converte o horário de Manaus para ISO com fuso e volta", () => {
    expect(fromLocalInput("2026-10-08T14:30")).toBe("2026-10-08T14:30:00-04:00");
    expect(toLocalInput("2026-10-08T18:30:00Z")).toBe("2026-10-08T14:30");
  });

  it("devolve vazio/nulo para valores vazios", () => {
    expect(fromLocalInput("")).toBeNull();
    expect(toLocalInput(null)).toBe("");
  });

  it("usa o dia de Manaus, não o de UTC", () => {
    expect(todayLocal(new Date("2026-10-09T02:00:00Z"))).toBe("2026-10-08");
  });

  it("soma dias e formata sem passar por fuso", () => {
    expect(addDays("2026-02-28", 1)).toBe("2026-03-01");
    expect(addDays("2026-01-01", -1)).toBe("2025-12-31");
    expect(formatDay("2024-03-01")).toBe("01/03/2024");
  });

  it("escreve a linha de data como em jornal", () => {
    expect(datelineFor(new Date("2026-10-08T15:00:00Z"))).toBe("Manaus, quinta-feira, 8 de outubro de 2026");
  });
});

describe("text", () => {
  it("gera slug aceito pela API", () => {
    expect(slugify("  Atendimento que Gera Valor!  ")).toBe("atendimento-que-gera-valor");
    expect(slugify("Gestão & Liderança — Módulo 2")).toBe("gestao-lideranca-modulo-2");
    expect(slugify("---")).toBe("");
  });

  it("escolhe o perfil principal e recusa quem não tem perfil do painel", () => {
    expect(primaryRole(["Editor", "Admin"])).toBe("Admin");
    expect(primaryRole(["Editor"])).toBe("Editor");
    expect(primaryRole(["Instructor"])).toBeNull();
  });

  it("formata nome, saudação e tamanho", () => {
    expect(firstName("Kilciene Lima Ferreira")).toBe("Kilciene");
    expect(greetingFor(9)).toBe("Bom dia");
    expect(greetingFor(14)).toBe("Boa tarde");
    expect(greetingFor(20)).toBe("Boa noite");
    expect(formatBytes(1536)).toBe("2 KB");
    expect(formatBytes(2.5 * 1024 * 1024)).toBe("2,5 MB");
  });
});

describe("params", () => {
  it("só aceita voltar para dentro do painel", () => {
    expect(safeReturnPath("/painel/servicos")).toBe("/painel/servicos");
    expect(safeReturnPath("https://outro.site")).toBe("/painel");
    expect(safeReturnPath("//outro.site/painel")).toBe("/painel");
    expect(safeReturnPath("/painel\\@outro.site")).toBe("/painel");
    expect(safeReturnPath(["/painel"])).toBe("/painel");
  });

  it("reconhece só os status de publicação da API", () => {
    expect(isPostStatus("Draft")).toBe(true);
    expect(isPostStatus("draft")).toBe(false);
    expect(isPostStatus(undefined)).toBe(false);
  });
});
