"use client";

import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";
import { adminFetch } from "@/lib/auth/session";

const CHANGED = "klf-messages-changed";
const REFRESH_MS = 60_000;

/** Avise depois de mudar a situação de uma mensagem (ler, responder, excluir): o contador do menu atualiza na hora. */
export function notifyMessagesChanged() {
  window.dispatchEvent(new Event(CHANGED));
}

/**
 * Quantas mensagens de contato ninguém abriu ainda. Atualiza ao trocar de página, ao voltar para a aba, a cada minuto e
 * quando o painel avisa que algo mudou. Só para a administração (`enabled`); para os outros perfis fica em 0.
 */
export function useUnreadMessageCount(enabled: boolean) {
  const pathname = usePathname();
  const [count, setCount] = useState(0);

  useEffect(() => {
    if (!enabled) return;
    let active = true;
    const load = () =>
      adminFetch<{ newCount: number }>("/admin/contact-messages/summary").then(
        (summary) => active && setCount(summary.newCount),
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
  }, [enabled, pathname]);

  return enabled ? count : 0;
}
