import { ArrowLink, Container, FramedImage, SectionHeading } from "@/components/site/blocks";
import { ClosingCta } from "@/components/site/contact";
import { ClientMarquee } from "@/components/site/client-marquee";
import { PostIndex, ServiceIndex } from "@/components/site/lists";
import { TestimonialCarousel } from "@/components/site/testimonials";
import { ButtonLink } from "@/components/ui/button";
import { Kicker, Rule } from "@/components/ui/typography";
import { copy } from "@/content/site-copy";
import { getClients, getPosts, getServices, getSettings, getTestimonials } from "@/lib/api/public";

const romans = ["I", "II", "III"];

export default async function HomePage() {
  const [settings, services, testimonials, clients, posts] = await Promise.all([
    getSettings(),
    getServices(),
    getTestimonials(),
    getClients(),
    getPosts({ pageSize: 3 }),
  ]);

  const about = settings.about ?? {};
  const featured = testimonials[0];
  let section = 0;

  return (
    <>
      <Container className="pt-10 sm:pt-14">
        <div className="grid gap-12 lg:grid-cols-12 lg:gap-10">
          <div className="lg:col-span-7">
            <Kicker>{copy.hero.kicker}</Kicker>
            <h1 className="mt-6 text-[clamp(3.25rem,9vw,6.5rem)] leading-[0.95] font-normal tracking-[-0.03em]">
              {copy.hero.titleStart} <em className="font-normal">{copy.hero.titleEmphasis}</em>
            </h1>
            <Rule variant="accent" className="mt-10 max-w-24" />
            <p className="mt-8 max-w-xl text-[1.375rem] leading-relaxed">{copy.hero.lede}</p>
            <div className="mt-10 flex flex-wrap items-center gap-x-8 gap-y-4">
              <ButtonLink href="/servicos" size="lg">
                {copy.hero.primary}
              </ButtonLink>
              <ArrowLink href="/contato">{copy.hero.secondary}</ArrowLink>
            </div>
          </div>
          <div className="lg:col-span-5 lg:pt-4">
            <FramedImage alt={copy.person.name} caption={copy.hero.portraitCaption} placeholder="Retrato em breve" priority />
          </div>
        </div>
      </Container>

      <Container className="mt-20">
        <ul className="grid border-y border-ink md:grid-cols-3 md:divide-x md:divide-rule">
          {copy.pillars.map((pillar, index) => (
            <li key={pillar.title} className="border-b border-rule px-0 py-8 last:border-b-0 md:border-b-0 md:px-8 md:first:pl-0 md:last:pr-0">
              <p className="font-mono text-sm text-accent">{romans[index]}</p>
              <h2 className="mt-3 text-3xl">{pillar.title}</h2>
              <p className="mt-3 text-lg leading-relaxed text-ink-soft">{pillar.text}</p>
            </li>
          ))}
        </ul>
      </Container>

      <Container className="mt-24">
        <SectionHeading
          number={++section}
          kicker={copy.services.kicker}
          title={copy.services.title}
          intro={copy.services.intro}
          link={services.length > 4 ? { href: "/servicos", label: "Ver todos os serviços" } : undefined}
        />
        <div className="mt-10">
          {services.length > 0 ? (
            <ServiceIndex services={services.slice(0, 4)} />
          ) : (
            <p className="border-y border-rule py-8 font-sans text-ink-soft">{copy.services.empty}</p>
          )}
        </div>
      </Container>

      <Container className="mt-24">
        <SectionHeading number={++section} kicker={copy.about.kicker} title={copy.about.title} />
        <div className="mt-10 grid gap-10 md:grid-cols-12">
          <blockquote className="md:col-span-8 md:col-start-5">
            <p className="font-serif text-[clamp(1.625rem,3.2vw,2.375rem)] leading-[1.25]">
              {about.mission ?? copy.about.teaser}
            </p>
            <footer className="mt-6 font-sans text-[0.9375rem]">
              <span className="font-semibold">{copy.person.name}</span>
              <span className="text-ink-soft"> · {copy.person.role}</span>
            </footer>
            <ArrowLink href="/sobre" className="mt-8">
              {copy.about.link}
            </ArrowLink>
          </blockquote>
        </div>
      </Container>

      {featured && (
        <Container className="mt-24">
          <SectionHeading
            number={++section}
            kicker={copy.testimonials.kicker}
            title={copy.testimonials.title}
            link={testimonials.length > 1 ? { href: "/clientes", label: "Ler todos os depoimentos" } : undefined}
          />
          <div className="mt-10 md:grid md:grid-cols-12">
            <div className="md:col-span-9 md:col-start-4">
              <TestimonialCarousel testimonials={testimonials} />
            </div>
          </div>
        </Container>
      )}

      {clients.length > 0 && (
        <>
          <Container className="mt-24">
            <SectionHeading
              number={++section}
              kicker={copy.clients.kicker}
              title={copy.clients.title}
              link={{ href: "/clientes", label: "Ver todas as empresas" }}
            />
          </Container>
          <div className="mt-10">
            <ClientMarquee clients={clients} />
          </div>
        </>
      )}

      {posts.items.length > 0 && (
        <Container className="mt-24">
          <SectionHeading
            number={++section}
            kicker={copy.posts.kicker}
            title={copy.posts.title}
            intro={copy.posts.intro}
            link={{ href: "/conteudo", label: "Ver todas as publicações" }}
          />
          <div className="mt-10">
            <PostIndex posts={posts.items} />
          </div>
        </Container>
      )}

      <ClosingCta contact={settings.contact} />
    </>
  );
}
