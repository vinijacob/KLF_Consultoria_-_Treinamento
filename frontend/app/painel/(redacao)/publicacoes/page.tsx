import type { Metadata } from "next";
import { PostsList } from "@/components/admin/content/posts";
import { isPostStatus } from "@/lib/admin/params";

export const metadata: Metadata = { title: "Publicações" };

export default async function Page({ searchParams }: PageProps<"/painel/publicacoes">) {
  const { status } = await searchParams;

  return <PostsList initialStatus={isPostStatus(status) ? status : undefined} />;
}
