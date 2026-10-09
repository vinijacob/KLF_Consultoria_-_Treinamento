"use client";

import { useState } from "react";
import { cn } from "@/lib/cn";
import { ALPHABET, groupByInitial, matchesQuery } from "@/lib/search";
import type { Client } from "@/types/site";
import { SearchField } from "./search-field";

const anchor = (letter: string) => `letra-${letter === "#" ? "outros" : letter}`;

/**
 * Diretório de empresas atendidas, como o índice remissivo de um livro: busca por nome, atalho por letra e
 * colunas agrupadas de A a Z. Continua legível com centenas de nomes.
 */
export function ClientDirectory({ clients }: { clients: Client[] }) {
  const [query, setQuery] = useState("");
  const found = clients.filter((client) => matchesQuery(query, client.name));
  const groups = groupByInitial(found, (client) => client.name);
  const present = new Set(groups.map(([letter]) => letter));
  const searching = query.trim().length > 0;

  const status = searching
    ? found.length === 0
      ? `Nenhuma empresa encontrada para “${query.trim()}”.`
      : `${found.length} de ${clients.length} ${clients.length === 1 ? "empresa" : "empresas"}.`
    : `${clients.length} ${clients.length === 1 ? "empresa atendida" : "empresas atendidas"}.`;

  return (
    <div>
      <SearchField
        label="Buscar empresa ou loja"
        placeholder="Digite o nome"
        value={query}
        onValueChange={setQuery}
        onClear={() => setQuery("")}
        status={status}
        className="max-w-2xl"
      />

      <nav aria-label="Índice alfabético" className="mt-6 w-fit border-t border-l border-rule">
        <ul className="grid grid-cols-7 font-mono text-sm sm:grid-cols-14">
          {ALPHABET.map((letter) => (
            <li key={letter} className="border-r border-b border-rule">
              {present.has(letter) ? (
                <a
                  href={`#${anchor(letter)}`}
                  className="grid size-11 place-items-center font-medium text-ink hover:bg-ink hover:text-paper"
                  aria-label={letter === "#" ? "Nomes que começam com número" : `Letra ${letter}`}
                >
                  {letter}
                </a>
              ) : (
                <span aria-hidden className="grid size-11 place-items-center text-ink-soft opacity-40">
                  {letter}
                </span>
              )}
            </li>
          ))}
        </ul>
      </nav>

      <div key={query} className="mt-10 gap-x-14 sm:columns-2 lg:columns-3">
        {groups.map(([letter, items]) => (
          <section
            key={letter}
            id={anchor(letter)}
            aria-label={letter === "#" ? "Outros" : `Letra ${letter}`}
            className="mb-10 scroll-mt-8 break-inside-avoid motion-safe:animate-rise"
          >
            <p aria-hidden className="flex items-baseline gap-4 border-b-2 border-ink pb-1 font-serif text-5xl leading-none">
              {letter}
              <span className="font-mono text-xs tracking-[0.12em] text-ink-soft">{items.length}</span>
            </p>
            <ul>
              {items.map((client) => (
                <li key={client.id} className="border-b border-rule">
                  {client.websiteUrl ? (
                    <a
                      href={client.websiteUrl}
                      target="_blank"
                      rel="noopener noreferrer"
                      className={cn("flex min-h-11 items-center py-2 font-serif text-xl leading-snug", "underline-offset-4 hover:underline")}
                    >
                      {client.name}
                      <span className="ml-2 font-mono text-[0.6875rem] uppercase tracking-[0.12em] text-ink-soft">site</span>
                      <span className="sr-only"> (abre em nova aba)</span>
                    </a>
                  ) : (
                    <span className="flex min-h-11 items-center py-2 font-serif text-xl leading-snug">{client.name}</span>
                  )}
                </li>
              ))}
            </ul>
          </section>
        ))}
      </div>
    </div>
  );
}
