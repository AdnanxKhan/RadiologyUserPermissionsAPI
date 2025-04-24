USE [master]
GO
/****** Object:  Database [ClientTask]    Script Date: 4/24/2025 5:46:12 PM ******/
CREATE DATABASE [ClientTask]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'ClientTask', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.LOCALDB\MSSQL\DATA\ClientTask.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'ClientTask_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.LOCALDB\MSSQL\DATA\ClientTask_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [ClientTask] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [ClientTask].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [ClientTask] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [ClientTask] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [ClientTask] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [ClientTask] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [ClientTask] SET ARITHABORT OFF 
GO
ALTER DATABASE [ClientTask] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [ClientTask] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [ClientTask] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [ClientTask] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [ClientTask] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [ClientTask] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [ClientTask] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [ClientTask] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [ClientTask] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [ClientTask] SET  DISABLE_BROKER 
GO
ALTER DATABASE [ClientTask] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [ClientTask] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [ClientTask] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [ClientTask] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [ClientTask] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [ClientTask] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [ClientTask] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [ClientTask] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [ClientTask] SET  MULTI_USER 
GO
ALTER DATABASE [ClientTask] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [ClientTask] SET DB_CHAINING OFF 
GO
ALTER DATABASE [ClientTask] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [ClientTask] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [ClientTask] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [ClientTask] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [ClientTask] SET QUERY_STORE = ON
GO
ALTER DATABASE [ClientTask] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [ClientTask]
GO
/****** Object:  Table [dbo].[AETitle]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AETitle](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AETitleModalityMapping]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AETitleModalityMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AETitleId] [int] NULL,
	[ModalityId] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Institution]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Institution](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[InstitutionAETitleMapping]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[InstitutionAETitleMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[InstitutionId] [int] NULL,
	[AETitleId] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Modality]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Modality](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Patient]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Patient](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[StudyInstanceUID] [varchar](255) NULL,
	[PatientName] [varchar](255) NULL,
	[PatientID] [varchar](50) NULL,
	[Age] [varchar](10) NULL,
	[Sex] [char](1) NULL,
	[StudyDescription] [varchar](255) NULL,
	[ModalityId] [int] NULL,
	[AETitleId] [int] NULL,
	[InstitutionId] [int] NULL,
	[ImageCount] [int] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PatientAttachments]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PatientAttachments](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NULL,
	[FileName] [nvarchar](255) NULL,
	[FileData] [varbinary](max) NULL,
	[UploadedBy] [int] NULL,
	[UploadDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PatientsHistory]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PatientsHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NULL,
	[UserId] [int] NULL,
	[History] [text] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedBy] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Reports]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reports](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[PatientId] [int] NULL,
	[RadiologistId] [int] NULL,
	[ReportText] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
	[Reviewed] [bit] NULL,
	[ReviewedBy] [int] NULL,
	[ReviewedAt] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ReportTemplates]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportTemplates](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](255) NULL,
	[TemplateText] [nvarchar](max) NULL,
	[CreatedBy] [int] NULL,
	[CreatedAt] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserAETitleMapping]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserAETitleMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[AETitleId] [int] NULL,
	[IsActive] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserCategory]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserCategory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](100) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserCategoryPermission]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserCategoryPermission](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserCategoryId] [int] NOT NULL,
	[UserPermissionId] [int] NOT NULL,
	[IsActive] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserInstitutionMapping]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserInstitutionMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[InstitutionId] [int] NULL,
	[IsActive] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserModalityMapping]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserModalityMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[ModalityId] [int] NULL,
	[IsActive] [bit] NULL,
	[CreatedDate] [datetime] NULL,
	[UpdatedDate] [datetime] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserPatientAssignmentMapping]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserPatientAssignmentMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[PatientId] [int] NULL,
	[AssignedAt] [datetime] NULL,
	[AssignedBy] [int] NULL,
	[UpdatedAt] [datetime] NULL,
	[UpdatedBy] [int] NULL,
	[IsActive] [bit] NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserPermission]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserPermission](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserPermissionMapping]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserPermissionMapping](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[UserCategoryId] [int] NOT NULL,
	[UserPermissionId] [int] NOT NULL,
	[IsAllowed] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[UpdatedAt] [datetime] NOT NULL
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](100) NOT NULL,
	[UserCategoryId] [int] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[CreatedByUserId] [int] NULL,
	[Password] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserSessions]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserSessions](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NULL,
	[LoginTime] [datetime] NULL,
	[IsActive] [bit] NULL,
	[SessionId] [uniqueidentifier] NULL,
	[LogoutTime] [datetime] NULL,
	[SessionKilled] [bit] NULL,
	[SessionKilledBy] [int] NULL
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[PatientsHistory] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserAETitleMapping] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[UserInstitutionMapping] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[UserModalityMapping] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[UserPatientAssignmentMapping] ADD  DEFAULT (getdate()) FOR [AssignedAt]
GO
ALTER TABLE [dbo].[UserPatientAssignmentMapping] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[UserPermissionMapping] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserPermissionMapping] ADD  DEFAULT (getdate()) FOR [UpdatedAt]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[UserSessions] ADD  DEFAULT (getdate()) FOR [LoginTime]
GO
ALTER TABLE [dbo].[UserSessions] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[UserSessions] ADD  DEFAULT ((0)) FOR [SessionKilled]
GO
/****** Object:  StoredProcedure [dbo].[CreateUserPermissions]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--EXEC CreateUserWithPermissions 'Technologist',5,3
CREATE PROCEDURE [dbo].[CreateUserPermissions] 
  @Username VARCHAR(100)
, @UserCategoryId INT
, @CreatedByUserId INT
AS
BEGIN
	-- Step 2: Fetch the creator's user category
	DECLARE @CreatorCategoryId INT;

	SELECT @CreatorCategoryId = UserCategoryId
	FROM Users
	WHERE Id = @CreatedByUserId;

	PRINT 'Creator Category ID: ' + CAST(@CreatorCategoryId AS VARCHAR);
	PRINT 'User Category to Create: ' + CAST(@UserCategoryId AS VARCHAR);

	-- Step 3: Check if the creator has permission to create a user
	DECLARE @CanCreateUser BIT = 0;

	PRINT 'CanCreateUser: ' + CAST(@CanCreateUser AS VARCHAR);

	-- Check creator permissions based on category
	IF @CreatorCategoryId = 1 -- Super Admin
	BEGIN
		SET @CanCreateUser = 1;
	END
	ELSE IF @CreatorCategoryId = 2 -- Admin
	BEGIN
		IF @UserCategoryId <> 1 -- Admin can't create Super Admin
		BEGIN
			SET @CanCreateUser = 1;
		END
	END
	ELSE IF @CreatorCategoryId = 3 -- Group Admin
	BEGIN
		IF @UserCategoryId BETWEEN 4 AND 7 -- Can only create Radiologists, Technologists, etc.
		BEGIN
			SET @CanCreateUser = 1;
		END
	END

	PRINT 'CanCreateUser: ' + CAST(@CanCreateUser AS VARCHAR);

	-- If the creator doesn't have permission, raise an error
	IF @CanCreateUser = 0
	BEGIN
		PRINT 'Permission denied. Final check failed.';

		RAISERROR ('You do not have permission to create this user.', 16, 1);

		RETURN;
	END

	-- Step 1: Insert the new user
	INSERT INTO Users (Username, UserCategoryId, CreatedByUserId)
	VALUES (@Username, @UserCategoryId, @CreatedByUserId);

	DECLARE @NewUserId INT = SCOPE_IDENTITY();-- Get the new user's ID
		-- Step 4: Insert permissions based on the user category
	DECLARE @PermissionId INT;

	-- Super Admin gets all permissions
	IF @UserCategoryId = 1
	BEGIN
		DECLARE permissions_cursor CURSOR
		FOR
		SELECT Id
		FROM UserPermission;

		OPEN permissions_cursor;

		FETCH NEXT
		FROM permissions_cursor
		INTO @PermissionId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO UserCategoryPermission (UserCategoryId, UserPermissionId, IsActive)
			VALUES (@UserCategoryId, @PermissionId, 1);

			FETCH NEXT
			FROM permissions_cursor
			INTO @PermissionId;
		END

		CLOSE permissions_cursor;

		DEALLOCATE permissions_cursor;
	END
			-- Admin gets all permissions except Delete Patient and Delete User
	ELSE IF @UserCategoryId = 2
	BEGIN
		DECLARE permissions_cursor CURSOR
		FOR
		SELECT Id
		FROM UserPermission
		WHERE Name NOT IN ('Delete Patient', 'Delete User');

		OPEN permissions_cursor;

		FETCH NEXT
		FROM permissions_cursor
		INTO @PermissionId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO UserCategoryPermission (UserCategoryId, UserPermissionId, IsActive)
			VALUES (@UserCategoryId, @PermissionId, 1);

			FETCH NEXT
			FROM permissions_cursor
			INTO @PermissionId;
		END

		CLOSE permissions_cursor;

		DEALLOCATE permissions_cursor;
	END
			-- Group Admin gets limited permissions
	ELSE IF @UserCategoryId = 3
	BEGIN
		DECLARE permissions_cursor CURSOR
		FOR
		SELECT Id
		FROM UserPermission
		WHERE Name IN ('Create User', 'Assigning', 'History Read', 'Attachment Read', 'Report View', 'Messaging');

		OPEN permissions_cursor;

		FETCH NEXT
		FROM permissions_cursor
		INTO @PermissionId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO UserCategoryPermission (UserCategoryId, UserPermissionId, IsActive)
			VALUES (@UserCategoryId, @PermissionId, 1);

			FETCH NEXT
			FROM permissions_cursor
			INTO @PermissionId;
		END

		CLOSE permissions_cursor;

		DEALLOCATE permissions_cursor;
	END
			-- Radiologists gets specific permissions
	ELSE IF @UserCategoryId = 4
	BEGIN
		DECLARE permissions_cursor CURSOR
		FOR
		SELECT Id
		FROM UserPermission
		WHERE Name IN ('History Read', 'Attachment Read', 'Report View', 'Report Review', 'Messaging');

		OPEN permissions_cursor;

		FETCH NEXT
		FROM permissions_cursor
		INTO @PermissionId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO UserCategoryPermission (UserCategoryId, UserPermissionId, IsActive)
			VALUES (@UserCategoryId, @PermissionId, 1);

			FETCH NEXT
			FROM permissions_cursor
			INTO @PermissionId;
		END

		CLOSE permissions_cursor;

		DEALLOCATE permissions_cursor;
	END
			-- Technologist gets specific permissions
	ELSE IF @UserCategoryId = 5
	BEGIN
		DECLARE permissions_cursor CURSOR
		FOR
		SELECT Id
		FROM UserPermission
		WHERE Name IN ('Assigning', 'Attachment Read', 'Messaging');

		OPEN permissions_cursor;

		FETCH NEXT
		FROM permissions_cursor
		INTO @PermissionId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO UserCategoryPermission (UserCategoryId, UserPermissionId, IsActive)
			VALUES (@UserCategoryId, @PermissionId, 1);

			FETCH NEXT
			FROM permissions_cursor
			INTO @PermissionId;
		END

		CLOSE permissions_cursor;

		DEALLOCATE permissions_cursor;
	END
			-- Reporting gets specific permissions
	ELSE IF @UserCategoryId = 6
	BEGIN
		DECLARE permissions_cursor CURSOR
		FOR
		SELECT Id
		FROM UserPermission
		WHERE Name IN ('Assigning', 'History Read', 'History Write', 'Attachment Read', 'Report View', 'Report Edit', 'Report Review', 'Report Validation', 'Messaging');

		OPEN permissions_cursor;

		FETCH NEXT
		FROM permissions_cursor
		INTO @PermissionId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO UserCategoryPermission (UserCategoryId, UserPermissionId, IsActive)
			VALUES (@UserCategoryId, @PermissionId, 1);

			FETCH NEXT
			FROM permissions_cursor
			INTO @PermissionId;
		END

		CLOSE permissions_cursor;

		DEALLOCATE permissions_cursor;
	END
			-- Referring Physician gets specific permissions
	ELSE IF @UserCategoryId = 7
	BEGIN
		DECLARE permissions_cursor CURSOR
		FOR
		SELECT Id
		FROM UserPermission
		WHERE Name IN ('History Read', 'Attachment Read', 'Report View', 'Messaging', 'Image View');

		OPEN permissions_cursor;

		FETCH NEXT
		FROM permissions_cursor
		INTO @PermissionId;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			INSERT INTO UserCategoryPermission (UserCategoryId, UserPermissionId, IsActive)
			VALUES (@UserCategoryId, @PermissionId, 1);

			FETCH NEXT
			FROM permissions_cursor
			INTO @PermissionId;
		END

		CLOSE permissions_cursor;

		DEALLOCATE permissions_cursor;
	END

	-- Additional user category permission assignments (Radiologist, Technologist, etc.) can be added similarly
	-- Return the newly created user ID
	SELECT @NewUserId AS NewUserId;
END;
GO
/****** Object:  StoredProcedure [dbo].[CreateUserWithPermissions]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreateUserWithPermissions] 
  @Username VARCHAR(100),
  @UserCategoryId INT,
  @CreatedByUserId INT,
  @Password NVARCHAR(MAX)
AS
BEGIN
  -- Step 2: Fetch the creator's user category
  DECLARE @CreatorCategoryId INT;
  SELECT @CreatorCategoryId = UserCategoryId
  FROM Users
  WHERE Id = @CreatedByUserId;

  -- Step 3: Check if the creator has permission to create a user
  DECLARE @CanCreateUser BIT = 0;

  -- Check creator permissions based on category
  IF @CreatorCategoryId = 1 -- Super Admin
  BEGIN
    SET @CanCreateUser = 1;
  END
  ELSE IF @CreatorCategoryId = 2 -- Admin
  BEGIN
    IF @UserCategoryId <> 1 -- Admin can't create Super Admin
    BEGIN
      SET @CanCreateUser = 1;
    END
  END
  ELSE IF @CreatorCategoryId = 3 -- Group Admin
  BEGIN
    IF @UserCategoryId BETWEEN 4 AND 7 -- Can only create Radiologists, Technologists, etc.
    BEGIN
      SET @CanCreateUser = 1;
    END
  END

  -- If the creator doesn't have permission, raise an error
  IF @CanCreateUser = 0
  BEGIN
    RAISERROR ('You do not have permission to create this user.', 16, 1);
    RETURN;
  END

  -- Step 1: Insert the new user into Users table
  INSERT INTO Users (Username, UserCategoryId, CreatedByUserId, Password)
  VALUES (@Username, @UserCategoryId, @CreatedByUserId, @Password);

  DECLARE @NewUserId INT = SCOPE_IDENTITY(); -- Get the new user's ID

  -- Step 4: Dynamically insert into UserPermissionMapping for this new user (if needed)
  -- Only if the user needs custom permissions or mappings
  -- If custom permissions are provided, they would be passed as part of a parameter (this could be a list of permissions)
  -- E.g., CreateUserDTO would carry custom permission IDs, and you'd insert those into UserPermissionMapping

  -- For now, let's assume that no custom permissions are being assigned, so we skip this

  -- Return the newly created user ID
  SELECT @NewUserId AS NewUserId;
END;
GO
/****** Object:  StoredProcedure [dbo].[GetPermissionsByCategory]    Script Date: 4/24/2025 5:46:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetPermissionsByCategory]
    @UserCategoryId INT
AS
BEGIN
    SELECT 
        up.Id AS UserPermissionId,
        up.Name,
        ucp.IsActive
    FROM UserPermission up
    INNER JOIN UserCategoryPermission ucp ON up.Id = ucp.UserPermissionId
    WHERE ucp.UserCategoryId = @UserCategoryId;
END
GO
USE [master]
GO
ALTER DATABASE [ClientTask] SET  READ_WRITE 
GO
