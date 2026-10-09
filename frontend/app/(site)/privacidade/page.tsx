import type { Metadata } from "next";
import { LegalDraft } from "@/components/site/legal-draft";

export const metadata: Metadata = { title: "Política de privacidade", robots: { index: false, follow: true } };

export default function PrivacyPage() {
  return (
    <LegalDraft
      title="Política de privacidade"
      intro="Como a KLF trata os dados pessoais de quem visita o site, pede contato ou participa de uma avaliação."
      sections={[
        { heading: "Quem é o controlador", text: "Razão social, CNPJ, endereço e o contato do encarregado pelos dados (DPO)." },
        { heading: "Quais dados coletamos e por quê", text: "Contato (nome, e-mail, telefone, mensagem) para responder ao pedido; avaliações anônimas, sem nome, IP, aparelho ou horário exato; base legal de cada tratamento." },
        { heading: "Por quanto tempo guardamos", text: "Mensagens de contato por até 2 anos; respostas de avaliação apenas de forma anônima e agregada." },
        { heading: "Com quem compartilhamos", text: "Fornecedores que ajudam a operar o site: hospedagem, envio de e-mail e armazenamento de imagens." },
        { heading: "Cookies", text: "Somente cookies essenciais: o da sessão do painel e o que evita responder duas vezes a mesma avaliação." },
        { heading: "Seus direitos", text: "Acesso, correção, exclusão e revogação do consentimento, com o canal para pedir e o prazo de resposta." },
      ]}
    />
  );
}
