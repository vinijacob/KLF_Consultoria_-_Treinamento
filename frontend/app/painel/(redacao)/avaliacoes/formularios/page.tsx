import type { Metadata } from "next";
import { FormsList, AdminOnly } from "@/components/admin/feedback/forms";

export const metadata: Metadata = { title: "Formulários de avaliação" };

export default function Page() {
  return <AdminOnly><FormsList /></AdminOnly>;
}
