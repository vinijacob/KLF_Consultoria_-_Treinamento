import { cn } from "@/lib/cn";
import { CheckIcon } from "./icons";

export type ChoiceOption = { id: string; label: string };

type ChoiceListProps = {
  name: string;
  type: "radio" | "checkbox";
  options: ChoiceOption[];
  value: string[];
  onChange: (value: string[]) => void;
  describedBy?: string;
  invalid?: boolean;
};

/** Lista de opções: círculo para escolha única, quadrado para múltipla escolha. */
export function ChoiceList({ name, type, options, value, onChange, describedBy, invalid }: ChoiceListProps) {
  function toggle(id: string) {
    if (type === "radio") return onChange([id]);
    onChange(value.includes(id) ? value.filter((item) => item !== id) : [...value, id]);
  }

  return (
    <div className="grid gap-2">
      {options.map((option) => (
        <label
          key={option.id}
          className={cn(
            "flex min-h-12 cursor-pointer items-center gap-3.5 rounded-xs border bg-card px-4 py-2.5 font-sans text-base",
            "transition-colors hover:border-ink has-checked:border-brand has-checked:bg-brand-soft",
            "has-focus-visible:outline-2 has-focus-visible:outline-offset-2 has-focus-visible:outline-ring",
            invalid ? "border-danger" : "border-rule-strong",
          )}
        >
          <input
            type={type}
            name={name}
            value={option.id}
            checked={value.includes(option.id)}
            onChange={() => toggle(option.id)}
            aria-describedby={describedBy}
            className="peer sr-only"
          />
          {type === "radio" ? (
            <span className="grid size-5 shrink-0 place-items-center rounded-full border border-rule-strong bg-card peer-checked:border-brand *:scale-0 peer-checked:*:scale-100">
              <span className="size-2.5 rounded-full bg-brand transition-transform" />
            </span>
          ) : (
            <span className="grid size-5 shrink-0 place-items-center rounded-xs border border-rule-strong bg-card text-brand-contrast peer-checked:border-brand peer-checked:bg-brand *:scale-0 peer-checked:*:scale-100">
              <CheckIcon className="text-sm transition-transform" />
            </span>
          )}
          <span>{option.label}</span>
        </label>
      ))}
    </div>
  );
}
