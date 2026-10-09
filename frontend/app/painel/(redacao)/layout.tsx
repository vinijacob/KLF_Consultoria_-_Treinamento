import type { ReactNode } from "react";
import { AuthProvider } from "@/components/admin/auth-provider";
import { AdminShell } from "@/components/admin/shell";

export default function NewsroomLayout({ children }: { children: ReactNode }) {
  return (
    <AuthProvider>
      <AdminShell>{children}</AdminShell>
    </AuthProvider>
  );
}
