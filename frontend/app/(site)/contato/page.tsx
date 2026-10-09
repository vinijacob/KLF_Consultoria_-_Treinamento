import type { Metadata } from "next";
import { Container, PageHeader } from "@/components/site/blocks";
import { ContactChannels } from "@/components/site/contact";
import { Kicker } from "@/components/ui/typography";
import { copy } from "@/content/site-copy";
import { getSettings } from "@/lib/api/public";

export const metadata: Metadata = {
  title: "Contato",
  description: copy.contact.intro,
};

export default async function ContactPage() {
  const { contact } = await getSettings();

  return (
    <>
      <PageHeader kicker={copy.contact.kicker} title={copy.contact.title} intro={copy.contact.intro} />
      <Container className="mt-12 grid gap-12 lg:grid-cols-12">
        <div className="lg:col-span-8">
          <ContactChannels contact={contact ?? {}} />
        </div>
        <aside className="lg:col-span-4">
          <Kicker>Atendimento</Kicker>
          <p className="mt-3 text-lg leading-relaxed">{copy.contact.hours}</p>
          <p className="mt-6 font-sans text-sm text-ink-soft">{copy.contact.formSoon}</p>
        </aside>
      </Container>
    </>
  );
}
