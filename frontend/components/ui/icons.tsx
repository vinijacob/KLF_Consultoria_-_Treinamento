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

export function PauseIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M8.5 5v14M15.5 5v14" />
    </Icon>
  );
}

export function PlayIcon(props: IconProps) {
  return (
    <Icon {...props}>
      <path d="M7 4.5v15L19 12 7 4.5Z" />
    </Icon>
  );
}
