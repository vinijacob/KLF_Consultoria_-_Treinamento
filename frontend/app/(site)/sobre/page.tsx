import type { Metadata } from "next";
import { Badge, Container, PageHeader, SectionHeading } from "@/components/site/blocks";
import { ClosingCta } from "@/components/site/contact";
import { PortraitCarousel } from "@/components/site/portrait-carousel";
import { Kicker } from "@/components/ui/typography";
import { copy } from "@/content/site-copy";
import { getCareer, getSettings } from "@/lib/api/public";
import { careerTypeLabel, formatMonthYear, twoDigits } from "@/lib/format";
import type { CareerEntry, CareerEntryType } from "@/types/site";

export const metadata: Metadata = {
  title: "Sobre",
  description: `${copy.person.name}, ${copy.person.role.toLowerCase()}.`,
};

const order: CareerEntryType[] = ["Experience", "Education", "Certification"];

function period(entry: CareerEntry) {
  const start = formatMonthYear(entry.startDate);
  if (entry.isOngoing) return `${start} — atual`;
  return entry.endDate ? `${start} — ${formatMonthYear(entry.endDate)}` : start;
}

export default async function AboutPage() {
  const [settings, career] = await Promise.all([getSettings(), getCareer()]);
  const about = settings.about ?? {};
  const paragraphs = (about.history ?? copy.about.historyFallback).split(/\n{2,}/);
  const groups = order
    .map((type) => ({ type, entries: career.filter((entry) => entry.entryType === type) }))
    .filter((group) => group.entries.length > 0);
  const lists = [
    { title: "Valores", items: about.values },
    { title: "Diferenciais", items: about.differentials },
  ].filter((block) => block.items && block.items.length > 0);

  return (
    <>
      <PageHeader kicker={copy.about.title} title={copy.person.name} intro={copy.person.role} />

      <Container className="mt-12 grid items-start gap-12 lg:grid-cols-12">
        <div className="lg:col-span-5">
          <PortraitCarousel ids={about.portraitIds ?? []} name={copy.person.name} priority />
        </div>
        <div className="space-y-5 text-xl leading-relaxed lg:col-span-7 lg:pt-2">
          {paragraphs.map((paragraph, index) => (
            <p
              key={index}
              className={
                index === 0
                  ? "first-letter:float-left first-letter:mr-3 first-letter:font-serif first-letter:text-[4.5rem] first-letter:leading-[0.8] first-letter:text-accent"
                  : undefined
              }
            >
              {paragraph}
            </p>
          ))}
        </div>
      </Container>

      {(about.mission || about.vision) && (
        <Container className="mt-28">
          <SectionHeading kicker="Propósito" title="Missão e visão" />
          <div className="mt-10 grid gap-5 md:grid-cols-2">
            {about.mission && (
              <div className="reveal rounded-2xl bg-brand p-8 text-brand-contrast sm:p-10">
                <p className="font-mono text-xs font-medium uppercase tracking-[0.14em]">Missão</p>
                <p className="mt-4 font-serif text-2xl leading-snug">{about.mission}</p>
              </div>
            )}
            {about.vision && (
              <div className="reveal rounded-2xl border border-rule bg-card p-8 sm:p-10">
                <Kicker>Visão</Kicker>
                <p className="mt-4 font-serif text-2xl leading-snug">{about.vision}</p>
              </div>
            )}
          </div>
        </Container>
      )}

      {lists.length > 0 && (
        <Container className="mt-28">
          <SectionHeading kicker="Como trabalhamos" title={lists.map((block) => block.title).join(" e ")} />
          <div className={`mt-10 grid gap-5 ${lists.length > 1 ? "md:grid-cols-2" : ""}`}>
            {lists.map((block) => (
              <section key={block.title} aria-label={block.title} className="reveal rounded-2xl bg-paper-deep p-8 sm:p-10">
                <h3 className="font-serif text-2xl">{block.title}</h3>
                <ol className="mt-6 grid gap-3">
                  {block.items!.map((item, index) => (
                    <li key={item} className="flex items-baseline gap-4 rounded-xl bg-card px-5 py-4">
                      <span className="font-mono text-sm text-ink-soft">{twoDigits(index + 1)}</span>
                      <span className="text-lg leading-snug">{item}</span>
                    </li>
                  ))}
                </ol>
              </section>
            ))}
          </div>
        </Container>
      )}

      {groups.length > 0 && (
        <Container className="mt-28">
          <SectionHeading kicker="Trajetória" title="Caminho até aqui" />
          <div className="mt-10 space-y-12">
            {groups.map((group) => (
              <section key={group.type} aria-labelledby={`t-${group.type}`}>
                <h3 id={`t-${group.type}`}>
                  <Badge tone="brand">{careerTypeLabel[group.type]}</Badge>
                </h3>
                <ol className="mt-5 grid gap-4 md:grid-cols-2">
                  {group.entries.map((entry) => (
                    <li key={entry.id} className="reveal rounded-2xl border border-rule bg-card p-7">
                      <span className="font-mono text-xs uppercase tracking-[0.12em] text-ink-soft">{period(entry)}</span>
                      <p className="mt-3 font-serif text-2xl leading-snug">{entry.title}</p>
                      {entry.institution && <p className="mt-1 font-sans text-base font-medium">{entry.institution}</p>}
                      {entry.description && <p className="mt-3 text-lg leading-relaxed text-ink-soft">{entry.description}</p>}
                    </li>
                  ))}
                </ol>
              </section>
            ))}
          </div>
        </Container>
      )}

      <ClosingCta contact={settings.contact} />
    </>
  );
}
