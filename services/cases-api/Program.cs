using AllChecksOut.Cases.Api.Data;
using AllChecksOut.Cases.Api.CurrentUser;
using AllChecksOut.Cases.Api.Authentication;
using AllChecksOut.Cases.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<EntraJwtOptions>(builder.Configuration.GetSection(EntraJwtOptions.SectionName));
builder.Services.Configure<CorsOptions>(builder.Configuration.GetSection(CorsOptions.SectionName));

builder.Services.AddDbContext<AllChecksOutDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("AllChecksOut");
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("ConnectionStrings:AllChecksOut is required.");
    }

    options.UseSqlServer(connectionString);
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ApplicationUserResolver>();

var entraJwtOptions = builder.Configuration
    .GetSection(EntraJwtOptions.SectionName)
    .Get<EntraJwtOptions>() ?? new EntraJwtOptions();
var authority = entraJwtOptions.GetAuthority();
var audience = entraJwtOptions.GetAudience();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        if (!string.IsNullOrWhiteSpace(authority))
        {
            options.Authority = authority;
        }

        if (!string.IsNullOrWhiteSpace(audience))
        {
            options.Audience = audience;
        }

        var validAudiences = entraJwtOptions.GetValidAudiences();
        if (validAudiences.Length > 0)
        {
            options.TokenValidationParameters.ValidAudiences = validAudiences;
        }

        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidateLifetime = true;
        options.TokenValidationParameters.NameClaimType = "name";
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

const string frontendCorsPolicy = "Frontend";
var corsOptions = builder.Configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>() ?? new CorsOptions();
builder.Services.AddCors(options =>
{
    options.AddPolicy(frontendCorsPolicy, policy =>
    {
        policy.WithOrigins(corsOptions.GetAllowedOrigins())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors(frontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

app.MapCasesApiEndpoints();

app.Run();

public partial class Program;
