# KLF Consultoria & Treinamento

> 🇧🇷 [Português](#-português) · 🇺🇸 [English](#-english)

---

## 🇧🇷 Português

Plataforma web da **KLF Consultoria & Treinamento**, empresa de Kilciene Lima Ferreira. O site reúne:

- portfólio profissional e trajetória da Kilciene;
- blog de projetos e conteúdos;
- vitrine de serviços;
- feed do Instagram profissional;
- ferramenta de **avaliações anônimas por QR Code** para os participantes dos treinamentos.

### ✨ Funcionalidades

| Área                                  | O que faz                                                                                                                                                |
| ------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Site público**                      | Início com feed do Instagram, Sobre, Trajetória, Serviços, Projetos, Blog, Galeria, Depoimentos e Contato (com WhatsApp)                                 |
| **Avaliação anônima**                 | O treinando lê o QR Code da turma e responde pelo celular, sem login e sem dados pessoais                                                                |
| **Painel administrativo** (`/painel`) | Login com 2FA. Gerencia conteúdo e imagens, cria sessões de avaliação, gera QR Codes (PNG/PDF), mostra resultados (médias, NPS) e exporta para Excel/PDF |

### 🧱 Tecnologias

| Camada             | Stack                                                                                                                                                                        |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Frontend**       | Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4, shadcn/ui, TanStack Query, React Hook Form + Zod, Tiptap, Recharts                                           |
| **Backend**        | .NET 10 (LTS), ASP.NET Core Web API, Clean Architecture, EF Core 10 + Npgsql, ASP.NET Core Identity (2FA), FluentValidation, Hangfire, QRCoder, QuestPDF, ClosedXML, Serilog |
| **Banco de dados** | PostgreSQL 17                                                                                                                                                                |
| **Imagens**        | Cloudflare R2 (compatível com S3)                                                                                                                                            |
| **E-mail**         | Resend                                                                                                                                                                       |
| **Anti-robô**      | Cloudflare Turnstile                                                                                                                                                         |
| **Hospedagem**     | Vercel (frontend) · Railway (backend) · Neon (PostgreSQL)                                                                                                                    |
| **Testes**         | xUnit, Testcontainers, Vitest, Testing Library, Playwright                                                                                                                   |
| **CI/CD**          | GitHub Actions                                                                                                                                                               |

### 📁 Estrutura

```text
.
├── backend/                         # API ASP.NET Core (.NET 10)
│   ├── src/
│   │   ├── Klf.Domain/              # Entidades e regras de negócio (sem dependências)
│   │   ├── Klf.Application/         # Casos de uso, DTOs e validações
│   │   ├── Klf.Infrastructure/      # EF Core, R2, Instagram, e-mail, jobs
│   │   └── Klf.Api/                 # Minimal APIs (/api/v1), autenticação e OpenAPI
│   ├── tests/                       # xUnit v3 (Microsoft.Testing.Platform)
│   ├── Directory.Build.props        # Configurações comuns (warnings como erro)
│   ├── Directory.Packages.props     # Versões centralizadas dos pacotes NuGet
│   └── Klf.slnx
├── frontend/                        # Next.js 16
│   ├── app/
│   │   ├── (site)/                  # Páginas públicas
│   │   ├── avaliar/[codigo]/        # Formulário anônimo (QR Code)
│   │   └── painel/                  # Área administrativa
│   ├── components/
│   │   ├── layout/                  # Cabeçalho, rodapé e estruturas de página
│   │   └── ui/                      # Componentes base (shadcn/ui)
│   ├── config/                      # Configurações do site (nome, URLs)
│   ├── lib/                         # Cliente da API, env e utilitários
│   └── types/                       # Tipos compartilhados
└── README.md
```

### ✅ Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 22 ou superior (LTS)
- [PostgreSQL 17](https://www.postgresql.org/) local (`brew install postgresql@17`) — o pgAdmin é opcional, só para visualizar
- Ferramenta de migrations: `dotnet tool install --global dotnet-ef`

### 🚀 Como rodar localmente

**1. Clone o repositório**

```bash
git clone https://github.com/<seu-usuario>/klf-consultoria.git
cd klf-consultoria
```

**2. Suba o banco de dados e configure a conexão**

```bash
brew services start postgresql@17
psql -d postgres -c "CREATE ROLE klf WITH LOGIN PASSWORD '<sua-senha>' CREATEDB;"
psql -d postgres -c "CREATE DATABASE klf OWNER klf;"

cd backend
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=klf;Username=klf;Password=<sua-senha>" --project src/Klf.Api
```

**3. Rode o backend**

```bash
cd backend
dotnet restore
dotnet ef database update --project src/Klf.Infrastructure --startup-project src/Klf.Api
dotnet run --project src/Klf.Api
```

- API: `http://localhost:5004`
- Documentação: `http://localhost:5004/scalar`

**4. Rode o frontend** (em outro terminal)

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```

Site: `http://localhost:3000`

### 🔐 Variáveis de ambiente

**Backend** — use `dotnet user-secrets` em desenvolvimento ou variáveis de ambiente em produção:

| Variável                     | Descrição                          |
| ---------------------------- | ---------------------------------- |
| `ConnectionStrings__Default` | String de conexão do PostgreSQL    |
| `Cors__AllowedOrigins`       | URL(s) do frontend                 |
| `R2__AccountId`              | Conta do Cloudflare R2             |
| `R2__AccessKeyId`            | Chave de acesso do R2              |
| `R2__SecretAccessKey`        | Chave secreta do R2                |
| `R2__Bucket`                 | Nome do bucket                     |
| `R2__PublicUrl`              | URL pública das imagens            |
| `Instagram__AppId`           | ID do app na Meta                  |
| `Instagram__AppSecret`       | Segredo do app na Meta             |
| `Resend__ApiKey`             | Chave do serviço de e-mail         |
| `Resend__From`               | Remetente dos e-mails              |
| `Turnstile__SecretKey`       | Chave secreta do anti-robô         |
| `Frontend__RevalidateUrl`    | Endpoint de revalidação do Next.js |
| `Frontend__RevalidateSecret` | Segredo da revalidação             |

**Frontend** — arquivo `.env.local`:

| Variável                         | Descrição                                    |
| -------------------------------- | -------------------------------------------- |
| `API_URL`                        | URL da API usada no servidor (SSR)           |
| `NEXT_PUBLIC_API_URL`            | URL da API usada no navegador                |
| `NEXT_PUBLIC_SITE_URL`           | URL pública do site                          |
| `NEXT_PUBLIC_TURNSTILE_SITE_KEY` | Chave pública do anti-robô                   |
| `NEXT_PUBLIC_GA_ID`              | Google Analytics (opcional)                  |
| `REVALIDATE_SECRET`              | Segredo da revalidação (igual ao do backend) |

> ⚠️ Nunca faça commit de arquivos `.env` ou segredos.

### 🧪 Testes

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend
npm run lint
npm run test        # testes unitários (Vitest)
npm run test:e2e    # testes ponta a ponta (Playwright)
```

### ☁️ Deploy

| Parte    | Serviço                      | Como                                                                                                |
| -------- | ---------------------------- | --------------------------------------------------------------------------------------------------- |
| Frontend | Vercel (Pro)                 | Importar o repositório com **Root Directory** = `frontend`. Deploy automático a cada push na `main` |
| Backend  | Railway                      | Serviço apontando para `backend/` (Dockerfile). Migrations rodam no pipeline antes do deploy        |
| Banco    | Neon                         | Copiar a connection string para `ConnectionStrings__Default` no Railway                             |
| Domínio  | Registro.br + DNS Cloudflare | `dominio` → Vercel, `api.dominio` → Railway                                                         |

### 🛡️ Privacidade

As avaliações são **anônimas por design**. O sistema não grava IP, user-agent, localização nem horário exato das respostas. O projeto segue a LGPD (Lei 13.709/2018).

### 🤝 Convenções

- Commits no padrão [Conventional Commits](https://www.conventionalcommits.org/pt-br/) (`feat:`, `fix:`, `docs:`…)
- Branches: `main` (produção), `develop` (homologação), `feat/<nome>`
- Formatação automática: Prettier/ESLint no frontend e `dotnet format` no backend

### 📄 Licença

Projeto privado. © KLF Consultoria & Treinamento. Todos os direitos reservados.

---

## 🇺🇸 English

Web platform for **KLF Consultoria & Treinamento** (KLF Consulting & Training), the company of Kilciene Ferreira Lima. The site brings together:

- a professional portfolio and Kilciene's career history;
- a blog for projects and articles;
- a showcase of services;
- a feed of the professional Instagram account;
- an **anonymous QR Code feedback** tool for training participants.

### ✨ Features

| Area                        | What it does                                                                                                                                           |
| --------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **Public site**             | Home with Instagram feed, About, Career, Services, Projects, Blog, Gallery, Testimonials and Contact (with WhatsApp)                                   |
| **Anonymous feedback**      | Participants scan the class QR Code and answer on their phone, with no login and no personal data                                                      |
| **Admin panel** (`/painel`) | 2FA login. Manages content and images, creates feedback sessions, generates QR Codes (PNG/PDF), shows results (averages, NPS) and exports to Excel/PDF |

### 🧱 Tech stack

| Layer              | Stack                                                                                                                                                                        |
| ------------------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **Frontend**       | Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4, shadcn/ui, TanStack Query, React Hook Form + Zod, Tiptap, Recharts                                           |
| **Backend**        | .NET 10 (LTS), ASP.NET Core Web API, Clean Architecture, EF Core 10 + Npgsql, ASP.NET Core Identity (2FA), FluentValidation, Hangfire, QRCoder, QuestPDF, ClosedXML, Serilog |
| **Database**       | PostgreSQL 17                                                                                                                                                                |
| **Images**         | Cloudflare R2 (S3-compatible)                                                                                                                                                |
| **Email**          | Resend                                                                                                                                                                       |
| **Bot protection** | Cloudflare Turnstile                                                                                                                                                         |
| **Hosting**        | Vercel (frontend) · Railway (backend) · Neon (PostgreSQL)                                                                                                                    |
| **Testing**        | xUnit, Testcontainers, Vitest, Testing Library, Playwright                                                                                                                   |
| **CI/CD**          | GitHub Actions                                                                                                                                                               |

### 📁 Structure

```text
.
├── backend/                         # ASP.NET Core API (.NET 10)
│   ├── src/
│   │   ├── Klf.Domain/              # Entities and business rules (no dependencies)
│   │   ├── Klf.Application/         # Use cases, DTOs and validation
│   │   ├── Klf.Infrastructure/      # EF Core, R2, Instagram, email, jobs
│   │   └── Klf.Api/                 # Minimal APIs (/api/v1), auth and OpenAPI
│   ├── tests/                       # xUnit v3 (Microsoft.Testing.Platform)
│   ├── Directory.Build.props        # Shared settings (warnings as errors)
│   ├── Directory.Packages.props     # Central NuGet package versions
│   └── Klf.slnx
├── frontend/                        # Next.js 16
│   ├── app/
│   │   ├── (site)/                  # Public pages
│   │   ├── avaliar/[codigo]/        # Anonymous form (QR Code)
│   │   └── painel/                  # Admin area
│   ├── components/
│   │   ├── layout/                  # Header, footer and page shells
│   │   └── ui/                      # Base components (shadcn/ui)
│   ├── config/                      # Site settings (name, URLs)
│   ├── lib/                         # API client, env and helpers
│   └── types/                       # Shared types
└── README.md
```

### ✅ Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) 22 or later (LTS)
- Local [PostgreSQL 17](https://www.postgresql.org/) (`brew install postgresql@17`) — pgAdmin is optional, just for browsing
- Migrations tool: `dotnet tool install --global dotnet-ef`

### 🚀 Running locally

**1. Clone the repository**

```bash
git clone https://github.com/<your-user>/klf-consultoria.git
cd klf-consultoria
```

**2. Start the database and configure the connection**

```bash
brew services start postgresql@17
psql -d postgres -c "CREATE ROLE klf WITH LOGIN PASSWORD '<your-password>' CREATEDB;"
psql -d postgres -c "CREATE DATABASE klf OWNER klf;"

cd backend
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=klf;Username=klf;Password=<your-password>" --project src/Klf.Api
```

**3. Run the backend**

```bash
cd backend
dotnet restore
dotnet ef database update --project src/Klf.Infrastructure --startup-project src/Klf.Api
dotnet run --project src/Klf.Api
```

- API: `http://localhost:5004`
- Docs: `http://localhost:5004/scalar`

**4. Run the frontend** (in another terminal)

```bash
cd frontend
cp .env.example .env.local
npm install
npm run dev
```

Site: `http://localhost:3000`

### 🔐 Environment variables

**Backend** — use `dotnet user-secrets` in development or environment variables in production:

| Variable                     | Description                   |
| ---------------------------- | ----------------------------- |
| `ConnectionStrings__Default` | PostgreSQL connection string  |
| `Cors__AllowedOrigins`       | Frontend URL(s)               |
| `R2__AccountId`              | Cloudflare R2 account         |
| `R2__AccessKeyId`            | R2 access key                 |
| `R2__SecretAccessKey`        | R2 secret key                 |
| `R2__Bucket`                 | Bucket name                   |
| `R2__PublicUrl`              | Public URL for images         |
| `Instagram__AppId`           | Meta app ID                   |
| `Instagram__AppSecret`       | Meta app secret               |
| `Resend__ApiKey`             | Email service key             |
| `Resend__From`               | Email sender                  |
| `Turnstile__SecretKey`       | Bot protection secret key     |
| `Frontend__RevalidateUrl`    | Next.js revalidation endpoint |
| `Frontend__RevalidateSecret` | Revalidation secret           |

**Frontend** — `.env.local` file:

| Variable                         | Description                           |
| -------------------------------- | ------------------------------------- |
| `API_URL`                        | API URL used on the server (SSR)      |
| `NEXT_PUBLIC_API_URL`            | API URL used in the browser           |
| `NEXT_PUBLIC_SITE_URL`           | Public site URL                       |
| `NEXT_PUBLIC_TURNSTILE_SITE_KEY` | Bot protection public key             |
| `NEXT_PUBLIC_GA_ID`              | Google Analytics (optional)           |
| `REVALIDATE_SECRET`              | Revalidation secret (same as backend) |

> ⚠️ Never commit `.env` files or secrets.

### 🧪 Tests

```bash
# Backend
cd backend && dotnet test

# Frontend
cd frontend
npm run lint
npm run test        # unit tests (Vitest)
npm run test:e2e    # end-to-end tests (Playwright)
```

### ☁️ Deployment

| Part     | Service                      | How                                                                                        |
| -------- | ---------------------------- | ------------------------------------------------------------------------------------------ |
| Frontend | Vercel (Pro)                 | Import the repo with **Root Directory** = `frontend`. Auto-deploys on every push to `main` |
| Backend  | Railway                      | Service pointing to `backend/` (Dockerfile). Migrations run in the pipeline before deploy  |
| Database | Neon                         | Copy the connection string to `ConnectionStrings__Default` on Railway                      |
| Domain   | Registro.br + Cloudflare DNS | `domain` → Vercel, `api.domain` → Railway                                                  |

### 🛡️ Privacy

Feedback is **anonymous by design**. The system does not store IP address, user agent, location or exact time of responses. The project complies with Brazil's LGPD (Law 13.709/2018).

### 🤝 Conventions

- Commits follow [Conventional Commits](https://www.conventionalcommits.org/) (`feat:`, `fix:`, `docs:`…)
- Branches: `main` (production), `develop` (staging), `feat/<name>`
- Auto-formatting: Prettier/ESLint on the frontend and `dotnet format` on the backend

### 📄 License

Private project. © KLF Consultoria & Treinamento. All rights reserved.
