import Image from "next/image";
import Link from "next/link";
import type { ReactNode } from "react";
import { ArrowRightIcon } from "@/components/ui/icons";
import { cn } from "@/lib/cn";

/** Largura e respiro padrão das páginas do site. */
export function Container({ className, children }: { className?: string; children: ReactNode }) {
  return <div className={cn("mx-auto w-full max-w-6xl px-5 sm:px-8", className)}>{children}</div>;
}

/** Etiqueta pequena (categoria, formato, carga horária). Texto sempre legível; nunca selo de "novo". */
export function Badge({ children, tone = "plain", className }: { children: ReactNode; tone?: "plain" | "brand" | "accent"; className?: string }) {
  return (
    <span
      className={cn(
        "inline-flex items-center rounded-full px-3 py-1 font-mono text-[0.6875rem] font-medium uppercase tracking-[0.12em]",
        tone === "plain" && "bg-paper-deep text-ink-soft",
        tone === "brand" && "bg-brand-soft text-brand",
        tone === "accent" && "border border-accent text-accent",
        className,
      )}
    >
      {children}
    </span>
  );
}

/** Abertura de página interna: etiqueta, título grande e introdução, com bastante respiro. */
export function PageHeader({ kicker, title, intro, children }: { kicker: string; title: ReactNode; intro?: string; children?: ReactNode }) {
  return (
    <Container className="pt-14 sm:pt-20">
      <Badge tone="brand">{kicker}</Badge>
      <h1 className="mt-5 max-w-4xl text-[clamp(2.5rem,7vw,4.75rem)] leading-[1.02] motion-safe:animate-rise">{title}</h1>
      {intro && <p className="mt-6 max-w-2xl text-xl leading-relaxed text-ink-soft">{intro}</p>}
      {children}
    </Container>
  );
}

/** Cabeçalho de seção: etiqueta, título e, à direita, o link para ver mais. */
export function SectionHeading({
  kicker,
  title,
  intro,
  link,
  id,
}: {
  kicker: string;
  title: string;
  intro?: string;
  link?: { href: string; label: string };
  id?: string;
}) {
  return (
    <div className="reveal flex flex-wrap items-end justify-between gap-x-10 gap-y-5">
      <div className="max-w-2xl">
        <Badge>{kicker}</Badge>
        <h2 id={id} className="mt-4 text-[clamp(2rem,4.5vw,3.25rem)] leading-[1.05]">
          {title}
        </h2>
        {intro && <p className="mt-4 text-lg leading-relaxed text-ink-soft">{intro}</p>}
      </div>
      {link && <ArrowLink href={link.href}>{link.label}</ArrowLink>}
    </div>
  );
}

export function ArrowLink({ href, children, className }: { href: string; children: ReactNode; className?: string }) {
  return (
    <Link
      href={href}
      className={cn(
        "group inline-flex min-h-11 items-center gap-2 font-sans text-[0.9375rem] font-medium text-brand underline decoration-1 underline-offset-[6px] hover:decoration-2",
        className,
      )}
    >
      {children}
      <ArrowRightIcon className="transition-transform group-hover:translate-x-1" />
    </Link>
  );
}

/** Foto com cantos suaves; sem foto, um espaço reservado honesto (nunca imagem genérica de banco). */
export function FramedImage({
  src,
  alt,
  caption,
  ratio = "aspect-[4/5]",
  placeholder = "Fotografia em breve",
  priority,
  sizes = "(min-width: 1024px) 40vw, 100vw",
  className,
}: {
  src?: string | null;
  alt: string;
  caption?: string;
  ratio?: string;
  placeholder?: string;
  priority?: boolean;
  sizes?: string;
  className?: string;
}) {
  return (
    <figure className={className}>
      <div className={cn("relative overflow-hidden rounded-2xl bg-paper-deep", ratio)}>
        {src ? (
          <Image src={src} alt={alt} fill unoptimized priority={priority} sizes={sizes} className="object-cover" />
        ) : (
          <div className="absolute inset-0 grid place-items-center p-6 text-center">
            <span className="font-mono text-xs uppercase tracking-[0.16em] text-ink-soft">{placeholder}</span>
          </div>
        )}
      </div>
      {caption && <figcaption className="mt-3 font-sans text-sm text-ink-soft">{caption}</figcaption>}
    </figure>
  );
}

function initials(name: string) {
  const words = name.replace(/\(.*?\)/g, "").trim().split(/\s+/).filter(Boolean);
  return ((words[0]?.[0] ?? "") + (words.length > 1 ? words[words.length - 1][0] : "")).toUpperCase();
}

/** Foto redonda da pessoa (decorativa: o nome sempre aparece ao lado); sem foto, as iniciais em letra serifada. */
export function Avatar({ name, src, size = "md" }: { name: string; src?: string | null; size?: "sm" | "md" | "lg" }) {
  const box = size === "sm" ? "size-10 text-base" : size === "lg" ? "size-20 text-3xl" : "size-14 text-xl";

  return (
    <span className={cn("relative grid shrink-0 place-items-center overflow-hidden rounded-full bg-brand-soft font-serif text-brand", box)}>
      {src ? (
        <Image src={src} alt="" fill unoptimized sizes="80px" className="object-cover" />
      ) : (
        <span aria-hidden>{initials(name)}</span>
      )}
    </span>
  );
}

export { initials };

/** Conteúdo vindo do editor do painel. O HTML já chega sanitizado pela API (IHtmlContentSanitizer). */
export function Prose({ html, className }: { html: string; className?: string }) {
  return <div className={cn("prose-klf", className)} dangerouslySetInnerHTML={{ __html: html }} />;
}

export function EmptyNote({ children }: { children: ReactNode }) {
  return <p className="rounded-xl border border-dashed border-rule-strong px-6 py-10 text-center font-sans text-base text-ink-soft">{children}</p>;
}
