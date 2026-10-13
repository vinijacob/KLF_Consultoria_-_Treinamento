import { describe, expect, it } from "vitest";
import { phoneDigits, phoneHref } from "./format";

describe("telefones", () => {
  it("põe o 55 quando o número vem só com DDD", () => {
    expect(phoneDigits("(92) 98888-7777")).toBe("5592988887777");
    expect(phoneHref("(92) 3333-4444")).toBe("tel:+559233334444");
  });

  it("mantém o número que já tem o código do país", () => {
    expect(phoneHref("+55 (92) 98888-7777")).toBe("tel:+5592988887777");
  });
});
