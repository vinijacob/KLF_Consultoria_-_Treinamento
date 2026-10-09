"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { ApiError } from "@/lib/api/client";
import { adminFetch } from "@/lib/auth/session";
import { setFlash, useFlash, useSubmit } from "./hooks";

const timeFormat = new Intl.DateTimeFormat("pt-BR", { hour: "2-digit", minute: "2-digit", timeZone: "America/Manaus" });

type EditorOptions = {
  /** Rota da API, ex.: `/admin/services`. */
  apiPath: string;
  /** Lista no painel, ex.: `/painel/servicos`. */
  listHref: string;
  id?: string;
  created: string;
  deleted: string;
};

/**
 * Criar, salvar e excluir um registro do painel. Ao criar, vai para a tela de edição do registro novo;
 * ao excluir, volta para a lista com um recado.
 */
export function useEntityEditor<TResponse extends { id: string }>({ apiPath, listHref, id, created, deleted }: EditorOptions) {
  const router = useRouter();
  const submit = useSubmit();
  const flash = useFlash();
  const [savedAt, setSavedAt] = useState<Date | null>(null);

  async function save(payload: unknown) {
    const result = await submit.run(() =>
      adminFetch<TResponse>(id ? `${apiPath}/${id}` : apiPath, { method: id ? "PUT" : "POST", json: payload }),
    );
    if (!result) return undefined;

    if (id) {
      setSavedAt(new Date());
    } else {
      setFlash(created);
      router.replace(`${listHref}/${result.id}`);
    }

    return result;
  }

  async function remove(): Promise<string | null> {
    try {
      await adminFetch(`${apiPath}/${id}`, { method: "DELETE" });
      setFlash(deleted);
      router.push(listHref);
      return null;
    } catch (error) {
      return error instanceof ApiError ? error.message : "Não foi possível excluir. Tente de novo.";
    }
  }

  return {
    ...submit,
    save,
    remove,
    savedNote: savedAt ? `Salvo às ${timeFormat.format(savedAt)}.` : flash,
  };
}

/** Campo de texto opcional: vazio vira `null`, como a API espera. */
export const optional = (value: string | null | undefined) => (value?.trim() ? value.trim() : null);

/** Número digitado num `<input type="number">` (texto) para a API. */
export const toInt = (value: string) => (value.trim() === "" ? 0 : Number.parseInt(value, 10));
