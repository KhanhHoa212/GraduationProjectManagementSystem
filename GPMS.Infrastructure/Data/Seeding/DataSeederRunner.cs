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
            try 
            {
                var startTime = DateTime.UtcNow;
                Console.WriteLine($"[Seeding] Starting {name}...");
                await seeder.SeedAsync();
                var duration = DateTime.UtcNow - startTime;
                Console.WriteLine($"[Seeding] Finished {name} in {duration.TotalSeconds:F2}s.");
            }
            catch (Exception ex)
            {
                var innerMsg = ex.InnerException != null ? $" -> {ex.InnerException.Message}" : "";
                Console.WriteLine($"[ERROR] Seeding failed for {name}: {ex.Message}{innerMsg}");
                // We continue to the next seeder so one failure doesn't block the rest
            }
        }
    }
}
