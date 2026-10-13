import type { Metadata } from "next";
import { MessageDetail } from "@/components/admin/contact/messages";
import { AdminOnly } from "@/components/admin/feedback/forms";

export const metadata: Metadata = { title: "Mensagens" };

export default async function Page({ params }: PageProps<"/painel/mensagens/[id]">) {
  const { id } = await params;

  return (
    <AdminOnly reason="As mensagens do formulário de contato têm dados pessoais de clientes em potencial e ficam só com a administração.">
      <MessageDetail key={id} id={id} />
    </AdminOnly>
  );
}
