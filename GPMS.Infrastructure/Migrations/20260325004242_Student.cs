using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GPMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Student : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Faculties",
                columns: table => new
                {
                    FacultyID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FacultyCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    FacultyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faculties", x => x.FacultyID);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    RoomID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    Building = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    RoomType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Classroom"),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Available"),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.RoomID);
                });

            migrationBuilder.CreateTable(
                name: "Semesters",
                columns: table => new
                {
                    SemesterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemesterCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    AcademicYear = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Upcoming")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Semesters", x => x.SemesterID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    AvatarUrl = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Active"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.CheckConstraint("CK_User_EmailOrUsername", "[Email] IS NOT NULL OR [Username] IS NOT NULL");
                });

            migrationBuilder.CreateTable(
                name: "Majors",
                columns: table => new
                {
                    MajorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MajorCode = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    MajorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FacultyID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Majors", x => x.MajorID);
                    table.ForeignKey(
                        name: "FK_Majors_Faculties_FacultyID",
                        column: x => x.FacultyID,
                        principalTable: "Faculties",
                        principalColumn: "FacultyID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewRounds",
                columns: table => new
                {
                    ReviewRoundID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SemesterID = table.Column<int>(type: "int", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    RoundType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionDeadline = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Planned")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewRounds", x => x.ReviewRoundID);
                    table.ForeignKey(
                        name: "FK_ReviewRounds_Semesters_SemesterID",
                        column: x => x.SemesterID,
                        principalTable: "Semesters",
                        principalColumn: "SemesterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RecipientID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RelatedEntityID = table.Column<int>(type: "int", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsEmailSent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_RecipientID",
                        column: x => x.RecipientID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCredentials",
                columns: table => new
                {
                    CredentialID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    AuthProvider = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ExternalProviderID = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PasswordResetExpiry = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCredentials", x => x.CredentialID);
                    table.ForeignKey(
                        name: "FK_UserCredentials_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserRoleID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.UserRoleID);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpertiseAreas",
                columns: table => new
                {
                    ExpertiseID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpertiseName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MajorID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpertiseAreas", x => x.ExpertiseID);
                    table.ForeignKey(
                        name: "FK_ExpertiseAreas_Majors_MajorID",
                        column: x => x.MajorID,
                        principalTable: "Majors",
                        principalColumn: "MajorID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    ProjectID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ProjectName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SemesterID = table.Column<int>(type: "int", nullable: false),
                    MajorID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.ProjectID);
                    table.ForeignKey(
                        name: "FK_Projects_Majors_MajorID",
                        column: x => x.MajorID,
                        principalTable: "Majors",
                        principalColumn: "MajorID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Semesters_SemesterID",
                        column: x => x.SemesterID,
                        principalTable: "Semesters",
                        principalColumn: "SemesterID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewChecklists",
                columns: table => new
                {
                    ChecklistID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewRoundID = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "YesNo")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewChecklists", x => x.ChecklistID);
                    table.ForeignKey(
                        name: "FK_ReviewChecklists_ReviewRounds_ReviewRoundID",
                        column: x => x.ReviewRoundID,
                        principalTable: "ReviewRounds",
                        principalColumn: "ReviewRoundID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewChecklists_Users_CreatedBy",
                        column: x => x.CreatedBy,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubmissionRequirements",
                columns: table => new
                {
                    RequirementID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewRoundID = table.Column<int>(type: "int", nullable: false),
                    DocumentName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AllowedFormats = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    MaxFileSizeMB = table.Column<int>(type: "int", nullable: true, defaultValue: 50),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Deadline = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmissionRequirements", x => x.RequirementID);
                    table.ForeignKey(
                        name: "FK_SubmissionRequirements_ReviewRounds_ReviewRoundID",
                        column: x => x.ReviewRoundID,
                        principalTable: "ReviewRounds",
                        principalColumn: "ReviewRoundID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LecturerExpertise",
                columns: table => new
                {
                    LecturerID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    ExpertiseID = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LecturerExpertise", x => new { x.LecturerID, x.ExpertiseID });
                    table.ForeignKey(
                        name: "FK_LecturerExpertise_ExpertiseAreas_ExpertiseID",
                        column: x => x.ExpertiseID,
                        principalTable: "ExpertiseAreas",
                        principalColumn: "ExpertiseID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LecturerExpertise_Users_LecturerID",
                        column: x => x.LecturerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProjectGroups",
                columns: table => new
                {
                    GroupID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProjectID = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectGroups", x => x.GroupID);
                    table.ForeignKey(
                        name: "FK_ProjectGroups_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectSupervisors",
                columns: table => new
                {
                    ProjectID = table.Column<int>(type: "int", nullable: false),
                    LecturerID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AssignedBy = table.Column<string>(type: "nvarchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectSupervisors", x => new { x.ProjectID, x.LecturerID });
                    table.ForeignKey(
                        name: "FK_ProjectSupervisors_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectSupervisors_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectSupervisors_Users_LecturerID",
                        column: x => x.LecturerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistItems",
                columns: table => new
                {
                    ItemID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChecklistID = table.Column<int>(type: "int", nullable: false),
                    ItemCode = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    ItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ItemContent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "YesNo"),
                    Section = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistItems", x => x.ItemID);
                    table.ForeignKey(
                        name: "FK_ChecklistItems_ReviewChecklists_ChecklistID",
                        column: x => x.ChecklistID,
                        principalTable: "ReviewChecklists",
                        principalColumn: "ChecklistID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Evaluations",
                columns: table => new
                {
                    EvaluationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewRoundID = table.Column<int>(type: "int", nullable: false),
                    ReviewerID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Draft"),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OverallComment = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluations", x => x.EvaluationID);
                    table.ForeignKey(
                        name: "FK_Evaluations_ProjectGroups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "ProjectGroups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Evaluations_ReviewRounds_ReviewRoundID",
                        column: x => x.ReviewRoundID,
                        principalTable: "ReviewRounds",
                        principalColumn: "ReviewRoundID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluations_Users_ReviewerID",
                        column: x => x.ReviewerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupMembers",
                columns: table => new
                {
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    RoleInGroup = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupMembers", x => new { x.GroupID, x.UserID });
                    table.ForeignKey(
                        name: "FK_GroupMembers_ProjectGroups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "ProjectGroups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupMembers_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GroupRoundProgresses",
                columns: table => new
                {
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    ReviewRoundID = table.Column<int>(type: "int", nullable: false),
                    MentorDecision = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    MentorComment = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupRoundProgresses", x => new { x.GroupID, x.ReviewRoundID });
                    table.ForeignKey(
                        name: "FK_GroupRoundProgresses_ProjectGroups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "ProjectGroups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GroupRoundProgresses_ReviewRounds_ReviewRoundID",
                        column: x => x.ReviewRoundID,
                        principalTable: "ReviewRounds",
                        principalColumn: "ReviewRoundID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MentorRoundReviews",
                columns: table => new
                {
                    ReviewRoundID = table.Column<int>(type: "int", nullable: false),
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    SupervisorID = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DecisionStatus = table.Column<int>(type: "int", nullable: false),
                    ProgressComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewerNotifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MentorRoundReviews", x => new { x.ReviewRoundID, x.GroupID });
                    table.ForeignKey(
                        name: "FK_MentorRoundReviews_ProjectGroups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "ProjectGroups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorRoundReviews_ReviewRounds_ReviewRoundID",
                        column: x => x.ReviewRoundID,
                        principalTable: "ReviewRounds",
                        principalColumn: "ReviewRoundID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MentorRoundReviews_Users_SupervisorID",
                        column: x => x.SupervisorID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewerAssignments",
                columns: table => new
                {
                    AssignmentID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewRoundID = table.Column<int>(type: "int", nullable: false),
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    ReviewerID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    IsRandom = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AssignedBy = table.Column<string>(type: "nvarchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewerAssignments", x => x.AssignmentID);
                    table.ForeignKey(
                        name: "FK_ReviewerAssignments_ProjectGroups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "ProjectGroups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewerAssignments_ReviewRounds_ReviewRoundID",
                        column: x => x.ReviewRoundID,
                        principalTable: "ReviewRounds",
                        principalColumn: "ReviewRoundID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewerAssignments_Users_AssignedBy",
                        column: x => x.AssignedBy,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewerAssignments_Users_ReviewerID",
                        column: x => x.ReviewerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReviewSessionInfo",
                columns: table => new
                {
                    SessionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewRoundID = table.Column<int>(type: "int", nullable: false),
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    MeetLink = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: true),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RoomID = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewSessionInfo", x => x.SessionID);
                    table.ForeignKey(
                        name: "FK_ReviewSessionInfo_ProjectGroups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "ProjectGroups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReviewSessionInfo_ReviewRounds_ReviewRoundID",
                        column: x => x.ReviewRoundID,
                        principalTable: "ReviewRounds",
                        principalColumn: "ReviewRoundID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewSessionInfo_Rooms_RoomID",
                        column: x => x.RoomID,
                        principalTable: "Rooms",
                        principalColumn: "RoomID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Submissions",
                columns: table => new
                {
                    SubmissionID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequirementID = table.Column<int>(type: "int", nullable: false),
                    GroupID = table.Column<int>(type: "int", nullable: false),
                    FileUrl = table.Column<string>(type: "varchar(500)", unicode: false, maxLength: 500, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    FileSizeMB = table.Column<decimal>(type: "decimal(6,2)", precision: 6, scale: 2, nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    SubmittedBy = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Submissions", x => x.SubmissionID);
                    table.ForeignKey(
                        name: "FK_Submissions_ProjectGroups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "ProjectGroups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Submissions_SubmissionRequirements_RequirementID",
                        column: x => x.RequirementID,
                        principalTable: "SubmissionRequirements",
                        principalColumn: "RequirementID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Submissions_Users_SubmittedBy",
                        column: x => x.SubmittedBy,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RubricDescriptions",
                columns: table => new
                {
                    RubricID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    GradeLevel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RubricDescriptions", x => x.RubricID);
                    table.ForeignKey(
                        name: "FK_RubricDescriptions_ChecklistItems_ItemID",
                        column: x => x.ItemID,
                        principalTable: "ChecklistItems",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EvaluationDetails",
                columns: table => new
                {
                    EvaluationID = table.Column<int>(type: "int", nullable: false),
                    ItemID = table.Column<int>(type: "int", nullable: false),
                    Assessment = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MentorComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GradeDescription = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvaluationDetails", x => new { x.EvaluationID, x.ItemID });
                    table.ForeignKey(
                        name: "FK_EvaluationDetails_ChecklistItems_ItemID",
                        column: x => x.ItemID,
                        principalTable: "ChecklistItems",
                        principalColumn: "ItemID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EvaluationDetails_Evaluations_EvaluationID",
                        column: x => x.EvaluationID,
                        principalTable: "Evaluations",
                        principalColumn: "EvaluationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluationID = table.Column<int>(type: "int", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackID);
                    table.ForeignKey(
                        name: "FK_Feedbacks_Evaluations_EvaluationID",
                        column: x => x.EvaluationID,
                        principalTable: "Evaluations",
                        principalColumn: "EvaluationID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeedbackApprovals",
                columns: table => new
                {
                    FeedbackID = table.Column<int>(type: "int", nullable: false),
                    SupervisorID = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    ApprovalStatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    SupervisorComment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoReleasedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsVisibleToStudent = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeedbackApprovals", x => x.FeedbackID);
                    table.ForeignKey(
                        name: "FK_FeedbackApprovals_Feedbacks_FeedbackID",
                        column: x => x.FeedbackID,
                        principalTable: "Feedbacks",
                        principalColumn: "FeedbackID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeedbackApprovals_Users_SupervisorID",
                        column: x => x.SupervisorID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "FacultyID", "FacultyCode", "FacultyName" },
                values: new object[] { 1, "SE", "Software Engineering Faculty" });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomID", "Building", "Capacity", "Notes", "RoomCode" },
                values: new object[,]
                {
                    { 1, "Alpha", 30, null, "B1.01" },
                    { 2, "Alpha", 30, null, "B1.02" }
                });

            migrationBuilder.InsertData(
                table: "Rooms",
                columns: new[] { "RoomID", "Building", "Capacity", "Notes", "RoomCode", "RoomType" },
                values: new object[] { 3, "Delta", 100, null, "Hall-A", "Hall" });

            migrationBuilder.InsertData(
                table: "Semesters",
                columns: new[] { "SemesterID", "AcademicYear", "EndDate", "SemesterCode", "StartDate", "Status" },
                values: new object[,]
                {
                    { 1, "2023-2024", new DateTime(2024, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "SP24", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Closed" },
                    { 2, "2023-2024", new DateTime(2024, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "SU24", new DateTime(2024, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Closed" },
                    { 3, "2024-2025", new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "FALL24", new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Closed" },
                    { 4, "2024-2025", new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "SP25", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Closed" },
                    { 5, "2024-2025", new DateTime(2025, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "SU25", new DateTime(2025, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Closed" },
                    { 6, "2025-2026", new DateTime(2025, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "FALL25", new DateTime(2025, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Closed" },
                    { 7, "2025-2026", new DateTime(2026, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "SP26", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active" }
                });

            migrationBuilder.InsertData(
                table: "Semesters",
                columns: new[] { "SemesterID", "AcademicYear", "EndDate", "SemesterCode", "StartDate" },
                values: new object[,]
                {
                    { 8, "2025-2026", new DateTime(2026, 8, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "SU26", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 9, "2026-2027", new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "FALL26", new DateTime(2026, 9, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserID", "AvatarUrl", "CreatedAt", "Email", "FullName", "Phone", "Username" },
                values: new object[,]
                {
                    { "ADMIN001", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4942), "admin@fpt.edu.vn", "System Admin", null, "admin" },
                    { "GV001", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4951), "giao-vien1@fpt.edu.vn", "Lecturer One", null, null },
                    { "GV002", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4953), "giao-vien2@fpt.edu.vn", "Lecturer Two", null, null },
                    { "GV003", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4956), "giao-vien3@fpt.edu.vn", "Lecturer Three", null, null },
                    { "HOD001", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4957), "hod@fpt.edu.vn", "Head of Department", null, null },
                    { "SE180001", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4959), "student1@fpt.edu.vn", "Student One", null, null },
                    { "SE180002", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4961), "student2@fpt.edu.vn", "Student Two", null, null },
                    { "SE180003", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4963), "student3@fpt.edu.vn", "Student Three", null, null },
                    { "SE180004", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4964), "student4@fpt.edu.vn", "Student Four", null, null },
                    { "SE180005", null, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4966), "student5@fpt.edu.vn", "Student Five", null, null }
                });

            migrationBuilder.InsertData(
                table: "Majors",
                columns: new[] { "MajorID", "FacultyID", "MajorCode", "MajorName" },
                values: new object[,]
                {
                    { 1, 1, "SE", "Software Engineering" },
                    { 2, 1, "SS", "Software Testing" }
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "UserRoleID", "AssignedAt", "RoleName", "UserID" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4988), "Admin", "ADMIN001" },
                    { 2, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4989), "Lecturer", "GV001" },
                    { 3, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4990), "Lecturer", "GV002" },
                    { 4, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4991), "Lecturer", "GV003" },
                    { 5, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4992), "Student", "SE180001" },
                    { 6, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4993), "Student", "SE180002" },
                    { 7, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4993), "Student", "SE180003" },
                    { 8, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4994), "Student", "SE180004" },
                    { 9, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4994), "Student", "SE180005" },
                    { 10, new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4991), "HeadOfDept", "HOD001" }
                });

            migrationBuilder.InsertData(
                table: "ExpertiseAreas",
                columns: new[] { "ExpertiseID", "ExpertiseName", "MajorID" },
                values: new object[,]
                {
                    { 1, "Web Development", 1 },
                    { 2, "Mobile Development", 1 },
                    { 3, "AI/ML", 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistItems_ChecklistID",
                table: "ChecklistItems",
                column: "ChecklistID");

            migrationBuilder.CreateIndex(
                name: "IX_EvaluationDetails_ItemID",
                table: "EvaluationDetails",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_GroupID",
                table: "Evaluations",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ReviewerID",
                table: "Evaluations",
                column: "ReviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_Evaluations_ReviewRoundID_ReviewerID_GroupID",
                table: "Evaluations",
                columns: new[] { "ReviewRoundID", "ReviewerID", "GroupID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpertiseAreas_MajorID",
                table: "ExpertiseAreas",
                column: "MajorID");

            migrationBuilder.CreateIndex(
                name: "IX_Faculties_FacultyCode",
                table: "Faculties",
                column: "FacultyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackApprovals_SupervisorID",
                table: "FeedbackApprovals",
                column: "SupervisorID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_EvaluationID",
                table: "Feedbacks",
                column: "EvaluationID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupMembers_UserID",
                table: "GroupMembers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_GroupRoundProgresses_ReviewRoundID",
                table: "GroupRoundProgresses",
                column: "ReviewRoundID");

            migrationBuilder.CreateIndex(
                name: "IX_LecturerExpertise_ExpertiseID",
                table: "LecturerExpertise",
                column: "ExpertiseID");

            migrationBuilder.CreateIndex(
                name: "IX_Majors_FacultyID",
                table: "Majors",
                column: "FacultyID");

            migrationBuilder.CreateIndex(
                name: "IX_Majors_MajorCode",
                table: "Majors",
                column: "MajorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MentorRoundReviews_GroupID",
                table: "MentorRoundReviews",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_MentorRoundReviews_SupervisorID",
                table: "MentorRoundReviews",
                column: "SupervisorID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientID",
                table: "Notifications",
                column: "RecipientID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_ProjectID",
                table: "ProjectGroups",
                column: "ProjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_MajorID",
                table: "Projects",
                column: "MajorID");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectCode",
                table: "Projects",
                column: "ProjectCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_SemesterID",
                table: "Projects",
                column: "SemesterID");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSupervisors_AssignedBy",
                table: "ProjectSupervisors",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectSupervisors_LecturerID",
                table: "ProjectSupervisors",
                column: "LecturerID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewChecklists_CreatedBy",
                table: "ReviewChecklists",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewChecklists_ReviewRoundID",
                table: "ReviewChecklists",
                column: "ReviewRoundID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignments_AssignedBy",
                table: "ReviewerAssignments",
                column: "AssignedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignments_GroupID",
                table: "ReviewerAssignments",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignments_ReviewerID",
                table: "ReviewerAssignments",
                column: "ReviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignments_ReviewRoundID_GroupID_ReviewerID",
                table: "ReviewerAssignments",
                columns: new[] { "ReviewRoundID", "GroupID", "ReviewerID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRounds_SemesterID_RoundNumber",
                table: "ReviewRounds",
                columns: new[] { "SemesterID", "RoundNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewSessionInfo_GroupID",
                table: "ReviewSessionInfo",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewSessionInfo_ReviewRoundID_GroupID",
                table: "ReviewSessionInfo",
                columns: new[] { "ReviewRoundID", "GroupID" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReviewSessionInfo_RoomID",
                table: "ReviewSessionInfo",
                column: "RoomID");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomCode",
                table: "Rooms",
                column: "RoomCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RubricDescriptions_ItemID",
                table: "RubricDescriptions",
                column: "ItemID");

            migrationBuilder.CreateIndex(
                name: "IX_Semesters_SemesterCode",
                table: "Semesters",
                column: "SemesterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmissionRequirements_ReviewRoundID",
                table: "SubmissionRequirements",
                column: "ReviewRoundID");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_GroupID",
                table: "Submissions",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_RequirementID_GroupID_Version",
                table: "Submissions",
                columns: new[] { "RequirementID", "GroupID", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Submissions_SubmittedBy",
                table: "Submissions",
                column: "SubmittedBy");

            migrationBuilder.CreateIndex(
                name: "IX_UserCredentials_UserID_AuthProvider",
                table: "UserCredentials",
                columns: new[] { "UserID", "AuthProvider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_UserID",
                table: "UserRoles",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true,
                filter: "[Username] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvaluationDetails");

            migrationBuilder.DropTable(
                name: "FeedbackApprovals");

            migrationBuilder.DropTable(
                name: "GroupMembers");

            migrationBuilder.DropTable(
                name: "GroupRoundProgresses");

            migrationBuilder.DropTable(
                name: "LecturerExpertise");

            migrationBuilder.DropTable(
                name: "MentorRoundReviews");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "ProjectSupervisors");

            migrationBuilder.DropTable(
                name: "ReviewerAssignments");

            migrationBuilder.DropTable(
                name: "ReviewSessionInfo");

            migrationBuilder.DropTable(
                name: "RubricDescriptions");

            migrationBuilder.DropTable(
                name: "Submissions");

            migrationBuilder.DropTable(
                name: "UserCredentials");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "ExpertiseAreas");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "ChecklistItems");

            migrationBuilder.DropTable(
                name: "SubmissionRequirements");

            migrationBuilder.DropTable(
                name: "Evaluations");

            migrationBuilder.DropTable(
                name: "ReviewChecklists");

            migrationBuilder.DropTable(
                name: "ProjectGroups");

            migrationBuilder.DropTable(
                name: "ReviewRounds");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "Majors");

            migrationBuilder.DropTable(
                name: "Semesters");

            migrationBuilder.DropTable(
                name: "Faculties");
        }
    }
}
