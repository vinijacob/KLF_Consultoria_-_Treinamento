"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useState, type ReactNode } from "react";
import { isActive, navFor, roleInfo } from "@/config/admin";
import { datelineFor } from "@/lib/admin/datetime";
import { cn } from "@/lib/cn";
import type { Role } from "@/types/admin";
import { Kicker, Rule } from "@/components/ui/typography";
import { useAuth } from "./auth-provider";

/** Carimbo do perfil: moldura dupla, caixa-alta mono. Aparece no banner e no expediente. */
export function RoleStamp({ role, inverted = false, className }: { role: Role; inverted?: boolean; className?: string }) {
  return (
    <span
      className={cn(
        "inline-flex items-center border-[3px] border-double px-2.5 py-1 font-mono text-[0.75rem] leading-none font-medium uppercase tracking-[0.2em]",
        inverted ? "border-brand-contrast text-brand-contrast" : role === "Admin" ? "border-brand text-brand" : "border-accent text-accent",
        className,
      )}
    >
      {roleInfo[role].label}
    </span>
  );
}

/**
 * Faixa do perfil no topo de todas as telas do painel: quem está logado, com que perfil e o que esse perfil alcança.
 * Administração em azul-marinho cheio; Edição em papel com o carimbo no acento, para não confundir as duas.
 */
export function RoleBanner() {
  const { user, role } = useAuth();
  const info = roleInfo[role];
  const isAdmin = role === "Admin";

  return (
    <div
      role="region"
      aria-label="Perfil de acesso"
      className={cn(isAdmin ? "bg-brand text-brand-contrast" : "border-b-2 border-ink bg-paper-deep text-ink")}
    >
      <div className="flex flex-wrap items-center gap-x-5 gap-y-2 px-5 py-2.5 sm:px-8">
        <RoleStamp role={role} inverted={isAdmin} />
        <p className="min-w-0 flex-1 font-sans text-sm">
          <span className="font-semibold">{user.fullName}</span>
          <span className="hidden md:inline"> — {info.summary}</span>
        </p>
        <p className={cn("hidden font-mono text-[0.6875rem] uppercase tracking-[0.14em] xl:block", !isAdmin && "text-ink-soft")}>
          {datelineFor()}
        </p>
      </div>
    </div>
  );
}

function PanelWordmark() {
  return (
    <Link href="/painel" className="flex items-baseline gap-3" aria-label="Painel KLF, visão geral">
      <span className="font-serif text-[1.75rem] leading-none font-semibold tracking-[-0.02em] text-ink">KLF</span>
      <span className="font-mono text-[0.6875rem] leading-tight uppercase tracking-[0.16em] text-ink-soft">
        Painel
        <br />
        da redação
      </span>
    </Link>
  );
}

function Navigation({ onNavigate }: { onNavigate?: () => void }) {
  const pathname = usePathname();
  const { role } = useAuth();
  const groups = navFor(role);
  const firstNumber = groups.map((_, g) => groups.slice(0, g).reduce((total, group) => total + group.items.length, 0));

  return (
    <nav aria-label="Painel">
      {groups.map((group, g) => (
        <div key={group.title} className="mt-6 first:mt-0">
          <Kicker className="text-[0.6875rem]">{group.title}</Kicker>
          <ul className="mt-2 border-t border-rule">
            {group.items.map((item, i) => {
              const current = isActive(item, pathname);
              const index = String(firstNumber[g] + i).padStart(2, "0");

              return (
                <li key={item.href} className="border-b border-rule">
                  <Link
                    href={item.href}
                    onClick={onNavigate}
                    aria-current={current ? "page" : undefined}
                    className={cn(
                      "flex min-h-11 items-baseline gap-3 py-2 font-sans text-[0.9375rem] text-ink transition-colors",
                      "hover:text-brand aria-[current=page]:font-semibold aria-[current=page]:text-brand",
                    )}
                  >
                    <span className="w-5 font-mono text-xs font-normal text-ink-soft">{index}</span>
                    <span className="flex-1">{item.label}</span>
                    {current && <span aria-hidden className="h-px w-4 self-center bg-accent" />}
                  </Link>
                </li>
              );
            })}
          </ul>
        </div>
      ))}
    </nav>
  );
}

function AccountFooter() {
  const { user, logout } = useAuth();
  const [leaving, setLeaving] = useState(false);

  return (
    <div className="mt-8 border-t-2 border-ink pt-4">
      <p className="truncate font-sans text-sm font-medium text-ink">{user.fullName}</p>
      <p className="truncate font-sans text-sm text-ink-soft">{user.email}</p>
      <div className="mt-2 flex flex-wrap items-center gap-x-5 font-sans text-sm">
        <Link href="/" target="_blank" rel="noopener" className="inline-flex min-h-11 items-center text-brand underline underline-offset-4">
          Ver o site
        </Link>
        <button
          type="button"
          disabled={leaving}
          onClick={() => {
            setLeaving(true);
            void logout();
          }}
          className="inline-flex min-h-11 items-center text-brand underline underline-offset-4 disabled:opacity-50"
        >
          {leaving ? "Saindo…" : "Sair"}
        </button>
      </div>
    </div>
  );
}

export function AdminShell({ children }: { children: ReactNode }) {
  const [open, setOpen] = useState(false);

  return (
    <div className="flex min-h-full flex-1 flex-col">
      <a
        href="#conteudo"
        className="sr-only focus:not-sr-only focus:absolute focus:top-2 focus:left-2 focus:z-50 focus:bg-paper focus:px-4 focus:py-2 focus:font-sans"
      >
        Pular para o conteúdo
      </a>
      <RoleBanner />

      <div className="flex flex-1">
        <aside className="sticky top-0 hidden h-screen w-72 shrink-0 flex-col overflow-y-auto border-r border-rule px-7 py-8 lg:flex">
          <PanelWordmark />
          <Rule variant="double" className="mt-4 mb-7" />
          <Navigation />
          <div className="flex-1" />
          <AccountFooter />
        </aside>

        <div className="min-w-0 flex-1">
          <div className="border-b border-ink px-5 sm:px-8 lg:hidden">
            <div className="flex items-center justify-between py-4">
              <PanelWordmark />
              <button
                type="button"
                className="min-h-11 border border-ink px-4 font-sans text-sm font-medium uppercase tracking-[0.12em]"
                aria-expanded={open}
                aria-controls="menu-painel"
                onClick={() => setOpen((value) => !value)}
              >
                {open ? "Fechar" : "Menu"}
              </button>
            </div>
            <div id="menu-painel" className={cn("pb-6", open ? "block" : "hidden")}>
              <Navigation onNavigate={() => setOpen(false)} />
              <AccountFooter />
            </div>
          </div>

          <main id="conteudo" className="mx-auto w-full max-w-6xl px-5 py-10 sm:px-8 lg:px-12 lg:py-12">
            {children}
          </main>
        </div>
      </div>
    </div>
  );
}
