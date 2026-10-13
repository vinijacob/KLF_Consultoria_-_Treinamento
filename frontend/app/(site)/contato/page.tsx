import type { Metadata } from "next";
import { Container, PageHeader } from "@/components/site/blocks";
import { ContactChannels } from "@/components/site/contact";
import { ContactForm } from "@/components/site/contact-form";
import { Kicker } from "@/components/ui/typography";
import { copy } from "@/content/site-copy";
import { getServices, getSettings } from "@/lib/api/public";

export const metadata: Metadata = {
  title: "Contato",
  description: copy.contact.intro,
};

export default async function ContactPage({ searchParams }: PageProps<"/contato">) {
  const [{ contact }, services, { servico }] = await Promise.all([getSettings(), getServices(), searchParams]);
  const chosen = services.find((service) => service.slug === servico);

  return (
    <>
      <PageHeader kicker={copy.contact.kicker} title={copy.contact.title} intro={copy.contact.intro} />
      <Container className="mt-12 grid items-start gap-12 lg:grid-cols-12">
        <div id="formulario" className="lg:col-span-7">
          <ContactForm services={services.map(({ id, title }) => ({ id, title }))} initialServiceId={chosen?.id} />
        </div>
        <aside className="grid gap-6 lg:col-span-5">
          <div>
            <Kicker>Prefere falar direto?</Kicker>
            <div className="mt-4">
              <ContactChannels contact={contact ?? {}} stacked />
            </div>
          </div>
          <div className="rounded-2xl bg-paper-deep p-7">
            <Kicker>Atendimento</Kicker>
            <p className="mt-3 text-lg leading-relaxed">{copy.contact.hours}</p>
          </div>
        </aside>
      </Container>
    </>
  );
}
