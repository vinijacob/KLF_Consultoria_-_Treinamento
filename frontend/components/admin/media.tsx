"use client";

import Image from "next/image";
import { useId, useRef, useState, type FormEvent } from "react";
import { useApi, useSubmit } from "@/lib/admin/hooks";
import { formatBytes } from "@/lib/admin/text";
import { adminFetch } from "@/lib/auth/session";
import { fieldError } from "@/lib/api/client";
import { mediaUrl } from "@/lib/media";
import { cn } from "@/lib/cn";
import type { MediaAsset, Paged } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { FieldError, Input } from "@/components/ui/field";
import { AlertIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { Dialog, Pagination, Tag } from "./ui";

export const ACCEPTED_TYPES = ["image/jpeg", "image/png", "image/webp"];
export const MAX_BYTES = 5 * 1024 * 1024;

/** Confere tipo e tamanho antes de enviar (a API confere de novo pelo conteúdo do arquivo). */
export function checkFile(file: File) {
  if (!ACCEPTED_TYPES.includes(file.type)) return "Envie uma imagem JPEG, PNG ou WebP.";
  if (file.size > MAX_BYTES) return `A imagem tem ${formatBytes(file.size)}; o limite é 5 MB.`;
  return null;
}

export async function uploadMedia(file: File, altText: string) {
  const body = new FormData();
  body.append("file", file);
  if (altText.trim()) body.append("altText", altText.trim());

  return adminFetch<MediaAsset>("/admin/media", { method: "POST", body });
}

/** Miniatura em moldura fina, como no site. */
export function Thumb({ src, alt, className }: { src: string; alt: string; className?: string }) {
  return (
    <div className={cn("border border-ink p-1", className)}>
      <div className="relative aspect-[4/3] overflow-hidden bg-paper-deep">
        <Image src={src} alt={alt} fill unoptimized sizes="240px" className="object-cover" />
      </div>
    </div>
  );
}

/** Envio de imagem com texto alternativo (LGPD/acessibilidade: o painel pede o `alt` já no envio). */
export function UploadForm({ onUploaded }: { onUploaded: (asset: MediaAsset) => void }) {
  const fileId = useId();
  const inputRef = useRef<HTMLInputElement>(null);
  const [file, setFile] = useState<File | null>(null);
  const [fileProblem, setFileProblem] = useState<string | null>(null);
  const [altText, setAltText] = useState("");
  const { pending, errors, message, run } = useSubmit();

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (!file) {
      setFileProblem("Escolha uma imagem.");
      return;
    }

    const asset = await run(() => uploadMedia(file, altText));
    if (asset) {
      setFile(null);
      setAltText("");
      if (inputRef.current) inputRef.current.value = "";
      onUploaded(asset);
    }
  }

  return (
    <form onSubmit={submit} noValidate className="grid gap-5 border border-rule-strong bg-card p-5">
      <div>
        <label htmlFor={fileId} className="mb-1.5 block font-sans text-sm font-medium text-ink">
          Imagem
        </label>
        <p className="mb-2 font-sans text-sm text-ink-soft">JPEG, PNG ou WebP, até 5 MB. Prefira fotos com autorização de uso.</p>
        <input
          ref={inputRef}
          id={fileId}
          type="file"
          accept={ACCEPTED_TYPES.join(",")}
          aria-invalid={fileProblem || fieldError(errors, "file") ? true : undefined}
          onChange={(event) => {
            const chosen = event.target.files?.[0] ?? null;
            setFile(chosen);
            setFileProblem(chosen ? checkFile(chosen) : null);
          }}
          className="block w-full font-sans text-sm file:mr-4 file:min-h-11 file:cursor-pointer file:rounded-xs file:border file:border-ink file:bg-transparent file:px-4 file:font-sans file:text-sm file:font-medium file:text-ink hover:file:bg-ink hover:file:text-paper"
        />
        <FieldError>{fileProblem ?? fieldError(errors, "file")}</FieldError>
      </div>
      <Input
        label="Texto alternativo"
        hint="Descreva a imagem para quem usa leitor de tela. Ex.: “Kilciene conduz treinamento para equipe de loja”."
        value={altText}
        maxLength={200}
        onChange={(event) => setAltText(event.target.value)}
        error={fieldError(errors, "altText")}
      />
      {message && (
        <Notice tone="error" role="alert">
          {message}
        </Notice>
      )}
      <div>
        <Button type="submit" disabled={pending || Boolean(fileProblem)}>
          {pending ? "Enviando…" : "Enviar imagem"}
        </Button>
      </div>
    </form>
  );
}

