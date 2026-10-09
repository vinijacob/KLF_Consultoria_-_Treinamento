import Link from "next/link";
import type { AnchorHTMLAttributes, ButtonHTMLAttributes } from "react";
import { cn } from "@/lib/cn";

type Variant = "primary" | "accent" | "outline" | "quiet";
type Size = "md" | "lg";

const base =
  "inline-flex items-center justify-center gap-2.5 border font-sans font-medium tracking-[0.02em] " +
  "transition-colors duration-150 disabled:cursor-not-allowed disabled:opacity-50 rounded-xs";

const variants: Record<Variant, string> = {
  primary:
    "border-brand bg-brand text-brand-contrast enabled:hover:bg-brand-deep enabled:hover:border-brand-deep",
  accent:
    "border-accent bg-accent text-accent-contrast enabled:hover:bg-accent-deep enabled:hover:border-accent-deep",
  outline:
    "border-ink bg-transparent text-ink enabled:hover:bg-ink enabled:hover:text-paper",
  quiet:
    "border-transparent bg-transparent text-brand underline decoration-1 underline-offset-4 enabled:hover:decoration-2",
};

const sizes: Record<Size, string> = {
  md: "min-h-11 px-5 py-2 text-[0.9375rem]",
  lg: "min-h-14 px-7 py-3 text-base",
};

type Common = { variant?: Variant; size?: Size };

export function buttonClasses({ variant = "primary", size = "md" }: Common = {}, className?: string) {
  return cn(base, variants[variant], sizes[size], className);
}

export function Button({
  variant,
  size,
  className,
  type = "button",
  ...props
}: Common & ButtonHTMLAttributes<HTMLButtonElement>) {
  return <button type={type} className={buttonClasses({ variant, size }, className)} {...props} />;
}

export function ButtonLink({
  variant,
  size,
  className,
  href,
  ...props
}: Common & Omit<AnchorHTMLAttributes<HTMLAnchorElement>, "href"> & { href: string }) {
  return <Link href={href} className={buttonClasses({ variant, size }, className)} {...props} />;
}
