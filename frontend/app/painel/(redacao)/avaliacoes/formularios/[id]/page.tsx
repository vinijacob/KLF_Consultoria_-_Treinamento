import type { Metadata } from "next";
import { FormTemplateEditor, AdminOnly } from "@/components/admin/feedback/forms";

export const metadata: Metadata = { title: "Formulários de avaliação" };

export default async function Page({ params }: PageProps<"/painel/avaliacoes/formularios/[id]">) {
  const { id } = await params;

  return <AdminOnly><FormTemplateEditor key={id} id={id} /></AdminOnly>;
}
