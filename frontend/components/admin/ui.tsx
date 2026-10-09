"use client";

import Link from "next/link";
import { useEffect, useId, useRef, useState, type FormEvent, type ReactNode } from "react";
import type { ApiError } from "@/lib/api/client";
import { cn } from "@/lib/cn";
import { Button, ButtonLink } from "@/components/ui/button";
import { Notice } from "@/components/ui/notice";
import { Kicker, Rule } from "@/components/ui/typography";

export function AdminHeader({
  kicker,
  title,
  description,
  actions,
  back,
}: {
  kicker: string;
  title: string;
  description?: ReactNode;
  actions?: ReactNode;
  back?: { href: string; label: string };
}) {
  return (
    <header className="mb-10">
      {back && (
        <Link href={back.href} className="mb-5 inline-flex min-h-11 items-center font-sans text-sm text-brand underline underline-offset-4">
          ← {back.label}
        </Link>
      )}
      <div className="flex flex-wrap items-end justify-between gap-x-8 gap-y-5">
        <div className="min-w-0 max-w-3xl">
          <Kicker>{kicker}</Kicker>
          <h1 className="mt-2 text-[2.5rem] leading-[1.05] sm:text-5xl">{title}</h1>
          {description && <div className="mt-3 font-sans text-[0.9375rem] leading-relaxed text-ink-soft">{description}</div>}
        </div>
        {actions && <div className="flex flex-wrap gap-3">{actions}</div>}
      </div>
      <Rule variant="double" className="mt-6" />
    </header>
  );
}

export function Loading({ label = "Carregando…" }: { label?: string }) {
  return (
    <div className="py-16" aria-busy="true">
      <Kicker role="status">{label}</Kicker>
    </div>
  );
}

export function LoadError({ error, onRetry }: { error: ApiError; onRetry?: () => void }) {
  const title =
    error.status === 403
      ? "Esta área não faz parte do seu perfil."
      : error.status === 404
        ? "Não encontramos este registro. Talvez tenha sido excluído."
        : "Não foi possível carregar.";

  return (
    <Notice tone="error" title={title} role="alert">
      {error.status !== 403 && error.status !== 404 && <p>{error.message}</p>}
      {onRetry && error.status !== 403 && error.status !== 404 && (
        <button type="button" onClick={onRetry} className="mt-2 min-h-11 font-medium text-brand underline underline-offset-4">
          Tentar de novo
        </button>
      )}
    </Notice>
  );
}

export function EmptyState({ title, children, action }: { title: string; children?: ReactNode; action?: ReactNode }) {
  return (
    <div className="border border-dashed border-rule-strong px-6 py-12 text-center">
      <p className="text-2xl">{title}</p>
      {children && <div className="mx-auto mt-2 max-w-md font-sans text-[0.9375rem] text-ink-soft">{children}</div>}
      {action && <div className="mt-6 flex justify-center">{action}</div>}
    </div>
  );
}

type Tone = "neutral" | "brand" | "success" | "warning" | "danger" | "accent";

const toneClasses: Record<Tone, string> = {
  neutral: "border-rule-strong text-ink-soft",
  brand: "border-brand text-brand",
  success: "border-success text-success",
  warning: "border-warning text-warning",
  danger: "border-danger text-danger",
  accent: "border-accent text-accent",
};

/** Etiqueta de estado em caixa-alta mono. O texto sempre diz o estado (a cor só reforça). */
export function Tag({ tone = "neutral", children }: { tone?: Tone; children: ReactNode }) {
  return (
    <span
      className={cn(
        "inline-flex items-center border px-1.5 py-0.5 font-mono text-[0.6875rem] leading-none font-medium uppercase tracking-[0.12em] whitespace-nowrap",
        toneClasses[tone],
      )}
    >
      {children}
    </span>
  );
}

