import type { HTMLAttributes } from "react";
import { cn } from "@/lib/cn";

/** Legenda em caixa-alta, tipo etiqueta de página de revista. */
export function Kicker({ className, ...props }: HTMLAttributes<HTMLParagraphElement>) {
  return (
    <p
      className={cn("font-mono text-xs font-medium uppercase tracking-[0.14em] text-ink-soft", className)}
      {...props}
    />
  );
}

/** Fio: simples (separador) ou duplo (cabeçalho de página, como em jornal). */
export function Rule({
  variant = "single",
  className,
  ...props
}: HTMLAttributes<HTMLHRElement> & { variant?: "single" | "double" | "strong" | "accent" }) {
  return (
    <hr
      className={cn(
        "w-full border-0",
        variant === "single" && "border-t border-rule",
        variant === "strong" && "border-t-2 border-ink",
        variant === "accent" && "border-t-[3px] border-accent",
        variant === "double" && "border-t-[5px] border-double border-ink",
        className,
      )}
      {...props}
    />
  );
}
