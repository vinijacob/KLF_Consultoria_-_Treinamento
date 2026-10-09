"use client";

import Link from "next/link";
import type { ReactNode } from "react";
import { roleInfo } from "@/config/admin";
import { addDays, formatShortDate, localHour, todayLocal } from "@/lib/admin/datetime";
import { useApi } from "@/lib/admin/hooks";
import { firstName, greetingFor, plural } from "@/lib/admin/text";
import type {
  AdminServiceListItem,
  AdminTestimonial,
  FeedbackSessionListItem,
  FeedbackSummary,
  MediaAsset,
  Paged,
  AdminPostListItem,
} from "@/types/admin";
import { ButtonLink } from "@/components/ui/button";
import { AlertIcon, CheckIcon, LockIcon } from "@/components/ui/icons";
import { Kicker, Rule } from "@/components/ui/typography";
import { useAuth } from "./auth-provider";
import { RoleStamp } from "./shell";
import { sessionStatusTag } from "./tags";
import { Stat } from "./ui";

function count(value: number | undefined) {
  return value === undefined ? "—" : value;
}

function Pending({ href, children }: { href: string; children: ReactNode }) {
  return (
    <li className="border-b border-rule">
      <Link href={href} className="flex min-h-12 items-start gap-3 py-3 font-sans text-[0.9375rem] hover:text-brand">
        <AlertIcon className="mt-0.5 shrink-0 text-warning" />
        <span className="underline decoration-rule-strong underline-offset-4">{children}</span>
      </Link>
    </li>
  );
}

function Masthead() {
  const { user, role } = useAuth();
  const info = roleInfo[role];

  return (
    <aside aria-labelledby="expediente" className="border-2 border-ink p-6">
      <div className="flex items-center justify-between gap-4">
        <h2 id="expediente" className="font-mono text-xs font-medium uppercase tracking-[0.16em]">
          Expediente
        </h2>
        <RoleStamp role={role} />
      </div>
      <Rule className="my-4" />
      <p className="text-xl leading-snug">{user.fullName}</p>
      <p className="font-sans text-sm text-ink-soft">{user.email}</p>
      <ul className="mt-5 grid gap-2.5 font-sans text-sm">
        {info.can.map((item) => (
          <li key={item} className="flex items-start gap-2.5">
            <CheckIcon className="mt-0.5 shrink-0 text-success" />
            {item}
          </li>
        ))}
        {info.cannot.map((item) => (
          <li key={item} className="flex items-start gap-2.5 text-ink-soft">
            <LockIcon className="mt-0.5 shrink-0" />
            {item}
          </li>
        ))}
      </ul>
      {!user.twoFactorEnabled && (
        <p className="mt-5 flex items-start gap-2.5 border-t border-rule pt-4 font-sans text-sm text-warning">
          <AlertIcon className="mt-0.5 shrink-0" />
          <span>
            Verificação em duas etapas desligada.{" "}
            <Link href="/painel/conta" className="text-brand underline underline-offset-4">
              Ver segurança
            </Link>
          </span>
        </p>
      )}
    </aside>
  );
}

function FeedbackOverview() {
  const today = todayLocal();
  const summary = useApi<FeedbackSummary>(`/admin/feedback-sessions/summary?from=${addDays(today, -90)}&to=${today}`);
  const recent = useApi<Paged<FeedbackSessionListItem>>("/admin/feedback-sessions?pageSize=5");
  const nps = summary.data?.nps;

  return (
    <section aria-labelledby="avaliacoes" className="mt-14">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <Kicker>Últimos 90 dias</Kicker>
          <h2 id="avaliacoes" className="mt-1 text-3xl">
            Avaliações das turmas
          </h2>
        </div>
        <ButtonLink href="/painel/avaliacoes/turmas/nova" variant="outline">
          Nova turma
        </ButtonLink>
      </div>
      <div className="mt-6 grid gap-8 sm:grid-cols-3">
        <Stat label="Turmas" value={count(summary.data?.sessionCount)} />
        <Stat label="Respostas" value={count(summary.data?.responseCount)} />
        <Stat
          label="NPS"
          value={nps ? nps.score : "—"}
          note={nps ? plural(nps.total, "nota", "notas") : "Aparece com pelo menos 3 respostas por turma."}
        />
      </div>
      {recent.data && recent.data.items.length > 0 && (
        <ol className="mt-8 border-t-2 border-ink">
          {recent.data.items.map((session) => (
            <li key={session.id} className="border-b border-rule">
              <Link href={`/painel/avaliacoes/turmas/${session.id}`} className="flex flex-wrap items-baseline gap-x-4 gap-y-1 py-3 hover:text-brand">
                <span className="min-w-0 flex-1 text-lg">{session.title}</span>
                <span className="font-sans text-sm text-ink-soft">
                  {formatShortDate(session.opensAt)} · {plural(session.responseCount, "resposta", "respostas")}
                </span>
                {sessionStatusTag(session.status)}
              </Link>
            </li>
          ))}
        </ol>
      )}
    </section>
  );
}