/** Lista no estilo de sumário de revista: número, título, detalhes e etiquetas. */
export function IndexList({ children }: { children: ReactNode }) {
  return <ol className="border-t-2 border-ink">{children}</ol>;
}

export function IndexRow({
  href,
  number,
  title,
  meta,
  tags,
  aside,
}: {
  href: string;
  number?: number;
  title: string;
  meta?: ReactNode;
  tags?: ReactNode;
  aside?: ReactNode;
}) {
  return (
    <li className="border-b border-rule">
      <Link href={href} className="group flex items-start gap-4 py-4 sm:gap-6">
        {number !== undefined && (
          <span className="w-7 shrink-0 pt-1.5 font-mono text-xs text-ink-soft">{String(number).padStart(2, "0")}</span>
        )}
        {aside}
        <span className="min-w-0 flex-1">
          <span className="block text-xl leading-snug group-hover:text-brand group-hover:underline group-hover:decoration-1 group-hover:underline-offset-4">
            {title}
          </span>
          {meta && <span className="mt-1 block font-sans text-sm text-ink-soft">{meta}</span>}
        </span>
        {tags && <span className="flex shrink-0 flex-wrap justify-end gap-1.5 pt-1.5">{tags}</span>}
      </Link>
    </li>
  );
}

export function Section({
  title,
  description,
  children,
  className,
}: {
  title: string;
  description?: ReactNode;
  children: ReactNode;
  className?: string;
}) {
  const id = useId();

  return (
    <section aria-labelledby={id} className={cn("grid gap-6 border-t border-ink pt-6 pb-10 lg:grid-cols-[16rem_1fr] lg:gap-10", className)}>
      <div>
        <h2 id={id} className="text-2xl">
          {title}
        </h2>
        {description && <div className="mt-2 font-sans text-sm leading-relaxed text-ink-soft">{description}</div>}
      </div>
      <div className="grid min-w-0 content-start gap-6">{children}</div>
    </section>
  );
}

/** Formulário do painel com a barra de salvar no fim (fica presa ao rodapé da tela em formulários longos). */
export function AdminForm({
  onSubmit,
  children,
  pending,
  message,
  savedNote,
  submitLabel = "Salvar",
  extraActions,
}: {
  onSubmit: () => void;
  children: ReactNode;
  pending: boolean;
  message?: string | null;
  savedNote?: string | null;
  submitLabel?: string;
  extraActions?: ReactNode;
}) {
  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    onSubmit();
  }

  return (
    <form onSubmit={handleSubmit} noValidate>
      {children}
      <div className="sticky bottom-0 z-10 -mx-5 mt-2 border-t-2 border-ink bg-paper px-5 py-4 sm:-mx-8 sm:px-8 lg:-mx-12 lg:px-12">
        {message && (
          <Notice tone="error" role="alert" className="mb-4">
            {message}
          </Notice>
        )}
        <div className="flex flex-wrap items-center gap-x-5 gap-y-3">
          <Button type="submit" disabled={pending}>
            {pending ? "Salvando…" : submitLabel}
          </Button>
          {extraActions}
          {savedNote && !message && (
            <p role="status" className="font-sans text-sm text-success">
              {savedNote}
            </p>
          )}
        </div>
      </div>
    </form>
  );
}

/** Diálogo modal nativo (`<dialog>`): foco preso e Esc para fechar já vêm do navegador. */
export function Dialog({
  open,
  onClose,
  title,
  children,
  wide = false,
}: {
  open: boolean;
  onClose: () => void;
  title: string;
  children: ReactNode;
  wide?: boolean;
}) {
  const ref = useRef<HTMLDialogElement>(null);
  const titleId = useId();

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    if (!open && dialog.open) dialog.close();
  }, [open]);

  return (
    <dialog
      ref={ref}
      onClose={onClose}
      aria-labelledby={titleId}
      className={cn(
        "m-auto max-h-[90vh] w-[calc(100%-2rem)] overflow-y-auto border-2 border-ink bg-paper p-0 text-ink backdrop:bg-paper-deep",
        wide ? "max-w-5xl" : "max-w-lg",
      )}
    >
      {open && (
        <div className="p-6 sm:p-8">
          <div className="flex items-start justify-between gap-6">
            <h2 id={titleId} className="text-[1.75rem] leading-tight">
              {title}
            </h2>
            <button
              type="button"
              onClick={onClose}
              className="min-h-11 shrink-0 font-sans text-sm font-medium uppercase tracking-[0.12em] text-ink underline underline-offset-4"
            >
              Fechar
            </button>
          </div>
          <Rule variant="strong" className="mt-4 mb-6" />
          {children}
        </div>
      )}
    </dialog>
  );
}

