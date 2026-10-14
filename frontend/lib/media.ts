import { env } from "@/lib/env";

/** Endereço público de uma imagem pelo id (a API redireciona para o arquivo no storage). */
export const mediaUrl = (id: string) => `${env.publicApiUrl}/api/v1/public/media/${id}`;

/** Caminho da imagem dentro dos textos do editor: relativo ao site, que encaminha para a API (ver `next.config.ts`). */
export const inlineMediaPath = (id: string) => `/midia/${id}`;
