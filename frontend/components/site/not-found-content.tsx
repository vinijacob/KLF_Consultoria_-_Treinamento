import { Kicker } from "@/components/ui/typography";
import { ArrowLink } from "./blocks";
import { LostPageStage } from "./lost-page-stage";

/** Conteúdo das páginas 404 (dentro do site e de endereços que não existem): texto, saídas e o vigia com a lanterna. */
export function NotFoundContent() {
  return (
    <div className="grid items-center gap-12 lg:grid-cols-12">
      <div className="lg:col-span-5">
        <Kicker>Erro 404</Kicker>
        <h1 className="mt-4 text-[clamp(2.5rem,6vw,4.25rem)] leading-[1.02]">Esta página saiu de cena.</h1>
        <p className="mt-6 max-w-xl text-xl leading-relaxed text-ink-soft">
          O endereço pode ter mudado ou o conteúdo foi retirado do ar. Nosso vigia já está procurando.
        </p>
        <div className="mt-8 flex flex-wrap gap-x-8 gap-y-2">
          <ArrowLink href="/">Ir para a página inicial</ArrowLink>
          <ArrowLink href="/servicos">Ver os serviços</ArrowLink>
        </div>
      </div>
      <div className="lg:col-span-7">
        <LostPageStage />
        <p aria-hidden className="mt-4 font-sans text-sm text-ink-soft">
          Dica: mexa o mouse (ou toque na cena) para apontar a lanterna. Dizem que há uma porta escondida no escuro.
        </p>
      </div>
    </div>
  );
}
