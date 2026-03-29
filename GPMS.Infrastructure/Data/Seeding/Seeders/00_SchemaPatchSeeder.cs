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

    public int Order => -10;

    public async Task SeedAsync()
    {
        await EnsureColumnAsync("EvaluationDetails", "GradeDescription", "NVARCHAR(MAX) NULL");
        await EnsureColumnAsync("GroupMembers", "Status", "NVARCHAR(20) NOT NULL DEFAULT 'InProgress'");
        await EnsureColumnAsync("Notifications", "RelatedEntityType", "NVARCHAR(50) NULL");
        await EnsureColumnAsync("Notifications", "RelatedEntityID", "INT NULL");

        // Committees Table and ReviewerAssignments foreign keys
        await _context.Database.ExecuteSqlRawAsync(@"
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'CommitteeRole' AND Object_ID = OBJECT_ID('ReviewerAssignments'))
                ALTER TABLE ReviewerAssignments ADD CommitteeRole INT NULL;
            
            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'CommitteeID' AND Object_ID = OBJECT_ID('ReviewerAssignments'))
                ALTER TABLE ReviewerAssignments ADD CommitteeID INT NULL;

            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'CommitteeID' AND Object_ID = OBJECT_ID('ReviewSessionInfo'))
                ALTER TABLE ReviewSessionInfo ADD CommitteeID INT NULL;

            IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'IsOnline' AND Object_ID = OBJECT_ID('ReviewSessionInfo'))
                ALTER TABLE ReviewSessionInfo ADD IsOnline BIT NOT NULL DEFAULT 0;

            IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID('Committees') AND type in (N'U'))
            BEGIN
                CREATE TABLE Committees (
                    CommitteeID INT PRIMARY KEY IDENTITY(1,1),
                    CommitteeName NVARCHAR(100) NOT NULL,
                    SemesterID INT NOT NULL,
                    ChairpersonID NVARCHAR(450) NOT NULL,
                    SecretaryID NVARCHAR(450) NOT NULL,
                    ReviewerID NVARCHAR(450) NOT NULL,
                    Reviewer2ID NVARCHAR(450) NULL,
                    Reviewer3ID NVARCHAR(450) NULL,
                    CONSTRAINT FK_Committees_Semester FOREIGN KEY (SemesterID) REFERENCES Semesters(SemesterID),
                    CONSTRAINT FK_Committees_Chairperson FOREIGN KEY (ChairpersonID) REFERENCES Users(UserID),
                    CONSTRAINT FK_Committees_Secretary FOREIGN KEY (SecretaryID) REFERENCES Users(UserID),
                    CONSTRAINT FK_Committees_Reviewer FOREIGN KEY (ReviewerID) REFERENCES Users(UserID),
                    CONSTRAINT FK_Committees_Reviewer2 FOREIGN KEY (Reviewer2ID) REFERENCES Users(UserID),
                    CONSTRAINT FK_Committees_Reviewer3 FOREIGN KEY (Reviewer3ID) REFERENCES Users(UserID)
                );
            END
            ELSE
            BEGIN
                -- Ensure columns exist if table was created previously
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'Reviewer2ID' AND Object_ID = OBJECT_ID('Committees'))
                    ALTER TABLE Committees ADD Reviewer2ID NVARCHAR(450) NULL;
                
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'Reviewer3ID' AND Object_ID = OBJECT_ID('Committees'))
                    ALTER TABLE Committees ADD Reviewer3ID NVARCHAR(450) NULL;
            END;

            -- Cleanup Round 4 SP26
            IF EXISTS (SELECT * FROM ReviewRounds WHERE RoundNumber = 4)
            BEGIN
                DELETE FROM ReviewerAssignments WHERE ReviewRoundID IN (SELECT ReviewRoundID FROM ReviewRounds WHERE RoundNumber = 4);
                DELETE FROM ReviewSessionInfo WHERE ReviewRoundID IN (SELECT ReviewRoundID FROM ReviewRounds WHERE RoundNumber = 4);
                DELETE FROM ReviewRounds WHERE RoundNumber = 4;
            END
        ");
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
