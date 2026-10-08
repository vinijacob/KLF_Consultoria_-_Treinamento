namespace Klf.Domain.Enums;

/// <summary>Kind of question in a feedback form, which defines the kind of answer it accepts.</summary>
public enum FeedbackQuestionType
{
    /// <summary>One line of text (up to 300 characters).</summary>
    ShortText,

    /// <summary>A paragraph (up to 3000 characters).</summary>
    LongText,

    /// <summary>A number in a configurable range (e.g. 1 to 5).</summary>
    Scale,

    /// <summary>Net Promoter Score: 0 to 10, "would you recommend...".</summary>
    Nps,

    /// <summary>Exactly one of the options.</summary>
    SingleChoice,

    /// <summary>One or more of the options.</summary>
    MultipleChoice,
}
