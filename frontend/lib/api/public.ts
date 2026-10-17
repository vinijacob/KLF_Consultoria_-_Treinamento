import { env } from "@/lib/env";
import type {
  Album,
  AlbumListItem,
  CareerEntry,
  Client,
  FeaturedQuote,
  Paged,
  PostDetail,
  PostListItem,
  PostType,
  ServiceDetail,
  ServiceListItem,
  SiteSettings,
  TaxonomyItem,
  Testimonial,
} from "@/types/site";

/** Conteúdo público muda pouco: cache de 5 minutos por resposta (ISR). */
const REVALIDATE_SECONDS = 300;

async function get<T>(path: string, tag: string): Promise<T | null> {
  const response = await fetch(`${env.apiUrl}/api/v1/public${path}`, {
    next: { revalidate: REVALIDATE_SECONDS, tags: [tag] },
  });

  if (response.status === 404) return null;
  if (!response.ok) throw new Error(`A API respondeu ${response.status} em ${path}.`);

  return (await response.json()) as T;
}

/**
 * Para blocos de uma página que reúne várias fontes: se a API falhar, o bloco some em vez de derrubar a página inteira.
 * Páginas de detalhe usam `get` direto, para cair na página de erro quando a API está fora.
 */
async function orFallback<T>(promise: Promise<T | null>, fallback: T): Promise<T> {
  try {
    return (await promise) ?? fallback;
  } catch (error) {
    console.error("[site] conteúdo indisponível:", error instanceof Error ? error.message : error);
    return fallback;
  }
}

export const getSettings = () => orFallback(get<SiteSettings>("/settings", "settings"), {});

export const getServices = () => orFallback(get<ServiceListItem[]>("/services", "services"), []);

export const getService = (slug: string) => get<ServiceDetail>(`/services/${encodeURIComponent(slug)}`, "services");

export const getCareer = () => orFallback(get<CareerEntry[]>("/career", "career"), []);

/** A citação da página inicial; `null` quando não há uma publicada. */
export const getFeaturedQuote = () => orFallback(get<FeaturedQuote>("/featured-quote", "featured-quote"), null);

export const getTestimonials = () => orFallback(get<Testimonial[]>("/testimonials", "testimonials"), []);

/** Quantos depoimentos o destaque mostra. Os outros ficam no mural de /clientes. */
const SPOTLIGHT_TESTIMONIALS = 5;

/** Os depoimentos cadastrados por último, para o destaque da home e de /clientes. */
export const getLatestTestimonials = () =>
  orFallback(get<Testimonial[]>(`/testimonials?latest=${SPOTLIGHT_TESTIMONIALS}`, "testimonials"), []);

export const getClients = () => orFallback(get<Client[]>("/clients", "clients"), []);

export const getAlbums = () => orFallback(get<AlbumListItem[]>("/albums", "albums"), []);

export const getAlbum = (slug: string) => get<Album>(`/albums/${encodeURIComponent(slug)}`, "albums");

const emptyPage: Paged<PostListItem> = {
  items: [],
  page: 1,
  pageSize: 10,
  totalItems: 0,
  totalPages: 0,
  hasNextPage: false,
  hasPreviousPage: false,
};

export function getPosts({
  type,
  search,
  category,
  tag,
  page = 1,
  pageSize = 10,
}: { type?: PostType; search?: string; category?: string; tag?: string; page?: number; pageSize?: number } = {}) {
  const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (type) query.set("type", type);
  if (search) query.set("search", search);
  if (category) query.set("category", category);
  if (tag) query.set("tag", tag);

  return orFallback(get<Paged<PostListItem>>(`/posts?${query}`, "posts"), { ...emptyPage, page, pageSize });
}

export const getPost = (slug: string) => get<PostDetail>(`/posts/${encodeURIComponent(slug)}`, "posts");

/** Categorias com publicações no ar (o filtro de /conteudo só mostra as que levam a algum lugar). */
export const getPostCategories = () => orFallback(get<TaxonomyItem[]>("/post-categories", "posts"), []);

export const getPostTags = () => orFallback(get<TaxonomyItem[]>("/post-tags", "posts"), []);

export { mediaUrl } from "@/lib/media";
