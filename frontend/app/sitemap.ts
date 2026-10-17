import type { MetadataRoute } from "next";
import { siteConfig } from "@/config/site";
import { getAlbums, getPosts, getServices, getSettings } from "@/lib/api/public";
import { legalUpdatedOn, missingLegalData } from "@/lib/legal";

export const revalidate = 3600;

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const base = siteConfig.url.replace(/\/$/, "");
  const [services, posts, albums, { legal }] = await Promise.all([getServices(), getPosts({ pageSize: 100 }), getAlbums(), getSettings()]);
  const legalPages = missingLegalData(legal).length === 0 ? ["/privacidade", "/termos", "/cookies"] : [];

  return [
    ...["", "/servicos", "/sobre", "/clientes", "/conteudo", "/galeria", "/contato", "/meus-dados"].map((path) => ({ url: `${base}${path}` })),
    ...services.map((service) => ({ url: `${base}/servicos/${service.slug}` })),
    ...posts.items.map((post) => ({ url: `${base}/conteudo/${post.slug}`, lastModified: post.publishedAt ?? undefined })),
    ...albums.map((album) => ({ url: `${base}/galeria/${album.slug}` })),
    ...legalPages.map((path) => ({ url: `${base}${path}`, lastModified: legalUpdatedOn(legal) })),
  ];
}
