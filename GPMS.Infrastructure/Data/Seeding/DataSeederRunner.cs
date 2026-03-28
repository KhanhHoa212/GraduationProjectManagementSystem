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
            
            // Aggressive skip for ultra-fast startup during troubleshooting
            bool isEssential = name.Contains("Schema") || name.Contains("User") || 
                              name.Contains("Semester") || name.Contains("Major") ||
                              name.Contains("Faculty") || name.Contains("Round") ||
                              name.Contains("Project") || name.Contains("Group");
            
            // Temporary block for heavy seeders to speed up login
            if (name.Contains("Assignment") || name.Contains("Submission") || 
                name.Contains("Evaluation") || name.Contains("Notification") ||
                name.Contains("WorkflowTest"))
            {
                isEssential = false;
            }
            
            if (!isEssential) 
            {
                Console.WriteLine($"[Seeding] Skipping non-essential seeder: {name}");
                continue;
            }

            Console.WriteLine($"[Seeding] Starting {name}...");
            await seeder.SeedAsync();
            Console.WriteLine($"[Seeding] Finished {name}.");
        }
    }
}
