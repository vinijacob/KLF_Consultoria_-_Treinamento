using Klf.Domain.Entities;
using Klf.Domain.Enums;

namespace Klf.Domain.Tests.Entities;

public sealed class ContactMessageTests
{
    private static readonly DateTime Received = new(2026, 10, 12, 15, 0, 0, DateTimeKind.Utc);

    private static ContactMessage Message() =>
        new("Ana", "ana@loja.com.br", null, "Loja Ana", null, null, "Quero um orçamento.", "2026-10-12", Received);

    [Fact]
    public void New_message_starts_unread_with_the_received_time()
    {
        var message = Message();

        Assert.Equal(ContactMessageStatus.New, message.Status);
        Assert.Equal(Received, message.ReceivedAt);
        Assert.Null(message.ReadAt);
        Assert.Null(message.AnsweredAt);
    }

    [Fact]
    public void Answering_also_marks_as_read_and_keeps_the_first_read_time()
    {
        var message = Message();
        var read = Received.AddHours(1);
        var answered = Received.AddHours(3);

        message.ChangeStatus(ContactMessageStatus.Read, read);
        message.ChangeStatus(ContactMessageStatus.Answered, answered);

        Assert.Equal(ContactMessageStatus.Answered, message.Status);
        Assert.Equal(read, message.ReadAt);
        Assert.Equal(answered, message.AnsweredAt);
    }

    [Fact]
    public void Going_back_from_answered_clears_the_answer_time()
    {
        var message = Message();
        message.ChangeStatus(ContactMessageStatus.Answered, Received.AddHours(1));

        message.ChangeStatus(ContactMessageStatus.New, Received.AddHours(2));

        Assert.Equal(ContactMessageStatus.New, message.Status);
        Assert.Null(message.AnsweredAt);
    }

    [Fact]
    public void Message_expires_two_years_after_the_last_contact()
    {
        var unanswered = Message();
        var answered = Message();
        answered.ChangeStatus(ContactMessageStatus.Answered, Received.AddYears(1));
        var twoYearsLater = Received.AddYears(2).AddDays(1);

        Assert.True(unanswered.IsExpired(twoYearsLater));
        Assert.False(answered.IsExpired(twoYearsLater));
        Assert.True(answered.IsExpired(Received.AddYears(3).AddDays(1)));
    }
}
