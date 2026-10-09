export type FeedbackTopic = "Company" | "Training";

export type QuestionType =
  | "ShortText"
  | "LongText"
  | "Scale"
  | "Nps"
  | "SingleChoice"
  | "MultipleChoice";

export type SessionStatus = "Scheduled" | "Open" | "Closed";

export type FormOption = { id: string; label: string };

export type QuestionCondition = { questionId: string; anyOf: string[] };

export type FormQuestion = {
  id: string;
  type: QuestionType;
  text: string;
  helpText?: string | null;
  required: boolean;
  options?: FormOption[] | null;
  scaleMin?: number | null;
  scaleMax?: number | null;
  minLabel?: string | null;
  maxLabel?: string | null;
  showIf?: QuestionCondition | null;
};

export type FormSection = {
  id: string;
  title: string;
  description?: string | null;
  topic: FeedbackTopic;
  questions: FormQuestion[];
};

export type FormDefinition = { sections: FormSection[] };

export type PublicFeedbackForm = {
  sessionTitle: string;
  formTitle: string;
  formDescription?: string | null;
  status: SessionStatus;
  opensAt: string;
  alreadyAnswered: boolean;
  definition?: FormDefinition | null;
};

/** O que a pessoa digitou/escolheu numa pergunta. */
export type AnswerValue = {
  text?: string;
  number?: number | null;
  choices?: string[];
};

export type AnswerState = Record<string, AnswerValue>;

export type AnswerPayload = {
  questionId: string;
  text?: string;
  number?: number;
  choices?: string[];
};
