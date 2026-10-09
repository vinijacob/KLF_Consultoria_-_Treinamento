import type { Metadata } from "next";
import { SiteSettingsPage } from "@/components/admin/content/settings";

export const metadata: Metadata = { title: "Dados do site" };

export default function Page() {
  return <SiteSettingsPage />;
}
