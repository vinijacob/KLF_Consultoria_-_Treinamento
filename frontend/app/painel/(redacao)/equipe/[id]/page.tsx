import type { Metadata } from "next";
import { TeamMemberPage } from "@/components/admin/team";

export const metadata: Metadata = { title: "Equipe" };

export default async function Page({ params }: PageProps<"/painel/equipe/[id]">) {
  const { id } = await params;

  return <TeamMemberPage key={id} id={id} />;
}
