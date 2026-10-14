import type { Metadata } from "next";
import { TeamList } from "@/components/admin/team";

export const metadata: Metadata = { title: "Equipe" };

export default function Page() {
  return <TeamList />;
}
