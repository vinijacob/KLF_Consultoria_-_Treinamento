"use client";

import { useState } from "react";
import { planOrder, type OrderPlan, type Ordered } from "@/lib/admin/display-order";
import { useApi } from "@/lib/admin/hooks";
import { adminFetch } from "@/lib/auth/session";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/field";
import { Dialog } from "./ui";

type Options<T> = {
  /** Lista da API, já na ordem do painel (ex.: `/admin/clients`). */
  listPath: string;
  /** Item em edição; sem id, é um item novo. */
  selfId?: string;
  /** Só os itens que dividem a numeração com este (ex.: o mesmo tipo, na trajetória). */
  scope?: (item: T) => boolean;
  /** Nome do item no aviso de troca. */
  name: (item: T) => string;
};

type Pending<T> = { plan: OrderPlan<T>; save: () => Promise<unknown> };

/**
 * Ordem de exibição sem números repetidos: antes de salvar, confere na API se a posição escolhida já é de outro item
 * e pergunta se os dois devem trocar de lugar (quem confirma a troca de fato é a API).
 */
export function useDisplayOrder<T extends Ordered>({ listPath, selfId, scope, name }: Options<T>) {
  const { data, reload } = useApi<T[]>(listPath);
  const [pending, setPending] = useState<Pending<T> | null>(null);
  const [busy, setBusy] = useState(false);
  const inScope = (items: T[]) => (scope ? items.filter(scope) : items);
  const current = data ? planOrder(inScope(data), selfId, "") : null;
  const joining = !selfId || (data !== undefined && !inScope(data).some((item) => item.id === selfId));

  async function guard(typed: string, save: () => Promise<unknown>) {
    const fresh = await adminFetch<T[]>(listPath).catch(() => data);
    const plan = fresh ? planOrder(inScope(fresh), selfId, typed) : null;

    if (plan?.occupant) {
      setPending({ plan, save });
      return;
    }

    await save();
    reload();
  }

  async function confirm() {
    if (!pending) return;
    setBusy(true);
    await pending.save();
    setBusy(false);
    setPending(null);
    reload();
  }

  const occupant = pending?.plan.occupant;

  return {
    /** Item novo, ou vindo de outro grupo: em branco, entra no fim. */
    joining,
    end: current?.from,
    last: current?.last,
    guard,
    dialog: (
      <Dialog open={pending !== null} onClose={() => setPending(null)} title="Trocar de posição?">
        {pending && occupant && (
          <>
            <p className="font-sans text-[0.9375rem] leading-relaxed">
              A posição {pending.plan.to} já é de “{name(occupant)}”. Se confirmar, os dois trocam de lugar: “{name(occupant)}” vai para a posição{" "}
              {pending.plan.from}
              {pending.plan.from === pending.plan.last ? ", no fim da lista" : ""}.
            </p>
            <div className="mt-7 flex flex-wrap gap-3">
              <Button disabled={busy} onClick={() => void confirm()}>
                {busy ? "Salvando…" : "Trocar e salvar"}
              </Button>
              <Button variant="outline" onClick={() => setPending(null)}>
                Cancelar
              </Button>
            </div>
          </>
        )}
      </Dialog>
    ),
  };
}

/** Campo "Ordem de exibição": 0 é o primeiro; em branco, o item novo vai para o fim e o existente fica onde está. */
export function DisplayOrderField({
  order,
  value,
  onChange,
  error,
  within,
}: {
  order: ReturnType<typeof useDisplayOrder>;
  value: string;
  onChange: (value: string) => void;
  error?: string;
  /** Nome do grupo, quando a numeração é por grupo (ex.: "Formação"). */
  within?: string;
}) {
  const first = within ? `0 é o primeiro em ${within}` : "0 é o primeiro da lista";
  let hint = `${first}.`;

  if (order.last !== undefined) {
    hint = order.joining
      ? `${first}. Em branco, entra no fim (posição ${order.end}); numa posição ocupada, o item que está nela vai para o fim.`
      : `${first}; vai até ${order.last}. Se escolher a posição de outro item, os dois trocam de lugar.`;
  }

  return (
    <Input
      label="Ordem de exibição"
      hint={hint}
      type="number"
      inputMode="numeric"
      min={0}
      max={order.last}
      placeholder={order.joining && order.end !== undefined ? String(order.end) : undefined}
      value={value}
      onChange={(event) => onChange(event.target.value)}
      error={error}
    />
  );
}
