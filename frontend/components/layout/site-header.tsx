"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { siteConfig } from "@/config/site";
import { ButtonLink } from "@/components/ui/button";
import { cn } from "@/lib/cn";

function isActive(pathname: string, href: string) {
  return pathname === href || pathname.startsWith(`${href}/`);
}

export function Wordmark() {
  return (
    <Link href="/" className="group flex items-baseline gap-3" aria-label={`${siteConfig.name}, página inicial`}>
      <span className="font-serif text-[2rem] leading-none font-semibold tracking-[-0.02em] text-ink">KLF</span>
      <span className="hidden font-mono text-[0.6875rem] leading-tight uppercase tracking-[0.16em] text-ink-soft sm:block">
        Consultoria
        <br />& Treinamento
      </span>
    </Link>
  );
}

/** Cabeçalho fixo no topo, fundo sólido (nada de vidro fosco) e um fio só embaixo. */
export function SiteHeader() {
  const pathname = usePathname();
  const [open, setOpen] = useState(false);

  return (
    <header className="sticky top-0 z-40 border-b border-rule bg-paper">
      <div className="mx-auto flex w-full max-w-6xl items-center justify-between gap-6 px-5 py-4 sm:px-8">
        <Wordmark />

        <nav aria-label="Principal" className="hidden lg:block">
          <ul className="flex items-center gap-1 font-sans text-[0.9375rem]">
            {siteConfig.nav.map((item) => (
              <li key={item.href}>
                <Link
                  href={item.href}
                  aria-current={isActive(pathname, item.href) ? "page" : undefined}
                  className="rounded-md px-3.5 py-2 text-ink transition-colors hover:bg-paper-deep aria-[current=page]:bg-brand-soft aria-[current=page]:font-medium aria-[current=page]:text-brand"
                >
                  {item.label}
                </Link>
              </li>
            ))}
          </ul>
        </nav>

        <div className="flex items-center gap-3">
          <ButtonLink href="/contato" variant="accent" className="hidden sm:inline-flex">
            Pedir proposta
          </ButtonLink>
          <button
            type="button"
            className="min-h-11 rounded-md border border-rule-strong px-4 font-sans text-sm font-medium lg:hidden"
            aria-expanded={open}
            aria-controls="menu-principal"
            onClick={() => setOpen((value) => !value)}
          >
            {open ? "Fechar" : "Menu"}
          </button>
        </div>
      </div>

      <nav id="menu-principal" aria-label="Principal" className={cn("border-t border-rule bg-paper px-5 pt-3 pb-6 sm:px-8 lg:hidden", open ? "block" : "hidden")}>
        <ul className="grid gap-1">
          {siteConfig.nav.map((item) => (
            <li key={item.href}>
              <Link
                href={item.href}
                onClick={() => setOpen(false)}
                aria-current={isActive(pathname, item.href) ? "page" : undefined}
                className="block rounded-xl px-4 py-3 font-serif text-2xl hover:bg-paper-deep aria-[current=page]:bg-brand-soft aria-[current=page]:text-brand"
              >
                {item.label}
              </Link>
            </li>
          ))}
        </ul>
        <ButtonLink href="/contato" variant="accent" className="mt-4 w-full sm:hidden" onClick={() => setOpen(false)}>
          Pedir proposta
        </ButtonLink>
      </nav>
    </header>
  );
}