export function Overview() {
  const { user, isAdmin } = useAuth();
  const services = useApi<AdminServiceListItem[]>("/admin/services");
  const drafts = useApi<Paged<AdminPostListItem>>("/admin/posts?status=Draft&pageSize=1");
  const published = useApi<Paged<AdminPostListItem>>("/admin/posts?status=Published&pageSize=1");
  const testimonials = useApi<AdminTestimonial[]>("/admin/testimonials");
  const media = useApi<Paged<MediaAsset>>("/admin/media?pageSize=100");

  const activeServices = services.data?.filter((service) => service.isActive).length;
  const visibleTestimonials = testimonials.data?.filter(
    (item) => item.isPublished && item.consentGivenAt && !item.consentRevokedAt,
  ).length;
  const withoutConsent = testimonials.data?.filter((item) => !item.consentGivenAt && !item.consentRevokedAt).length ?? 0;
  const withoutAlt = media.data?.items.filter((asset) => !asset.altText).length ?? 0;
  const draftCount = drafts.data?.totalItems ?? 0;

  return (
    <div>
      <header>
        <Kicker>Visão geral</Kicker>
        <h1 className="mt-2 text-[2.75rem] leading-[1.05] sm:text-6xl">
          {greetingFor(localHour())}, <em className="font-normal">{firstName(user.fullName)}.</em>
        </h1>
        <Rule variant="double" className="mt-6" />
      </header>

      <div className="mt-10 grid gap-12 lg:grid-cols-[1fr_20rem]">
        <div>
          <section aria-labelledby="no-site">
            <h2 id="no-site" className="sr-only">
              No site agora
            </h2>
            <div className="grid grid-cols-2 gap-8 sm:grid-cols-4">
              <Stat label="Serviços ativos" value={count(activeServices)} />
              <Stat label="Publicações" value={count(published.data?.totalItems)} note={draftCount ? plural(draftCount, "rascunho", "rascunhos") : undefined} />
              <Stat label="Depoimentos" value={count(visibleTestimonials)} note="no ar" />
              <Stat label="Imagens" value={count(media.data?.totalItems)} />
            </div>
          </section>

          <section aria-labelledby="pendencias" className="mt-14">
            <h2 id="pendencias" className="text-3xl">
              Pendências
            </h2>
            <ul className="mt-4 border-t-2 border-ink">
              {draftCount > 0 && (
                <Pending href="/painel/publicacoes?status=Draft">
                  {plural(draftCount, "publicação em rascunho", "publicações em rascunho")}
                </Pending>
              )}
              {withoutAlt > 0 && (
                <Pending href="/painel/imagens?sem-texto=1">
                  {plural(withoutAlt, "imagem sem texto alternativo", "imagens sem texto alternativo")} (acessibilidade)
                </Pending>
              )}
              {withoutConsent > 0 && (
                <Pending href="/painel/depoimentos">
                  {plural(withoutConsent, "depoimento sem consentimento registrado", "depoimentos sem consentimento registrado")}
                </Pending>
              )}
              {draftCount === 0 && withoutAlt === 0 && withoutConsent === 0 && (
                <li className="flex items-center gap-3 border-b border-rule py-3 font-sans text-[0.9375rem] text-ink-soft">
                  <CheckIcon className="text-success" /> Nada pendente por aqui.
                </li>
              )}
            </ul>
          </section>

          {isAdmin && <FeedbackOverview />}
        </div>

        <div className="grid content-start gap-8">
          <Masthead />
          <div>
            <Kicker>Atalhos</Kicker>
            <div className="mt-3 grid gap-3">
              <ButtonLink href="/painel/publicacoes/nova">Nova publicação</ButtonLink>
              <ButtonLink href="/painel/imagens" variant="outline">
                Enviar imagens
              </ButtonLink>
              <ButtonLink href="/" variant="quiet" target="_blank" rel="noopener">
                Ver o site
              </ButtonLink>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
