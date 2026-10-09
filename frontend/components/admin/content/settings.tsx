"use client";

import { useState, type ReactNode } from "react";
import { optional } from "@/lib/admin/editor";
import { useApi, useSubmit } from "@/lib/admin/hooks";
import { adminFetch } from "@/lib/auth/session";
import { fieldError } from "@/lib/api/client";
import type { AboutSettings, ContactSettings, SeoSettings, SiteSettings, SocialSettings } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Input, Textarea } from "@/components/ui/field";
import { Notice } from "@/components/ui/notice";
import { MediaField } from "../media";
import { AdminHeader, FieldRow, LoadError, Loading, Section } from "../ui";

type Errors = Record<string, string[]> | undefined;

function Block<T>({
  settingKey,
  title,
  description,
  toPayload,
  children,
}: {
  settingKey: "about" | "contact" | "social" | "seo";
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
        void save();
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
  });
  const [contact, setContact] = useState({
    whatsapp: settings.contact?.whatsapp ?? "",
    email: settings.contact?.email ?? "",
    phone: settings.contact?.phone ?? "",
    address: settings.contact?.address ?? "",
    mapUrl: settings.contact?.mapUrl ?? "",
  });
  const [social, setSocial] = useState({
    instagram: settings.social?.instagram ?? "",
    linkedin: settings.social?.linkedin ?? "",
    facebook: settings.social?.facebook ?? "",
    youtube: settings.social?.youtube ?? "",
  });
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
        description="Textos da página Sobre e do trecho “Quem é” da página inicial."
        toPayload={() => ({
          mission: optional(about.mission),
          vision: optional(about.vision),
          values: lines(about.values),
          differentials: lines(about.differentials),
          history: optional(about.history),
        })}
      >
        {(errors) => (
          <>
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
              <Input
                label="WhatsApp"
                hint="Com código do país e DDD, ex.: 5592999999999."
                inputMode="tel"
                value={contact.whatsapp}
                onChange={(event) => setContact({ ...contact, whatsapp: event.target.value })}
                error={fieldError(errors, "whatsapp")}
              />
              <Input
                label="Telefone"
                optional
                hint="Como deve aparecer, ex.: +55 (92) 3333-3333."
                inputMode="tel"
                value={contact.phone}
                onChange={(event) => setContact({ ...contact, phone: event.target.value })}
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
        description="Links completos dos perfis, começando com https://."
        toPayload={() => ({
          instagram: optional(social.instagram),
          linkedin: optional(social.linkedin),
          facebook: optional(social.facebook),
          youtube: optional(social.youtube),
        })}
      >
        {(errors) => (
          <FieldRow>
            {(["instagram", "linkedin", "facebook", "youtube"] as const).map((network) => (
              <Input
                key={network}
                label={{ instagram: "Instagram", linkedin: "LinkedIn", facebook: "Facebook", youtube: "YouTube" }[network]}
                optional
                type="url"
                placeholder="https://"
                value={social[network]}
                onChange={(event) => setSocial({ ...social, [network]: event.target.value })}
                error={fieldError(errors, network)}
              />
            ))}
          </FieldRow>
        )}
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
    </>
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
