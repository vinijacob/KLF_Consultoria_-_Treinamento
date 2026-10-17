import { describe, expect, it } from "vitest";
import { CONSENT_VERSION, makeChoice, needsDecision, parseConsent, readCookie, serializeConsent, type ConsentCategoryInfo } from "./consent";

const inUse: ConsentCategoryInfo[] = [{ id: "thirdParty", label: "Terceiros", description: "", inUse: true }];
const notInUse: ConsentCategoryInfo[] = [{ id: "thirdParty", label: "Terceiros", description: "", inUse: false }];

describe("consentimento", () => {
  it("lê de volta a escolha gravada", () => {
    const choice = makeChoice({ thirdParty: true }, "2026-10-12", ["thirdParty"]);
    expect(parseConsent(serializeConsent(choice))).toEqual({ version: CONSENT_VERSION, decidedOn: "2026-10-12", asked: ["thirdParty"], thirdParty: true });
  });

  it("marca como recusada a categoria que não foi aceita", () => {
    expect(makeChoice({}, "2026-10-12").thirdParty).toBe(false);
  });

  it("descarta escolha de outra versão, corrompida ou sem alguma categoria", () => {
    expect(parseConsent(encodeURIComponent(JSON.stringify({ version: 0, decidedOn: "2026-01-01", thirdParty: true })))).toBeNull();
    expect(parseConsent("%7Bquebrado")).toBeNull();
    expect(parseConsent(encodeURIComponent(JSON.stringify({ version: CONSENT_VERSION, decidedOn: "2026-01-01" })))).toBeNull();
    expect(parseConsent(undefined)).toBeNull();
  });

  it("mostra o banner na primeira visita, mesmo sem categoria opcional em uso", () => {
    expect(needsDecision(null, inUse)).toBe(true);
    expect(needsDecision(null, notInUse)).toBe(true);
  });

  it("não pergunta de novo o que já foi perguntado", () => {
    expect(needsDecision(makeChoice({}, "2026-10-12", []), notInUse)).toBe(false);
    expect(needsDecision(makeChoice({}, "2026-10-12", ["thirdParty"]), inUse)).toBe(false);
  });

  it("pergunta de novo quando entra em uso uma categoria que a pessoa não viu", () => {
    const sawOnlyTheNotice = makeChoice({}, "2026-10-12", []);
    expect(needsDecision(sawOnlyTheNotice, inUse)).toBe(true);
    const oldCookie = parseConsent(encodeURIComponent(JSON.stringify({ version: CONSENT_VERSION, decidedOn: "2026-10-01", thirdParty: false })));
    expect(needsDecision(oldCookie, inUse)).toBe(true);
  });

  it("acha o cookie certo entre vários", () => {
    expect(readCookie("a=1; klf_consent=abc; b=2", "klf_consent")).toBe("abc");
    expect(readCookie("klf_consent_old=x", "klf_consent")).toBeUndefined();
  });
});
