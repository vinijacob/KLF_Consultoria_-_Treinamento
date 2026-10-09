"use client";

import Image from "next/image";
import { useState } from "react";
import { formatShortDate } from "@/lib/admin/datetime";
import { useApi, useSubmit } from "@/lib/admin/hooks";
import { formatBytes, plural } from "@/lib/admin/text";
import { adminFetch } from "@/lib/auth/session";
import { ApiError, fieldError } from "@/lib/api/client";
import type { MediaAsset, Paged } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Checkbox, Input } from "@/components/ui/field";
import { Notice } from "@/components/ui/notice";
import { AltTextTag, Thumb, UploadForm } from "../media";
import { AdminHeader, ConfirmButton, Dialog, EmptyState, LoadError, Loading, Pagination, Section } from "../ui";

function AssetDialog({ asset, onClose, onChanged }: { asset: MediaAsset; onClose: () => void; onChanged: () => void }) {
  const [altText, setAltText] = useState(asset.altText ?? "");
  const [saved, setSaved] = useState(false);
  const { pending, errors, message, run } = useSubmit();

  async function save() {
    setSaved(false);
    const result = await run(() => adminFetch<MediaAsset>(`/admin/media/${asset.id}`, { method: "PUT", json: { altText: altText.trim() || null } }));
    if (result) {
      setSaved(true);
      onChanged();
    }
  }

  async function remove() {
    try {
      await adminFetch(`/admin/media/${asset.id}`, { method: "DELETE" });
      onChanged();
      onClose();
      return null;
    } catch (error) {
      return error instanceof ApiError ? error.message : "Não foi possível excluir.";
    }
  }

  return (
    <Dialog open onClose={onClose} title={asset.originalFileName} wide>
      <div className="grid gap-8 md:grid-cols-[1.2fr_1fr]">
        <div className="border border-ink p-2">
          <div className="relative aspect-[4/3] bg-paper-deep">
            <Image src={asset.url} alt={asset.altText ?? ""} fill unoptimized sizes="600px" className="object-contain" />
          </div>
        </div>
        <div className="grid content-start gap-5">
          <dl className="grid grid-cols-[auto_1fr] gap-x-4 gap-y-1 font-sans text-sm">
            <dt className="text-ink-soft">Tipo</dt>
            <dd>{asset.contentType.replace("image/", "").toUpperCase()}</dd>
            <dt className="text-ink-soft">Tamanho</dt>
            <dd>{formatBytes(asset.sizeBytes)}</dd>
            <dt className="text-ink-soft">Enviada em</dt>
            <dd>{formatShortDate(asset.createdAt)}</dd>
          </dl>
          <form
            className="grid gap-4"
            onSubmit={(event) => {
              event.preventDefault();
              void save();
            }}
          >
            <Input
              label="Texto alternativo"
              hint="O que a imagem mostra, para quem usa leitor de tela."
              value={altText}
              maxLength={200}
              onChange={(event) => {
                setSaved(false);
                setAltText(event.target.value);
              }}
              error={fieldError(errors, "altText")}
            />
            {message && <Notice tone="error">{message}</Notice>}
            <div className="flex flex-wrap items-center gap-3">
              <Button type="submit" disabled={pending}>
                {pending ? "Salvando…" : "Salvar texto"}
              </Button>
              {saved && (
                <p role="status" className="font-sans text-sm text-success">
                  Salvo.
                </p>
              )}
            </div>
          </form>
          <div className="border-t border-rule pt-5">
            <ConfirmButton
              label="Excluir imagem"
              title="Excluir esta imagem?"
              description={<p>Se ela estiver em uso (capa, logo, foto ou galeria), a exclusão é recusada: troque a imagem lá antes.</p>}
              confirmLabel="Excluir"
              onConfirm={remove}
            />
          </div>
        </div>
      </div>
    </Dialog>
  );
}

export function MediaLibrary({ onlyMissingAlt }: { onlyMissingAlt: boolean }) {
  const [page, setPage] = useState(1);
  const [missingOnly, setMissingOnly] = useState(onlyMissingAlt);
  const [selected, setSelected] = useState<MediaAsset | null>(null);
  const [uploaded, setUploaded] = useState<string | null>(null);
  const { data, error, reload } = useApi<Paged<MediaAsset>>(`/admin/media?page=${page}&pageSize=48`);
  const items = data?.items.filter((asset) => !missingOnly || !asset.altText) ?? [];
  const missingCount = data?.items.filter((asset) => !asset.altText).length ?? 0;

  return (
    <div>
      <AdminHeader
        kicker="Conteúdo do site"
        title="Imagens"
        description="Biblioteca usada nas capas, logos, fotos e na galeria. Toda imagem precisa de texto alternativo (acessibilidade)."
      />
      <Section title="Enviar" description="O tipo do arquivo é conferido pelo conteúdo, não pelo nome.">
        <UploadForm
          onUploaded={(asset) => {
            setUploaded(asset.originalFileName);
            setPage(1);
            reload();
          }}
        />
        {uploaded && (
          <Notice tone="success" role="status">
            “{uploaded}” enviada.
          </Notice>
        )}
      </Section>

      <Section
        title="Biblioteca"
        description={data && plural(data.totalItems, "imagem no total", "imagens no total")}
      >
        <Checkbox
          label={`Só imagens sem texto alternativo${missingCount ? ` (${missingCount} nesta página)` : ""}`}
          checked={missingOnly}
          onChange={(event) => setMissingOnly(event.target.checked)}
        />
        {error && <LoadError error={error} onRetry={reload} />}
        {!data && !error && <Loading />}
        {data && items.length === 0 && (
          <EmptyState title={missingOnly ? "Todas com texto alternativo" : "Nenhuma imagem ainda"}>
            {missingOnly ? "Nada a completar nesta página." : "Envie a primeira imagem acima."}
          </EmptyState>
        )}
        <ul className="grid grid-cols-2 gap-x-5 gap-y-7 sm:grid-cols-3 xl:grid-cols-4">
          {items.map((asset) => (
            <li key={asset.id}>
              <button type="button" onClick={() => setSelected(asset)} className="group block w-full text-left" aria-label={`Abrir ${asset.originalFileName}`}>
                <Thumb src={asset.url} alt="" className="group-hover:outline-2 group-hover:outline-brand" />
                <span className="mt-2 block truncate font-sans text-sm">{asset.altText || asset.originalFileName}</span>
                <span className="mt-1 flex flex-wrap items-center gap-2 font-sans text-xs text-ink-soft">
                  {formatBytes(asset.sizeBytes)}
                  <AltTextTag altText={asset.altText} />
                </span>
              </button>
            </li>
          ))}
        </ul>
        {data && <Pagination page={page} totalPages={data.totalPages} onChange={setPage} />}
      </Section>

      {selected && <AssetDialog key={selected.id} asset={selected} onClose={() => setSelected(null)} onChanged={reload} />}
    </div>
  );
}
