"use client";

import Link from "next/link";
import { useEffect, useId, useRef, useState, useSyncExternalStore, type ReactNode } from "react";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/field";
import { CloseIcon } from "@/components/ui/icons";
import { cn } from "@/lib/cn";
import {
  activeCategories,
  CONSENT_COOKIE,
  CONSENT_MAX_AGE_DAYS,
  makeChoice,
  needsDecision,
  parseConsent,
  readCookie,
  serializeConsent,
  type ConsentCategory,
  type ConsentChoice,
} from "@/lib/consent";

const CHANGE_EVENT = "klf-consent-change";
const OPEN_EVENT = "klf-consent-open";

function subscribe(onChange: () => void) {
  window.addEventListener(CHANGE_EVENT, onChange);
  return () => window.removeEventListener(CHANGE_EVENT, onChange);
}

const UNDECIDED = "";

/** A escolha atual. No servidor e antes de hidratar responde `undefined` (ainda não se sabe), para nada piscar. */
export function useConsent(): ConsentChoice | null | undefined {
  const raw = useSyncExternalStore(
    subscribe,
    () => readCookie(document.cookie, CONSENT_COOKIE) ?? UNDECIDED,
    () => undefined,
  );
  return raw === undefined ? undefined : parseConsent(raw);
}

function today() {
  return new Intl.DateTimeFormat("en-CA", { timeZone: "America/Manaus" }).format(new Date());
}

function saveConsent(granted: Partial<Record<ConsentCategory, boolean>>) {
  const secure = window.location.protocol === "https:" ? "; Secure" : "";
  const maxAge = CONSENT_MAX_AGE_DAYS * 24 * 60 * 60;
  document.cookie = `${CONSENT_COOKIE}=${serializeConsent(makeChoice(granted, today()))}; Max-Age=${maxAge}; Path=/; SameSite=Lax${secure}`;
  window.dispatchEvent(new Event(CHANGE_EVENT));
}

const allOptional = (value: boolean) => Object.fromEntries(activeCategories().map((category) => [category.id, value]));

/** Botão com cara de link, para o rodapé e para o texto das políticas: abre as preferências de cookies. */
export function CookiePreferencesButton({ className, children = "Preferências de cookies" }: { className?: string; children?: ReactNode }) {
  return (
    <button type="button" onClick={() => window.dispatchEvent(new Event(OPEN_EVENT))} className={className}>
      {children}
    </button>
  );
}

function BannerBox({ onCustomize, onChoose, className }: { onCustomize: () => void; onChoose: (accept: boolean) => void; className?: string }) {
  const titleId = useId();

  return (
    <section aria-labelledby={titleId} className={cn("rounded-2xl border border-rule-strong bg-card p-5 sm:p-6", className)}>
      <h2 id={titleId} className="text-xl">
        Sua privacidade
      </h2>
      <p className="mt-2 font-sans text-[0.9375rem] leading-relaxed text-ink-soft">
        Usamos cookies essenciais para o site funcionar. Com a sua permissão, também carregamos conteúdo de outros serviços, que
        podem gravar cookies próprios. Você escolhe e pode mudar quando quiser.{" "}
        <Link href="/cookies" className="text-brand underline underline-offset-4">
          Política de cookies
        </Link>
      </p>
      <div className="mt-4 flex flex-wrap gap-3">
        <Button variant="outline" onClick={() => onChoose(true)}>
          Aceitar
        </Button>
        <Button variant="outline" onClick={() => onChoose(false)}>
          Recusar
        </Button>
        <Button variant="outline" onClick={onCustomize}>
          Personalizar
        </Button>
      </div>
    </section>
  );
}

/** Só para o guia visual (`/design`): o banner parado, sem gravar nada. */
export function CookieBannerPreview() {
  return <BannerBox onCustomize={() => undefined} onChoose={() => undefined} />;
}

