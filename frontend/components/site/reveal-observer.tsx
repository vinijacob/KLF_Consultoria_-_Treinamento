"use client";

import { useLayoutEffect } from "react";

const READY = "reveal-ready";
const VISIBLE = "is-visible";
const STAGGER_MS = 90;
const MAX_STAGGER = 4;

/**
 * Só na página inicial: faz os elementos com a classe `reveal` subirem e aparecerem quando entram na tela. Só esconde depois de o JavaScript
 * rodar (classe `reveal-ready` no `<html>`), então sem JS tudo aparece normal. O que já está na tela ao abrir a página
 * aparece na hora, sem piscar. Irmãos que entram juntos (cartões de uma grade) entram em sequência, com um pequeno atraso.
 * Ao sair da página, tira a classe do `<html>`: nas outras páginas a classe `reveal` não faz nada.
 * O CSS (em `globals.css`) ignora tudo isso para quem pediu menos movimento.
 */
export function RevealObserver() {
  useLayoutEffect(() => {
    const root = document.documentElement;
    const viewport = window.innerHeight;

    const observer = new IntersectionObserver(
      (entries) => {
        const entering = entries.filter((entry) => entry.isIntersecting).map((entry) => entry.target as HTMLElement);

        entering.forEach((element) => {
          const siblings = entering.filter((other) => other.parentElement === element.parentElement);
          const order = Math.min(siblings.indexOf(element), MAX_STAGGER);
          element.style.transitionDelay = `${order * STAGGER_MS}ms`;
          element.addEventListener("transitionend", () => element.style.removeProperty("transition-delay"), { once: true });
          element.classList.add(VISIBLE);
          observer.unobserve(element);
        });
      },
      { rootMargin: "0px 0px -8% 0px", threshold: 0.12 },
    );

    const watch = (element: Element) => {
      if (element.classList.contains(VISIBLE)) return;
      const top = element.getBoundingClientRect().top;
      if (top < viewport) element.classList.add(VISIBLE);
      else observer.observe(element);
    };

    document.querySelectorAll(".reveal").forEach(watch);
    root.classList.add(READY);

    const mutations = new MutationObserver((records) => {
      records.forEach((record) =>
        record.addedNodes.forEach((node) => {
          if (!(node instanceof Element)) return;
          if (node.classList.contains("reveal")) watch(node);
          node.querySelectorAll(".reveal").forEach(watch);
        }),
      );
    });
    mutations.observe(document.body, { childList: true, subtree: true });

    return () => {
      observer.disconnect();
      mutations.disconnect();
      root.classList.remove(READY);
    };
  }, []);

  return null;
}
