namespace Klf.Application.DTOs.Settings;

/// <summary>
/// Company data used by the privacy policy, terms of use and cookie policy (LGPD art. 9: who the controller is and how
/// to reach the person in charge of personal data).
/// </summary>
/// <param name="CompanyName">Legal company name (razão social), up to 200 characters.</param>
/// <param name="Cnpj">CNPJ, numeric or alphanumeric, with or without punctuation; saved as <c>00.000.000/0000-00</c>.</param>
/// <param name="Address">Registered address, up to 300 characters.</param>
/// <param name="ForumCity">City and state of the court chosen in the terms of use (e.g. <c>Manaus/AM</c>).</param>
/// <param name="DpoName">Name of the person in charge of personal data (encarregado/DPO).</param>
/// <param name="DpoEmail">E-mail where data subjects send their requests.</param>
/// <param name="UpdatedOn">Date of the last change, in Manaus time. Filled in by the server on every save; any value sent is ignored.</param>
public sealed record LegalSettings(
    string? CompanyName,
    string? Cnpj,
    string? Address,
    string? ForumCity,
    string? DpoName,
    string? DpoEmail,
    DateOnly? UpdatedOn = null);
