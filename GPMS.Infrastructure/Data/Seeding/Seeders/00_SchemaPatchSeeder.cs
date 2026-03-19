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
        await _context.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[MentorRoundReviews]', N'U') IS NULL
            BEGIN
                CREATE TABLE [MentorRoundReviews](
                    [ReviewRoundID] INT NOT NULL,
                    [GroupID] INT NOT NULL,
                    [SupervisorID] NVARCHAR(20) NOT NULL,
                    [DecisionStatus] INT NOT NULL CONSTRAINT [DF_MentorRoundReviews_DecisionStatus] DEFAULT 0,
                    [ProgressComment] NVARCHAR(MAX) NULL,
                    [ReviewedAt] DATETIME2 NULL,
                    [ReviewerNotifiedAt] DATETIME2 NULL,
                    CONSTRAINT [PK_MentorRoundReviews] PRIMARY KEY ([ReviewRoundID], [GroupID]),
                    CONSTRAINT [FK_MentorRoundReviews_ReviewRounds_ReviewRoundID] FOREIGN KEY ([ReviewRoundID]) REFERENCES [ReviewRounds]([ReviewRoundID]),
                    CONSTRAINT [FK_MentorRoundReviews_ProjectGroups_GroupID] FOREIGN KEY ([GroupID]) REFERENCES [ProjectGroups]([GroupID]),
                    CONSTRAINT [FK_MentorRoundReviews_Users_SupervisorID] FOREIGN KEY ([SupervisorID]) REFERENCES [Users]([UserID])
                );
            END
            """);

        await EnsureColumnAsync("ChecklistItems", "ItemName", "NVARCHAR(256) NULL");
        await EnsureColumnAsync("ChecklistItems", "SectionCode", "NVARCHAR(64) NULL");
        await EnsureColumnAsync("ChecklistItems", "SectionTitle", "NVARCHAR(256) NULL");
        await EnsureColumnAsync("ChecklistItems", "PriorityLabel", "NVARCHAR(64) NULL");
        await EnsureColumnAsync("ChecklistItems", "InputType", "INT NOT NULL CONSTRAINT [DF_ChecklistItems_InputType] DEFAULT 0");
        await EnsureColumnAsync("ChecklistItems", "ExcellentRubric", "NVARCHAR(MAX) NULL");
        await EnsureColumnAsync("ChecklistItems", "GoodRubric", "NVARCHAR(MAX) NULL");
        await EnsureColumnAsync("ChecklistItems", "AcceptableRubric", "NVARCHAR(MAX) NULL");
        await EnsureColumnAsync("ChecklistItems", "FailRubric", "NVARCHAR(MAX) NULL");

        await EnsureColumnAsync("EvaluationDetails", "AssessmentValue", "NVARCHAR(32) NULL");
        await EnsureColumnAsync("EvaluationDetails", "MentorComment", "NVARCHAR(MAX) NULL");
        await EnsureColumnAsync("EvaluationDetails", "GradeDescription", "NVARCHAR(MAX) NULL");
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
