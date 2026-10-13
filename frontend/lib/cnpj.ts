/** Os 14 caracteres do CNPJ: os 12 primeiros podem ter letras (CNPJ alfanumérico, desde 07/2026); os 2 últimos são dígitos. */
function cnpjCharacters(text: string) {
  let result = "";
  for (const char of text.toUpperCase()) {
    if (result.length >= 14) break;
    const isDigit = char >= "0" && char <= "9";
    const isLetter = char >= "A" && char <= "Z";
    if (isDigit || (isLetter && result.length < 12)) result += char;
  }
  return result;
}

/** "12ABC34501DE35" → "12.ABC.345/01DE-35", parcial enquanto a pessoa digita. */
export function formatCnpj(text: string) {
  const c = cnpjCharacters(text);
  let out = c.slice(0, 2);
  if (c.length > 2) out += `.${c.slice(2, 5)}`;
  if (c.length > 5) out += `.${c.slice(5, 8)}`;
  if (c.length > 8) out += `/${c.slice(8, 12)}`;
  if (c.length > 12) out += `-${c.slice(12, 14)}`;
  return out;
}

/** Próximo valor do campo de CNPJ. Apagar uma pontuação apaga o caractere anterior. */
export function nextCnpjValue(previous: string, raw: string) {
  const before = cnpjCharacters(previous);
  const now = cnpjCharacters(raw);
  if (raw.length < previous.length && now === before) return formatCnpj(now.slice(0, -1));
  return formatCnpj(now);
}
