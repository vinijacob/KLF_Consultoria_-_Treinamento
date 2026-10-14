import type { Metadata } from "next";
import { TeamInvitePage } from "@/components/admin/team";

export const metadata: Metadata = { title: "Convidar pessoa" };

export default function Page() {
  return <TeamInvitePage />;
}
