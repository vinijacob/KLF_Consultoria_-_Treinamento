/*
 * O painel trabalha no horário de Manaus (`AppTimeZone` da API), independente do fuso do computador.
 * `<input type="datetime-local">` usa "2026-10-08T14:30"; a API recebe e devolve ISO com fuso.
 */
export const TIME_ZONE = "America/Manaus";

const parts = new Intl.DateTimeFormat("en-CA", {
  timeZone: TIME_ZONE,
  year: "numeric",
  month: "2-digit",
  day: "2-digit",
  hour: "2-digit",
  minute: "2-digit",
  hourCycle: "h23",
});

function localParts(date: Date) {
  const map = Object.fromEntries(parts.formatToParts(date).map((part) => [part.type, part.value]));
  return { date: `${map.year}-${map.month}-${map.day}`, time: `${map.hour}:${map.minute}` };
}

/** ISO da API → valor do `datetime-local` no horário de Manaus. */
export function toLocalInput(iso: string | null | undefined) {
  if (!iso) return "";
  const local = localParts(new Date(iso));
  return `${local.date}T${local.time}`;
}

/** Diferença entre o horário de Manaus e o UTC num instante, em minutos (ex.: -240). */
function offsetMinutes(at: Date) {
  const local = localParts(at);
  const asUtc = Date.parse(`${local.date}T${local.time}:00Z`);
  return Math.round((asUtc - Math.floor(at.getTime() / 60_000) * 60_000) / 60_000);
}

/** Valor do `datetime-local` (horário de Manaus) → ISO com fuso, para a API. */
export function fromLocalInput(value: string) {
  if (!value) return null;
  const guess = new Date(`${value}:00Z`);
  const offset = offsetMinutes(guess);
  const sign = offset < 0 ? "-" : "+";
  const abs = Math.abs(offset);
  const hh = String(Math.floor(abs / 60)).padStart(2, "0");
  const mm = String(abs % 60).padStart(2, "0");
  return `${value}:00${sign}${hh}:${mm}`;
}

/** Dia de hoje em Manaus ("2026-10-08"). */
export function todayLocal(now = new Date()) {
  return localParts(now).date;
}

/** Soma dias a um dia "AAAA-MM-DD", sem passar por fuso. */
export function addDays(day: string, amount: number) {
  const date = new Date(`${day}T00:00:00Z`);
  date.setUTCDate(date.getUTCDate() + amount);
  return date.toISOString().slice(0, 10);
}

const dateTimeFormat = new Intl.DateTimeFormat("pt-BR", {
  timeZone: TIME_ZONE,
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
  hour: "2-digit",
  minute: "2-digit",
});

const shortDateFormat = new Intl.DateTimeFormat("pt-BR", {
  timeZone: TIME_ZONE,
  day: "2-digit",
  month: "2-digit",
  year: "numeric",
});

/** "08/10/2026, 14:30" no horário de Manaus. */
export const formatDateTime = (iso: string) => dateTimeFormat.format(new Date(iso));

/** "08/10/2026" no horário de Manaus. */
export const formatShortDate = (iso: string) => shortDateFormat.format(new Date(iso));

/** Dia sem hora ("2024-03-01") → "01/03/2024", sem passar por fuso. */
export function formatDay(day: string) {
  const [year, month, date] = day.split("-");
  return `${date}/${month}/${year}`;
}

/** Linha de data no estilo de jornal: "Manaus, quinta-feira, 8 de outubro de 2026". */
export function datelineFor(now = new Date()) {
  const text = new Intl.DateTimeFormat("pt-BR", {
    timeZone: TIME_ZONE,
    weekday: "long",
    day: "numeric",
    month: "long",
    year: "numeric",
  }).format(now);
  return `Manaus, ${text}`;
}

/** Hora cheia em Manaus, para a saudação. */
export function localHour(now = new Date()) {
  return Number(localParts(now).time.slice(0, 2));
}
