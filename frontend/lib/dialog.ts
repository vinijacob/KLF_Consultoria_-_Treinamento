import { useRef, type MouseEvent } from "react";

/** Se o clique foi fora da caixa do `<dialog>`, isto é, no fundo (o `::backdrop` conta como o próprio dialog). */
function onBackdrop(event: MouseEvent<HTMLDialogElement>) {
  const box = event.currentTarget.getBoundingClientRect();
  return event.clientX < box.left || event.clientX > box.right || event.clientY < box.top || event.clientY > box.bottom;
}

/**
 * Fecha um `<dialog>` com clique no fundo, sem salvar nada. Só quando o clique começa e termina fora da caixa:
 * arrastar uma seleção de texto de dentro para fora não fecha, nem clicar na barra de rolagem da janela.
 */
export function useBackdropClose(onClose: () => void) {
  const pressedOutside = useRef(false);

  return {
    onMouseDown: (event: MouseEvent<HTMLDialogElement>) => {
      pressedOutside.current = onBackdrop(event);
    },
    onClick: (event: MouseEvent<HTMLDialogElement>) => {
      if (pressedOutside.current && onBackdrop(event)) onClose();
      pressedOutside.current = false;
    },
  };
}
