"use client";

import { useRouter } from "next/navigation";
import { useEffect, useRef, useState, type MouseEvent, type PointerEvent } from "react";
import { usePrefersReducedMotion } from "@/lib/motion";

const WIDTH = 800;
const HEIGHT = 360;
const FLOOR = 300;
const PIVOT = { x: 208, y: 196 };
const MIN_ANGLE = -42;
const MAX_ANGLE = 28;
const SWEEP_CENTER = (MIN_ANGLE + MAX_ANGLE) / 2;
const SWEEP_RANGE = 33;
const REACH = 7;
const IDLE_AFTER_MS = 2500;
const POKE_MS = 2600;
const BEAM = "40,-11 720,-118 720,118 40,11";
const BOB_ORIGIN = { transformBox: "view-box", transformOrigin: "150px 302px" } as const;

const INK = "var(--stage-ink)";
const LIGHT = "var(--stage-light)";

const DOOR = { left: 690, right: 758, top: 140, bottom: FLOOR };

function angleTo(x: number, y: number) {
  return (Math.atan2(y - PIVOT.y, x - PIVOT.x) * 180) / Math.PI;
}

/** Duas casas bastam para o desenho, e o servidor e o navegador chegam ao mesmo texto (sem erro de hidratação). */
function round(value: number) {
  return value.toFixed(2);
}

function clampAngle(angle: number) {
  return Math.min(MAX_ANGLE, Math.max(MIN_ANGLE, angle));
}

type Spot = "frame" | "plaque" | "door" | "paper";

const spots: { id: Spot; angle: number; line: string }[] = [
  { id: "frame", angle: angleTo(400, 50), line: "Um quadro torto. Bonito, mas não é a página." },
  { id: "plaque", angle: angleTo(560, 105), line: "Sala 404… vazia. A página não está aqui." },
  { id: "door", angle: angleTo(724, 220), line: "Uma saída! Entre pela porta para voltar ao início." },
  { id: "paper", angle: angleTo(430, 268), line: "Uma folha no chão! Ah, é só a lista de presença." },
];

const IDLE_LINE = "Cadê essa página? Ela estava aqui agorinha…";
const POKE_LINES = ["Ei! Assim eu derrubo a lanterna.", "Estou de plantão, viu?", "Pode deixar que eu continuo procurando."];
const START_ANGLE = spots[1].angle;

/**
 * Easter egg da página 404: o vigia do site procura a página perdida com uma lanterna num palco escuro. A luz segue
 * o mouse (ou o toque) e revela o que está no escuro; uma legenda de cinema mudo comenta o que ele acha, e clicar ou
 * tocar na porta de saída leva à página inicial. Cutucar o vigia faz o boné pular. Sem mouse, a lanterna varre a
 * sala sozinha; para quem pediu menos movimento ela fica parada até ser apontada. Personagem e desenho são originais.
 */
