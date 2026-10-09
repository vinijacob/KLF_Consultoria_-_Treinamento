import type { Metadata } from "next";
import { Overview } from "@/components/admin/overview";

export const metadata: Metadata = { title: "Visão geral" };

export default function OverviewPage() {
  return <Overview />;
}
