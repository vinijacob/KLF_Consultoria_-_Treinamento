import type { Metadata } from "next";
import { PostEditor } from "@/components/admin/content/posts";

export const metadata: Metadata = { title: "Publicações" };

export default async function Page({ params }: PageProps<"/painel/publicacoes/[id]">) {
  const { id } = await params;

  return <PostEditor key={id} id={id} />;
}
