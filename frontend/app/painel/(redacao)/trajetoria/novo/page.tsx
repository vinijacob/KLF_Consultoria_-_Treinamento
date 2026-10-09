import type { Metadata } from "next";
import { CareerEditor } from "@/components/admin/content/career";

export const metadata: Metadata = { title: "Novo item da trajetória" };

export default function Page() {
  return <CareerEditor />;
}
