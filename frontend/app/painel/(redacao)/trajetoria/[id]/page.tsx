import type { Metadata } from "next";
import { CareerEditor } from "@/components/admin/content/career";

export const metadata: Metadata = { title: "Trajetória" };

export default async function Page({ params }: PageProps<"/painel/trajetoria/[id]">) {
  const { id } = await params;

  return <CareerEditor key={id} id={id} />;
}
