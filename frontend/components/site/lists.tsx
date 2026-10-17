import Image from "next/image";
import Link from "next/link";
import { ArrowRightIcon } from "@/components/ui/icons";
import { Kicker } from "@/components/ui/typography";
import { cn } from "@/lib/cn";
import { formatDate, formatWorkload, postTypeLabel, serviceFormatLabel, twoDigits } from "@/lib/format";
import { mediaUrl } from "@/lib/media";
import type { PostListItem, ServiceListItem, Testimonial } from "@/types/site";
import { Avatar, Badge } from "./blocks";

/** Cartão de serviço: número, formato e carga horária em etiquetas, título, resumo e a seta que anda no hover. */
export function ServiceCard({ service, number, className }: { service: ServiceListItem; number: number; className?: string }) {
  return (
    <Link
      href={`/servicos/${service.slug}`}
      className={cn("lift group flex h-full flex-col rounded-2xl border border-rule bg-card p-7 sm:p-8", className)}
    >
      <span className="flex items-center justify-between gap-4">
        <span className="font-mono text-sm text-ink-soft">{twoDigits(number)}</span>
        <span className="flex flex-wrap justify-end gap-2">
          <Badge>{serviceFormatLabel[service.format]}</Badge>
          <Badge>{formatWorkload(service.workloadHours)}</Badge>
        </span>
      </span>
      <span className="mt-6 block font-serif text-[1.75rem] leading-tight">{service.title}</span>
      {service.summary && <span className="mt-3 block text-lg leading-relaxed text-ink-soft">{service.summary}</span>}
      <span className="mt-auto flex items-center gap-2 pt-7 font-sans text-[0.9375rem] font-medium text-brand">
        Ver detalhes
        <ArrowRightIcon className="transition-transform group-hover:translate-x-1" />
      </span>
    </Link>
  );
}

/** Serviços em grade de dois. `numbers` mantém a numeração original quando a lista está filtrada pela busca. */
export function ServiceGrid({ services, numbers }: { services: ServiceListItem[]; numbers?: number[] }) {
  return (
    <ul className="grid gap-5 md:grid-cols-2">
      {services.map((service, index) => (
        <li key={service.id} className="reveal">
          <ServiceCard service={service} number={numbers?.[index] ?? index + 1} />
        </li>
      ))}
    </ul>
  );
}

export function PostCard({ post }: { post: PostListItem }) {
  return (
    <Link href={`/conteudo/${post.slug}`} className="lift group flex h-full flex-col overflow-hidden rounded-2xl border border-rule bg-card">
      {post.coverId && (
        <span className="relative block aspect-video overflow-hidden bg-paper-deep">
          <Image
            src={mediaUrl(post.coverId)}
            alt=""
            fill
            unoptimized
            sizes="(min-width: 1024px) 30vw, (min-width: 640px) 45vw, 100vw"
            className="object-cover transition-transform duration-500 group-hover:scale-[1.03]"
          />
        </span>
      )}
      <span className="flex flex-1 flex-col p-7">
        <span className="flex flex-wrap items-center gap-3">
          <Badge tone="brand">{postTypeLabel[post.type]}</Badge>
          {post.category && <Badge>{post.category.name}</Badge>}
          {post.publishedAt && (
            <time dateTime={post.publishedAt} className="font-sans text-sm text-ink-soft">
              {formatDate(post.publishedAt)}
            </time>
          )}
        </span>
        <span className="mt-5 block font-serif text-[1.5rem] leading-snug">{post.title}</span>
        {post.summary && <span className="mt-3 block text-base leading-relaxed text-ink-soft">{post.summary}</span>}
        <span className="mt-auto flex items-center gap-2 pt-6 font-sans text-[0.9375rem] font-medium text-brand">
          Ler
          <ArrowRightIcon className="transition-transform group-hover:translate-x-1" />
        </span>
      </span>
    </Link>
  );
}

export function PostGrid({ posts }: { posts: PostListItem[] }) {
  return (
    <ul className="grid gap-5 sm:grid-cols-2 lg:grid-cols-3">
      {posts.map((post) => (
        <li key={post.id} className="reveal">
          <PostCard post={post} />
        </li>
      ))}
    </ul>
  );
}

/** Depoimento: aspas tipográficas, texto em itálico e a pessoa com foto (ou iniciais). Nunca estrelas. */
export function TestimonialQuote({ testimonial, large }: { testimonial: Testimonial; large?: boolean }) {
  const byline = [testimonial.authorRole, testimonial.companyName].filter(Boolean).join(", ");

  return (
    <figure>
      <blockquote
        className={large ? "font-serif text-[clamp(1.625rem,3.2vw,2.5rem)] leading-tight italic" : "font-serif text-[1.3125rem] leading-snug italic"}
      >
        <span aria-hidden className="mr-1 font-serif text-accent not-italic">
          “
        </span>
        {testimonial.quote}
        <span aria-hidden className="ml-0.5 font-serif text-accent not-italic">
          ”
        </span>
      </blockquote>
      <figcaption className={cn("flex items-center gap-4 font-sans text-[0.9375rem]", large ? "mt-8" : "mt-5")}>
        <Avatar name={testimonial.authorName} src={testimonial.photoId ? mediaUrl(testimonial.photoId) : null} size={large ? "md" : "sm"} />
        <span>
          <span className="block font-semibold text-ink">{testimonial.authorName}</span>
          {byline && <span className="block text-ink-soft">{byline}</span>}
        </span>
      </figcaption>
    </figure>
  );
}

export function FactList({ items }: { items: { label: string; value: string }[] }) {
  return (
    <dl className="grid gap-4">
      {items.map((item) => (
        <div key={item.label}>
          <dt>
            <Kicker>{item.label}</Kicker>
          </dt>
          <dd className="mt-1 font-sans text-base">{item.value}</dd>
        </div>
      ))}
    </dl>
  );
}
