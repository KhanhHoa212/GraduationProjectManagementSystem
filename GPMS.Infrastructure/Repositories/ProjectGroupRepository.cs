using GPMS.Application.DTOs.Lecturer;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Infrastructure.Repositories;

public class ProjectGroupRepository : IProjectGroupRepository
{
    private readonly GpmsDbContext _context;
    public ProjectGroupRepository(GpmsDbContext context) => _context = context;

    // ─── Entity methods (write-path) ───────────────────────────────────────────

    public async Task<ProjectGroup?> GetByIdAsync(int groupId) =>
        await _context.ProjectGroups
            .AsSplitQuery()
            .Include(pg => pg.Project)
                .ThenInclude(p => p.Semester)
            .Include(pg => pg.Project)
                .ThenInclude(p => p.ProjectSupervisors)
                    .ThenInclude(ps => ps.Lecturer)
            .Include(pg => pg.GroupMembers)
                .ThenInclude(m => m.User)
            .Include(pg => pg.MentorRoundReviews)
                .ThenInclude(mr => mr.ReviewRound)
            .Include(pg => pg.MentorRoundReviews)
                .ThenInclude(mr => mr.Supervisor)
            .Include(pg => pg.ReviewerAssignments)
                .ThenInclude(ra => ra.ReviewRound)
            .Include(pg => pg.ReviewerAssignments)
                .ThenInclude(ra => ra.Reviewer)
            .Include(pg => pg.ReviewSessions)
                .ThenInclude(rs => rs.ReviewRound)
            .Include(pg => pg.ReviewSessions)
                .ThenInclude(rs => rs.Room)
            .Include(pg => pg.Submissions)
                .ThenInclude(s => s.Requirement)
            .Include(pg => pg.Submissions)
                .ThenInclude(s => s.Submitter)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.ReviewRound)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.Reviewer)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.EvaluationDetails)
                    .ThenInclude(ed => ed.Item)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.Feedback)
                    .ThenInclude(f => f!.FeedbackApproval)
            .FirstOrDefaultAsync(pg => pg.GroupID == groupId);

    public async Task<IEnumerable<ProjectGroup>> GetByProjectIdAsync(int projectId) =>
        await _context.ProjectGroups.Where(pg => pg.ProjectID == projectId).ToListAsync();

    public async Task<IEnumerable<ProjectGroup>> GetBySupervisorAsync(string supervisorId) =>
        await _context.ProjectGroups
            .AsSplitQuery()
            .Include(pg => pg.Project)
                .ThenInclude(p => p.Semester)
            .Include(pg => pg.Project)
                .ThenInclude(p => p.ProjectSupervisors)
                    .ThenInclude(ps => ps.Lecturer)
            .Include(pg => pg.GroupMembers)
                .ThenInclude(m => m.User)
            .Include(pg => pg.ReviewSessions)
                .ThenInclude(rs => rs.ReviewRound)
            .Include(pg => pg.ReviewSessions)
                .ThenInclude(rs => rs.Room)
            .Include(pg => pg.Submissions)
                .ThenInclude(s => s.Requirement)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.ReviewRound)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.Reviewer)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.EvaluationDetails)
                    .ThenInclude(ed => ed.Item)
            .Include(pg => pg.Evaluations)
                .ThenInclude(e => e.Feedback)
                    .ThenInclude(f => f!.FeedbackApproval)
            .Where(pg => pg.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId))
            .Distinct()
            .ToListAsync();

    public async Task<ProjectGroup?> GetByProjectIdWithMembersAsync(int projectId) =>
        await _context.ProjectGroups
            .Include(g => g.GroupMembers)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(g => g.ProjectID == projectId);

    public async Task<GroupMember?> GetMemberAsync(int groupId, string userId) =>
        await _context.GroupMembers
            .FirstOrDefaultAsync(m => m.GroupID == groupId && m.UserID == userId);

    public async Task AddAsync(ProjectGroup group) =>
        await _context.ProjectGroups.AddAsync(group);

    public async Task AddMemberAsync(GroupMember member) =>
        await _context.GroupMembers.AddAsync(member);

    public Task RemoveMemberAsync(GroupMember member)
    {
        _context.GroupMembers.Remove(member);
        return Task.CompletedTask;
    }

    public Task UpdateMemberAsync(GroupMember member)
    {
        _context.GroupMembers.Update(member);
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ProjectGroup>> GetAllWithDetailsAsync() =>
        await _context.ProjectGroups
            .Include(g => g.Project)
                .ThenInclude(p => p.ProjectSupervisors)
                    .ThenInclude(ps => ps.Lecturer)
            .Include(g => g.GroupMembers)
                .ThenInclude(m => m.User)
            .ToListAsync();

    public async Task<bool> IsUserInAnyGroupAsync(string userId) =>
        await _context.GroupMembers.AnyAsync(m => m.UserID == userId);

    public async Task<bool> HasUserGraduatedAsync(string userId) =>
        await _context.GroupMembers.AnyAsync(m => m.UserID == userId && m.Status == GraduationStatus.Passed);

    public async Task<ReviewSessionInfo?> GetGroupDefenseSessionAsync(int groupId) =>
        await _context.ReviewSessions
            .AsSplitQuery()
            .Include(s => s.Room)
            .Include(s => s.ReviewRound)
            .Include(s => s.Group)
                .ThenInclude(g => g.ReviewerAssignments)
                    .ThenInclude(ra => ra.Reviewer)
            .Include(s => s.Group)
                .ThenInclude(g => g.Project)
                    .ThenInclude(p => p.ProjectSupervisors)
                        .ThenInclude(ps => ps.Lecturer)
            .OrderByDescending(s => s.ReviewRound.RoundNumber)
            .FirstOrDefaultAsync(s => s.GroupID == groupId);

    public async Task<IEnumerable<ReviewSessionInfo>> GetGroupSchedulesAsync(int groupId) =>
        await _context.ReviewSessions
            .AsSplitQuery()
            .Include(s => s.Room)
            .Include(s => s.ReviewRound)
            .Include(s => s.Group)
                .ThenInclude(g => g.ReviewerAssignments)
                    .ThenInclude(ra => ra.Reviewer)
            .Include(s => s.Group)
                .ThenInclude(g => g.Project)
                    .ThenInclude(p => p.ProjectSupervisors)
                        .ThenInclude(ps => ps.Lecturer)
            .Where(s => s.GroupID == groupId)
            .OrderBy(s => s.ReviewRound.RoundNumber)
            .ToListAsync();

    public async Task SaveChangesAsync() =>
        await _context.SaveChangesAsync();

    // ─── DTO Projection methods (read-only) ────────────────────────────────────

    public async Task<IEnumerable<ProjectGroupSummaryDto>> GetSummariesBySupervisorAsync(string supervisorId) =>
        await _context.ProjectGroups
            .Where(pg => pg.Project.ProjectSupervisors.Any(ps => ps.LecturerID == supervisorId))
            .AsSplitQuery()
            .Select(pg => new ProjectGroupSummaryDto
            {
                GroupId = pg.GroupID,
                GroupName = pg.GroupName,
                ProjectId = pg.ProjectID,
                ProjectName = pg.Project.ProjectName,
                ProjectCode = pg.Project.ProjectCode,
                SemesterId = pg.Project.SemesterID,
                SemesterCode = pg.Project.Semester.SemesterCode,
                SupervisorIds = pg.Project.ProjectSupervisors
                    .Select(ps => ps.LecturerID).ToList(),
                SupervisorRoles = pg.Project.ProjectSupervisors
                    .Select(ps => new SupervisorRoleDto
                    {
                        LecturerId = ps.LecturerID,
                        Role = ps.Role.ToString()
                    }).ToList(),
                MemberNames = pg.GroupMembers
                    .Select(m => m.User.FullName).ToList(),
                Sessions = pg.ReviewSessions
                    .Select(rs => new GroupSessionSummaryDto
                    {
                        ReviewRoundId = rs.ReviewRoundID,
                        RoundNumber = rs.ReviewRound.RoundNumber,
                        RoundType = rs.ReviewRound.RoundType.ToString(),
                        IsOnline = rs.ReviewRound.RoundType == RoundType.Online,
                        ScheduledAt = rs.ScheduledAt,

                        RoomCode = rs.Room != null ? rs.Room.RoomCode : null,
                        Building = rs.Room != null ? rs.Room.Building : null
                    }).ToList(),
                Evaluations = pg.Evaluations
                    .Select(e => new GroupEvaluationSummaryDto
                    {
                        EvaluationId = e.EvaluationID,
                        ReviewRoundId = e.ReviewRoundID,
                        ReviewerId = e.ReviewerID,
                        Status = e.Status.ToString(),
                        SubmittedAt = e.SubmittedAt,
                        FeedbackId = e.Feedback != null ? e.Feedback.FeedbackID : (int?)null,
                        ApprovalStatus = e.Feedback != null && e.Feedback.FeedbackApproval != null
                            ? e.Feedback.FeedbackApproval.ApprovalStatus.ToString()
                            : null
                    }).ToList(),
                Submissions = pg.Submissions
                    .Select(s => new GroupSubmissionSummaryDto
                    {
                        RequirementId = s.RequirementID,
                        ReviewRoundId = s.Requirement.ReviewRoundID,
                        DocumentName = s.Requirement.DocumentName
                    }).ToList()
            })
            .ToListAsync();

    public async Task<LecturerProjectGroupDetailDto?> GetDetailDtoAsync(int groupId) =>
        await _context.ProjectGroups
            .Where(pg => pg.GroupID == groupId)
            .Select(pg => new LecturerProjectGroupDetailDto
            {
                GroupId = pg.GroupID,
                GroupName = pg.GroupName,
                ProjectName = pg.Project.ProjectName,
                ProjectCode = pg.Project.ProjectCode,
                Semester = pg.Project.Semester.SemesterCode,
                SupervisorName = pg.Project.ProjectSupervisors
                    .OrderBy(ps => ps.Role == ProjectRole.Main ? 0 : 1)
                    .Select(ps => ps.Lecturer.FullName)
                    .FirstOrDefault() ?? "N/A",
                Members = pg.GroupMembers
                    .Select(m => new StudentMemberDto
                    {
                        UserId = m.UserID,
                        FullName = m.User.FullName,
                        Email = m.User.Email,
                        RoleInGroup = m.RoleInGroup.ToString(),
                        AvatarUrl = "https://ui-avatars.com/api/?name=" +
                                    Uri.EscapeDataString(m.User.FullName ?? "U") +
                                    "&background=E5E7EB&color=374151"
                    }).ToList(),
                // Milestones & NextMeeting are populated in Service after round data is fetched
                Milestones = new List<MilestoneDetailDto>(),
                NextMeeting = null
            })
            .FirstOrDefaultAsync();
}
