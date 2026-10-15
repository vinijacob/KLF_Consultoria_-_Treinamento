"use client";

import Link from "next/link";
import { useState } from "react";
import { auditAreas, describeAudit, type AuditEntry } from "@/lib/admin/audit";
import { toLocalInput } from "@/lib/admin/datetime";
import { useApi } from "@/lib/admin/hooks";
import { formatDay } from "@/lib/format";
import type { Paged, TeamMember } from "@/types/admin";
import { Input, Select } from "@/components/ui/field";
import { AdminOnly } from "./feedback/forms";
import { AdminHeader, EmptyState, LoadError, Loading, Pagination } from "./ui";

function groupByDay(entries: AuditEntry[]) {
  const groups: { day: string; entries: AuditEntry[] }[] = [];
  for (const entry of entries) {
    const day = toLocalInput(entry.occurredAt).slice(0, 10);
    const last = groups.at(-1);
    if (last?.day === day) last.entries.push(entry);
    else groups.push({ day, entries: [entry] });
  }
  return groups;
}

function AuditLogView() {
  const [area, setArea] = useState("");
  const [actorId, setActorId] = useState("");
  const [from, setFrom] = useState("");
  const [to, setTo] = useState("");
  const [page, setPage] = useState(1);
  const team = useApi<TeamMember[]>("/admin/users");

  const params = new URLSearchParams({ page: String(page), pageSize: "30" });
  if (area) params.set("area", area);
  if (actorId) params.set("actorId", actorId);
  if (from) params.set("from", from);
  if (to) params.set("to", to);
  const { data, error, reload } = useApi<Paged<AuditEntry>>(`/admin/audit-log?${params}`);
  const update = (set: (value: string) => void) => (value: string) => {
    setPage(1);
    set(value);
  };

  return (
    <div>
      <AdminHeader
        kicker="Administração"
        title="Atividades"
        description="Quem fez o quê no painel e quando. Respostas das avaliações nunca aparecem aqui, e depoimentos e mensagens vêm sem o nome da pessoa. Os registros são apagados depois de 2 anos."
      />
      <div className="mb-8 grid gap-4 sm:grid-cols-2 lg:grid-cols-4 lg:items-end">
        <Select label="Área" value={area} onChange={(event) => update(setArea)(event.target.value)}>
          <option value="">Todas</option>
          {auditAreas.map((item) => (
            <option key={item.value} value={item.value}>
              {item.label}
            </option>
          ))}
        </Select>
        <Select label="Pessoa" value={actorId} onChange={(event) => update(setActorId)(event.target.value)}>
          <option value="">Todas</option>
          {team.data?.map((member) => (
            <option key={member.id} value={member.id}>
              {member.fullName}
            </option>
          ))}
        </Select>
        <Input label="De" type="date" value={from} max={to || undefined} onChange={(event) => update(setFrom)(event.target.value)} />
        <Input label="Até" type="date" value={to} min={from || undefined} onChange={(event) => update(setTo)(event.target.value)} />
      </div>

      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.items.length === 0 && <EmptyState title="Nada registrado">Nenhuma atividade com esses filtros.</EmptyState>}
      {data && data.items.length > 0 && (
        <>
          <div className="grid gap-10">
            {groupByDay(data.items).map((group) => (
              <section key={group.day} aria-label={formatDay(group.day)}>
                <h2 className="border-b-2 border-ink pb-2 font-mono text-xs font-medium uppercase tracking-[0.14em] text-ink-soft">
                  {formatDay(group.day)}
                </h2>
                <ol className="divide-y divide-rule">
                  {group.entries.map((entry) => {
                    const line = describeAudit(entry);
                    return (
                      <li key={entry.id} className="flex gap-4 py-3 font-sans text-[0.9375rem]">
                        <time dateTime={entry.occurredAt} className="w-12 shrink-0 pt-0.5 font-mono text-xs text-ink-soft">
                          {toLocalInput(entry.occurredAt).slice(11)}
                        </time>
                        <p className="min-w-0">
                          <span className="font-semibold">{line.actor}</span> {line.text}
                          {line.href && (
                            <>
                              {" "}
                              <Link href={line.href} className="whitespace-nowrap text-brand underline underline-offset-4">
                                Abrir
                              </Link>
                            </>
                          )}
                        </p>
                      </li>
                    );
                  })}
                </ol>
              </section>
            ))}
          </div>
          <Pagination page={page} totalPages={data.totalPages} onChange={setPage} />
        </>
      )}
    </div>
  );
}

export function AuditLogPage() {
  return (
    <AdminOnly reason="O registro de atividades mostra o que cada pessoa fez no painel e fica só com a administração.">
      <AuditLogView />
    </AdminOnly>
  );
}
