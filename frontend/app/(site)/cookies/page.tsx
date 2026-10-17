import type { Metadata } from "next";
import Link from "next/link";
import { Badge } from "@/components/site/blocks";
import { CookiePreferencesButton } from "@/components/site/consent";
import { LegalDocument, legalRobots } from "@/components/site/legal-document";
import { getSettings } from "@/lib/api/public";
import { activeCategories, CONSENT_COOKIE } from "@/lib/consent";
import { THEME_STORAGE_KEY } from "@/lib/theme";

export async function generateMetadata(): Promise<Metadata> {
  const { legal } = await getSettings();
  return {
    title: "Política de cookies",
    description: "Quais cookies o site da KLF usa, para que servem e quanto tempo duram.",
    robots: legalRobots(legal),
  };
}

const essentialCookies = [
  {
    name: "klf_refresh",
    purpose: "Mantém a pessoa da equipe KLF conectada ao painel. Só existe para quem faz login no painel.",
    duration: "até 8 horas",
  },
  {
    name: "klf_device",
    purpose:
      "Reconhece o navegador de quem já entrou no painel, para avisar a pessoa da equipe por e-mail quando houver um acesso de um aparelho novo. Guarda só um código aleatório. Só existe para quem faz login no painel.",
    duration: "1 ano desde o último login",
  },
  {
    name: "klf_fb_[código da turma]",
    purpose:
      "Evita que o mesmo navegador responda duas vezes a avaliação de uma turma. Guarda só o código da turma, cifrado, e não identifica quem respondeu.",
    duration: "até 1 dia depois do fim da avaliação",
  },
  {
    name: CONSENT_COOKIE,
    purpose: "Guarda que você já viu o aviso de cookies e a sua escolha sobre os opcionais, para não perguntarmos de novo a cada página.",
    duration: "6 meses",
  },
];

function CookieCard({ name, purpose, duration, kind }: { name: string; purpose: string; duration: string; kind: string }) {
  return (
    <li className="mt-0 rounded-2xl border border-rule bg-card p-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <code className="font-mono text-base">{name}</code>
        <Badge>{kind}</Badge>
      </div>
      <p className="mt-3 font-sans text-[0.9375rem] leading-relaxed text-ink-soft">{purpose}</p>
      <p className="mt-2 font-sans text-sm">
        <span className="text-ink-soft">Duração:</span> {duration}
      </p>
    </li>
  );
}

export default async function CookiesPage() {
  const { legal } = await getSettings();
  const optional = activeCategories();

  return (
    <LegalDocument
      title="Política de cookies"
      intro="Quais cookies o site usa, para que servem e quanto tempo duram. Hoje são poucos: os essenciais e a lembrança do tema que você escolher."
      legal={legal}
      sections={[
        {
          id: "o-que-sao",
          heading: "O que são cookies",
          body: (
            <p>
              São pequenos arquivos que o site grava no seu navegador para lembrar de algo entre uma página e outra, como uma
              sessão aberta. Alguns são essenciais para o site funcionar; outros só podem ser usados com a sua permissão.
            </p>
          ),
        },
        {
          id: "essenciais",
          heading: "Cookies essenciais",
          body: (
            <>
              <p>
                Sem eles, partes do site não funcionam. Por isso não dependem de permissão, mas também não servem para identificar
                visitantes nem para publicidade.
              </p>
              <ul className="mt-6 grid list-none gap-4 p-0">
                {essentialCookies.map((cookie) => (
                  <CookieCard key={cookie.name} {...cookie} kind="Essencial" />
                ))}
              </ul>
            </>
          ),
        },
        {
          id: "tema",
          heading: "Tema diurno ou noturno",
          body: (
            <>
              <p>
                Se você usar o botão de tema, guardamos a escolha no próprio navegador (armazenamento local, não é um cookie e não
                vai para o nosso servidor), para a página abrir sempre do jeito que você prefere.
              </p>
              <ul className="mt-6 grid list-none gap-4 p-0">
                <CookieCard
                  name={THEME_STORAGE_KEY}
                  purpose="Lembra se você escolheu o tema diurno ou o noturno. Só existe depois que você usa o botão de tema."
                  duration="até você apagar os dados do site no navegador"
                  kind="Preferência"
                />
              </ul>
            </>
          ),
        },
        {
          id: "medicao",
          heading: "Medição de visitas sem cookies",
          body: (
            <p>
              Para saber quais páginas são mais úteis, usamos a Vercel Web Analytics, que conta visitas de forma agregada sem
              gravar cookies e sem identificar ninguém. Saiba mais na <Link href="/privacidade#dados">política de privacidade</Link>.
            </p>
          ),
        },
        {
          id: "opcionais",
          heading: "Cookies opcionais",
          body:
            optional.length === 0 ? (
              <p>
                Hoje não usamos nenhum. Se um dia o site mostrar conteúdo de outros serviços, como vídeos do YouTube, eles só vão
                carregar depois que você permitir, e esta página vai listá-los.
              </p>
            ) : (
              <>
                <p>Só carregam depois que você permite. Sem permissão, o site funciona normalmente, só sem esse conteúdo.</p>
                <ul className="mt-6 grid list-none gap-4 p-0">
                  {optional.map((category) => (
                    <CookieCard key={category.id} name={category.label} purpose={category.description} duration="definida por cada serviço" kind="Opcional" />
                  ))}
                </ul>
              </>
            ),
        },
        {
          id: "escolha",
          heading: "Como mudar a sua escolha",
          body: (
            <p>
              A qualquer momento, em{" "}
              <CookiePreferencesButton className="text-brand underline underline-offset-4">preferências de cookies</CookiePreferencesButton>{" "}
              (também no rodapé de todas as páginas). Você também pode apagar ou bloquear cookies nas configurações do navegador;
              se bloquear os essenciais, o painel e as avaliações podem não funcionar.
            </p>
          ),
        },
      ]}
    />
  );
}
