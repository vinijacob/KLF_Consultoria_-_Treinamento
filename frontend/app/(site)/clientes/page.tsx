import type { Metadata } from "next";
import { Container, EmptyNote, PageHeader, SectionHeading } from "@/components/site/blocks";
import { ClientDirectory } from "@/components/site/client-directory";
import { ClientMarquee } from "@/components/site/client-marquee";
import { ClosingCta } from "@/components/site/contact";
import { TestimonialCarousel, TestimonialWall } from "@/components/site/testimonials";
import { copy } from "@/content/site-copy";
import { getClients, getSettings, getTestimonials } from "@/lib/api/public";

export const metadata: Metadata = {
  title: "Clientes e depoimentos",
  description: copy.clients.title,
};

export default async function ClientsPage() {
  const [clients, testimonials, settings] = await Promise.all([getClients(), getTestimonials(), getSettings()]);

  return (
    <>
      <PageHeader kicker={copy.clients.kicker} title={copy.clients.title} />

      {clients.length > 0 ? (
        <>
          <div className="mt-12">
            <ClientMarquee clients={clients} />
          </div>
          <Container className="mt-20">
            <ClientDirectory clients={clients} />
          </Container>
        </>
      ) : (
        <Container className="mt-12">
          <EmptyNote>{copy.clients.empty}</EmptyNote>
        </Container>
      )}

      {testimonials.length > 0 && (
        <Container className="mt-24">
          <SectionHeading kicker={copy.testimonials.kicker} title={copy.testimonials.title} />
          <div className="mt-10 md:grid md:grid-cols-12">
            <div className="md:col-span-10 md:col-start-2">
              <TestimonialCarousel testimonials={testimonials} />
            </div>
          </div>
          {testimonials.length > 1 && (
            <div className="mt-20">
              <h3 className="mb-8 border-b-2 border-ink pb-3 font-serif text-3xl">Todos os depoimentos</h3>
              <TestimonialWall testimonials={testimonials} />
            </div>
          )}
        </Container>
      )}

      <ClosingCta contact={settings.contact} />
    </>
  );
}
