import { describe, expect, it } from "vitest";
import { deadlineState, needsDetails } from "./data-requests";

describe("deadlineState", () => {
  it("avisa quando o prazo está perto, vence hoje ou já venceu", () => {
    expect(deadlineState("2026-10-30", "2026-10-15")).toEqual({ text: "prazo em 15 dias", tone: "neutral" });
    expect(deadlineState("2026-10-16", "2026-10-15")).toEqual({ text: "prazo vence em 1 dia", tone: "warning" });
    expect(deadlineState("2026-10-15", "2026-10-15")).toEqual({ text: "prazo vence hoje", tone: "danger" });
    expect(deadlineState("2026-10-13", "2026-10-15")).toEqual({ text: "prazo vencido há 2 dias", tone: "danger" });
  });

  it("atravessa o fim do mês", () => {
    expect(deadlineState("2026-11-02", "2026-10-31").text).toBe("prazo vence em 2 dias");
  });
});

describe("needsDetails", () => {
  it("pede explicação só para correção e outros pedidos", () => {
    expect(needsDetails("Correction")).toBe(true);
    expect(needsDetails("Other")).toBe(true);
    expect(needsDetails("Deletion")).toBe(false);
  });
});
