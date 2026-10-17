"use client";

import { useRouter } from "next/navigation";
import { useState, type ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { SearchField } from "./search-field";

/**
 * Busca nas publicações. É um formulário GET comum (`/conteudo?busca=...`): funciona sem JavaScript, o link pode ser
 * compartilhado e quem busca é a API. Mantém os filtros escolhidos (tipo, categoria, tag).
 */
export function PostSearch({
  initial,
  keep,
  clearHref,
  status,
}: {
  initial: string;
  keep: Record<string, string | undefined>;
  clearHref: string;
  status: ReactNode;
}) {
  const router = useRouter();
  const [value, setValue] = useState(initial);

  return (
    <form action="/conteudo" method="get" className="flex max-w-2xl items-start gap-4">
      {Object.entries(keep).map(([name, value]) => value && <input key={name} type="hidden" name={name} value={value} />)}
      <SearchField
        label="Buscar nas publicações"
        placeholder="Ex.: fidelização, pós-venda"
        name="busca"
        value={value}
        onValueChange={setValue}
        onClear={() => {
          setValue("");
          router.push(clearHref);
        }}
        status={status}
        className="flex-1"
      />
      <Button type="submit" variant="outline" className="mt-7">
        Buscar
      </Button>
    </form>
  );
}
