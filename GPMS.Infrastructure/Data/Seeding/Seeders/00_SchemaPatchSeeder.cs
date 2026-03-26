using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Data.Seeding.Seeders;

public class SchemaPatchSeeder : IDataSeeder
{
    private readonly GpmsDbContext _context;

    public SchemaPatchSeeder(GpmsDbContext context)
    {
        _context = context;
    }

    public int Order => 0;

    public async Task SeedAsync()
    {
        await EnsureColumnAsync("EvaluationDetails", "GradeDescription", "NVARCHAR(MAX) NULL");
        await EnsureColumnAsync("GroupMembers", "Status", "NVARCHAR(20) NOT NULL DEFAULT 'InProgress'");
    }

    private async Task EnsureColumnAsync(string tableName, string columnName, string sqlDefinition)
    {
        var sql =
            $"""
              IF COL_LENGTH('{tableName}', '{columnName}') IS NULL
              BEGIN
                  ALTER TABLE [{tableName}] ADD [{columnName}] {sqlDefinition};
              END
              """;
        await _context.Database.ExecuteSqlRawAsync(sql);
    }
}
