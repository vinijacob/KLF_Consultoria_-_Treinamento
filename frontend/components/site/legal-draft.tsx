import { Container, PageHeader } from "@/components/site/blocks";
import { Notice } from "@/components/ui/notice";

/** Estrutura das páginas legais enquanto o texto definitivo não chega. Fica fora dos buscadores (noindex). */
export function LegalDraft({ title, intro, sections }: { title: string; intro: string; sections: { heading: string; text: string }[] }) {
  return (
    <>
      <PageHeader kicker="Informações" title={title} intro={intro} />
      <Container className="mt-12">
        <div className="max-w-[42rem]">
          <Notice tone="wait" title="Texto provisório">
            Esta página mostra apenas a estrutura. O texto definitivo precisa ser escrito com os dados da empresa (razão social, CNPJ,
            contato do encarregado) e revisado por um profissional antes de o site ir ao ar.
          </Notice>
          <ol className="mt-12 space-y-10">
            {sections.map((section, index) => (
              <li key={section.heading}>
                <h2 className="text-2xl">
                  <span className="mr-3 font-mono text-sm text-ink-soft">{String(index + 1).padStart(2, "0")}</span>
                  {section.heading}
                </h2>
                <p className="mt-3 text-lg leading-relaxed text-ink-soft">{section.text}</p>
              </li>
            ))}
          </ol>
        </div>
      </Container>
    </>
  );
}
