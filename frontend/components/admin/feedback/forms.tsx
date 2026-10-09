"use client";

import { useState, type ReactNode } from "react";
import { optional, useEntityEditor } from "@/lib/admin/editor";
import { useApi } from "@/lib/admin/hooks";
import { plural } from "@/lib/admin/text";
import { fieldError } from "@/lib/api/client";
import { starterDefinition, toPayload } from "@/lib/feedback/builder";
import type { FeedbackFormDetail, FeedbackFormListItem } from "@/types/admin";
import type { FormDefinition } from "@/types/feedback";
import { Input, Textarea } from "@/components/ui/field";
import { Notice } from "@/components/ui/notice";
import { useAuth } from "../auth-provider";
import { EntityLoader, FlashNotice } from "../entity";
import { AdminForm, AdminHeader, ConfirmButton, EmptyState, IndexList, IndexRow, LoadError, Loading, NewButton, Section } from "../ui";
import { FormBuilder, PreviewButton } from "./builder";

const LIST = "/painel/avaliacoes/formularios";

/** Avaliações são só da administração (a API devolve 403 para Edição; aqui só explicamos). */
export function AdminOnly({ children }: { children: ReactNode }) {
  const { isAdmin } = useAuth();
  if (isAdmin) return <>{children}</>;

  return (
    <Notice tone="privacy" title="Área da administração">
      Avaliações e resultados das turmas ficam só com a administração, porque as respostas falam de empresas clientes.
    </Notice>
  );
}

export function FormsList() {
  const { data, error, reload } = useApi<FeedbackFormListItem[]>("/admin/feedback-forms");

  return (
    <div>
      <AdminHeader
        kicker="Avaliações"
        title="Formulários"
        description="Modelos de perguntas. Cada turma copia o modelo ao ser criada: mudar o modelo depois não altera turmas existentes."
        actions={<NewButton href={`${LIST}/novo`}>Novo formulário</NewButton>}
      />
      <FlashNotice />
      {error && <LoadError error={error} onRetry={reload} />}
      {!data && !error && <Loading />}
      {data?.length === 0 && (
        <EmptyState title="Nenhum formulário ainda" action={<NewButton href={`${LIST}/novo`}>Criar o primeiro</NewButton>}>
          O formulário novo já vem com as duas partes: sobre a empresa do participante e sobre o treinamento.
        </EmptyState>
      )}
      {data && data.length > 0 && (
        <IndexList>
          {data.map((form, index) => (
            <IndexRow
              key={form.id}
              href={`${LIST}/${form.id}`}
              number={index + 1}
              title={form.title}
              meta={`${plural(form.sectionCount, "parte", "partes")} · ${plural(form.questionCount, "pergunta", "perguntas")}`}
            />
          ))}
        </IndexList>
      )}
    </div>
  );
}

function FormTemplate({ form }: { form?: FeedbackFormDetail }) {
  const [title, setTitle] = useState(form?.title ?? "Avaliação do treinamento");
  const [description, setDescription] = useState(form?.description ?? "");
  const [definition, setDefinition] = useState<FormDefinition>(() => form?.definition ?? starterDefinition());
  const editor = useEntityEditor<FeedbackFormDetail>({
    apiPath: "/admin/feedback-forms",
    listHref: LIST,
    id: form?.id,
    created: "Formulário criado.",
    deleted: "Formulário excluído.",
  });

  return (
    <>
      <AdminHeader
        kicker="Formulários de avaliação"
        title={form ? form.title : "Novo formulário"}
        back={{ href: LIST, label: "Todos os formulários" }}
        actions={<PreviewButton title={title} description={description} definition={definition} />}
      />
      <AdminForm
        pending={editor.pending}
        message={editor.message}
        savedNote={editor.savedNote}
        submitLabel={form ? "Salvar formulário" : "Criar formulário"}
        onSubmit={() => void editor.save({ title, description: optional(description), definition: toPayload(definition) })}
        extraActions={
          form && (
            <ConfirmButton
              label="Excluir formulário"
              title="Excluir este formulário?"
              description={<p>Turmas já criadas com ele não mudam: cada uma tem a própria cópia das perguntas.</p>}
              confirmLabel="Excluir"
              onConfirm={editor.remove}
            />
          )
        }
      >
        <Section title="Apresentação" description="O participante vê o título e o texto no topo do formulário.">
          <Input label="Título" value={title} maxLength={200} onChange={(event) => setTitle(event.target.value)} error={fieldError(editor.errors, "title")} />
          <Textarea
            label="Texto de abertura"
            optional
            value={description}
            onChange={(event) => setDescription(event.target.value)}
            error={fieldError(editor.errors, "description")}
            className="min-h-24"
          />
        </Section>
        <Section title="Perguntas" description="Perguntas sobre a empresa são as mais sensíveis: o participante avalia o próprio empregador. Nunca peça nome, cargo ou dados que identifiquem a pessoa.">
          <FormBuilder value={definition} onChange={setDefinition} errors={editor.errors} />
        </Section>
      </AdminForm>
    </>
  );
}

export function FormTemplateEditor({ id }: { id?: string }) {
  return (
    <EntityLoader<FeedbackFormDetail> path={id ? `/admin/feedback-forms/${id}` : null}>
      {(form) => <FormTemplate form={form} />}
    </EntityLoader>
  );
}
