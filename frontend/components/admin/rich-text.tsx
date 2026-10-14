"use client";

import { EditorContent, useEditor, useEditorState, type Editor } from "@tiptap/react";
import { useId, useState, type ReactNode } from "react";
import { cn } from "@/lib/cn";
import { inlineMediaPath } from "@/lib/media";
import { FieldError } from "@/components/ui/field";
import {
  AlignCenterIcon,
  AlignJustifyIcon,
  AlignLeftIcon,
  AlignRightIcon,
  ChevronDownIcon,
  ImageIcon,
  IndentIcon,
  OutdentIcon,
} from "@/components/ui/icons";
import { MediaPicker } from "./media";
import { richTextExtensions } from "./rich-text-extensions";
import { Dialog } from "./ui";

export type RichTextValue = { html: string; json: string };

function parseJson(json?: string | null) {
  if (!json) return null;
  try {
    return JSON.parse(json) as object;
  } catch {
    return null;
  }
}

function ToolButton({
  label,
  active,
  disabled,
  onClick,
  children,
}: {
  label: string;
  active?: boolean;
  disabled?: boolean;
  onClick: () => void;
  children: ReactNode;
}) {
  return (
    <button
      type="button"
      title={label}
      aria-label={label}
      aria-pressed={active}
      disabled={disabled}
      onMouseDown={(event) => event.preventDefault()}
      onClick={onClick}
      className={cn(
        "grid min-h-10 min-w-10 place-items-center border border-transparent px-2 font-sans text-sm text-ink transition-colors",
        "enabled:hover:border-ink disabled:opacity-40 aria-pressed:border-brand aria-pressed:bg-brand aria-pressed:text-brand-contrast",
      )}
    >
      {children}
    </button>
  );
}

function ToolSelect({ label, value, onChange, children }: { label: string; value: string; onChange: (value: string) => void; children: ReactNode }) {
  return (
    <label className="relative inline-flex">
      <span className="sr-only">{label}</span>
      <select
        title={label}
        value={value}
        onChange={(event) => onChange(event.target.value)}
        className="min-h-10 appearance-none border border-rule-strong bg-card py-1 pr-8 pl-2.5 font-sans text-sm text-ink hover:border-ink"
      >
        {children}
      </select>
      <ChevronDownIcon aria-hidden className="pointer-events-none absolute top-1/2 right-2 -translate-y-1/2 text-ink-soft" />
    </label>
  );
}

const Divider = () => <span aria-hidden className="mx-1 h-6 w-px bg-rule-strong" />;

const blockOptions = [
  { value: "p", label: "Parágrafo" },
  { value: "h2", label: "Título" },
  { value: "h3", label: "Subtítulo" },
  { value: "h4", label: "Tópico" },
] as const;

const listOptions = [
  { value: "none", label: "Sem lista" },
  { value: "ul:square", label: "■ Marcadores" },
  { value: "ul:disc", label: "● Marcadores redondos" },
  { value: "ul:circle", label: "○ Marcadores vazados" },
  { value: "ul:dash", label: "— Travessões" },
  { value: "ol:1", label: "1. Números" },
  { value: "ol:a", label: "a. Letras" },
  { value: "ol:A", label: "A. Letras maiúsculas" },
  { value: "ol:i", label: "i. Romanos" },
  { value: "ol:I", label: "I. Romanos maiúsculos" },
] as const;

const alignments = [
  { value: "left", label: "Alinhar à esquerda", Icon: AlignLeftIcon },
  { value: "center", label: "Centralizar", Icon: AlignCenterIcon },
  { value: "right", label: "Alinhar à direita", Icon: AlignRightIcon },
  { value: "justify", label: "Justificar", Icon: AlignJustifyIcon },
] as const;

function currentList(editor: Editor) {
  if (editor.isActive("orderedList")) return `ol:${(editor.getAttributes("orderedList").type as string | null) ?? "1"}`;
  if (editor.isActive("bulletList")) return `ul:${(editor.getAttributes("bulletList").listStyle as string | null) ?? "square"}`;
  return "none";
}

