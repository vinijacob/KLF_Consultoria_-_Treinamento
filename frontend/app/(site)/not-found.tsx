import { ArrowLink, Container } from "@/components/site/blocks";
import { Kicker, Rule } from "@/components/ui/typography";

export default function SiteNotFound() {
  return (
    <Container className="py-24">
      <Kicker>Erro 404</Kicker>
      <h1 className="mt-4 text-[clamp(2.5rem,7vw,4.75rem)] leading-[1.02]">Esta página não existe.</h1>
      <Rule variant="accent" className="my-8 max-w-24" />
      <p className="max-w-xl text-xl leading-relaxed text-ink-soft">O endereço pode ter mudado ou o conteúdo foi retirado do ar.</p>
      <ArrowLink href="/" className="mt-8">
        Ir para a página inicial
      </ArrowLink>
    </Container>
  );
}
