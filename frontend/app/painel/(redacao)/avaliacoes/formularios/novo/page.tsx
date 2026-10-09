import type { Metadata } from "next";
import { FormTemplateEditor, AdminOnly } from "@/components/admin/feedback/forms";

export const metadata: Metadata = { title: "Novo formulário" };

export default function Page() {
  return <AdminOnly><FormTemplateEditor /></AdminOnly>;
}
