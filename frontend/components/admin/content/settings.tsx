"use client";

import { useId, useRef, useState, type ReactNode } from "react";
import { optional } from "@/lib/admin/editor";
import { useApi, useSubmit } from "@/lib/admin/hooks";
import { adminFetch } from "@/lib/auth/session";
import { fieldError } from "@/lib/api/client";
import { formatCnpj } from "@/lib/cnpj";
import { formatDay } from "@/lib/format";
import { phoneFieldValue } from "@/lib/phone";
import type { AboutSettings, ContactSettings, LegalSettings, SeoSettings, SiteSettings, SocialSettings } from "@/types/admin";
import type { SocialLink } from "@/types/site";
import { Button } from "@/components/ui/button";
import { CnpjInput, FieldError, Input, PhoneInput, Textarea } from "@/components/ui/field";
import { ArrowDownIcon, ArrowUpIcon, CloseIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { MediaField, MediaListField } from "../media";
import { AdminHeader, FieldRow, LoadError, Loading, Section } from "../ui";

type Errors = Record<string, string[]> | undefined;

function Block<T>({
  settingKey,
  title,
  description,
  toPayload,
  children,
}: {
  settingKey: "about" | "contact" | "social" | "seo" | "legal";
  title: string;
  description?: ReactNode;
  toPayload: () => T;
  children: (errors: Errors) => ReactNode;
}) {
  const { pending, errors, message, run } = useSubmit();
  const [saved, setSaved] = useState(false);

  async function save() {
    setSaved(false);
    const result = await run(() => adminFetch(`/admin/settings/${settingKey}`, { method: "PUT", json: toPayload() }));
    if (result !== undefined) setSaved(true);
  }

  return (
    <form
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        if (event.target === event.currentTarget) void save();
      }}
    >
      <Section title={title} description={description}>
        {children(errors)}
        {message && (
          <Notice tone="error" role="alert">
            {message}
          </Notice>
        )}
        <div className="flex flex-wrap items-center gap-4">
          <Button type="submit" disabled={pending}>
            {pending ? "Salvando…" : `Salvar ${title.toLowerCase()}`}
          </Button>
          {saved && (
            <p role="status" className="font-sans text-sm text-success">
              Salvo. O site atualiza em até 5 minutos.
            </p>
          )}
        </div>
      </Section>
    </form>
  );
}

type SocialRow = SocialLink & { key: string };

let rowCounter = 0;
const newRowKey = () => `rede-${++rowCounter}`;

const MAX_SOCIAL_LINKS = 12;
const socialSuggestions = ["Instagram", "LinkedIn", "Facebook", "YouTube", "TikTok", "X (Twitter)", "Threads", "Pinterest", "Spotify", "Kwai"];

/**
 * Redes sociais em lista livre: nome + link, até 12, com mover e tirar. Linhas totalmente vazias não são enviadas; os
 * erros da API (`Links[0].Url`) usam o índice da lista enviada, então `sentKeys` guarda quais linhas foram e em que
 * ordem: o erro continua na linha certa mesmo se ela for movida depois.
 */
