CREATE DATABASE GPMS_Database;
GO
use  GPMS_Database
IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

CREATE TABLE [Faculties] (
    [FacultyID] int NOT NULL IDENTITY,
    [FacultyCode] varchar(10) NOT NULL,
    [FacultyName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Faculties] PRIMARY KEY ([FacultyID])
);
GO

CREATE TABLE [Rooms] (
    [RoomID] int NOT NULL IDENTITY,
    [RoomCode] varchar(20) NOT NULL,
    [Building] nvarchar(100) NULL,
    [Capacity] int NOT NULL,
    [RoomType] nvarchar(20) NOT NULL DEFAULT N'Classroom',
    [Status] nvarchar(20) NOT NULL DEFAULT N'Available',
    [Notes] nvarchar(500) NULL,
    CONSTRAINT [PK_Rooms] PRIMARY KEY ([RoomID])
);
GO

CREATE TABLE [Semesters] (
    [SemesterID] int NOT NULL IDENTITY,
    [SemesterCode] varchar(10) NOT NULL,
    [AcademicYear] nvarchar(10) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Upcoming',
    CONSTRAINT [PK_Semesters] PRIMARY KEY ([SemesterID])
);
GO

CREATE TABLE [Users] (
    [UserID] nvarchar(20) NOT NULL,
    [Email] nvarchar(100) NULL,
    [Username] nvarchar(50) NULL,
    [FullName] nvarchar(100) NOT NULL,
    [Phone] nvarchar(15) NULL,
    [AvatarUrl] nvarchar(255) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Active',
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserID]),
    CONSTRAINT [CK_User_EmailOrUsername] CHECK ([Email] IS NOT NULL OR [Username] IS NOT NULL)
);
GO

