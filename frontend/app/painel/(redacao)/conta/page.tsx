import type { Metadata } from "next";
import { AccountPage } from "@/components/admin/account";

export const metadata: Metadata = { title: "Segurança" };

export default function Page() {
  return <AccountPage />;
}
