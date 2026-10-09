"use client";

import { usePathname, useRouter } from "next/navigation";
import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from "react";
import { adminFetch, onSessionExpired, signOut } from "@/lib/auth/session";
import { primaryRole } from "@/lib/admin/text";
import type { CurrentUser, Role } from "@/types/admin";
import { Kicker } from "@/components/ui/typography";

type AuthContextValue = {
  user: CurrentUser;
  role: Role;
  isAdmin: boolean;
  refreshUser: () => Promise<void>;
  logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextValue | null>(null);

export function useAuth() {
  const value = useContext(AuthContext);
  if (!value) throw new Error("useAuth precisa estar dentro de <AuthProvider>.");
  return value;
}

function loginUrl(pathname: string) {
  return pathname === "/painel" ? "/painel/entrar" : `/painel/entrar?voltar=${encodeURIComponent(pathname)}`;
}

/** Abre a sessão (refresh pelo cookie), carrega a pessoa logada e manda para o login quando não há sessão. */
export function AuthProvider({ children }: { children: ReactNode }) {
  const router = useRouter();
  const pathname = usePathname();
  const pathRef = useRef(pathname);
  const [user, setUser] = useState<CurrentUser | null>(null);

  useEffect(() => {
    pathRef.current = pathname;
  }, [pathname]);

  useEffect(() => {
    let active = true;

    adminFetch<CurrentUser>("/auth/me").then(
      (current) => {
        if (!active) return;
        if (primaryRole(current.roles)) {
          setUser(current);
        } else {
          void signOut().then(() => router.replace("/painel/entrar"));
        }
      },
      () => active && router.replace(loginUrl(pathRef.current)),
    );

    const stop = onSessionExpired(() => router.replace(loginUrl(pathRef.current)));

    return () => {
      active = false;
      stop();
    };
  }, [router]);

  const refreshUser = useCallback(async () => {
    setUser(await adminFetch<CurrentUser>("/auth/me"));
  }, []);

  const logout = useCallback(async () => {
    await signOut();
    router.replace("/painel/entrar?saiu=1");
  }, [router]);

  if (!user) {
    return (
      <div className="grid flex-1 place-items-center px-6" aria-busy="true">
        <Kicker role="status">Abrindo o painel…</Kicker>
      </div>
    );
  }

  const role = primaryRole(user.roles)!;

  return (
    <AuthContext.Provider value={{ user, role, isAdmin: role === "Admin", refreshUser, logout }}>
      {children}
    </AuthContext.Provider>
  );
}
