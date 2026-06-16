namespace AllChecksOut.Cases.Api.Authentication;

public sealed class EntraJwtOptions
{
    public const string SectionName = "Authentication:Entra";

    public string? Authority { get; set; }
    public string? TenantId { get; set; }
    public string? Audience { get; set; }
    public string? ClientId { get; set; }
    public string[] ValidAudiences { get; set; } = [];

    public string? GetAuthority()
    {
        if (!string.IsNullOrWhiteSpace(Authority))
        {
            return Authority.TrimEnd('/');
        }

        return string.IsNullOrWhiteSpace(TenantId)
            ? null
            : $"https://login.microsoftonline.com/{TenantId}";
    }

    public string? GetAudience() =>
        !string.IsNullOrWhiteSpace(Audience) ? Audience : ClientId;

    public string[] GetValidAudiences()
    {
        var configured = ValidAudiences
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToArray();

        return configured.Length > 0
            ? configured
            : new[] { Audience, ClientId }
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value!.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
    }
}

public sealed class CorsOptions
{
    public const string SectionName = "Cors";

    public string[] AllowedOrigins { get; set; } = [];

    public string[] GetAllowedOrigins()
    {
        var configured = AllowedOrigins
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim().TrimEnd('/'))
            .ToArray();

        return configured.Length > 0
            ? configured
            : ["http://localhost:5173", "http://127.0.0.1:5173"];
    }
}
