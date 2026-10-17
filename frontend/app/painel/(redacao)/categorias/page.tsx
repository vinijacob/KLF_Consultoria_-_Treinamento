import type { Metadata } from "next";
import { TaxonomyPage } from "@/components/admin/content/categories";

export const metadata: Metadata = { title: "Categorias e tags" };

export default function Page() {
  return <TaxonomyPage />;
}
