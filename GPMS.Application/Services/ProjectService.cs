using System;
using AutoMapper;
using GPMS.Application.DTOs;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPMS.Application.Services;

public class ProjectService : IProjectService
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProjectGroupRepository _groupRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IEvaluationRepository _evaluationRepository;
    private readonly ISemesterRepository _semesterRepository;
    private readonly IFileService _fileService;
    private readonly IReviewRoundService _reviewRoundService;
    private readonly IMajorRepository _majorRepository;
    private readonly IMapper _mapper;

    public ProjectService(
        IProjectRepository projectRepository,
        IProjectGroupRepository groupRepository,
        IUserRepository userRepository,
        ISubmissionRepository submissionRepository,
        IFeedbackRepository feedbackRepository,
        IEvaluationRepository evaluationRepository,
        ISemesterRepository semesterRepository,
        IFileService fileService,
        IReviewRoundService reviewRoundService,
        IMajorRepository majorRepository,
        IMapper mapper)
    {
        _projectRepository = projectRepository;
        _groupRepository = groupRepository;
        _userRepository = userRepository;
        _submissionRepository = submissionRepository;
        _feedbackRepository = feedbackRepository;
        _evaluationRepository = evaluationRepository;
        _semesterRepository = semesterRepository;
        _fileService = fileService;
        _reviewRoundService = reviewRoundService;
        _majorRepository = majorRepository;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProjectDto>> GetAllProjectsAsync()
    {
        var projects = await _projectRepository.GetAllWithDetailsAsync();
        return _mapper.Map<IEnumerable<ProjectDto>>(projects);
    }

    public async Task<IEnumerable<ProjectDto>> GetProjectsBySemesterAsync(int semesterId)
    {
        var projects = await _projectRepository.GetBySemesterWithDetailsAsync(semesterId);
        return _mapper.Map<IEnumerable<ProjectDto>>(projects);
    }

    public async Task<ProjectDto?> GetProjectByStudentAsync(string studentId)
    {
        var project = await _projectRepository.GetProjectByStudentIdAsync(studentId);
        if (project == null) return null;

        var dto = _mapper.Map<ProjectDto>(project);
        
        // Filter members to only show the user's group
        var userGroup = project.ProjectGroups.FirstOrDefault(g => g.GroupMembers.Any(m => m.UserID == studentId));
        if (userGroup != null)
        {
            dto.Members = userGroup.GroupMembers.Select(m => new ProjectMemberDto
            {
                UserID = m.UserID,
                FullName = m.User?.FullName ?? string.Empty,
                RoleInGroup = m.RoleInGroup,
                GroupName = userGroup.GroupName
            }).ToList();
        }
        else
        {
            dto.Members = new List<ProjectMemberDto>();
        }

        return dto;
    }

    public async Task<IEnumerable<SubmissionItemDto>> GetDashboardSubmissionsAsync(string studentId)
    {
        var submissions = await _submissionRepository.GetActiveSubmissionsByStudentAsync(studentId);
        
        return submissions.Select(s => new SubmissionItemDto
        {
            RequirementID = s.Requirement.RequirementID,
            DocumentName = s.Requirement.DocumentName,
            Description = s.Requirement.Description,
            RoundNumber = s.Requirement.ReviewRound?.RoundNumber ?? 0,
            Deadline = s.Requirement.Deadline,
            
            SubmissionID = s.Submission?.SubmissionID,
            FileName = s.Submission?.FileName,
            SubmittedAt = s.Submission?.SubmittedAt,
            Status = s.Submission?.Status,
            FileUrl = s.Submission?.FileUrl,
            Version = s.Submission?.Version,
            AllowedFormats = s.Requirement.AllowedFormats,
            MaxFileSizeMB = s.Requirement.MaxFileSizeMB
        }).ToList();
    }

    public async Task<IEnumerable<SubmissionItemDto>> GetSubmissionsByStudentAsync(string studentId)
    {
        var submissions = await _submissionRepository.GetAllSubmissionsByStudentAsync(studentId);
        
        return submissions.Select(s => new SubmissionItemDto
        {
            RequirementID = s.Requirement.RequirementID,
            DocumentName = s.Requirement.DocumentName,
            Description = s.Requirement.Description,
            RoundNumber = s.Requirement.ReviewRound?.RoundNumber ?? 0,
            Deadline = s.Requirement.Deadline,
            
            SubmissionID = s.Submission?.SubmissionID,
            FileName = s.Submission?.FileName,
            SubmittedAt = s.Submission?.SubmittedAt,
            Status = s.Submission?.Status,
            FileUrl = s.Submission?.FileUrl,
            Version = s.Submission?.Version,
            AllowedFormats = s.Requirement.AllowedFormats,
            MaxFileSizeMB = s.Requirement.MaxFileSizeMB
        }).ToList();
    }

    public async Task<IEnumerable<DashboardFeedbackDto>> GetDashboardFeedbacksAsync(string studentId, int count = 5)
    {
        var feedbacks = await _feedbackRepository.GetRecentFeedbacksByStudentAsync(studentId, count);

        return feedbacks.Select(f => new DashboardFeedbackDto
        {
            FeedbackID = f.FeedbackID,
            ReviewerName = f.Evaluation.Reviewer.FullName,
            Content = f.Content,
            CreatedAt = f.CreatedAt
        }).ToList();
    }

    public async Task<ProjectDetailDto?> GetProjectDetailAsync(int projectId)
    {
        var project = await _projectRepository.GetDetailAsync(projectId);
        if (project == null) return null;

        var dto = _mapper.Map<ProjectDetailDto>(project);
        
        // Populate Review Rounds for the semester
        dto.ReviewRounds = (await _reviewRoundService.GetReviewRoundsBySemesterAsync(dto.SemesterID)).ToList();

        return dto;
    }

    public async Task CreateProjectAsync(CreateProjectDto dto)
    {
        string code = dto.ProjectCode;
        if (string.IsNullOrWhiteSpace(code))
        {
            var allProjects = await _projectRepository.GetAllAsync();
            var count = allProjects.Count();
            code = $"GP-{dto.SemesterID}-{count + 1:D3}";
        }

        var project = new Project
        {
            ProjectCode = code,
            ProjectName = dto.ProjectName,
            Description = dto.Description,
            SemesterID = dto.SemesterID,
            MajorID = dto.MajorID,
            Status = dto.Status,
            CreatedAt = DateTime.UtcNow
        };

        await _projectRepository.AddAsync(project);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task UpdateProjectAsync(UpdateProjectDto dto)
    {
        var project = await _projectRepository.GetByIdAsync(dto.ProjectID);
        if (project == null) return;

        project.ProjectName = dto.ProjectName;
        project.Description = dto.Description;
        project.Status = dto.Status;

        await _projectRepository.UpdateAsync(project);
        await _projectRepository.SaveChangesAsync();
    }

    public async Task<SupervisorAssignmentDto> GetSupervisorAssignmentDataAsync(int? semesterId)
    {
        var allProjects = semesterId.HasValue
            ? await _projectRepository.GetBySemesterWithDetailsAsync(semesterId.Value)
            : await _projectRepository.GetAllWithDetailsAsync();

        var lecturers = await _userRepository.GetByRoleAsync(RoleName.Lecturer);

        var dto = new SupervisorAssignmentDto
        {
            UnassignedProjects = _mapper.Map<List<ProjectDto>>(allProjects.Where(p => !p.ProjectSupervisors.Any())),
            AssignedProjects = _mapper.Map<List<ProjectDto>>(allProjects.Where(p => p.ProjectSupervisors.Any())),
            Lecturers = lecturers.Select(l => {
                var primaryExpertise = l.LecturerExpertises.OrderByDescending(le => le.IsPrimary).FirstOrDefault();
                return new LecturerWorkloadDto
                {
                    LecturerID = l.UserID,
                    FullName = l.FullName,
                    Level = primaryExpertise?.Level.ToString() ?? "Basic",
                    Specialty = primaryExpertise?.Expertise?.ExpertiseName ?? "General",
                    CurrentWorkload = allProjects.Count(p => p.ProjectSupervisors.Any(ps => ps.LecturerID == l.UserID)),
                    MaxWorkload = 5
                };
            }).ToList()
        };

        return dto;
    }

    public async Task<(bool success, string message)> AssignSupervisorAsync(int projectId, string lecturerId, string? assignedBy)
    {
        var project = await _projectRepository.GetDetailAsync(projectId);
        if (project == null) return (false, "Project not found.");

        var lecturer = await _userRepository.GetByIdAsync(lecturerId);
        if (lecturer == null) return (false, "Lecturer not found.");

        // Check if lecturer is actually a lecturer
        if (!lecturer.UserRoles.Any(r => r.RoleName == RoleName.Lecturer))
            return (false, "User is not a lecturer.");

        // Remove existing main supervisors
        var existingMain = project.ProjectSupervisors.Where(ps => ps.Role == ProjectRole.Main).ToList();
        foreach (var ps in existingMain)
        {
            project.ProjectSupervisors.Remove(ps);
        }

        // Add new main supervisor
        project.ProjectSupervisors.Add(new ProjectSupervisor
        {
            ProjectID = projectId,
            LecturerID = lecturerId,
            Role = ProjectRole.Main,
            AssignedAt = DateTime.UtcNow,
            AssignedBy = assignedBy
        });

        await _projectRepository.UpdateAsync(project);
        await _projectRepository.SaveChangesAsync();

        return (true, "Supervisor assigned successfully.");
    }

    public async Task<(int total, int withGroup, int missingSupervisor, int missingMembers)> GetDashboardStatsAsync(int? semesterId = null)
    {
        var projects = semesterId.HasValue
            ? (await _projectRepository.GetBySemesterWithDetailsAsync(semesterId.Value)).ToList()
            : (await _projectRepository.GetAllWithDetailsAsync()).ToList();

        var total = projects.Count;
        var withGroup = projects.Count(p => p.ProjectGroups.Any());
        var missingSupervisor = projects.Count(p => !p.ProjectSupervisors.Any());
        var missingMembers = projects.Count(p => p.ProjectGroups.Any(g => !g.GroupMembers.Any()));

        return (total, withGroup, missingSupervisor, missingMembers);
    }

    // ============================================================
    // Member Management
    // ============================================================

    public async Task<IEnumerable<StudentSearchDto>> SearchStudentsAsync(string query)
    {
        var students = await _userRepository.GetByRoleAsync(RoleName.Student);
        var filtered = students
            .Where(u => u.UserID.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || u.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)
                     || (u.Email != null && u.Email.Contains(query, StringComparison.OrdinalIgnoreCase)))
            .Take(10);
            
        return _mapper.Map<IEnumerable<StudentSearchDto>>(filtered).ToList();
    }

    public async Task<(bool success, string message)> AddMemberAsync(int projectId, string userId)
    {
        var student = await _userRepository.GetByIdAsync(userId);
        if (student == null)
            return (false, "Student not found.");

        if (await _groupRepository.IsUserInAnyGroupAsync(userId))
            return (false, "Student is already assigned to a project group.");

        var group = await _groupRepository.GetByProjectIdWithMembersAsync(projectId);
        if (group == null)
        {
            // Auto-create default group
            group = new ProjectGroup
            {
                ProjectID = projectId,
                GroupName = "Group 1",
                CreatedAt = DateTime.UtcNow
            };
            await _groupRepository.AddAsync(group);
            await _groupRepository.SaveChangesAsync();
            group = await _groupRepository.GetByProjectIdWithMembersAsync(projectId);
            if (group == null) return (false, "Failed to create group.");
        }

        if (group.GroupMembers.Any(m => m.UserID == userId))
            return (false, "Student is already a member of this group.");

        var role = group.GroupMembers.Any() ? GroupRole.Member : GroupRole.Leader;

        await _groupRepository.AddMemberAsync(new GroupMember
        {
            GroupID = group.GroupID,
            UserID = userId,
            RoleInGroup = role,
            JoinedAt = DateTime.UtcNow
        });
        await _groupRepository.SaveChangesAsync();

        return (true, $"Student added as {role}.");
    }

    public async Task<bool> RemoveMemberAsync(int projectId, string userId)
    {
        var group = await _groupRepository.GetByProjectIdWithMembersAsync(projectId);
        if (group == null) return false;

        var member = group.GroupMembers.FirstOrDefault(m => m.UserID == userId);
        if (member == null) return false;

        await _groupRepository.RemoveMemberAsync(member);
        await _groupRepository.SaveChangesAsync();
        return true;
    }

    public async Task<(bool success, string message)> UpdateMemberRoleAsync(int projectId, string userId, string role)
    {
        if (!Enum.TryParse<GroupRole>(role, out var newRole))
            return (false, "Invalid role.");

        var group = await _groupRepository.GetByProjectIdWithMembersAsync(projectId);
        if (group == null) return (false, "Group not found.");

        var member = group.GroupMembers.FirstOrDefault(m => m.UserID == userId);
        if (member == null) return (false, "Member not found.");

        if (newRole == GroupRole.Leader)
        {
            var existingLeader = group.GroupMembers
                .FirstOrDefault(m => m.RoleInGroup == GroupRole.Leader && m.UserID != userId);
            if (existingLeader != null)
                return (false, $"There is already a Leader in this group. Please demote them first.");
        }

        member.RoleInGroup = newRole;
        await _groupRepository.UpdateMemberAsync(member);
        await _groupRepository.SaveChangesAsync();

        return (true, "Role updated successfully.");
    }

    public async Task<(bool success, string message)> SubmitProjectWorkAsync(string studentId, int requirementId, Microsoft.AspNetCore.Http.IFormFile file)
    {
        // 1. Get Student's project and group
        var project = await _projectRepository.GetProjectByStudentIdAsync(studentId);
        if (project == null) return (false, "Project not found for this student.");

        var group = project.ProjectGroups.FirstOrDefault(g => g.GroupMembers.Any(m => m.UserID == studentId));
        if (group == null) return (false, "You are not assigned to any group.");

        var semester = await _semesterRepository.GetByIdAsync(project.SemesterID);
        var semesterName = semester?.SemesterCode ?? "UnknownSemester";
        var groupName = group.GroupName.Replace(" ", "_");

        // 2. Build folder path: semesters/{semesterName}/groups/{groupName}
        string folderPath = $"semesters/{semesterName}/groups/{groupName}";

        // 3. Upload to Cloudinary
        string fileUrl;
        try
        {
            fileUrl = await _fileService.UploadFileAsync(file, folderPath);
        }
        catch (Exception ex)
        {
            return (false, $"Upload failed: {ex.Message}");
        }

        if (string.IsNullOrEmpty(fileUrl))
            return (false, "Failed to get file URL from Cloudinary.");

        // 4. Save to Database
        var requirement = await _submissionRepository.GetRequirementByIdAsync(requirementId);
        if (requirement == null) return (false, "Submission requirement not found.");

        // Determine status based on deadline
        var status = DateTime.UtcNow <= requirement.Deadline ? SubmissionStatus.OnTime : SubmissionStatus.Late;

        var existingSubmissions = await _submissionRepository.GetByGroupAndRequirementAsync(group.GroupID, requirementId);
        var submission = existingSubmissions.OrderByDescending(s => s.Version).FirstOrDefault();

        if (submission != null)
        {
            // Delete old file from Cloudinary
            try
            {
                var oldPublicId = $"{folderPath}/{submission.FileName}";
                await _fileService.DeleteFileAsync(oldPublicId);
            }
            catch (Exception ex)
            {
                // Log warning but continue, maybe the file was already deleted manually
            }

            submission.FileUrl = fileUrl;
            submission.FileName = file.FileName;
            submission.FileSizeMB = (decimal)file.Length / (1024 * 1024);
            submission.SubmittedAt = DateTime.UtcNow;
            submission.SubmittedBy = studentId;
            submission.Status = status;
            submission.Version += 1;
        }
        else
        {
            submission = new Submission
            {
                RequirementID = requirementId,
                GroupID = group.GroupID,
                FileUrl = fileUrl,
                FileName = file.FileName,
                FileSizeMB = (decimal)file.Length / (1024 * 1024),
                SubmittedAt = DateTime.UtcNow,
                SubmittedBy = studentId,
                Status = status,
                Version = 1
            };
            await _submissionRepository.AddAsync(submission);
        }

        await _submissionRepository.SaveChangesAsync();

        return (true, $"File uploaded and submitted successfully as {status}. Version: {submission.Version}");
    }

    public async Task<StudentFeedbackDto> GetStudentFeedbackAsync(string studentId, int? roundId)
    {
        var result = new StudentFeedbackDto();

        // 1. Get Student's project and group
        var project = await _projectRepository.GetProjectByStudentIdAsync(studentId);
        if (project == null) return result;

        var group = project.ProjectGroups.FirstOrDefault(g => g.GroupMembers.Any(m => m.UserID == studentId));
        if (group == null) return result;

        // 2. Get all rounds for this semester
        var rounds = await _semesterRepository.GetRoundsBySemesterAsync(project.SemesterID);
        var roundList = rounds.Where(r => r.Status == RoundStatus.Completed).OrderBy(r => r.RoundNumber).ToList();

        // 3. Get all evaluations for the group
        var evaluations = await _evaluationRepository.GetByGroupWithDetailsAsync(group.GroupID);
        var evaluationList = evaluations.ToList();

        // 4. Map Rounds
        result.Rounds = roundList.Select(r => new FeedbackRoundDto
        {
            ReviewRoundId = r.ReviewRoundID,
            RoundNumber = r.RoundNumber,
            Title = r.RoundNumber == 0 ? "Defense" : $"Round {r.RoundNumber}",
            Status = r.Status.ToString(),
            IsSelected = roundId.HasValue ? r.ReviewRoundID == roundId : false
        }).ToList();

        // 5. Determine default selected round (latest completed or ongoing)
        if (!roundId.HasValue)
        {
            var defaultRound = roundList
                .Where(r => evaluationList.Any(e => e.ReviewRoundID == r.ReviewRoundID))
                .OrderByDescending(r => r.RoundNumber)
                .FirstOrDefault();

            if (defaultRound != null)
            {
                result.SelectedRoundId = defaultRound.ReviewRoundID;
                var roundDto = result.Rounds.FirstOrDefault(r => r.ReviewRoundId == defaultRound.ReviewRoundID);
                if (roundDto != null) roundDto.IsSelected = true;
            }
            else if (roundList.Any())
            {
                result.SelectedRoundId = roundList.First().ReviewRoundID;
                result.Rounds.First().IsSelected = true;
            }
        }
        else
        {
            result.SelectedRoundId = roundId;
        }

        // 6. Map Details for selected round
        var selectedEval = evaluationList.FirstOrDefault(e => e.ReviewRoundID == result.SelectedRoundId);
        if (selectedEval != null)
        {
            result.Summary = new FeedbackSummaryDto
            {
                StatusText = $"Submitted by {selectedEval.Reviewer.FullName}",
                UpdatedAt = selectedEval.SubmittedAt
            };

            result.Details = selectedEval.EvaluationDetails.Select(d => new FeedbackCriteriaDto
            {
                ItemCode = d.Item.ItemCode,
                Title = d.Item.ItemContent,
                Assessment = d.Assessment,
                Comment = d.Comment
            }).ToList();

            result.OverallFeedback = selectedEval.OverallComment;
        }

        return result;
    }

    public async Task<IEnumerable<ProjectDefenseScheduleDto>> GetProjectDefenseScheduleAsync(string studentId)
    {
        var project = await _projectRepository.GetProjectByStudentIdAsync(studentId);
        if (project == null) return Enumerable.Empty<ProjectDefenseScheduleDto>();
 
        var group = project.ProjectGroups.FirstOrDefault(g => g.GroupMembers.Any(m => m.UserID == studentId));
        if (group == null) return Enumerable.Empty<ProjectDefenseScheduleDto>();
 
        var sessions = await _groupRepository.GetGroupSchedulesAsync(group.GroupID);
        var result = new List<ProjectDefenseScheduleDto>();
 
        foreach (var session in sessions)
        {
            var dto = new ProjectDefenseScheduleDto
            {
                RoundNumber = session.ReviewRound.RoundNumber,
                RoundName = session.ReviewRound.RoundNumber == 0 ? "Final Defense" : $"Round {session.ReviewRound.RoundNumber}",
                ScheduledAt = session.ScheduledAt,
                RoomName = session.Room?.RoomCode,
                Building = session.Room?.Building,
                Notes = session.Notes,
                MeetLink = session.MeetLink,
                LocationDetails = !string.IsNullOrEmpty(session.MeetLink) ? "Online" : "Campus Room"
            };
 
            // Get committee members
            var reviewers = session.Group.ReviewerAssignments
                .Where(ra => ra.ReviewRoundID == session.ReviewRoundID)
                .Select(ra => new CommitteeMemberDto
                {
                    FullName = ra.Reviewer.FullName,
                    UserID = ra.Reviewer.UserID,
                    Role = "Reviewer",
                    AvatarUrl = ra.Reviewer.AvatarUrl
                }).ToList();
 
            if (reviewers.Any()) reviewers[0].Role = "Chairperson";
            dto.CommitteeMembers.AddRange(reviewers);
 
            var supervisor = session.Group.Project.ProjectSupervisors
                .FirstOrDefault(ps => ps.Role == ProjectRole.Main);
            
            if (supervisor != null)
            {
                dto.CommitteeMembers.Add(new CommitteeMemberDto
                {
                    FullName = supervisor.Lecturer.FullName,
                    UserID = supervisor.Lecturer.UserID,
                    Role = "Supervisor",
                    AvatarUrl = supervisor.Lecturer.AvatarUrl
                });
            }
 
            result.Add(dto);
        }
 
        return result;
    }

    public async Task<(int successCount, string message)> BulkImportProjectsAsync(IEnumerable<ProjectImportRowDto> projects, int semesterId, string? requestedBy)
    {
        int count = 0;
        var majors = await _majorRepository.GetAllAsync();
        var lecturers = await _userRepository.GetByRoleAsync(RoleName.Lecturer);
        var students = await _userRepository.GetByRoleAsync(RoleName.Student);
        
        foreach (var row in projects)
        {
            try
            {
                var major = majors.FirstOrDefault(m => m.MajorName.Equals(row.MajorName, StringComparison.OrdinalIgnoreCase));
                var lecturer = lecturers.FirstOrDefault(l => l.Email?.Equals(row.SupervisorEmail, StringComparison.OrdinalIgnoreCase) == true);
                
                if (major == null || lecturer == null) continue;

                // 1. Create Project
                var project = new Project
                {
                    ProjectCode = row.ProjectCode,
                    ProjectName = row.ProjectName,
                    Description = row.Description,
                    SemesterID = semesterId,
                    MajorID = major.MajorID,
                    Status = ProjectStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };
                await _projectRepository.AddAsync(project);
                await _projectRepository.SaveChangesAsync();

                // 2. Assign Supervisor (using internal entities for speed in loop)
                project.ProjectSupervisors.Add(new ProjectSupervisor
                {
                    ProjectID = project.ProjectID,
                    LecturerID = lecturer.UserID,
                    Role = ProjectRole.Main,
                    AssignedAt = DateTime.UtcNow,
                    AssignedBy = requestedBy
                });

                // 3. Create Default Group
                var group = new ProjectGroup
                {
                    ProjectID = project.ProjectID,
                    GroupName = "Group 1",
                    CreatedAt = DateTime.UtcNow
                };
                await _groupRepository.AddAsync(group);
                await _groupRepository.SaveChangesAsync();

                // 4. Add Members
                for (int i = 0; i < row.StudentEmails.Count; i++)
                {
                    var mssv = row.StudentEmails[i];
                    var student = students.FirstOrDefault(s => s.UserID == mssv);
                    if (student != null)
                    {
                        await _groupRepository.AddMemberAsync(new GroupMember
                        {
                            GroupID = group.GroupID,
                            UserID = student.UserID,
                            RoleInGroup = i == 0 ? GroupRole.Leader : GroupRole.Member,
                            JoinedAt = DateTime.UtcNow
                        });
                    }
                }

                await _groupRepository.SaveChangesAsync();
                count++;
            }
            catch (Exception)
            {
                // Silently skip if error occurs for one row
            }
        }

        return (count, $"Đã nhập thành công {count} dự án.");
    }
}
