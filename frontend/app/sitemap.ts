import type { MetadataRoute } from "next";
import { siteConfig } from "@/config/site";
import { getAlbums, getPosts, getServices } from "@/lib/api/public";

export const revalidate = 3600;

export default async function sitemap(): Promise<MetadataRoute.Sitemap> {
  const base = siteConfig.url.replace(/\/$/, "");
  const [services, posts, albums] = await Promise.all([getServices(), getPosts({ pageSize: 100 }), getAlbums()]);

  return [
    ...["", "/servicos", "/sobre", "/clientes", "/conteudo", "/galeria", "/contato"].map((path) => ({ url: `${base}${path}` })),
    ...services.map((service) => ({ url: `${base}/servicos/${service.slug}` })),
    ...posts.items.map((post) => ({ url: `${base}/conteudo/${post.slug}`, lastModified: post.publishedAt ?? undefined })),
    ...albums.map((album) => ({ url: `${base}/galeria/${album.slug}` })),
  ];
}
