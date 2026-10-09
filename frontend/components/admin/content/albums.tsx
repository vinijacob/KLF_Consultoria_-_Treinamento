"use client";

import Image from "next/image";
import { useState } from "react";
import { optional, toInt, useEntityEditor } from "@/lib/admin/editor";
import { useApi, useSubmit } from "@/lib/admin/hooks";
import { plural, slugify } from "@/lib/admin/text";
import { adminFetch } from "@/lib/auth/session";
import { fieldError } from "@/lib/api/client";
import { move } from "@/lib/feedback/builder";
import type { AdminAlbum, AdminAlbumListItem } from "@/types/admin";
import { Button } from "@/components/ui/button";
import { Checkbox, Input, Textarea } from "@/components/ui/field";
import { ArrowDownIcon, ArrowUpIcon } from "@/components/ui/icons";
import { Notice } from "@/components/ui/notice";
import { EntityLoader, FlashNotice } from "../entity";
import { AltTextTag, MediaField, MediaPicker, Thumb } from "../media";
import { visibilityTag } from "../tags";
import {
  AdminForm,
  AdminHeader,
  ConfirmButton,
  Dialog,
  EmptyState,
  FieldRow,
  IndexList,
  IndexRow,
  LoadError,
  Loading,
  NewButton,
  Section,
} from "../ui";

const LIST = "/painel/galeria";

export function AlbumsList() {
  const { data, error, reload } = useApi<AdminAlbumListItem[]>("/admin/albums");

  return (
    <div>
      <AdminHeader
        kicker="Conteúdo do site"
        title="Galeria"
        description="Álbuns de fotos de treinamentos e palestras, na página Galeria."
        actions={<NewButton href={`${LIST}/novo`}>Novo álbum</NewButton>}
      />
      <FlashNotice />
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.length === 0 && (
        <EmptyState title="Nenhum álbum ainda" action={<NewButton href={`${LIST}/novo`}>Criar álbum</NewButton>}>
          Crie um álbum e depois escolha as fotos.
        </EmptyState>
      )}
      {data && data.length > 0 && (
        <IndexList>
          {data.map((album) => (
            <IndexRow
              key={album.id}
              href={`${LIST}/${album.id}`}
              number={album.displayOrder}
              title={album.title}
              meta={`${plural(album.itemCount, "foto", "fotos")} · /galeria/${album.slug}`}
              aside={album.coverUrl ? <Thumb src={album.coverUrl} alt="" className="hidden w-24 sm:block" /> : undefined}
              tags={visibilityTag(album.isActive)}
            />
          ))}
        </IndexList>
      )}
    </div>
  );
}

type Item = { mediaAssetId: string; url: string; altText?: string | null; caption: string };

function AlbumItems({ album }: { album: AdminAlbum }) {
  const [items, setItems] = useState<Item[]>(() =>
    album.items.map((item) => ({ mediaAssetId: item.mediaAssetId, url: item.url, altText: item.altText, caption: item.caption ?? "" })),
  );
  const [picking, setPicking] = useState(false);
  const [dirty, setDirty] = useState(false);
  const [saved, setSaved] = useState(false);
  const { pending, errors, message, run } = useSubmit();

  function update(next: Item[]) {
    setItems(next);
    setDirty(true);
    setSaved(false);
  }

  async function save() {
    const result = await run(() =>
      adminFetch<AdminAlbum>(`/admin/albums/${album.id}/items`, {
        method: "PUT",
        json: { items: items.map((item) => ({ mediaAssetId: item.mediaAssetId, caption: optional(item.caption) })) },
      }),
    );
    if (result) {
      setDirty(false);
      setSaved(true);
    }
  }

  return (
    <Section
      title="Fotos"
      description={
        <>
          {plural(items.length, "foto", "fotos")}, na ordem em que aparecem no site. Salve as fotos separadamente dos dados do
          álbum.
        </>
      }
    >
      {items.length === 0 && <EmptyState title="Álbum sem fotos">Adicione fotos da biblioteca de imagens.</EmptyState>}
      <ol className="grid gap-4">
        {items.map((item, index) => (
          <li key={item.mediaAssetId} className="grid grid-cols-[6rem_1fr] gap-4 border-b border-rule pb-4 sm:grid-cols-[8rem_1fr_auto]">
            <div className="border border-ink p-1">
              <div className="relative aspect-[4/3] bg-paper-deep">
                <Image src={item.url} alt="" fill unoptimized sizes="128px" className="object-cover" />
              </div>
            </div>
            <div className="grid content-start gap-2">
              <Input
                label={`Legenda da foto ${index + 1}`}
                optional
                value={item.caption}
                maxLength={300}
                onChange={(event) => update(items.map((current, i) => (i === index ? { ...current, caption: event.target.value } : current)))}
                error={fieldError(errors, `items[${index}].caption`)}
              />
              <AltTextTag altText={item.altText} />
            </div>
            <div className="col-span-2 flex gap-2 sm:col-span-1 sm:flex-col">
              <Button variant="outline" aria-label={`Subir foto ${index + 1}`} disabled={index === 0} onClick={() => update(move(items, index, index - 1))}>
                <ArrowUpIcon />
              </Button>
              <Button
                variant="outline"
                aria-label={`Descer foto ${index + 1}`}
                disabled={index === items.length - 1}
                onClick={() => update(move(items, index, index + 1))}
              >
                <ArrowDownIcon />
              </Button>
              <Button variant="quiet" className="text-danger" onClick={() => update(items.filter((_, i) => i !== index))}>
                Tirar
              </Button>
            </div>
          </li>
        ))}
      </ol>
      {message && (
        <Notice tone="error" role="alert">
          {message}
        </Notice>
      )}
      <div className="flex flex-wrap items-center gap-3">
        <Button variant="outline" onClick={() => setPicking(true)}>
          Adicionar fotos
        </Button>
        <Button onClick={() => void save()} disabled={pending || !dirty}>
          {pending ? "Salvando…" : "Salvar fotos"}
        </Button>
        {dirty && <p className="font-sans text-sm text-warning">Há mudanças nas fotos ainda não salvas.</p>}
        {saved && (
          <p role="status" className="font-sans text-sm text-success">
            Fotos salvas.
          </p>
        )}
      </div>
      <Dialog open={picking} onClose={() => setPicking(false)} title="Adicionar fotos ao álbum" wide>
        <p className="mb-6 font-sans text-sm text-ink-soft">Clique nas fotos para adicionar. Feche quando terminar e salve.</p>
        <MediaPicker
          picked={items.map((item) => item.mediaAssetId)}
          onPick={(asset) =>
            update([...items, { mediaAssetId: asset.id, url: asset.url, altText: asset.altText, caption: "" }])
          }
        />
      </Dialog>
    </Section>
  );
}

