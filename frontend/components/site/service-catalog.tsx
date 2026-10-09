"use client";

import { useState } from "react";
import { Button } from "@/components/ui/button";
import { serviceFormatLabel } from "@/lib/format";
import { matchesQuery } from "@/lib/search";
import type { ServiceListItem } from "@/types/site";
import { ServiceIndex } from "./lists";
import { SearchField } from "./search-field";

const FIRST_PAGE = 6;

/** Catálogo de serviços com busca na hora (sem acentos) e só os 6 primeiros até a pessoa pedir o resto. */
export function ServiceCatalog({ services }: { services: ServiceListItem[] }) {
  const [query, setQuery] = useState("");
  const [expanded, setExpanded] = useState(false);

  const numbered = services.map((service, index) => ({ service, number: index + 1 }));
  const found = numbered.filter(({ service }) =>
    matchesQuery(query, service.title, service.summary, serviceFormatLabel[service.format]),
  );
  const searching = query.trim().length > 0;
  const shown = searching || expanded ? found : found.slice(0, FIRST_PAGE);
  const hidden = found.length - shown.length;

  const status = searching
    ? found.length === 0
      ? `Nenhum serviço encontrado para “${query.trim()}”.`
      : `${found.length} ${found.length === 1 ? "serviço encontrado" : "serviços encontrados"}.`
    : `${services.length} ${services.length === 1 ? "serviço" : "serviços"} no catálogo.`;

  return (
    <div>
      <SearchField
        label="Buscar treinamento ou palestra"
        placeholder="Ex.: vendas, atendimento, liderança"
        value={query}
        onValueChange={setQuery}
        onClear={() => setQuery("")}
        status={status}
        className="mb-8 max-w-2xl"
      />
      {shown.length > 0 && (
        <ServiceIndex
          key={searching ? "busca" : "catalogo"}
          services={shown.map(({ service }) => service)}
          numbers={shown.map(({ number }) => number)}
          itemClassName="motion-safe:animate-rise"
        />
      )}
      {hidden > 0 && (
        <div className="mt-10 flex justify-center">
          <Button variant="outline" size="lg" onClick={() => setExpanded(true)}>
            Ver os outros {hidden} {hidden === 1 ? "serviço" : "serviços"}
          </Button>
        </div>
      )}
    </div>
  );
}
