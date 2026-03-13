namespace GPMS.Infrastructure.Data.Seeding;

public class DataSeederRunner
{
    private readonly IEnumerable<IDataSeeder> _seeders;

    public DataSeederRunner(IEnumerable<IDataSeeder> seeders)
        => _seeders = seeders;

    public async Task RunAsync()
    {
        foreach (var seeder in _seeders.OrderBy(s => s.Order))
            await seeder.SeedAsync();
    }
}
