"use client";

import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";
import { adminFetch } from "@/lib/auth/session";

const CHANGED = "klf-messages-changed";
const REFRESH_MS = 60_000;

/**
 * Avise depois de mudar algo do Atendimento (ler, responder ou excluir uma mensagem; concluir um pedido LGPD): os
 * contadores do menu atualizam na hora.
 */
export function notifyMessagesChanged() {
  window.dispatchEvent(new Event(CHANGED));
}

/**
 * Um número do Atendimento que o menu mostra (mensagens não lidas, pedidos LGPD em aberto). Atualiza ao trocar de
 * página, ao voltar para a aba, a cada minuto e quando o painel avisa que algo mudou. Só para a administração
 * (`enabled`); para os outros perfis fica em 0.
 */
function usePolledCount<T>(path: string, pick: (summary: T) => number, enabled: boolean) {
  const pathname = usePathname();
  const [count, setCount] = useState(0);

  useEffect(() => {
    if (!enabled) return;
    let active = true;
    const load = () =>
      adminFetch<T>(path).then(
        (summary) => active && setCount(pick(summary)),
        () => undefined,
      );
    const onFocus = () => document.visibilityState === "visible" && void load();

    void load();
    const timer = window.setInterval(() => void load(), REFRESH_MS);
    window.addEventListener(CHANGED, load);
    document.addEventListener("visibilitychange", onFocus);
    return () => {
      active = false;
      window.clearInterval(timer);
      window.removeEventListener(CHANGED, load);
      document.removeEventListener("visibilitychange", onFocus);
    };
    // `pick` é sempre uma função fixa de quem chama; não precisa refazer a busca quando muda de identidade.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [enabled, pathname, path]);

  return enabled ? count : 0;
}

const pickNew = (summary: { newCount: number }) => summary.newCount;
const pickOpen = (summary: { openCount: number }) => summary.openCount;

/** Mensagens de contato que ninguém abriu ainda. */
export const useUnreadMessageCount = (enabled: boolean) => usePolledCount("/admin/contact-messages/summary", pickNew, enabled);

/** Pedidos LGPD ainda sem resposta. */
export const useOpenDataRequestCount = (enabled: boolean) => usePolledCount("/admin/data-requests/summary", pickOpen, enabled);
