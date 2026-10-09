"use client";

import type { ReactNode } from "react";
import { useApi, useFlash } from "@/lib/admin/hooks";
import { Notice } from "@/components/ui/notice";
import { LoadError, Loading } from "./ui";

/** Carrega o registro (na edição) antes de montar o formulário, para o estado inicial já vir completo. */
export function EntityLoader<T>({
  path,
  children,
}: {
  path: string | null;
  children: (data: T | undefined, replace: (data: T) => void) => ReactNode;
}) {
  const { data, error, reload, replace } = useApi<T>(path);

  if (!path) return <>{children(undefined, replace)}</>;
  if (error) return <LoadError error={error} onRetry={reload} />;
  if (!data) return <Loading />;

  return <>{children(data, replace)}</>;
}

/** Recado vindo da tela anterior ("Serviço excluído."). */
export function FlashNotice() {
  const message = useFlash();
  if (!message) return null;

  return (
    <Notice tone="success" role="status" className="mb-8">
      {message}
    </Notice>
  );
}
