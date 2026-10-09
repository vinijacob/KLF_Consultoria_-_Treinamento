import type { Metadata } from "next";
import { LoginFlow } from "@/components/admin/auth-forms";
import { safeReturnPath } from "@/lib/admin/params";

export const metadata: Metadata = { title: "Entrar" };

export default async function LoginPage({ searchParams }: PageProps<"/painel/entrar">) {
  const { voltar, saiu } = await searchParams;

  return <LoginFlow returnTo={safeReturnPath(voltar)} signedOut={saiu === "1"} />;
}
