import type { Metadata } from "next";
import { Container, EmptyNote, PageHeader } from "@/components/site/blocks";
import { ClosingCta } from "@/components/site/contact";
import { ServiceCatalog } from "@/components/site/service-catalog";
import { copy } from "@/content/site-copy";
import { getServices, getSettings } from "@/lib/api/public";

export const metadata: Metadata = {
  title: "Serviços",
  description: copy.services.intro,
};

export default async function ServicesPage() {
  const [services, settings] = await Promise.all([getServices(), getSettings()]);

  return (
    <>
      <PageHeader kicker={copy.services.kicker} title={copy.services.title} intro={copy.services.intro} />
      <Container className="mt-12">
        {services.length > 0 ? <ServiceCatalog services={services} /> : <EmptyNote>{copy.services.empty}</EmptyNote>}
      </Container>
      <ClosingCta contact={settings.contact} />
    </>
  );
}
