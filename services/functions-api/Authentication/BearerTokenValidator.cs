using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace AllChecksOut.FunctionsApi.Authentication;

public sealed class BearerTokenValidator(IOptions<EntraJwtOptions> entraOptions)
{
    private readonly EntraJwtOptions options = entraOptions.Value;
    private readonly JwtSecurityTokenHandler tokenHandler = new();
    private readonly Lazy<ConfigurationManager<OpenIdConnectConfiguration>?> configurationManager =
        new(() =>
        {
            var authority = entraOptions.Value.GetAuthority();
            return string.IsNullOrWhiteSpace(authority)
                ? null
                : new ConfigurationManager<OpenIdConnectConfiguration>(
                    $"{authority}/.well-known/openid-configuration",
                    new OpenIdConnectConfigurationRetriever());
        });

    public async Task<ClaimsPrincipal?> ValidateAsync(string? authorizationHeader, CancellationToken cancellationToken)
    {
        var token = ReadBearerToken(authorizationHeader);
        if (token is null)
        {
            return null;
        }

        var manager = configurationManager.Value;
        var configuration = manager is null
            ? null
            : await manager.GetConfigurationAsync(cancellationToken);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = configuration?.Issuer,
            ValidateAudience = true,
            ValidAudiences = options.GetValidAudiences(),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKeys = configuration?.SigningKeys ?? [],
            NameClaimType = "name",
        };

        var result = await tokenHandler.ValidateTokenAsync(token, validationParameters);
        return result.IsValid ? new ClaimsPrincipal(result.ClaimsIdentity) : null;
    }

    private static string? ReadBearerToken(string? authorizationHeader)
    {
        if (string.IsNullOrWhiteSpace(authorizationHeader))
        {
            return null;
        }

        const string prefix = "Bearer ";
        return authorizationHeader.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? authorizationHeader[prefix.Length..].Trim()
            : null;
    }
}
