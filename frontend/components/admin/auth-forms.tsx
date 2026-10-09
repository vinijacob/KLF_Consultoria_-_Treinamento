"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useState, type FormEvent, type ReactNode } from "react";
import { useSubmit } from "@/lib/admin/hooks";
import { ApiError, fieldError } from "@/lib/api/client";
import { authRequest, refreshSession, startSession } from "@/lib/auth/session";
import type { LoginResponse, TwoFactorEnabledResponse, TwoFactorSetupResponse } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Checkbox, Input } from "@/components/ui/field";
import { DownloadIcon, KeyIcon, LockIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { Kicker } from "@/components/ui/typography";
import { QrCode } from "./qr-code";

function StepHeading({ step, title, children }: { step: string; title: string; children?: ReactNode }) {
  return (
    <div className="mb-8">
      <Kicker>{step}</Kicker>
      <h1 className="mt-2 text-4xl leading-tight sm:text-[2.75rem]">{title}</h1>
      {children && <div className="mt-3 font-sans text-[0.9375rem] leading-relaxed text-ink-soft">{children}</div>}
    </div>
  );
}

function ErrorMessage({ message }: { message: string | null }) {
  if (!message) return null;
  return (
    <Notice tone="error" role="alert">
      {message}
    </Notice>
  );
}

type Step =
  | { name: "password" }
  | { name: "code"; token: string }
  | { name: "recovery"; token: string }
  | { name: "setup"; token: string; setup: TwoFactorSetupResponse | null }
  | { name: "codes"; codes: string[] };

/** Entrada no painel: senha → código do aplicativo (ou cadastro do aplicativo no primeiro acesso) → painel. */
export function LoginFlow({ returnTo, signedOut }: { returnTo: string; signedOut: boolean }) {
  const router = useRouter();
  const [step, setStep] = useState<Step>({ name: "password" });
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [code, setCode] = useState("");
  const [notice, setNotice] = useState<string | null>(signedOut ? "Você saiu do painel." : null);
  const { pending, errors, message, run, setMessage } = useSubmit();

  useEffect(() => {
    if (signedOut) return;
    let active = true;
    void refreshSession().then((session) => {
      if (active && session) router.replace(returnTo);
    });
    return () => {
      active = false;
    };
  }, [router, returnTo, signedOut]);

  function enter(accessToken: string, expiresAt: string) {
    startSession(accessToken, expiresAt);
    router.replace(returnTo);
  }

  function restart(text: string) {
    setStep({ name: "password" });
    setCode("");
    setPassword("");
    setMessage(text);
  }

  /** Etapas com o `twoFactorToken` (5 min): se ele venceu, volta para a senha. */
  async function secondStep<T>(path: string, body: unknown) {
    let expired = false;
    const result = await run(() =>
      authRequest<T>(path, body).catch((error: unknown) => {
        expired = error instanceof ApiError && error.status === 401 && error.message.includes("expirou");
        throw error;
      }),
    );
    if (expired) restart("A verificação expirou. Entre novamente com e-mail e senha.");
    return result;
  }

  async function submitPassword(event: FormEvent) {
    event.preventDefault();
    setNotice(null);
    const response = await run(() => authRequest<LoginResponse>("/login", { email, password }));
    if (!response) return;

    if (response.accessToken && response.expiresAt) return enter(response.accessToken, response.expiresAt);
    if (response.twoFactorToken && response.twoFactorSetupRequired) {
      setStep({ name: "setup", token: response.twoFactorToken, setup: null });
      const setup = await secondStep<TwoFactorSetupResponse>("/2fa/setup", { twoFactorToken: response.twoFactorToken });
      if (setup) setStep({ name: "setup", token: response.twoFactorToken, setup });
      return;
    }
    if (response.twoFactorToken) setStep({ name: "code", token: response.twoFactorToken });
  }

  async function submitCode(event: FormEvent) {
    event.preventDefault();
    if (step.name !== "code" && step.name !== "recovery") return;

    const path = step.name === "code" ? "/2fa/verify" : "/2fa/recover";
    const body =
      step.name === "code" ? { twoFactorToken: step.token, code: code.trim() } : { twoFactorToken: step.token, recoveryCode: code.trim() };
    const response = await secondStep<LoginResponse>(path, body);

    if (response?.accessToken && response.expiresAt) enter(response.accessToken, response.expiresAt);
  }

  async function submitEnable(event: FormEvent) {
    event.preventDefault();
    if (step.name !== "setup") return;

    const response = await secondStep<TwoFactorEnabledResponse>("/2fa/enable", { twoFactorToken: step.token, code: code.trim() });
    if (!response) return;

    startSession(response.accessToken, response.expiresAt);
    setStep({ name: "codes", codes: response.recoveryCodes });
  }

  if (step.name === "codes") {
    return <RecoveryCodes codes={step.codes} onDone={() => router.replace(returnTo)} doneLabel="Entrar no painel" />;
  }

  if (step.name === "setup") {
    return (
      <form onSubmit={submitEnable} noValidate>
        <StepHeading step="Primeiro acesso · Segurança" title="Cadastre o aplicativo de verificação">
          Toda entrada no painel pede um código de 6 números gerado no seu celular. Use um aplicativo como Google
          Authenticator, Microsoft Authenticator ou o gerenciador de senhas que você já usa.
        </StepHeading>
        <ol className="grid gap-8">
          <li className="grid gap-4 sm:grid-cols-[auto_1fr] sm:gap-6">
            <span className="font-mono text-xs text-ink-soft">01</span>
            <div>
              <p className="font-sans text-[0.9375rem] font-medium">No aplicativo, toque em adicionar e aponte a câmera para o código.</p>
              <div className="mt-4 w-56 border border-ink p-2">
                {step.setup ? (
                  <QrCode value={step.setup.otpAuthUri} label="QR Code para cadastrar o painel KLF no aplicativo de verificação" />
                ) : (
                  <div className="grid aspect-square place-items-center">
                    <Kicker role="status">Gerando…</Kicker>
                  </div>
                )}
              </div>
              {step.setup && (
                <details className="mt-4 font-sans text-sm">
                  <summary className="min-h-11 cursor-pointer text-brand underline underline-offset-4">Não consigo ler o QR Code</summary>
                  <p className="mt-2 text-ink-soft">Digite esta chave no aplicativo, escolhendo “baseada em tempo”:</p>
                  <p className="mt-2 font-mono text-base tracking-[0.08em] break-all select-all">{step.setup.sharedKey}</p>
                </details>
              )}
            </div>
          </li>
          <li className="grid gap-4 sm:grid-cols-[auto_1fr] sm:gap-6">
            <span className="font-mono text-xs text-ink-soft">02</span>
            <div className="grid max-w-xs gap-5">
              <Input
                label="Código de 6 números que aparece no aplicativo"
                inputMode="numeric"
                autoComplete="one-time-code"
                pattern="[0-9]*"
                maxLength={6}
                value={code}
                onChange={(event) => setCode(event.target.value.replace(/\D/g, ""))}
                error={fieldError(errors, "code")}
                className="font-mono text-2xl tracking-[0.3em]"
              />
              <ErrorMessage message={errors ? null : message} />
              <Button type="submit" disabled={pending || !step.setup || code.length !== 6}>
                {pending ? "Conferindo…" : "Ativar e continuar"}
              </Button>
            </div>
          </li>
        </ol>
      </form>
    );
  }

  if (step.name === "code" || step.name === "recovery") {
    const recovery = step.name === "recovery";

    return (
      <form onSubmit={submitCode} noValidate className="grid gap-6">
        <StepHeading step="Etapa 2 de 2 · Verificação" title={recovery ? "Use um código de recuperação" : "Digite o código do aplicativo"}>
          {recovery
            ? "Use um dos códigos que você guardou ao ativar a verificação. Cada código funciona uma vez só."
            : "Abra o aplicativo de verificação no celular e digite os 6 números do painel KLF."}
        </StepHeading>
        <Input
          key={step.name}
          label={recovery ? "Código de recuperação" : "Código de 6 números"}
          inputMode={recovery ? "text" : "numeric"}
          autoComplete="one-time-code"
          autoFocus
          maxLength={recovery ? 30 : 6}
          value={code}
          onChange={(event) => setCode(recovery ? event.target.value.toUpperCase() : event.target.value.replace(/\D/g, ""))}
          error={fieldError(errors, recovery ? "recoveryCode" : "code")}
          className="font-mono text-2xl tracking-[0.3em]"
        />
        <ErrorMessage message={errors ? null : message} />
        <div className="flex flex-wrap items-center gap-x-6 gap-y-3">
          <Button type="submit" size="lg" disabled={pending || (!recovery && code.length !== 6) || (recovery && code.trim().length < 5)}>
            {pending ? "Conferindo…" : "Entrar"}
          </Button>
          <button
            type="button"
            className="min-h-11 font-sans text-sm text-brand underline underline-offset-4"
            onClick={() => {
              setCode("");
              setMessage(null);
              setStep(recovery ? { name: "code", token: step.token } : { name: "recovery", token: step.token });
            }}
          >
            {recovery ? "Voltar para o código do aplicativo" : "Perdi o celular: usar código de recuperação"}
          </button>
        </div>
      </form>
    );
  }

  return (
    <form onSubmit={submitPassword} noValidate className="grid gap-6">
      <StepHeading step="Acesso restrito · Equipe KLF" title="Entrar no painel">
        Só quem trabalha com a KLF tem conta. Se precisar de acesso, fale com a administração.
      </StepHeading>
      {notice && (
        <Notice tone="success" role="status">
          {notice}
        </Notice>
      )}
      <Input
        label="E-mail"
        type="email"
        autoComplete="username"
        required
        value={email}
        onChange={(event) => setEmail(event.target.value)}
        error={fieldError(errors, "email")}
      />
      <Input
        label="Senha"
        type="password"
        autoComplete="current-password"
        required
        value={password}
        onChange={(event) => setPassword(event.target.value)}
        error={fieldError(errors, "password")}
      />
      <ErrorMessage message={errors ? null : message} />
      <div className="flex flex-wrap items-center gap-x-6 gap-y-3">
        <Button type="submit" size="lg" disabled={pending}>
          {pending ? "Entrando…" : "Entrar"}
        </Button>
        <Link href="/painel/esqueci-a-senha" className="min-h-11 py-3 font-sans text-sm text-brand underline underline-offset-4">
          Esqueci minha senha
        </Link>
      </div>
    </form>
  );
}

/** Códigos de recuperação: mostrados uma vez só, com opção de baixar e confirmação de que foram guardados. */
export function RecoveryCodes({ codes, onDone, doneLabel }: { codes: string[]; onDone: () => void; doneLabel: string }) {
  const [saved, setSaved] = useState(false);
  const [copied, setCopied] = useState(false);
  const text = `Códigos de recuperação do painel KLF\nCada código funciona uma vez. Guarde em lugar seguro.\n\n${codes.join("\n")}\n`;

  function download() {
    const url = URL.createObjectURL(new Blob([text], { type: "text/plain;charset=utf-8" }));
    const link = document.createElement("a");
    link.href = url;
    link.download = "klf-codigos-de-recuperacao.txt";
    link.click();
    URL.revokeObjectURL(url);
  }

  return (
    <div>
      <StepHeading step="Guarde agora · Aparece uma vez só" title="Seus códigos de recuperação">
        Se você perder o celular, cada código abaixo permite entrar uma vez sem o aplicativo. Guarde num gerenciador de
        senhas ou imprima e deixe num lugar seguro. Eles não aparecem de novo.
      </StepHeading>
      <ol className="grid grid-cols-2 gap-x-6 border-y-2 border-ink py-5 font-mono text-lg tracking-[0.08em] sm:grid-cols-2">
        {codes.map((value, index) => (
          <li key={value} className="flex items-baseline gap-3 py-1.5">
            <span className="w-5 text-xs text-ink-soft">{String(index + 1).padStart(2, "0")}</span>
            <span className="select-all">{value}</span>
          </li>
        ))}
      </ol>
      <div className="mt-5 flex flex-wrap gap-3">
        <Button variant="outline" onClick={download}>
          <DownloadIcon /> Baixar em .txt
        </Button>
        <Button
          variant="outline"
          onClick={() => {
            void navigator.clipboard.writeText(text).then(() => setCopied(true));
          }}
        >
          {copied ? "Copiados" : "Copiar"}
        </Button>
      </div>
      <Checkbox
        className="mt-8"
        label="Guardei os códigos em lugar seguro"
        checked={saved}
        onChange={(event) => setSaved(event.target.checked)}
      />
      <Button size="lg" className="mt-5" disabled={!saved} onClick={onDone}>
        {doneLabel}
      </Button>
    </div>
  );
}

export function ForgotPasswordForm() {
  const [email, setEmail] = useState("");
  const [sent, setSent] = useState(false);
  const { pending, errors, message, run } = useSubmit();

  async function submit(event: FormEvent) {
    event.preventDefault();
    const done = await run(async () => {
      await authRequest("/forgot", { email });
      return true;
    });
    if (done) setSent(true);
  }

  if (sent) {
    return (
      <div>
        <StepHeading step="Recuperar acesso" title="Confira seu e-mail">
          Se <strong className="font-medium text-ink">{email}</strong> tiver conta no painel, enviamos um link para criar
          uma nova senha. O link vale por 30 minutos e funciona uma vez. Veja também a caixa de spam.
        </StepHeading>
        <Link href="/painel/entrar" className="font-sans text-sm text-brand underline underline-offset-4">
          Voltar para a entrada
        </Link>
      </div>
    );
  }

  return (
    <form onSubmit={submit} noValidate className="grid gap-6">
      <StepHeading step="Recuperar acesso" title="Esqueci minha senha">
        Informe o e-mail da sua conta. Vamos enviar um link para você criar uma senha nova.
      </StepHeading>
      <Input
        label="E-mail"
        type="email"
        autoComplete="username"
        value={email}
        onChange={(event) => setEmail(event.target.value)}
        error={fieldError(errors, "email")}
      />
      <ErrorMessage message={errors ? null : message} />
      <div className="flex flex-wrap items-center gap-x-6 gap-y-3">
        <Button type="submit" size="lg" disabled={pending}>
          {pending ? "Enviando…" : "Enviar link"}
        </Button>
        <Link href="/painel/entrar" className="min-h-11 py-3 font-sans text-sm text-brand underline underline-offset-4">
          Voltar para a entrada
        </Link>
      </div>
    </form>
  );
}

const passwordRules = [
  { test: (value: string) => value.length >= 10, label: "Pelo menos 10 caracteres" },
  { test: (value: string) => /[a-z]/.test(value) && /[A-Z]/.test(value), label: "Letras maiúsculas e minúsculas" },
  { test: (value: string) => /\d/.test(value), label: "Pelo menos um número" },
  { test: (value: string) => /[^a-zA-Z0-9]/.test(value), label: "Pelo menos um símbolo (ex.: ! @ # %)" },
];

export function ResetPasswordForm({ email, token }: { email: string; token: string }) {
  const [password, setPassword] = useState("");
  const [confirmation, setConfirmation] = useState("");
  const [mismatch, setMismatch] = useState(false);
  const [done, setDone] = useState(false);
  const { pending, errors, message, run } = useSubmit();

  if (!email || !token) {
    return (
      <div>
        <StepHeading step="Nova senha" title="Link incompleto">
          Abra o link exatamente como chegou no e-mail, ou peça um novo.
        </StepHeading>
        <Link href="/painel/esqueci-a-senha" className="font-sans text-sm text-brand underline underline-offset-4">
          Pedir um novo link
        </Link>
      </div>
    );
  }

  if (done) {
    return (
      <div>
        <StepHeading step="Nova senha" title="Senha trocada">
          Por segurança, encerramos as sessões abertas em outros aparelhos. Entre de novo com a senha nova; o código do
          aplicativo de verificação continua sendo pedido.
        </StepHeading>
        <Link href="/painel/entrar" className="font-sans text-sm text-brand underline underline-offset-4">
          Ir para a entrada
        </Link>
      </div>
    );
  }

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (password !== confirmation) {
      setMismatch(true);
      return;
    }
    setMismatch(false);
    const ok = await run(async () => {
      await authRequest("/reset", { email, token, newPassword: password });
      return true;
    });
    if (ok) setDone(true);
  }

  const tokenProblem = fieldError(errors, "token");

  return (
    <form onSubmit={submit} noValidate className="grid gap-6">
      <StepHeading step="Nova senha" title="Crie uma senha nova">
        Conta: <span className="font-medium text-ink">{email}</span>
      </StepHeading>
      {tokenProblem && (
        <Notice tone="error" role="alert" title={tokenProblem}>
          <Link href="/painel/esqueci-a-senha" className="text-brand underline underline-offset-4">
            Pedir um novo link
          </Link>
        </Notice>
      )}
      <Input
        label="Nova senha"
        type="password"
        autoComplete="new-password"
        value={password}
        onChange={(event) => setPassword(event.target.value)}
        error={fieldError(errors, "newPassword")}
      />
      <ul className="grid gap-1.5 font-sans text-sm" aria-label="Requisitos da senha">
        {passwordRules.map((rule) => {
          const ok = rule.test(password);
          return (
            <li key={rule.label} className={ok ? "text-success" : "text-ink-soft"}>
              <span className="mr-2 font-mono">{ok ? "[ok]" : "[  ]"}</span>
              {rule.label}
            </li>
          );
        })}
      </ul>
      <Input
        label="Repita a nova senha"
        type="password"
        autoComplete="new-password"
        value={confirmation}
        onChange={(event) => setConfirmation(event.target.value)}
        error={mismatch ? "As duas senhas não são iguais." : undefined}
      />
      <ErrorMessage message={errors ? null : message} />
      <div>
        <Button type="submit" size="lg" disabled={pending || !passwordRules.every((rule) => rule.test(password))}>
          {pending ? "Salvando…" : "Salvar nova senha"}
        </Button>
      </div>
    </form>
  );
}

export function AccessFrame({ children }: { children: ReactNode }) {
  return (
    <div className="mx-auto flex w-full max-w-xl flex-1 flex-col px-5 pb-16 sm:px-8">
      <header className="pt-8">
        <div className="flex items-baseline justify-between gap-4">
          <p className="flex items-baseline gap-3">
            <span className="font-serif text-[1.75rem] leading-none font-semibold tracking-[-0.02em]">KLF</span>
            <span className="font-mono text-[0.6875rem] uppercase tracking-[0.16em] text-ink-soft">Painel da redação</span>
          </p>
          <Kicker className="flex items-center gap-1.5">
            <LockIcon /> Área restrita
          </Kicker>
        </div>
        <hr className="mt-3 border-0 border-t-[5px] border-double border-ink" />
      </header>
      <main className="flex-1 pt-12 sm:pt-16">{children}</main>
      <footer className="mt-16 flex items-center gap-2 border-t border-rule pt-4">
        <KeyIcon className="text-ink-soft" />
        <Kicker>Entrada protegida por verificação em duas etapas</Kicker>
      </footer>
    </div>
  );
}
