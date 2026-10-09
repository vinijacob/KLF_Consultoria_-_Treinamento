import type { Metadata } from "next";
import { AdminOnly } from "@/components/admin/feedback/forms";
import { SessionDetail } from "@/components/admin/feedback/sessions";

export const metadata: Metadata = { title: "Turma" };

export default async function Page({ params }: PageProps<"/painel/avaliacoes/turmas/[id]">) {
  const { id } = await params;

  return (
    <AdminOnly>
      <SessionDetail key={id} id={id} />
    </AdminOnly>
  );
}
