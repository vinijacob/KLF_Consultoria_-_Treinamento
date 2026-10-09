import type { Metadata } from "next";
import { TestimonialEditor } from "@/components/admin/content/testimonials";

export const metadata: Metadata = { title: "Depoimentos" };

export default async function Page({ params }: PageProps<"/painel/depoimentos/[id]">) {
  const { id } = await params;

  return <TestimonialEditor key={id} id={id} />;
}
