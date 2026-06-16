using AllChecksOut.Cases.Api.CurrentUser;

namespace AllChecksOut.Cases.Api.Endpoints;

public static class CasesApiEndpoints
{
    public static IEndpointRouteBuilder MapCasesApiEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/", () => Results.Redirect("/health")).AllowAnonymous();
        app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

        app.MapGet("/api/me", async (ApplicationUserResolver resolver, CancellationToken cancellationToken) =>
        {
            var profile = await resolver.ResolveAsync(cancellationToken);
            return Results.Ok(profile);
        });

        return app;
    }
}
