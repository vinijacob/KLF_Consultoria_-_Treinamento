import type { Metadata } from "next";
import { AcceptInviteForm } from "@/components/admin/auth-forms";

export const metadata: Metadata = { title: "Convite", referrer: "no-referrer", robots: { index: false, follow: false } };

export default async function AcceptInvitePage({ searchParams }: PageProps<"/painel/aceitar-convite">) {
  const { email, token } = await searchParams;

  return <AcceptInviteForm email={typeof email === "string" ? email : ""} token={typeof token === "string" ? token : ""} />;
}
