"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState } from "react";
import { siteConfig } from "@/config/site";
import { ButtonLink } from "@/components/ui/button";
import { Kicker, Rule } from "@/components/ui/typography";
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

export function SiteHeader() {
  const pathname = usePathname();
  const [open, setOpen] = useState(false);

  return (
    <header className="mx-auto w-full max-w-6xl px-5 sm:px-8">
      <div className="flex items-center justify-between gap-4 pt-4 pb-3">
        <Kicker className="text-[0.6875rem]">Kilciene Lima Ferreira</Kicker>
        <Kicker className="hidden text-[0.6875rem] sm:block">Elegância · Energia · Resultado</Kicker>
      </div>
      <Rule variant="strong" />

      <div className="flex items-center justify-between gap-6 py-5">
        <Wordmark />

        <nav aria-label="Principal" className="hidden lg:block">
          <ul className="flex items-center gap-7 font-sans text-[0.9375rem]">
            {siteConfig.nav.map((item) => (
              <li key={item.href}>
                <Link
                  href={item.href}
                  aria-current={isActive(pathname, item.href) ? "page" : undefined}
                  className="py-2 text-ink underline-offset-[6px] decoration-1 hover:underline aria-[current=page]:underline aria-[current=page]:decoration-2"
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
            className="min-h-11 border border-ink px-4 font-sans text-sm font-medium uppercase tracking-[0.12em] lg:hidden"
            aria-expanded={open}
            aria-controls="menu-principal"
            onClick={() => setOpen((value) => !value)}
          >
            {open ? "Fechar" : "Menu"}
          </button>
        </div>
      </div>

      <nav
        id="menu-principal"
        aria-label="Principal"
        className={cn("border-t border-ink pb-6 lg:hidden", open ? "block" : "hidden")}
      >
        <ul className="divide-y divide-rule">
          {siteConfig.nav.map((item, index) => (
            <li key={item.href}>
              <Link
                href={item.href}
                onClick={() => setOpen(false)}
                aria-current={isActive(pathname, item.href) ? "page" : undefined}
                className="flex items-baseline gap-4 py-3.5 font-serif text-2xl aria-[current=page]:italic"
              >
                <span className="font-mono text-xs text-ink-soft">{String(index + 1).padStart(2, "0")}</span>
                {item.label}
              </Link>
            </li>
          ))}
        </ul>
        <ButtonLink href="/contato" variant="accent" className="mt-5 w-full sm:hidden" onClick={() => setOpen(false)}>
          Pedir proposta
        </ButtonLink>
      </nav>
      <Rule variant="double" />
    </header>
  );
}
