using Klf.Application.Interfaces.Identity;
using Klf.Application.Services.Auth;
using Klf.Application.Tests.Fakes;
using Klf.Domain.Entities;

namespace Klf.Application.Tests.Services;

public sealed class LoginDeviceServiceTests
{
    private const string MacChrome =
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36";

    private static readonly Guid UserId = Guid.CreateVersion7();
    private static readonly DateTimeOffset Now = new(2026, 10, 15, 14, 30, 0, TimeSpan.Zero);

    private readonly InMemoryKnownDeviceRepository _devices = new();
    private readonly FakeEmailSender _email = new();
    private readonly MutableTimeProvider _clock = new(Now);
    private readonly FakeClientDevice _client = new() { UserAgent = MacChrome };
    private readonly LoginDeviceService _service;

    public LoginDeviceServiceTests()
    {
        _service = new LoginDeviceService(_devices, _client, _email, new FakeFrontendLinks(), _clock);
    }

    [Fact]
    public async Task First_device_of_an_account_is_registered_without_an_alert()
    {
        var result = await SignInAsync();

        Assert.Null(result.NewDevice);
        Assert.Equal(43, result.Token.Length);
        var device = Assert.Single(_devices.Devices);
        Assert.Equal("Chrome no macOS", device.Description);
        Assert.NotEqual(result.Token, device.TokenHash);
        Assert.Empty(_email.Sent);
    }

    [Fact]
    public async Task Known_device_signs_in_without_an_alert_and_keeps_its_token()
    {
        var first = await SignInAsync();
        _client.DeviceToken = first.Token;
        _clock.Now = Now.AddDays(3);

        var again = await SignInAsync();

        Assert.Null(again.NewDevice);
        Assert.Equal(first.Token, again.Token);
        Assert.Equal(Now.UtcDateTime.AddDays(3), Assert.Single(_devices.Devices).LastSeenAt);
        Assert.Empty(_email.Sent);
    }

    [Fact]
    public async Task Unknown_device_of_an_account_with_devices_sends_an_alert_with_the_device_and_local_time()
    {
        await SignInAsync();
        _client.DeviceToken = null;
        _client.UserAgent = "Mozilla/5.0 (iPhone; CPU iPhone OS 18_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.0 Mobile/15E148 Safari/604.1";

        var result = await SignInAsync();

        Assert.Equal("Safari no iPhone", result.NewDevice);
        Assert.Equal(2, _devices.Devices.Count);
        var message = Assert.Single(_email.Sent);
        Assert.Equal("ana@klf.test", message.To);
        Assert.Contains("Safari no iPhone", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("15/10/2026 às 10:30", message.TextBody, StringComparison.Ordinal);
        Assert.Contains("https://klf.test/painel/esqueci-a-senha", message.HtmlBody, StringComparison.Ordinal);
        Assert.Contains("Ana &lt;b&gt;", message.HtmlBody, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Device_known_by_another_account_keeps_the_token_but_alerts_this_account()
    {
        var other = await _service.RegisterSignInAsync(Guid.CreateVersion7(), "outra@klf.test", "Outra", TestContext.Current.CancellationToken);
        await SignInAsync();
        _client.DeviceToken = other.Token;

        var result = await SignInAsync();

        Assert.Equal(other.Token, result.Token);
        Assert.NotNull(result.NewDevice);
    }

    [Fact]
    public async Task Malformed_cookie_value_is_replaced_by_a_new_token()
    {
        _client.DeviceToken = "nao-e-um-token";

        var result = await SignInAsync();

        Assert.NotEqual("nao-e-um-token", result.Token);
        Assert.Equal(43, result.Token.Length);
    }

    [Fact]
    public async Task Purge_deletes_devices_unused_for_a_year()
    {
        _devices.Add(new KnownDevice(UserId, "velho", "Chrome", Now.UtcDateTime.AddDays(-KnownDevice.RetentionDays - 1)));
        _devices.Add(new KnownDevice(UserId, "recente", "Chrome", Now.UtcDateTime.AddDays(-10)));

        var deleted = await _service.PurgeUnusedAsync(TestContext.Current.CancellationToken);

        Assert.Equal(1, deleted);
        Assert.Equal("recente", Assert.Single(_devices.Devices).TokenHash);
    }

    [Theory]
    [InlineData(MacChrome, "Chrome no macOS")]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/141.0.0.0 Safari/537.36 Edg/141.0.0.0", "Edge no Windows")]
    [InlineData("Mozilla/5.0 (Linux; Android 14; SM-S911B) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/25.0 Chrome/121.0.0.0 Mobile Safari/537.36", "Samsung Internet no Android")]
    [InlineData("Mozilla/5.0 (X11; Linux x86_64; rv:131.0) Gecko/20100101 Firefox/131.0", "Firefox no Linux")]
    [InlineData("Mozilla/5.0 (iPad; CPU OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/141.0 Mobile/15E148 Safari/604.1", "Chrome no iPad")]
    [InlineData("curl/8.7.1", "Navegador desconhecido")]
    [InlineData(null, "Navegador desconhecido")]
    public void Description_names_browser_and_system(string? userAgent, string expected) =>
        Assert.Equal(expected, DeviceDescription.FromUserAgent(userAgent));

    private Task<DeviceSignIn> SignInAsync() =>
        _service.RegisterSignInAsync(UserId, "ana@klf.test", "Ana <b>", TestContext.Current.CancellationToken);

    private sealed class FakeClientDevice : IClientDevice
    {
        public string? DeviceToken { get; set; }

        public string? UserAgent { get; set; }
    }
}
