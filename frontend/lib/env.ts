export const env = {
  apiUrl: process.env.API_URL ?? process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5004",
  publicApiUrl: process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5004",
} as const;
