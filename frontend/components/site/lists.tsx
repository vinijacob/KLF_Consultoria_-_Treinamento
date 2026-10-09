import Link from "next/link";
import { ArrowRightIcon } from "@/components/ui/icons";
import { Kicker } from "@/components/ui/typography";
import { formatDate, formatWorkload, postTypeLabel, serviceFormatLabel, twoDigits } from "@/lib/format";
import type { Client, PostListItem, ServiceListItem, Testimonial } from "@/types/site";

/** Serviços como um sumário de revista: número, título, resumo e ficha técnica em uma linha. */
export function ServiceIndex({ services }: { services: ServiceListItem[] }) {
  return (
    <ol className="border-t border-ink">
      {services.map((service, index) => (
        <li key={service.id} className="border-b border-rule">
          <Link
            href={`/servicos/${service.slug}`}
            className="group grid gap-x-8 gap-y-3 py-7 md:grid-cols-12 md:items-baseline"
          >
            <span className="font-mono text-sm text-ink-soft md:col-span-1">{twoDigits(index + 1)}</span>
            <span className="md:col-span-6">
              <span className="block font-serif text-[1.75rem] leading-tight group-hover:underline group-hover:decoration-1 group-hover:underline-offset-[6px]">
                {service.title}
              </span>
              {service.summary && <span className="mt-2 block text-lg leading-relaxed text-ink-soft">{service.summary}</span>}
            </span>
            <span className="flex items-center justify-between gap-4 font-mono text-xs uppercase tracking-[0.12em] text-ink-soft md:col-span-5 md:justify-end">
              <span>
                {serviceFormatLabel[service.format]} · {formatWorkload(service.workloadHours)}
              </span>
              <ArrowRightIcon className="shrink-0 text-xl text-ink transition-transform group-hover:translate-x-1" />
            </span>
          </Link>
        </li>
      ))}
    </ol>
  );
}

export function PostIndex({ posts }: { posts: PostListItem[] }) {
  return (
    <ol className="border-t border-ink">
      {posts.map((post) => (
        <li key={post.id} className="border-b border-rule">
          <Link href={`/conteudo/${post.slug}`} className="group grid gap-x-8 gap-y-2 py-7 md:grid-cols-12 md:items-baseline">
            <span className="font-mono text-xs uppercase tracking-[0.12em] text-ink-soft md:col-span-3">
              {postTypeLabel[post.type]}
              {post.publishedAt && (
                <>
                  <br className="hidden md:block" />
                  <span className="md:hidden"> · </span>
                  <time dateTime={post.publishedAt}>{formatDate(post.publishedAt)}</time>
                </>
              )}
            </span>
            <span className="md:col-span-9">
              <span className="block font-serif text-[1.625rem] leading-tight group-hover:underline group-hover:decoration-1 group-hover:underline-offset-[6px]">
                {post.title}
              </span>
              {post.summary && <span className="mt-2 block max-w-2xl text-lg leading-relaxed text-ink-soft">{post.summary}</span>}
            </span>
          </Link>
        </li>
      ))}
    </ol>
  );
}

/** Depoimento em destaque: aspas tipográficas grandes, nunca em cartão com estrelas. */
export function TestimonialQuote({ testimonial, large }: { testimonial: Testimonial; large?: boolean }) {
  const byline = [testimonial.authorRole, testimonial.companyName].filter(Boolean).join(", ");

  return (
    <figure>
      <blockquote
        className={
          large
            ? "font-serif text-[clamp(1.75rem,3.6vw,2.75rem)] leading-[1.2] italic"
            : "font-serif text-[1.375rem] leading-snug italic"
        }
      >
        <span aria-hidden className="mr-1 font-serif text-accent not-italic">
          “
        </span>
        {testimonial.quote}
        <span aria-hidden className="ml-0.5 font-serif text-accent not-italic">
          ”
        </span>
      </blockquote>
      <figcaption className="mt-5 font-sans text-[0.9375rem]">
        <span className="font-semibold text-ink">{testimonial.authorName}</span>
        {byline && <span className="text-ink-soft"> · {byline}</span>}
      </figcaption>
    </figure>
  );
}

/** Empresas atendidas em texto (nome sempre legível); com site, vira link. */
export function ClientRoll({ clients }: { clients: Client[] }) {
  return (
    <ul className="grid border-t border-l border-rule sm:grid-cols-2 lg:grid-cols-4">
      {clients.map((client) => (
        <li key={client.id} className="flex min-h-28 items-center border-r border-b border-rule px-6 py-5">
          {client.websiteUrl ? (
            <a
              href={client.websiteUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="font-serif text-xl leading-snug underline-offset-4 hover:underline"
            >
              {client.name}
              <span className="sr-only"> (site da empresa, abre em nova aba)</span>
            </a>
          ) : (
            <span className="font-serif text-xl leading-snug">{client.name}</span>
          )}
        </li>
      ))}
    </ul>
  );
}

export function FactList({ items }: { items: { label: string; value: string }[] }) {
  return (
    <dl className="divide-y divide-rule border-y border-ink">
      {items.map((item) => (
        <div key={item.label} className="grid grid-cols-[8.5rem_1fr] gap-4 py-3.5">
          <dt>
            <Kicker>{item.label}</Kicker>
          </dt>
          <dd className="font-sans text-base">{item.value}</dd>
        </div>
      ))}
    </dl>
  );
}
