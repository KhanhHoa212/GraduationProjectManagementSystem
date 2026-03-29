using GPMS.Infrastructure.Data.Seeding;
using GPMS.Infrastructure.Data.Seeding.Seeders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GPMS.Application.Interfaces.Repositories;
using GPMS.Domain.Entities;
using GPMS.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using GPMS.Application.Interfaces.Services;
using GPMS.Application.Services;
using GPMS.Infrastructure.Services;
using GPMS.Infrastructure.Repositories;
using GPMS.Infrastructure.Email;

namespace GPMS.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Repositories
        services.AddScoped<ICommitteeRepository, CommitteeRepository>();
        services.AddScoped<IChecklistRepository, ChecklistRepository>();
        services.AddScoped<IEvaluationRepository, EvaluationRepository>();
        services.AddScoped<IFacultyRepository, FacultyRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IGroupRoundProgressRepository, GroupRoundProgressRepository>();
        services.AddScoped<IMajorRepository, MajorRepository>();
        services.AddScoped<IMentorRoundReviewRepository, MentorRoundReviewRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IProjectGroupRepository, ProjectGroupRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IReviewRoundRepository, ReviewRoundRepository>();
        services.AddScoped<IReviewSessionRepository, ReviewSessionRepository>();
        services.AddScoped<IReviewerAssignmentRepository, ReviewerAssignmentRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();
        services.AddScoped<ISemesterRepository, SemesterRepository>();
        services.AddScoped<ISubmissionRepository, SubmissionRepository>();
        services.AddScoped<IUserRepository, UserRepository>();

        // Application Services
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ICommitteeService, CommitteeService>();
        services.AddScoped<ISemesterService, SemesterService>();
        services.AddScoped<IReviewRoundService, ReviewRoundService>();
        services.AddScoped<IChecklistService, ChecklistService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IMajorService, MajorService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ILecturerService, LecturerService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IFeedbackAutoReleaseService, FeedbackAutoReleaseService>();

        // Infrastructure Services
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileService, CloudinaryService>();

        // Data Seeders
        services.AddScoped<IDataSeeder, FacultySeeder>();
        services.AddScoped<IDataSeeder, MajorSeeder>();
        services.AddScoped<IDataSeeder, SchemaPatchSeeder>();
        services.AddScoped<IDataSeeder, UserSeeder>();
        services.AddScoped<IDataSeeder, ProjectSeeder>();
        services.AddScoped<IDataSeeder, GroupSeeder>();
        services.AddScoped<IDataSeeder, ReviewRoundSeeder>();
        services.AddScoped<IDataSeeder, ReviewerAssignmentSeeder>();
        services.AddScoped<IDataSeeder, SubmissionSeeder>();
        services.AddScoped<IDataSeeder, EvaluationSeeder>();
        services.AddScoped<IDataSeeder, NotificationSeeder>();
        services.AddScoped<IDataSeeder, LecturerWorkflowTestSeeder>();
        services.AddScoped<DataSeederRunner>();

        return services;
    }
}
