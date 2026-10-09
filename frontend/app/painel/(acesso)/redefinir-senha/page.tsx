import type { Metadata } from "next";
import { ResetPasswordForm } from "@/components/admin/auth-forms";

export const metadata: Metadata = { title: "Nova senha", referrer: "no-referrer" };

export default async function ResetPasswordPage({ searchParams }: PageProps<"/painel/redefinir-senha">) {
  const { email, token } = await searchParams;

  return <ResetPasswordForm email={typeof email === "string" ? email : ""} token={typeof token === "string" ? token : ""} />;
}
