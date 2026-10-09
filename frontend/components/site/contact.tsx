import Link from "next/link";
import { ButtonLink, buttonClasses } from "@/components/ui/button";
import { Kicker } from "@/components/ui/typography";
import { Container } from "@/components/site/blocks";
import { copy } from "@/content/site-copy";
import { phoneHref, whatsappLink } from "@/lib/format";
import type { ContactSettings } from "@/types/site";

/** Faixa final das páginas: convite para conversar. Usa o acento (uma vez por tela). */
export function ClosingCta({ contact, message }: { contact?: ContactSettings; message?: string }) {
  const whatsapp = contact?.whatsapp;

  return (
    <section aria-labelledby="cta-title" className="-mb-24 mt-24 border-t border-ink bg-paper-deep">
      <Container className="grid gap-8 py-16 md:grid-cols-12 md:items-end">
        <div className="md:col-span-8">
          <Kicker>{copy.contact.kicker}</Kicker>
          <h2 id="cta-title" className="mt-3 text-[clamp(2rem,5vw,3.5rem)] leading-[1.04]">
            {copy.contact.title}
          </h2>
          <p className="mt-4 max-w-xl text-lg leading-relaxed text-ink-soft">{copy.contact.intro}</p>
        </div>
        <div className="flex flex-col gap-3 md:col-span-4 md:items-end">
          {whatsapp ? (
            <a
              href={whatsappLink(whatsapp, message ?? copy.contact.whatsappMessage)}
              rel="noopener"
              className={buttonClasses({ variant: "accent", size: "lg" }, "w-full md:w-auto")}
            >
              Conversar pelo WhatsApp
            </a>
          ) : (
            <ButtonLink href="/contato" variant="accent" size="lg" className="w-full md:w-auto">
              Ver formas de contato
            </ButtonLink>
          )}
          {whatsapp && (
            <Link href="/contato" className="font-sans text-sm text-brand underline underline-offset-4">
              Outras formas de contato
            </Link>
          )}
        </div>
      </Container>
    </section>
  );
}

/** Canais de contato em lista, cada um com o que fazer (abrir conversa, escrever, ligar). */
export function ContactChannels({ contact }: { contact: ContactSettings }) {
  const channels = [
    contact.whatsapp && {
      label: "WhatsApp",
      value: "Abrir conversa",
      href: whatsappLink(contact.whatsapp, copy.contact.whatsappMessage),
    },
    contact.email && { label: "E-mail", value: contact.email, href: `mailto:${contact.email}` },
    contact.phone && { label: "Telefone", value: contact.phone, href: phoneHref(contact.phone) },
    contact.address && { label: "Endereço", value: contact.address, href: contact.mapUrl ?? undefined },
  ].filter(Boolean) as { label: string; value: string; href?: string }[];

  if (channels.length === 0) {
    return <p className="font-sans text-base text-ink-soft">Os canais de contato serão publicados em breve.</p>;
  }

  return (
    <dl className="border-t border-ink">
      {channels.map((channel) => (
        <div key={channel.label} className="grid gap-2 border-b border-rule py-6 sm:grid-cols-[10rem_1fr] sm:items-baseline">
          <dt>
            <Kicker>{channel.label}</Kicker>
          </dt>
          <dd className="font-serif text-[1.625rem] leading-snug break-words">
            {channel.href ? (
              <a
                href={channel.href}
                rel="noopener"
                target={channel.label === "Endereço" ? "_blank" : undefined}
                className="underline decoration-1 underline-offset-[6px] hover:decoration-2"
              >
                {channel.value}
                {channel.label === "Endereço" && <span className="sr-only"> (abre o mapa em nova aba)</span>}
              </a>
            ) : (
              channel.value
            )}
          </dd>
        </div>
      ))}
    </dl>
  );
}
