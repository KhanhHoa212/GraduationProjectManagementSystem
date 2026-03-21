using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GPMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuthAndAuthorizeFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetExpiry",
                table: "UserCredentials",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "UserCredentials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1586));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1588));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1589));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1590));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1591));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1592));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1593));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1594));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1595));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1487));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1513));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1516));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1519));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1523));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1526));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1529));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1533));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 2, 3, 46, 9, 608, DateTimeKind.Utc).AddTicks(1536));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetExpiry",
                table: "UserCredentials");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "UserCredentials");

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4401));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4402));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4403));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4404));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4405));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4405));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4406));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4407));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4408));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4329));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4340));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4343));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4344));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4346));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4348));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4349));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4377));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 2, 27, 2, 47, 38, 572, DateTimeKind.Utc).AddTicks(4379));
        }
    }
}