function PreferencesDialog({ open, onClose, choice }: { open: boolean; onClose: () => void; choice: ConsentChoice | null }) {
  const ref = useRef<HTMLDialogElement>(null);
  const titleId = useId();
  const categories = activeCategories();
  const [granted, setGranted] = useState<Partial<Record<ConsentCategory, boolean>>>(() => choice ?? {});

  useEffect(() => {
    const dialog = ref.current;
    if (!dialog) return;
    if (open && !dialog.open) dialog.showModal();
    if (!open && dialog.open) dialog.close();
  }, [open]);

  function decide(value: Partial<Record<ConsentCategory, boolean>>) {
    saveConsent(value);
    onClose();
  }

  return (
    <dialog
      ref={ref}
      onClose={onClose}
      onClick={(event) => {
        if (event.target === event.currentTarget) onClose();
      }}
      aria-labelledby={titleId}
      className="m-auto w-[calc(100%-2rem)] max-w-xl bg-transparent p-0 text-ink backdrop:bg-paper-deep"
    >
      {open && (
        <div className="motion-safe:animate-rise rounded-3xl border border-rule bg-card p-6 sm:p-8">
          <div className="flex items-start justify-between gap-4">
            <h2 id={titleId} className="text-[1.75rem] leading-tight">
              Preferências de cookies
            </h2>
            <button
              type="button"
              onClick={onClose}
              aria-label="Fechar"
              className="-mt-1 -mr-2 grid size-11 shrink-0 place-items-center rounded-md text-2xl hover:bg-paper-deep"
            >
              <CloseIcon />
            </button>
          </div>

          <div className="mt-6 grid gap-4">
            <div className="rounded-2xl bg-paper-deep p-5">
              <p className="font-sans font-medium">Essenciais · sempre ativos</p>
              <p className="mt-1 font-sans text-sm leading-relaxed text-ink-soft">
                Mantêm a sessão da equipe no painel, evitam respostas duplicadas nas avaliações e guardam esta escolha e o tema. Não
                identificam visitantes. A medição de visitas do site é feita sem cookies.
              </p>
            </div>
            {categories.map((category) => (
              <div key={category.id} className="rounded-2xl border border-rule p-5">
                <Checkbox
                  label={category.label}
                  hint={category.description}
                  checked={granted[category.id] === true}
                  onChange={(event) => setGranted({ ...granted, [category.id]: event.target.checked })}
                />
              </div>
            ))}
            {categories.length === 0 && (
              <p className="font-sans text-[0.9375rem] leading-relaxed text-ink-soft">
                Hoje o site usa apenas cookies essenciais, então não há nada para escolher. Quando houver conteúdo de outros
                serviços (como vídeos do YouTube), a escolha aparece aqui.
              </p>
            )}
          </div>

          <div className="mt-6 flex flex-wrap items-center gap-3">
            {categories.length > 0 ? (
              <>
                <Button onClick={() => decide(granted)}>Salvar escolhas</Button>
                <Button variant="outline" onClick={() => decide(allOptional(true))}>
                  Aceitar todos
                </Button>
                <Button variant="outline" onClick={() => decide(allOptional(false))}>
                  Recusar todos
                </Button>
              </>
            ) : (
              <Button variant="outline" onClick={onClose}>
                Entendi
              </Button>
            )}
            <Link href="/cookies" onClick={onClose} className="min-h-11 py-3 font-sans text-sm text-brand underline underline-offset-4">
              Política de cookies
            </Link>
          </div>
        </div>
      )}
    </dialog>
  );
}

/**
 * Banner e preferências de cookies do site público. O banner só aparece quando há alguma categoria opcional em uso
 * (`lib/consent.ts`) e a pessoa ainda não escolheu; Aceitar, Recusar e Personalizar têm o mesmo destaque.
 * As preferências abrem pelo link do rodapé a qualquer momento.
 */
export function ConsentManager() {
  const choice = useConsent();
  const [open, setOpen] = useState(false);

  useEffect(() => {
    const show = () => setOpen(true);
    window.addEventListener(OPEN_EVENT, show);
    return () => window.removeEventListener(OPEN_EVENT, show);
  }, []);

  const showBanner = choice !== undefined && needsDecision(choice) && !open;

  return (
    <>
      {showBanner && (
        <div className="fixed inset-x-0 bottom-0 z-40 px-4 pb-4 sm:px-6 sm:pb-6">
          <BannerBox
            className="motion-safe:animate-rise mx-auto max-w-3xl"
            onCustomize={() => setOpen(true)}
            onChoose={(accept) => saveConsent(allOptional(accept))}
          />
        </div>
      )}
      <PreferencesDialog key={String(open)} open={open} onClose={() => setOpen(false)} choice={choice ?? null} />
    </>
  );
}

/**
 * Conteúdo de outro serviço (vídeo do YouTube, post do Instagram) que só carrega com consentimento. Antes disso
 * mostra uma caixa explicando o que é e um botão para carregar, que grava o aceite daquela categoria.
 */
export function ConsentGate({ category, service, children }: { category: ConsentCategory; service: string; children: ReactNode }) {
  const choice = useConsent();
  if (choice?.[category]) return <>{children}</>;

  return (
    <div className="grid place-items-start gap-4 rounded-2xl border border-rule bg-paper-deep p-6">
      <p className="font-sans text-[0.9375rem] leading-relaxed text-ink-soft">
        Este conteúdo vem do {service}. Ao carregar, o {service} pode gravar cookies no seu navegador.{" "}
        <Link href="/cookies" className="text-brand underline underline-offset-4">
          Saiba mais
        </Link>
      </p>
      <Button variant="outline" onClick={() => saveConsent({ ...(choice ?? {}), [category]: true })}>
        Carregar conteúdo do {service}
      </Button>
    </div>
  );
}