CREATE TABLE [Majors] (
    [MajorID] int NOT NULL IDENTITY,
    [MajorCode] varchar(10) NOT NULL,
    [MajorName] nvarchar(100) NOT NULL,
    [FacultyID] int NOT NULL,
    CONSTRAINT [PK_Majors] PRIMARY KEY ([MajorID]),
    CONSTRAINT [FK_Majors_Faculties_FacultyID] FOREIGN KEY ([FacultyID]) REFERENCES [Faculties] ([FacultyID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ReviewRounds] (
    [ReviewRoundID] int NOT NULL IDENTITY,
    [SemesterID] int NOT NULL,
    [RoundNumber] int NOT NULL,
    [RoundType] nvarchar(20) NOT NULL,
    [StartDate] datetime2 NOT NULL,
    [EndDate] datetime2 NOT NULL,
    [SubmissionDeadline] datetime2 NOT NULL,
    [Description] nvarchar(500) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Planned',
    CONSTRAINT [PK_ReviewRounds] PRIMARY KEY ([ReviewRoundID]),
    CONSTRAINT [FK_ReviewRounds_Semesters_SemesterID] FOREIGN KEY ([SemesterID]) REFERENCES [Semesters] ([SemesterID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [Notifications] (
    [NotificationID] int NOT NULL IDENTITY,
    [RecipientID] nvarchar(20) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [Type] nvarchar(20) NOT NULL,
    [RelatedEntityType] nvarchar(50) NULL,
    [RelatedEntityID] int NULL,
    [IsRead] bit NOT NULL DEFAULT CAST(0 AS bit),
    [IsEmailSent] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [ReadAt] datetime2 NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([NotificationID]),
    CONSTRAINT [FK_Notifications_Users_RecipientID] FOREIGN KEY ([RecipientID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserCredentials] (
    [CredentialID] int NOT NULL IDENTITY,
    [UserID] nvarchar(20) NOT NULL,
    [AuthProvider] nvarchar(20) NOT NULL,
    [PasswordHash] nvarchar(255) NULL,
    [ExternalProviderID] nvarchar(255) NULL,
    [LastLoginAt] datetime2 NULL,
    [PasswordResetToken] nvarchar(255) NULL,
    [PasswordResetExpiry] datetime2 NULL,
    CONSTRAINT [PK_UserCredentials] PRIMARY KEY ([CredentialID]),
    CONSTRAINT [FK_UserCredentials_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);
GO

CREATE TABLE [UserRoles] (
    [UserRoleID] int NOT NULL IDENTITY,
    [UserID] nvarchar(20) NOT NULL,
    [RoleName] nvarchar(20) NOT NULL,
    [AssignedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserRoleID]),
    CONSTRAINT [FK_UserRoles_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);
GO

CREATE TABLE [ExpertiseAreas] (
    [ExpertiseID] int NOT NULL IDENTITY,
    [ExpertiseName] nvarchar(100) NOT NULL,
    [MajorID] int NULL,
    CONSTRAINT [PK_ExpertiseAreas] PRIMARY KEY ([ExpertiseID]),
    CONSTRAINT [FK_ExpertiseAreas_Majors_MajorID] FOREIGN KEY ([MajorID]) REFERENCES [Majors] ([MajorID]) ON DELETE SET NULL
);
GO

CREATE TABLE [Projects] (
    [ProjectID] int NOT NULL IDENTITY,
    [ProjectCode] varchar(20) NOT NULL,
    [ProjectName] nvarchar(200) NOT NULL,
    [Description] nvarchar(max) NULL,
    [SemesterID] int NOT NULL,
    [MajorID] int NOT NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Projects] PRIMARY KEY ([ProjectID]),
    CONSTRAINT [FK_Projects_Majors_MajorID] FOREIGN KEY ([MajorID]) REFERENCES [Majors] ([MajorID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Projects_Semesters_SemesterID] FOREIGN KEY ([SemesterID]) REFERENCES [Semesters] ([SemesterID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ReviewChecklists] (
    [ChecklistID] int NOT NULL IDENTITY,
    [ReviewRoundID] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [CreatedBy] nvarchar(20) NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_ReviewChecklists] PRIMARY KEY ([ChecklistID]),
    CONSTRAINT [FK_ReviewChecklists_ReviewRounds_ReviewRoundID] FOREIGN KEY ([ReviewRoundID]) REFERENCES [ReviewRounds] ([ReviewRoundID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReviewChecklists_Users_CreatedBy] FOREIGN KEY ([CreatedBy]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [SubmissionRequirements] (
    [RequirementID] int NOT NULL IDENTITY,
    [ReviewRoundID] int NOT NULL,
    [DocumentName] nvarchar(200) NOT NULL,
    [Description] nvarchar(500) NULL,
    [AllowedFormats] varchar(100) NULL,
    [MaxFileSizeMB] int NULL DEFAULT 50,
    [IsRequired] bit NOT NULL DEFAULT CAST(1 AS bit),
    [Deadline] datetime2 NOT NULL,
    CONSTRAINT [PK_SubmissionRequirements] PRIMARY KEY ([RequirementID]),
    CONSTRAINT [FK_SubmissionRequirements_ReviewRounds_ReviewRoundID] FOREIGN KEY ([ReviewRoundID]) REFERENCES [ReviewRounds] ([ReviewRoundID]) ON DELETE CASCADE
);
GO

CREATE TABLE [LecturerExpertise] (
    [LecturerID] nvarchar(20) NOT NULL,
    [ExpertiseID] int NOT NULL,
    [Level] nvarchar(20) NOT NULL,
    [IsPrimary] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_LecturerExpertise] PRIMARY KEY ([LecturerID], [ExpertiseID]),
    CONSTRAINT [FK_LecturerExpertise_ExpertiseAreas_ExpertiseID] FOREIGN KEY ([ExpertiseID]) REFERENCES [ExpertiseAreas] ([ExpertiseID]) ON DELETE CASCADE,
    CONSTRAINT [FK_LecturerExpertise_Users_LecturerID] FOREIGN KEY ([LecturerID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);
GO

CREATE TABLE [ProjectGroups] (
    [GroupID] int NOT NULL IDENTITY,
    [ProjectID] int NOT NULL,
    [GroupName] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_ProjectGroups] PRIMARY KEY ([GroupID]),
    CONSTRAINT [FK_ProjectGroups_Projects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [Projects] ([ProjectID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ProjectSupervisors] (
    [ProjectID] int NOT NULL,
    [LecturerID] nvarchar(20) NOT NULL,
    [Role] nvarchar(20) NOT NULL,
    [AssignedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [AssignedBy] nvarchar(20) NULL,
    CONSTRAINT [PK_ProjectSupervisors] PRIMARY KEY ([ProjectID], [LecturerID]),
    CONSTRAINT [FK_ProjectSupervisors_Projects_ProjectID] FOREIGN KEY ([ProjectID]) REFERENCES [Projects] ([ProjectID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProjectSupervisors_Users_AssignedBy] FOREIGN KEY ([AssignedBy]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProjectSupervisors_Users_LecturerID] FOREIGN KEY ([LecturerID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ChecklistItems] (
    [ItemID] int NOT NULL IDENTITY,
    [ChecklistID] int NOT NULL,
    [ItemCode] varchar(20) NOT NULL,
    [ItemContent] nvarchar(500) NOT NULL,
    [MaxScore] decimal(5,2) NOT NULL,
    [Weight] decimal(5,2) NOT NULL DEFAULT 1.0,
    [OrderIndex] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK_ChecklistItems] PRIMARY KEY ([ItemID]),
    CONSTRAINT [FK_ChecklistItems_ReviewChecklists_ChecklistID] FOREIGN KEY ([ChecklistID]) REFERENCES [ReviewChecklists] ([ChecklistID]) ON DELETE CASCADE
);
GO

CREATE TABLE [Evaluations] (
    [EvaluationID] int NOT NULL IDENTITY,
    [ReviewRoundID] int NOT NULL,
    [ReviewerID] nvarchar(20) NOT NULL,
    [GroupID] int NOT NULL,
    [TotalScore] decimal(6,2) NULL,
    [Status] nvarchar(20) NOT NULL DEFAULT N'Draft',
    [SubmittedAt] datetime2 NULL,
    CONSTRAINT [PK_Evaluations] PRIMARY KEY ([EvaluationID]),
    CONSTRAINT [FK_Evaluations_ProjectGroups_GroupID] FOREIGN KEY ([GroupID]) REFERENCES [ProjectGroups] ([GroupID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Evaluations_ReviewRounds_ReviewRoundID] FOREIGN KEY ([ReviewRoundID]) REFERENCES [ReviewRounds] ([ReviewRoundID]) ON DELETE CASCADE,
    CONSTRAINT [FK_Evaluations_Users_ReviewerID] FOREIGN KEY ([ReviewerID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [GroupMembers] (
    [GroupID] int NOT NULL,
    [UserID] nvarchar(20) NOT NULL,
    [RoleInGroup] nvarchar(20) NOT NULL,
    [JoinedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_GroupMembers] PRIMARY KEY ([GroupID], [UserID]),
    CONSTRAINT [FK_GroupMembers_ProjectGroups_GroupID] FOREIGN KEY ([GroupID]) REFERENCES [ProjectGroups] ([GroupID]) ON DELETE CASCADE,
    CONSTRAINT [FK_GroupMembers_Users_UserID] FOREIGN KEY ([UserID]) REFERENCES [Users] ([UserID]) ON DELETE CASCADE
);
GO

CREATE TABLE [ReviewerAssignments] (
    [AssignmentID] int NOT NULL IDENTITY,
    [ReviewRoundID] int NOT NULL,
    [GroupID] int NOT NULL,
    [ReviewerID] nvarchar(20) NOT NULL,
    [AssignedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [IsRandom] bit NOT NULL DEFAULT CAST(1 AS bit),
    [AssignedBy] nvarchar(20) NULL,
    CONSTRAINT [PK_ReviewerAssignments] PRIMARY KEY ([AssignmentID]),
    CONSTRAINT [FK_ReviewerAssignments_ProjectGroups_GroupID] FOREIGN KEY ([GroupID]) REFERENCES [ProjectGroups] ([GroupID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ReviewerAssignments_ReviewRounds_ReviewRoundID] FOREIGN KEY ([ReviewRoundID]) REFERENCES [ReviewRounds] ([ReviewRoundID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReviewerAssignments_Users_AssignedBy] FOREIGN KEY ([AssignedBy]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ReviewerAssignments_Users_ReviewerID] FOREIGN KEY ([ReviewerID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [ReviewSessionInfo] (
    [SessionID] int NOT NULL IDENTITY,
    [ReviewRoundID] int NOT NULL,
    [GroupID] int NOT NULL,
    [MeetLink] varchar(500) NULL,
    [ScheduledAt] datetime2 NOT NULL,
    [RoomID] int NULL,
    [Notes] nvarchar(500) NULL,
    CONSTRAINT [PK_ReviewSessionInfo] PRIMARY KEY ([SessionID]),
    CONSTRAINT [FK_ReviewSessionInfo_ProjectGroups_GroupID] FOREIGN KEY ([GroupID]) REFERENCES [ProjectGroups] ([GroupID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ReviewSessionInfo_ReviewRounds_ReviewRoundID] FOREIGN KEY ([ReviewRoundID]) REFERENCES [ReviewRounds] ([ReviewRoundID]) ON DELETE CASCADE,
    CONSTRAINT [FK_ReviewSessionInfo_Rooms_RoomID] FOREIGN KEY ([RoomID]) REFERENCES [Rooms] ([RoomID]) ON DELETE SET NULL
);
GO

CREATE TABLE [Submissions] (
    [SubmissionID] int NOT NULL IDENTITY,
    [RequirementID] int NOT NULL,
    [GroupID] int NOT NULL,
    [FileUrl] varchar(500) NOT NULL,
    [FileName] nvarchar(200) NOT NULL,
    [FileSizeMB] decimal(6,2) NULL,
    [SubmittedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [SubmittedBy] nvarchar(20) NOT NULL,
    [Status] nvarchar(20) NOT NULL,
    [Version] int NOT NULL DEFAULT 1,
    CONSTRAINT [PK_Submissions] PRIMARY KEY ([SubmissionID]),
    CONSTRAINT [FK_Submissions_ProjectGroups_GroupID] FOREIGN KEY ([GroupID]) REFERENCES [ProjectGroups] ([GroupID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Submissions_SubmissionRequirements_RequirementID] FOREIGN KEY ([RequirementID]) REFERENCES [SubmissionRequirements] ([RequirementID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Submissions_Users_SubmittedBy] FOREIGN KEY ([SubmittedBy]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);
GO

CREATE TABLE [EvaluationDetails] (
    [EvaluationID] int NOT NULL,
    [ItemID] int NOT NULL,
    [Score] decimal(5,2) NOT NULL,
    [Comment] nvarchar(max) NULL,
    CONSTRAINT [PK_EvaluationDetails] PRIMARY KEY ([EvaluationID], [ItemID]),
    CONSTRAINT [FK_EvaluationDetails_ChecklistItems_ItemID] FOREIGN KEY ([ItemID]) REFERENCES [ChecklistItems] ([ItemID]) ON DELETE NO ACTION,
    CONSTRAINT [FK_EvaluationDetails_Evaluations_EvaluationID] FOREIGN KEY ([EvaluationID]) REFERENCES [Evaluations] ([EvaluationID]) ON DELETE CASCADE
);
GO

CREATE TABLE [Feedbacks] (
    [FeedbackID] int NOT NULL IDENTITY,
    [EvaluationID] int NOT NULL,
    [Content] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_Feedbacks] PRIMARY KEY ([FeedbackID]),
    CONSTRAINT [FK_Feedbacks_Evaluations_EvaluationID] FOREIGN KEY ([EvaluationID]) REFERENCES [Evaluations] ([EvaluationID]) ON DELETE CASCADE
);
GO

CREATE TABLE [FeedbackApprovals] (
    [FeedbackID] int NOT NULL,
    [SupervisorID] nvarchar(20) NULL,
    [ApprovalStatus] nvarchar(20) NOT NULL DEFAULT N'Pending',
    [SupervisorComment] nvarchar(max) NULL,
    [ApprovedAt] datetime2 NULL,
    [AutoReleasedAt] datetime2 NULL,
    [IsVisibleToStudent] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_FeedbackApprovals] PRIMARY KEY ([FeedbackID]),
    CONSTRAINT [FK_FeedbackApprovals_Feedbacks_FeedbackID] FOREIGN KEY ([FeedbackID]) REFERENCES [Feedbacks] ([FeedbackID]) ON DELETE CASCADE,
    CONSTRAINT [FK_FeedbackApprovals_Users_SupervisorID] FOREIGN KEY ([SupervisorID]) REFERENCES [Users] ([UserID]) ON DELETE NO ACTION
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'FacultyID', N'FacultyCode', N'FacultyName') AND [object_id] = OBJECT_ID(N'[Faculties]'))
    SET IDENTITY_INSERT [Faculties] ON;
INSERT INTO [Faculties] ([FacultyID], [FacultyCode], [FacultyName])
VALUES (1, 'SE', N'Software Engineering Faculty');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'FacultyID', N'FacultyCode', N'FacultyName') AND [object_id] = OBJECT_ID(N'[Faculties]'))
    SET IDENTITY_INSERT [Faculties] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoomID', N'Building', N'Capacity', N'Notes', N'RoomCode') AND [object_id] = OBJECT_ID(N'[Rooms]'))
    SET IDENTITY_INSERT [Rooms] ON;
INSERT INTO [Rooms] ([RoomID], [Building], [Capacity], [Notes], [RoomCode])
VALUES (1, N'Alpha', 30, NULL, 'B1.01'),
(2, N'Alpha', 30, NULL, 'B1.02');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoomID', N'Building', N'Capacity', N'Notes', N'RoomCode') AND [object_id] = OBJECT_ID(N'[Rooms]'))
    SET IDENTITY_INSERT [Rooms] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoomID', N'Building', N'Capacity', N'Notes', N'RoomCode', N'RoomType') AND [object_id] = OBJECT_ID(N'[Rooms]'))
    SET IDENTITY_INSERT [Rooms] ON;
INSERT INTO [Rooms] ([RoomID], [Building], [Capacity], [Notes], [RoomCode], [RoomType])
VALUES (3, N'Delta', 100, NULL, 'Hall-A', N'Hall');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'RoomID', N'Building', N'Capacity', N'Notes', N'RoomCode', N'RoomType') AND [object_id] = OBJECT_ID(N'[Rooms]'))
    SET IDENTITY_INSERT [Rooms] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SemesterID', N'AcademicYear', N'EndDate', N'SemesterCode', N'StartDate', N'Status') AND [object_id] = OBJECT_ID(N'[Semesters]'))
    SET IDENTITY_INSERT [Semesters] ON;
INSERT INTO [Semesters] ([SemesterID], [AcademicYear], [EndDate], [SemesterCode], [StartDate], [Status])
VALUES (1, N'2024-2025', '2025-04-30T00:00:00.0000000', 'SP25', '2025-01-01T00:00:00.0000000', N'Active');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SemesterID', N'AcademicYear', N'EndDate', N'SemesterCode', N'StartDate', N'Status') AND [object_id] = OBJECT_ID(N'[Semesters]'))
    SET IDENTITY_INSERT [Semesters] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserID', N'AvatarUrl', N'CreatedAt', N'Email', N'FullName', N'Phone', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserID], [AvatarUrl], [CreatedAt], [Email], [FullName], [Phone], [Username])
VALUES (N'ADMIN001', NULL, '2026-02-27T02:47:38.5724329Z', N'admin@fpt.edu.vn', N'System Admin', NULL, N'admin'),
(N'GV001', NULL, '2026-02-27T02:47:38.5724340Z', N'giao-vien1@fpt.edu.vn', N'Lecturer One', NULL, NULL),
(N'GV002', NULL, '2026-02-27T02:47:38.5724343Z', N'giao-vien2@fpt.edu.vn', N'Lecturer Two', NULL, NULL),
(N'GV003', NULL, '2026-02-27T02:47:38.5724344Z', N'giao-vien3@fpt.edu.vn', N'Lecturer Three', NULL, NULL),
(N'SE180001', NULL, '2026-02-27T02:47:38.5724346Z', N'student1@fpt.edu.vn', N'Student One', NULL, NULL),
(N'SE180002', NULL, '2026-02-27T02:47:38.5724348Z', N'student2@fpt.edu.vn', N'Student Two', NULL, NULL),
(N'SE180003', NULL, '2026-02-27T02:47:38.5724349Z', N'student3@fpt.edu.vn', N'Student Three', NULL, NULL),
(N'SE180004', NULL, '2026-02-27T02:47:38.5724377Z', N'student4@fpt.edu.vn', N'Student Four', NULL, NULL),
(N'SE180005', NULL, '2026-02-27T02:47:38.5724379Z', N'student5@fpt.edu.vn', N'Student Five', NULL, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserID', N'AvatarUrl', N'CreatedAt', N'Email', N'FullName', N'Phone', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MajorID', N'FacultyID', N'MajorCode', N'MajorName') AND [object_id] = OBJECT_ID(N'[Majors]'))
    SET IDENTITY_INSERT [Majors] ON;
INSERT INTO [Majors] ([MajorID], [FacultyID], [MajorCode], [MajorName])
VALUES (1, 1, 'SE', N'Software Engineering'),
(2, 1, 'SS', N'Software Testing');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'MajorID', N'FacultyID', N'MajorCode', N'MajorName') AND [object_id] = OBJECT_ID(N'[Majors]'))
    SET IDENTITY_INSERT [Majors] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserRoleID', N'AssignedAt', N'RoleName', N'UserID') AND [object_id] = OBJECT_ID(N'[UserRoles]'))
    SET IDENTITY_INSERT [UserRoles] ON;
INSERT INTO [UserRoles] ([UserRoleID], [AssignedAt], [RoleName], [UserID])
VALUES (1, '2026-02-27T02:47:38.5724401Z', N'Admin', N'ADMIN001'),
(2, '2026-02-27T02:47:38.5724402Z', N'Lecturer', N'GV001'),
(3, '2026-02-27T02:47:38.5724403Z', N'Lecturer', N'GV002'),
(4, '2026-02-27T02:47:38.5724404Z', N'Lecturer', N'GV003'),
(5, '2026-02-27T02:47:38.5724405Z', N'Student', N'SE180001'),
(6, '2026-02-27T02:47:38.5724405Z', N'Student', N'SE180002'),
(7, '2026-02-27T02:47:38.5724406Z', N'Student', N'SE180003'),
(8, '2026-02-27T02:47:38.5724407Z', N'Student', N'SE180004'),
(9, '2026-02-27T02:47:38.5724408Z', N'Student', N'SE180005');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserRoleID', N'AssignedAt', N'RoleName', N'UserID') AND [object_id] = OBJECT_ID(N'[UserRoles]'))
    SET IDENTITY_INSERT [UserRoles] OFF;
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ExpertiseID', N'ExpertiseName', N'MajorID') AND [object_id] = OBJECT_ID(N'[ExpertiseAreas]'))
    SET IDENTITY_INSERT [ExpertiseAreas] ON;
INSERT INTO [ExpertiseAreas] ([ExpertiseID], [ExpertiseName], [MajorID])
VALUES (1, N'Web Development', 1),
(2, N'Mobile Development', 1),
(3, N'AI/ML', 1);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'ExpertiseID', N'ExpertiseName', N'MajorID') AND [object_id] = OBJECT_ID(N'[ExpertiseAreas]'))
    SET IDENTITY_INSERT [ExpertiseAreas] OFF;
GO

CREATE INDEX [IX_ChecklistItems_ChecklistID] ON [ChecklistItems] ([ChecklistID]);
GO

CREATE INDEX [IX_EvaluationDetails_ItemID] ON [EvaluationDetails] ([ItemID]);
GO

CREATE INDEX [IX_Evaluations_GroupID] ON [Evaluations] ([GroupID]);
GO

CREATE INDEX [IX_Evaluations_ReviewerID] ON [Evaluations] ([ReviewerID]);
GO

CREATE UNIQUE INDEX [IX_Evaluations_ReviewRoundID_ReviewerID_GroupID] ON [Evaluations] ([ReviewRoundID], [ReviewerID], [GroupID]);
GO

CREATE INDEX [IX_ExpertiseAreas_MajorID] ON [ExpertiseAreas] ([MajorID]);
GO

CREATE UNIQUE INDEX [IX_Faculties_FacultyCode] ON [Faculties] ([FacultyCode]);
GO

CREATE INDEX [IX_FeedbackApprovals_SupervisorID] ON [FeedbackApprovals] ([SupervisorID]);
GO

CREATE UNIQUE INDEX [IX_Feedbacks_EvaluationID] ON [Feedbacks] ([EvaluationID]);
GO

CREATE INDEX [IX_GroupMembers_UserID] ON [GroupMembers] ([UserID]);
GO

CREATE INDEX [IX_LecturerExpertise_ExpertiseID] ON [LecturerExpertise] ([ExpertiseID]);
GO

CREATE INDEX [IX_Majors_FacultyID] ON [Majors] ([FacultyID]);
GO

CREATE UNIQUE INDEX [IX_Majors_MajorCode] ON [Majors] ([MajorCode]);
GO

CREATE INDEX [IX_Notifications_RecipientID] ON [Notifications] ([RecipientID]);
GO

CREATE INDEX [IX_ProjectGroups_ProjectID] ON [ProjectGroups] ([ProjectID]);
GO

CREATE INDEX [IX_Projects_MajorID] ON [Projects] ([MajorID]);
GO

CREATE UNIQUE INDEX [IX_Projects_ProjectCode] ON [Projects] ([ProjectCode]);
GO

CREATE INDEX [IX_Projects_SemesterID] ON [Projects] ([SemesterID]);
GO

CREATE INDEX [IX_ProjectSupervisors_AssignedBy] ON [ProjectSupervisors] ([AssignedBy]);
GO

CREATE INDEX [IX_ProjectSupervisors_LecturerID] ON [ProjectSupervisors] ([LecturerID]);
GO

CREATE INDEX [IX_ReviewChecklists_CreatedBy] ON [ReviewChecklists] ([CreatedBy]);
GO

CREATE UNIQUE INDEX [IX_ReviewChecklists_ReviewRoundID] ON [ReviewChecklists] ([ReviewRoundID]);
GO

CREATE INDEX [IX_ReviewerAssignments_AssignedBy] ON [ReviewerAssignments] ([AssignedBy]);
GO

CREATE INDEX [IX_ReviewerAssignments_GroupID] ON [ReviewerAssignments] ([GroupID]);
GO

CREATE INDEX [IX_ReviewerAssignments_ReviewerID] ON [ReviewerAssignments] ([ReviewerID]);
GO

CREATE UNIQUE INDEX [IX_ReviewerAssignments_ReviewRoundID_GroupID_ReviewerID] ON [ReviewerAssignments] ([ReviewRoundID], [GroupID], [ReviewerID]);
GO

CREATE UNIQUE INDEX [IX_ReviewRounds_SemesterID_RoundNumber] ON [ReviewRounds] ([SemesterID], [RoundNumber]);
GO

CREATE INDEX [IX_ReviewSessionInfo_GroupID] ON [ReviewSessionInfo] ([GroupID]);
GO

CREATE UNIQUE INDEX [IX_ReviewSessionInfo_ReviewRoundID_GroupID] ON [ReviewSessionInfo] ([ReviewRoundID], [GroupID]);
GO

CREATE INDEX [IX_ReviewSessionInfo_RoomID] ON [ReviewSessionInfo] ([RoomID]);
GO

CREATE UNIQUE INDEX [IX_Rooms_RoomCode] ON [Rooms] ([RoomCode]);
GO

CREATE UNIQUE INDEX [IX_Semesters_SemesterCode] ON [Semesters] ([SemesterCode]);
GO

CREATE INDEX [IX_SubmissionRequirements_ReviewRoundID] ON [SubmissionRequirements] ([ReviewRoundID]);
GO

CREATE INDEX [IX_Submissions_GroupID] ON [Submissions] ([GroupID]);
GO

CREATE UNIQUE INDEX [IX_Submissions_RequirementID_GroupID_Version] ON [Submissions] ([RequirementID], [GroupID], [Version]);
GO

CREATE INDEX [IX_Submissions_SubmittedBy] ON [Submissions] ([SubmittedBy]);
GO

CREATE UNIQUE INDEX [IX_UserCredentials_UserID_AuthProvider] ON [UserCredentials] ([UserID], [AuthProvider]);
GO

CREATE INDEX [IX_UserRoles_UserID] ON [UserRoles] ([UserID]);
GO

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]) WHERE [Email] IS NOT NULL;
GO

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]) WHERE [Username] IS NOT NULL;
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260227024739_InitialCreate', N'8.0.0');
GO

COMMIT;
GO

-- add hashed password "123456"
TRUNCATE TABLE UserCredentials;
INSERT INTO UserCredentials (UserID, AuthProvider, PasswordHash) VALUES 
('ADMIN001', 0, '$2a$12$5rpNYY6iiwYuRGsm83m06.T77MdMmrgHgMHHZHmH9xE8sr5aGtNuW'), 
('GV001', 0, '$2a$12$5rpNYY6iiwYuRGsm83m06.T77MdMmrgHgMHHZHmH9xE8sr5aGtNuW'), 
('SE180001', 0, '$2a$12$5rpNYY6iiwYuRGsm83m06.T77MdMmrgHgMHHZHmH9xE8sr5aGtNuW');
GO