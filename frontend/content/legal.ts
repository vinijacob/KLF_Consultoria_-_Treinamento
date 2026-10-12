/**
 * Fornecedores que tratam dados em nome da KLF (operadores, na LGPD). Precisa bater com o que estiver de fato no ar:
 * ao trocar de hospedagem, banco, e-mail ou armazenamento, atualize esta lista e `LEGAL_TEXT_REVISED_ON` em `lib/legal.ts`.
 */
export const dataProcessors = [
  { name: "Vercel", role: "hospedagem do site e medição de visitas sem cookies", country: "Estados Unidos" },
  { name: "Railway", role: "hospedagem do sistema que guarda os conteúdos e as avaliações", country: "Estados Unidos" },
  { name: "Neon", role: "banco de dados", country: "Estados Unidos" },
  { name: "Cloudflare", role: "armazenamento das imagens do site", country: "Estados Unidos" },
  { name: "Resend", role: "envio dos e-mails do painel (como a redefinição de senha)", country: "Estados Unidos" },
] as const;
