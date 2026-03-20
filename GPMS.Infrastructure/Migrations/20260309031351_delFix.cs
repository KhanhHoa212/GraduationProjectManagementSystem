using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class delFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fix",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3502));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3504));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3505));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3506));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3509));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3510));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3511));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3513));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3514));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3508));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3387));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3407));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3410));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3414));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3418));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3421));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3425));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3437));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3441));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 3, 13, 51, 75, DateTimeKind.Utc).AddTicks(3444));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "fix",
                table: "Users",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1750));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1752));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1753));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1754));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1756));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1757));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1758));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1759));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1760));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1755));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1643), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1663), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1666), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1669), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1672), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1674), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1677), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1680), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1683), "" });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                columns: new[] { "CreatedAt", "fix" },
                values: new object[] { new DateTime(2026, 3, 9, 3, 12, 24, 782, DateTimeKind.Utc).AddTicks(1695), "" });
        }
    }
}
