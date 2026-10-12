import type { Metadata } from "next";
import Link from "next/link";
import { Datum, LegalDocument, legalRobots, siteHost } from "@/components/site/legal-document";
import { siteConfig } from "@/config/site";
import { getSettings } from "@/lib/api/public";

export async function generateMetadata(): Promise<Metadata> {
  const { legal } = await getSettings();
  return {
    title: "Termos de uso",
    description: "Regras para usar o site e as avaliações por QR Code da KLF Consultoria & Treinamento.",
    robots: legalRobots(legal),
  };
}

export default async function TermsPage() {
  const { legal, contact } = await getSettings();
  const email = contact?.email ?? legal?.dpoEmail;

  return (
    <LegalDocument
      title="Termos de uso"
      intro="As regras para usar o site e as avaliações por QR Code. São poucas e diretas."
      legal={legal}
      sections={[
        {
          id: "aceite",
          heading: "Quem somos e o aceite",
          body: (
            <p>
              O site {siteHost(siteConfig.url)} é de <Datum value={legal?.companyName} label="razão social" /> (“KLF”), CNPJ{" "}
              <Datum value={legal?.cnpj} label="CNPJ" />. Ao usar o site ou responder a uma avaliação, você concorda com estes termos e
              com a <Link href="/privacidade">política de privacidade</Link>.
            </p>
          ),
        },
        {
          id: "site",
          heading: "Para que serve o site",
          body: (
            <p>
              Apresentar os treinamentos, palestras e o trabalho da Kilciene Lima Ferreira, publicar conteúdos e receber as
              avaliações das turmas. As informações sobre serviços, cargas horárias e formatos são uma apresentação: não são uma
              proposta comercial. Valores, datas e condições só valem quando combinados por escrito.
            </p>
          ),
        },
        {
          id: "avaliacoes",
          heading: "Avaliações por QR Code",
          body: (
            <>
              <p>
                A avaliação é feita para quem participou da turma, uma resposta por pessoa, e é anônima: nem a KLF nem a empresa
                contratante sabem quem respondeu. Para que continue assim e seja útil para todos, não é permitido:
              </p>
              <ul>
                <li>tentar descobrir quem respondeu ou pedir que alguém mostre a própria resposta;</li>
                <li>responder mais de uma vez ou usar programas para enviar respostas;</li>
                <li>escrever ofensas, discriminação ou dados pessoais seus ou de outras pessoas;</li>
                <li>compartilhar o QR Code fora da turma.</li>
              </ul>
              <p>A KLF pode desconsiderar respostas que descumpram estas regras.</p>
            </>
          ),
        },
        {
          id: "uso",
          heading: "Uso permitido",
          body: (
            <p>
              Você pode navegar, ler e compartilhar os links do site à vontade. Não é permitido tentar invadir o site ou o painel,
              burlar as proteções, sobrecarregar os servidores ou copiar o conteúdo em massa por programas.
            </p>
          ),
        },
        {
          id: "propriedade",
          heading: "Textos, imagens e marca",
          body: (
            <p>
              Textos, fotos, materiais e a marca KLF pertencem à KLF ou a quem autorizou o uso, e são protegidos pela Lei de
              Direitos Autorais (Lei 9.610/1998) e pela Lei de Propriedade Industrial (Lei 9.279/1996). Você pode citar trechos com a
              fonte e o link; para reproduzir mais do que isso, peça autorização. Depoimentos, logotipos de clientes e fotos de
              terceiros aparecem com autorização dos donos e continuam sendo deles.
            </p>
          ),
        },
        {
          id: "terceiros",
          heading: "Links e serviços de terceiros",
          body: (
            <p>
              O site tem links para WhatsApp, redes sociais e outros sites. Eles têm regras e políticas próprias, e a KLF não
              responde pelo que acontece neles.
            </p>
          ),
        },
        {
          id: "responsabilidade",
          heading: "Disponibilidade e responsabilidade",
          body: (
            <p>
              Fazemos o possível para manter o site no ar e com informações corretas, mas ele pode ficar fora do ar para manutenção
              ou por falhas que não dependem de nós. Os conteúdos publicados são educativos: ajudam a pensar e a praticar, mas os
              resultados de cada equipe dependem de muitos fatores.
            </p>
          ),
        },
        {
          id: "mudancas",
          heading: "Mudanças nestes termos",
          body: <p>Podemos atualizar estes termos. A data no topo da página mostra a versão em vigor.</p>,
        },
        {
          id: "foro",
          heading: "Lei e foro",
          body: (
            <p>
              Valem as leis brasileiras. Fica eleito o foro da comarca de <Datum value={legal?.forumCity} label="cidade do foro" />,
              ressalvado o direito do consumidor de propor ação no foro do próprio domicílio (Código de Defesa do Consumidor, art.
              101, I). Dúvidas sobre estes termos:{" "}
              {email ? <a href={`mailto:${email}`}>{email}</a> : <Datum label="e-mail de contato" />}.
            </p>
          ),
        },
      ]}
    />
  );
}
