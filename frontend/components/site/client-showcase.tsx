"use client";

import Image from "next/image";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/cn";
import { mediaUrl } from "@/lib/media";
import { usePrefersReducedMotion } from "@/lib/motion";
import { matchesQuery } from "@/lib/search";
import type { Client } from "@/types/site";
import { initials } from "./blocks";
import { SearchField } from "./search-field";

const SWAP_MS = 2600;
const PAGE = 20;

/** Cartão da empresa: logo (se houver) ou as iniciais, e o nome. Com site, o cartão vira link. */
export function ClientTile({ client, className }: { client: Client; className?: string }) {
  const body = (
    <>
      <span className="relative grid h-20 w-full place-items-center">
        {client.logoId ? (
          <Image src={mediaUrl(client.logoId)} alt={`Logo de ${client.name}`} fill unoptimized sizes="200px" className="object-contain" />
        ) : (
          <span aria-hidden className="grid size-16 place-items-center rounded-full bg-paper-deep font-serif text-2xl text-ink-soft">
            {initials(client.name)}
          </span>
        )}
      </span>
      <span className="mt-4 block text-center font-serif text-lg leading-snug">{client.name}</span>
    </>
  );
  const box = cn("flex h-full flex-col items-center justify-center rounded-2xl border border-rule bg-card px-4 py-6", className);

  return client.websiteUrl ? (
    <a href={client.websiteUrl} target="_blank" rel="noopener noreferrer" className={cn("lift", box)}>
      {body}
      <span className="sr-only"> (site da empresa, abre em nova aba)</span>
    </a>
  ) : (
    <div className={box}>{body}</div>
  );
}

function sortByName(clients: Client[]) {
  return [...clients].sort((a, b) => a.name.localeCompare(b.name, "pt-BR"));
}

/**
 * Vitrine das empresas atendidas: uma seleção sorteada (no servidor) que vai trocando um cartão por vez, com o novo
 * aparecendo suavemente. Para com o mouse em cima e não se mexe para quem pediu menos movimento.
 * Com `searchable`, ganha busca e a lista completa em ordem alfabética, de 20 em 20.
 */
export function ClientShowcase({
  clients,
  initialIds,
  searchable = false,
}: {
  clients: Client[];
  initialIds: string[];
  searchable?: boolean;
}) {
  const reducedMotion = usePrefersReducedMotion();
  const [slots, setSlots] = useState(initialIds);
  const [hovering, setHovering] = useState(false);
  const [query, setQuery] = useState("");
  const [showAll, setShowAll] = useState(false);
  const [limit, setLimit] = useState(PAGE);
  const byId = new Map(clients.map((client) => [client.id, client]));
  const searching = query.trim().length > 0;
  const listing = searching || showAll;
  const rotating = !listing && !hovering && !reducedMotion && clients.length > slots.length;

  useEffect(() => {
    if (!rotating) return;

    const timer = window.setInterval(() => {
      setSlots((current) => {
        const outside = clients.filter((client) => !current.includes(client.id));
        if (outside.length === 0) return current;
        const slot = Math.floor(Math.random() * current.length);
        const next = [...current];
        next[slot] = outside[Math.floor(Math.random() * outside.length)].id;
        return next;
      });
    }, SWAP_MS);

    return () => window.clearInterval(timer);
  }, [rotating, clients]);

  const found = sortByName(clients.filter((client) => matchesQuery(query, client.name)));
  const listed = found.slice(0, limit);
  const status = searching
    ? found.length === 0
      ? `Nenhuma empresa encontrada para “${query.trim()}”.`
      : `${found.length} de ${clients.length} ${clients.length === 1 ? "empresa" : "empresas"}.`
    : showAll
      ? `Todas as ${clients.length} empresas, em ordem alfabética.`
      : clients.length > slots.length
        ? `Uma seleção de ${slots.length} das ${clients.length} empresas atendidas, que vai mudando.`
        : `${clients.length} ${clients.length === 1 ? "empresa atendida" : "empresas atendidas"}.`;

  return (
    <div>
      {searchable && (
        <div className="mb-8 flex flex-wrap items-start justify-between gap-x-8 gap-y-4">
          <SearchField
            label="Buscar empresa ou loja"
            placeholder="Digite o nome"
            value={query}
            onValueChange={(value) => {
              setQuery(value);
              setLimit(PAGE);
            }}
            onClear={() => setQuery("")}
            status={status}
            className="w-full max-w-xl"
          />
          {!searching && clients.length > slots.length && (
            <Button variant="outline" className="sm:mt-7" onClick={() => setShowAll((value) => !value)} aria-pressed={showAll}>
              {showAll ? "Voltar para a seleção" : `Ver todas as ${clients.length}`}
            </Button>
          )}
        </div>
      )}

      {listing ? (
        <>
          <ul className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5">
            {listed.map((client) => (
              <li key={client.id} className="motion-safe:animate-rise">
                <ClientTile client={client} />
              </li>
            ))}
          </ul>
          {found.length > listed.length && (
            <div className="mt-8 flex justify-center">
              <Button variant="outline" size="lg" onClick={() => setLimit((current) => current + PAGE)}>
                Mostrar mais ({found.length - listed.length})
              </Button>
            </div>
          )}
        </>
      ) : (
        <ul
          className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-5"
          onMouseEnter={() => setHovering(true)}
          onMouseLeave={() => setHovering(false)}
        >
          {slots.map((clientId, slot) => {
            const client = byId.get(clientId);
            return (
              client && (
                <li key={`${slot}-${clientId}`} className="motion-safe:animate-fade">
                  <ClientTile client={client} />
                </li>
              )
            );
          })}
        </ul>
      )}
    </div>
  );
}
