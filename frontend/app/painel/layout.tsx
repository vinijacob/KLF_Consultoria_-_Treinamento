import type { Metadata } from "next";

export const metadata: Metadata = {
  title: { default: "Painel", template: "%s · Painel KLF" },
  robots: { index: false, follow: false },
  referrer: "same-origin",
};

export default function PanelLayout({ children }: LayoutProps<"/painel">) {
  return children;
}
