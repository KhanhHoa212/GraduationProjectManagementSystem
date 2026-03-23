using AutoMapper;
using GPMS.Domain.Entities;
using GPMS.Application.DTOs;
using GPMS.Domain.Enums;
using System.Linq;

namespace GPMS.Application.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User -> UserDto
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.UserRoles.Select(r => r.RoleName.ToString()).ToList()));

        // User -> StudentSearchDto
        CreateMap<User, StudentSearchDto>();

        // Semester -> SemesterDto
        CreateMap<Semester, SemesterDto>()
            .ForMember(dest => dest.ProjectsCount, opt => opt.MapFrom(src => src.Projects != null ? src.Projects.Count : 0));

        // Project -> ProjectDto
        CreateMap<Project, ProjectDto>()
            .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major != null ? src.Major.MajorName : null))
            .ForMember(dest => dest.SemesterCode, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.SemesterCode : null))
            .ForMember(dest => dest.AcademicYear, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.AcademicYear : null))
            .ForMember(dest => dest.SupervisorID, opt => opt.MapFrom(src => 
                src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main) != null ? src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main)!.LecturerID : null))
            .ForMember(dest => dest.SupervisorName, opt => opt.MapFrom(src => 
                src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main) != null && src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main)!.Lecturer != null ? src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main)!.Lecturer!.FullName : null))
            .ForMember(dest => dest.GroupCount, opt => opt.MapFrom(src => src.ProjectGroups != null ? src.ProjectGroups.Count : 0))
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => 
                src.ProjectGroups.SelectMany(g => g.GroupMembers.Select(m => new ProjectMemberDto
                {
                    UserID = m.UserID,
                    FullName = m.User != null ? m.User.FullName : string.Empty,
                    RoleInGroup = m.RoleInGroup,
                    GroupName = g.GroupName
                })).ToList()))
            .ForMember(dest => dest.StudentName, opt => opt.MapFrom(src => 
                src.ProjectGroups.FirstOrDefault() != null ? src.ProjectGroups.FirstOrDefault()!.GroupName : null));

        // Project -> ProjectDetailDto
        CreateMap<Project, ProjectDetailDto>()
            .ForMember(dest => dest.SemesterCode, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.SemesterCode : null))
            .ForMember(dest => dest.AcademicYear, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.AcademicYear : null))
            .ForMember(dest => dest.MajorName, opt => opt.MapFrom(src => src.Major != null ? src.Major.MajorName : null))
            .ForMember(dest => dest.SupervisorID, opt => opt.MapFrom(src => 
                src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main) != null ? src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main)!.LecturerID : null))
            .ForMember(dest => dest.SupervisorName, opt => opt.MapFrom(src => 
                src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main) != null && src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main)!.Lecturer != null ? src.ProjectSupervisors.FirstOrDefault(ps => ps.Role == ProjectRole.Main)!.Lecturer!.FullName : null))
            .ForMember(dest => dest.Members, opt => opt.MapFrom(src => 
                src.ProjectGroups.SelectMany(g => g.GroupMembers.Select(m => new ProjectMemberDto
                {
                    UserID = m.UserID,
                    FullName = m.User != null ? m.User.FullName : string.Empty,
                    RoleInGroup = m.RoleInGroup,
                    GroupName = g.GroupName
                })).ToList()))
            .ForMember(dest => dest.Submissions, opt => opt.MapFrom(src => 
                src.ProjectGroups.SelectMany(g => g.Submissions).ToList()));

        // Submission mapping
        CreateMap<Submission, SubmissionDto>()
            .ForMember(dest => dest.DocumentName, opt => opt.MapFrom(src => src.Requirement.DocumentName));

        // ReviewRound mapping
        CreateMap<ReviewRound, ReviewRoundDto>()
            .ForMember(dest => dest.SemesterCode, opt => opt.MapFrom(src => src.Semester != null ? src.Semester.SemesterCode : string.Empty));
            
        CreateMap<CreateReviewRoundDto, ReviewRound>()
            .ForMember(dest => dest.SubmissionRequirements, opt => opt.Ignore());

        // SubmissionRequirement mapping
        CreateMap<SubmissionRequirement, SubmissionRequirementDto>().ReverseMap();

    }
}
