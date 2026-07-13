namespace SnipeIt.Api.Common;

/// <summary>
/// Extension seam for external identity providers (LDAP / SAML / OIDC). This is a
/// placeholder architecture per the migration decision "local auth now, extensible
/// interfaces for later": local authentication ships today, and a real directory
/// integration can be dropped in by registering an implementation of this interface
/// without touching the login endpoint's contract.
/// </summary>
public interface IExternalAuthProvider
{
    /// <summary>Stable provider key, e.g. "ldap", "saml", "google".</summary>
    string ProviderName { get; }

    /// <summary>Whether this provider is currently enabled via configuration.</summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Attempts to authenticate the given credentials against the external directory.
    /// Returns a normalized result the login flow can use to find/provision a local user.
    /// </summary>
    Task<ExternalAuthResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default);
}

/// <summary>Normalized outcome of an external authentication attempt.</summary>
public sealed class ExternalAuthResult
{
    public bool Succeeded { get; init; }
    public string? Username { get; init; }
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
    public string? FailureReason { get; init; }

    public static ExternalAuthResult Fail(string reason) =>
        new() { Succeeded = false, FailureReason = reason };

    public static ExternalAuthResult Success(string username, string? email = null, string? displayName = null) =>
        new() { Succeeded = true, Username = username, Email = email, DisplayName = displayName };
}

/// <summary>
/// Default no-op provider so the DI graph and login flow are complete before any real
/// directory is wired. Always disabled; authentication falls through to local BCrypt.
/// </summary>
public sealed class NullExternalAuthProvider : IExternalAuthProvider
{
    public string ProviderName => "none";
    public bool IsEnabled => false;

    public Task<ExternalAuthResult> AuthenticateAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(ExternalAuthResult.Fail("No external auth provider configured"));
}
