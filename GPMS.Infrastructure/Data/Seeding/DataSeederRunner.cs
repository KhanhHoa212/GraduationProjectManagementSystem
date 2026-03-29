using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Data.Seeding;

public class DataSeederRunner
{
    private readonly GpmsDbContext _context;
    private readonly IEnumerable<IDataSeeder> _seeders;

    public DataSeederRunner(GpmsDbContext context, IEnumerable<IDataSeeder> seeders)
    {
        _context = context;
        _seeders = seeders;
    }

    public async Task RunAsync()
    {
        foreach (var seeder in _seeders.OrderBy(s => s.Order))
        {
            var name = seeder.GetType().Name;
            
            // Run all seeders to restore full test data
            Console.WriteLine($"[Seeding] Starting {name}...");
            await seeder.SeedAsync();
            Console.WriteLine($"[Seeding] Finished {name}.");
        }
    }
}
