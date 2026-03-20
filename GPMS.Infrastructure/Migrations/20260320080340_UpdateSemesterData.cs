using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GPMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSemesterData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 1,
                columns: new[] { "AcademicYear", "EndDate", "SemesterCode", "StartDate", "Status" },
                values: new object[] { "2023-2024", new DateTime(2024, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "SP24", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Closed" });

            migrationBuilder.InsertData(
                table: "Semesters",
                columns: new[] { "SemesterID", "AcademicYear", "EndDate", "SemesterCode", "StartDate", "Status" },
                values: new object[,]
                {
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

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6938));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6941));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6942));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6943));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6944));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6945));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6946));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6947));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6947));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6943));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6834));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6845));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6848));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6850));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6851));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6853));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6855));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6856));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6858));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 20, 8, 3, 39, 740, DateTimeKind.Utc).AddTicks(6909));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 9);

            migrationBuilder.UpdateData(
                table: "Semesters",
                keyColumn: "SemesterID",
                keyValue: 1,
                columns: new[] { "AcademicYear", "EndDate", "SemesterCode", "StartDate", "Status" },
                values: new object[] { "2024-2025", new DateTime(2025, 4, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "SP25", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active" });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4964));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4967));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4968));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4969));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4971));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4972));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4973));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4973));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4970));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4881));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4893));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4896));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4898));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4929));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4931));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4932));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4935));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4937));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 19, 17, 24, 11, 723, DateTimeKind.Utc).AddTicks(4938));
        }
    }
}
