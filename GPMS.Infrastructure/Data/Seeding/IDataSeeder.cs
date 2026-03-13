namespace GPMS.Infrastructure.Data.Seeding;

public interface IDataSeeder
{
    int Order { get; }  // Số nhỏ chạy trước (FK dependency order)
    Task SeedAsync();
}
