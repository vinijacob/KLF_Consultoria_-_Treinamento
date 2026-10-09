import type { Metadata } from "next";
import { TestimonialsList } from "@/components/admin/content/testimonials";

export const metadata: Metadata = { title: "Depoimentos" };

export default function Page() {
  return <TestimonialsList />;
}
