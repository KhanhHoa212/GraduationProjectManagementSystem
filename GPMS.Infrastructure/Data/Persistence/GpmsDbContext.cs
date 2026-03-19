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
    public DbSet<MentorRoundReview> MentorRoundReviews { get; set; }
    public DbSet<Room> Rooms { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GpmsDbContext).Assembly);
        modelBuilder.Entity<MentorRoundReview>(entity =>
        {
            entity.HasKey(m => new { m.ReviewRoundID, m.GroupID });

            entity.Property(m => m.SupervisorID)
                .HasMaxLength(20);

            entity.HasOne(m => m.ReviewRound)
                .WithMany(r => r.MentorRoundReviews)
                .HasForeignKey(m => m.ReviewRoundID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Group)
                .WithMany(g => g.MentorRoundReviews)
                .HasForeignKey(m => m.GroupID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Supervisor)
                .WithMany(u => u.MentorRoundReviews)
                .HasForeignKey(m => m.SupervisorID)
                .OnDelete(DeleteBehavior.Restrict);
        });
        
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
            new Project { ProjectID = 101, ProjectCode = "PRJ-02", ProjectName = "Smart Healthcare System", Status = ProjectStatus.Active, SemesterID = 1, MajorID = 1 },
            new Project { ProjectID = 102, ProjectCode = "PRJ-03", ProjectName = "E-Commerce Platform", Status = ProjectStatus.Active, SemesterID = 1, MajorID = 1 }
        );
        modelBuilder.Entity<ProjectGroup>().HasData(
            new ProjectGroup { GroupID = 100, ProjectID = 100, GroupName = "Group AI-01", CreatedAt = new DateTime(2025, 1, 5) },
            new ProjectGroup { GroupID = 101, ProjectID = 101, GroupName = "Group HC-01", CreatedAt = new DateTime(2025, 1, 5) },
            new ProjectGroup { GroupID = 102, ProjectID = 102, GroupName = "Group EC-01", CreatedAt = new DateTime(2025, 1, 5) }
        );

        // 9. Members & Supervisors
        modelBuilder.Entity<GroupMember>().HasData(
            // Group 100 (AI Traffic Analyzer) - GV001 mentor
            new GroupMember { GroupID = 100, RoleInGroup = GroupRole.Leader, UserID = "SE180001" },
            new GroupMember { GroupID = 100, RoleInGroup = GroupRole.Member, UserID = "SE180002" },
            // Group 101 (Smart Healthcare) - GV002 mentor
            new GroupMember { GroupID = 101, RoleInGroup = GroupRole.Leader, UserID = "SE180003" },
            new GroupMember { GroupID = 101, RoleInGroup = GroupRole.Member, UserID = "SE180004" },
            // Group 102 (E-Commerce) - GV001 mentor
            new GroupMember { GroupID = 102, RoleInGroup = GroupRole.Leader, UserID = "SE180005" }
        );
        modelBuilder.Entity<ProjectSupervisor>().HasData(
            new ProjectSupervisor { ProjectID = 100, Role = ProjectRole.Main, LecturerID = "GV001", AssignedAt = new DateTime(2025, 1, 5) },
            new ProjectSupervisor { ProjectID = 101, Role = ProjectRole.Main, LecturerID = "GV002", AssignedAt = new DateTime(2025, 1, 5) },
            new ProjectSupervisor { ProjectID = 102, Role = ProjectRole.Main, LecturerID = "GV001", AssignedAt = new DateTime(2025, 1, 5) }
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
            new ReviewSessionInfo { SessionID = 1, ReviewRoundID = 1, GroupID = 101, RoomID = 1, MeetLink = "https://meet.google.com/abc-defg-hij", ScheduledAt = new DateTime(2025, 3, 10, 10, 0, 0) },
            new ReviewSessionInfo { SessionID = 2, ReviewRoundID = 1, GroupID = 100, RoomID = 2, MeetLink = "https://meet.google.com/xyz-uvw-qrs", ScheduledAt = new DateTime(2025, 3, 12, 14, 0, 0) },
            new ReviewSessionInfo { SessionID = 3, ReviewRoundID = 1, GroupID = 102, RoomID = 1, MeetLink = "https://meet.google.com/pqr-stu-vwx", ScheduledAt = new DateTime(2025, 3, 14, 9, 0, 0) }
        );
        modelBuilder.Entity<ReviewerAssignment>().HasData(
            // GV001 reviews Group 101 (supervised by GV002)
            new ReviewerAssignment { AssignmentID = 1, GroupID = 101, ReviewerID = "GV001", ReviewRoundID = 1, AssignedAt = new DateTime(2025, 3, 1) },
            // GV002 reviews Group 100 (supervised by GV001)
            new ReviewerAssignment { AssignmentID = 2, GroupID = 100, ReviewerID = "GV002", ReviewRoundID = 1, AssignedAt = new DateTime(2025, 3, 1) },
            // GV001 reviews Group 102 on round 2 defense (not yet evaluated)
            new ReviewerAssignment { AssignmentID = 3, GroupID = 102, ReviewerID = "GV003", ReviewRoundID = 2, AssignedAt = new DateTime(2025, 3, 1) }
        );

        modelBuilder.Entity<Evaluation>().HasData(
            // GV001 reviewed Group 101 (GV002 is supervisor → GV002 must approve)
            new Evaluation { EvaluationID = 1, GroupID = 101, ReviewerID = "GV001", ReviewRoundID = 1, TotalScore = 8.5m, Status = EvaluationStatus.Submitted, SubmittedAt = new DateTime(2025, 3, 10, 11, 0, 0) },
            // GV002 reviewed Group 100 (GV001 is supervisor → GV001 must approve)
            new Evaluation { EvaluationID = 2, GroupID = 100, ReviewerID = "GV002", ReviewRoundID = 1, TotalScore = 7.0m, Status = EvaluationStatus.Submitted, SubmittedAt = new DateTime(2025, 3, 12, 15, 0, 0) }
        );
        modelBuilder.Entity<EvaluationDetail>().HasData(
            // Scores for Evaluation 1 (GV001 reviewed Group 101)
            new EvaluationDetail { EvaluationID = 1, ItemID = 1, Score = 4.5m, Comment = "Solid microservices architecture" },
            new EvaluationDetail { EvaluationID = 1, ItemID = 2, Score = 4.0m, Comment = "Code is clean but lacks unit tests" },
            // Scores for Evaluation 2 (GV002 reviewed Group 100)
            new EvaluationDetail { EvaluationID = 2, ItemID = 1, Score = 3.5m, Comment = "Architecture needs refinement" },
            new EvaluationDetail { EvaluationID = 2, ItemID = 2, Score = 3.5m, Comment = "Average code quality for this stage" }
        );
        modelBuilder.Entity<Feedback>().HasData(
            // Feedback from GV001's evaluation of Group 101 → awaiting GV002 approval
            new Feedback { FeedbackID = 1, EvaluationID = 1, Content = "Great architecture decisions. The code quality is above average but needs more inline comments and unit tests for the core business logic modules. Overall this is a solid submission.", CreatedAt = new DateTime(2025, 3, 10, 11, 30, 0) },
            // Feedback from GV002's evaluation of Group 100 → awaiting GV001 approval
            new Feedback { FeedbackID = 2, EvaluationID = 2, Content = "The AI model design needs more justification. The architecture diagram is unclear for the traffic ingestion pipeline. Please revise the technical report with more detailed diagrams before the next round.", CreatedAt = new DateTime(2025, 3, 12, 15, 30, 0) }
        );
        modelBuilder.Entity<FeedbackApproval>().HasData(
            // Pending for GV002 (supervisor of Group 101) to approve
            new FeedbackApproval { FeedbackID = 1, SupervisorID = "GV002", ApprovalStatus = ApprovalStatus.Pending, SupervisorComment = "" },
            // Pending for GV001 (supervisor of Group 100) to approve
            new FeedbackApproval { FeedbackID = 2, SupervisorID = "GV001", ApprovalStatus = ApprovalStatus.Pending, SupervisorComment = "" }
        );
    }
}
