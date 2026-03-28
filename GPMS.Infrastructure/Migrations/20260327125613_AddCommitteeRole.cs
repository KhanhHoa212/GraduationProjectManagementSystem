using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GPMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommitteeRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(174));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(177));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(178));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(180));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(182));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(184));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(185));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(186));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(187));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(181));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(71));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(91));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(96));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(101));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(106));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(110));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(113));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(121));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(125));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 27, 12, 56, 12, 541, DateTimeKind.Utc).AddTicks(128));

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4798));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4801));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4802));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4803));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4804));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4805));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4806));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4807));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4808));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4804));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4657));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4674));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4677));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4679));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4681));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4731));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4733));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4735));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4737));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 25, 8, 47, 37, 859, DateTimeKind.Utc).AddTicks(4739));
        }
    }
}
