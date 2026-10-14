import type { NextConfig } from "next";

const apiUrl = process.env.API_URL ?? process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5004";

const nextConfig: NextConfig = {
  /** Imagens dentro dos textos apontam para `/midia/{id}` do próprio site: o texto não depende do endereço da API. */
  rewrites() {
    return [{ source: "/midia/:id([0-9a-fA-F-]{36})", destination: `${apiUrl}/api/v1/public/media/:id` }];
  },
};

export default nextConfig;
