/** Item de uma lista ordenada à mão no painel (serviços, clientes, depoimentos, álbuns, trajetória). */
export type Ordered = { id: string; displayOrder: number };

export type OrderPlan<T> = {
  /** Onde o item está: a posição atual ou, se for novo na lista, o fim dela. */
  from: number;
  /** Posição escolhida (em branco, a mesma de `from`). */
  to: number;
  /** Última posição possível. */
  last: number;
  /** Quem está na posição escolhida e vai trocar de lugar com o item. */
  occupant?: T;
};

/**
 * O que acontece ao salvar `typed` como posição do item `selfId` (sem id: item novo). `list` vem da API já na ordem
 * da lista, e a posição é o lugar na lista (0 é o primeiro), como a API conta.
 */
export function planOrder<T extends Ordered>(list: readonly T[], selfId: string | undefined, typed: string): OrderPlan<T> {
  const self = selfId ? list.findIndex((item) => item.id === selfId) : -1;
  const last = self >= 0 ? list.length - 1 : list.length;
  const from = self >= 0 ? self : last;
  const to = typed.trim() === "" ? from : Number.parseInt(typed, 10);
  const occupant = to === from ? undefined : list.find((item, index) => index === to && item.id !== selfId);

  return { from, to, last, occupant };
}
