"use client";

import { Container } from "@/components/site/blocks";
import { Button } from "@/components/ui/button";
import { Kicker, Rule } from "@/components/ui/typography";

export default function SiteError({ reset }: { error: Error & { digest?: string }; reset: () => void }) {
  return (
    <Container className="py-24">
      <Kicker>Instabilidade</Kicker>
      <h1 className="mt-4 text-[clamp(2.5rem,7vw,4.75rem)] leading-[1.02]">Não conseguimos carregar esta página.</h1>
      <Rule variant="accent" className="my-8 max-w-24" />
      <p className="max-w-xl text-xl leading-relaxed text-ink-soft">Tente de novo em instantes. Se continuar, fale com a gente pelos contatos do rodapé.</p>
      <Button className="mt-8" onClick={reset}>
        Tentar de novo
      </Button>
    </Container>
  );
}
