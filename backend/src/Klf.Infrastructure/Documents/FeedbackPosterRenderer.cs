using System.Globalization;

using Klf.Application.Interfaces.Documents;

using QRCoder;

using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Klf.Infrastructure.Documents;

/// <summary>
/// QR Code with QRCoder (MIT) and the poster with QuestPDF (Community license: free for companies with less than
/// US$ 1 million of annual gross revenue; review if KLF grows past it).
/// </summary>
internal sealed class FeedbackPosterRenderer : IFeedbackPosterRenderer
{
    private const string Brand = "#1F66C9";
    private const string BrandDark = "#0F2A52";
    private const string BrandSoft = "#EEF8FE";
    private const string Foreground = "#0F1B2D";
    private const string Muted = "#5B6B82";

    private static readonly byte[] DarkModule = [0x0F, 0x2A, 0x52, 0xFF];
    private static readonly byte[] LightModule = [0xFF, 0xFF, 0xFF, 0xFF];

    static FeedbackPosterRenderer()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] RenderQrCodePng(Uri url)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(url.AbsoluteUri, QRCodeGenerator.ECCLevel.Q);

        return new PngByteQRCode(data).GetGraphic(20, DarkModule, LightModule);
    }

    public byte[] RenderPosterPdf(FeedbackPoster poster)
    {
        var qrCode = RenderQrCodePng(poster.Url);
        var culture = CultureInfo.GetCultureInfo("pt-BR");
        var period = $"Respostas de {poster.OpensAt.ToString("dd/MM/yyyy 'às' HH:mm", culture)} até {poster.ClosesAt.ToString("dd/MM/yyyy 'às' HH:mm", culture)}";

        return Document.Create(document => document.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(0);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(style => style.FontSize(13).FontColor(Foreground));

            page.Header().Background(Brand).PaddingVertical(28).PaddingHorizontal(48).Column(header =>
            {
                header.Item().Text("KLF Consultoria & Treinamento").FontColor(Colors.White).FontSize(13).SemiBold();
                header.Item().PaddingTop(6).Text("Avalie o treinamento").FontColor(Colors.White).FontSize(34).Bold();
            });

            page.Content().PaddingHorizontal(48).PaddingVertical(28).Column(content =>
            {
                content.Spacing(10);
                content.Item().Text(poster.SessionTitle).FontSize(22).SemiBold();
                content.Item().Text(poster.FormTitle).FontColor(Muted);
                content.Item().PaddingTop(10).AlignCenter().Width(280).Image(qrCode);
                content.Item().AlignCenter().Text("Aponte a câmera do celular para o código").FontSize(17).SemiBold().FontColor(BrandDark);
                content.Item().AlignCenter().Text(poster.Url.AbsoluteUri).FontSize(10).FontColor(Brand);
                content.Item().AlignCenter().Text(period).FontSize(11).FontColor(Muted);

                content.Item().PaddingTop(14).Background(BrandSoft).Padding(18).Column(notice =>
                {
                    notice.Spacing(4);
                    notice.Item().Text("Sua resposta é anônima").FontSize(15).SemiBold().FontColor(BrandDark);
                    notice.Item().Text(
                        "Não pedimos nome, e-mail ou telefone e não guardamos dados do seu celular, endereço de internet nem o horário da resposta. "
                        + "Os resultados só aparecem agrupados, a partir de 3 respostas. Seja sincero: sua opinião melhora o seu dia a dia de trabalho.")
                        .FontSize(11);
                });
            });

            page.Footer().PaddingHorizontal(48).PaddingBottom(24).AlignCenter()
                .Text("KLF Consultoria & Treinamento · Obrigado por participar!").FontSize(10).FontColor(Muted);
        })).GeneratePdf();
    }
}
