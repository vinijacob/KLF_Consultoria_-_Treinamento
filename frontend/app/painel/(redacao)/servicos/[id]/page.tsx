import type { Metadata } from "next";
import { ServiceEditor } from "@/components/admin/content/services";

export const metadata: Metadata = { title: "Serviços" };

export default async function Page({ params }: PageProps<"/painel/servicos/[id]">) {
  const { id } = await params;

  return <ServiceEditor key={id} id={id} />;
}
