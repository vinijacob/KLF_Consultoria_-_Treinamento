import type { ReactNode } from "react";
import { Container, PageHeader } from "@/components/site/blocks";
import { Notice } from "@/components/ui/notice";
import { formatDay, twoDigits } from "@/lib/format";
import { legalUpdatedOn, missingLegalData } from "@/lib/legal";
import type { LegalSettings } from "@/types/site";

export type LegalSection = { id: string; heading: string; body: ReactNode };

/** Dado da empresa no texto legal; vazio, aparece marcado como pendente (a página avisa no topo o que falta). */
export function Datum({ value, label }: { value?: string | null; label: string }) {
  if (value?.trim()) return <>{value}</>;
  return <span className="rounded-md bg-paper-deep px-1.5 py-0.5 font-sans text-[0.9em] text-warning">[{label} a definir]</span>;
}

/** O endereço do site sem o protocolo, como aparece no texto. */
export function siteHost(url: string) {
  return url.replace(/^https?:\/\//, "").replace(/\/$/, "");
}

/** Página legal: índice ao lado, data da última atualização e seções numeradas com âncora. */
export function LegalDocument({
  title,
  intro,
  legal,
  sections,
}: {
  title: string;
  intro: string;
  legal?: LegalSettings;
  sections: LegalSection[];
}) {
  const missing = missingLegalData(legal);

  return (
    <>
      <PageHeader kicker="Informações" title={title} intro={intro} />
      <Container className="mt-12 grid items-start gap-12 lg:grid-cols-12">
        <nav aria-label="Nesta página" className="rounded-2xl bg-paper-deep p-6 lg:sticky lg:top-28 lg:col-span-4">
          <p className="font-mono text-xs uppercase tracking-[0.14em] text-ink-soft">Nesta página</p>
          <ol className="mt-4 grid gap-1 font-sans text-[0.9375rem]">
            {sections.map((section, index) => (
              <li key={section.id}>
                <a href={`#${section.id}`} className="flex min-h-11 items-baseline gap-3 py-2 hover:text-brand hover:underline hover:underline-offset-4">
                  <span className="font-mono text-xs text-ink-soft">{twoDigits(index + 1)}</span>
                  {section.heading}
                </a>
              </li>
            ))}
          </ol>
        </nav>

        <div className="lg:col-span-8">
          <p className="font-mono text-xs uppercase tracking-[0.14em] text-ink-soft">
            Última atualização: <time dateTime={legalUpdatedOn(legal)}>{formatDay(legalUpdatedOn(legal))}</time>
          </p>
          {missing.length > 0 && (
            <Notice tone="wait" title="Faltam dados da empresa" className="mt-6">
              Preencha em Dados do site → Dados legais: {missing.join(", ")}. Enquanto isso, esta página fica fora dos buscadores.
            </Notice>
          )}
          <div className="mt-10 space-y-14">
            {sections.map((section, index) => (
              <section key={section.id} id={section.id} aria-labelledby={`${section.id}-titulo`} className="scroll-mt-28">
                <h2 id={`${section.id}-titulo`} className="text-[1.875rem] leading-tight">
                  <span className="mr-3 font-mono text-sm text-ink-soft">{twoDigits(index + 1)}</span>
                  {section.heading}
                </h2>
                <div className="prose-klf mt-5">{section.body}</div>
              </section>
            ))}
          </div>
        </div>
      </Container>
    </>
  );
}

/** Metadados das páginas legais: fora dos buscadores enquanto faltar algum dado da empresa. */
export function legalRobots(legal?: LegalSettings) {
  return missingLegalData(legal).length > 0 ? { index: false, follow: true } : undefined;
}
