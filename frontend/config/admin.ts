import type { Role } from "@/types/admin";

export type AdminNavItem = { href: string; label: string; exact?: boolean };
export type AdminNavGroup = { title: string; adminOnly?: boolean; items: AdminNavItem[] };

export const adminNav: AdminNavGroup[] = [
  { title: "Início", items: [{ href: "/painel", label: "Visão geral", exact: true }] },
  {
    title: "Conteúdo do site",
    items: [
      { href: "/painel/servicos", label: "Serviços" },
      { href: "/painel/publicacoes", label: "Publicações" },
      { href: "/painel/trajetoria", label: "Trajetória" },
      { href: "/painel/depoimentos", label: "Depoimentos" },
      { href: "/painel/clientes", label: "Clientes" },
      { href: "/painel/imagens", label: "Imagens" },
      { href: "/painel/galeria", label: "Galeria" },
      { href: "/painel/dados-do-site", label: "Dados do site" },
    ],
  },
  {
    title: "Avaliações",
    adminOnly: true,
    items: [
      { href: "/painel/avaliacoes", label: "Resultados", exact: true },
      { href: "/painel/avaliacoes/turmas", label: "Turmas" },
      { href: "/painel/avaliacoes/formularios", label: "Formulários" },
    ],
  },
  { title: "Conta", items: [{ href: "/painel/conta", label: "Segurança" }] },
];

export function navFor(role: Role) {
  return adminNav.filter((group) => !group.adminOnly || role === "Admin");
}

export function isActive(item: AdminNavItem, pathname: string) {
  return item.exact ? pathname === item.href : pathname === item.href || pathname.startsWith(`${item.href}/`);
}

/** O que cada perfil faz no painel: aparece no banner do topo e no expediente da visão geral. */
export const roleInfo: Record<Role, { label: string; summary: string; can: string[]; cannot: string[] }> = {
  Admin: {
    label: "Administração",
    summary: "Acesso completo: conteúdo do site, avaliações das turmas e segurança.",
    can: [
      "Editar todo o conteúdo do site",
      "Criar turmas, formulários e ver resultados das avaliações",
      "Baixar QR Code e cartaz das turmas",
    ],
    cannot: [],
  },
  Editor: {
    label: "Edição",
    summary: "Conteúdo do site: serviços, publicações, trajetória, depoimentos, clientes e galeria.",
    can: ["Editar serviços, publicações e trajetória", "Cuidar de depoimentos, clientes, imagens e galeria", "Atualizar os dados do site"],
    cannot: ["Avaliações e resultados das turmas ficam com a administração"],
  },
};
