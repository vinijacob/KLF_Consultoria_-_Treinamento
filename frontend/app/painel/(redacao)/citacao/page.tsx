import type { Metadata } from "next";
import { FeaturedQuotePage } from "@/components/admin/content/featured-quote";

export const metadata: Metadata = { title: "Citação" };

export default function Page() {
  return <FeaturedQuotePage />;
}
