import type { Metadata } from "next";
import { MediaLibrary } from "@/components/admin/content/media-library";

export const metadata: Metadata = { title: "Imagens" };

export default async function Page({ searchParams }: PageProps<"/painel/imagens">) {
  const params = await searchParams;

  return <MediaLibrary onlyMissingAlt={params["sem-texto"] === "1"} />;
}
