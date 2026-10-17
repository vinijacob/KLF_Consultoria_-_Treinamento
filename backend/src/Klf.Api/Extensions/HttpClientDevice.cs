using Klf.Application.Interfaces.Identity;

namespace Klf.Api.Extensions;

/// <summary>The browser of the current request: the device cookie and the <c>User-Agent</c> header.</summary>
internal sealed class HttpClientDevice(IHttpContextAccessor accessor) : IClientDevice
{
    public string? DeviceToken => accessor.HttpContext is { } context ? DeviceCookie.Read(context.Request) : null;

    public string? UserAgent => accessor.HttpContext?.Request.Headers.UserAgent.ToString() is { Length: > 0 } agent ? agent : null;
}
