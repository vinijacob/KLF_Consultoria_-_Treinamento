import type { Metadata } from "next";
import { Container, EmptyNote, PageHeader, SectionHeading } from "@/components/site/blocks";
import { ClosingCta } from "@/components/site/contact";
import { ClientRoll, TestimonialQuote } from "@/components/site/lists";
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
      <Container className="mt-12">
        {clients.length > 0 ? <ClientRoll clients={clients} /> : <EmptyNote>{copy.clients.empty}</EmptyNote>}
      </Container>

      {testimonials.length > 0 && (
        <Container className="mt-24">
          <SectionHeading kicker={copy.testimonials.kicker} title={copy.testimonials.title} />
          <ul className="mt-10 grid gap-x-12 md:grid-cols-2">
            {testimonials.map((testimonial) => (
              <li key={testimonial.id} className="border-t border-rule py-10">
                <TestimonialQuote testimonial={testimonial} />
              </li>
            ))}
          </ul>
        </Container>
      )}

      <ClosingCta contact={settings.contact} />
    </>
  );
}
