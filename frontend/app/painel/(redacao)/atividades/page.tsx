import type { Metadata } from "next";
import { AuditLogPage } from "@/components/admin/audit-log";

export const metadata: Metadata = { title: "Atividades" };

export default function Page() {
  return <AuditLogPage />;
}