function SocialLinksField({
  rows,
  onChange,
  sentKeys,
  errors,
}: {
  rows: SocialRow[];
  onChange: (rows: SocialRow[]) => void;
  sentKeys: string[];
  errors: Errors;
}) {
  const listId = useId();
  const update = (key: string, patch: Partial<SocialLink>) => onChange(rows.map((row) => (row.key === key ? { ...row, ...patch } : row)));
  const move = (from: number, to: number) => {
    const next = [...rows];
    [next[from], next[to]] = [next[to], next[from]];
    onChange(next);
  };
  const rowError = (row: SocialRow, field: "name" | "url") => {
    const index = sentKeys.indexOf(row.key);
    return index < 0 ? undefined : fieldError(errors, `links[${index}].${field}`);
  };

  return (
    <div className="grid gap-4">
      <datalist id={listId}>
        {socialSuggestions
          .filter((name) => !rows.some((row) => row.name.trim().toLowerCase() === name.toLowerCase()))
          .map((name) => (
            <option key={name} value={name} />
          ))}
      </datalist>
      {rows.length === 0 && <p className="font-sans text-[0.9375rem] text-ink-soft">Nenhuma rede ainda. No site, o rodapé mostra “Em breve”.</p>}
      {rows.map((row, index) => (
        <fieldset key={row.key} className="grid gap-3 rounded-2xl border border-rule bg-card p-4 sm:grid-cols-[minmax(0,11rem)_minmax(0,1fr)_auto] sm:items-start">
          <legend className="sr-only">Rede social {index + 1}</legend>
          <Input
            label="Nome da rede"
            list={listId}
            placeholder="Ex.: Instagram"
            value={row.name}
            maxLength={40}
            onChange={(event) => update(row.key, { name: event.target.value })}
            error={rowError(row, "name")}
          />
          <Input
            label="Link do perfil"
            type="url"
            placeholder="https://"
            value={row.url}
            maxLength={300}
            onChange={(event) => update(row.key, { url: event.target.value })}
            error={rowError(row, "url")}
          />
          <div className="flex sm:mt-7">
            <button
              type="button"
              disabled={index === 0}
              onClick={() => move(index, index - 1)}
              aria-label={`Subir ${row.name || `a rede ${index + 1}`}`}
              className="grid size-11 place-items-center hover:text-brand disabled:opacity-40"
            >
              <ArrowUpIcon />
            </button>
            <button
              type="button"
              disabled={index === rows.length - 1}
              onClick={() => move(index, index + 1)}
              aria-label={`Descer ${row.name || `a rede ${index + 1}`}`}
              className="grid size-11 place-items-center hover:text-brand disabled:opacity-40"
            >
              <ArrowDownIcon />
            </button>
            <button
              type="button"
              onClick={() => onChange(rows.filter((item) => item.key !== row.key))}
              aria-label={`Tirar ${row.name || `a rede ${index + 1}`}`}
              className="grid size-11 place-items-center hover:text-danger"
            >
              <CloseIcon />
            </button>
          </div>
        </fieldset>
      ))}
      <FieldError>{fieldError(errors, "links")}</FieldError>
      <div>
        <Button
          variant="outline"
          disabled={rows.length >= MAX_SOCIAL_LINKS}
          onClick={() => onChange([...rows, { key: newRowKey(), name: "", url: "" }])}
        >
          {rows.length >= MAX_SOCIAL_LINKS ? `Limite de ${MAX_SOCIAL_LINKS} redes` : "Adicionar rede social"}
        </Button>
      </div>
    </div>
  );
}

const lines = (text: string) =>
  text
    .split("\n")
    .map((line) => line.trim())
    .filter(Boolean);

