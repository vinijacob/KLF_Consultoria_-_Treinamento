export const siteConfig = {
  name: "KLF Consultoria & Treinamento",
  shortName: "KLF",
  description:
    "Kilciene Lima Ferreira: treinamentos e palestras em vendas e experiência do cliente. Atendimento que gera valor.",
  url: process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3000",
  nav: [
    { href: "/servicos", label: "Serviços" },
    { href: "/sobre", label: "Sobre" },
    { href: "/clientes", label: "Clientes" },
    { href: "/conteudo", label: "Conteúdo" },
    { href: "/galeria", label: "Galeria" },
    { href: "/contato", label: "Contato" },
  ],
} as const;
