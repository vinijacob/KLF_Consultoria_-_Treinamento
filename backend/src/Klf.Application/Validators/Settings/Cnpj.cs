namespace Klf.Application.Validators.Settings;

/// <summary>
/// CNPJ check digits, including the alphanumeric format of IN RFB 2.229/2024: the first 12 characters may be digits or
/// uppercase letters (each worth its ASCII code minus 48) and the last 2 are always digits.
/// </summary>
internal static class Cnpj
{
    private static readonly int[] FirstWeights = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
    private static readonly int[] SecondWeights = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

    public static string Normalize(string value) =>
        new([.. value.Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant)]);

    public static bool IsValid(string? value)
    {
        if (value is null || value.Any(c => !char.IsLetterOrDigit(c) && c is not ('.' or '/' or '-' or ' ')))
        {
            return false;
        }

        var cnpj = Normalize(value);

        if (cnpj.Length != 14 || !cnpj[..12].All(IsCnpjChar) || !cnpj[12..].All(char.IsAsciiDigit) || cnpj.Distinct().Count() == 1)
        {
            return false;
        }

        return cnpj[12] - '0' == CheckDigit(cnpj[..12], FirstWeights)
            && cnpj[13] - '0' == CheckDigit(cnpj[..13], SecondWeights);
    }

    public static string Format(string value)
    {
        var cnpj = Normalize(value);

        return $"{cnpj[..2]}.{cnpj[2..5]}.{cnpj[5..8]}/{cnpj[8..12]}-{cnpj[12..]}";
    }

    private static bool IsCnpjChar(char c) => char.IsAsciiDigit(c) || char.IsAsciiLetterUpper(c);

    private static int CheckDigit(string characters, int[] weights)
    {
        var sum = characters.Select((c, index) => (c - '0') * weights[index]).Sum();
        var remainder = sum % 11;

        return remainder < 2 ? 0 : 11 - remainder;
    }
}
