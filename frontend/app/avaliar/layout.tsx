import { siteConfig } from "@/config/site";
import { Kicker, Rule } from "@/components/ui/typography";

/** Layout enxuto da avaliação anônima: sem menu, sem links para fora, nada que distraia ou rastreie. */
export default function FeedbackLayout({ children }: LayoutProps<"/avaliar">) {
  return (
    <div className="mx-auto flex w-full max-w-3xl flex-1 flex-col px-5 pb-16 sm:px-8">
      <header className="pt-8">
        <div className="flex items-baseline justify-between gap-4">
          <p className="font-sans text-sm font-semibold uppercase tracking-[0.18em] text-ink">{siteConfig.shortName}</p>
          <Kicker>Consultoria &amp; Treinamento</Kicker>
        </div>
        <Rule variant="double" className="mt-3" />
      </header>
      <main className="flex-1 pt-10 sm:pt-14">{children}</main>
      <footer className="mt-20">
        <Rule variant="single" />
        <Kicker className="mt-4">{siteConfig.name}</Kicker>
      </footer>
    </div>
  );
}
