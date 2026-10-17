namespace Klf.Domain.Enums;

/// <summary>What the data subject asks for (LGPD, art. 18).</summary>
public enum DataRequestKind
{
    /// <summary>Confirmation that KLF holds data about the person, and a copy of it.</summary>
    Access,

    /// <summary>Correction of incomplete, wrong or outdated data.</summary>
    Correction,

    /// <summary>Deletion of the personal data.</summary>
    Deletion,

    /// <summary>Withdrawal of a consent (e.g. a published testimonial).</summary>
    ConsentWithdrawal,

    /// <summary>Anything else (portability, information about sharing, objection…).</summary>
    Other,
}

/// <summary>Where a data request stands.</summary>
public enum DataRequestStatus
{
    /// <summary>Waiting for KLF; the deadline counts.</summary>
    Open,

    /// <summary>Answered and done.</summary>
    Completed,

    /// <summary>Not carried out, with the reason recorded (e.g. identity not confirmed, legal obligation to keep).</summary>
    Declined,
}
