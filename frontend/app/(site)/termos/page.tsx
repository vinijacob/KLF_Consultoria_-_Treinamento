import type { Metadata } from "next";
import { LegalDraft } from "@/components/site/legal-draft";

export const metadata: Metadata = { title: "Termos de uso", robots: { index: false, follow: true } };

export default function TermsPage() {
  return (
    <LegalDraft
      title="Termos de uso"
      intro="Regras para usar o site e o formulário de avaliação da KLF Consultoria & Treinamento."
      sections={[
        { heading: "Uso do site", text: "Finalidade do site, condutas não permitidas e disponibilidade do serviço." },
        { heading: "Avaliações anônimas", text: "Como funciona a avaliação por QR Code, o que é coletado e o compromisso com o anonimato." },
        { heading: "Propriedade do conteúdo", text: "Textos, imagens e materiais pertencem à KLF ou a quem autorizou o uso; reprodução depende de autorização." },
        { heading: "Responsabilidades", text: "Limites de responsabilidade sobre informações publicadas e links externos." },
        { heading: "Foro e contato", text: "Legislação aplicável, foro e o canal para dúvidas sobre estes termos." },
      ]}
    />
  );
}
