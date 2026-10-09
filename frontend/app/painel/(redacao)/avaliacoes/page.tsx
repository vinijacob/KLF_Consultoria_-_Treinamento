import type { Metadata } from "next";
import { AdminOnly } from "@/components/admin/feedback/forms";
import { FeedbackSummaryPage } from "@/components/admin/feedback/sessions";

export const metadata: Metadata = { title: "Resultados das avaliações" };

export default function Page() {
  return (
    <AdminOnly>
      <FeedbackSummaryPage />
    </AdminOnly>
  );
}