/** Botão que pede confirmação antes de uma ação sem volta (excluir, revogar). */
export function ConfirmButton({
  label,
  title,
  description,
  confirmLabel,
  onConfirm,
  variant = "outline",
}: {
  label: string;
  title: string;
  description: ReactNode;
  confirmLabel: string;
  onConfirm: () => Promise<string | null | void>;
  variant?: "outline" | "quiet";
}) {
  const [open, setOpen] = useState(false);
  const [pending, setPending] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function confirm() {
    setPending(true);
    setError(null);
    const problem = await onConfirm();
    setPending(false);
    if (problem) setError(problem);
    else setOpen(false);
  }

  return (
    <>
      <Button
        variant={variant}
        className={variant === "outline" ? "border-danger text-danger enabled:hover:bg-danger enabled:hover:text-paper" : "text-danger"}
        onClick={() => {
          setError(null);
          setOpen(true);
        }}
      >
        {label}
      </Button>
      <Dialog open={open} onClose={() => setOpen(false)} title={title}>
        <div className="font-sans text-[0.9375rem] leading-relaxed">{description}</div>
        {error && (
          <Notice tone="error" role="alert" className="mt-5">
            {error}
          </Notice>
        )}
        <div className="mt-7 flex flex-wrap gap-3">
          <Button
            className="border-danger bg-danger text-paper enabled:hover:border-ink enabled:hover:bg-ink"
            disabled={pending}
            onClick={() => void confirm()}
          >
            {pending ? "Aguarde…" : confirmLabel}
          </Button>
          <Button variant="outline" onClick={() => setOpen(false)}>
            Cancelar
          </Button>
        </div>
      </Dialog>
    </>
  );
}

export function Pagination({ page, totalPages, onChange }: { page: number; totalPages: number; onChange: (page: number) => void }) {
  if (totalPages <= 1) return null;

  return (
    <nav aria-label="Páginas" className="mt-8 flex items-center justify-between gap-4 font-sans text-sm">
      <Button variant="outline" disabled={page <= 1} onClick={() => onChange(page - 1)}>
        Anterior
      </Button>
      <span className="font-mono text-xs uppercase tracking-[0.12em] text-ink-soft">
        Página {page} de {totalPages}
      </span>
      <Button variant="outline" disabled={page >= totalPages} onClick={() => onChange(page + 1)}>
        Próxima
      </Button>
    </nav>
  );
}

export function NewButton({ href, children }: { href: string; children: ReactNode }) {
  return <ButtonLink href={href}>{children}</ButtonLink>;
}

/** Grupo de dois campos lado a lado (empilha no celular). */
export function FieldRow({ children, className }: { children: ReactNode; className?: string }) {
  return <div className={cn("grid gap-6 sm:grid-cols-2", className)}>{children}</div>;
}

export function Stat({ label, value, note }: { label: string; value: ReactNode; note?: ReactNode }) {
  return (
    <div className="border-t-2 border-ink pt-3">
      <Kicker>{label}</Kicker>
      <p className="mt-1 text-5xl leading-none font-medium tracking-[-0.02em]">{value}</p>
      {note && <p className="mt-2 font-sans text-sm text-ink-soft">{note}</p>}
    </div>
  );
}
