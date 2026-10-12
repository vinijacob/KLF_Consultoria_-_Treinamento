"use client";

import { Fragment, useState, type ReactNode } from "react";
import { useApi, useFlash } from "@/lib/admin/hooks";
import { Notice } from "@/components/ui/notice";
import { LoadError, Loading } from "./ui";

/** Carrega o registro (na edição) antes de montar o formulário, para o estado inicial já vir completo. */
export function EntityLoader<T>({
  path,
  children,
}: {
  path: string | null;
  children: (data: T | undefined, replace: (data: T) => void, reset: () => void) => ReactNode;
}) {
  const { data, error, reload, replace } = useApi<T>(path);
  const [version, setVersion] = useState(0);
  const reset = () => setVersion((current) => current + 1);

  if (!path) return <Fragment key={version}>{children(undefined, replace, reset)}</Fragment>;
  if (error) return <LoadError error={error} onRetry={reload} />;
  if (!data) return <Loading />;

  return <>{children(data, replace, reset)}</>;
}

/** Recado vindo da tela anterior ("Serviço excluído."). */
export function FlashNotice() {
  const flash = useFlash();
  if (!flash) return null;

  return (
    <Notice tone="success" role="status" className="mb-8">
      {flash.message}
    </Notice>
  );
}
