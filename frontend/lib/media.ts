import { env } from "@/lib/env";

/** Endereço público de uma imagem pelo id (a API redireciona para o arquivo no storage). */
export const mediaUrl = (id: string) => `${env.publicApiUrl}/api/v1/public/media/${id}`;
