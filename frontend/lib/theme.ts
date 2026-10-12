/** Tema escolhido pela pessoa, guardado no navegador. Sem escolha, o site segue o tema do sistema. */
export const THEME_STORAGE_KEY = "klf-theme";

export type Theme = "light" | "dark";

/**
 * Roda no `<head>`, antes de a página aparecer, para não piscar o tema errado. Fica como texto porque vai inline
 * no HTML; qualquer erro (navegador sem armazenamento) só deixa o tema do sistema.
 */
export const themeBootScript = `try{var t=localStorage.getItem("${THEME_STORAGE_KEY}");if(t==="light"||t==="dark")document.documentElement.dataset.theme=t}catch(e){}`;
