import type { Metadata } from "next";
import { AdminOnly } from "@/components/admin/feedback/forms";
import { SessionsList } from "@/components/admin/feedback/sessions";

export const metadata: Metadata = { title: "Turmas" };

export default function Page() {
  return (
    <AdminOnly>
      <SessionsList />
    </AdminOnly>
  );
}
