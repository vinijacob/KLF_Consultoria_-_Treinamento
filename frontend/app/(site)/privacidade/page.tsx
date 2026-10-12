import type { Metadata } from "next";
import Link from "next/link";
import { CookiePreferencesButton } from "@/components/site/consent";
import { Datum, LegalDocument, legalRobots, siteHost } from "@/components/site/legal-document";
import { siteConfig } from "@/config/site";
import { dataProcessors } from "@/content/legal";
import { getSettings } from "@/lib/api/public";

export async function generateMetadata(): Promise<Metadata> {
  const { legal } = await getSettings();
  return {
    title: "Política de privacidade",
    description: "Como a KLF trata os dados pessoais de quem visita o site, fala com a gente ou participa de uma avaliação.",
    robots: legalRobots(legal),
  };
}

export default async function PrivacyPage() {
  const { legal } = await getSettings();
  const company = <Datum value={legal?.companyName} label="razão social" />;
  const dpoEmail = legal?.dpoEmail ? <a href={`mailto:${legal.dpoEmail}`}>{legal.dpoEmail}</a> : <Datum label="e-mail do encarregado" />;

  return (
    <LegalDocument
      title="Política de privacidade"
      intro="Como tratamos os dados pessoais de quem visita o site, fala com a gente, participa de uma avaliação ou dá um depoimento. Escrevemos em linguagem simples; se algo não ficar claro, pergunte."
      legal={legal}
      sections={[
        {
          id: "quem-somos",
          heading: "Quem cuida dos seus dados",
          body: (
            <>
              <p>
                A controladora dos dados tratados no site {siteHost(siteConfig.url)}, no painel da equipe e nas avaliações por QR Code é{" "}
                {company} (“KLF”), CNPJ <Datum value={legal?.cnpj} label="CNPJ" />, com sede em <Datum value={legal?.address} label="endereço" />.
              </p>
              <p>
                O encarregado pelo tratamento de dados pessoais é <Datum value={legal?.dpoName} label="nome do encarregado" />, pelo
                e-mail {dpoEmail}. É com essa pessoa que você fala para tirar dúvidas, fazer pedidos sobre os seus dados ou reclamar.
              </p>
            </>
          ),
        },
        {
          id: "dados",
          heading: "Que dados tratamos e por quê",
          body: (
            <>
              <p>Tratamos só o necessário para cada situação. A base legal de cada uma está entre parênteses (LGPD, art. 7º).</p>
              <h3>Ao navegar pelo site</h3>
              <ul>
                <li>
                  <strong>Medição de visitas sem cookies:</strong> contamos páginas vistas, país, tipo de aparelho, navegador e o site
                  de onde você veio, de forma agregada e sem identificar ninguém. Serve para saber o que é útil e melhorar o
                  conteúdo (legítimo interesse).
                </li>
                <li>
                  <strong>Registros de acesso:</strong> endereço IP, data e hora e a página pedida ficam guardados pela hospedagem
                  por 6 meses, como exige o Marco Civil da Internet (Lei 12.965/2014, art. 15), e servem também para proteger o
                  site contra abusos (cumprimento de obrigação legal e legítimo interesse).
                </li>
              </ul>
              <h3>Quando você fala com a gente</h3>
              <p>
                Pelo WhatsApp, e-mail ou telefone, recebemos o que você decidir enviar: normalmente nome, contato, empresa e a
                mensagem. Usamos para responder e preparar uma proposta (procedimentos preliminares a um contrato, a seu pedido).
                O WhatsApp é um serviço da Meta e segue também a política de privacidade dela.
              </p>
              <h3>Nas avaliações por QR Code</h3>
              <p>
                A avaliação de um treinamento é anônima: não pedimos nome, e-mail ou documento e não guardamos endereço IP,
                aparelho, localização nem o horário exato da resposta, só a data. As respostas aparecem agrupadas por turma, só
                depois de pelo menos 3 respostas, e os comentários são mostrados em ordem aleatória. A empresa que contratou o
                treinamento recebe apenas esses resultados agrupados.
              </p>
              <p>
                Como não identificam ninguém, essas respostas não são dados pessoais (LGPD, art. 12). Por isso mesmo, pedimos que
                você não escreva nos comentários o seu nome nem o de colegas ou clientes.
              </p>
              <h3>Nos depoimentos publicados</h3>
              <p>
                Nome, cargo, empresa, o texto do depoimento e, se você autorizar, a sua foto. Só publicamos com um termo de
                autorização assinado (consentimento), e você pode retirá-lo a qualquer momento: o depoimento sai do site.
              </p>
              <h3>Clientes e lojas</h3>
              <p>
                Nomes e logotipos das empresas atendidas aparecem no site com autorização delas. São dados de empresas, não de
                pessoas.
              </p>
              <h3>No painel da equipe</h3>
              <p>
                Quem trabalha com a KLF entra no painel com nome, e-mail, senha (guardada só de forma cifrada, nunca em texto) e um
                segundo fator de verificação. Registramos as sessões para manter a conta segura (execução de contrato e legítimo
                interesse).
              </p>
            </>
          ),
        },
        {
          id: "prazos",
          heading: "Por quanto tempo guardamos",
          body: (
            <ul>
              <li>Registros de acesso ao site: 6 meses.</li>
              <li>Conversas e pedidos de contato: até 2 anos depois do último contato, a menos que vire um contrato.</li>
              <li>Depoimentos: enquanto a autorização valer. Retirada a autorização, o depoimento sai do site.</li>
              <li>Avaliações: as respostas anônimas ficam como histórico das turmas, sem prazo, porque não identificam ninguém.</li>
              <li>Contas do painel: enquanto a pessoa trabalhar com a KLF.</li>
            </ul>
          ),
        },
        {
          id: "compartilhamento",
          heading: "Com quem compartilhamos",
          body: (
            <>
              <p>Não vendemos nem alugamos dados. Para o site funcionar, contamos com estes fornecedores, que tratam dados em nosso nome e seguindo nossas instruções:</p>
              <ul>
                {dataProcessors.map((processor) => (
                  <li key={processor.name}>
                    <strong>{processor.name}</strong>: {processor.role}.
                  </li>
                ))}
              </ul>
              <p>Também podemos entregar dados a autoridades quando a lei ou uma ordem judicial obrigar.</p>
            </>
          ),
        },
        {
          id: "transferencia",
          heading: "Dados fora do Brasil",
          body: (
            <p>
              Esses fornecedores guardam dados em servidores fora do Brasil, principalmente nos Estados Unidos. Essa transferência
              segue o art. 33 da LGPD e as regras da Autoridade Nacional de Proteção de Dados (ANPD) sobre transferência
              internacional.
            </p>
          ),
        },
        {
          id: "cookies",
          heading: "Cookies",
          body: (
            <p>
              Usamos só cookies essenciais, medimos visitas sem cookies e, se você escolher um tema, guardamos essa escolha no seu
              navegador. A lista completa, com o que cada um faz e quanto dura, está
              na <Link href="/cookies">política de cookies</Link>. Você pode rever sua escolha em{" "}
              <CookiePreferencesButton className="text-brand underline underline-offset-4">preferências de cookies</CookiePreferencesButton>.
            </p>
          ),
        },
        {
          id: "seguranca",
          heading: "Como protegemos",
          body: (
            <p>
              Todo o tráfego é cifrado (HTTPS); senhas são guardadas só de forma cifrada; o painel exige verificação em duas
              etapas; cada pessoa da equipe só vê o que precisa; e coletamos o mínimo possível. Nenhum sistema é infalível: se
              acontecer um incidente que traga risco a você, avisaremos você e a ANPD, como manda a lei.
            </p>
          ),
        },
        {
          id: "direitos",
          heading: "Seus direitos",
          body: (
            <>
              <p>Pela LGPD (art. 18), você pode pedir, a qualquer momento:</p>
              <ul>
                <li>confirmação de que tratamos dados seus e acesso a eles;</li>
                <li>correção de dados incompletos, errados ou desatualizados;</li>
                <li>anonimização, bloqueio ou eliminação de dados desnecessários ou tratados em desacordo com a lei;</li>
                <li>portabilidade dos dados a outro fornecedor;</li>
                <li>eliminação dos dados tratados com o seu consentimento e a retirada desse consentimento;</li>
                <li>informação sobre com quem compartilhamos seus dados;</li>
                <li>oposição a um tratamento que não respeite a lei.</li>
              </ul>
              <p>
                Envie o pedido para {dpoEmail}. Respondemos em até 15 dias. Se não ficar satisfeito, você também pode reclamar à
                ANPD, em <a href="https://www.gov.br/anpd" rel="noopener noreferrer">gov.br/anpd</a>.
              </p>
            </>
          ),
        },
        {
          id: "criancas",
          heading: "Crianças",
          body: <p>O site não é feito para crianças e não coletamos de propósito dados de menores de 12 anos.</p>,
        },
        {
          id: "mudancas",
          heading: "Mudanças nesta política",
          body: (
            <p>
              Quando esta política mudar, a data no topo da página muda junto. Se a mudança for importante, avisaremos aqui no
              site.
            </p>
          ),
        },
      ]}
    />
  );
}
