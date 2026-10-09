import type { Metadata } from "next";
import { notFound } from "next/navigation";
import { ArrowLink, Container, FramedImage, Prose } from "@/components/site/blocks";
import { FactList } from "@/components/site/lists";
import { buttonClasses, ButtonLink } from "@/components/ui/button";
import { Kicker, Rule } from "@/components/ui/typography";
import { getService, getSettings, mediaUrl } from "@/lib/api/public";
import { formatWorkload, serviceFormatLabel, whatsappLink } from "@/lib/format";

export async function generateMetadata({ params }: PageProps<"/servicos/[slug]">): Promise<Metadata> {
  const { slug } = await params;
  const service = await getService(slug);

  if (!service) return { title: "Serviço não encontrado" };

  return {
    title: service.title,
    description: service.summary ?? undefined,
    openGraph: service.coverId ? { images: [mediaUrl(service.coverId)] } : undefined,
  };
}

export default async function ServicePage({ params }: PageProps<"/servicos/[slug]">) {
  const { slug } = await params;
  const [service, settings] = await Promise.all([getService(slug), getSettings()]);

  if (!service) notFound();

  const whatsapp = settings.contact?.whatsapp;
  const message = `Olá, Kilciene! Tenho interesse no treinamento "${service.title}" para a minha equipe.`;

  return (
    <article>
      <Container className="pt-12 sm:pt-16">
        <Kicker>
          Serviço · {serviceFormatLabel[service.format]} · {formatWorkload(service.workloadHours)}
        </Kicker>
        <h1 className="mt-4 max-w-4xl text-[clamp(2.5rem,7vw,4.75rem)] leading-[1.02]">{service.title}</h1>
        {service.summary && <p className="mt-6 max-w-2xl text-[1.375rem] leading-relaxed">{service.summary}</p>}
        <Rule variant="double" className="mt-10" />
      </Container>

      <Container className="mt-12 grid gap-14 lg:grid-cols-12">
        <div className="lg:col-span-8">
          {service.coverId && (
            <div className="mb-12">
              <FramedImage src={mediaUrl(service.coverId)} alt="" ratio="aspect-[16/9]" sizes="(min-width: 1024px) 60vw, 100vw" />
            </div>
          )}
          <Prose html={service.contentHtml} />
        </div>

        <aside className="lg:col-span-4" aria-label="Ficha do treinamento">
          <div className="lg:sticky lg:top-8">
            <Kicker>Ficha técnica</Kicker>
            <div className="mt-4">
              <FactList
                items={[
                  { label: "Formato", value: serviceFormatLabel[service.format] },
                  { label: "Carga horária", value: formatWorkload(service.workloadHours) },
                  { label: "Para quem", value: service.audience },
                ]}
              />
            </div>
            <div className="mt-8 flex flex-col gap-3">
              {whatsapp ? (
                <a href={whatsappLink(whatsapp, message)} rel="noopener" className={buttonClasses({ size: "lg" }, "w-full")}>
                  Pedir proposta pelo WhatsApp
                </a>
              ) : (
                <ButtonLink href="/contato" size="lg" className="w-full">
                  Pedir proposta
                </ButtonLink>
              )}
              <p className="font-sans text-sm text-ink-soft">O conteúdo é ajustado à realidade da sua equipe.</p>
            </div>
          </div>
        </aside>
      </Container>

      <Container className="mt-20">
        <Rule />
        <ArrowLink href="/servicos" className="mt-6">
          Ver todos os serviços
        </ArrowLink>
      </Container>
    </article>
  );
}
