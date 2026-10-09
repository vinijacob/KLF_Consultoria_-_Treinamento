import type { Metadata } from "next";
import { CareerList } from "@/components/admin/content/career";

export const metadata: Metadata = { title: "Trajetória" };

export default function Page() {
  return <CareerList />;
}
