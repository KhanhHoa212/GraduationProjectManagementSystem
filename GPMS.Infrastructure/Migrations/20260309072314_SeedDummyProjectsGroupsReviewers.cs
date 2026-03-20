using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GPMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDummyProjectsGroupsReviewers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Projects",
                columns: new[] { "ProjectID", "CreatedAt", "Description", "MajorID", "ProjectCode", "ProjectName", "SemesterID", "Status" },
                values: new object[,]
                {
                    { 100, new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(733), null, 1, "PRJ-01", "AI Traffic Analyzer", 1, "Active" },
                    { 101, new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(738), null, 1, "PRJ-02", "Smart Healthcare System", 1, "Active" }
                });

            migrationBuilder.InsertData(
                table: "ReviewRounds",
                columns: new[] { "ReviewRoundID", "Description", "EndDate", "RoundNumber", "RoundType", "SemesterID", "StartDate", "Status", "SubmissionDeadline" },
                values: new object[] { 1, null, new DateTime(2026, 3, 19, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1102), 1, "Online", 1, new DateTime(2026, 2, 27, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1092), "Ongoing", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.InsertData(
                table: "ReviewRounds",
                columns: new[] { "ReviewRoundID", "Description", "EndDate", "RoundNumber", "RoundType", "SemesterID", "StartDate", "SubmissionDeadline" },
                values: new object[] { 2, null, new DateTime(2026, 4, 8, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1107), 3, "Offline", 1, new DateTime(2026, 3, 29, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1106), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 1,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(659));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 2,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(662));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 3,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(663));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 4,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(664));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 5,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(666));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 6,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(667));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 7,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(668));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 8,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(669));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 9,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(670));

            migrationBuilder.UpdateData(
                table: "UserRoles",
                keyColumn: "UserRoleID",
                keyValue: 10,
                column: "AssignedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(665));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "ADMIN001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(437));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(467));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(470));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "GV003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(474));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "HOD001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(480));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180001",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(489));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180002",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(493));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180003",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(562));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180004",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(567));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: "SE180005",
                column: "CreatedAt",
                value: new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(570));

            migrationBuilder.InsertData(
                table: "ProjectGroups",
                columns: new[] { "GroupID", "CreatedAt", "GroupName", "ProjectID" },
                values: new object[,]
                {
                    { 100, new DateTime(2026, 3, 9, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(810), "Group 1", 100 },
                    { 101, new DateTime(2026, 3, 9, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(830), "Group 2", 101 }
                });

            migrationBuilder.InsertData(
                table: "ProjectSupervisors",
                columns: new[] { "LecturerID", "ProjectID", "AssignedAt", "AssignedBy", "Role" },
                values: new object[,]
                {
                    { "GV001", 100, new DateTime(2026, 3, 9, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1022), null, "Main" },
                    { "GV002", 101, new DateTime(2026, 3, 9, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1025), null, "Main" }
                });

            migrationBuilder.InsertData(
                table: "ReviewChecklists",
                columns: new[] { "ChecklistID", "CreatedAt", "CreatedBy", "Description", "ReviewRoundID", "Title" },
                values: new object[] { 1, new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(1165), null, "Evaluate early stage architecture", 1, "Cross Review 1 Checklist" });

            migrationBuilder.InsertData(
                table: "ChecklistItems",
                columns: new[] { "ItemID", "ChecklistID", "ItemCode", "ItemContent", "MaxScore", "OrderIndex", "Weight" },
                values: new object[,]
                {
                    { 1, 1, "ARCH-01", "Is the architecture solid?", 5m, 1, 50m },
                    { 2, 1, "CODE-01", "Code quality for MVP", 5m, 2, 50m }
                });

            migrationBuilder.InsertData(
                table: "Evaluations",
                columns: new[] { "EvaluationID", "GroupID", "ReviewRoundID", "ReviewerID", "Status", "SubmittedAt", "TotalScore" },
                values: new object[] { 1, 101, 1, "GV002", "Submitted", new DateTime(2026, 3, 7, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1489), 8.5m });

            migrationBuilder.InsertData(
                table: "GroupMembers",
                columns: new[] { "GroupID", "UserID", "JoinedAt", "RoleInGroup" },
                values: new object[,]
                {
                    { 100, "SE180001", new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(907), "Member" },
                    { 100, "SE180002", new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(909), "Member" },
                    { 101, "SE180003", new DateTime(2026, 3, 9, 7, 23, 13, 81, DateTimeKind.Utc).AddTicks(911), "Member" }
                });

            migrationBuilder.InsertData(
                table: "ReviewSessionInfo",
                columns: new[] { "SessionID", "GroupID", "MeetLink", "Notes", "ReviewRoundID", "RoomID", "ScheduledAt" },
                values: new object[,]
                {
                    { 1, 101, "https://meet.google.com/abc-defg-hij", null, 1, 1, new DateTime(2026, 3, 10, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1286) },
                    { 2, 100, "https://meet.google.com/xyz-uvw-qrs", null, 1, 2, new DateTime(2026, 3, 14, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1293) }
                });

            migrationBuilder.InsertData(
                table: "ReviewerAssignments",
                columns: new[] { "AssignmentID", "AssignedAt", "AssignedBy", "GroupID", "IsRandom", "ReviewRoundID", "ReviewerID" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 3, 9, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1359), null, 101, true, 1, "GV001" },
                    { 2, new DateTime(2026, 3, 9, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1423), null, 100, true, 2, "GV001" }
                });

            migrationBuilder.InsertData(
                table: "Feedbacks",
                columns: new[] { "FeedbackID", "Content", "CreatedAt", "EvaluationID" },
                values: new object[] { 1, "Great architecture. Code needs more comments.", new DateTime(2026, 3, 7, 14, 23, 13, 81, DateTimeKind.Local).AddTicks(1544), 1 });

            migrationBuilder.InsertData(
                table: "FeedbackApprovals",
                columns: new[] { "FeedbackID", "ApprovedAt", "AutoReleasedAt", "SupervisorComment", "SupervisorID" },
                values: new object[] { 1, null, null, "", "GV002" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ChecklistItems",
                keyColumn: "ItemID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ChecklistItems",
                keyColumn: "ItemID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "FeedbackApprovals",
                keyColumn: "FeedbackID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "GroupMembers",
                keyColumns: new[] { "GroupID", "UserID" },
                keyValues: new object[] { 100, "SE180001" });

            migrationBuilder.DeleteData(
                table: "GroupMembers",
                keyColumns: new[] { "GroupID", "UserID" },
                keyValues: new object[] { 100, "SE180002" });

            migrationBuilder.DeleteData(
                table: "GroupMembers",
                keyColumns: new[] { "GroupID", "UserID" },
                keyValues: new object[] { 101, "SE180003" });

            migrationBuilder.DeleteData(
                table: "ProjectSupervisors",
                keyColumns: new[] { "LecturerID", "ProjectID" },
                keyValues: new object[] { "GV001", 100 });

            migrationBuilder.DeleteData(
                table: "ProjectSupervisors",
                keyColumns: new[] { "LecturerID", "ProjectID" },
                keyValues: new object[] { "GV002", 101 });

            migrationBuilder.DeleteData(
                table: "ReviewSessionInfo",
                keyColumn: "SessionID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ReviewSessionInfo",
                keyColumn: "SessionID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "ReviewerAssignments",
                keyColumn: "AssignmentID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ReviewerAssignments",
                keyColumn: "AssignmentID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Feedbacks",
                keyColumn: "FeedbackID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ProjectGroups",
                keyColumn: "GroupID",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "ReviewChecklists",
                keyColumn: "ChecklistID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "ReviewRounds",
                keyColumn: "ReviewRoundID",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Evaluations",
                keyColumn: "EvaluationID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "ProjectID",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "ProjectGroups",
                keyColumn: "GroupID",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "ReviewRounds",
                keyColumn: "ReviewRoundID",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Projects",
                keyColumn: "ProjectID",
                keyValue: 101);

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
    }
}
