import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { Button } from "@/components/ui/button";
import { controlClasses, FieldError } from "@/components/ui/field";
import { AlertIcon, ArrowRightIcon, CheckIcon, ClockIcon, LockIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { Kicker, Rule } from "@/components/ui/typography";
import { ChoiceDemo, ScaleDemo } from "./demos";

export const metadata: Metadata = { title: "Guia visual", robots: { index: false, follow: false } };

const swatches = [
  ["paper", "bg-paper", "Fundo da página"],
  ["paper-deep", "bg-paper-deep", "Seções alternadas"],
  ["card", "bg-card", "Campos e cartões"],
  ["rule", "bg-rule", "Fios decorativos"],
  ["rule-strong", "bg-rule-strong", "Bordas de campos"],
  ["ink", "bg-ink", "Texto"],
  ["ink-soft", "bg-ink-soft", "Texto secundário"],
  ["brand", "bg-brand", "Cor da marca"],
  ["brand-soft", "bg-brand-soft", "Fundo azulado"],
  ["accent", "bg-accent", "Acento (uma vez por tela)"],
  ["success", "bg-success", "Sucesso"],
  ["warning", "bg-warning", "Atenção"],
  ["danger", "bg-danger", "Erro"],
] as const;

function Block({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="mt-16">
      <Rule variant="strong" />
      <Kicker className="mt-3">{title}</Kicker>
      <div className="mt-8">{children}</div>
    </section>
  );
}

/** Guia visual: só existe em desenvolvimento. Serve para conferir o sistema de design em um lugar só. */
export default function DesignPage() {
  if (process.env.NODE_ENV === "production") notFound();

  return (
    <main className="mx-auto w-full max-w-5xl px-6 pb-24 pt-12">
      <Kicker>Guia visual · só em desenvolvimento</Kicker>
      <h1 className="mt-3 text-6xl leading-none">Papel, tinta e um acento</h1>
      <Rule variant="double" className="mt-6" />

      <Block title="Cores">
        <ul className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
          {swatches.map(([name, cls, use]) => (
            <li key={name}>
              <div className={`h-20 border border-rule-strong ${cls}`} />
              <p className="mt-2 font-mono text-xs uppercase tracking-wider">{name}</p>
              <p className="font-sans text-sm text-ink-soft">{use}</p>
            </li>
          ))}
        </ul>
      </Block>

      <Block title="Tipografia">
        <p className="font-mono text-xs uppercase tracking-wider text-ink-soft">Newsreader · títulos e leitura</p>
        <p className="mt-2 text-7xl leading-none">Aa Consultoria</p>
        <h2 className="mt-8 text-4xl">Um título de seção com personalidade</h2>
        <p className="mt-4 max-w-prose text-xl leading-relaxed">
          Texto de leitura em serifa, com entrelinha folgada e linhas curtas. É o tom de uma boa página de revista: calmo, legível e sem pressa.{" "}
          <em>Itálico</em> para ênfase, nunca cor.
        </p>
        <p className="mt-8 font-mono text-xs uppercase tracking-wider text-ink-soft">IBM Plex Sans · interface · IBM Plex Mono · legendas</p>
        <p className="mt-2 font-sans text-base">Rótulos, botões e textos de apoio em sans, com 16 px ou mais.</p>
        <Kicker className="mt-2">Legenda em caixa-alta · Nº 01 · 2026</Kicker>
      </Block>

      <Block title="Botões">
        <div className="flex flex-wrap items-center gap-4">
          <Button>Primário</Button>
          <Button variant="accent">
            Acento <ArrowRightIcon />
          </Button>
          <Button variant="outline">Contorno</Button>
          <Button variant="quiet">Link discreto</Button>
          <Button disabled>Desativado</Button>
          <Button size="lg">Grande</Button>
        </div>
      </Block>

      <Block title="Campos e escolhas">
        <div className="grid gap-10 md:grid-cols-2">
          <div className="space-y-6">
            <div>
              <label htmlFor="d1" className="mb-1.5 block font-sans text-sm font-medium">Nome</label>
              <input id="d1" className={controlClasses} placeholder="Como devemos chamar você?" />
            </div>
            <div>
              <label htmlFor="d2" className="mb-1.5 block font-sans text-sm font-medium">E-mail</label>
              <input id="d2" aria-invalid="true" className={controlClasses} defaultValue="isso-nao-e-um-email" />
              <FieldError>Informe um e-mail válido.</FieldError>
            </div>
          </div>
          <ChoiceDemo />
        </div>
        <div className="mt-8 max-w-xl">
          <ScaleDemo />
        </div>
      </Block>

      <Block title="Avisos">
        <div className="grid max-w-2xl gap-4">
          <Notice tone="privacy" title="Sua resposta é anônima">Texto de apoio do aviso.</Notice>
          <Notice tone="success" title="Enviado">Tudo certo por aqui.</Notice>
          <Notice tone="wait" title="Ainda não começou">Volte mais tarde.</Notice>
          <Notice tone="error" title="Falta pouco">Corrija os campos marcados.</Notice>
        </div>
      </Block>

      <Block title="Ícones (traço único, sempre com função)">
        <div className="flex gap-6 text-4xl">
          <CheckIcon title="Confirmado" />
          <AlertIcon title="Atenção" />
          <ClockIcon title="Horário" />
          <LockIcon title="Privado" />
          <ArrowRightIcon title="Avançar" />
        </div>
      </Block>

      <Block title="Fios">
        <div className="space-y-6">
          <Rule variant="single" />
          <Rule variant="strong" />
          <Rule variant="double" />
          <Rule variant="accent" className="max-w-24" />
        </div>
      </Block>
    </main>
  );
}
