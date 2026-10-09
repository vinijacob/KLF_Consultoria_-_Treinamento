import Link from "next/link";
import { copy } from "@/content/site-copy";
import { siteConfig } from "@/config/site";
import { phoneHref, whatsappLink } from "@/lib/format";
import type { SiteSettings } from "@/types/site";

function Column({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <div>
      <p className="font-mono text-xs uppercase tracking-[0.14em] text-footer-ink/70">{title}</p>
      <ul className="mt-4 space-y-2.5 font-sans text-[0.9375rem]">{children}</ul>
    </div>
  );
}

const linkClass = "underline-offset-4 hover:underline";

export function SiteFooter({ settings }: { settings: SiteSettings }) {
  const contact = settings.contact ?? {};
  const social = settings.social ?? {};
  const socials = [
    ["Instagram", social.instagram],
    ["LinkedIn", social.linkedin],
    ["Facebook", social.facebook],
    ["YouTube", social.youtube],
  ].filter((entry): entry is [string, string] => !!entry[1]);

  return (
    <footer className="bg-footer text-footer-ink">
      <div className="mx-auto w-full max-w-6xl px-5 py-14 sm:px-8">
        <p className="max-w-3xl font-serif text-[clamp(2rem,5vw,3.25rem)] leading-[1.05]">{siteConfig.name}</p>
        <p className="mt-3 max-w-xl font-sans text-base text-footer-ink/80">{copy.footer.tagline}</p>

        <div className="mt-12 grid gap-10 border-t border-footer-ink/30 pt-10 sm:grid-cols-2 lg:grid-cols-4">
          <Column title="Navegação">
            {siteConfig.nav.map((item) => (
              <li key={item.href}>
                <Link href={item.href} className={linkClass}>
                  {item.label}
                </Link>
              </li>
            ))}
          </Column>

          <Column title="Contato">
            {contact.whatsapp && (
              <li>
                <a href={whatsappLink(contact.whatsapp, copy.contact.whatsappMessage)} className={linkClass} rel="noopener">
                  WhatsApp
                </a>
              </li>
            )}
            {contact.email && (
              <li>
                <a href={`mailto:${contact.email}`} className={`${linkClass} break-all`}>
                  {contact.email}
                </a>
              </li>
            )}
            {contact.phone && (
              <li>
                <a href={phoneHref(contact.phone)} className={linkClass}>
                  {contact.phone}
                </a>
              </li>
            )}
            {contact.address && <li className="text-footer-ink/80">{contact.address}</li>}
            {!contact.whatsapp && !contact.email && !contact.phone && (
              <li>
                <Link href="/contato" className={linkClass}>
                  Fale conosco
                </Link>
              </li>
            )}
          </Column>

          <Column title="Redes">
            {socials.length === 0 && <li className="text-footer-ink/70">Em breve</li>}
            {socials.map(([label, href]) => (
              <li key={label}>
                <a href={href} className={linkClass} rel="noopener noreferrer" target="_blank">
                  {label}
                  <span className="sr-only"> (abre em nova aba)</span>
                </a>
              </li>
            ))}
          </Column>

          <Column title="Informações">
            <li>
              <Link href="/privacidade" className={linkClass}>
                Política de privacidade
              </Link>
            </li>
            <li>
              <Link href="/termos" className={linkClass}>
                Termos de uso
              </Link>
            </li>
          </Column>
        </div>

        <div className="mt-12 flex flex-col gap-2 border-t border-footer-ink/30 pt-6 font-mono text-xs uppercase tracking-[0.12em] text-footer-ink/70 sm:flex-row sm:justify-between">
          <span>
            © {new Date().getFullYear()} {siteConfig.name}
          </span>
          <span>{copy.footer.colophon}</span>
        </div>
      </div>
    </footer>
  );
}
