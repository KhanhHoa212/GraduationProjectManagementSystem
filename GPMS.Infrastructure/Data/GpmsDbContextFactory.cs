using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GPMS.Infrastructure.Data;

public class GpmsDbContextFactory : IDesignTimeDbContextFactory<GpmsDbContext>
{
    public GpmsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GpmsDbContext>();
        // Using the LocalDB instance that is confirmed running
        optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=GPMS_DB;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true");

        return new GpmsDbContext(optionsBuilder.Options);
    }
}