function SettingsForms({ settings }: { settings: SiteSettings }) {
  const [about, setAbout] = useState({
    mission: settings.about?.mission ?? "",
    vision: settings.about?.vision ?? "",
    values: (settings.about?.values ?? []).join("\n"),
    differentials: (settings.about?.differentials ?? []).join("\n"),
    history: settings.about?.history ?? "",
    portraitIds: settings.about?.portraitIds ?? [],
  });
  const [contact, setContact] = useState({
    whatsapp: phoneFieldValue(settings.contact?.whatsapp),
    email: settings.contact?.email ?? "",
    phone: phoneFieldValue(settings.contact?.phone),
    address: settings.contact?.address ?? "",
    mapUrl: settings.contact?.mapUrl ?? "",
  });
  const [social, setSocial] = useState<SocialRow[]>(() => (settings.social?.links ?? []).map((link) => ({ key: newRowKey(), ...link })));
  const socialSentKeys = useRef<string[]>([]);
  const [seo, setSeo] = useState({
    title: settings.seo?.title ?? "",
    description: settings.seo?.description ?? "",
    shareImageId: settings.seo?.shareImageId ?? null,
  });

  return (
    <>
      <Block<AboutSettings>
        settingKey="about"
        title="Sobre"
        description="Retrato e textos da página Sobre e do trecho “Quem é” da página inicial."
        toPayload={() => ({
          mission: optional(about.mission),
          vision: optional(about.vision),
          values: lines(about.values),
          differentials: lines(about.differentials),
          history: optional(about.history),
          portraitIds: about.portraitIds,
        })}
      >
        {(errors) => (
          <>
            <MediaListField
              label="Fotos da Kilciene"
              hint="Viram um carrossel no topo da página inicial e na página Sobre, nesta ordem; a primeira também aparece como foto pequena. Prefira fotos verticais (proporção 4:5), com boa luz."
              value={about.portraitIds}
              onChange={(ids) => setAbout({ ...about, portraitIds: ids })}
              max={10}
              error={fieldError(errors, "portraitIds")}
            />
            <Textarea
              label="História"
              hint="Texto corrido, com parágrafos separados por uma linha em branco."
              value={about.history}
              maxLength={5000}
              onChange={(event) => setAbout({ ...about, history: event.target.value })}
              error={fieldError(errors, "history")}
              className="min-h-56"
            />
            <FieldRow>
              <Textarea label="Missão" value={about.mission} maxLength={1000} onChange={(event) => setAbout({ ...about, mission: event.target.value })} error={fieldError(errors, "mission")} />
              <Textarea label="Visão" value={about.vision} maxLength={1000} onChange={(event) => setAbout({ ...about, vision: event.target.value })} error={fieldError(errors, "vision")} />
            </FieldRow>
            <FieldRow>
              <Textarea
                label="Valores"
                hint="Um por linha (até 20)."
                value={about.values}
                onChange={(event) => setAbout({ ...about, values: event.target.value })}
                error={fieldError(errors, "values")}
              />
              <Textarea
                label="Diferenciais"
                hint="Um por linha (até 20)."
                value={about.differentials}
                onChange={(event) => setAbout({ ...about, differentials: event.target.value })}
                error={fieldError(errors, "differentials")}
              />
            </FieldRow>
          </>
        )}
      </Block>

      <Block<ContactSettings>
        settingKey="contact"
        title="Contato"
        description="Usados nos botões de WhatsApp, na página Contato e no rodapé."
        toPayload={() => ({
          whatsapp: optional(contact.whatsapp.replace(/\D/g, "")),
          email: optional(contact.email),
          phone: optional(contact.phone),
          address: optional(contact.address),
          mapUrl: optional(contact.mapUrl),
        })}
      >
        {(errors) => (
          <>
            <FieldRow>
              <PhoneInput
                label="WhatsApp"
                hint="Número do WhatsApp da KLF, com DDD."
                value={contact.whatsapp}
                onValueChange={(whatsapp) => setContact({ ...contact, whatsapp })}
                error={fieldError(errors, "whatsapp")}
              />
              <PhoneInput
                label="Telefone"
                optional
                hint="Aparece no site exatamente assim."
                value={contact.phone}
                onValueChange={(phone) => setContact({ ...contact, phone })}
                error={fieldError(errors, "phone")}
              />
            </FieldRow>
            <Input label="E-mail" type="email" value={contact.email} maxLength={200} onChange={(event) => setContact({ ...contact, email: event.target.value })} error={fieldError(errors, "email")} />
            <Input label="Endereço" optional value={contact.address} maxLength={300} onChange={(event) => setContact({ ...contact, address: event.target.value })} error={fieldError(errors, "address")} />
            <Input
              label="Link do mapa"
              optional
              type="url"
              placeholder="https://"
              value={contact.mapUrl}
              onChange={(event) => setContact({ ...contact, mapUrl: event.target.value })}
              error={fieldError(errors, "mapUrl")}
            />
          </>
        )}
      </Block>

      <Block<SocialSettings>
        settingKey="social"
        title="Redes sociais"
        description="Qualquer rede: escreva o nome como deve aparecer no site e o link completo do perfil (começando com https://). A ordem aqui é a ordem do rodapé."
        toPayload={() => {
          const sent = social.filter((row) => row.name.trim() || row.url.trim());
          socialSentKeys.current = sent.map((row) => row.key);
          return { links: sent.map((row) => ({ name: row.name.trim(), url: row.url.trim() })) };
        }}
      >
        {(errors) => <SocialLinksField rows={social} onChange={setSocial} sentKeys={socialSentKeys.current} errors={errors} />}
      </Block>

      <Block<SeoSettings>
        settingKey="seo"
        title="Busca e compartilhamento"
        description="Como o site aparece no Google e ao ser compartilhado no WhatsApp e nas redes."
        toPayload={() => ({ title: optional(seo.title), description: optional(seo.description), shareImageId: seo.shareImageId })}
      >
        {(errors) => (
          <>
            <Input label="Título" optional value={seo.title} maxLength={60} onChange={(event) => setSeo({ ...seo, title: event.target.value })} error={fieldError(errors, "title")} />
            <Textarea
              label="Descrição"
              optional
              hint="Até 160 caracteres."
              value={seo.description}
              maxLength={160}
              onChange={(event) => setSeo({ ...seo, description: event.target.value })}
              error={fieldError(errors, "description")}
              className="min-h-20"
            />
            <MediaField
              label="Imagem de compartilhamento"
              hint="Aparece na prévia do link. Ideal: 1200 × 630."
              value={seo.shareImageId}
              onChange={(id) => setSeo({ ...seo, shareImageId: id })}
              error={fieldError(errors, "shareImageId")}
            />
          </>
        )}
      </Block>

      <LegalBlock initial={settings.legal} />
    </>
  );
}

