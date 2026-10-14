import type { SVGProps } from "react";

/*
 * Ícones desenhados à mão: traço único de 1,75, pontas retas, 24x24.
 * Só existem aqui os que têm função na interface.
 */
type IconProps = SVGProps<SVGSVGElement> & { title?: string };

function Icon({ title, children, ...props }: IconProps) {
  return (
    <svg
      viewBox="0 0 24 24"
      width="1em"
      height="1em"
      fill="none"
      stroke="currentColor"
      strokeWidth="1.75"
      strokeLinecap="square"
      strokeLinejoin="miter"
      role={title ? "img" : undefined}
      aria-hidden={title ? undefined : true}
      aria-label={title}
      {...props}
    >
      {children}
    </svg>
  );
}

export function CheckIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M4.5 12.5 9.5 17.5 19.5 6.5" />
    </Icon>
  );
}

export function AlertIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M12 3.5 21.5 20h-19L12 3.5Z" />
      <path d="M12 10v5M12 17.5v.5" />
    </Icon>
  );
}

export function ClockIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <circle cx="12" cy="12" r="8.5" />
      <path d="M12 7v5.5l3.5 2" />
    </Icon>
  );
}

export function LockIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <rect x="5" y="10.5" width="14" height="9.5" />
      <path d="M8 10.5V8a4 4 0 0 1 8 0v2.5" />
    </Icon>
  );
}

export function ArrowRightIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M4 12h15M13.5 6.5 19 12l-5.5 5.5" />
    </Icon>
  );
}

export function ChevronDownIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M6 9.5 12 15.5 18 9.5" />
    </Icon>
  );
}

export function ArrowUpIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M12 20V5M6.5 10.5 12 5l5.5 5.5" />
    </Icon>
  );
}

export function ArrowDownIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M12 4v15M6.5 13.5 12 19l5.5-5.5" />
    </Icon>
  );
}

export function DownloadIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M12 4v11M7 10.5l5 5 5-5M5 20h14" />
    </Icon>
  );
}

export function KeyIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <circle cx="8" cy="12" r="3.5" />
      <path d="M11.5 12H20M17 12v3.5M20 12v2.5" />
    </Icon>
  );
}

export function SearchIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <circle cx="10.5" cy="10.5" r="6.5" />
      <path d="m15.5 15.5 5 5" />
    </Icon>
  );
}

export function ArrowLeftIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M20 12H5M10.5 6.5 5 12l5.5 5.5" />
    </Icon>
  );
}

export function EyeIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M2.5 12S6 5.5 12 5.5 21.5 12 21.5 12 18 18.5 12 18.5 2.5 12 2.5 12Z" />
      <circle cx="12" cy="12" r="3" />
    </Icon>
  );
}

export function EyeOffIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M9.5 6A9.6 9.6 0 0 1 12 5.5C18 5.5 21.5 12 21.5 12a16 16 0 0 1-2.6 3.4M14.1 14.1a3 3 0 0 1-4.2-4.2M6.6 7.6C4 9.4 2.5 12 2.5 12S6 18.5 12 18.5c1.6 0 3-.4 4.3-1.1M4 4l16 16" />
    </Icon>
  );
}

export function CloseIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="m6 6 12 12M18 6 6 18" />
    </Icon>
  );
}

export function SunIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <circle cx="12" cy="12" r="4" />
      <path d="M12 2.5v2.5M12 19v2.5M2.5 12H5M19 12h2.5M5.3 5.3l1.8 1.8M16.9 16.9l1.8 1.8M5.3 18.7l1.8-1.8M16.9 7.1l1.8-1.8" />
    </Icon>
  );
}

export function MoonIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M20 14.5A8 8 0 0 1 9.5 4a8 8 0 1 0 10.5 10.5Z" />
    </Icon>
  );
}

export function AlignLeftIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M4 6h16M4 10h10M4 14h16M4 18h10" />
    </Icon>
  );
}

export function AlignCenterIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M4 6h16M7 10h10M4 14h16M7 18h10" />
    </Icon>
  );
}

export function AlignRightIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M4 6h16M10 10h10M4 14h16M10 18h10" />
    </Icon>
  );
}

export function AlignJustifyIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M4 6h16M4 10h16M4 14h16M4 18h16" />
    </Icon>
  );
}

export function ImageIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <rect x="3.5" y="5" width="17" height="14" />
      <path d="m3.5 16 5-5 4 4 3-3 4.5 4.5" />
      <circle cx="15.5" cy="9" r="1.25" />
    </Icon>
  );
}

export function IndentIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M4 5h16M10 10h10M10 14h10M4 19h16M4 9.5l3 2.5-3 2.5" />
    </Icon>
  );
}

export function OutdentIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M4 5h16M10 10h10M10 14h10M4 19h16M7 9.5 4 12l3 2.5" />
    </Icon>
  );
}