type Form = { title: string; slug: string; description: string; coverId: string | null; displayOrder: string; isActive: boolean };

function AlbumForm({ album }: { album?: AdminAlbum }) {
  const [form, setForm] = useState<Form>(() => ({
    title: album?.title ?? "",
    slug: album?.slug ?? "",
    description: album?.description ?? "",
    coverId: album?.coverId ?? null,
    displayOrder: String(album?.displayOrder ?? 0),
    isActive: album?.isActive ?? true,
  }));
  const [slugTouched, setSlugTouched] = useState(Boolean(album));
  const editor = useEntityEditor<AdminAlbum>({
    apiPath: "/admin/albums",
    listHref: LIST,
    id: album?.id,
    created: "Álbum criado. Agora adicione as fotos.",
    deleted: "Álbum excluído.",
  });
  const error = (field: string) => fieldError(editor.errors, field);
  const set = <K extends keyof Form>(key: K, value: Form[K]) => setForm((current) => ({ ...current, [key]: value }));

  return (
    <>
      <AdminHeader
        kicker="Galeria"
        title={album ? album.title : "Novo álbum"}
        back={{ href: LIST, label: "Todos os álbuns" }}
        description={album && <>No site: /galeria/{album.slug}</>}
      />
      {album && <AlbumItems album={album} />}
      <AdminForm
        pending={editor.pending}
        message={editor.message}
        savedNote={editor.savedNote}
        submitLabel={album ? "Salvar dados do álbum" : "Criar álbum"}
        onSubmit={() =>
          void editor.save({ ...form, description: optional(form.description), displayOrder: toInt(form.displayOrder) })
        }
        extraActions={
          album && (
            <ConfirmButton
              label="Excluir álbum"
              title="Excluir este álbum?"
              description={<p>O álbum sai do site. As fotos continuam na biblioteca de imagens.</p>}
              confirmLabel="Excluir"
              onConfirm={editor.remove}
            />
          )
        }
      >
        <Section title="Álbum">
          <Input
            label="Título"
            value={form.title}
            maxLength={200}
            onChange={(event) => {
              const title = event.target.value;
              setForm((current) => ({ ...current, title, slug: slugTouched ? current.slug : slugify(title) }));
            }}
            error={error("title")}
          />
          <Input
            label="Endereço (slug)"
            value={form.slug}
            maxLength={200}
            onChange={(event) => {
              setSlugTouched(true);
              set("slug", event.target.value);
            }}
            error={error("slug")}
          />
          <Textarea
            label="Descrição"
            optional
            value={form.description}
            maxLength={1000}
            onChange={(event) => set("description", event.target.value)}
            error={error("description")}
            className="min-h-24"
          />
          <MediaField label="Capa" hint="Aparece na lista de álbuns do site." value={form.coverId} onChange={(id) => set("coverId", id)} error={error("coverId")} />
          <FieldRow>
            <Input
              label="Ordem de exibição"
              type="number"
              min={0}
              value={form.displayOrder}
              onChange={(event) => set("displayOrder", event.target.value)}
              error={error("displayOrder")}
            />
          </FieldRow>
          <Checkbox label="Mostrar no site" checked={form.isActive} onChange={(event) => set("isActive", event.target.checked)} />
        </Section>
      </AdminForm>
    </>
  );
}

export function AlbumEditor({ id }: { id?: string }) {
  return (
    <EntityLoader<AdminAlbum> path={id ? `/admin/albums/${id}` : null}>
      {(album) => <AlbumForm album={album} />}
    </EntityLoader>
  );
}
