import { siteConfig } from "@/config/site";

export function SiteHeader() {
  return (
    <header className="mx-auto flex w-full max-w-6xl items-center justify-between px-6 py-6">
      <div className="flex items-center gap-3">
        <span className="grid size-10 place-items-center rounded-full bg-brand font-display text-sm font-semibold tracking-wide text-background">
          {siteConfig.shortName}
        </span>
        <span className="hidden text-sm font-medium text-muted sm:block">
          {siteConfig.name}
        </span>
      </div>
      <span className="rounded-full border border-border bg-surface px-3 py-1 text-xs font-medium text-muted backdrop-blur">
        Em breve
      </span>
    </header>
  );
}
