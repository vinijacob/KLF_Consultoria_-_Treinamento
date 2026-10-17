import type { Metadata } from "next";
import Link from "next/link";
import { Container, PageHeader } from "@/components/site/blocks";
import { DataRequestForm } from "@/components/site/data-request-form";
import { Kicker } from "@/components/ui/typography";
import { getSettings } from "@/lib/api/public";

export const metadata: Metadata = {
  title: "Seus dados pessoais",
  description: "Peça à KLF acesso, correção ou exclusão dos seus dados pessoais, ou retire um consentimento. Respondemos em até 15 dias.",
};

export default async function DataRequestPage() {
  const { legal } = await getSettings();

  return (
    <>
      <PageHeader
        kicker="LGPD"
        title="Seus dados pessoais"
        intro="Você pode pedir para saber quais dados a KLF tem sobre você, corrigir, apagar ou retirar um consentimento. Respondemos em até 15 dias."
      />
      <Container className="mt-12 grid items-start gap-12 lg:grid-cols-12">
        <div id="formulario" className="lg:col-span-7">
          <DataRequestForm />
        </div>
        <aside className="grid gap-6 lg:col-span-5">
          <div className="rounded-2xl bg-paper-deep p-7">
            <Kicker>Como funciona</Kicker>
            <ol className="mt-4 grid list-decimal gap-3 pl-5 font-sans text-[0.9375rem] leading-relaxed">
              <li>Você recebe um protocolo na hora.</li>
              <li>Confirmamos por e-mail que o pedido é mesmo seu.</li>
              <li>Respondemos em até 15 dias, dizendo o que foi feito ou, se algo precisar ser guardado por lei, o motivo.</li>
            </ol>
          </div>
          <div>
            <Kicker>Prefere e-mail?</Kicker>
            <p className="mt-3 font-sans text-[0.9375rem] leading-relaxed">
              {legal?.dpoEmail ? (
                <>
                  Escreva para o encarregado de dados, {legal.dpoName ?? "da KLF"}:{" "}
                  <a href={`mailto:${legal.dpoEmail}`} className="text-brand underline underline-offset-4">
                    {legal.dpoEmail}
                  </a>
                  .
                </>
              ) : (
                "Use o formulário: ele chega direto para quem cuida dos dados na KLF."
              )}{" "}
              Detalhes na{" "}
              <Link href="/privacidade#direitos" className="text-brand underline underline-offset-4">
                política de privacidade
              </Link>
              .
            </p>
          </div>
        </aside>
      </Container>
    </>
  );
}
