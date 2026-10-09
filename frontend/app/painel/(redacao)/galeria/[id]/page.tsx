import type { Metadata } from "next";
import { AlbumEditor } from "@/components/admin/content/albums";

export const metadata: Metadata = { title: "Galeria" };

export default async function Page({ params }: PageProps<"/painel/galeria/[id]">) {
  const { id } = await params;

  return <AlbumEditor key={id} id={id} />;
}
