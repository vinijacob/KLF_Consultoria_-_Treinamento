const MAX_DIGITS = 13;

const digitsOf = (text: string) => text.replace(/\D/g, "");

/** "5592988887777" → "+55 (92) 98888-7777". Fixo de 8 dígitos sai como 4-4; celular de 9, como 5-4. */
export function formatPhone(digits: string) {
  const d = digitsOf(digits).slice(0, MAX_DIGITS);
  if (!d) return "";

  const country = d.slice(0, 2);
  const area = d.slice(2, 4);
  const number = d.slice(4);
  let text = `+${country}`;

  if (area) text += ` (${area}${area.length === 2 ? ")" : ""}`;
  if (number) {
    const split = number.length <= 8 ? 4 : 5;
    text += ` ${number.length > split ? `${number.slice(0, split)}-${number.slice(split)}` : number}`;
  }

  return text;
}

/**
 * Próximo valor do campo de telefone enquanto a pessoa digita, apaga ou cola. Sem "+", o número é brasileiro e ganha o
 * 55 (a menos que já venha com ele e o tamanho de número internacional). Apagar uma pontuação apaga o dígito anterior.
 */
export function nextPhoneValue(previous: string, raw: string) {
  const previousDigits = digitsOf(previous);
  let digits = digitsOf(raw);

  if (!digits) return raw.trim() === "+" ? "+" : "";

  if (raw.length < previous.length && digits === previousDigits) {
    digits = digits.slice(0, -1);
    return formatPhone(digits);
  }

  const continuing = previousDigits !== "" && (digits.startsWith(previousDigits) || previousDigits.startsWith(digits));
  const national = !raw.includes("+") && !continuing && !(digits.length >= 12 && digits.startsWith("55"));

  return formatPhone(national ? `55${digits}` : digits);
}

/** Valor salvo (só dígitos com DDI, ou texto livre antigo) → como aparece no campo. */
export function phoneFieldValue(saved: string | null | undefined) {
  if (!saved) return "";
  return nextPhoneValue("", saved);
}
