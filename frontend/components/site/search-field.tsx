"use client";

import { useId, type InputHTMLAttributes, type ReactNode } from "react";
import { SearchIcon } from "@/components/ui/icons";
import { cn } from "@/lib/cn";

/**
 * Busca no estilo do site: caixa com lupa e texto serifado. A contagem de resultados é anunciada
 * a leitores de tela (`aria-live`). Funciona solta (filtro na hora) ou dentro de um `<form>` (busca na API).
 */
export function SearchField({
  label,
  value,
  onValueChange,
  onClear,
  status,
  className,
  ...props
}: {
  label: string;
  value: string;
  onValueChange?: (value: string) => void;
  onClear?: () => void;
  status?: ReactNode;
  className?: string;
} & Omit<InputHTMLAttributes<HTMLInputElement>, "value" | "onChange" | "className">) {
  const id = useId();

  return (
    <div role="search" className={className}>
      <label htmlFor={id} className="font-mono text-xs font-medium uppercase tracking-[0.14em] text-ink-soft">
        {label}
      </label>
      <div className="mt-2 flex items-center gap-3 rounded-xl border border-rule-strong bg-card px-4 transition-colors focus-within:border-ink hover:border-ink">
        <SearchIcon className="shrink-0 text-xl text-ink-soft" />
        <input
          id={id}
          type="search"
          autoComplete="off"
          spellCheck={false}
          maxLength={100}
          value={value}
          onChange={(event) => onValueChange?.(event.target.value)}
          className={cn(
            "min-h-14 w-full min-w-0 bg-transparent py-2 font-serif text-xl text-ink placeholder:text-ink-soft/70",
            "[&::-webkit-search-cancel-button]:hidden",
          )}
          {...props}
        />
        {value && onClear && (
          <button
            type="button"
            onClick={onClear}
            className="min-h-11 shrink-0 font-sans text-sm font-medium text-brand underline underline-offset-4"
          >
            Limpar
          </button>
        )}
      </div>
      <p aria-live="polite" className="mt-3 min-h-6 font-sans text-sm text-ink-soft">
        {status}
      </p>
    </div>
  );
}
