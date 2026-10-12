import { describe, expect, it } from "vitest";
import { CONSENT_VERSION, makeChoice, needsDecision, parseConsent, readCookie, serializeConsent, type ConsentCategoryInfo } from "./consent";

const inUse: ConsentCategoryInfo[] = [{ id: "thirdParty", label: "Terceiros", description: "", inUse: true }];
const notInUse: ConsentCategoryInfo[] = [{ id: "thirdParty", label: "Terceiros", description: "", inUse: false }];

describe("consentimento", () => {
  it("lê de volta a escolha gravada", () => {
    const choice = makeChoice({ thirdParty: true }, "2026-10-12");
    expect(parseConsent(serializeConsent(choice))).toEqual({ version: CONSENT_VERSION, decidedOn: "2026-10-12", thirdParty: true });
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

  it("só pede decisão quando há categoria opcional em uso e nada foi escolhido", () => {
    expect(needsDecision(null, inUse)).toBe(true);
    expect(needsDecision(null, notInUse)).toBe(false);
    expect(needsDecision(makeChoice({}, "2026-10-12"), inUse)).toBe(false);
  });

  it("acha o cookie certo entre vários", () => {
    expect(readCookie("a=1; klf_consent=abc; b=2", "klf_consent")).toBe("abc");
    expect(readCookie("klf_consent_old=x", "klf_consent")).toBeUndefined();
  });
});
