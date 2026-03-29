using GPMS.Application.Interfaces.Repositories;
using GPMS.Application.Interfaces.Services;
using GPMS.Application.Mapping;
using GPMS.Application.Services;
using GPMS.Infrastructure.Data;
using GPMS.Infrastructure.Email;
using GPMS.Infrastructure.Services;
using GPMS.Application.Common.Settings;
using GPMS.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GPMS.Infrastructure;
using GPMS.Infrastructure.Data.Seeding;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging - Clear all default providers (including EventLog) to prevent permission errors on Windows
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<GpmsDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<IProjectGroupRepository, ProjectGroupRepository>();
builder.Services.AddScoped<IReviewRoundRepository, ReviewRoundRepository>();
builder.Services.AddScoped<IReviewerAssignmentRepository, ReviewerAssignmentRepository>();
builder.Services.AddScoped<ISubmissionRepository, SubmissionRepository>();
builder.Services.AddScoped<IEvaluationRepository, EvaluationRepository>();
builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
builder.Services.AddScoped<IMentorRoundReviewRepository, MentorRoundReviewRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<ISemesterRepository, SemesterRepository>();
builder.Services.AddScoped<IChecklistRepository, ChecklistRepository>();
builder.Services.AddScoped<IMajorRepository, MajorRepository>();
builder.Services.AddScoped<IFacultyRepository, FacultyRepository>();
builder.Services.AddScoped<IGroupRoundProgressRepository, GroupRoundProgressRepository>();
builder.Services.AddScoped<IReviewSessionRepository, ReviewSessionRepository>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
// Register Services
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISemesterService, SemesterService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ILecturerService, LecturerService>();
builder.Services.AddScoped<ILecturerScheduleService, LecturerScheduleService>();
builder.Services.AddScoped<ILecturerWorkflowService, LecturerWorkflowService>();
builder.Services.AddScoped<ISubmissionAccessService, SubmissionAccessService>();
builder.Services.AddScoped<IReviewRoundService, ReviewRoundService>();
builder.Services.AddScoped<IChecklistService, ChecklistService>();
builder.Services.AddScoped<IFeedbackAutoReleaseService, FeedbackAutoReleaseService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IMajorService, MajorService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IMeetingService, GoogleCalendarService>();


// Register Infrastructure (including Seeders)
builder.Services.AddInfrastructure(builder.Configuration);

// Register AutoMapper
builder.Services.AddAutoMapper(cfg => { }, typeof(MappingProfile).Assembly);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    })
    .AddRazorRuntimeCompilation();

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

// Memory cache (for system logs + visit tracking)
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

// Add Google Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? string.Empty;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? string.Empty;
    options.CallbackPath = "/signin-google";
});

// Add email service
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Cloudinary service
builder.Services.Configure<CloudinarySettings>(
    builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.AddScoped<IFileService, CloudinaryService>();

// CRITICAL: Prevent BackgroundService exceptions from killing the whole app (default .NET 6+ behavior)
builder.Services.Configure<HostOptions>(options =>
    options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore);

// Register Background Services
builder.Services.AddHostedService<GPMS.Web.Services.FeedbackAutoReleaseHostedService>();
builder.Services.AddHostedService<GPMS.Web.Services.ReminderHostedService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Seed Data
    try 
    {
        using (var scope = app.Services.CreateScope())
        {
            var runner = scope.ServiceProvider.GetRequiredService<DataSeederRunner>();
            await runner.RunAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[CRITICAL] Data seeding FAILED: {ex.Message}");
    }
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.MapRazorPages();

app.Run();

