using AllChecksOut.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AllChecksOutDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AllChecksOut");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("ConnectionStrings:AllChecksOut is required.");
    }

    options.UseSqlServer(connectionString);
});

var app = builder.Build();

app.MapGet("/", () => Results.Redirect("/health"));
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.Run();
