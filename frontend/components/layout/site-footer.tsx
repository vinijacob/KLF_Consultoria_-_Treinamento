import { siteConfig } from "@/config/site";

export function SiteFooter() {
  return (
    <footer className="mx-auto w-full max-w-6xl px-6 py-8 text-center text-xs text-muted">
      © {new Date().getFullYear()} {siteConfig.name}. Todos os direitos
      reservados.
    </footer>
  );
}
