"use client";

import { EditorContent, useEditor, useEditorState, type Editor } from "@tiptap/react";
import StarterKit from "@tiptap/starter-kit";
import { useId, useState, type ReactNode } from "react";
import { cn } from "@/lib/cn";
import { FieldError } from "@/components/ui/field";

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
        "min-h-10 min-w-10 border border-transparent px-2.5 font-sans text-sm text-ink transition-colors",
        "enabled:hover:border-ink disabled:opacity-40 aria-pressed:border-brand aria-pressed:bg-brand aria-pressed:text-brand-contrast",
      )}
    >
      {children}
    </button>
  );
}

function Toolbar({ editor, onLink }: { editor: Editor; onLink: () => void }) {
  const state = useEditorState({
    editor,
    selector: ({ editor: current }) => ({
      h2: current.isActive("heading", { level: 2 }),
      h3: current.isActive("heading", { level: 3 }),
      bold: current.isActive("bold"),
      italic: current.isActive("italic"),
      bullet: current.isActive("bulletList"),
      ordered: current.isActive("orderedList"),
      quote: current.isActive("blockquote"),
      link: current.isActive("link"),
      canUndo: current.can().undo(),
      canRedo: current.can().redo(),
    }),
  });

  const chain = () => editor.chain().focus();

  return (
    <div role="toolbar" aria-label="Formatação" className="flex flex-wrap items-center gap-1 border-b border-rule-strong bg-paper-deep px-2 py-1.5">
      <ToolButton label="Título de seção" active={state.h2} onClick={() => chain().toggleHeading({ level: 2 }).run()}>
        <span className="font-serif text-base font-semibold">T</span>
      </ToolButton>
      <ToolButton label="Subtítulo" active={state.h3} onClick={() => chain().toggleHeading({ level: 3 }).run()}>
        <span className="font-serif text-sm font-semibold">t</span>
      </ToolButton>
      <span aria-hidden className="mx-1 h-6 w-px bg-rule-strong" />
      <ToolButton label="Negrito" active={state.bold} onClick={() => chain().toggleBold().run()}>
        <span className="font-serif font-bold">N</span>
      </ToolButton>
      <ToolButton label="Itálico" active={state.italic} onClick={() => chain().toggleItalic().run()}>
        <span className="font-serif italic">I</span>
      </ToolButton>
      <ToolButton label="Link" active={state.link} onClick={onLink}>
        Link
      </ToolButton>
      <span aria-hidden className="mx-1 h-6 w-px bg-rule-strong" />
      <ToolButton label="Lista com marcadores" active={state.bullet} onClick={() => chain().toggleBulletList().run()}>
        Lista
      </ToolButton>
      <ToolButton label="Lista numerada" active={state.ordered} onClick={() => chain().toggleOrderedList().run()}>
        1. 2.
      </ToolButton>
      <ToolButton label="Citação" active={state.quote} onClick={() => chain().toggleBlockquote().run()}>
        <span className="font-serif text-lg leading-none">“ ”</span>
      </ToolButton>
      <ToolButton label="Linha separadora" onClick={() => chain().setHorizontalRule().run()}>
        ―
      </ToolButton>
      <span aria-hidden className="mx-1 h-6 w-px bg-rule-strong" />
      <ToolButton label="Desfazer" disabled={!state.canUndo} onClick={() => chain().undo().run()}>
        Desfazer
      </ToolButton>
      <ToolButton label="Refazer" disabled={!state.canRedo} onClick={() => chain().redo().run()}>
        Refazer
      </ToolButton>
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
        className="min-h-10 min-w-0 flex-1 rounded-xs border border-rule-strong bg-paper px-3 font-sans text-sm"
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
 * Editor de texto das publicações e páginas de serviço. Só os formatos que o site mostra bem (títulos, listas,
 * citação, link). A API limpa o HTML de novo antes de salvar.
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

  const editor = useEditor({
    immediatelyRender: false,
    extensions: [
      StarterKit.configure({
        heading: { levels: [2, 3] },
        code: false,
        codeBlock: false,
        strike: false,
        underline: false,
        link: { openOnClick: false, autolink: true, protocols: ["https", "mailto"], defaultProtocol: "https" },
      }),
    ],
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
          "rounded-xs border bg-card has-focus-visible:outline-2 has-focus-visible:outline-offset-2 has-focus-visible:outline-ring",
          error ? "border-2 border-danger" : "border-rule-strong",
        )}
      >
        {editor ? (
          <>
            <Toolbar editor={editor} onLink={() => setLinkOpen((open) => !open)} />
            {linkOpen && <LinkBar editor={editor} onClose={() => setLinkOpen(false)} />}
            <EditorContent editor={editor} />
          </>
        ) : (
          <div className="min-h-72 px-5 py-4 font-sans text-sm text-ink-soft">Carregando o editor…</div>
        )}
      </div>
      <FieldError>{error}</FieldError>
    </div>
  );
}
