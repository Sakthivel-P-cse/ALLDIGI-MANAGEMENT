using System.Security.Claims;

namespace SnipeIt.Api.Common;

/// <summary>
/// Ambient access to the authenticated caller, resolved from the JWT on the current
/// HTTP request. Centralizes the claim names ("id", role) so controllers/services
/// don't each re-parse <see cref="ClaimsPrincipal"/>.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>Authenticated user id, or null when unauthenticated.</summary>
    int? UserId { get; }

    /// <summary>Username (JWT name claim), or null.</summary>
    string? Username { get; }

    /// <summary>True when the caller holds the "admin" role.</summary>
    bool IsAdmin { get; }

    /// <summary>Best-effort client IP address for audit records.</summary>
    string? RemoteIp { get; }

    /// <summary>Client user-agent string for audit records.</summary>
    string? UserAgent { get; }
}

/// <inheritdoc />
public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _accessor;

    public CurrentUserService(IHttpContextAccessor accessor)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    private ClaimsPrincipal? Principal => _accessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var raw = Principal?.FindFirst("id")?.Value;
            return int.TryParse(raw, out var id) ? id : null;
        }
    }

    public string? Username => Principal?.Identity?.Name;

    public bool IsAdmin => Principal?.IsInRole("admin") ?? false;

    public string? RemoteIp =>
        _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public string? UserAgent
    {
        get
        {
            var ctx = _accessor.HttpContext;
            if (ctx is null) return null;
            return ctx.Request.Headers.TryGetValue("User-Agent", out var ua)
                ? ua.ToString()
                : null;
        }
    }
}
