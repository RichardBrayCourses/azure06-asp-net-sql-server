using AllChecksOut.FunctionsApi.Authentication;
using AllChecksOut.FunctionsApi.CurrentUser;
using AllChecksOut.FunctionsApi.Data;
using AllChecksOut.FunctionsApi.Domain;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(config =>
    {
        config.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<EntraJwtOptions>(context.Configuration.GetSection(EntraJwtOptions.SectionName));
        services.Configure<CorsOptions>(context.Configuration.GetSection(CorsOptions.SectionName));
        services.Configure<DemoSignInOptions>(context.Configuration.GetSection(DemoSignInOptions.SectionName));

        services.AddDbContext<AllChecksOutDbContext>(options =>
        {
            var connectionString = context.Configuration.GetConnectionString("AllChecksOut")
                ?? context.Configuration["ConnectionStrings__AllChecksOut"];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("ConnectionStrings:AllChecksOut is required.");
            }

            options.UseSqlServer(connectionString);
        });

        services.AddScoped<CurrentUserAccessor>();
        services.AddScoped<ApplicationUserResolver>();
        services.AddScoped<CasesDomainService>();
        services.AddSingleton<BearerTokenValidator>();
    })
    .Build();

host.Run();
