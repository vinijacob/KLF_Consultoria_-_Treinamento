"use client";

import { useState } from "react";
import { roleInfo } from "@/config/admin";
import { formatDateTime, formatShortDate } from "@/lib/admin/datetime";
import { setFlash, useApi, useSubmit } from "@/lib/admin/hooks";
import { adminFetch } from "@/lib/auth/session";
import { ApiError, fieldError } from "@/lib/api/client";
import type { Role, TeamInvite, TeamMember } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Checkbox, Input, Select } from "@/components/ui/field";
import { Notice } from "@/components/ui/notice";
import { useAuth } from "./auth-provider";
import { FlashNotice } from "./entity";
import { AdminOnly } from "./feedback/forms";
import { AdminForm, AdminHeader, ConfirmButton, FieldRow, IndexList, IndexRow, LoadError, Loading, NewButton, Section, Tag } from "./ui";

const LIST = "/painel/equipe";
const roles: Role[] = ["Admin", "Editor"];
const reason = "Convidar pessoas e mudar perfis é uma tarefa da administração.";

function statusTags(member: TeamMember) {
  return (
    <>
      <Tag tone={member.role === "Admin" ? "brand" : "neutral"}>{member.role ? roleInfo[member.role].label : "Sem perfil"}</Tag>
      {!member.isActive && <Tag tone="danger">Desativada</Tag>}
      {member.isActive && member.invitePending && <Tag tone="warning">Convite pendente</Tag>}
      {member.isActive && !member.invitePending && !member.twoFactorEnabled && <Tag tone="warning">Sem 2 etapas</Tag>}
    </>
  );
}

function RoleSelect({ value, onChange, disabled, hint, error }: { value: Role; onChange: (role: Role) => void; disabled?: boolean; hint?: string; error?: string }) {
  return (
    <Select label="Perfil" value={value} disabled={disabled} hint={hint ?? roleInfo[value].summary} onChange={(event) => onChange(event.target.value as Role)} error={error}>
      {roles.map((role) => (
        <option key={role} value={role}>
          {roleInfo[role].label}
        </option>
      ))}
    </Select>
  );
}

/** O link do convite, para copiar e mandar por outro meio (WhatsApp) enquanto o e-mail não estiver configurado. */
function InviteLinkBox({ invite }: { invite: TeamInvite }) {
  const [copied, setCopied] = useState(false);
  const message = `Olá, ${invite.user.fullName.split(" ")[0]}! Este é o seu convite para o painel da KLF: ${invite.inviteLink}`;

  async function copy() {
    try {
      await navigator.clipboard.writeText(invite.inviteLink);
      setCopied(true);
    } catch {
      setCopied(false);
    }
  }

  return (
    <Notice tone="success" title={`Convite pronto para ${invite.user.fullName}`} role="status" className="mb-8">
      <p>
        O link também segue por e-mail para {invite.user.email}. Se a pessoa não receber, copie e envie por outro meio. Ele vale
        até {formatDateTime(invite.expiresAt)} e funciona uma vez só.
      </p>
      <input
        readOnly
        aria-label="Link do convite"
        value={invite.inviteLink}
        onFocus={(event) => event.currentTarget.select()}
        className="mt-3 w-full rounded-md border border-rule-strong bg-card px-3 py-2 font-mono text-xs"
      />
      <div className="mt-3 flex flex-wrap items-center gap-3">
        <Button type="button" variant="outline" onClick={() => void copy()}>
          {copied ? "Link copiado" : "Copiar link"}
        </Button>
        <a
          href={`https://wa.me/?text=${encodeURIComponent(message)}`}
          target="_blank"
          rel="noopener noreferrer"
          className="inline-flex min-h-11 items-center font-sans text-sm text-brand underline underline-offset-4"
        >
          Enviar pelo WhatsApp<span className="sr-only"> (abre em nova aba)</span>
        </a>
      </div>
    </Notice>
  );
}

