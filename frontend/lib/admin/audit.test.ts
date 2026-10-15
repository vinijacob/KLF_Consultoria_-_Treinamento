import { describe, expect, it } from "vitest";
import { describeAudit, type AuditEntry } from "./audit";

const entry = (action: string, extra: Partial<AuditEntry> = {}): AuditEntry => ({
  id: "1",
  occurredAt: "2026-10-14T15:00:00Z",
  actorName: "Kilciene",
  action,
  targetId: "abc",
  ...extra,
});

describe("registro de atividades", () => {
  it("monta a frase de conteúdo com o rótulo entre aspas e o link", () => {
    expect(describeAudit(entry("service.created", { targetLabel: "Vendas consultivas" }))).toEqual({
      actor: "Kilciene",
      text: "criou o serviço “Vendas consultivas”",
      href: "/painel/servicos/abc",
    });
    expect(describeAudit(entry("post.published", { targetLabel: "Novidade" })).text).toBe("publicou “Novidade”");
  });

  it("não oferece link para o que foi excluído", () => {
    expect(describeAudit(entry("client.deleted", { targetLabel: "Loja" })).href).toBeNull();
  });

  it("descreve depoimentos e mensagens sem nome de pessoa", () => {
    expect(describeAudit(entry("testimonial.consent-revoked")).text).toBe("revogou o consentimento de um depoimento");
    expect(describeAudit(entry("contact.received", { actorName: null })).actor).toBe("Visitante do site");
  });

  it("descreve ações da equipe, de acesso e do sistema", () => {
    expect(describeAudit(entry("user.deactivated", { targetLabel: "Bruno" })).text).toBe("desativou a conta de Bruno");
    expect(describeAudit(entry("login.succeeded")).text).toBe("entrou no painel");
    expect(describeAudit(entry("contact.purged", { actorName: null, targetLabel: "3 mensagens", targetId: null }))).toEqual({
      actor: "Sistema",
      text: "excluiu 3 mensagens com mais de 2 anos (limpeza automática)",
      href: null,
    });
  });

  it("traduz a chave dos dados do site", () => {
    expect(describeAudit(entry("settings.updated", { targetLabel: "legal" })).text).toBe("editou os dados do site: Dados legais");
  });
});
