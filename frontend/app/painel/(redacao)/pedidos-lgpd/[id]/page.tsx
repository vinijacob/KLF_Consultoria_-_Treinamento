import type { Metadata } from "next";
import { DataRequestDetail } from "@/components/admin/contact/data-requests";
import { AdminOnly } from "@/components/admin/feedback/forms";

export const metadata: Metadata = { title: "Pedidos LGPD" };

export default async function Page({ params }: PageProps<"/painel/pedidos-lgpd/[id]">) {
  const { id } = await params;

  return (
    <AdminOnly reason="Os pedidos sobre dados pessoais (LGPD) têm nome e e-mail de quem pediu e permitem apagar dados de vez: ficam só com a administração.">
      <DataRequestDetail key={id} id={id} />
    </AdminOnly>
  );
}
