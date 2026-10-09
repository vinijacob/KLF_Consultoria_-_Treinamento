"use client";

import { useEffect, useState } from "react";
import { FeedbackForm } from "@/components/feedback/feedback-form";
import { StatusPanel } from "@/components/feedback/status-panel";
import { Button } from "@/components/ui/button";
import { AlertIcon, CheckIcon, ClockIcon, LockIcon } from "@/components/ui/icons";
import { ApiError, apiFetch } from "@/lib/api/client";
import type { PublicFeedbackForm } from "@/types/feedback";

type State =
  | { kind: "loading" }
  | { kind: "not-found" }
  | { kind: "failed" }
  | { kind: "ready"; form: PublicFeedbackForm }
  | { kind: "sent" };

const when = new Intl.DateTimeFormat("pt-BR", {
  dateStyle: "long",
  timeStyle: "short",
  timeZone: "America/Manaus",
});

export function FeedbackExperience({ code }: { code: string }) {
  const [state, setState] = useState<State>({ kind: "loading" });
  const [attempt, setAttempt] = useState(0);

  useEffect(() => {
    const controller = new AbortController();

    apiFetch<PublicFeedbackForm>(`/public/feedback/${encodeURIComponent(code)}`, {
      credentials: "include",
      cache: "no-store",
      signal: controller.signal,
    })
      .then((form) => setState({ kind: "ready", form }))
      .catch((error: unknown) => {
        if (controller.signal.aborted) return;
        setState(error instanceof ApiError && error.status === 404 ? { kind: "not-found" } : { kind: "failed" });
      });

    return () => controller.abort();
  }, [code, attempt]);

  function reload() {
    setState({ kind: "loading" });
    setAttempt((value) => value + 1);
  }

  switch (state.kind) {
    case "loading":
      return (
        <p role="status" className="py-24 font-mono text-sm uppercase tracking-[0.14em] text-ink-soft">
          Abrindo a avaliação…
        </p>
      );

    case "not-found":
      return (
        <StatusPanel icon={<AlertIcon />} kicker="Erro 404" title="Avaliação não encontrada">
          <p>Confira se o QR Code foi lido por inteiro ou se o endereço está completo.</p>
          <p>Se continuar assim, peça um novo cartaz ao instrutor.</p>
        </StatusPanel>
      );

    case "failed":
      return (
        <StatusPanel
          icon={<AlertIcon />}
          kicker="Sem conexão"
          title="Não conseguimos abrir agora"
          actions={<Button onClick={reload}>Tentar de novo</Button>}
        >
          <p>Parece que a internet falhou ou o serviço está fora do ar. Suas respostas não foram perdidas, porque ainda não começou.</p>
        </StatusPanel>
      );

    case "sent":
      return (
        <StatusPanel icon={<CheckIcon />} kicker="Avaliação enviada" title="Obrigado.">
          <p>Sua opinião chegou até nós de forma anônima. Ela ajuda a melhorar o treinamento e o seu dia a dia de trabalho.</p>
          <p className="text-ink-soft">Você já pode fechar esta página.</p>
        </StatusPanel>
      );

    case "ready": {
      const { form } = state;

      if (form.alreadyAnswered) {
        return (
          <StatusPanel icon={<LockIcon />} kicker="Resposta recebida" title="Você já respondeu">
            <p>Esta avaliação já foi enviada deste aparelho. Obrigado por participar!</p>
          </StatusPanel>
        );
      }

      if (form.status === "Scheduled") {
        return (
          <StatusPanel icon={<ClockIcon />} kicker={form.sessionTitle} title="Ainda não começou">
            <p>Esta avaliação abre em {when.format(new Date(form.opensAt))} (horário de Manaus).</p>
            <p>Volte a ler o QR Code nesse horário.</p>
          </StatusPanel>
        );
      }

      if (form.status === "Closed" || !form.definition) {
        return (
          <StatusPanel icon={<ClockIcon />} kicker={form.sessionTitle} title="Avaliação encerrada">
            <p>Esta avaliação já não recebe respostas. Obrigado pelo interesse!</p>
          </StatusPanel>
        );
      }

      return (
        <FeedbackForm
          code={code}
          form={{ ...form, definition: form.definition }}
          onSent={() => setState({ kind: "sent" })}
          onStateChanged={reload}
        />
      );
    }
  }
}
