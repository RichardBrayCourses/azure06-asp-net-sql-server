using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AllChecksOut.Cases.Api.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AllChecksOutDbContext>
{
    public AllChecksOutDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable("AZURE_SQL_CONNECTION_STRING")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__AllChecksOut")
            ?? "Server=tcp:placeholder.database.windows.net,1433;Initial Catalog=AllChecksOut;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;";

        var options = new DbContextOptionsBuilder<AllChecksOutDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AllChecksOutDbContext(options);
    }
}
