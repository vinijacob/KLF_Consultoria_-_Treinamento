import type { ReactNode } from "react";
import { Kicker, Rule } from "@/components/ui/typography";

/** Tela inteira de estado (não encontrada, ainda não abriu, encerrada, já respondeu, obrigado). */
export function StatusPanel({
  icon,
  kicker,
  title,
  children,
  actions,
}: {
  icon: ReactNode;
  kicker: string;
  title: string;
  children?: ReactNode;
  actions?: ReactNode;
}) {
  return (
    <section aria-labelledby="status-title" className="py-6 sm:py-12">
      <div className="mb-8 grid size-14 place-items-center border-2 border-ink text-3xl text-ink">{icon}</div>
      <Kicker>{kicker}</Kicker>
      <h1 id="status-title" className="mt-3 text-[clamp(2.25rem,7vw,3.75rem)] leading-[1.05] text-ink">
        {title}
      </h1>
      <Rule variant="accent" className="my-7 max-w-24" />
      {children && <div className="max-w-prose space-y-4 text-xl leading-relaxed text-ink">{children}</div>}
      {actions && <div className="mt-8">{actions}</div>}
    </section>
  );
}
