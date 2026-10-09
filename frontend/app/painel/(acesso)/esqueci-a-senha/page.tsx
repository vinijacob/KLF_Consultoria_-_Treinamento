import type { Metadata } from "next";
import { ForgotPasswordForm } from "@/components/admin/auth-forms";

export const metadata: Metadata = { title: "Esqueci minha senha" };

export default function ForgotPasswordPage() {
  return <ForgotPasswordForm />;
}
