import type { Metadata } from "next";
import { ClientEditor } from "@/components/admin/content/clients";

export const metadata: Metadata = { title: "Clientes" };

export default async function Page({ params }: PageProps<"/painel/clientes/[id]">) {
  const { id } = await params;

  return <ClientEditor key={id} id={id} />;
}
