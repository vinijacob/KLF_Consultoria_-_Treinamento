import { cn } from "@/lib/cn";

type NumberScaleProps = {
  name: string;
  min: number;
  max: number;
  value: number | null;
  onChange: (value: number) => void;
  minLabel?: string | null;
  maxLabel?: string | null;
  describedBy?: string;
  invalid?: boolean;
};

/** Escala numérica (1 a 5, 0 a 10...): botões quadrados de 44 px, navegáveis pelo teclado como um grupo de opções. */
export function NumberScale({ name, min, max, value, onChange, minLabel, maxLabel, describedBy, invalid }: NumberScaleProps) {
  const numbers = Array.from({ length: max - min + 1 }, (_, index) => min + index);

  return (
    <div>
      <div className="grid grid-cols-[repeat(auto-fit,minmax(2.75rem,1fr))] gap-1.5">
        {numbers.map((number) => (
          <label
            key={number}
            className={cn(
              "grid min-h-12 cursor-pointer place-items-center rounded-xs border bg-card font-mono text-lg font-medium",
              "transition-colors hover:border-ink has-checked:border-brand has-checked:bg-brand has-checked:text-brand-contrast",
              "has-focus-visible:outline-2 has-focus-visible:outline-offset-2 has-focus-visible:outline-ring",
              invalid ? "border-danger" : "border-rule-strong",
            )}
          >
            <input
              type="radio"
              name={name}
              value={number}
              checked={value === number}
              onChange={() => onChange(number)}
              aria-describedby={describedBy}
              className="sr-only"
            />
            {number}
          </label>
        ))}
      </div>
      {(minLabel || maxLabel) && (
        <div className="mt-2 flex justify-between gap-4 font-sans text-sm text-ink-soft">
          <span>{minLabel}</span>
          <span className="text-right">{maxLabel}</span>
        </div>
      )}
    </div>
  );
}
