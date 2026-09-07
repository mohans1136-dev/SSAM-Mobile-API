using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SSAMMobileApp.Infrastructure;

/// <summary>
/// LOCAL DEVELOPMENT ONLY. When enabled, every request is treated as an
/// authenticated caller so you can exercise secured endpoints without a real
/// Microsoft Entra ID token (useful outside the AVD).
///
/// It is wired up in Program.cs ONLY when BOTH are true:
///   * the environment is Development
///   * configuration "DevAuth:Enabled" = true
/// so it can never be active in Azure.
/// </summary>
public sealed class DevAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemeName = "DevAuth";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "dev-user"),
            new Claim(ClaimTypes.Name, "Local Dev User"),
            new Claim("scp", "Mr.Read Mr.Write"),
            new Claim("roles", "Mr.Read")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), SchemeName);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
