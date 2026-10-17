export type ServiceFormat = "InPerson" | "Online" | "InCompany";
export type PostType = "Project" | "Article" | "News";
export type CareerEntryType = "Education" | "Certification" | "Experience";

export type AboutSettings = {
  mission?: string | null;
  vision?: string | null;
  values?: string[] | null;
  differentials?: string[] | null;
  history?: string | null;
  /** Fotos da Kilciene em ordem (carrossel da página inicial e da Sobre); a primeira também é o avatar. */
  portraitIds?: string[] | null;
};

/** A citação da Kilciene na página inicial. */
export type FeaturedQuote = { text: string; author: string; source?: string | null };

export type ContactSettings = {
  whatsapp?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  mapUrl?: string | null;
};

/** Redes sociais da KLF, na ordem do site: qualquer rede, pelo nome. */
export type SocialSettings = { links?: SocialLink[] | null };

export type SocialLink = { name: string; url: string };

export type SeoSettings = { title?: string | null; description?: string | null; shareImageId?: string | null };

export type LegalSettings = {
  companyName?: string | null;
  cnpj?: string | null;
  address?: string | null;
  forumCity?: string | null;
  dpoName?: string | null;
  dpoEmail?: string | null;
  updatedOn?: string | null;
};

export type SiteSettings = {
  about?: AboutSettings;
  contact?: ContactSettings;
  social?: SocialSettings;
  seo?: SeoSettings;
  legal?: LegalSettings;
};

export type ServiceListItem = {
  id: string;
  title: string;
  slug: string;
  summary?: string | null;
  workloadHours: number;
  format: ServiceFormat;
  coverId?: string | null;
  displayOrder: number;
};

export type ServiceDetail = {
  id: string;
  title: string;
  slug: string;
  summary?: string | null;
  contentHtml: string;
  audience: string;
  workloadHours: number;
  format: ServiceFormat;
  coverId?: string | null;
};

export type CareerEntry = {
  id: string;
  entryType: CareerEntryType;
  title: string;
  institution?: string | null;
  description?: string | null;
  startDate: string;
  endDate?: string | null;
  isOngoing: boolean;
  displayOrder: number;
};

export type PostListItem = {
  id: string;
  type: PostType;
  title: string;
  slug: string;
  summary?: string | null;
  coverId?: string | null;
  publishedAt?: string | null;
  category?: TaxonomyRef | null;
};

/** Categoria (tema) ou tag de uma publicação. */
export type TaxonomyRef = { name: string; slug: string };

/** Categoria ou tag com publicações visíveis, para os filtros do site. */
export type TaxonomyItem = TaxonomyRef & { postCount: number };

export type PostDetail = PostListItem & {
  contentHtml: string;
  seoTitle?: string | null;
  seoDescription?: string | null;
  tags: TaxonomyRef[];
};

export type Paged<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
};

export type Testimonial = {
  id: string;
  authorName: string;
  authorRole?: string | null;
  companyName?: string | null;
  quote: string;
  photoId?: string | null;
};

export type Client = { id: string; name: string; websiteUrl?: string | null; logoId?: string | null };

export type AlbumListItem = {
  id: string;
  title: string;
  slug: string;
  description?: string | null;
  coverUrl?: string | null;
  itemCount: number;
};

export type Album = {
  id: string;
  title: string;
  slug: string;
  description?: string | null;
  coverUrl?: string | null;
  items: { url: string; altText?: string | null; caption?: string | null }[];
};
