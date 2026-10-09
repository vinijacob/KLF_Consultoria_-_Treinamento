import type { ReactNode } from "react";
import { cn } from "@/lib/cn";
import { AlertIcon, CheckIcon, ClockIcon, LockIcon } from "./icons";

type Tone = "info" | "error" | "success" | "wait" | "privacy";

const tones: Record<Tone, { box: string; icon: ReactNode }> = {
  info: { box: "border-brand bg-brand-soft", icon: <ClockIcon /> },
  wait: { box: "border-warning bg-paper-deep", icon: <ClockIcon /> },
  error: { box: "border-danger bg-card", icon: <AlertIcon /> },
  success: { box: "border-success bg-card", icon: <CheckIcon /> },
  privacy: { box: "border-brand bg-brand-soft", icon: <LockIcon /> },
};

const iconColor: Record<Tone, string> = {
  info: "text-brand",
  wait: "text-warning",
  error: "text-danger",
  success: "text-success",
  privacy: "text-brand",
};

/** Aviso com filete grosso à esquerda; o ícone sempre acompanha a cor (não só a cor informa). */
export function Notice({
  tone = "info",
  title,
  children,
  className,
  role,
}: {
  tone?: Tone;
  title?: string;
  children?: ReactNode;
  className?: string;
  role?: "alert" | "status";
}) {
  const { box, icon } = tones[tone];

  return (
    <div role={role} className={cn("flex gap-3 border border-l-[5px] px-4 py-3.5", box, className)}>
      <span className={cn("mt-1 shrink-0 text-xl", iconColor[tone])}>{icon}</span>
      <div className="min-w-0 font-sans text-[0.9375rem] leading-relaxed">
        {title && <p className="font-semibold text-ink">{title}</p>}
        {children && <div className={cn("text-ink", title && "mt-0.5")}>{children}</div>}
      </div>
    </div>
  );
}