function LegalBlock({ initial }: { initial?: LegalSettings }) {
  const [legal, setLegal] = useState({
    companyName: initial?.companyName ?? "",
    cnpj: formatCnpj(initial?.cnpj ?? ""),
    address: initial?.address ?? "",
    forumCity: initial?.forumCity ?? "",
    dpoName: initial?.dpoName ?? "",
    dpoEmail: initial?.dpoEmail ?? "",
  });
  const set = (key: keyof typeof legal) => (event: { target: { value: string } }) => setLegal({ ...legal, [key]: event.target.value });

  return (
    <Block<LegalSettings>
      settingKey="legal"
      title="Dados legais"
      description={
        <>
          Aparecem na política de privacidade, nos termos de uso e na política de cookies. Enquanto faltar algum, essas páginas
          avisam o que falta e ficam fora do Google.
          {initial?.updatedOn && <> Última alteração: {formatDay(initial.updatedOn)}.</>}
        </>
      }
      toPayload={() => ({
        companyName: optional(legal.companyName),
        cnpj: optional(legal.cnpj),
        address: optional(legal.address),
        forumCity: optional(legal.forumCity),
        dpoName: optional(legal.dpoName),
        dpoEmail: optional(legal.dpoEmail),
      })}
    >
      {(errors) => (
        <>
          <FieldRow>
            <Input label="Razão social" value={legal.companyName} maxLength={200} onChange={set("companyName")} error={fieldError(errors, "companyName")} />
            <CnpjInput
              label="CNPJ"
              hint="Aceita o CNPJ novo, com letras."
              value={legal.cnpj}
              onValueChange={(cnpj) => setLegal({ ...legal, cnpj })}
              error={fieldError(errors, "cnpj")}
            />
          </FieldRow>
          <Input label="Endereço da sede" value={legal.address} maxLength={300} onChange={set("address")} error={fieldError(errors, "address")} />
          <FieldRow>
            <Input
              label="Cidade do foro"
              hint="Cidade e estado, como em Manaus/AM."
              value={legal.forumCity}
              maxLength={83}
              onChange={set("forumCity")}
              error={fieldError(errors, "forumCity")}
            />
          </FieldRow>
          <FieldRow>
            <Input
              label="Encarregado pelos dados (DPO)"
              hint="Quem responde aos pedidos sobre dados pessoais. Pode ser a própria Kilciene."
              value={legal.dpoName}
              maxLength={120}
              onChange={set("dpoName")}
              error={fieldError(errors, "dpoName")}
            />
            <Input
              label="E-mail do encarregado"
              type="email"
              value={legal.dpoEmail}
              maxLength={200}
              onChange={set("dpoEmail")}
              error={fieldError(errors, "dpoEmail")}
            />
          </FieldRow>
        </>
      )}
    </Block>
  );
}

export function SiteSettingsPage() {
  const { data, error, reload } = useApi<SiteSettings>("/admin/settings");

  return (
    <div>
      <AdminHeader kicker="Conteúdo do site" title="Dados do site" description="Textos e contatos que aparecem em várias páginas." />
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data && <SettingsForms settings={data} />}
    </div>
  );
}
