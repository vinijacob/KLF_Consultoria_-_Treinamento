import type { Metadata } from "next";
import { AlbumsList } from "@/components/admin/content/albums";

export const metadata: Metadata = { title: "Galeria" };

export default function Page() {
  return <AlbumsList />;
}
