import type { Metadata } from "next";
import { Container, FramedImage, PageHeader, SectionHeading } from "@/components/site/blocks";
import { ClosingCta } from "@/components/site/contact";
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
  let section = 0;

  return (
    <>
      <PageHeader kicker={copy.about.title} title={copy.person.name} intro={copy.person.role} />

      <Container className="mt-12 grid gap-12 lg:grid-cols-12">
        <div className="lg:col-span-5">
          <FramedImage alt={copy.person.name} placeholder="Retrato em breve" priority />
        </div>
        <div className="space-y-5 text-xl leading-relaxed lg:col-span-7 lg:pt-2">
          {paragraphs.map((paragraph, index) => (
            <p key={index} className={index === 0 ? "first-letter:float-left first-letter:mr-3 first-letter:font-serif first-letter:text-[4.5rem] first-letter:leading-[0.8] first-letter:text-accent" : undefined}>
              {paragraph}
            </p>
          ))}
        </div>
      </Container>

      {(about.mission || about.vision) && (
        <Container className="mt-24">
          <SectionHeading number={++section} kicker="Propósito" title="Missão e visão" />
          <div className="mt-10 grid gap-10 md:grid-cols-2 md:divide-x md:divide-rule">
            {about.mission && (
              <div>
                <Kicker>Missão</Kicker>
                <p className="mt-3 font-serif text-2xl leading-snug">{about.mission}</p>
              </div>
            )}
            {about.vision && (
              <div className="md:pl-10">
                <Kicker>Visão</Kicker>
                <p className="mt-3 font-serif text-2xl leading-snug">{about.vision}</p>
              </div>
            )}
          </div>
        </Container>
      )}

      {[
        { title: "Valores", items: about.values },
        { title: "Diferenciais", items: about.differentials },
      ]
        .filter((block) => block.items && block.items.length > 0)
        .map((block) => (
          <Container key={block.title} className="mt-24">
            <SectionHeading number={++section} kicker="Como trabalhamos" title={block.title} />
            <ol className="mt-10 grid border-t border-ink sm:grid-cols-2">
              {block.items!.map((item, index) => (
                <li key={item} className="flex gap-5 border-b border-rule py-5 sm:odd:pr-8 sm:even:border-l sm:even:pl-8">
                  <span className="font-mono text-sm text-ink-soft">{twoDigits(index + 1)}</span>
                  <span className="text-xl leading-snug">{item}</span>
                </li>
              ))}
            </ol>
          </Container>
        ))}

      {groups.length > 0 && (
        <Container className="mt-24">
          <SectionHeading number={++section} kicker="Trajetória" title="Caminho até aqui" />
          <div className="mt-10 space-y-14">
            {groups.map((group) => (
              <section key={group.type} aria-labelledby={`t-${group.type}`}>
                <h3 id={`t-${group.type}`} className="font-mono text-xs font-medium uppercase tracking-[0.14em] text-ink-soft">
                  {careerTypeLabel[group.type]}
                </h3>
                <ol className="mt-4 border-t border-ink">
                  {group.entries.map((entry) => (
                    <li key={entry.id} className="grid gap-2 border-b border-rule py-6 md:grid-cols-12 md:gap-8">
                      <span className="font-mono text-sm text-ink-soft md:col-span-3">{period(entry)}</span>
                      <div className="md:col-span-9">
                        <p className="font-serif text-2xl leading-snug">{entry.title}</p>
                        {entry.institution && <p className="mt-1 font-sans text-base font-medium">{entry.institution}</p>}
                        {entry.description && <p className="mt-3 max-w-2xl text-lg leading-relaxed text-ink-soft">{entry.description}</p>}
                      </div>
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
