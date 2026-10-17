import type { Metadata } from "next";
import { DataRequestsList } from "@/components/admin/contact/data-requests";
import { AdminOnly } from "@/components/admin/feedback/forms";

export const metadata: Metadata = { title: "Pedidos LGPD" };

export default function Page() {
  return (
    <AdminOnly reason="Os pedidos sobre dados pessoais (LGPD) têm nome e e-mail de quem pediu e permitem apagar dados de vez: ficam só com a administração.">
      <DataRequestsList />
    </AdminOnly>
  );
}
