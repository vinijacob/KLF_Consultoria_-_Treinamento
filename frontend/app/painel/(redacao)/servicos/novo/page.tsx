import type { Metadata } from "next";
import { ServiceEditor } from "@/components/admin/content/services";

export const metadata: Metadata = { title: "Novo serviço" };

export default function Page() {
  return <ServiceEditor />;
}