function TeamListView() {
  const { user } = useAuth();
  const { data, error, reload } = useApi<TeamMember[]>("/admin/users");

  return (
    <div>
      <AdminHeader
        kicker="Administração"
        title="Equipe"
        description="Quem entra no painel. Cada pessoa recebe um convite, escolhe a própria senha e configura a verificação em duas etapas."
        actions={<NewButton href={`${LIST}/convidar`}>Convidar pessoa</NewButton>}
      />
      <FlashNotice />
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data && (
        <IndexList>
          {data.map((member) => (
            <IndexRow
              key={member.id}
              href={`${LIST}/${member.id}`}
              title={member.id === user.id ? `${member.fullName} (você)` : member.fullName}
              meta={`${member.email} · desde ${formatShortDate(member.createdAt)}`}
              tags={statusTags(member)}
            />
          ))}
        </IndexList>
      )}
    </div>
  );
}

function InviteView() {
  const [form, setForm] = useState({ fullName: "", email: "", role: "Editor" as Role });
  const [invite, setInvite] = useState<TeamInvite | null>(null);
  const { pending, errors, message, run } = useSubmit();

  async function submit() {
    const created = await run(() => adminFetch<TeamInvite>("/admin/users", { method: "POST", json: form }));
    if (created) {
      setInvite(created);
      setForm({ fullName: "", email: "", role: "Editor" });
      window.scrollTo({ top: 0 });
    }
  }

  return (
    <>
      <AdminHeader kicker="Equipe" title="Convidar pessoa" back={{ href: LIST, label: "Toda a equipe" }} />
      {invite && <InviteLinkBox invite={invite} />}
      <AdminForm pending={pending} message={message} submitLabel="Criar convite" onSubmit={() => void submit()}>
        <Section title="Quem" description="A pessoa entra com este e-mail e escolhe a própria senha pelo link do convite.">
          <FieldRow>
            <Input label="Nome" value={form.fullName} maxLength={150} onChange={(event) => setForm({ ...form, fullName: event.target.value })} error={fieldError(errors, "fullName")} />
            <Input label="E-mail" type="email" value={form.email} maxLength={256} onChange={(event) => setForm({ ...form, email: event.target.value })} error={fieldError(errors, "email")} />
          </FieldRow>
          <FieldRow>
            <RoleSelect value={form.role} onChange={(role) => setForm({ ...form, role })} error={fieldError(errors, "role")} />
          </FieldRow>
        </Section>
      </AdminForm>
    </>
  );
}

