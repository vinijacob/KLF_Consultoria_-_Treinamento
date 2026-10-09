export type ServiceFormat = "InPerson" | "Online" | "InCompany";
export type PostType = "Project" | "Article" | "News";
export type CareerEntryType = "Education" | "Certification" | "Experience";

export type AboutSettings = {
  mission?: string | null;
  vision?: string | null;
  values?: string[] | null;
  differentials?: string[] | null;
  history?: string | null;
};

export type ContactSettings = {
  whatsapp?: string | null;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  mapUrl?: string | null;
};

export type SocialSettings = {
  instagram?: string | null;
  linkedin?: string | null;
  facebook?: string | null;
  youtube?: string | null;
};

export type SeoSettings = { title?: string | null; description?: string | null; shareImageId?: string | null };

export type SiteSettings = {
  about?: AboutSettings;
  contact?: ContactSettings;
  social?: SocialSettings;
  seo?: SeoSettings;
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
};

export type PostDetail = PostListItem & {
  contentHtml: string;
  seoTitle?: string | null;
  seoDescription?: string | null;
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
