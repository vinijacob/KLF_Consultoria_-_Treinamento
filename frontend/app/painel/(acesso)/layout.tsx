import type { ReactNode } from "react";
import { AccessFrame } from "@/components/admin/auth-forms";

export default function AccessLayout({ children }: { children: ReactNode }) {
  return <AccessFrame>{children}</AccessFrame>;
}
