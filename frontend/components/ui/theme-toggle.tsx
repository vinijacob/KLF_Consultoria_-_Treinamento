"use client";

import { useSyncExternalStore } from "react";
import { cn } from "@/lib/cn";
import { THEME_STORAGE_KEY, type Theme } from "@/lib/theme";
import { MoonIcon, SunIcon } from "./icons";

const CHANGE_EVENT = "klf-theme-change";
const DARK_QUERY = "(prefers-color-scheme: dark)";

function subscribe(onChange: () => void) {
  const query = window.matchMedia(DARK_QUERY);
  query.addEventListener("change", onChange);
  window.addEventListener(CHANGE_EVENT, onChange);
  return () => {
    query.removeEventListener("change", onChange);
    window.removeEventListener(CHANGE_EVENT, onChange);
  };
}

function currentTheme(): Theme {
  const chosen = document.documentElement.dataset.theme;
  if (chosen === "light" || chosen === "dark") return chosen;
  return window.matchMedia(DARK_QUERY).matches ? "dark" : "light";
}

function applyTheme(theme: Theme) {
  document.documentElement.dataset.theme = theme;
  try {
    localStorage.setItem(THEME_STORAGE_KEY, theme);
  } catch {
    // Sem armazenamento (navegação privada): o tema vale só nesta página.
  }
  window.dispatchEvent(new Event(CHANGE_EVENT));
}

/**
 * Alterna entre o tema diurno e o noturno. Sem escolha, vale o do sistema; depois de escolher, a escolha fica guardada
 * neste navegador. O ícone mostra para onde o clique leva (lua = ir para o noturno).
 */
export function ThemeToggle({
  className,
  withLabel = false,
}: {
  className?: string;
  withLabel?: boolean;
}) {
  const theme = useSyncExternalStore<Theme | null>(
    subscribe,
    currentTheme,
    () => null,
  );
  const next: Theme = theme === "dark" ? "light" : "dark";
  const label = next === "dark" ? "Tema noturno" : "Tema diurno";
  const Icon = next === "dark" ? MoonIcon : SunIcon;

  return (
    <button
      type="button"
      onClick={() => applyTheme(next)}
      aria-label={withLabel ? undefined : `Mudar para o ${label.toLowerCase()}`}
      title={withLabel ? undefined : `Mudar para o ${label.toLowerCase()}`}
      className={cn(
        withLabel
          ? "inline-flex min-h-11 items-center gap-2 text-brand underline underline-offset-4 hover:cursor-pointer"
          : "inline-flex size-11 items-center justify-center rounded-md text-ink transition-colors hover:bg-paper-deep hover:cursor-pointer",
        theme === null && "invisible",
        className,
      )}
    >
      <Icon className={withLabel ? "text-base" : "text-xl"} />
      {withLabel && label}
    </button>
  );
}
