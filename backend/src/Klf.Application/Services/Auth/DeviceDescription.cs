namespace Klf.Application.Services.Auth;

/// <summary>Turns a <c>User-Agent</c> into a short description people recognize, such as <c>Chrome no macOS</c>.</summary>
internal static class DeviceDescription
{
    public const int MaxLength = 120;

    private static readonly (string Token, string Name)[] Browsers =
    [
        ("Edg", "Edge"),
        ("OPR/", "Opera"),
        ("SamsungBrowser", "Samsung Internet"),
        ("Firefox/", "Firefox"),
        ("FxiOS", "Firefox"),
        ("CriOS", "Chrome"),
        ("Chrome/", "Chrome"),
        ("Safari/", "Safari"),
    ];

    private static readonly (string Token, string Name)[] Systems =
    [
        ("iPhone", "iPhone"),
        ("iPad", "iPad"),
        ("Android", "Android"),
        ("Windows", "Windows"),
        ("CrOS", "ChromeOS"),
        ("Macintosh", "macOS"),
        ("Mac OS X", "macOS"),
        ("Linux", "Linux"),
    ];

    public static string FromUserAgent(string? userAgent)
    {
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return "Navegador desconhecido";
        }

        var browser = Find(Browsers, userAgent) ?? "Navegador desconhecido";
        var system = Find(Systems, userAgent);

        return system is null ? browser : $"{browser} no {system}";
    }

    private static string? Find((string Token, string Name)[] candidates, string userAgent) =>
        candidates.FirstOrDefault(candidate => userAgent.Contains(candidate.Token, StringComparison.Ordinal)).Name;
}
