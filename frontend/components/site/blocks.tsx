import Image from "next/image";
import Link from "next/link";
import type { ReactNode } from "react";
import { ArrowRightIcon } from "@/components/ui/icons";
import { Kicker, Rule } from "@/components/ui/typography";
import { cn } from "@/lib/cn";

/** Largura e respiro padrão das páginas do site. */
export function Container({ className, children }: { className?: string; children: ReactNode }) {
  return <div className={cn("mx-auto w-full max-w-6xl px-5 sm:px-8", className)}>{children}</div>;
}

/** Abertura de página interna: legenda, título grande, introdução e fio duplo. */
export function PageHeader({ kicker, title, intro, children }: { kicker: string; title: ReactNode; intro?: string; children?: ReactNode }) {
  return (
    <Container className="pt-12 sm:pt-16">
      <Kicker>{kicker}</Kicker>
      <h1 className="mt-4 max-w-4xl text-[clamp(2.5rem,7vw,4.75rem)] leading-[1.02]">{title}</h1>
      {intro && <p className="mt-6 max-w-2xl text-xl leading-relaxed text-ink-soft">{intro}</p>}
      {children}
      <Rule variant="double" className="mt-10" />
    </Container>
  );
}

/** Cabeçalho de seção numerado, como numa revista: "02 — Serviços". */
export function SectionHeading({
  number,
  kicker,
  title,
  intro,
  link,
  id,
}: {
  number?: number;
  kicker: string;
  title: string;
  intro?: string;
  link?: { href: string; label: string };
  id?: string;
}) {
  return (
    <div>
      <Rule variant="strong" />
      <div className="mt-4 grid gap-6 md:grid-cols-12">
        <div className="md:col-span-4">
          <Kicker>
            {number !== undefined && <span className="mr-2 text-ink">{String(number).padStart(2, "0")}</span>}
            {kicker}
          </Kicker>
        </div>
        <div className="md:col-span-8">
          <h2 id={id} className="text-[clamp(2rem,4.5vw,3.25rem)] leading-[1.05]">
            {title}
          </h2>
          {intro && <p className="mt-4 max-w-2xl text-lg leading-relaxed text-ink-soft">{intro}</p>}
          {link && <ArrowLink href={link.href} className="mt-5">{link.label}</ArrowLink>}
        </div>
      </div>
    </div>
  );
}

export function ArrowLink({ href, children, className }: { href: string; children: ReactNode; className?: string }) {
  return (
    <Link
      href={href}
      className={cn(
        "group inline-flex items-center gap-2 font-sans text-[0.9375rem] font-medium text-brand underline decoration-1 underline-offset-[6px] hover:decoration-2",
        className,
      )}
    >
      {children}
      <ArrowRightIcon className="transition-transform group-hover:translate-x-0.5" />
    </Link>
  );
}

/** Foto com moldura fina e legenda; sem foto, um espaço reservado honesto (nunca imagem genérica de banco). */
export function FramedImage({
  src,
  alt,
  caption,
  ratio = "aspect-[4/5]",
  placeholder = "Fotografia em breve",
  priority,
  sizes = "(min-width: 1024px) 40vw, 100vw",
}: {
  src?: string | null;
  alt: string;
  caption?: string;
  ratio?: string;
  placeholder?: string;
  priority?: boolean;
  sizes?: string;
}) {
  return (
    <figure>
      <div className="border border-ink p-2">
        <div className={cn("relative overflow-hidden bg-paper-deep", ratio)}>
          {src ? (
            <Image src={src} alt={alt} fill unoptimized priority={priority} sizes={sizes} className="object-cover" />
          ) : (
            <div className="absolute inset-0 grid place-items-center p-6 text-center">
              <span className="font-mono text-xs uppercase tracking-[0.16em] text-ink-soft">{placeholder}</span>
            </div>
          )}
        </div>
      </div>
      {caption && <figcaption className="mt-3 font-sans text-sm text-ink-soft">{caption}</figcaption>}
    </figure>
  );
}

/** Conteúdo vindo do editor do painel. O HTML já chega sanitizado pela API (IHtmlContentSanitizer). */
export function Prose({ html, className }: { html: string; className?: string }) {
  return <div className={cn("prose-klf", className)} dangerouslySetInnerHTML={{ __html: html }} />;
}

export function EmptyNote({ children }: { children: ReactNode }) {
  return <p className="border-y border-rule py-8 font-sans text-base text-ink-soft">{children}</p>;
}
