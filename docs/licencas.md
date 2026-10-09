# Licenças de fontes, imagens e bibliotecas

Registro exigido pelo item 9 da seção "Conformidade legal" do `CLAUDE.md`. Atualizar a cada fonte, imagem ou biblioteca nova.

## Fontes (servidas pelo próprio site via `next/font`, sem CDN de terceiros no navegador)

| Fonte | Uso | Licença | Origem |
| --- | --- | --- | --- |
| Newsreader | Títulos e texto de leitura | SIL Open Font License 1.1 (uso comercial e embutir no site permitidos) | Google Fonts (Production Type) |
| IBM Plex Sans | Interface: rótulos, botões, textos de apoio | SIL Open Font License 1.1 | Google Fonts / IBM |
| IBM Plex Mono | Legendas, números, etiquetas | SIL Open Font License 1.1 | Google Fonts / IBM |

A OFL permite uso comercial, modificação e redistribuição, desde que o nome reservado não seja usado em versões modificadas e a licença acompanhe a fonte. Não redistribuímos os arquivos separadamente.

## Imagens

Nenhuma imagem de terceiros em uso. Fotos e logos do site são enviadas pelo painel (`MediaAsset`); para cada uma, guardar aqui a origem e a licença, ou a autorização de uso de imagem das pessoas retratadas.

| Arquivo | Origem | Licença/autorização | Observações |
| --- | --- | --- | --- |
| (nenhum ainda) | | | |

## Bibliotecas relevantes

| Biblioteca | Licença | Observação |
| --- | --- | --- |
| QuestPDF (backend) | Community (grátis para empresa com faturamento anual abaixo de US$ 1 milhão) | Rever se a KLF passar do limite |
| QRCoder (backend) | MIT | |
| Next.js, React, Tailwind CSS | MIT | |
| Tiptap (`@tiptap/react`, `starter-kit`, `pm`) | MIT | Editor de texto do painel (só o núcleo aberto; nada do Tiptap Pro/Cloud) |
| qrcode (node-qrcode) | MIT | QR Code do cadastro do 2FA, gerado no navegador |
