import type { Metadata } from "next";
import { AdminOnly } from "@/components/admin/feedback/forms";
import { SessionCreate } from "@/components/admin/feedback/sessions";

export const metadata: Metadata = { title: "Nova turma" };

export default function Page() {
  return (
    <AdminOnly>
      <SessionCreate />
    </AdminOnly>
  );
}
