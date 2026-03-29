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
    private readonly ISubmissionAccessService _submissionAccessService;
    private readonly System.Net.Http.IHttpClientFactory _httpClientFactory;
    private readonly IReviewerAssignmentRepository _assignmentRepository;
    private readonly IRoomRepository _roomRepository;
    private readonly IReviewSessionRepository _sessionRepository;
    private readonly ICommitteeRepository _committeeRepository;

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
        IMapper mapper,
        ISubmissionAccessService submissionAccessService,
        System.Net.Http.IHttpClientFactory httpClientFactory,
        IReviewerAssignmentRepository assignmentRepository,
        IRoomRepository roomRepository,
        IReviewSessionRepository sessionRepository,
        ICommitteeRepository committeeRepository)
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
        _submissionAccessService = submissionAccessService;
        _httpClientFactory = httpClientFactory;
        _assignmentRepository = assignmentRepository;
        _roomRepository = roomRepository;
        _sessionRepository = sessionRepository;
        _committeeRepository = committeeRepository;
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

    public async Task<IEnumerable<ProjectDto>> GetRecentProjectsForDashboardAsync(int semesterId, int count)
    {
        var projects = await _projectRepository.GetDashboardProjectsAsync(semesterId, count);
        return _mapper.Map<IEnumerable<ProjectDto>>(projects);
    }

    public async Task<IEnumerable<ProjectDto>> GetFilteredProjectsAsync(int? semesterId, string? status, string? search, string? majorName)
    {
        var projects = await _projectRepository.GetFilteredProjectsAsync(semesterId, status, search, majorName);
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

    public async Task<bool> DeleteProjectAsync(int projectId)
    {
        var project = await _projectRepository.GetDetailAsync(projectId);
        if (project == null) return false;

        // Note: ProjectGroups, ProjectSupervisors should be deleted either by cascade or explicitly
        // If cascade is not set, we might need to remove them here.
        // Assuming _projectRepository.DeleteAsync handles it or it's cascade.
        // Based on GetDetailAsync, we have the details.

        await _projectRepository.DeleteAsync(project);
        await _projectRepository.SaveChangesAsync();
        return true;
    }

    public async Task<SupervisorAssignmentDto> GetSupervisorAssignmentDataAsync(int? semesterId)
    {
        var allProjects = await _projectRepository.GetSupervisorAssignmentProjectsAsync(semesterId);

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
                    CurrentWorkload = allProjects.Where(p => p.ProjectSupervisors.Any(ps => ps.LecturerID == l.UserID)).Sum(p => p.ProjectGroups.Count),
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

        // Check if the lecturer is already the main supervisor
        var alreadyMain = project.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main && ps.LecturerID == lecturerId);
        if (alreadyMain != null) return (true, "Lecturer is already assigned as the supervisor.");

        // Remove existing main supervisors
        var currentMainSupervisors = project.ProjectSupervisors.Where(ps => ps.Role == ProjectRole.Main).ToList();
        foreach (var ps in currentMainSupervisors)
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

        // return (true, "Supervisor assigned successfully.");
    }

    public async Task<(int total, int withGroup, int missingSupervisor, int missingMembers, int draftCount, int activeCount, int completedCount)> GetDashboardStatsAsync(int? semesterId = null)
    {
        if (semesterId.HasValue)
        {
            return await _projectRepository.GetDashboardStatsBySemesterAsync(semesterId.Value);
        }

        var projects = (await _projectRepository.GetAllWithDetailsAsync()).ToList();

        return (
            projects.Count,
            projects.Count(p => p.ProjectGroups.Any()),
            projects.Count(p => !p.ProjectSupervisors.Any()),
            projects.Count(p => p.ProjectGroups.Any(g => !g.GroupMembers.Any())),
            projects.Count(p => p.Status == ProjectStatus.Draft),
            projects.Count(p => p.Status == ProjectStatus.Active),
            projects.Count(p => p.Status == ProjectStatus.Completed)
        );
    }

    public async Task<(int total, int withGroup, int missingSupervisor, int missingMembers, int draftCount, int activeCount, int completedCount)> GetDashboardStatsBySemesterAsync(int semesterId)
    {
        return await _projectRepository.GetDashboardStatsBySemesterAsync(semesterId);
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

        var project = await _projectRepository.GetByIdAsync(projectId);
        if (project == null)
            return (false, "Project not found.");

        // 1. Check if graduated (Passed in any project)
        if (await _groupRepository.HasUserGraduatedAsync(userId))
            return (false, "Sinh viên đã hoàn thành đồ án tốt nghiệp ở học kỳ trước.");

        // 2. Check if already in a group for the SAME semester
        var projectsInSemester = await _projectRepository.GetBySemesterWithDetailsAsync(project.SemesterID);
        var isAlreadyInSemesterGroup = projectsInSemester.Any(p => p.ProjectGroups.Any(g => g.GroupMembers.Any(m => m.UserID == userId)));
        
        if (isAlreadyInSemesterGroup)
            return (false, "Sinh viên đã được chỉ định vào một nhóm đồ án khác trong học kỳ này.");

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
                Notes = session.Notes
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

    public async Task<(byte[] content, string fileName, string contentType)?> GetSubmissionFileAsync(int submissionId)
    {
        return await _submissionAccessService.GetSubmissionFileAsync(submissionId);
    }

    public async Task<bool> CanUserAccessSubmissionAsync(string userId, int submissionId, string role)
    {
        return await _submissionAccessService.CanUserAccessSubmissionAsync(userId, submissionId, role);
    }

    // ============================================================
    // NEW HOD FEATURES
    // ============================================================

    public async Task<DefenseScheduleViewModel> GetDefenseScheduleDataAsync(int? roundId, DateTime date)
    {
        try 
        {
            var allSemesters = await _semesterRepository.GetAllAsync();
            var activeSemester = allSemesters.FirstOrDefault(s => s.Status == SemesterStatus.Active) ?? allSemesters.FirstOrDefault();
            
            if (activeSemester == null) 
            {
                Console.WriteLine("[HOD] No semester found in database.");
                return new DefenseScheduleViewModel { Rounds = new List<ReviewRoundDto>() };
            }

            var rounds = (await _reviewRoundService.GetReviewRoundsBySemesterAsync(activeSemester.SemesterID))
                            .Where(r => r.RoundNumber >= 3)
                            .ToList();
            var targetRound = roundId.HasValue ? rounds.FirstOrDefault(r => r.ReviewRoundID == roundId.Value) : rounds.OrderBy(r => r.RoundNumber).FirstOrDefault();
            
            if (targetRound == null) return new DefenseScheduleViewModel { Rounds = rounds };


            List<CommitteeDto> availableCommittees = new();
            if (targetRound.RoundNumber == 3)
            {
                var committees = await _committeeRepository.GetBySemesterAsync(activeSemester.SemesterID);
                availableCommittees = committees.Select(c => new CommitteeDto
                {
                    CommitteeID = c.CommitteeID,
                    CommitteeName = c.CommitteeName,
                    ChairpersonName = c.Chairperson?.FullName ?? "N/A",
                    SecretaryName = c.Secretary?.FullName ?? "N/A",
                    ReviewerName = c.Reviewer?.FullName ?? "N/A",
                    Reviewer2Name = c.Reviewer2?.FullName,
                    Reviewer3Name = c.Reviewer3?.FullName
                }).ToList();
            }

            var rooms = await _roomRepository.GetAllAsync();
            var sessions = await _sessionRepository.GetByRoundAndDateAsync(targetRound.ReviewRoundID, date);
            
            var allGroupsInSemester = await _groupRepository.GetBySemesterWithDetailsAsync(activeSemester.SemesterID);
            var scheduledInRound = (await _sessionRepository.GetByRoundAsync(targetRound.ReviewRoundID)).Select(s => s.GroupID).ToHashSet();
            var unscheduled = allGroupsInSemester.Where(g => !scheduledInRound.Contains(g.GroupID)).ToList();

            return new DefenseScheduleViewModel
            {
                ReviewRoundID = targetRound.ReviewRoundID,
                RoundNumber = targetRound.RoundNumber,
                RoundDescription = targetRound.Description,
                SelectedDate = date,
                IsFinalRound = targetRound.RoundNumber >= 3,
                Rounds = rounds,
                Rooms = rooms.Select(r => new RoomDto { RoomID = r.RoomID, RoomCode = r.RoomCode, Capacity = r.Capacity, Building = r.Building }).ToList(),
                ScheduledSessions = sessions.Select(s => new DefenseScheduleSessionDto
                {
                    SessionID = s.SessionID,
                    GroupID = s.GroupID,
                    ProjectCode = s.Group.Project.ProjectCode,
                    GroupName = s.Group.GroupName,
                    ProjectName = s.Group.Project.ProjectName,
                    ScheduledAt = s.ScheduledAt,
                    RoomID = s.RoomID ?? 0,
                    RoomCode = s.Room?.RoomCode,
                    MajorName = s.Group.Project.Major?.MajorName,
                    SupervisorName = s.Group.Project.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main)?.Lecturer.FullName,
                    MeetLink = s.MeetLink,
                    IsOnline = s.IsOnline,
                    Reviewers = s.Group.ReviewerAssignments.Where(ra => ra.ReviewRoundID == targetRound.ReviewRoundID).Select(ra => new SessionReviewerDto
                    {
                        ReviewerID = ra.ReviewerID,
                        ReviewerName = ra.Reviewer.FullName,
                        Role = Enum.IsDefined(typeof(CommitteeRole), ra.CommitteeRole ?? 2) ? (CommitteeRole)(ra.CommitteeRole ?? 2) : CommitteeRole.Reviewer 
                    }).ToList()
                }).ToList(),
                UnscheduledGroups = unscheduled.Select(g => new UnscheduledGroupDto
                {
                    GroupID = g.GroupID,
                    ProjectCode = g.Project.ProjectCode,
                    GroupName = g.GroupName,
                    ProjectName = g.Project.ProjectName,
                    MajorName = g.Project.Major?.MajorName ?? "N/A",
                    IsReady = true, // Simplified for now
                    PlagiarismScore = 15.5, // Mock data
                    IsApprovedBySupervisor = true, // Mock data
                    SupervisorName = g.Project.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main)?.Lecturer.FullName,
                    Reviewers = g.ReviewerAssignments.Where(ra => ra.ReviewRoundID == targetRound.ReviewRoundID).Select(ra => new SessionReviewerDto
                    {
                        ReviewerID = ra.ReviewerID,
                        ReviewerName = ra.Reviewer.FullName,
                        Role = Enum.IsDefined(typeof(CommitteeRole), ra.CommitteeRole ?? 2) ? (CommitteeRole)(ra.CommitteeRole ?? 2) : CommitteeRole.Reviewer 
                    }).ToList()
                }).ToList(),
                AvailableCommittees = availableCommittees
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HOD] Error getting Defense Schedule: {ex.Message}");
            return new DefenseScheduleViewModel { Rounds = new List<ReviewRoundDto>() };
        }
    }

    public async Task<(bool success, string message)> SaveDefenseSessionAsync(CreateDefenseSessionRequest request)
    {
        // Parse time
        if (!DateTime.TryParse($"{request.ScheduledDate} {request.StartTime}", out var scheduledAt))
            return (false, "Invalid date or time format.");

        var existing = await _sessionRepository.GetByRoundAndGroupAsync(request.ReviewRoundID, request.GroupID);
        var round = await _reviewRoundService.GetReviewRoundByIdAsync(request.ReviewRoundID);
        bool isFinalRound = round?.RoundNumber >= 3;

        // 1. Round 3: Mentor Conflict & Committee Membership validation
        if (isFinalRound && request.CommitteeID.HasValue)
        {
            var group = await _groupRepository.GetByIdAsync(request.GroupID);
            if (group?.Project?.ProjectSupervisors != null)
            {
                var supervisors = group.Project.ProjectSupervisors.Select(ps => ps.LecturerID).ToList();
                var committee = await _committeeRepository.GetByIdAsync(request.CommitteeID.Value);
                if (committee != null)
                {
                    var committeeMembers = new List<string> { committee.ChairpersonID, committee.SecretaryID, committee.ReviewerID };
                    if (!string.IsNullOrEmpty(committee.Reviewer2ID)) committeeMembers.Add(committee.Reviewer2ID);
                    if (!string.IsNullOrEmpty(committee.Reviewer3ID)) committeeMembers.Add(committee.Reviewer3ID);

                    if (supervisors.Any(s => committeeMembers.Contains(s)))
                        return (false, "Mentor cannot be a member of the defense committee for their own group.");
                }
            }
        }

        // 2. Schedule Overlap checks (Global check across all rounds on the same day/time)
        var overlappingSessions = (await _sessionRepository.GetByDateAsync(scheduledAt))
            .Where(s => s.ScheduledAt == scheduledAt && s.SessionID != (existing?.SessionID ?? 0))
            .ToList();

        if (overlappingSessions.Any())
        {
            // A. Check Committee double-booking
            if (request.CommitteeID.HasValue && overlappingSessions.Any(s => s.CommitteeID == request.CommitteeID))
                return (false, "This committee is already scheduled for another group at this time.");

            // B. Check Individual Reviewer double-booking
            var proposedMembers = new List<string>();
            if (request.CommitteeID.HasValue)
            {
                var committee = await _committeeRepository.GetByIdAsync(request.CommitteeID.Value);
                if (committee != null)
                {
                    proposedMembers.AddRange(new[] { committee.ChairpersonID, committee.SecretaryID, committee.ReviewerID });
                    if (!string.IsNullOrEmpty(committee.Reviewer2ID)) proposedMembers.Add(committee.Reviewer2ID);
                    if (!string.IsNullOrEmpty(committee.Reviewer3ID)) proposedMembers.Add(committee.Reviewer3ID);
                }
            }
            else
            {
                // Round 1/2: Get assigned reviewers for THIS group
                var currentAssignments = await _assignmentRepository.GetByRoundAndGroupAsync(request.ReviewRoundID, request.GroupID);
                proposedMembers.AddRange(currentAssignments.Select(a => a.ReviewerID));
            }

            if (proposedMembers.Any())
            {
                foreach (var session in overlappingSessions)
                {
                    var sessionMembers = new List<string>();
                    if (session.CommitteeID.HasValue)
                    {
                        // In GetByDateAsync, I included Committee
                        var c = session.Committee;
                        if (c != null)
                        {
                            sessionMembers.AddRange(new[] { c.ChairpersonID, c.SecretaryID, c.ReviewerID });
                            if (!string.IsNullOrEmpty(c.Reviewer2ID)) sessionMembers.Add(c.Reviewer2ID);
                            if (!string.IsNullOrEmpty(c.Reviewer3ID)) sessionMembers.Add(c.Reviewer3ID);
                        }
                    }
                    else
                    {
                        // Round 1/2: Check assignments directly
                        var assignments = await _assignmentRepository.GetByRoundAndGroupAsync(session.ReviewRoundID, session.GroupID);
                        sessionMembers.AddRange(assignments.Select(a => a.ReviewerID));
                    }

                    if (proposedMembers.Intersect(sessionMembers).Any())
                    {
                        var conflictUserIds = proposedMembers.Intersect(sessionMembers).ToList();
                        // Optional: get names for better message
                        return (false, "One or more lecturers in this session are already scheduled for another concurrent session.");
                    }
                }
            }
        }

        if (existing != null)
        {
            existing.ScheduledAt = scheduledAt;
            existing.RoomID = request.RoomID;
            existing.IsOnline = request.IsOnline;
            existing.MeetLink = request.MeetLink;
            
            // Auto-generate meet link if online and empty
            if (existing.IsOnline && string.IsNullOrEmpty(existing.MeetLink))
            {
                existing.MeetLink = GenerateMeetLink();
            }
            
            existing.Notes = request.Notes;
            existing.CommitteeID = request.CommitteeID;
            await _sessionRepository.UpdateAsync(existing);
        }
        else
        {
            var meetLink = request.MeetLink;
            if (request.IsOnline && string.IsNullOrEmpty(meetLink))
            {
                meetLink = GenerateMeetLink();
            }

            existing = new ReviewSessionInfo
            {
                ReviewRoundID = request.ReviewRoundID,
                GroupID = request.GroupID,
                ScheduledAt = scheduledAt,
                RoomID = request.RoomID,
                IsOnline = request.IsOnline,
                MeetLink = meetLink,
                Notes = request.Notes,
                CommitteeID = request.CommitteeID
            };
            await _sessionRepository.AddAsync(existing);
        }

        // --- ROUND 3 SPECIAL: Sync Committee Members to ReviewerAssignments ---
        round = await _reviewRoundService.GetReviewRoundByIdAsync(request.ReviewRoundID);
        if (round != null && round.RoundNumber == 3 && request.CommitteeID.HasValue)
        {
            var committee = await _committeeRepository.GetByIdAsync(request.CommitteeID.Value);
            if (committee != null)
            {
                // 1. Remove existing assignments for this group in Round 3
                var oldAssignments = await _assignmentRepository.GetByRoundAndGroupAsync(request.ReviewRoundID, request.GroupID);
                foreach (var old in oldAssignments)
                {
                    await _assignmentRepository.RemoveAsync(old);
                }

                // 2. Map Committee Members
                var members = new List<(string? userId, CommitteeRole role)>
                {
                    (committee.ChairpersonID, CommitteeRole.Chairperson),
                    (committee.SecretaryID, CommitteeRole.Secretary),
                    (committee.ReviewerID, CommitteeRole.Reviewer),
                    (committee.Reviewer2ID, CommitteeRole.Reviewer),
                    (committee.Reviewer3ID, CommitteeRole.Reviewer)
                };

                foreach (var member in members
                    .Where(m => !string.IsNullOrEmpty(m.userId))
                    .GroupBy(m => m.userId)
                    .Select(g => g.First()))
                {
                    await _assignmentRepository.AddAsync(new ReviewerAssignment
                    {
                        ReviewRoundID = request.ReviewRoundID,
                        GroupID = request.GroupID,
                        ReviewerID = member.userId!,
                        CommitteeRole = (int)member.role
                    });
                }
            }
        }
        
        await _sessionRepository.SaveChangesAsync();
        await _assignmentRepository.SaveChangesAsync();
        return (true, "Schedule and assignments updated successfully.");
    }

    public async Task<(bool success, string message)> DeleteDefenseSessionAsync(int sessionId)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId);
        if (session == null) return (false, "Session not found.");

        // Check if session has passed
        if (session.ScheduledAt < DateTime.UtcNow)
            return (false, "Cannot delete a session that has already started or passed.");

        // If Round 3, also clear reviewer assignments (synced from committee)
        var round = await _reviewRoundService.GetReviewRoundByIdAsync(session.ReviewRoundID);
        if (round != null && round.RoundNumber == 3)
        {
            var assignments = await _assignmentRepository.GetByRoundAndGroupAsync(session.ReviewRoundID, session.GroupID);
            foreach (var a in assignments) await _assignmentRepository.RemoveAsync(a);
            await _assignmentRepository.SaveChangesAsync();
        }

        await _sessionRepository.RemoveAsync(session);
        await _sessionRepository.SaveChangesAsync();
        return (true, "Session deleted successfully.");
    }

    public async Task<ReviewerAssignmentDto> GetReviewerAssignmentDataAsync(int? roundId)
    {
        try 
        {
            var semesters = await _semesterRepository.GetAllAsync();
            var activeSemester = semesters.FirstOrDefault(s => s.Status == SemesterStatus.Active) ?? semesters.FirstOrDefault();
            
            if (activeSemester == null) 
            {
                Console.WriteLine("[HOD] No semester found in database.");
                return new ReviewerAssignmentDto { AllRounds = new List<ReviewRoundDto>() };
            }

            var rounds = (await _reviewRoundService.GetReviewRoundsBySemesterAsync(activeSemester.SemesterID)).ToList();
            var targetRound = roundId.HasValue ? rounds.FirstOrDefault(r => r.ReviewRoundID == roundId.Value) : rounds.OrderBy(r => r.RoundNumber).FirstOrDefault();
            
            if (targetRound == null) return new ReviewerAssignmentDto { AllRounds = rounds, ReviewRoundID = rounds.FirstOrDefault()?.ReviewRoundID ?? 0 };

            var allGroups = await _groupRepository.GetReviewerAssignmentGroupsAsync(activeSemester.SemesterID);
            var lecturers = await _userRepository.GetByRoleAsync(RoleName.Lecturer);

            // Refined group mapping with MentorIDs for frontend filtering
            Func<ProjectGroup, ReviewRoundDto, dynamic> mapGroup = (g, round) => new {
                g.GroupID,
                ProjectID = g.Project.ProjectID.ToString(),
                g.Project.ProjectCode,
                g.Project.ProjectName,
                MajorName = g.Project.Major?.MajorName ?? "N/A",
                g.GroupName,
                SupervisorName = g.Project.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main)?.Lecturer.FullName ?? "N/A",
                MentorIDs = g.Project.ProjectSupervisors.Select(ps => ps.LecturerID).ToList(),
                Reviewers = g.ReviewerAssignments.Where(ra => ra.ReviewRoundID == round.ReviewRoundID).Select(ra => new SessionReviewerDto
                {
                    ReviewerID = ra.ReviewerID,
                    ReviewerName = ra.Reviewer.FullName,
                    Role = ra.CommitteeRole.HasValue ? (CommitteeRole)ra.CommitteeRole.Value : CommitteeRole.Reviewer
                }).ToList()
            };

            var assignedGroups = allGroups.Where(g => g.ReviewerAssignments.Any(ra => ra.ReviewRoundID == targetRound.ReviewRoundID))
                .Select(g => {
                    var mapped = mapGroup(g, targetRound);
                    return new AssignedGroupDto {
                        GroupID = mapped.GroupID,
                        ProjectID = mapped.ProjectID,
                        ProjectCode = mapped.ProjectCode,
                        ProjectName = mapped.ProjectName,
                        MajorName = mapped.MajorName,
                        GroupName = mapped.GroupName,
                        SupervisorName = mapped.SupervisorName,
                        MentorIDs = mapped.MentorIDs,
                        Reviewers = mapped.Reviewers
                    };
                }).ToList();

            var unassignedGroups = allGroups.Where(g => !g.ReviewerAssignments.Any(ra => ra.ReviewRoundID == targetRound.ReviewRoundID))
                .Select(g => {
                    var mapped = mapGroup(g, targetRound);
                    return new UnassignedGroupDto {
                        GroupID = mapped.GroupID,
                        ProjectID = mapped.ProjectID,
                        ProjectCode = mapped.ProjectCode,
                        ProjectName = mapped.ProjectName,
                        MajorName = mapped.MajorName,
                        GroupName = mapped.GroupName,
                        SupervisorName = mapped.SupervisorName,
                        MentorIDs = mapped.MentorIDs,
                        Reviewers = mapped.Reviewers
                    };
                }).ToList();

            return new ReviewerAssignmentDto
            {
                ReviewRoundID = targetRound.ReviewRoundID,
                IsFinalRound = targetRound.RoundNumber >= 3,
                AllRounds = rounds,
                AssignedGroups = assignedGroups,
                UnassignedGroups = unassignedGroups,
                Lecturers = lecturers.Select(l => new LecturerDto
                {
                    LecturerID = l.UserID,
                    FullName = l.FullName,
                    Level = l.LecturerExpertises.FirstOrDefault()?.Level.ToString() ?? "Lecturer",
                    Specialty = l.LecturerExpertises.FirstOrDefault()?.Expertise?.ExpertiseName ?? "General",
                    CurrentWorkload = allGroups.Count(g => g.ReviewerAssignments.Any(ra => ra.ReviewerID == l.UserID && ra.ReviewRoundID == targetRound.ReviewRoundID)),
                    MaxWorkload = 10
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HOD] Error getting Reviewer Assignment: {ex.Message}");
            return new ReviewerAssignmentDto { AllRounds = new List<ReviewRoundDto>() };
        }
    }

    public async Task<(bool success, string message)> SaveReviewerAssignmentsAsync(UpdateReviewerAssignmentRequest request)
    {
        // 1. Clear existing assignments for THIS group in THIS specific round
        var existing = await _assignmentRepository.GetByRoundAndGroupAsync(request.ReviewRoundID, request.GroupID);
        foreach (var old in existing)
        {
            await _assignmentRepository.RemoveAsync(old);
        }

        // 2. Clear invalid assignments (Mentor cannot be Reviewer)
        var project = (await _groupRepository.GetByIdAsync(request.GroupID))?.Project;
        var mentorIds = project?.ProjectSupervisors.Select(ps => ps.LecturerID).ToList() ?? new List<string>();

        // 3. Add new assignments with validation
        foreach (var assignment in request.Assignments)
        {
            if (mentorIds.Contains(assignment.LecturerID))
            {
                return (false, "Giảng viên đang hướng dẫn nhóm này không thể làm phản biện.");
            }

            await _assignmentRepository.AddAsync(new ReviewerAssignment
            {
                ReviewRoundID = request.ReviewRoundID,
                GroupID = request.GroupID,
                ReviewerID = assignment.LecturerID,
                CommitteeRole = (int)assignment.Role, // Save the role
                AssignedAt = DateTime.UtcNow,
                IsRandom = false
            });
        }
        await _assignmentRepository.SaveChangesAsync();
        return (true, "Assignments saved successfully.");
    }

    public async Task<(bool success, string message)> RemoveReviewerAsync(RemoveReviewerRequest request)
    {
        var existing = await _assignmentRepository.GetByRoundAndGroupAsync(request.ReviewRoundID, request.GroupID);
        var target = existing.FirstOrDefault(ra => ra.ReviewerID == request.LecturerID);
        if (target != null)
        {
            await _assignmentRepository.RemoveAsync(target);
            await _assignmentRepository.SaveChangesAsync();
            return (true, "Reviewer removed successfully.");
        }
        return (false, "Assignment not found.");
    }

    private string GenerateMeetLink()
    {
        var chars = "abcdefghijklmnopqrstuvwxyz";
        var random = new Random();
        string Gen(int len) => new string(Enumerable.Repeat(chars, len).Select(s => s[random.Next(s.Length)]).ToArray());
        return $"https://meet.google.com/{Gen(3)}-{Gen(4)}-{Gen(3)}";
    }
}
