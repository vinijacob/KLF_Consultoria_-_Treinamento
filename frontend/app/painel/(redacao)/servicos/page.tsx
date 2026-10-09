import type { Metadata } from "next";
import { ServicesList } from "@/components/admin/content/services";

export const metadata: Metadata = { title: "Serviços" };

export default function Page() {
  return <ServicesList />;
}