export function MediaPicker({ onPick, picked = [] }: { onPick: (asset: MediaAsset) => void; picked?: string[] }) {
  const [page, setPage] = useState(1);
  const { data, error, reload } = useApi<Paged<MediaAsset>>(`/admin/media?page=${page}&pageSize=24`);

  return (
    <div className="grid gap-8">
      <UploadForm
        onUploaded={(asset) => {
          reload();
          onPick(asset);
        }}
      />
      <div>
        <h3 className="mb-4 text-xl">Ou escolha da biblioteca</h3>
        {error && <Notice tone="error">{error.message}</Notice>}
        {data && data.items.length === 0 && <p className="font-sans text-sm text-ink-soft">Nenhuma imagem enviada ainda.</p>}
        <ul className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
          {data?.items.map((asset) => (
            <li key={asset.id}>
              <button
                type="button"
                onClick={() => onPick(asset)}
                disabled={picked.includes(asset.id)}
                className="group block w-full text-left disabled:cursor-default"
                aria-label={`Escolher ${asset.altText || asset.originalFileName}`}
              >
                <Thumb
                  src={asset.url}
                  alt=""
                  className={cn(
                    "group-enabled:group-hover:outline-2 group-enabled:group-hover:outline-brand",
                    picked.includes(asset.id) && "border-brand outline-2 outline-brand",
                  )}
                />
                <span className="mt-1.5 block truncate font-sans text-xs text-ink-soft">
                  {picked.includes(asset.id) ? (
                    <span className="font-medium text-brand">Já está no álbum</span>
                  ) : (
                    asset.altText || <span className="text-warning">Sem texto alternativo</span>
                  )}
                </span>
              </button>
            </li>
          ))}
        </ul>
        {data && <Pagination page={page} totalPages={data.totalPages} onChange={setPage} />}
      </div>
    </div>
  );
}

/** Campo de imagem (capa, logo, foto): mostra a escolhida e abre a biblioteca para trocar. */
export function MediaField({
  label,
  hint,
  value,
  onChange,
  error,
  optional = true,
}: {
  label: string;
  hint?: string;
  value: string | null | undefined;
  onChange: (id: string | null) => void;
  error?: string;
  optional?: boolean;
}) {
  const [open, setOpen] = useState(false);
  const [missingAlt, setMissingAlt] = useState(false);

  return (
    <div>
      <p className="mb-1.5 font-sans text-sm font-medium text-ink">
        {label}
        {optional && <span className="ml-2 font-normal text-ink-soft">(opcional)</span>}
      </p>
      {hint && <p className="mb-2 font-sans text-sm text-ink-soft">{hint}</p>}
      <div className="flex flex-wrap items-start gap-5">
        {value ? (
          <Thumb src={mediaUrl(value)} alt="" className="w-44" />
        ) : (
          <div className="grid aspect-[4/3] w-44 place-items-center border border-dashed border-rule-strong">
            <span className="font-mono text-[0.6875rem] uppercase tracking-[0.14em] text-ink-soft">Sem imagem</span>
          </div>
        )}
        <div className="flex flex-col items-start gap-2">
          <Button variant="outline" onClick={() => setOpen(true)}>
            {value ? "Trocar imagem" : "Escolher imagem"}
          </Button>
          {value && (
            <Button
              variant="quiet"
              onClick={() => {
                onChange(null);
                setMissingAlt(false);
              }}
            >
              Remover
            </Button>
          )}
          {missingAlt && value && (
            <p className="flex max-w-xs items-start gap-1.5 font-sans text-sm text-warning">
              <AlertIcon className="mt-0.5 shrink-0" />
              Esta imagem não tem texto alternativo. Complete em Imagens.
            </p>
          )}
        </div>
      </div>
      <FieldError>{error}</FieldError>
      <Dialog open={open} onClose={() => setOpen(false)} title={label} wide>
        <MediaPicker
          onPick={(asset) => {
            onChange(asset.id);
            setMissingAlt(!asset.altText);
            setOpen(false);
          }}
        />
      </Dialog>
    </div>
  );
}

export function AltTextTag({ altText }: { altText?: string | null }) {
  return altText ? null : <Tag tone="warning">Sem texto alternativo</Tag>;
}