function currentBlock(editor: Editor) {
  for (const level of [2, 3, 4] as const) {
    if (editor.isActive("heading", { level })) return `h${level}`;
  }
  return "p";
}

function applyList(editor: Editor, value: string) {
  const chain = editor.chain().focus();
  if (value === "none") {
    if (editor.isActive("orderedList")) chain.toggleOrderedList();
    else if (editor.isActive("bulletList")) chain.toggleBulletList();
    chain.run();
    return;
  }

  const [kind, style] = value.split(":");
  if (kind === "ul") {
    if (!editor.isActive("bulletList") || editor.isActive("orderedList")) chain.toggleBulletList();
    chain.updateAttributes("bulletList", { listStyle: style === "square" ? null : style });
  } else {
    if (!editor.isActive("orderedList")) chain.toggleOrderedList();
    chain.updateAttributes("orderedList", { type: style === "1" ? null : style });
  }
  chain.run();
}

function applyBlock(editor: Editor, value: string) {
  const chain = editor.chain().focus();
  if (value === "p") chain.setParagraph().run();
  else chain.setHeading({ level: Number(value.slice(1)) as 2 | 3 | 4 }).run();
}

function Toolbar({ editor, onLink, onImage }: { editor: Editor; onLink: () => void; onImage: () => void }) {
  const state = useEditorState({
    editor,
    selector: ({ editor: current }) => ({
      block: currentBlock(current),
      list: currentList(current),
      inList: current.isActive("listItem"),
      canSink: current.can().sinkListItem("listItem"),
      canLift: current.can().liftListItem("listItem"),
      tone: (current.getAttributes("siteColor").tone as string | null) ?? "",
      bold: current.isActive("bold"),
      italic: current.isActive("italic"),
      underline: current.isActive("underline"),
      strike: current.isActive("strike"),
      superscript: current.isActive("superscript"),
      subscript: current.isActive("subscript"),
      highlight: current.isActive("highlight"),
      quote: current.isActive("blockquote"),
      link: current.isActive("link"),
      align: alignments.find((item) => item.value !== "left" && current.isActive({ textAlign: item.value }))?.value ?? "left",
      canUndo: current.can().undo(),
      canRedo: current.can().redo(),
    }),
  });

  const chain = () => editor.chain().focus();

  return (
    <div role="toolbar" aria-label="Formatação" className="grid gap-1 border-b border-rule-strong bg-paper-deep px-2 py-1.5">
      <div className="flex flex-wrap items-center gap-1">
        <ToolSelect label="Estilo do parágrafo" value={state.block} onChange={(value) => applyBlock(editor, value)}>
          {blockOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </ToolSelect>
        <Divider />
        <ToolButton label="Negrito (Ctrl+B)" active={state.bold} onClick={() => chain().toggleBold().run()}>
          <span className="font-serif font-bold">N</span>
        </ToolButton>
        <ToolButton label="Itálico (Ctrl+I)" active={state.italic} onClick={() => chain().toggleItalic().run()}>
          <span className="font-serif italic">I</span>
        </ToolButton>
        <ToolButton label="Sublinhado (Ctrl+U)" active={state.underline} onClick={() => chain().toggleUnderline().run()}>
          <span className="font-serif underline underline-offset-2">S</span>
        </ToolButton>
        <ToolButton label="Tachado" active={state.strike} onClick={() => chain().toggleStrike().run()}>
          <span className="font-serif line-through">T</span>
        </ToolButton>
        <ToolButton label="Sobrescrito (m²)" active={state.superscript} onClick={() => chain().toggleSuperscript().run()}>
          <span className="font-serif">
            x<sup>2</sup>
          </span>
        </ToolButton>
        <ToolButton label="Subscrito (H₂O)" active={state.subscript} onClick={() => chain().toggleSubscript().run()}>
          <span className="font-serif">
            x<sub>2</sub>
          </span>
        </ToolButton>
        <Divider />
        <ToolSelect
          label="Cor do texto"
          value={state.tone}
          onChange={(tone) => (tone ? chain().setMark("siteColor", { tone }).run() : chain().unsetMark("siteColor").run())}
        >
          <option value="">Cor padrão</option>
          <option value="brand">Azul-marinho</option>
          <option value="accent">Tijolo</option>
        </ToolSelect>
        <ToolButton label="Marca-texto" active={state.highlight} onClick={() => chain().toggleHighlight().run()}>
          <span className="bg-highlight px-1 font-serif">M</span>
        </ToolButton>
        <ToolButton label="Link" active={state.link} onClick={onLink}>
          Link
        </ToolButton>
        <ToolButton label="Limpar formatação" onClick={() => chain().unsetAllMarks().clearNodes().unsetTextAlign().run()}>
          Limpar
        </ToolButton>
      </div>
      <div className="flex flex-wrap items-center gap-1">
        <ToolSelect label="Lista" value={state.list} onChange={(value) => applyList(editor, value)}>
          {listOptions.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </ToolSelect>
        <ToolButton label="Diminuir recuo da lista (Shift+Tab)" disabled={!state.inList || !state.canLift} onClick={() => chain().liftListItem("listItem").run()}>
          <OutdentIcon className="text-lg" />
        </ToolButton>
        <ToolButton label="Aumentar recuo da lista (Tab)" disabled={!state.inList || !state.canSink} onClick={() => chain().sinkListItem("listItem").run()}>
          <IndentIcon className="text-lg" />
        </ToolButton>
        <Divider />
        {alignments.map(({ value, label, Icon }) => (
          <ToolButton
            key={value}
            label={label}
            active={state.align === value}
            onClick={() => (value === "left" ? chain().unsetTextAlign().run() : chain().setTextAlign(value).run())}
          >
            <Icon className="text-lg" />
          </ToolButton>
        ))}
        <Divider />
        <ToolButton label="Citação" active={state.quote} onClick={() => chain().toggleBlockquote().run()}>
          <span className="font-serif text-lg leading-none">“ ”</span>
        </ToolButton>
        <ToolButton label="Linha separadora" onClick={() => chain().setHorizontalRule().run()}>
          ―
        </ToolButton>
        <ToolButton label="Inserir imagem da biblioteca" onClick={onImage}>
          <ImageIcon className="text-lg" />
        </ToolButton>
        <Divider />
        <ToolButton label="Desfazer (Ctrl+Z)" disabled={!state.canUndo} onClick={() => chain().undo().run()}>
          Desfazer
        </ToolButton>
        <ToolButton label="Refazer (Ctrl+Shift+Z)" disabled={!state.canRedo} onClick={() => chain().redo().run()}>
          Refazer
        </ToolButton>
      </div>
    </div>
  );
}

function LinkBar({ editor, onClose }: { editor: Editor; onClose: () => void }) {
  const [href, setHref] = useState(() => (editor.getAttributes("link").href as string | undefined) ?? "https://");
  const [problem, setProblem] = useState<string | null>(null);
  const inputId = useId();

  function apply() {
    const value = href.trim();
    if (!/^(https:\/\/|mailto:)/.test(value)) {
      setProblem("Use um endereço que comece com https:// (ou mailto: para e-mail).");
      return;
    }
    editor.chain().focus().extendMarkRange("link").setLink({ href: value }).run();
    onClose();
  }

  return (
    <div className="flex flex-wrap items-center gap-2 border-b border-rule-strong bg-card px-3 py-2">
      <label className="sr-only" htmlFor={inputId}>
        Endereço do link
      </label>
      <input
        id={inputId}
        autoFocus
        value={href}
        onChange={(event) => setHref(event.target.value)}
        onKeyDown={(event) => {
          if (event.key === "Enter") {
            event.preventDefault();
            apply();
          }
          if (event.key === "Escape") onClose();
        }}
        className="min-h-10 min-w-0 flex-1 rounded-md border border-rule-strong bg-paper px-3 font-sans text-sm"
      />
      <button type="button" onClick={apply} className="min-h-10 border border-ink px-3 font-sans text-sm font-medium">
        Aplicar
      </button>
      <button
        type="button"
        onClick={() => {
          editor.chain().focus().extendMarkRange("link").unsetLink().run();
          onClose();
        }}
        className="min-h-10 px-3 font-sans text-sm text-brand underline underline-offset-4"
      >
        Tirar link
      </button>
      {problem && <p className="w-full font-sans text-sm text-danger">{problem}</p>}
    </div>
  );
}

/**
 * Editor de texto das publicações e páginas de serviço, no jeito do Word: estilos de parágrafo, negrito a subscrito,
 * cores e marca-texto do site, listas (marcadores, números, letras, romanos, com níveis), alinhamento, citação,
 * link e imagens da biblioteca. A API limpa o HTML de novo antes de salvar e só aceita esses formatos.
 */
export function RichTextEditor({
  label,
  hint,
  initialHtml,
  initialJson,
  onChange,
  error,
}: {
  label: string;
  hint?: string;
  initialHtml?: string | null;
  initialJson?: string | null;
  onChange: (value: RichTextValue) => void;
  error?: string;
}) {
  const labelId = useId();
  const [linkOpen, setLinkOpen] = useState(false);
  const [imageOpen, setImageOpen] = useState(false);

  const editor = useEditor({
    immediatelyRender: false,
    extensions: richTextExtensions,
    content: parseJson(initialJson) ?? initialHtml ?? "",
    editorProps: {
      attributes: {
        class: "prose-klf min-h-72 max-w-none px-5 py-4 focus:outline-none",
        "aria-labelledby": labelId,
        "aria-multiline": "true",
        role: "textbox",
      },
    },
    onUpdate: ({ editor: current }) =>
      onChange({ html: current.isEmpty ? "" : current.getHTML(), json: JSON.stringify(current.getJSON()) }),
  });

  return (
    <div>
      <p id={labelId} className="mb-1.5 font-sans text-sm font-medium text-ink">
        {label}
      </p>
      {hint && <p className="mb-2 font-sans text-sm text-ink-soft">{hint}</p>}
      <div
        className={cn(
          "rounded-md border bg-card has-focus-visible:outline-2 has-focus-visible:outline-offset-2 has-focus-visible:outline-ring",
          error ? "border-2 border-danger" : "border-rule-strong",
        )}
      >
        {editor ? (
          <>
            <Toolbar editor={editor} onLink={() => setLinkOpen((open) => !open)} onImage={() => setImageOpen(true)} />
            {linkOpen && <LinkBar editor={editor} onClose={() => setLinkOpen(false)} />}
            <EditorContent editor={editor} />
          </>
        ) : (
          <div className="min-h-72 px-5 py-4 font-sans text-sm text-ink-soft">Carregando o editor…</div>
        )}
      </div>
      <FieldError>{error}</FieldError>
      {editor && (
        <Dialog open={imageOpen} onClose={() => setImageOpen(false)} title="Inserir imagem no texto" wide>
          <p className="mb-6 font-sans text-[0.9375rem] text-ink-soft">
            A imagem entra onde está o cursor, ocupando a largura do texto. O texto alternativo vem da biblioteca: confira se
            ele descreve a imagem para quem usa leitor de tela.
          </p>
          <MediaPicker
            onPick={(asset) => {
              editor.chain().focus().setImage({ src: inlineMediaPath(asset.id), alt: asset.altText ?? "" }).run();
              setImageOpen(false);
            }}
          />
        </Dialog>
      )}
    </div>
  );
}
