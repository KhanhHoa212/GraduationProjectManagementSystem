using GPMS.Domain.Entities;
using GPMS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GPMS.Infrastructure.Data;

public class GpmsDbContext : DbContext
{
    public GpmsDbContext(DbContextOptions<GpmsDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserCredential> UserCredentials { get; set; }
    public DbSet<Faculty> Faculties { get; set; }
    public DbSet<Major> Majors { get; set; }
    public DbSet<ExpertiseArea> ExpertiseAreas { get; set; }
    public DbSet<LecturerExpertise> LecturerExpertises { get; set; }
    public DbSet<Semester> Semesters { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<ProjectSupervisor> ProjectSupervisors { get; set; }
    public DbSet<ProjectGroup> ProjectGroups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<ReviewRound> ReviewRounds { get; set; }
    public DbSet<ReviewSessionInfo> ReviewSessions { get; set; }
    public DbSet<ReviewerAssignment> ReviewerAssignments { get; set; }
    public DbSet<SubmissionRequirement> SubmissionRequirements { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<ReviewChecklist> ReviewChecklists { get; set; }
    public DbSet<ChecklistItem> ChecklistItems { get; set; }
    public DbSet<Evaluation> Evaluations { get; set; }
    public DbSet<EvaluationDetail> EvaluationDetails { get; set; }
    public DbSet<Feedback> Feedbacks { get; set; }
    public DbSet<FeedbackApproval> FeedbackApprovals { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GpmsDbContext).Assembly);
        
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // 1. Faculty
        modelBuilder.Entity<Faculty>().HasData(
            new Faculty { FacultyID = 1, FacultyCode = "SE", FacultyName = "Software Engineering Faculty" }
        );

        // 2. Majors
        modelBuilder.Entity<Major>().HasData(
            new Major { MajorID = 1, FacultyID = 1, MajorCode = "SE", MajorName = "Software Engineering" },
            new Major { MajorID = 2, FacultyID = 1, MajorCode = "SS", MajorName = "Software Testing" }
        );

        // 3. Expertise Areas
        modelBuilder.Entity<ExpertiseArea>().HasData(
            new ExpertiseArea { ExpertiseID = 1, MajorID = 1, ExpertiseName = "Web Development" },
            new ExpertiseArea { ExpertiseID = 2, MajorID = 1, ExpertiseName = "Mobile Development" },
            new ExpertiseArea { ExpertiseID = 3, MajorID = 1, ExpertiseName = "AI/ML" }
        );

        // 4. Semester
        modelBuilder.Entity<Semester>().HasData(
            new Semester 
            { 
                SemesterID = 1, 
                SemesterCode = "SP25", 
                AcademicYear = "2024-2025", 
                StartDate = new DateTime(2025, 1, 1), 
                EndDate = new DateTime(2025, 4, 30), 
                Status = SemesterStatus.Active 
            }
        );

        // 5. Rooms
        modelBuilder.Entity<Room>().HasData(
            new Room { RoomID = 1, RoomCode = "B1.01", Building = "Alpha", Capacity = 30, RoomType = RoomType.Classroom, Status = RoomStatus.Available },
            new Room { RoomID = 2, RoomCode = "B1.02", Building = "Alpha", Capacity = 30, RoomType = RoomType.Classroom, Status = RoomStatus.Available },
            new Room { RoomID = 3, RoomCode = "Hall-A", Building = "Delta", Capacity = 100, RoomType = RoomType.Hall, Status = RoomStatus.Available }
        );

        // 6. Users (Admin, Lecturers, Students)
        modelBuilder.Entity<User>().HasData(
            new User { UserID = "ADMIN001", Username = "admin", FullName = "System Admin", Email = "admin@fpt.edu.vn", Status = UserStatus.Active },
            new User { UserID = "GV001", FullName = "Lecturer One", Email = "giao-vien1@fpt.edu.vn", Status = UserStatus.Active },
            new User { UserID = "GV002", FullName = "Lecturer Two", Email = "giao-vien2@fpt.edu.vn", Status = UserStatus.Active },
            new User { UserID = "GV003", FullName = "Lecturer Three", Email = "giao-vien3@fpt.edu.vn", Status = UserStatus.Active },
            new User { UserID = "HOD001", FullName = "Head of Department", Email = "hod@fpt.edu.vn", Status = UserStatus.Active },
            new User { UserID = "SE180001", FullName = "Student One", Email = "student1@fpt.edu.vn", Status = UserStatus.Active },
            new User { UserID = "SE180002", FullName = "Student Two", Email = "student2@fpt.edu.vn", Status = UserStatus.Active },
            new User { UserID = "SE180003", FullName = "Student Three", Email = "student3@fpt.edu.vn", Status = UserStatus.Active },
            new User { UserID = "SE180004", FullName = "Student Four", Email = "student4@fpt.edu.vn", Status = UserStatus.Active },
            new User { UserID = "SE180005", FullName = "Student Five", Email = "student5@fpt.edu.vn", Status = UserStatus.Active }
        );

        // 7. Roles
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { UserRoleID = 1, UserID = "ADMIN001", RoleName = RoleName.Admin },
            new UserRole { UserRoleID = 2, UserID = "GV001", RoleName = RoleName.Lecturer },
            new UserRole { UserRoleID = 3, UserID = "GV002", RoleName = RoleName.Lecturer },
            new UserRole { UserRoleID = 4, UserID = "GV003", RoleName = RoleName.Lecturer },
            new UserRole { UserRoleID = 10, UserID = "HOD001", RoleName = RoleName.HeadOfDept },
            new UserRole { UserRoleID = 5, UserID = "SE180001", RoleName = RoleName.Student },
            new UserRole { UserRoleID = 6, UserID = "SE180002", RoleName = RoleName.Student },
            new UserRole { UserRoleID = 7, UserID = "SE180003", RoleName = RoleName.Student },
            new UserRole { UserRoleID = 8, UserID = "SE180004", RoleName = RoleName.Student },
            new UserRole { UserRoleID = 9, UserID = "SE180005", RoleName = RoleName.Student }
        );

        // 8. Projects & Groups
        modelBuilder.Entity<Project>().HasData(
            new Project { ProjectID = 100, ProjectCode = "PRJ-01", ProjectName = "AI Traffic Analyzer", Status = ProjectStatus.Active, SemesterID = 1, MajorID = 1 },
            new Project { ProjectID = 101, ProjectCode = "PRJ-02", ProjectName = "Smart Healthcare System", Status = ProjectStatus.Active, SemesterID = 1, MajorID = 1 }
        );
        modelBuilder.Entity<ProjectGroup>().HasData(
            new ProjectGroup { GroupID = 100, ProjectID = 100, GroupName = "Group 1", CreatedAt = DateTime.Now },
            new ProjectGroup { GroupID = 101, ProjectID = 101, GroupName = "Group 2", CreatedAt = DateTime.Now }
        );

        // 9. Members & Supervisors
        modelBuilder.Entity<GroupMember>().HasData(
            new GroupMember { GroupID = 100, RoleInGroup = GroupRole.Member, UserID = "SE180001" },
            new GroupMember { GroupID = 100, RoleInGroup = GroupRole.Member, UserID = "SE180002" },
            new GroupMember { GroupID = 101, RoleInGroup = GroupRole.Member, UserID = "SE180003" }
        );
        modelBuilder.Entity<ProjectSupervisor>().HasData(
            new ProjectSupervisor { ProjectID = 100, Role = ProjectRole.Main, LecturerID = "GV001", AssignedAt = DateTime.Now },
            new ProjectSupervisor { ProjectID = 101, Role = ProjectRole.Main, LecturerID = "GV002", AssignedAt = DateTime.Now }
        );

        // 10. Reviews & Checklists
        modelBuilder.Entity<ReviewRound>().HasData(
            new ReviewRound { ReviewRoundID = 1, SemesterID = 1, RoundNumber = 1, RoundType = RoundType.Online, StartDate = DateTime.Now.AddDays(-10), EndDate = DateTime.Now.AddDays(10), Status = RoundStatus.Ongoing },
            new ReviewRound { ReviewRoundID = 2, SemesterID = 1, RoundNumber = 3, RoundType = RoundType.Offline, StartDate = DateTime.Now.AddDays(20), EndDate = DateTime.Now.AddDays(30), Status = RoundStatus.Planned }
        );
        modelBuilder.Entity<ReviewChecklist>().HasData(
            new ReviewChecklist { ChecklistID = 1, ReviewRoundID = 1, Title = "Cross Review 1 Checklist", Description = "Evaluate early stage architecture" }
        );
        modelBuilder.Entity<ChecklistItem>().HasData(
            new ChecklistItem { ItemID = 1, ChecklistID = 1, ItemCode = "ARCH-01", ItemContent = "Is the architecture solid?", MaxScore = 5, Weight = 50, OrderIndex = 1 },
            new ChecklistItem { ItemID = 2, ChecklistID = 1, ItemCode = "CODE-01", ItemContent = "Code quality for MVP", MaxScore = 5, Weight = 50, OrderIndex = 2 }
        );

        // 11. Review Assignments & Feedback
        modelBuilder.Entity<ReviewSessionInfo>().HasData(
            new ReviewSessionInfo { SessionID = 1, ReviewRoundID = 1, GroupID = 101, RoomID = 1, MeetLink = "https://meet.google.com/abc-defg-hij", ScheduledAt = DateTime.Now.AddDays(1) },
            new ReviewSessionInfo { SessionID = 2, ReviewRoundID = 1, GroupID = 100, RoomID = 2, MeetLink = "https://meet.google.com/xyz-uvw-qrs", ScheduledAt = DateTime.Now.AddDays(5) }
        );
        modelBuilder.Entity<ReviewerAssignment>().HasData(
            // GV001 is reviewing Group 101 (Not their own group!)
            new ReviewerAssignment { AssignmentID = 1, GroupID = 101, ReviewerID = "GV001", ReviewRoundID = 1, AssignedAt = DateTime.Now },
            // GV001 is reviewing Group 100 on Defense
            new ReviewerAssignment { AssignmentID = 2, GroupID = 100, ReviewerID = "GV001", ReviewRoundID = 2, AssignedAt = DateTime.Now }
        );

        modelBuilder.Entity<Evaluation>().HasData(
            new Evaluation { EvaluationID = 1, GroupID = 101, ReviewerID = "GV002", ReviewRoundID = 1, TotalScore = 8.5m, Status = EvaluationStatus.Submitted, SubmittedAt = DateTime.Now.AddDays(-2) }
        );
        modelBuilder.Entity<Feedback>().HasData(
            new Feedback { FeedbackID = 1, EvaluationID = 1, Content = "Great architecture. Code needs more comments.", CreatedAt = DateTime.Now.AddDays(-2) }
        );
        modelBuilder.Entity<FeedbackApproval>().HasData(
            new FeedbackApproval { FeedbackID = 1, SupervisorID = "GV002", ApprovalStatus = ApprovalStatus.Pending, SupervisorComment = "" }
        );
    }
}
