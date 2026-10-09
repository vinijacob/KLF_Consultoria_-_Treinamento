import type { Metadata } from "next";
import { FeedbackExperience } from "@/components/feedback/feedback-experience";

export const metadata: Metadata = {
  title: "Avaliação",
  robots: { index: false, follow: false },
  referrer: "no-referrer",
};

export default async function FeedbackPage({ params }: PageProps<"/avaliar/[codigo]">) {
  const { codigo } = await params;

  return <FeedbackExperience code={codigo} />;
}
