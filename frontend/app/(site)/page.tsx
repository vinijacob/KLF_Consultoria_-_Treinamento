const pillars = ["Consultoria", "Treinamentos", "Avaliações"];

export default function HomePage() {
  return (
    <section className="mx-auto flex w-full max-w-4xl flex-1 flex-col items-center justify-center px-6 py-16 text-center">
      <span className="mb-8 inline-flex items-center gap-2 rounded-full border border-border bg-surface px-4 py-1.5 text-xs font-medium tracking-wide text-muted uppercase backdrop-blur">
        <span className="size-1.5 animate-pulse rounded-full bg-accent" />
        Site em construção
      </span>

      <h1 className="font-display text-7xl leading-none font-semibold tracking-tight text-brand sm:text-9xl">
        KLF
      </h1>
      <p className="mt-4 font-display text-2xl text-foreground italic sm:text-4xl">
        Consultoria <span className="text-accent">&amp;</span> Treinamento
      </p>

      <div className="my-10 h-px w-24 bg-linear-to-r from-transparent via-accent to-transparent" />

      <p className="max-w-xl text-base leading-relaxed text-muted sm:text-lg">
        Desenvolvendo pessoas e fortalecendo equipes. Em breve, um novo espaço
        para conhecer nossos serviços, projetos e conteúdos.
      </p>

      <ul className="mt-10 flex flex-wrap justify-center gap-3">
        {pillars.map((pillar) => (
          <li
            key={pillar}
            className="rounded-full border border-border bg-surface px-5 py-2 text-sm font-medium text-foreground backdrop-blur hover:bg-primary hover:text-white hover:-translate-y-0.5 hover:shadow-md hover:cursor-pointer"
          >
            {pillar}
          </li>
        ))}
      </ul>
    </section>
  );
}
