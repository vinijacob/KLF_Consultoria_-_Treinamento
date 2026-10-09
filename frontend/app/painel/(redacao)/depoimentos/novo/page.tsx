import type { Metadata } from "next";
import { TestimonialEditor } from "@/components/admin/content/testimonials";

export const metadata: Metadata = { title: "Novo depoimento" };

export default function Page() {
  return <TestimonialEditor />;
}
