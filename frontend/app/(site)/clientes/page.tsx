import type { Metadata } from "next";
import { Container, EmptyNote, PageHeader, SectionHeading } from "@/components/site/blocks";
import { ClientShowcase } from "@/components/site/client-showcase";
import { ClosingCta } from "@/components/site/contact";
import { TestimonialWall } from "@/components/site/testimonials";
import { copy } from "@/content/site-copy";
import { getClients, getSettings, getTestimonials } from "@/lib/api/public";
import { pickRandom } from "@/lib/search";

export const metadata: Metadata = {
  title: "Clientes e depoimentos",
  description: copy.clients.title,
};

export default async function ClientsPage() {
  const [clients, testimonials, settings] = await Promise.all([getClients(), getTestimonials(), getSettings()]);
  const featured = pickRandom(clients, 20).map((client) => client.id);

  return (
    <>
      <PageHeader kicker={copy.clients.kicker} title={copy.clients.title} />

      <Container className="mt-12">
        {clients.length > 0 ? <ClientShowcase clients={clients} initialIds={featured} searchable /> : <EmptyNote>{copy.clients.empty}</EmptyNote>}
      </Container>

      {testimonials.length > 0 && (
        <Container className="mt-28">
          <div id="depoimentos" className="scroll-mt-28">
            <SectionHeading kicker={copy.testimonials.kicker} title={copy.testimonials.title} />
          </div>
          <div className="mt-10">
            <TestimonialWall testimonials={testimonials} />
          </div>
        </Container>
      )}

      <ClosingCta contact={settings.contact} />
    </>
  );
}
