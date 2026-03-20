using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fix",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(238));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(240));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(241));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(241));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(243));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(244));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(244));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(245));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(246));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(242));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(152));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(163));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(165));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(166));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(169));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(170));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(172));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(174));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(176));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 6, 10, 1, 52, 710, DateTimeKind.Utc).AddTicks(178));
        }
    }
}
