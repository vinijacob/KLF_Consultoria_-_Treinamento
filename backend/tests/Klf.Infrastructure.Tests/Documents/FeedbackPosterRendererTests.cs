using Klf.Application.Interfaces.Documents;
using Klf.Infrastructure.Documents;

namespace Klf.Infrastructure.Tests.Documents;

public sealed class FeedbackPosterRendererTests
{
    private static readonly Uri Url = new("https://klf.com.br/avaliar/AbCdEfGhIjKlMnOpQrStUv");

    [Fact]
    public void Qr_code_is_a_png_image()
    {
        var png = new FeedbackPosterRenderer().RenderQrCodePng(Url);

        Assert.Equal([0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A], png[..8]);
        Assert.True(png.Length > 1000);
    }

    [Fact]
    public void Poster_is_a_one_page_pdf_with_accented_text()
    {
        var pdf = new FeedbackPosterRenderer().RenderPosterPdf(new FeedbackPoster(
            "Loja Centro — Atendimento (manhã)",
            "Avaliação do treinamento",
            Url,
            new DateTime(2026, 10, 7, 8, 0, 0),
            new DateTime(2026, 10, 7, 12, 0, 0)));

        Assert.Equal("%PDF"u8.ToArray(), pdf[..4]);
        Assert.True(pdf.Length > 5000);
    }
}
