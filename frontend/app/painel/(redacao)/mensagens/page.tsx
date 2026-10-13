import type { Metadata } from "next";
import { MessagesList } from "@/components/admin/contact/messages";
import { AdminOnly } from "@/components/admin/feedback/forms";
import { isContactStatus } from "@/lib/admin/params";

export const metadata: Metadata = { title: "Mensagens" };

const reason = "As mensagens do formulário de contato têm dados pessoais de clientes em potencial e ficam só com a administração.";

export default async function Page({ searchParams }: PageProps<"/painel/mensagens">) {
  const { status } = await searchParams;

  return (
    <AdminOnly reason={reason}>
      <MessagesList initialStatus={isContactStatus(status) ? status : undefined} />
    </AdminOnly>
  );
}
