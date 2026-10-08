namespace Klf.Domain.Enums;

/// <summary>What a section of the feedback form is about. Results are grouped by it.</summary>
public enum FeedbackTopic
{
    /// <summary>The trainee's own company (store or client). Sensitive: employees criticize their employer.</summary>
    Company,

    /// <summary>KLF and the training itself.</summary>
    Training,
}
