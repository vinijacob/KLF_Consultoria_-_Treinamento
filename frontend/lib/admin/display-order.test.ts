import { describe, expect, it } from "vitest";
import { planOrder } from "./display-order";

const list = [
  { id: "a", displayOrder: 0 },
  { id: "b", displayOrder: 1 },
  { id: "c", displayOrder: 2 },
];

describe("planOrder", () => {
  it("item novo em branco entra no fim, sem troca", () => {
    expect(planOrder(list, undefined, "")).toEqual({ from: 3, to: 3, last: 3, occupant: undefined });
  });

  it("item novo numa posição ocupada troca com quem está lá", () => {
    const plan = planOrder(list, undefined, "1");
    expect(plan.occupant?.id).toBe("b");
    expect(plan.from).toBe(3);
  });

  it("edição sem mudar a posição não troca nada", () => {
    expect(planOrder(list, "c", "2").occupant).toBeUndefined();
    expect(planOrder(list, "c", "").to).toBe(2);
  });

  it("edição para uma posição ocupada troca os dois", () => {
    const plan = planOrder(list, "c", "0");
    expect(plan).toMatchObject({ from: 2, to: 0, last: 2 });
    expect(plan.occupant?.id).toBe("a");
  });

  it("posição fora da lista não tem ocupante (a API recusa)", () => {
    expect(planOrder(list, "a", "9").occupant).toBeUndefined();
  });

  it("conta pelo lugar na lista, mesmo com números antigos repetidos", () => {
    const messy = [
      { id: "a", displayOrder: 0 },
      { id: "b", displayOrder: 0 },
      { id: "c", displayOrder: 7 },
    ];
    expect(planOrder(messy, "c", "1").occupant?.id).toBe("b");
  });
});
