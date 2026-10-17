import { Kicker } from "@/components/ui/typography";
import { ArrowLink } from "./blocks";
import { StoreBreak } from "./store-break";

/** Conteúdo das páginas 404 (dentro do site e de endereços que não existem): texto, saídas e a cena da loja. */
export function NotFoundContent() {
  return (
    <div className="grid items-center gap-12 lg:grid-cols-12">
      <div className="lg:col-span-6">
        <Kicker>Erro 404</Kicker>
        <h1 className="mt-4 text-[clamp(2.5rem,6vw,4.25rem)] leading-[1.02]">A página saiu para o almoço.</h1>
        <p className="mt-6 max-w-xl text-xl leading-relaxed text-ink-soft">
          O endereço pode ter mudado ou o conteúdo foi retirado do ar. Se quiser, toque o sininho: alguém vem atender.
        </p>
        <div className="mt-8 flex flex-wrap gap-x-8 gap-y-2">
          <ArrowLink href="/">Ir para a página inicial</ArrowLink>
          <ArrowLink href="/servicos">Ver os serviços</ArrowLink>
        </div>
      </div>
      <div className="lg:col-span-6">
        <StoreBreak />
        <p aria-hidden className="mt-4 hidden font-sans text-sm text-ink-soft pointer-fine:block">Dica: passe o mouse na placa para ver o outro lado.</p>
      </div>
    </div>
  );
}
