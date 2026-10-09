import type { CareerEntryType, PostType, ServiceFormat } from "@/types/site";

export const serviceFormatLabel: Record<ServiceFormat, string> = {
  InPerson: "Presencial",
  Online: "Online",
  InCompany: "In company",
};

export const postTypeLabel: Record<PostType, string> = {
  Article: "Artigo",
  Project: "Projeto",
  News: "Notícia",
};

export const postTypePlural: Record<PostType, string> = {
  Article: "Artigos",
  Project: "Projetos",
  News: "Notícias",
};

export const careerTypeLabel: Record<CareerEntryType, string> = {
  Experience: "Experiência",
  Education: "Formação",
  Certification: "Certificações",
};

const longDate = new Intl.DateTimeFormat("pt-BR", { day: "numeric", month: "long", year: "numeric", timeZone: "America/Manaus" });

export const formatDate = (iso: string) => longDate.format(new Date(iso));

/** Datas só de dia ("2024-03-01") viram "mar. 2024" sem passar por fuso. */
export function formatMonthYear(day: string) {
  const [year, month] = day.split("-").map(Number);
  const months = ["jan.", "fev.", "mar.", "abr.", "maio", "jun.", "jul.", "ago.", "set.", "out.", "nov.", "dez."];
  return `${months[month - 1]} ${year}`;
}

export const formatWorkload = (hours: number) => (hours === 1 ? "1 hora" : `${hours} horas`);

export const twoDigits = (value: number) => String(value).padStart(2, "0");

/** Link do WhatsApp com mensagem já escrita; o número vem das configurações (só dígitos, com DDI). */
export function whatsappLink(number: string, message: string) {
  return `https://wa.me/${number.replace(/\D/g, "")}?text=${encodeURIComponent(message)}`;
}

export function phoneHref(phone: string) {
  return `tel:+${phone.replace(/\D/g, "")}`;
}
