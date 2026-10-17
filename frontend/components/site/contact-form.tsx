"use client";

import Link from "next/link";
import { useId, useRef, useState, type FormEvent } from "react";
import { Button } from "@/components/ui/button";
import { Checkbox, Input, PhoneInput, Select, Textarea } from "@/components/ui/field";
import { Notice } from "@/components/ui/notice";
import { ApiError, apiFetch, fieldError } from "@/lib/api/client";
import { LEGAL_TEXT_REVISED_ON } from "@/lib/legal";
import type { ServiceListItem } from "@/types/site";

type Form = { name: string; email: string; phone: string; company: string; serviceId: string; message: string; accepts: boolean; website: string };

const empty = (serviceId = ""): Form => ({ name: "", email: "", phone: "", company: "", serviceId, message: "", accepts: false, website: "" });

const MESSAGE_MAX = 4000;

/**
 * Formulário de contato e pedido de proposta. Pede só o necessário para responder (nome, e-mail e a mensagem); o
 * resto é opcional. O campo `website` fica escondido de pessoas: robôs costumam preenchê-lo e a API descarta a mensagem.
 */
export function ContactForm({ services, initialServiceId }: { services: Pick<ServiceListItem, "id" | "title">[]; initialServiceId?: string }) {
  const [form, setForm] = useState(() => empty(initialServiceId));
  const [pending, setPending] = useState(false);
  const [errors, setErrors] = useState<Record<string, string[]>>();
  const [problem, setProblem] = useState<string | null>(null);
  const [sent, setSent] = useState(false);
  const topRef = useRef<HTMLDivElement>(null);
  const honeypotId = useId();
  const set = <K extends keyof Form>(key: K, value: Form[K]) => {
    setForm((current) => ({ ...current, [key]: value }));
    const field = key === "accepts" ? "acceptsprivacypolicy" : key.toLowerCase();
    setErrors((current) => current && Object.fromEntries(Object.entries(current).filter(([name]) => name.toLowerCase() !== field)));
  };
  const error = (field: string) => fieldError(errors, field);

  async function submit(event: FormEvent) {
    event.preventDefault();
    setPending(true);
    setErrors(undefined);
    setProblem(null);
    try {
      await apiFetch("/public/contact", {
        method: "POST",
        body: JSON.stringify({
          name: form.name,
          email: form.email,
          phone: form.phone.trim() || null,
          company: form.company.trim() || null,
          serviceId: form.serviceId || null,
          message: form.message,
          acceptsPrivacyPolicy: form.accepts,
          privacyPolicyVersion: LEGAL_TEXT_REVISED_ON,
          website: form.website || null,
        }),
      });
      setSent(true);
      setForm(empty());
    } catch (failure) {
      if (failure instanceof ApiError && failure.errors) {
        setErrors(failure.errors);
        setProblem("Confira os campos marcados.");
      } else {
        setProblem(failure instanceof ApiError ? failure.message : "Não foi possível enviar agora. Verifique a conexão e tente de novo.");
      }
    } finally {
      setPending(false);
      topRef.current?.scrollIntoView({ block: "start" });
    }
  }

  if (sent) {
    return (
      <div ref={topRef} className="scroll-mt-28 rounded-3xl border border-rule bg-card p-7 sm:p-10">
        <Notice tone="success" title="Mensagem enviada" role="status">
          Recebemos o seu contato. A Kilciene responde pelo e-mail que você informou.
        </Notice>
        <Button variant="outline" className="mt-6" onClick={() => setSent(false)}>
          Enviar outra mensagem
        </Button>
      </div>
    );
  }

  return (
    <div ref={topRef} className="scroll-mt-28 rounded-3xl border border-rule bg-card p-7 sm:p-10">
      <h2 className="text-3xl">Pedir proposta ou tirar dúvidas</h2>
      <p className="mt-2 font-sans text-[0.9375rem] text-ink-soft">Campos sem “(opcional)” são necessários para respondermos.</p>

      <form noValidate onSubmit={(event) => void submit(event)} className="mt-8 grid gap-6">
        {problem && (
          <Notice tone="error" role="alert">
            {problem}
          </Notice>
        )}
        <div className="grid gap-6 sm:grid-cols-2">
          <Input label="Nome" autoComplete="name" required maxLength={120} value={form.name} onChange={(event) => set("name", event.target.value)} error={error("name")} />
          <Input
            label="E-mail"
            type="email"
            autoComplete="email"
            required
            maxLength={200}
            value={form.email}
            onChange={(event) => set("email", event.target.value)}
            error={error("email")}
          />
          <PhoneInput
            label="Telefone ou WhatsApp"
            optional
            autoComplete="tel"
            value={form.phone}
            onValueChange={(value) => set("phone", value)}
            error={error("phone")}
          />
          <Input
            label="Empresa ou loja"
            optional
            autoComplete="organization"
            maxLength={150}
            value={form.company}
            onChange={(event) => set("company", event.target.value)}
            error={error("company")}
          />
        </div>
        {services.length > 0 && (
          <Select label="Interesse" optional value={form.serviceId} onChange={(event) => set("serviceId", event.target.value)} error={error("serviceId")}>
            <option value="">Ainda não sei / outro assunto</option>
            {services.map((service) => (
              <option key={service.id} value={service.id}>
                {service.title}
              </option>
            ))}
          </Select>
        )}
        <Textarea
          label="Mensagem"
          hint="Conte o momento da empresa, o tamanho da equipe e o que você espera."
          required
          maxLength={MESSAGE_MAX}
          value={form.message}
          onChange={(event) => set("message", event.target.value)}
          error={error("message")}
          className="min-h-40"
        />

        <div aria-hidden className="absolute -left-[9999px] h-px w-px overflow-hidden">
          <label htmlFor={honeypotId}>Não preencha este campo</label>
          <input id={honeypotId} name="website" tabIndex={-1} autoComplete="off" value={form.website} onChange={(event) => set("website", event.target.value)} />
        </div>

        <Checkbox
          label={
            <>
              Li a{" "}
              <Link href="/privacidade" target="_blank" className="text-brand underline underline-offset-4">
                política de privacidade
                <span className="sr-only"> (abre em nova aba)</span>
              </Link>{" "}
              e concordo que a KLF use estes dados para responder ao meu pedido.
            </>
          }
          hint={
            <>
              Guardamos a mensagem por até 2 anos e você pode{" "}
              <Link href="/meus-dados" className="text-brand underline underline-offset-4">
                pedir a exclusão
              </Link>{" "}
              a qualquer momento.
            </>
          }
          checked={form.accepts}
          onChange={(event) => set("accepts", event.target.checked)}
          error={error("acceptsPrivacyPolicy")}
        />

        <div>
          <Button type="submit" variant="accent" size="lg" disabled={pending}>
            {pending ? "Enviando…" : "Enviar mensagem"}
          </Button>
        </div>
      </form>
    </div>
  );
}
