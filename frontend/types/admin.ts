import type { FeedbackTopic, FormDefinition, QuestionType, SessionStatus } from "./feedback";
import type { CareerEntryType, PostType, ServiceFormat } from "./site";

export type { AboutSettings, ContactSettings, Paged, SeoSettings, SiteSettings, SocialSettings } from "./site";

export type Role = "Admin" | "Editor";

export type CurrentUser = {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  twoFactorEnabled: boolean;
};

export type LoginResponse = {
  accessToken?: string | null;
  expiresAt?: string | null;
  twoFactorRequired?: boolean;
  twoFactorSetupRequired?: boolean;
  twoFactorToken?: string | null;
};

export type TwoFactorSetupResponse = { sharedKey: string; otpAuthUri: string };
export type TwoFactorEnabledResponse = { accessToken: string; expiresAt: string; recoveryCodes: string[] };
export type RecoveryCodesResponse = { recoveryCodes: string[] };

export type AdminService = {
  id: string;
  title: string;
  slug: string;
  summary?: string | null;
  contentHtml: string;
  audience: string;
  workloadHours: number;
  format: ServiceFormat;
  coverId?: string | null;
  displayOrder: number;
  isActive: boolean;
};

export type AdminServiceListItem = Omit<AdminService, "contentHtml" | "audience">;

export type PostStatus = "Draft" | "Scheduled" | "Published";

export type AdminPost = {
  id: string;
  type: PostType;
  title: string;
  slug: string;
  summary?: string | null;
  contentJson: string;
  contentHtml: string;
  coverId?: string | null;
  status: PostStatus;
  publishedAt?: string | null;
  seoTitle?: string | null;
  seoDescription?: string | null;
  authorId: string;
};

export type AdminPostListItem = Pick<
  AdminPost,
  "id" | "type" | "title" | "slug" | "summary" | "coverId" | "status" | "publishedAt"
>;

export type AdminCareerEntry = {
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

export type AdminTestimonial = {
  id: string;
  authorName: string;
  authorRole?: string | null;
  companyName?: string | null;
  quote: string;
  photoId?: string | null;
  consentGivenAt?: string | null;
  consentCoversImage: boolean;
  consentRevokedAt?: string | null;
  isPublished: boolean;
  displayOrder: number;
};

export type AdminClient = {
  id: string;
  name: string;
  websiteUrl?: string | null;
  logoId?: string | null;
  displayOrder: number;
  isActive: boolean;
};

export type MediaAsset = {
  id: string;
  url: string;
  originalFileName: string;
  contentType: string;
  sizeBytes: number;
  altText?: string | null;
  createdAt: string;
};

export type AdminAlbumListItem = {
  id: string;
  title: string;
  slug: string;
  description?: string | null;
  coverUrl?: string | null;
  itemCount: number;
  displayOrder: number;
  isActive: boolean;
};

export type AdminAlbumItem = {
  mediaAssetId: string;
  url: string;
  altText?: string | null;
  caption?: string | null;
  displayOrder: number;
};

export type AdminAlbum = {
  id: string;
  title: string;
  slug: string;
  description?: string | null;
  coverId?: string | null;
  coverUrl?: string | null;
  displayOrder: number;
  isActive: boolean;
  items: AdminAlbumItem[];
};

export type FeedbackFormListItem = {
  id: string;
  title: string;
  description?: string | null;
  sectionCount: number;
  questionCount: number;
};

export type FeedbackFormDetail = {
  id: string;
  title: string;
  description?: string | null;
  definition: FormDefinition;
};

export type FeedbackSessionListItem = {
  id: string;
  title: string;
  publicCode: string;
  status: SessionStatus;
  responseCount: number;
  opensAt: string;
  closesAt: string;
  maxResponses?: number | null;
  clientId?: string | null;
  serviceId?: string | null;
};

export type FeedbackSessionDetail = FeedbackSessionListItem & {
  publicUrl: string;
  formId?: string | null;
  formTitle: string;
  formDescription?: string | null;
  definition: FormDefinition;
  closedAt?: string | null;
  ownerId: string;
};

export type NpsResult = { score: number; promoters: number; passives: number; detractors: number; total: number };

export type QuestionResult = {
  id: string;
  text: string;
  type: QuestionType;
  answerCount: number;
  isHidden: boolean;
  average?: number | null;
  distribution?: { value: number; count: number }[] | null;
  nps?: NpsResult | null;
  options?: { id: string; label: string; count: number }[] | null;
  texts?: string[] | null;
};

export type SessionResults = {
  sessionId: string;
  title: string;
  status: SessionStatus;
  responseCount: number;
  minimumResponses: number;
  hasEnoughResponses: boolean;
  nps?: NpsResult | null;
  sections: { id: string; title: string; topic: FeedbackTopic; questions: QuestionResult[] }[];
};

export type FeedbackSummary = {
  sessionCount: number;
  responseCount: number;
  nps?: NpsResult | null;
  sessions: { id: string; title: string; opensAt: string; responseCount: number; nps?: NpsResult | null }[];
};
