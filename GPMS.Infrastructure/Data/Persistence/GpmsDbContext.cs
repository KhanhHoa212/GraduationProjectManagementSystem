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
    }
}
