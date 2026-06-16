using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AllChecksOut.Cases.Api.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AllChecksOutDbContext>
{
    public AllChecksOutDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AllChecksOutDbContext>()
            .UseSqlServer("Server=localhost,1433;Database=AllChecksOut;User Id=sa;Password=AllChecksOut_2026!;TrustServerCertificate=True;Encrypt=False")
            .Options;

        return new AllChecksOutDbContext(options);
    }
}
