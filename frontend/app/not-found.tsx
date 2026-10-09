import Link from "next/link";
import { Kicker, Rule } from "@/components/ui/typography";

export default function NotFound() {
  return (
    <main className="mx-auto w-full max-w-3xl px-6 py-24">
      <Kicker>Erro 404</Kicker>
      <h1 className="mt-4 text-[clamp(2.5rem,7vw,4.75rem)] leading-[1.02]">Esta página não existe.</h1>
      <Rule variant="accent" className="my-8 max-w-24" />
      <Link href="/" className="font-sans text-brand underline underline-offset-4">
        Ir para a página inicial
      </Link>
    </main>
  );
}
