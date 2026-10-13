import Link from "next/link";
import { ButtonLink, buttonClasses } from "@/components/ui/button";
import { Kicker } from "@/components/ui/typography";
import { Container } from "@/components/site/blocks";
import { copy } from "@/content/site-copy";
import { phoneHref, whatsappLink } from "@/lib/format";
import type { ContactSettings } from "@/types/site";

/** Caixa final das páginas: convite para conversar, em azul-marinho. Usa o acento no botão (uma vez por tela). */
export function ClosingCta({ contact, message }: { contact?: ContactSettings; message?: string }) {
  const whatsapp = contact?.whatsapp;

  return (
    <Container className="mt-24">
      <section aria-labelledby="cta-title" className="reveal grid gap-8 rounded-3xl bg-brand px-7 py-12 text-brand-contrast sm:px-12 sm:py-16 md:grid-cols-12 md:items-end">
        <div className="md:col-span-8">
          <p className="font-mono text-xs font-medium uppercase tracking-[0.14em]">{copy.contact.kicker}</p>
          <h2 id="cta-title" className="mt-3 text-[clamp(2rem,5vw,3.5rem)] leading-[1.04]">
            {copy.contact.title}
          </h2>
          <p className="mt-4 max-w-xl text-lg leading-relaxed">{copy.contact.intro}</p>
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
            <Link href="/contato" className="inline-flex min-h-11 items-center font-sans text-sm underline underline-offset-4">
              Outras formas de contato
            </Link>
          )}
        </div>
      </section>
    </Container>
  );
}

/** Canais de contato em caixas, cada um com o que fazer (abrir conversa, escrever, ligar). */
export function ContactChannels({ contact, stacked = false }: { contact: ContactSettings; stacked?: boolean }) {
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
    <dl className={stacked ? "grid gap-4" : "grid gap-4 sm:grid-cols-2"}>
      {channels.map((channel) => (
        <div key={channel.label} className="reveal rounded-2xl border border-rule bg-card p-6">
          <dt>
            <Kicker>{channel.label}</Kicker>
          </dt>
          <dd className={`mt-2 font-serif leading-snug break-words ${stacked ? "text-xl" : "text-[1.5rem]"}`}>
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
