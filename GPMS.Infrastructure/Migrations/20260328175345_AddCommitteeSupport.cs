using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GPMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommitteeSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_ProjectGroups_GroupID",
                table: "Evaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectGroups_Projects_ProjectID",
                table: "ProjectGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewerAssignments_ProjectGroups_GroupID",
                table: "ReviewerAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewSessionInfo_ProjectGroups_GroupID",
                table: "ReviewSessionInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_ProjectGroups_GroupID",
                table: "Submissions");

            migrationBuilder.AddColumn<int>(
                name: "CommitteeID",
                table: "ReviewSessionInfo",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnline",
                table: "ReviewSessionInfo",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CommitteeID",
                table: "ReviewerAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CommitteeRole",
                table: "ReviewerAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MajorCode",
                table: "Majors",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "GroupMembers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "InProgress");

            migrationBuilder.AlterColumn<string>(
                name: "FacultyCode",
                table: "Faculties",
                type: "varchar(3)",
                unicode: false,
                maxLength: 3,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldUnicode: false,
                oldMaxLength: 10);

            migrationBuilder.CreateTable(
                name: "Committees",
                columns: table => new
                {
                    CommitteeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommitteeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SemesterID = table.Column<int>(type: "int", nullable: false),
                    ChairpersonID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    SecretaryID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    ReviewerID = table.Column<string>(type: "nvarchar(20)", nullable: false),
                    Reviewer2ID = table.Column<string>(type: "nvarchar(20)", nullable: true),
                    Reviewer3ID = table.Column<string>(type: "nvarchar(20)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Committees", x => x.CommitteeID);
                    table.ForeignKey(
                        name: "FK_Committees_Semesters_SemesterID",
                        column: x => x.SemesterID,
                        principalTable: "Semesters",
                        principalColumn: "SemesterID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Committees_Users_ChairpersonID",
                        column: x => x.ChairpersonID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Committees_Users_Reviewer2ID",
                        column: x => x.Reviewer2ID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Committees_Users_Reviewer3ID",
                        column: x => x.Reviewer3ID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Committees_Users_ReviewerID",
                        column: x => x.ReviewerID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Committees_Users_SecretaryID",
                        column: x => x.SecretaryID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                table: "Faculties",
                keyColumn: "FacultyID",
                keyValue: 1,
                columns: new[] { "FacultyCode", "FacultyName" },
                values: new object[] { "IT", "Information Technology Faculty" });

            migrationBuilder.InsertData(
                table: "Faculties",
                columns: new[] { "FacultyID", "FacultyCode", "FacultyName" },
                values: new object[,]
                {
                    { 2, "EC", "Economy Faculty" },
                    { 3, "DA", "Digital Arts Faculty" },
                    { 4, "LA", "Language Faculty" }
                });

            migrationBuilder.UpdateData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 2,
                columns: new[] { "FacultyID", "MajorCode", "MajorName" },
                values: new object[] { 2, "DM", "Digital Marketing" });

            migrationBuilder.InsertData(
                table: "Majors",
                columns: new[] { "MajorID", "FacultyID", "MajorCode", "MajorName" },
                values: new object[,]
                {
                    { 8, 1, "IA", "Information Assurance" },
                    { 9, 1, "IC", "Integrated Circuit Design" }
                });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9484));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9485));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9486));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9487));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9488));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9489));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9490));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9490));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9491));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9488));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9397));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9411));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9413));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9415));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9417));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9419));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9421));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9453));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9455));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 28, 17, 53, 45, 181, DateTimeKind.Utc).AddTicks(9457));

            migrationBuilder.InsertData(
                table: "Majors",
                columns: new[] { "MajorID", "FacultyID", "MajorCode", "MajorName" },
                values: new object[,]
                {
                    { 3, 3, "GD", "Graphic Design" },
                    { 4, 4, "EN", "English" },
                    { 5, 4, "JP", "Japanese" },
                    { 6, 4, "KR", "Korean" },
                    { 7, 4, "CN", "Chinese" },
                    { 10, 2, "FI", "Finance" },
                    { 11, 2, "MK", "Marketing" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewSessionInfo_CommitteeID",
                table: "ReviewSessionInfo",
                column: "CommitteeID");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewerAssignments_CommitteeID",
                table: "ReviewerAssignments",
                column: "CommitteeID");

            migrationBuilder.CreateIndex(
                name: "IX_Committees_ChairpersonID",
                table: "Committees",
                column: "ChairpersonID");

            migrationBuilder.CreateIndex(
                name: "IX_Committees_Reviewer2ID",
                table: "Committees",
                column: "Reviewer2ID");

            migrationBuilder.CreateIndex(
                name: "IX_Committees_Reviewer3ID",
                table: "Committees",
                column: "Reviewer3ID");

            migrationBuilder.CreateIndex(
                name: "IX_Committees_ReviewerID",
                table: "Committees",
                column: "ReviewerID");

            migrationBuilder.CreateIndex(
                name: "IX_Committees_SecretaryID",
                table: "Committees",
                column: "SecretaryID");

            migrationBuilder.CreateIndex(
                name: "IX_Committees_SemesterID",
                table: "Committees",
                column: "SemesterID");

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_ProjectGroups_GroupID",
                table: "Evaluations",
                column: "GroupID",
                principalTable: "ProjectGroups",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectGroups_Projects_ProjectID",
                table: "ProjectGroups",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewerAssignments_Committees_CommitteeID",
                table: "ReviewerAssignments",
                column: "CommitteeID",
                principalTable: "Committees",
                principalColumn: "CommitteeID",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewerAssignments_ProjectGroups_GroupID",
                table: "ReviewerAssignments",
                column: "GroupID",
                principalTable: "ProjectGroups",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewSessionInfo_Committees_CommitteeID",
                table: "ReviewSessionInfo",
                column: "CommitteeID",
                principalTable: "Committees",
                principalColumn: "CommitteeID");

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewSessionInfo_ProjectGroups_GroupID",
                table: "ReviewSessionInfo",
                column: "GroupID",
                principalTable: "ProjectGroups",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_ProjectGroups_GroupID",
                table: "Submissions",
                column: "GroupID",
                principalTable: "ProjectGroups",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Evaluations_ProjectGroups_GroupID",
                table: "Evaluations");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectGroups_Projects_ProjectID",
                table: "ProjectGroups");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewerAssignments_Committees_CommitteeID",
                table: "ReviewerAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewerAssignments_ProjectGroups_GroupID",
                table: "ReviewerAssignments");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewSessionInfo_Committees_CommitteeID",
                table: "ReviewSessionInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_ReviewSessionInfo_ProjectGroups_GroupID",
                table: "ReviewSessionInfo");

            migrationBuilder.DropForeignKey(
                name: "FK_Submissions_ProjectGroups_GroupID",
                table: "Submissions");

            migrationBuilder.DropTable(
                name: "Committees");

            migrationBuilder.DropIndex(
                name: "IX_ReviewSessionInfo_CommitteeID",
                table: "ReviewSessionInfo");

            migrationBuilder.DropIndex(
                name: "IX_ReviewerAssignments_CommitteeID",
                table: "ReviewerAssignments");

            migrationBuilder.DeleteData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "FacultyID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "FacultyID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Faculties",
                keyColumn: "FacultyID",
                keyValue: 4);

            migrationBuilder.DropColumn(
                name: "CommitteeID",
                table: "ReviewSessionInfo");

            migrationBuilder.DropColumn(
                name: "IsOnline",
                table: "ReviewSessionInfo");

            migrationBuilder.DropColumn(
                name: "CommitteeID",
                table: "ReviewerAssignments");

            migrationBuilder.DropColumn(
                name: "CommitteeRole",
                table: "ReviewerAssignments");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "GroupMembers");

            migrationBuilder.AlterColumn<string>(
                name: "MajorCode",
                table: "Majors",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3);

            migrationBuilder.AlterColumn<string>(
                name: "FacultyCode",
                table: "Faculties",
                type: "varchar(10)",
                unicode: false,
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)",
                oldUnicode: false,
                oldMaxLength: 3);

            migrationBuilder.UpdateData(
                table: "Faculties",
                keyColumn: "FacultyID",
                keyValue: 1,
                columns: new[] { "FacultyCode", "FacultyName" },
                values: new object[] { "SE", "Software Engineering Faculty" });

            migrationBuilder.UpdateData(
                table: "Majors",
                keyColumn: "MajorID",
                keyValue: 2,
                columns: new[] { "FacultyID", "MajorCode", "MajorName" },
                values: new object[] { 1, "SS", "Software Testing" });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4988));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4989));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4990));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4991));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4992));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4993));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4993));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4994));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4994));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4991));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4942));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4951));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4953));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4956));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4957));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4959));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4961));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4963));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4964));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 0, 42, 41, 858, DateTimeKind.Utc).AddTicks(4966));

            migrationBuilder.AddForeignKey(
                name: "FK_Evaluations_ProjectGroups_GroupID",
                table: "Evaluations",
                column: "GroupID",
                principalTable: "ProjectGroups",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectGroups_Projects_ProjectID",
                table: "ProjectGroups",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewerAssignments_ProjectGroups_GroupID",
                table: "ReviewerAssignments",
                column: "GroupID",
                principalTable: "ProjectGroups",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReviewSessionInfo_ProjectGroups_GroupID",
                table: "ReviewSessionInfo",
                column: "GroupID",
                principalTable: "ProjectGroups",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Submissions_ProjectGroups_GroupID",
                table: "Submissions",
                column: "GroupID",
                principalTable: "ProjectGroups",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
