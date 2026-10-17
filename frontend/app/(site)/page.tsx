import { ArrowLink, Avatar, Badge, Container, SectionHeading } from "@/components/site/blocks";
import { ZoomablePhoto } from "@/components/site/bubble";
import { ClientShowcase } from "@/components/site/client-showcase";
import { FeaturedQuoteCard } from "@/components/site/featured-quote";
import { ClosingCta } from "@/components/site/contact";
import { RevealObserver } from "@/components/site/reveal-observer";
import { PostGrid, ServiceGrid } from "@/components/site/lists";
import { PortraitCarousel } from "@/components/site/portrait-carousel";
import { TestimonialSpotlight } from "@/components/site/testimonials";
import { ButtonLink } from "@/components/ui/button";
import { copy } from "@/content/site-copy";
import { getClients, getFeaturedQuote, getLatestTestimonials, getPosts, getServices, getSettings, mediaUrl } from "@/lib/api/public";
import { pickRandom } from "@/lib/search";

const romans = ["I", "II", "III"];

export default async function HomePage() {
  const [settings, services, testimonials, clients, posts, quote] = await Promise.all([
    getSettings(),
    getServices(),
    getLatestTestimonials(),
    getClients(),
    getPosts({ pageSize: 3 }),
    getFeaturedQuote(),
  ]);

  const about = settings.about ?? {};
  const portraits = about.portraitIds ?? [];
  const portrait = portraits[0] ? mediaUrl(portraits[0]) : null;
  const featuredClients = pickRandom(clients, 10).map((client) => client.id);

  return (
    <>
      <Container className="pt-12 sm:pt-16">
        <div className="grid items-center gap-14 lg:grid-cols-12 lg:gap-10">
          <div className="lg:col-span-7">
            <Badge tone="brand">{copy.hero.kicker}</Badge>
            <h1 className="mt-6 text-[clamp(3.25rem,9vw,6.5rem)] leading-[0.95] font-normal tracking-[-0.03em] motion-safe:animate-rise">
              {copy.hero.titleStart} <em className="font-normal">{copy.hero.titleEmphasis}</em>
            </h1>
            <p className="mt-8 max-w-xl text-[1.375rem] leading-relaxed">{copy.hero.lede}</p>
            <div className="mt-10 flex flex-wrap items-center gap-x-8 gap-y-4">
              <ButtonLink href="/servicos" size="lg">
                {copy.hero.primary}
              </ButtonLink>
              <ArrowLink href="/contato">{copy.hero.secondary}</ArrowLink>
            </div>
          </div>
          <div className="lg:col-span-5 motion-safe:animate-rise">
            <PortraitCarousel ids={portraits} name={copy.person.name} priority />
          </div>
        </div>
      </Container>

      <Container className="mt-24">
        <ul className="grid gap-5 md:grid-cols-3">
          {copy.pillars.map((pillar, index) => (
            <li key={pillar.title} className="reveal rounded-2xl border border-rule bg-card p-7">
              <p className="font-serif text-3xl text-accent italic">{romans[index]}</p>
              <h2 className="mt-4 text-3xl">{pillar.title}</h2>
              <p className="mt-3 text-lg leading-relaxed text-ink-soft">{pillar.text}</p>
            </li>
          ))}
        </ul>
      </Container>

      {quote && (
        <Container className="mt-24">
          <FeaturedQuoteCard quote={quote} />
        </Container>
      )}

      <Container className="mt-28">
        <SectionHeading
          kicker={copy.services.kicker}
          title={copy.services.title}
          intro={copy.services.intro}
          link={services.length > 4 ? { href: "/servicos", label: "Ver todos os serviços" } : undefined}
        />
        <div className="mt-10">
          {services.length > 0 ? (
            <ServiceGrid services={services.slice(0, 4)} />
          ) : (
            <p className="rounded-xl border border-dashed border-rule-strong px-6 py-10 text-center font-sans text-ink-soft">
              {copy.services.empty}
            </p>
          )}
        </div>
      </Container>

      <Container className="mt-28">
        <section aria-labelledby="sobre-titulo" className="reveal grid gap-10 rounded-3xl bg-paper-deep px-7 py-12 sm:px-12 sm:py-16 md:grid-cols-12">
          <div className="md:col-span-4">
            <Badge className="bg-card">{copy.about.kicker}</Badge>
            <h2 id="sobre-titulo" className="mt-4 text-[clamp(2rem,4vw,3rem)] leading-[1.05]">
              {copy.about.title}
            </h2>
          </div>
          <blockquote className="md:col-span-8">
            <p className="font-serif text-[clamp(1.5rem,3vw,2.25rem)] leading-[1.3]">{about.mission ?? copy.about.teaser}</p>
            <footer className="mt-8 flex items-center gap-4 font-sans text-[0.9375rem]">
              {portrait ? (
                <ZoomablePhoto
                  photo={{ src: portrait, alt: copy.person.name }}
                  className="shrink-0 rounded-full ring-2 ring-transparent ring-offset-4 ring-offset-paper-deep transition-[box-shadow] duration-500 hover:ring-accent"
                >
                  <Avatar name={copy.person.name} src={portrait} />
                </ZoomablePhoto>
              ) : (
                <Avatar name={copy.person.name} src={portrait} />
              )}
              <span>
                <span className="block font-semibold">{copy.person.name}</span>
                <span className="block text-ink-soft">{copy.person.role}</span>
              </span>
            </footer>
            <ArrowLink href="/sobre" className="mt-6">
              {copy.about.link}
            </ArrowLink>
          </blockquote>
        </section>
      </Container>

      {testimonials.length > 0 && (
        <Container className="mt-28">
          <SectionHeading
            kicker={copy.testimonials.kicker}
            title={copy.testimonials.title}
            link={testimonials.length > 1 ? { href: "/clientes#depoimentos", label: "Ler todos os depoimentos" } : undefined}
          />
          <div className="reveal mt-10">
            <TestimonialSpotlight testimonials={testimonials} />
          </div>
        </Container>
      )}

      {clients.length > 0 && (
        <Container className="mt-28">
          <SectionHeading kicker={copy.clients.kicker} title={copy.clients.title} link={{ href: "/clientes", label: "Ver todas as empresas" }} />
          <div className="reveal mt-10">
            <ClientShowcase clients={clients} initialIds={featuredClients} />
          </div>
        </Container>
      )}

      {posts.items.length > 0 && (
        <Container className="mt-28">
          <SectionHeading
            kicker={copy.posts.kicker}
            title={copy.posts.title}
            intro={copy.posts.intro}
            link={{ href: "/conteudo", label: "Ver todas as publicações" }}
          />
          <div className="mt-10">
            <PostGrid posts={posts.items} />
          </div>
        </Container>
      )}

      <ClosingCta contact={settings.contact} />
      <RevealObserver />
    </>
  );
}
