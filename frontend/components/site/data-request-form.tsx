"use client";

import { useId, useRef, useState, type FormEvent } from "react";
import { Button } from "@/components/ui/button";
import { ChoiceList } from "@/components/ui/choice";
import { FieldError, Input, Textarea } from "@/components/ui/field";
import { Notice } from "@/components/ui/notice";
import { ApiError, apiFetch, fieldError } from "@/lib/api/client";
import { dataRequestKinds, needsDetails, type DataRequestKind } from "@/lib/data-requests";
import { formatDay } from "@/lib/format";

type Receipt = { protocol: string; dueOn: string };
type Form = { name: string; email: string; kind: DataRequestKind | ""; details: string; website: string };

const empty: Form = { name: "", email: "", kind: "", details: "", website: "" };

/**
 * Pedido do titular sobre os próprios dados (LGPD, art. 18). Pede só nome, e-mail (por onde respondemos e confirmamos
 * que é a pessoa) e o que ela quer. O campo `website` é a armadilha para robôs, como no contato.
 */
export function DataRequestForm() {
  const [form, setForm] = useState(empty);
  const [pending, setPending] = useState(false);
  const [errors, setErrors] = useState<Record<string, string[]>>();
  const [problem, setProblem] = useState<string | null>(null);
  const [receipt, setReceipt] = useState<Receipt | null>(null);
  const topRef = useRef<HTMLDivElement>(null);
  const honeypotId = useId();
  const kindLegendId = useId();
  const error = (field: string) => fieldError(errors, field);
  const set = <K extends keyof Form>(key: K, value: Form[K]) => {
    setForm((current) => ({ ...current, [key]: value }));
    setErrors((current) => current && Object.fromEntries(Object.entries(current).filter(([name]) => name.toLowerCase() !== key.toLowerCase())));
  };

  async function submit(event: FormEvent) {
    event.preventDefault();
    if (!form.kind) {
      setErrors({ kind: ["Escolha o que você quer pedir."] });
      setProblem("Confira os campos marcados.");
      return;
    }
    setPending(true);
    setErrors(undefined);
    setProblem(null);
    try {
      setReceipt(
        await apiFetch<Receipt>("/public/data-requests", {
          method: "POST",
          body: JSON.stringify({
            name: form.name,
            email: form.email,
            kind: form.kind,
            details: form.details.trim() || null,
            website: form.website || null,
          }),
        }),
      );
      setForm(empty);
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

  if (receipt) {
    return (
      <div ref={topRef} className="scroll-mt-28 rounded-3xl border border-rule bg-card p-7 sm:p-10">
        <Notice tone="success" title="Pedido recebido" role="status">
          Anote o protocolo <strong className="font-mono">{receipt.protocol}</strong>. Respondemos pelo e-mail informado até{" "}
          <strong>{formatDay(receipt.dueOn)}</strong>. Antes de entregar, corrigir ou apagar dados, podemos pedir que você confirme
          por e-mail que o pedido é seu: é para ninguém pedir os seus dados no seu lugar.
        </Notice>
        <Button variant="outline" className="mt-6" onClick={() => setReceipt(null)}>
          Fazer outro pedido
        </Button>
      </div>
    );
  }

  return (
    <div ref={topRef} className="scroll-mt-28 rounded-3xl border border-rule bg-card p-7 sm:p-10">
      <h2 className="text-3xl">Fazer um pedido</h2>
      <p className="mt-2 font-sans text-[0.9375rem] text-ink-soft">Campos sem “(opcional)” são necessários para atendermos.</p>

      <form noValidate onSubmit={(event) => void submit(event)} className="mt-8 grid gap-6">
        {problem && (
          <Notice tone="error" role="alert">
            {problem}
          </Notice>
        )}

        <fieldset>
          <legend id={kindLegendId} className="mb-2 font-sans text-sm font-medium text-ink">
            O que você quer pedir?
          </legend>
          <ChoiceList
            name="kind"
            type="radio"
            options={dataRequestKinds.map((kind) => ({ id: kind.id, label: `${kind.label}. ${kind.hint}` }))}
            value={form.kind ? [form.kind] : []}
            onChange={([value]) => set("kind", value as DataRequestKind)}
            invalid={Boolean(error("kind"))}
          />
          <FieldError>{error("kind")}</FieldError>
        </fieldset>

        <div className="grid gap-6 sm:grid-cols-2">
          <Input label="Nome completo" autoComplete="name" required maxLength={120} value={form.name} onChange={(event) => set("name", event.target.value)} error={error("name")} />
          <Input
            label="E-mail"
            hint="Respondemos por aqui."
            type="email"
            autoComplete="email"
            required
            maxLength={200}
            value={form.email}
            onChange={(event) => set("email", event.target.value)}
            error={error("email")}
          />
        </div>

        <Textarea
          label="Detalhes"
          optional={!form.kind || !needsDetails(form.kind)}
          hint="Ajuda a encontrar os seus dados: onde você falou com a KLF (formulário do site, depoimento, WhatsApp), com qual e-mail ou nome, e o que precisa."
          maxLength={2000}
          value={form.details}
          onChange={(event) => set("details", event.target.value)}
          error={error("details")}
          className="min-h-32"
        />

        <div aria-hidden className="absolute -left-[9999px] h-px w-px overflow-hidden">
          <label htmlFor={honeypotId}>Não preencha este campo</label>
          <input id={honeypotId} name="website" tabIndex={-1} autoComplete="off" value={form.website} onChange={(event) => set("website", event.target.value)} />
        </div>

        <p className="font-sans text-sm leading-relaxed text-ink-soft">
          Usamos o nome e o e-mail só para atender a este pedido e guardamos o registro por 5 anos depois de respondido, como
          prova de que atendemos.
        </p>

        <div>
          <Button type="submit" size="lg" disabled={pending}>
            {pending ? "Enviando…" : "Enviar pedido"}
          </Button>
        </div>
      </form>
    </div>
  );
}
