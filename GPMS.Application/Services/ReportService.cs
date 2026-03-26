using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class ReportService : IReportService
{
    private readonly IProjectRepository _projectRepo;
    private readonly IReviewRoundRepository _reviewRoundRepo;
    private readonly ISubmissionRepository _submissionRepo;
    private readonly IGroupRoundProgressRepository _groupProgressRepo;

    public ReportService(
        IProjectRepository projectRepo,
        IReviewRoundRepository reviewRoundRepo,
        ISubmissionRepository submissionRepo,
        IGroupRoundProgressRepository groupProgressRepo)
    {
        _projectRepo = projectRepo;
        _reviewRoundRepo = reviewRoundRepo;
        _submissionRepo = submissionRepo;
        _groupProgressRepo = groupProgressRepo;
    }

    public async Task<HODReportDto> GetHODReportAsync(int? semesterId)
    {
        var activeProjects = semesterId.HasValue 
            ? (await _projectRepo.GetBySemesterWithDetailsAsync(semesterId.Value)).ToList()
            : (await _projectRepo.GetAllWithDetailsAsync()).ToList();

        var reviewRounds = semesterId.HasValue
            ? (await _reviewRoundRepo.GetBySemesterWithRequirementsAsync(semesterId.Value)).ToList()
            : (await _reviewRoundRepo.GetAllWithRequirementsAsync()).ToList();
        
        var groupIds = activeProjects.SelectMany(p => p.ProjectGroups).Select(g => g.GroupID).ToList();
        
        var submissions = groupIds.Any() ? 
            (await _submissionRepo.GetByGroupIdsAsync(groupIds)).ToList() : 
            new List<Domain.Entities.Submission>();

        var roundIds = reviewRounds.Select(r => r.ReviewRoundID).ToList();
        var mentorProgress = (roundIds.Any() && groupIds.Any()) ? 
            (await _groupProgressRepo.GetByReviewRoundIdsAndGroupIdsAsync(roundIds, groupIds)).ToList() : 
            new List<Domain.Entities.GroupRoundProgress>();

        var report = new HODReportDto
        {
            TotalProjects = activeProjects.Count,
            TotalGroups = activeProjects.SelectMany(p => p.ProjectGroups).Count(),
            TotalStudents = activeProjects.SelectMany(p => p.ProjectGroups)
                .SelectMany(g => g.GroupMembers)
                .Select(m => m.UserID)
                .Distinct().Count(),
            TotalSupervisors = activeProjects.SelectMany(p => p.ProjectSupervisors)
                .Where(ps => ps.Role == ProjectRole.Main)
                .Select(ps => ps.LecturerID)
                .Distinct().Count(),

            DraftProjects = activeProjects.Count(p => p.Status == ProjectStatus.Draft),
            ActiveProjects = activeProjects.Count(p => p.Status == ProjectStatus.Active),
            CompletedProjects = activeProjects.Count(p => p.Status == ProjectStatus.Completed),
            CancelledProjects = activeProjects.Count(p => p.Status == ProjectStatus.Cancelled),

            MajorDistribution = activeProjects
                .GroupBy(p => p.Major?.MajorName ?? "Unknown")
                .Select(g => new MajorDistributionDto
                {
                    MajorName = g.Key,
                    ProjectCount = g.Count()
                })
                .OrderByDescending(m => m.ProjectCount)
                .ToList(),

            RoundSubmissionStats = reviewRounds.Select(r =>
            {
                var reqIds = r.SubmissionRequirements.Select(req => req.RequirementID).ToList();
                var roundSubmissions = submissions.Where(s => reqIds.Contains(s.RequirementID)).ToList();
                var totalExpected = groupIds.Count * r.SubmissionRequirements.Count;

                return new RoundSubmissionStatDto
                {
                    RoundNumber = r.RoundNumber,
                    RoundDescription = r.Description ?? $"Review {r.RoundNumber}",
                    TotalRequired = totalExpected,
                    OnTimeCount = roundSubmissions.Count(s => s.Status == SubmissionStatus.OnTime),
                    LateCount = roundSubmissions.Count(s => s.Status == SubmissionStatus.Late),
                    NotSubmittedCount = Math.Max(0, totalExpected - roundSubmissions.Count(s => s.Status != SubmissionStatus.Replaced))
                };
            }).ToList(),

            SupervisorWorkloads = activeProjects
                .SelectMany(p => p.ProjectSupervisors.Where(ps => ps.Role == ProjectRole.Main)
                    .Select(ps => new { ps.LecturerID, LecturerName = ps.Lecturer?.FullName ?? ps.LecturerID, Project = p }))
                .GroupBy(x => new { x.LecturerID, x.LecturerName })
                .Select(g => new SupervisorWorkloadDto
                {
                    LecturerName = g.Key.LecturerName,
                    ProjectCount = g.Count(),
                    GroupCount = g.SelectMany(x => x.Project.ProjectGroups).Count(),
                    StudentCount = g.SelectMany(x => x.Project.ProjectGroups)
                        .SelectMany(gr => gr.GroupMembers)
                        .Select(m => m.UserID).Distinct().Count()
                })
                .OrderByDescending(s => s.ProjectCount)
                .ToList(),

            RoundMentorStats = reviewRounds.Select(r =>
            {
                var rp = mentorProgress.Where(m => m.ReviewRoundID == r.ReviewRoundID).ToList();
                return new RoundMentorDecisionStatDto
                {
                    RoundNumber = r.RoundNumber,
                    AcceptedCount = rp.Count(m => m.MentorDecision == MentorDecision.Accepted),
                    RejectedCount = rp.Count(m => m.MentorDecision == MentorDecision.Rejected),
                    PendingCount = rp.Count(m => m.MentorDecision == MentorDecision.Pending),
                    StoppedCount = rp.Count(m => m.MentorDecision == MentorDecision.Stopped)
                };
            }).ToList()
        };

        return report;
    }
}
