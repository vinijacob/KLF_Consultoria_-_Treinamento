#!/usr/bin/env python3
"""
Cadastra CONTEÚDO PROVISÓRIO no banco de DESENVOLVIMENTO pela própria API, para o site público ter o que mostrar.

Tudo aqui é exemplo e deve ser substituído pelo painel. Pessoas, empresas e formações usam nomes claramente
fictícios ("exemplo") para nada inventado parecer fato. Rodar de novo é seguro: o que já existe é pulado.

Uso (com a API rodando em Development, onde o 2FA não é exigido):
    python3 scripts/seed_placeholder.py
Credenciais: variáveis KLF_EMAIL e KLF_PASSWORD, ou lidas do `dotnet user-secrets` do Klf.Api.
"""
import json
import os
import subprocess
import sys
import urllib.error
import urllib.request
from datetime import datetime, timedelta, timezone

API = os.environ.get("KLF_API", "http://localhost:5004") + "/api/v1"
ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def secret(name):
    output = subprocess.run(
        ["dotnet", "user-secrets", "list", "--project", os.path.join(ROOT, "backend/src/Klf.Api")],
        capture_output=True, text=True, check=True,
    ).stdout
    for line in output.splitlines():
        if line.startswith(name + " = "):
            return line.split(" = ", 1)[1]
    sys.exit(f"Segredo {name} não encontrado.")


def call(method, path, body=None, token=None):
    request = urllib.request.Request(API + path, method=method, data=json.dumps(body).encode() if body is not None else None)
    request.add_header("Content-Type", "application/json")
    if token:
        request.add_header("Authorization", f"Bearer {token}")
    try:
        with urllib.request.urlopen(request) as response:
            raw = response.read()
            return response.status, json.loads(raw) if raw else None
    except urllib.error.HTTPError as error:
        return error.code, json.loads(error.read() or b"null")


def login():
    email = os.environ.get("KLF_EMAIL") or secret("Seed:Admin:Email")
    password = os.environ.get("KLF_PASSWORD") or secret("Seed:Admin:Password")
    status, body = call("POST", "/auth/login", {"email": email, "password": password})
    if status != 200 or not body.get("accessToken"):
        sys.exit(f"Login falhou ({status}). A API está em Development (sem 2FA obrigatório)? {body}")
    return body["accessToken"]


def report(label, status, body):
    if status in (200, 201, 204):
        print(f"  ok        {label}")
    elif status == 409:
        print(f"  já existe {label}")
    else:
        print(f"  ERRO {status} {label}: {json.dumps(body, ensure_ascii=False)}")


SETTINGS = {
    "about": {
        "mission": "Preparar equipes de vendas e de atendimento para que cada contato com o cliente gere valor para quem compra e para quem vende.",
        "vision": "Ser referência em treinamento de atendimento e vendas, reconhecida pela elegância do método e pelos resultados das equipes.",
        "values": [
            "Respeito pelo cliente e por quem atende",
            "Elegância em cada detalhe",
            "Energia para fazer acontecer",
            "Compromisso com resultado medido",
            "Aprendizado contínuo",
        ],
        "differentials": [
            "Conteúdo adaptado à realidade de cada loja",
            "Exercícios com situações reais da equipe",
            "Acompanhamento depois do treinamento",
            "Avaliação anônima das turmas por QR Code",
        ],
        "history": (
            "Kilciene Lima Ferreira é treinadora em vendas e experiência do cliente. Para ela, atendimento bom não é sorte: "
            "é preparo, postura e hábito construído em equipe.\n\n"
            "Em treinamentos e palestras, combina elegância, energia e foco em resultado para que as pessoas saiam da sala "
            "com ferramentas práticas, prontas para o dia seguinte.\n\n"
            "(Texto provisório: substituir pela história real da Kilciene.)"
        ),
    },
    "contact": {
        "whatsapp": "5500000000000",
        "email": "contato@example.com",
        "phone": "+55 (00) 0000-0000",
        "address": "Endereço a definir",
    },
    "seo": {
        "title": "KLF Consultoria & Treinamento · Kilciene Lima Ferreira",
        "description": "Treinamentos e palestras em vendas e experiência do cliente. Atendimento que gera valor.",
    },
}

