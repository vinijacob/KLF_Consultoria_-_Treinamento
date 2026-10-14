import { Mark, mergeAttributes } from "@tiptap/react";
import { BulletList, OrderedList } from "@tiptap/extension-list";
import Highlight from "@tiptap/extension-highlight";
import Image from "@tiptap/extension-image";
import Subscript from "@tiptap/extension-subscript";
import Superscript from "@tiptap/extension-superscript";
import TextAlign from "@tiptap/extension-text-align";
import StarterKit from "@tiptap/starter-kit";

/*
 * O que o editor produz precisa passar no sanitizador da API (HtmlContentSanitizer): classes `klf-*` e `text-align`
 * são as únicas coisas de estilo aceitas. Ao criar um formato novo aqui, libere-o lá também.
 */

export const bulletStyles = ["disc", "circle", "dash"] as const;
export type BulletStyle = (typeof bulletStyles)[number];

export const textTones = ["brand", "accent"] as const;
export type TextTone = (typeof textTones)[number];

/** Lista com marcadores com o símbolo escolhido (sem classe = o quadrado padrão do site). */
const StyledBulletList = BulletList.extend({
  addAttributes() {
    return {
      ...this.parent?.(),
      listStyle: {
        default: null,
        parseHTML: (element: HTMLElement) => bulletStyles.find((style) => element.classList.contains(`klf-list-${style}`)) ?? null,
        renderHTML: (attributes: { listStyle?: string | null }) =>
          attributes.listStyle ? { class: `klf-list-${attributes.listStyle}` } : {},
      },
    };
  },
});

/** Tipos de numeração (o `type` do HTML) e a classe que o site usa para desenhar cada um. */
export const orderedTypes = { a: "lower-alpha", A: "upper-alpha", i: "lower-roman", I: "upper-roman" } as const;

/**
 * Lista numerada que, além do `type` do HTML, grava a classe `klf-list-*`: no HTML o `type` é comparado sem diferenciar
 * maiúsculas, então só pela classe o CSS separa "a" de "A".
 */
const StyledOrderedList = OrderedList.extend({
  renderHTML({ node, HTMLAttributes }) {
    const [tag, attrs, hole] = this.parent!({ node, HTMLAttributes }) as [string, Record<string, unknown>, 0];
    const style = orderedTypes[node.attrs.type as keyof typeof orderedTypes];
    return [tag, style ? mergeAttributes(attrs, { class: `klf-list-${style}` }) : attrs, hole];
  },
});

/** Cor do texto, só com as cores do site (classes que mudam sozinhas no modo escuro). */
export const SiteColor = Mark.create({
  name: "siteColor",

  addAttributes() {
    return {
      tone: {
        default: null,
        parseHTML: (element: HTMLElement) => textTones.find((tone) => element.classList.contains(`klf-color-${tone}`)) ?? null,
        renderHTML: (attributes: { tone?: string | null }) => (attributes.tone ? { class: `klf-color-${attributes.tone}` } : {}),
      },
    };
  },

  parseHTML() {
    return textTones.map((tone) => ({ tag: `span.klf-color-${tone}` }));
  },

  renderHTML({ HTMLAttributes }) {
    return ["span", mergeAttributes(HTMLAttributes), 0];
  },
});

export const richTextExtensions = [
  StarterKit.configure({
    heading: { levels: [2, 3, 4] },
    bulletList: false,
    orderedList: false,
    code: false,
    codeBlock: false,
    link: { openOnClick: false, autolink: true, defaultProtocol: "https" },
  }),
  StyledBulletList,
  StyledOrderedList,
  Subscript,
  Superscript,
  Highlight.configure({ multicolor: false }),
  TextAlign.configure({ types: ["heading", "paragraph"], alignments: ["left", "center", "right", "justify"] }),
  Image.configure({ inline: false, allowBase64: false }),
  SiteColor,
];
