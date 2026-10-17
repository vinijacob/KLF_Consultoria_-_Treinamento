import { describe, expect, it } from "vitest";
import { addTags, MAX_TAGS } from "./post-tags";

describe("addTags", () => {
  it("separa por vírgula, limpa espaços e ignora vazias", () => {
    expect(addTags([], "  Liderança ,  pós   venda,, ").tags).toEqual(["Liderança", "pós venda"]);
  });

  it("não repete a mesma tag com outro acento, maiúscula ou hífen", () => {
    expect(addTags(["Pós-venda"], "pos venda, POS-VENDA").tags).toEqual(["Pós-venda"]);
  });

  it("usa o nome da tag que já existe", () => {
    expect(addTags([], "fidelizacao", ["Fidelização", "Metas"]).tags).toEqual(["Fidelização"]);
  });

  it("para no limite e avisa", () => {
    const full = Array.from({ length: MAX_TAGS }, (_, n) => `Tag ${n}`);
    expect(addTags(full, "Outra")).toEqual({ tags: full, problem: `Use no máximo ${MAX_TAGS} tags por publicação.` });
  });

  it("recusa nome comprido demais", () => {
    expect(addTags([], "a".repeat(41)).problem).toContain("40 caracteres");
  });
});
