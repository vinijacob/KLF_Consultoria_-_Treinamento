namespace Klf.Domain.Enums;

/// <summary>How a service is delivered to the client.</summary>
public enum ServiceFormat
{
    /// <summary>In person, at a venue.</summary>
    InPerson,

    /// <summary>Online, through video conference.</summary>
    Online,

    /// <summary>In company: delivered at the client's own premises.</summary>
    InCompany,
}