CAREER = [
    {"entryType": "Experience", "title": "Treinadora em vendas e experiência do cliente", "institution": "KLF Consultoria & Treinamento",
     "description": "Treinamentos, palestras e acompanhamento de equipes de loja. (Exemplo: ajustar datas e descrição.)",
     "startDate": "2020-01-01", "endDate": None, "displayOrder": 0},
    {"entryType": "Experience", "title": "Cargo anterior (exemplo)", "institution": "Empresa (exemplo)",
     "description": "Descrever a experiência no varejo e no atendimento.", "startDate": "2014-03-01", "endDate": "2019-12-01", "displayOrder": 1},
    {"entryType": "Education", "title": "Formação (exemplo)", "institution": "Instituição (exemplo)",
     "description": None, "startDate": "2010-02-01", "endDate": "2013-12-01", "displayOrder": 0},
    {"entryType": "Certification", "title": "Certificação (exemplo)", "institution": "Instituição (exemplo)",
     "description": None, "startDate": "2021-06-01", "endDate": "2021-06-01", "displayOrder": 0},
]

PROVISIONAL = "<p><em>Texto provisório: conteúdo a confirmar com a Kilciene.</em></p>"

SERVICES = [
    {"title": "Atendimento que gera valor", "slug": "atendimento-que-gera-valor", "format": "InCompany", "workloadHours": 8,
     "audience": "Equipes de loja e de atendimento",
     "summary": "Postura, escuta e linguagem para transformar cada atendimento em uma experiência que o cliente quer repetir.",
     "contentHtml": "<h2>Objetivo</h2><p>Preparar a equipe para receber, entender e encantar o cliente, com uma rotina de atendimento clara e elegante.</p>"
                    "<h2>O que a equipe vai praticar</h2><ul><li>Primeiro contato e postura</li><li>Escuta ativa e perguntas certas</li>"
                    "<li>Como lidar com objeções e reclamações</li><li>Pós-venda e relacionamento</li></ul>"
                    "<h2>Como funciona</h2><p>Encontros práticos, com situações reais da loja e combinados de equipe para o dia seguinte.</p>" + PROVISIONAL},
    {"title": "Vendas consultivas no varejo", "slug": "vendas-consultivas", "format": "InCompany", "workloadHours": 12,
     "audience": "Vendedores e consultores",
     "summary": "Vender ajudando o cliente a decidir: diagnóstico, apresentação de valor e fechamento sem pressão.",
     "contentHtml": "<h2>Objetivo</h2><p>Aumentar a conversão e o ticket médio com uma venda que o cliente sente como ajuda, e não como empurrão.</p>"
                    "<h2>Conteúdo</h2><ol><li>Diagnóstico da necessidade</li><li>Apresentação de valor</li><li>Objeções e negociação</li>"
                    "<li>Fechamento e venda adicional</li></ol>" + PROVISIONAL},
    {"title": "Experiência do cliente para lideranças", "slug": "experiencia-do-cliente-liderancas", "format": "InPerson", "workloadHours": 8,
     "audience": "Gerentes, supervisores e líderes de loja",
     "summary": "Como o líder sustenta um padrão de atendimento: rotina, feedback e indicadores que a equipe entende.",
     "contentHtml": "<h2>Objetivo</h2><p>Dar às lideranças ferramentas para manter o padrão de atendimento depois que o treinamento acaba.</p>"
                    "<blockquote>O padrão da loja é o que o líder tolera no dia a dia.</blockquote>" + PROVISIONAL},
    {"title": "Palestra: elegância, energia e resultado", "slug": "palestra-elegancia-energia-resultado", "format": "InPerson", "workloadHours": 2,
     "audience": "Convenções, eventos e equipes de todos os setores",
     "summary": "Uma palestra para abrir convenções e reuniões de equipe, com histórias de atendimento e ideias para aplicar no dia seguinte.",
     "contentHtml": "<h2>Para quem</h2><p>Eventos de empresas, convenções de vendas e encontros de equipe.</p>" + PROVISIONAL},
]

