using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace GPMS.Infrastructure.Data;

public class GpmsDbContextFactory : IDesignTimeDbContextFactory<GpmsDbContext>
{
    public GpmsDbContext CreateDbContext(string[] args)
    {
        // Get the path to the GPMS.Web project to find appsettings.json
        var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "GPMS.Web");
        
        // If the base path doesn't exist (e.g. running from a different location), use current directory
        if (!Directory.Exists(basePath))
        {
            basePath = Directory.GetCurrentDirectory();
        }

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<GpmsDbContext>();
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        
        optionsBuilder.UseSqlServer(connectionString);

        return new GpmsDbContext(optionsBuilder.Options);
    }
}
