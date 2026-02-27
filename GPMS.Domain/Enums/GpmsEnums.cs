namespace GPMS.Domain.Enums;

public enum UserStatus
{
    Active,
    Inactive
}

public enum RoleName
{
    Student,
    Lecturer,
    Admin,
    HeadOfDept
}

public enum AuthProvider
{
    Internal,
    SSO,
    Google,
    Microsoft
}

public enum LecturerLevel
{
    Basic,
    Advanced,
    Expert
}

public enum SemesterStatus
{
    Upcoming,
    Active,
    Closed
}

public enum ProjectStatus
{
    Draft,
    Active,
    Completed,
    Cancelled
}

public enum ProjectRole
{
    Main,
    Co
}

public enum GroupRole
{
    Leader,
    Member
}

public enum RoundType
{
    Online,
    Offline
}

public enum RoundStatus
{
    Planned,
    Ongoing,
    Completed
}

public enum SubmissionStatus
{
    OnTime,
    Late,
    Replaced
}

public enum EvaluationStatus
{
    Draft,
    Submitted
}

public enum ApprovalStatus
{
    Pending,
    Approved,
    Rejected,
    AutoReleased
}

public enum RoomType
{
    Classroom,
    Lab,
    Hall
}

public enum RoomStatus
{
    Available,
    Maintenance,
    Reserved
}

public enum NotificationType
{
    Deadline,
    Schedule,
    Feedback,
    Review,
    System
}
