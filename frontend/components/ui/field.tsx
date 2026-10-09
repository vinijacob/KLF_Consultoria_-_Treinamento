"use client";

import { useId, type InputHTMLAttributes, type ReactNode, type TextareaHTMLAttributes } from "react";
import { cn } from "@/lib/cn";
import { AlertIcon } from "./icons";

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
