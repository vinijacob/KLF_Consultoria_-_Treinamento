import type { Metadata } from "next";
import { ClientsList } from "@/components/admin/content/clients";

export const metadata: Metadata = { title: "Clientes" };

export default function Page() {
  return <ClientsList />;
}