function MemberForm({ member, onChange }: { member: TeamMember; onChange: (member: TeamMember) => void }) {
  const { user } = useAuth();
  const self = member.id === user.id;
  const [form, setForm] = useState({ fullName: member.fullName, role: (member.role || "Editor") as Role, isActive: member.isActive });
  const [invite, setInvite] = useState<TeamInvite | null>(null);
  const [problem, setProblem] = useState<string | null>(null);
  const { pending, errors, message, run } = useSubmit();

  async function save() {
    const saved = await run(() => adminFetch<TeamMember>(`/admin/users/${member.id}`, { method: "PUT", json: form }));
    if (saved) {
      setFlash(`Alterações de ${saved.fullName} salvas.`);
      onChange(saved);
    }
  }

  async function resendInvite() {
    setProblem(null);
    try {
      setInvite(await adminFetch<TeamInvite>(`/admin/users/${member.id}/invite`, { method: "POST" }));
      window.scrollTo({ top: 0 });
    } catch (failure) {
      setProblem(failure instanceof ApiError ? failure.message : "Não foi possível gerar o convite. Tente de novo.");
    }
  }

  async function resetTwoFactor() {
    try {
      onChange(await adminFetch<TeamMember>(`/admin/users/${member.id}/reset-two-factor`, { method: "POST" }));
      setFlash(`Verificação em duas etapas de ${member.fullName} redefinida. No próximo acesso, a pessoa configura o aplicativo de novo.`);
      return null;
    } catch (failure) {
      return failure instanceof ApiError ? failure.message : "Não foi possível redefinir. Tente de novo.";
    }
  }

  return (
    <>
      <AdminHeader
        kicker="Equipe"
        title={self ? `${member.fullName} (você)` : member.fullName}
        back={{ href: LIST, label: "Toda a equipe" }}
        description={<span className="flex flex-wrap items-center gap-2">{statusTags(member)}</span>}
      />
      <FlashNotice />
      {invite && <InviteLinkBox invite={invite} />}
      {problem && (
        <Notice tone="error" role="alert" className="mb-8">
          {problem}
        </Notice>
      )}
      <AdminForm
        pending={pending}
        message={message}
        submitLabel="Salvar alterações"
        onSubmit={() => void save()}
        extraActions={
          !self &&
          member.isActive &&
          member.twoFactorEnabled && (
            <ConfirmButton
              label="Redefinir verificação em duas etapas"
              title="Redefinir a verificação em duas etapas?"
              description={
                <p>
                  Use quando {member.fullName} perder o celular ou trocar de aparelho. As sessões abertas são encerradas e, no
                  próximo acesso, a pessoa configura o aplicativo autenticador de novo. Ela recebe um aviso por e-mail.
                </p>
              }
              confirmLabel="Redefinir"
              variant="quiet"
              onConfirm={resetTwoFactor}
            />
          )
        }
      >
        <Section title="Dados" description={`E-mail de acesso: ${member.email}. Para trocar o e-mail, convide de novo com o endereço novo e desative esta conta.`}>
          <FieldRow>
            <Input label="Nome" value={form.fullName} maxLength={150} onChange={(event) => setForm({ ...form, fullName: event.target.value })} error={fieldError(errors, "fullName")} />
          </FieldRow>
          <FieldRow>
            <RoleSelect
              value={form.role}
              disabled={self}
              hint={self ? "Ninguém muda o próprio perfil. Peça a outra pessoa da administração." : undefined}
              onChange={(role) => setForm({ ...form, role })}
              error={fieldError(errors, "role")}
            />
          </FieldRow>
        </Section>
        <Section title="Acesso" description="Desativar bloqueia a entrada e encerra as sessões abertas. A conta continua na lista e pode ser reativada.">
          <Checkbox
            label="Conta ativa"
            hint={self ? "Você não pode desativar a própria conta." : undefined}
            checked={form.isActive}
            disabled={self}
            onChange={(event) => setForm({ ...form, isActive: event.target.checked })}
          />
          {member.invitePending && member.isActive && (
            <div className="grid gap-3">
              <p className="font-sans text-[0.9375rem] text-ink-soft">
                {member.fullName} ainda não aceitou o convite. Gere um link novo se o anterior venceu ou se perdeu.
              </p>
              <div>
                <Button type="button" variant="outline" onClick={() => void resendInvite()}>
                  Gerar convite de novo
                </Button>
              </div>
            </div>
          )}
        </Section>
      </AdminForm>
    </>
  );
}

function MemberView({ id }: { id: string }) {
  const { data, error, reload } = useApi<TeamMember>(`/admin/users/${id}`);
  const [member, setMember] = useState<TeamMember | null>(null);
  const current = member ?? data;

  if (error) return <LoadError error={error} onRetry={reload} />;
  if (!current) return <Loading />;

  return <MemberForm key={`${current.id}-${current.role}-${current.isActive}-${current.twoFactorEnabled}`} member={current} onChange={setMember} />;
}

export function TeamList() {
  return (
    <AdminOnly reason={reason}>
      <TeamListView />
    </AdminOnly>
  );
}

export function TeamInvitePage() {
  return (
    <AdminOnly reason={reason}>
      <InviteView />
    </AdminOnly>
  );
}

export function TeamMemberPage({ id }: { id: string }) {
  return (
    <AdminOnly reason={reason}>
      <MemberView id={id} />
    </AdminOnly>
  );
}