export function LostPageStage() {
  const router = useRouter();
  const reducedMotion = usePrefersReducedMotion();
  const [angle, setAngle] = useState(START_ANGLE);
  const [pokes, setPokes] = useState(0);
  const [poking, setPoking] = useState(false);
  const angleRef = useRef(START_ANGLE);
  const targetRef = useRef(START_ANGLE);
  const lastPointerRef = useRef(Number.NEGATIVE_INFINITY);

  useEffect(() => {
    if (reducedMotion) return;
    let frame = 0;

    const tick = (now: number) => {
      if (now - lastPointerRef.current > IDLE_AFTER_MS) {
        targetRef.current = SWEEP_CENTER + SWEEP_RANGE * Math.sin(now / 2200);
      }
      const current = angleRef.current;
      const next = current + (targetRef.current - current) * 0.12;
      if (Math.abs(next - current) > 0.02) {
        angleRef.current = next;
        setAngle(next);
      }
      frame = requestAnimationFrame(tick);
    };

    frame = requestAnimationFrame(tick);
    return () => cancelAnimationFrame(frame);
  }, [reducedMotion]);

  useEffect(() => {
    if (!poking) return;
    const timer = window.setTimeout(() => setPoking(false), POKE_MS);
    return () => window.clearTimeout(timer);
  }, [poking, pokes]);

  function toStage(event: MouseEvent<SVGSVGElement>) {
    const box = event.currentTarget.getBoundingClientRect();
    return {
      x: ((event.clientX - box.left) / box.width) * WIDTH,
      y: ((event.clientY - box.top) / box.height) * HEIGHT,
    };
  }

  function aim(event: PointerEvent<SVGSVGElement>) {
    const { x, y } = toStage(event);
    const next = clampAngle(angleTo(x, y));
    lastPointerRef.current = performance.now();
    targetRef.current = next;
    if (reducedMotion) {
      angleRef.current = next;
      setAngle(next);
    }
  }

  function openDoor(event: MouseEvent<SVGSVGElement>) {
    const { x, y } = toStage(event);
    if (x >= DOOR.left && x <= DOOR.right && y >= DOOR.top && y <= DOOR.bottom) router.push("/");
  }

  function poke() {
    setPokes((count) => count + 1);
    setPoking(true);
  }

  const lit = spots.find((spot) => Math.abs(spot.angle - angle) <= REACH)?.id ?? null;
  const caption = poking ? POKE_LINES[(pokes - 1) % POKE_LINES.length] : (spots.find((spot) => spot.id === lit)?.line ?? IDLE_LINE);
  const light = `translate(${PIVOT.x} ${PIVOT.y}) rotate(${round(angle)})`;
  const gaze = round(Math.sin((angle * Math.PI) / 180) * 5);

  return (
    <figure aria-hidden className="overflow-hidden rounded-3xl bg-[var(--stage)]">
      <svg
        viewBox={`0 0 ${WIDTH} ${HEIGHT}`}
        className="block h-auto w-full touch-pan-y select-none"
        onPointerMove={aim}
        onPointerDown={aim}
        onClick={openDoor}
      >
        <rect y={FLOOR} width={WIDTH} height={HEIGHT - FLOOR} fill="var(--stage-floor)" />

        <defs>
          <clipPath id="klf-lantern">
            <polygon points={BEAM} transform={light} />
          </clipPath>
        </defs>

        <g clipPath="url(#klf-lantern)">
          <rect width={WIDTH} height={HEIGHT} fill={LIGHT} />
          <path d={`M0 ${FLOOR}H${WIDTH}`} stroke={INK} strokeWidth="3" />
          <path d={`M0 ${FLOOR + 26}H${WIDTH}`} stroke={INK} strokeWidth="1.5" strokeDasharray="56 18" />

          <g transform="rotate(-8 400 50)" fill="none" stroke={INK}>
            <path d="M386 22 400 8 414 22" strokeWidth="2" />
            <rect x="362" y="22" width="76" height="56" rx="3" strokeWidth="4" />
            <path d="M370 70 388 48l12 12 14-18 16 28" strokeWidth="3" strokeLinejoin="round" />
            <circle cx="421" cy="35" r="5" fill={INK} stroke="none" />
          </g>

          <g>
            <rect x="485" y="75" width="150" height="62" rx="6" fill="none" stroke={INK} strokeWidth="4" />
            {[
              [495, 85],
              [625, 85],
              [495, 127],
              [625, 127],
            ].map(([cx, cy]) => (
              <circle key={`${cx}-${cy}`} cx={cx} cy={cy} r="2.5" fill={INK} />
            ))}
            <text x="560" y="94" textAnchor="middle" fill={INK} className="font-mono" fontSize="11" letterSpacing="3">
              SALA
            </text>
            <text x="560" y="128" textAnchor="middle" fill={INK} className="font-serif" fontSize="34">
              404
            </text>
          </g>

          <g transform="rotate(14 430 270)" fill="none" stroke={INK}>
            <rect x="408" y="244" width="44" height="50" rx="2" strokeWidth="3" />
            <path d="M416 256h28M416 265h28M416 274h20" strokeWidth="2" />
            <path d="m416 283 4 4 8-8" strokeWidth="2" />
          </g>

          <g className={lit === "door" ? "cursor-pointer" : undefined}>
            <rect x="692" y="140" width="64" height="20" rx="3" fill={INK} />
            <text x="724" y="154.5" textAnchor="middle" fill={LIGHT} className="font-mono" fontSize="11" letterSpacing="2">
              SAÍDA
            </text>
            <rect x="690" y="168" width="68" height="132" fill={LIGHT} stroke={INK} strokeWidth="4" />
            <rect x="698" y="180" width="40" height="44" fill="none" stroke={INK} strokeWidth="2" />
            <rect x="698" y="236" width="40" height="52" fill="none" stroke={INK} strokeWidth="2" />
            <circle cx="748" cy="234" r="3.5" fill={INK} />
          </g>
        </g>

        <g onClick={poke} onPointerDown={(event) => event.stopPropagation()} className="cursor-pointer">
          <g className="motion-safe:animate-bob" style={BOB_ORIGIN}>
            <g fill="none" stroke={INK} strokeWidth="7" strokeLinecap="round">
              <path d="M138 236c-8 23 2 38-12 56" />
              <path d="M162 236c8 23-4 38 10 56" />
              <path d="M128 184c-18 12-8 32-24 42" />
              <path d="M174 185c16 15 22 13 31 11" />
            </g>
            <ellipse cx="118" cy="293" rx="18" ry="9" fill={INK} />
            <ellipse cx="182" cy="293" rx="18" ry="9" fill={INK} />
            <circle cx="102" cy="229" r="8" fill={LIGHT} stroke={INK} strokeWidth="3" />

            <path
              d="M150 166c-28 0-40 40-32 64 6 12 58 12 64 0 8-24-4-64-32-64Z"
              fill={LIGHT}
              stroke={INK}
              strokeWidth="4"
            />
            {[190, 206, 222].map((cy) => (
              <circle key={cy} cx="158" cy={cy} r="2.5" fill={INK} />
            ))}
            <path d="m150 170-14-8v16Zm0 0 14-8v16Z" fill="var(--stage-accent)" stroke={INK} strokeWidth="3" strokeLinejoin="round" />

            <g transform={`rotate(${round(angle * 0.3)} 150 162)`}>
              <circle cx="150" cy="128" r="34" fill={LIGHT} stroke={INK} strokeWidth="4" />
              <g transform={`translate(0 ${gaze})`}>
                <g className="motion-safe:animate-blink" style={{ transformBox: "fill-box", transformOrigin: "center" }}>
                  <ellipse cx="156" cy="126" rx="4.5" ry="7" fill={INK} />
                  <ellipse cx="172" cy="126" rx="4.5" ry="7" fill={INK} />
                </g>
              </g>
              <circle cx="186" cy="138" r="6.5" fill={LIGHT} stroke={INK} strokeWidth="3" />
              <path d="M163 150c5-7 17-8 26-3-6 7-19 8-26 3Z" fill={INK} stroke={INK} strokeWidth="2" strokeLinejoin="round" />
              <path d="M169 159q6 4 12 0" fill="none" stroke={INK} strokeWidth="2.5" strokeLinecap="round" />
              <g
                key={pokes}
                className={pokes > 0 ? "motion-safe:animate-hat-pop" : undefined}
                style={{ transformBox: "fill-box", transformOrigin: "center bottom" }}
              >
                <path d="M118 108c0-28 64-28 64 0Z" fill={INK} />
                <rect x="112" y="104" width="88" height="9" rx="4.5" fill={INK} />
                <circle cx="150" cy="96" r="4" fill={LIGHT} />
              </g>
            </g>
          </g>

          <g transform={light}>
            <rect x="-6" y="-7" width="34" height="14" rx="3" fill={INK} />
            <path d="M28-11 40-14v28l-12-3Z" fill={INK} />
            <rect x="39" y="-13" width="3" height="26" fill={LIGHT} />
          </g>
          <g className="motion-safe:animate-bob" style={BOB_ORIGIN}>
            <circle cx={PIVOT.x} cy={PIVOT.y} r="8" fill={LIGHT} stroke={INK} strokeWidth="3" />
          </g>
        </g>
      </svg>

      <figcaption className="p-3">
        <p className="flex min-h-16 items-center justify-center rounded-xl border border-[var(--stage-light)] px-5 py-3 text-center font-serif text-lg text-[var(--stage-light)] italic sm:text-xl">
          <span key={caption} className="motion-safe:animate-fade">
            {caption}
          </span>
        </p>
      </figcaption>
    </figure>
  );
}
