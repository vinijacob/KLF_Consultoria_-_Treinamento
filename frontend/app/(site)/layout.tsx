import type { Metadata } from "next";
import { SiteFooter } from "@/components/layout/site-footer";
import { SiteHeader } from "@/components/layout/site-header";
import { siteConfig } from "@/config/site";
import { getSettings, mediaUrl } from "@/lib/api/public";

export async function generateMetadata(): Promise<Metadata> {
  const { seo } = await getSettings();
  const description = seo?.description ?? siteConfig.description;

  return {
    title: { default: seo?.title ?? siteConfig.name, template: `%s | ${siteConfig.name}` },
    description,
    openGraph: {
      siteName: siteConfig.name,
      locale: "pt_BR",
      type: "website",
      description,
      images: seo?.shareImageId ? [mediaUrl(seo.shareImageId)] : undefined,
    },
  };
}

export default async function SiteLayout({ children }: LayoutProps<"/">) {
  const settings = await getSettings();

  return (
    <>
      <a
        href="#conteudo"
        className="sr-only focus:not-sr-only focus:fixed focus:top-3 focus:left-3 focus:z-50 focus:bg-brand focus:px-4 focus:py-2 focus:font-sans focus:text-brand-contrast"
      >
        Pular para o conteúdo
      </a>
      <SiteHeader />
      <main id="conteudo" className="flex-1 pb-24">
        {children}
      </main>
      <SiteFooter settings={settings} />
    </>
  );
}
