"use client";

import { useState } from "react";
import { ChoiceList } from "@/components/ui/choice";
import { NumberScale } from "@/components/ui/number-scale";

export function ChoiceDemo() {
  const [radio, setRadio] = useState(["b"]);
  const [check, setCheck] = useState(["a"]);

  return (
    <div className="space-y-6">
      <ChoiceList name="d3" type="radio" value={radio} onChange={setRadio} options={[{ id: "a", label: "Presencial" }, { id: "b", label: "Online" }]} />
      <ChoiceList name="d4" type="checkbox" value={check} onChange={setCheck} options={[{ id: "a", label: "Vendas" }, { id: "b", label: "Liderança" }]} />
    </div>
  );
}

export function ScaleDemo() {
  const [value, setValue] = useState<number | null>(8);

  return <NumberScale name="d5" min={0} max={10} value={value} onChange={setValue} minLabel="Nada provável" maxLabel="Muito provável" />;
}
