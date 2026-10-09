import type { Metadata } from "next";
import { ClientEditor } from "@/components/admin/content/clients";

export const metadata: Metadata = { title: "Novo cliente" };

export default function Page() {
  return <ClientEditor />;
}
