import type { Metadata } from "next";
import { AlbumEditor } from "@/components/admin/content/albums";

export const metadata: Metadata = { title: "Novo álbum" };

export default function Page() {
  return <AlbumEditor />;
}
