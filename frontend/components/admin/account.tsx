"use client";

import { useState, type FormEvent } from "react";
import { roleInfo } from "@/config/admin";
import { useSubmit } from "@/lib/admin/hooks";
import { adminFetch, authRequest } from "@/lib/auth/session";
import { fieldError } from "@/lib/api/client";
import type { RecoveryCodesResponse } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/field";
import { CheckIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { RecoveryCodes } from "./auth-forms";
import { useAuth } from "./auth-provider";
import { RoleStamp } from "./shell";
import { AdminHeader, Section } from "./ui";

function NewRecoveryCodes() {
  const [code, setCode] = useState("");
  const [codes, setCodes] = useState<string[] | null>(null);
  const { pending, errors, message, run } = useSubmit();

  async function submit(event: FormEvent) {
    event.preventDefault();
    const response = await run(() =>
      adminFetch<RecoveryCodesResponse>("/auth/2fa/recovery-codes", { method: "POST", json: { code: code.trim() } }),
    );
    if (response) {
      setCodes(response.recoveryCodes);
      setCode("");
    }
  }

  if (codes) return <RecoveryCodes codes={codes} doneLabel="Pronto" onDone={() => setCodes(null)} />;

  return (
    <form onSubmit={submit} noValidate className="grid max-w-sm gap-5">
      <p className="font-sans text-[0.9375rem] leading-relaxed">
        Gere códigos novos se você usou quase todos ou acha que alguém viu os antigos. Os anteriores param de funcionar.
      </p>
      <Input
        label="Código atual do aplicativo"
        inputMode="numeric"
        autoComplete="one-time-code"
        maxLength={6}
        value={code}
        onChange={(event) => setCode(event.target.value.replace(/\D/g, ""))}
        error={fieldError(errors, "code")}
        className="font-mono text-xl tracking-[0.3em]"
      />
      {message && !errors && (
        <Notice tone="error" role="alert">
          {message}
        </Notice>
      )}
      <div>
        <Button type="submit" disabled={pending || code.length !== 6}>
          {pending ? "Gerando…" : "Gerar novos códigos"}
        </Button>
      </div>
    </form>
  );
}

function PasswordLink({ email }: { email: string }) {
  const [sent, setSent] = useState(false);
  const { pending, message, run } = useSubmit();

  return (
    <div className="grid gap-4">
      <p className="font-sans text-[0.9375rem] leading-relaxed">
        Para trocar a senha, enviamos um link para <strong className="font-medium">{email}</strong>. Ao trocar, as sessões
        abertas em outros aparelhos são encerradas.
      </p>
      {message && <Notice tone="error">{message}</Notice>}
      {sent ? (
        <p role="status" className="flex items-center gap-2 font-sans text-sm text-success">
          <CheckIcon /> Link enviado. Ele vale por 30 minutos.
        </p>
      ) : (
        <div>
          <Button
            variant="outline"
            disabled={pending}
            onClick={() =>
              void run(async () => {
                await authRequest("/forgot", { email });
                setSent(true);
              })
            }
          >
            {pending ? "Enviando…" : "Enviar link para trocar a senha"}
          </Button>
        </div>
      )}
    </div>
  );
}

export function AccountPage() {
  const { user, role, logout } = useAuth();

  return (
    <div>
      <AdminHeader kicker="Conta" title="Segurança" description="Sua conta, sua verificação em duas etapas e sua senha." />

      <Section title="Sua conta">
        <div className="flex flex-wrap items-center gap-4">
          <RoleStamp role={role} />
          <p className="font-sans text-[0.9375rem] text-ink-soft">{roleInfo[role].summary}</p>
        </div>
        <dl className="grid grid-cols-[auto_1fr] gap-x-6 gap-y-2 font-sans text-[0.9375rem]">
          <dt className="text-ink-soft">Nome</dt>
          <dd>{user.fullName}</dd>
          <dt className="text-ink-soft">E-mail</dt>
          <dd>{user.email}</dd>
        </dl>
        <p className="font-sans text-sm text-ink-soft">Para mudar nome, e-mail ou perfil, fale com a administração.</p>
      </Section>

      <Section title="Verificação em duas etapas" description="O código do aplicativo protege a conta mesmo se alguém descobrir a senha.">
        {user.twoFactorEnabled ? (
          <>
            <p className="flex items-center gap-2 font-sans text-[0.9375rem] font-medium text-success">
              <CheckIcon /> Ativada
            </p>
            <NewRecoveryCodes />
          </>
        ) : (
          <Notice tone="wait" title="Ainda não ativada">
            O cadastro do aplicativo é pedido ao entrar no painel. Em produção ele é obrigatório; neste ambiente de
            desenvolvimento está desligado.
          </Notice>
        )}
      </Section>

      <Section title="Senha">
        <PasswordLink email={user.email} />
      </Section>

      <Section title="Sair" description="Encerra a sessão neste aparelho.">
        <div>
          <Button variant="outline" onClick={() => void logout()}>
            Sair do painel
          </Button>
        </div>
      </Section>
    </div>
  );
}