POSTS = [
    {"type": "Article", "title": "Cinco atitudes que fazem o cliente voltar", "slug": "cinco-atitudes-que-fazem-o-cliente-voltar",
     "summary": "Pequenos gestos de atendimento que pesam mais do que o desconto na decisão de voltar à loja.",
     "contentHtml": "<p>O cliente raramente lembra do preço exato. Lembra de como foi tratado.</p><h2>1. Receber com presença</h2>"
                    "<p>Olhar, cumprimentar e dar atenção total nos primeiros segundos.</p><h2>2. Perguntar antes de oferecer</h2>"
                    "<p>Entender o que a pessoa procura evita a venda errada.</p>" + PROVISIONAL},
    {"type": "Article", "title": "O que acontece depois do treinamento", "slug": "o-que-acontece-depois-do-treinamento",
     "summary": "Treinamento sem acompanhamento vira lembrança. Como transformar o que foi visto em sala em hábito da equipe.",
     "contentHtml": "<p>O trabalho de verdade começa no dia seguinte, no balcão.</p><ul><li>Combinados claros</li>"
                    "<li>Acompanhamento do líder</li><li>Indicadores simples</li></ul>" + PROVISIONAL},
    {"type": "News", "title": "Turmas agora avaliam o treinamento pelo celular, de forma anônima", "slug": "avaliacao-anonima-por-qr-code",
     "summary": "Ao fim de cada encontro, a turma responde por QR Code, sem identificação, sobre o treinamento e sobre a própria empresa.",
     "contentHtml": "<p>Cada turma recebe um cartaz com QR Code. As respostas são anônimas e só aparecem agrupadas, a partir de três respostas.</p>" + PROVISIONAL},
]

TESTIMONIALS = [
    {"authorName": "Pessoa participante (exemplo)", "authorRole": "Gerente de loja", "companyName": "Empresa exemplo",
     "quote": "Depoimento provisório: aqui entra o relato real de quem participou, com a autorização registrada no painel.", "displayOrder": 0},
    {"authorName": "Outra pessoa (exemplo)", "authorRole": "Vendedora", "companyName": "Empresa exemplo",
     "quote": "Segundo depoimento provisório, para mostrar como a página fica com mais de um relato.", "displayOrder": 1},
]

CLIENTS = [f"Cliente exemplo {letter}" for letter in "ABCDEF"]


def main():
    token = login()
    now = datetime.now(timezone.utc)

    print("Configurações")
    for key, value in SETTINGS.items():
        report(key, *call("PUT", f"/admin/settings/{key}", value, token))

    print("Trajetória")
    _, existing = call("GET", "/admin/career", token=token)
    titles = {entry["title"] for entry in existing or []}
    for entry in CAREER:
        if entry["title"] in titles:
            report(entry["title"], 409, None)
        else:
            report(entry["title"], *call("POST", "/admin/career", entry, token))

    print("Serviços")
    for order, service in enumerate(SERVICES):
        body = {**service, "coverId": None, "displayOrder": order, "isActive": True}
        report(service["title"], *call("POST", "/admin/services", body, token))

    print("Posts")
    for post in POSTS:
        body = {**post, "contentJson": "{}", "coverId": None, "status": "Published", "scheduledFor": None,
                "seoTitle": None, "seoDescription": None}
        report(post["title"], *call("POST", "/admin/posts", body, token))

    print("Depoimentos")
    _, existing = call("GET", "/admin/testimonials", token=token)
    names = {item["authorName"] for item in existing or []}
    for testimonial in TESTIMONIALS:
        if testimonial["authorName"] in names:
            report(testimonial["authorName"], 409, None)
            continue
        body = {**testimonial, "photoId": None, "consentGivenAt": (now - timedelta(days=1)).isoformat(),
                "consentCoversImage": False, "isPublished": True}
        report(testimonial["authorName"], *call("POST", "/admin/testimonials", body, token))

    print("Clientes")
    _, existing = call("GET", "/admin/clients", token=token)
    names = {item["name"] for item in existing or []}
    for order, name in enumerate(CLIENTS):
        if name in names:
            report(name, 409, None)
        else:
            report(name, *call("POST", "/admin/clients", {"name": name, "websiteUrl": None, "logoId": None, "displayOrder": order, "isActive": True}, token))


if __name__ == "__main__":
    main()
