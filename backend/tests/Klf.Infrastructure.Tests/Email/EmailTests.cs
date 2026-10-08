using System.Net;
using System.Text.Json;

using Klf.Application.Interfaces.Email;
using Klf.Infrastructure.Email;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Klf.Infrastructure.Tests.Email;

public sealed class EmailTests
{
    private static readonly EmailMessage Message = new("a@klf.test", "Assunto", "<p>oi</p>", "oi");

    [Fact]
    public void Reset_link_points_to_frontend_with_escaped_email_and_token()
    {
        var links = new FrontendLinks(Options.Create(new FrontendOptions { BaseUrl = "https://klf.com.br/" }));

        var link = links.PasswordReset("a+b@klf.com.br", "tok/en+==");

        Assert.Equal("https://klf.com.br/painel/redefinir-senha?email=a%2Bb%40klf.com.br&token=tok%2Fen%2B%3D%3D", link.AbsoluteUri);
    }

    [Fact]
    public async Task Resend_transport_posts_message_with_bearer_key_to_emails_endpoint()
    {
        var handler = new RecordingHandler(HttpStatusCode.OK);
        var transport = new ResendEmailTransport(
            new HttpClient(handler) { BaseAddress = new Uri("https://api.resend.com/") },
            Options.Create(new EmailOptions { Provider = "Resend", From = "KLF <no-reply@klf.test>", ResendApiKey = "re_secret" }));

        await transport.DeliverAsync(Message, TestContext.Current.CancellationToken);

        var request = Assert.Single(handler.Requests);
        Assert.Equal("https://api.resend.com/emails", request.Uri);
        Assert.Equal("Bearer re_secret", request.Authorization);
        using var json = JsonDocument.Parse(request.Body);
        Assert.Equal("KLF <no-reply@klf.test>", json.RootElement.GetProperty("from").GetString());
        Assert.Equal("a@klf.test", json.RootElement.GetProperty("to")[0].GetString());
        Assert.Equal("<p>oi</p>", json.RootElement.GetProperty("html").GetString());
    }

    [Fact]
    public async Task Resend_transport_throws_when_provider_rejects_or_settings_are_missing()
    {
        var rejecting = new ResendEmailTransport(
            new HttpClient(new RecordingHandler(HttpStatusCode.UnprocessableEntity)) { BaseAddress = new Uri("https://api.resend.com/") },
            Options.Create(new EmailOptions { From = "x@klf.test", ResendApiKey = "k" }));
        var unconfigured = new ResendEmailTransport(new HttpClient(), Options.Create(new EmailOptions()));

        await Assert.ThrowsAsync<HttpRequestException>(() => rejecting.DeliverAsync(Message, TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidOperationException>(() => unconfigured.DeliverAsync(Message, TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Dispatcher_retries_a_failing_transport_and_stops_after_success()
    {
        var transport = new FlakyTransport(failures: 2);
        var dispatcher = CreateDispatcher(transport);

        await dispatcher.DeliverWithRetryAsync(Message, TestContext.Current.CancellationToken);

        Assert.Equal(3, transport.Attempts);
        Assert.Single(transport.Delivered);
    }

    [Fact]
    public async Task Dispatcher_gives_up_after_three_attempts_without_throwing()
    {
        var transport = new FlakyTransport(failures: 10);
        var dispatcher = CreateDispatcher(transport);

        await dispatcher.DeliverWithRetryAsync(Message, TestContext.Current.CancellationToken);

        Assert.Equal(3, transport.Attempts);
        Assert.Empty(transport.Delivered);
    }

    [Fact]
    public async Task Queued_sender_only_enqueues_the_message()
    {
        var queue = new EmailQueue();
        var sender = new QueuedEmailSender(queue);

        await sender.SendAsync(Message, TestContext.Current.CancellationToken);

        Assert.True(queue.Reader.TryRead(out var read));
        Assert.Equal(Message, read);
    }

    private static EmailDispatcher CreateDispatcher(IEmailTransport transport)
    {
        var services = new ServiceCollection().AddSingleton(transport).BuildServiceProvider();

        return new EmailDispatcher(new EmailQueue(), services.GetRequiredService<IServiceScopeFactory>(), NullLogger<EmailDispatcher>.Instance)
        {
            RetryDelay = TimeSpan.Zero,
        };
    }

    private sealed class FlakyTransport(int failures) : IEmailTransport
    {
        public int Attempts { get; private set; }

        public List<EmailMessage> Delivered { get; } = [];

        public Task DeliverAsync(EmailMessage message, CancellationToken cancellationToken)
        {
            Attempts++;

            if (Attempts <= failures)
            {
                throw new HttpRequestException("falhou");
            }

            Delivered.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingHandler(HttpStatusCode status) : HttpMessageHandler
    {
        public List<(string Uri, string? Authorization, string Body)> Requests { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add((request.RequestUri!.AbsoluteUri, request.Headers.Authorization?.ToString(), await request.Content!.ReadAsStringAsync(cancellationToken)));

            return new HttpResponseMessage(status);
        }
    }
}
