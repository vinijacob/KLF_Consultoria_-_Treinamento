"use client";

import QRCode from "qrcode";
import { useMemo } from "react";

const QUIET_ZONE = 4;

/**
 * QR Code desenhado como SVG no próprio navegador: o endereço (que no 2FA contém o segredo) nunca sai da página.
 * Cores fixas escuro-sobre-claro nos dois modos, porque leitores de câmera falham com o QR invertido.
 */
export function QrCode({ value, label, className }: { value: string; label: string; className?: string }) {
  const { size, path } = useMemo(() => {
    const { modules } = QRCode.create(value, { errorCorrectionLevel: "M" });
    const commands: string[] = [];

    for (let row = 0; row < modules.size; row++) {
      for (let col = 0; col < modules.size; col++) {
        if (modules.get(row, col)) commands.push(`M${col + QUIET_ZONE} ${row + QUIET_ZONE}h1v1h-1z`);
      }
    }

    return { size: modules.size + QUIET_ZONE * 2, path: commands.join("") };
  }, [value]);

  return (
    <svg viewBox={`0 0 ${size} ${size}`} role="img" aria-label={label} shapeRendering="crispEdges" className={className}>
      <rect width={size} height={size} fill="var(--qr-paper)" />
      <path d={path} fill="var(--qr-ink)" />
    </svg>
  );
}
