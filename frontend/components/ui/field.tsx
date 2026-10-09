"use client";

import {
  useId,
  type InputHTMLAttributes,
  type ReactNode,
  type SelectHTMLAttributes,
  type TextareaHTMLAttributes,
} from "react";
import { cn } from "@/lib/cn";
import { AlertIcon, CheckIcon, ChevronDownIcon } from "./icons";

export const controlClasses =
  "w-full rounded-xs border border-rule-strong bg-card px-3.5 py-3 font-sans text-base text-ink " +
  "placeholder:text-ink-soft/70 transition-colors hover:border-ink aria-[invalid=true]:border-danger aria-[invalid=true]:border-2";

export function FieldError({ id, children }: { id?: string; children?: ReactNode }) {
  if (!children) return null;

  return (
    <p id={id} className="mt-2 flex items-start gap-1.5 font-sans text-sm font-medium text-danger">
      <AlertIcon className="mt-0.5 shrink-0 text-base" />
      <span>{children}</span>
    </p>
  );
}

type FieldProps = {
  label: string;
  hint?: string;
  error?: string;
  optional?: boolean;
};

export function Input({
  label,
  hint,
  error,
  optional,
  className,
  ...props
}: FieldProps & InputHTMLAttributes<HTMLInputElement>) {
  const id = useId();
  const describedBy = [hint && `${id}-hint`, error && `${id}-error`].filter(Boolean).join(" ") || undefined;

  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block font-sans text-sm font-medium text-ink">
        {label}
        {optional && <span className="ml-2 font-normal text-ink-soft">(opcional)</span>}
      </label>
      {hint && (
        <p id={`${id}-hint`} className="mb-2 font-sans text-sm text-ink-soft">
          {hint}
        </p>
      )}
      <input
        id={id}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        className={cn(controlClasses, className)}
        {...props}
      />
      <FieldError id={`${id}-error`}>{error}</FieldError>
    </div>
  );
}

export function Textarea({
  label,
  hint,
  error,
  optional,
  className,
  ...props
}: FieldProps & TextareaHTMLAttributes<HTMLTextAreaElement>) {
  const id = useId();
  const describedBy = [hint && `${id}-hint`, error && `${id}-error`].filter(Boolean).join(" ") || undefined;

  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block font-sans text-sm font-medium text-ink">
        {label}
        {optional && <span className="ml-2 font-normal text-ink-soft">(opcional)</span>}
      </label>
      {hint && (
        <p id={`${id}-hint`} className="mb-2 font-sans text-sm text-ink-soft">
          {hint}
        </p>
      )}
      <textarea
        id={id}
        aria-invalid={error ? true : undefined}
        aria-describedby={describedBy}
        className={cn(controlClasses, "min-h-32 resize-y leading-relaxed", className)}
        {...props}
      />
      <FieldError id={`${id}-error`}>{error}</FieldError>
    </div>
  );
}

export function Select({
  label,
  hint,
  error,
  optional,
  className,
  children,
  ...props
}: FieldProps & SelectHTMLAttributes<HTMLSelectElement>) {
  const id = useId();
  const describedBy = [hint && `${id}-hint`, error && `${id}-error`].filter(Boolean).join(" ") || undefined;

  return (
    <div>
      <label htmlFor={id} className="mb-1.5 block font-sans text-sm font-medium text-ink">
        {label}
        {optional && <span className="ml-2 font-normal text-ink-soft">(opcional)</span>}
      </label>
      {hint && (
        <p id={`${id}-hint`} className="mb-2 font-sans text-sm text-ink-soft">
          {hint}
        </p>
      )}
      <div className="relative">
        <select
          id={id}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          className={cn(controlClasses, "appearance-none pr-10", className)}
          {...props}
        >
          {children}
        </select>
        <ChevronDownIcon className="pointer-events-none absolute top-1/2 right-3.5 -translate-y-1/2 text-ink-soft" />
      </div>
      <FieldError id={`${id}-error`}>{error}</FieldError>
    </div>
  );
}

/** Caixa de seleção isolada (ligar/desligar algo), com o mesmo desenho da múltipla escolha. */
export function Checkbox({
  label,
  hint,
  error,
  className,
  ...props
}: { label: ReactNode; hint?: string; error?: string } & Omit<InputHTMLAttributes<HTMLInputElement>, "type">) {
  const id = useId();

  return (
    <div className={className}>
      <label
        htmlFor={id}
        className={cn(
          "flex min-h-11 cursor-pointer items-start gap-3 font-sans text-[0.9375rem] text-ink",
          "has-focus-visible:outline-2 has-focus-visible:outline-offset-2 has-focus-visible:outline-ring",
        )}
      >
        <input id={id} type="checkbox" className="peer sr-only" aria-describedby={hint ? `${id}-hint` : undefined} {...props} />
        <span className="mt-0.5 grid size-5 shrink-0 place-items-center rounded-xs border border-rule-strong bg-card text-brand-contrast peer-checked:border-brand peer-checked:bg-brand peer-disabled:opacity-50 *:scale-0 peer-checked:*:scale-100">
          <CheckIcon className="text-sm transition-transform" />
        </span>
        <span>
          <span className="font-medium">{label}</span>
          {hint && (
            <span id={`${id}-hint`} className="mt-0.5 block text-sm text-ink-soft">
              {hint}
            </span>
          )}
        </span>
      </label>
      <FieldError>{error}</FieldError>
    </div>
  );
}
