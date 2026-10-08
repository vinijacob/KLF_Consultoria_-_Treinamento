namespace Klf.Application.Interfaces.Documents;

/// <summary>Data printed on the QR Code poster of a session.</summary>
/// <param name="SessionTitle">Name of the session.</param>
/// <param name="FormTitle">Title of the form.</param>
/// <param name="Url">Address encoded in the QR Code and printed below it.</param>
/// <param name="OpensAt">Local opening time (America/Manaus).</param>
/// <param name="ClosesAt">Local closing time (America/Manaus).</param>
public sealed record FeedbackPoster(string SessionTitle, string FormTitle, Uri Url, DateTime OpensAt, DateTime ClosesAt);

/// <summary>Draws the QR Code of a session as an image and as a printable poster. Implemented in Infrastructure.</summary>
public interface IFeedbackPosterRenderer
{
    /// <summary>Returns a PNG with the QR Code of the address.</summary>
    /// <param name="url">Address to encode.</param>
    byte[] RenderQrCodePng(Uri url);

    /// <summary>Returns an A4 PDF with the QR Code, the session name, the period and the anonymity notice.</summary>
    /// <param name="poster">What to print.</param>
    byte[] RenderPosterPdf(FeedbackPoster poster);
}
