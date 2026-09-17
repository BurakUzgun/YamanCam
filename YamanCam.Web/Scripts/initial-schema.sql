SET QUOTED_IDENTIFIER ON
GO

/*
  Yaman Cam - Veritabani sema scripti
  - Yeni kurulum: tablolari olusturur
  - Mevcut kurulum: eksik kolonlari ekler (COL_LENGTH ile idempotent)
  - App_SchemaVersion ile surum takibi yapilir

  Guncel surum: 16
*/
GO

/* ------------------------------------------------------------------ */
/* App_SchemaVersion - sema surum takibi                              */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_SchemaVersion](
        [RecId]       [int] IDENTITY(1,1) NOT NULL,
        [VersionNo]   [int] NOT NULL,
        [ScriptName]  [nvarchar](100) NOT NULL,
        [Description] [nvarchar](500) NULL,
        [AppliedUtc]  [datetime2](0) NOT NULL CONSTRAINT [DF_App_SchemaVersion_AppliedUtc] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_SchemaVersion] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.App_SchemaVersion', 'VersionNo') IS NULL
BEGIN
    ALTER TABLE [dbo].[App_SchemaVersion] ADD [VersionNo] [int] NOT NULL CONSTRAINT [DF_App_SchemaVersion_VersionNo] DEFAULT (0);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.App_SchemaVersion', 'ScriptName') IS NULL
BEGIN
    ALTER TABLE [dbo].[App_SchemaVersion] ADD [ScriptName] [nvarchar](100) NOT NULL CONSTRAINT [DF_App_SchemaVersion_ScriptName] DEFAULT (N'YamanCam.Core');
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.App_SchemaVersion', 'Description') IS NULL
BEGIN
    ALTER TABLE [dbo].[App_SchemaVersion] ADD [Description] [nvarchar](500) NULL;
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.App_SchemaVersion', 'AppliedUtc') IS NULL
BEGIN
    ALTER TABLE [dbo].[App_SchemaVersion] ADD [AppliedUtc] [datetime2](0) NOT NULL CONSTRAINT [DF_App_SchemaVersion_AppliedUtc2] DEFAULT (SYSUTCDATETIME());
END
GO

/* ================================================================== */
/* SURUM 1 - App_Company, App_WorkPlace, App_User, App_UserWorkPlace, */
/*           App_UserRight                                            */
/* ================================================================== */

/* ------------------------------------------------------------------ */
/* App_Company                                                        */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_Company](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [CompanyCode]  [nvarchar](50) NULL,
        [CompanyName]  [nvarchar](200) NULL,
        [TaxNumber]    [nvarchar](20) NULL,
        [TaxOffice]    [nvarchar](100) NULL,
        [Address]      [nvarchar](500) NULL,
        [City]         [nvarchar](100) NULL,
        [Phone]        [nvarchar](30) NULL,
        [Email]        [nvarchar](150) NULL,
        [IsActive]     [bit] NULL CONSTRAINT [DF_App_Company_IsActive] DEFAULT (1),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_Company_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_Company] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'CompanyCode') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [CompanyCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'CompanyName') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [CompanyName] [nvarchar](200) NULL;
GO
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'TaxNumber') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [TaxNumber] [nvarchar](20) NULL;
GO
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'TaxOffice') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [TaxOffice] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'Address') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [Address] [nvarchar](500) NULL;
GO
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'City') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [City] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'Phone') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [Phone] [nvarchar](30) NULL;
GO
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'Email') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [Email] [nvarchar](150) NULL;
GO
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_Company_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Company', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_Company] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_Company_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

/* ------------------------------------------------------------------ */
/* App_WorkPlace (isyeri / sube)                                      */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_WorkPlace](
        [RecId]           [int] IDENTITY(1,1) NOT NULL,
        [WorkPlaceCode]   [nvarchar](50) NULL,
        [WorkPlaceName]   [nvarchar](200) NULL,
        [CompanyId]       [int] NOT NULL,
        [Address]         [nvarchar](500) NULL,
        [City]            [nvarchar](100) NULL,
        [Phone]           [nvarchar](30) NULL,
        [IsDefault]       [bit] NULL CONSTRAINT [DF_App_WorkPlace_IsDefault] DEFAULT (0),
        [IsActive]        [bit] NULL CONSTRAINT [DF_App_WorkPlace_IsActive] DEFAULT (1),
        [CreatedDate]     [datetime2](0) NULL CONSTRAINT [DF_App_WorkPlace_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_WorkPlace] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_WorkPlace', 'WorkPlaceCode') IS NULL
    ALTER TABLE [dbo].[App_WorkPlace] ADD [WorkPlaceCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_WorkPlace', 'WorkPlaceName') IS NULL
    ALTER TABLE [dbo].[App_WorkPlace] ADD [WorkPlaceName] [nvarchar](200) NULL;
GO
IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_WorkPlace', 'CompanyId') IS NULL
    ALTER TABLE [dbo].[App_WorkPlace] ADD [CompanyId] [int] NOT NULL CONSTRAINT [DF_App_WorkPlace_CompanyId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_WorkPlace', 'Address') IS NULL
    ALTER TABLE [dbo].[App_WorkPlace] ADD [Address] [nvarchar](500) NULL;
GO
IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_WorkPlace', 'City') IS NULL
    ALTER TABLE [dbo].[App_WorkPlace] ADD [City] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_WorkPlace', 'Phone') IS NULL
    ALTER TABLE [dbo].[App_WorkPlace] ADD [Phone] [nvarchar](30) NULL;
GO
IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_WorkPlace', 'IsDefault') IS NULL
    ALTER TABLE [dbo].[App_WorkPlace] ADD [IsDefault] [bit] NULL CONSTRAINT [DF_App_WorkPlace_IsDefault_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_WorkPlace', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_WorkPlace] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_WorkPlace_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_WorkPlace', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_WorkPlace] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_WorkPlace_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_WorkPlace_App_Company')
BEGIN
    ALTER TABLE [dbo].[App_WorkPlace] WITH CHECK
    ADD CONSTRAINT [FK_App_WorkPlace_App_Company]
    FOREIGN KEY ([CompanyId]) REFERENCES [dbo].[App_Company]([RecId]);
END
GO

/* ------------------------------------------------------------------ */
/* App_User                                                           */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_User', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_User](
        [RecId]         [int] IDENTITY(1,1) NOT NULL,
        [Code]          [nvarchar](50) NULL,
        [NameSurname]   [nvarchar](100) NULL,
        [Password]      [nvarchar](100) NULL,
        [Email]         [nvarchar](150) NULL,
        [Phone]         [nvarchar](30) NULL,
        [IsRight]       [bit] NULL CONSTRAINT [DF_App_User_IsRight] DEFAULT (0),
        [IsActive]      [bit] NULL CONSTRAINT [DF_App_User_IsActive] DEFAULT (1),
        [LastLoginDate] [datetime2](0) NULL,
        [CreatedDate]   [datetime2](0) NULL CONSTRAINT [DF_App_User_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_User] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'Code') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [Code] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'NameSurname') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [NameSurname] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'Password') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [Password] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'Email') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [Email] [nvarchar](150) NULL;
GO
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'Phone') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [Phone] [nvarchar](30) NULL;
GO
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'IsRight') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [IsRight] [bit] NULL CONSTRAINT [DF_App_User_IsRight_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_User_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'LastLoginDate') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [LastLoginDate] [datetime2](0) NULL;
GO
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_User_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

/* Eski HotelsId kolonu (geriye uyumluluk; yeni yetki App_UserWorkPlace uzerinden) */
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_User', 'HotelsId') IS NULL
    ALTER TABLE [dbo].[App_User] ADD [HotelsId] [nvarchar](1000) NULL;
GO

/* ------------------------------------------------------------------ */
/* App_UserWorkPlace - kullanici / isyeri (coklu sube yetkisi)        */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_UserWorkPlace', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_UserWorkPlace](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [UserId]       [int] NOT NULL,
        [WorkPlaceId]  [int] NOT NULL,
        [IsDefault]    [bit] NULL CONSTRAINT [DF_App_UserWorkPlace_IsDefault] DEFAULT (0),
        [IsActive]     [bit] NULL CONSTRAINT [DF_App_UserWorkPlace_IsActive] DEFAULT (1),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_UserWorkPlace_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_UserWorkPlace] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_UserWorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserWorkPlace', 'UserId') IS NULL
    ALTER TABLE [dbo].[App_UserWorkPlace] ADD [UserId] [int] NOT NULL CONSTRAINT [DF_App_UserWorkPlace_UserId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_UserWorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserWorkPlace', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_UserWorkPlace] ADD [WorkPlaceId] [int] NOT NULL CONSTRAINT [DF_App_UserWorkPlace_WorkPlaceId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_UserWorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserWorkPlace', 'IsDefault') IS NULL
    ALTER TABLE [dbo].[App_UserWorkPlace] ADD [IsDefault] [bit] NULL CONSTRAINT [DF_App_UserWorkPlace_IsDefault_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_UserWorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserWorkPlace', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_UserWorkPlace] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_UserWorkPlace_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_UserWorkPlace', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserWorkPlace', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_UserWorkPlace] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_UserWorkPlace_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_UserWorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_UserWorkPlace_App_User')
BEGIN
    ALTER TABLE [dbo].[App_UserWorkPlace] WITH CHECK
    ADD CONSTRAINT [FK_App_UserWorkPlace_App_User]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[App_User]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_UserWorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_UserWorkPlace_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_UserWorkPlace] WITH CHECK
    ADD CONSTRAINT [FK_App_UserWorkPlace_App_WorkPlace]
    FOREIGN KEY ([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_UserWorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_UserWorkPlace_User_WorkPlace' AND object_id = OBJECT_ID(N'dbo.App_UserWorkPlace'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_UserWorkPlace_User_WorkPlace]
    ON [dbo].[App_UserWorkPlace]([UserId], [WorkPlaceId]);
END
GO

/* ------------------------------------------------------------------ */
/* App_UserRight - kullanici yetkilendirme                            */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_UserRight](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [UserId]       [int] NOT NULL,
        [WorkPlaceId]  [int] NULL,
        [RightCode]    [nvarchar](100) NOT NULL,
        [RightName]    [nvarchar](200) NULL,
        [IsAllowed]    [bit] NULL CONSTRAINT [DF_App_UserRight_IsAllowed] DEFAULT (1),
        [IsActive]     [bit] NULL CONSTRAINT [DF_App_UserRight_IsActive] DEFAULT (1),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_UserRight_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_UserRight] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserRight', 'UserId') IS NULL
    ALTER TABLE [dbo].[App_UserRight] ADD [UserId] [int] NOT NULL CONSTRAINT [DF_App_UserRight_UserId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserRight', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_UserRight] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserRight', 'RightCode') IS NULL
    ALTER TABLE [dbo].[App_UserRight] ADD [RightCode] [nvarchar](100) NOT NULL CONSTRAINT [DF_App_UserRight_RightCode] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserRight', 'RightName') IS NULL
    ALTER TABLE [dbo].[App_UserRight] ADD [RightName] [nvarchar](200) NULL;
GO
IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserRight', 'IsAllowed') IS NULL
    ALTER TABLE [dbo].[App_UserRight] ADD [IsAllowed] [bit] NULL CONSTRAINT [DF_App_UserRight_IsAllowed_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserRight', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_UserRight] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_UserRight_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_UserRight', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_UserRight] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_UserRight_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_UserRight_App_User')
BEGIN
    ALTER TABLE [dbo].[App_UserRight] WITH CHECK
    ADD CONSTRAINT [FK_App_UserRight_App_User]
    FOREIGN KEY ([UserId]) REFERENCES [dbo].[App_User]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_UserRight_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_UserRight] WITH CHECK
    ADD CONSTRAINT [FK_App_UserRight_App_WorkPlace]
    FOREIGN KEY ([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_UserRight', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_UserRight_User_Right' AND object_id = OBJECT_ID(N'dbo.App_UserRight'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_UserRight_User_Right]
    ON [dbo].[App_UserRight]([UserId], [RightCode], [WorkPlaceId]);
END
GO

/* ------------------------------------------------------------------ */
/* Surum 1 - varsayilan veriler                                       */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_User', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM [dbo].[App_User])
BEGIN
    INSERT INTO [dbo].[App_User] ([Code], [NameSurname], [Password], [IsRight], [IsActive])
    VALUES (N'Center', N'Center', N'1234', 1, 1);
END
GO

/* ------------------------------------------------------------------ */
/* Surum 2 - App_Log                                                  */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_Log](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [LogDateUtc]   [datetime2](0) NOT NULL CONSTRAINT [DF_App_Log_LogDateUtc] DEFAULT (SYSUTCDATETIME()),
        [Level]        [nvarchar](20) NOT NULL,
        [Category]     [nvarchar](100) NULL,
        [EventName]    [nvarchar](150) NULL,
        [Message]      [nvarchar](2000) NOT NULL,
        [OldValue]     [nvarchar](4000) NULL,
        [NewValue]     [nvarchar](4000) NULL,
        [UserName]     [nvarchar](100) NULL,
        [UserId]       [int] NULL,
        [IpAddress]    [nvarchar](50) NULL,
        [Method]       [nvarchar](10) NULL,
        [Path]         [nvarchar](300) NULL,
        [QueryString]  [nvarchar](1000) NULL,
        [StatusCode]   [int] NULL,
        [DurationMs]   [bigint] NULL,
        CONSTRAINT [PK_App_Log] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'LogDateUtc') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [LogDateUtc] [datetime2](0) NOT NULL CONSTRAINT [DF_App_Log_LogDateUtc_Alt] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'Level') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [Level] [nvarchar](20) NOT NULL CONSTRAINT [DF_App_Log_Level] DEFAULT (N'Info');
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'Category') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [Category] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'EventName') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [EventName] [nvarchar](150) NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'Message') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [Message] [nvarchar](2000) NOT NULL CONSTRAINT [DF_App_Log_Message] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'OldValue') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [OldValue] [nvarchar](4000) NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'NewValue') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [NewValue] [nvarchar](4000) NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'UserName') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [UserName] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'UserId') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [UserId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'IpAddress') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [IpAddress] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'Method') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [Method] [nvarchar](10) NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'Path') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [Path] [nvarchar](300) NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'QueryString') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [QueryString] [nvarchar](1000) NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'StatusCode') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [StatusCode] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Log', 'DurationMs') IS NULL
    ALTER TABLE [dbo].[App_Log] ADD [DurationMs] [bigint] NULL;
GO

IF OBJECT_ID(N'dbo.App_Log', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_Log_LogDateUtc' AND object_id = OBJECT_ID(N'dbo.App_Log'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_Log_LogDateUtc]
    ON [dbo].[App_Log]([LogDateUtc] DESC);
END
GO

/* Surum kaydi */
IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 1
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (1, N'YamanCam.Core', N'App_Company, App_WorkPlace, App_User, App_UserWorkPlace, App_UserRight', SYSUTCDATETIME());
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 2
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (2, N'YamanCam.Core', N'App_Log merkezi log altyapisi', SYSUTCDATETIME());
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 3
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (3, N'YamanCam.Core', N'App_Log degisim izleme alanlari (OldValue/NewValue)', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 4 - App_AccountPlan (Muhasebe Hesap Plani)                   */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_AccountPlan](
        [RecId]            [int] IDENTITY(1,1) NOT NULL,
        [AccountCode]      [nvarchar](50) NOT NULL,
        [AccountName]      [nvarchar](200) NOT NULL,
        [ParentAccountId]  [int] NULL,
        [LevelNo]          [int] NULL CONSTRAINT [DF_App_AccountPlan_LevelNo] DEFAULT (1),
        [AccountType]      [nvarchar](30) NULL,
        [BalanceType]      [nvarchar](1) NULL,
        [CurrencyCode]     [nvarchar](3) NOT NULL CONSTRAINT [DF_App_AccountPlan_CurrencyCode] DEFAULT (N'TRY'),
        [SpecialCode]      [nvarchar](50) NULL,
        [Tax]              [nvarchar](100) NULL,
        [TaxNo]            [nvarchar](20) NULL,
        [Address]          [nvarchar](500) NULL,
        [City]             [nvarchar](100) NULL,
        [Country]          [nvarchar](100) NULL,
        [EMail]            [nvarchar](150) NULL,
        [Person]           [nvarchar](100) NULL,
        [Tel]              [nvarchar](30) NULL,
        [Fax]              [nvarchar](30) NULL,
        [Gsm]              [nvarchar](30) NULL,
        [IsDetail]         [bit] NULL CONSTRAINT [DF_App_AccountPlan_IsDetail] DEFAULT (1),
        [IsActive]         [bit] NULL CONSTRAINT [DF_App_AccountPlan_IsActive] DEFAULT (1),
        [CreatedDate]      [datetime2](0) NULL CONSTRAINT [DF_App_AccountPlan_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_AccountPlan] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'AccountCode') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [AccountCode] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_AccountPlan_AccountCode] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'AccountName') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [AccountName] [nvarchar](200) NOT NULL CONSTRAINT [DF_App_AccountPlan_AccountName] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'ParentAccountId') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [ParentAccountId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'LevelNo') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [LevelNo] [int] NULL CONSTRAINT [DF_App_AccountPlan_LevelNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'AccountType') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [AccountType] [nvarchar](30) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'BalanceType') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [BalanceType] [nvarchar](1) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'CurrencyCode') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [CurrencyCode] [nvarchar](3) NOT NULL CONSTRAINT [DF_App_AccountPlan_CurrencyCode_Alt] DEFAULT (N'TRY');
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'IsDetail') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [IsDetail] [bit] NULL CONSTRAINT [DF_App_AccountPlan_IsDetail_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_AccountPlan_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_AccountPlan_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_AccountPlan_Parent')
BEGIN
    ALTER TABLE [dbo].[App_AccountPlan] WITH CHECK
    ADD CONSTRAINT [FK_App_AccountPlan_Parent]
    FOREIGN KEY([ParentAccountId]) REFERENCES [dbo].[App_AccountPlan]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_AccountPlan_AccountCode' AND object_id = OBJECT_ID(N'dbo.App_AccountPlan'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_AccountPlan_AccountCode]
    ON [dbo].[App_AccountPlan]([AccountCode]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 4
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (4, N'YamanCam.Core', N'App_AccountPlan muhasebe hesap plani tablosu', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 5 - App_AccountPlan global (CompanyId kaldirildi)              */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_AccountPlan_App_Company')
BEGIN
    ALTER TABLE [dbo].[App_AccountPlan] DROP CONSTRAINT [FK_App_AccountPlan_App_Company];
END
GO

IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_AccountPlan_Company_AccountCode' AND object_id = OBJECT_ID(N'dbo.App_AccountPlan'))
BEGIN
    DROP INDEX [UX_App_AccountPlan_Company_AccountCode] ON [dbo].[App_AccountPlan];
END
GO

IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.App_AccountPlan', 'CompanyId') IS NOT NULL
BEGIN
    DECLARE @dfAccountPlanCompanyId nvarchar(200);
    SELECT @dfAccountPlanCompanyId = dc.name
    FROM sys.default_constraints dc
    INNER JOIN sys.columns c ON dc.parent_object_id = c.object_id AND dc.parent_column_id = c.column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.App_AccountPlan') AND c.name = N'CompanyId';
    IF @dfAccountPlanCompanyId IS NOT NULL
        EXEC(N'ALTER TABLE [dbo].[App_AccountPlan] DROP CONSTRAINT [' + @dfAccountPlanCompanyId + N']');
    ALTER TABLE [dbo].[App_AccountPlan] DROP COLUMN [CompanyId];
END
GO

IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_AccountPlan_AccountCode' AND object_id = OBJECT_ID(N'dbo.App_AccountPlan'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_AccountPlan_AccountCode]
    ON [dbo].[App_AccountPlan]([AccountCode]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 5
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (5, N'YamanCam.Core', N'App_AccountPlan global hesap plani (CompanyId kaldirildi)', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 6 - App_AccountPlan CurrencyCode                             */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'CurrencyCode') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [CurrencyCode] [nvarchar](3) NOT NULL CONSTRAINT [DF_App_AccountPlan_CurrencyCode_V6] DEFAULT (N'TRY');
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 6
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (6, N'YamanCam.Core', N'App_AccountPlan doviz alani (CurrencyCode)', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 7 - App_VatDefinition (KDV Tanimlari)                        */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_VatDefinition](
        [RecId]                    [int] IDENTITY(1,1) NOT NULL,
        [VatCode]                  [nvarchar](20) NOT NULL,
        [VatName]                  [nvarchar](100) NOT NULL,
        [VatRate]                  [decimal](13, 10) NOT NULL,
        [PurchaseAccountCode]      [nvarchar](50) NULL,
        [SalesAccountCode]         [nvarchar](50) NULL,
        [PurchaseReturnAccountCode] [nvarchar](50) NULL,
        [SalesReturnAccountCode]   [nvarchar](50) NULL,
        [IsActive]                 [bit] NULL CONSTRAINT [DF_App_VatDefinition_IsActive] DEFAULT (1),
        [CreatedDate]              [datetime2](0) NULL CONSTRAINT [DF_App_VatDefinition_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_VatDefinition] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatDefinition', 'VatCode') IS NULL
    ALTER TABLE [dbo].[App_VatDefinition] ADD [VatCode] [nvarchar](20) NOT NULL CONSTRAINT [DF_App_VatDefinition_VatCode] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatDefinition', 'VatName') IS NULL
    ALTER TABLE [dbo].[App_VatDefinition] ADD [VatName] [nvarchar](100) NOT NULL CONSTRAINT [DF_App_VatDefinition_VatName] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatDefinition', 'VatRate') IS NULL
    ALTER TABLE [dbo].[App_VatDefinition] ADD [VatRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_VatDefinition_VatRate] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatDefinition', 'PurchaseAccountCode') IS NULL
    ALTER TABLE [dbo].[App_VatDefinition] ADD [PurchaseAccountCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatDefinition', 'SalesAccountCode') IS NULL
    ALTER TABLE [dbo].[App_VatDefinition] ADD [SalesAccountCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatDefinition', 'PurchaseReturnAccountCode') IS NULL
    ALTER TABLE [dbo].[App_VatDefinition] ADD [PurchaseReturnAccountCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatDefinition', 'SalesReturnAccountCode') IS NULL
    ALTER TABLE [dbo].[App_VatDefinition] ADD [SalesReturnAccountCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatDefinition', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_VatDefinition] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_VatDefinition_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatDefinition', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_VatDefinition] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_VatDefinition_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_VatDefinition_VatCode' AND object_id = OBJECT_ID(N'dbo.App_VatDefinition'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_VatDefinition_VatCode]
    ON [dbo].[App_VatDefinition]([VatCode]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 7
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (7, N'YamanCam.Core', N'App_VatDefinition KDV tanimlari tablosu', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 8 - App_AccountPlan iletisim ve ek alanlar                   */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'SpecialCode') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [SpecialCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'Tax') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [Tax] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'TaxNo') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [TaxNo] [nvarchar](20) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'Address') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [Address] [nvarchar](500) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'City') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [City] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'Country') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [Country] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'EMail') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [EMail] [nvarchar](150) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'Person') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [Person] [nvarchar](100) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'Tel') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [Tel] [nvarchar](30) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'Fax') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [Fax] [nvarchar](30) NULL;
GO
IF OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_AccountPlan', 'Gsm') IS NULL
    ALTER TABLE [dbo].[App_AccountPlan] ADD [Gsm] [nvarchar](30) NULL;
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 8
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (8, N'YamanCam.Core', N'App_AccountPlan iletisim ve ek alanlar', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 9 - App_StockGroup (Stok Grup)                               */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockGroup', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockGroup](
        [RecId]               [int] IDENTITY(1,1) NOT NULL,
        [GroupCode]           [nvarchar](50) NOT NULL,
        [GroupName]           [nvarchar](200) NOT NULL,
        [PurchaseAccountCode] [nvarchar](50) NULL,
        [SalesAccountCode]    [nvarchar](50) NULL,
        [IsActive]            [bit] NULL CONSTRAINT [DF_App_StockGroup_IsActive] DEFAULT (1),
        [CreatedDate]         [datetime2](0) NULL CONSTRAINT [DF_App_StockGroup_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockGroup] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockGroup', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockGroup', 'GroupCode') IS NULL
    ALTER TABLE [dbo].[App_StockGroup] ADD [GroupCode] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_StockGroup_GroupCode] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_StockGroup', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockGroup', 'GroupName') IS NULL
    ALTER TABLE [dbo].[App_StockGroup] ADD [GroupName] [nvarchar](200) NOT NULL CONSTRAINT [DF_App_StockGroup_GroupName] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_StockGroup', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockGroup', 'PurchaseAccountCode') IS NULL
    ALTER TABLE [dbo].[App_StockGroup] ADD [PurchaseAccountCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_StockGroup', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockGroup', 'SalesAccountCode') IS NULL
    ALTER TABLE [dbo].[App_StockGroup] ADD [SalesAccountCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_StockGroup', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockGroup', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_StockGroup] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_StockGroup_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockGroup', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockGroup', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockGroup] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockGroup_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockGroup', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_StockGroup_GroupCode' AND object_id = OBJECT_ID(N'dbo.App_StockGroup'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_StockGroup_GroupCode]
    ON [dbo].[App_StockGroup]([GroupCode]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 9
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (9, N'YamanCam.Core', N'App_StockGroup stok grup tablosu', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 10 - App_StockUnit (Stok Birimleri)                          */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockUnit](
        [RecId]       [int] IDENTITY(1,1) NOT NULL,
        [UnitCode]    [nvarchar](20) NOT NULL,
        [UnitName]    [nvarchar](100) NOT NULL,
        [IsActive]    [bit] NULL CONSTRAINT [DF_App_StockUnit_IsActive] DEFAULT (1),
        [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockUnit_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockUnit] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockUnit', 'UnitCode') IS NULL
    ALTER TABLE [dbo].[App_StockUnit] ADD [UnitCode] [nvarchar](20) NOT NULL CONSTRAINT [DF_App_StockUnit_UnitCode] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockUnit', 'UnitName') IS NULL
    ALTER TABLE [dbo].[App_StockUnit] ADD [UnitName] [nvarchar](100) NOT NULL CONSTRAINT [DF_App_StockUnit_UnitName] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockUnit', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_StockUnit] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_StockUnit_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockUnit', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockUnit] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockUnit_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_StockUnit_UnitCode' AND object_id = OBJECT_ID(N'dbo.App_StockUnit'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_StockUnit_UnitCode]
    ON [dbo].[App_StockUnit]([UnitCode]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 10
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (10, N'YamanCam.Core', N'App_StockUnit stok birimleri tablosu', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 11 - App_Stock (Stoklar)                                     */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_Stock](
        [RecId]              [int] IDENTITY(1,1) NOT NULL,
        [StockCode]          [nvarchar](50) NOT NULL,
        [StockName]          [nvarchar](200) NOT NULL,
        [StockGroupId]       [int] NULL,
        [StockUnitId]        [int] NULL,
        [PurchaseVatId]      [int] NULL,
        [SalesVatId]         [int] NULL,
        [Barcode]            [nvarchar](50) NULL,
        [StockType]          [nvarchar](30) NULL,
        [ProductionWeight]   [decimal](28, 10) NULL,
        [IsActive]           [bit] NULL CONSTRAINT [DF_App_Stock_IsActive] DEFAULT (1),
        [CreatedDate]        [datetime2](0) NULL CONSTRAINT [DF_App_Stock_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_Stock] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'StockCode') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [StockCode] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_Stock_StockCode] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'StockName') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [StockName] [nvarchar](200) NOT NULL CONSTRAINT [DF_App_Stock_StockName] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'StockGroupId') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [StockGroupId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'StockUnitId') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [StockUnitId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'PurchaseVatId') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [PurchaseVatId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'SalesVatId') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [SalesVatId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'Barcode') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [Barcode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'StockType') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [StockType] [nvarchar](30) NULL;
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'ProductionWeight') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [ProductionWeight] [decimal](28, 10) NULL;
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_Stock_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_Stock_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockGroup', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_Stock_App_StockGroup')
BEGIN
    ALTER TABLE [dbo].[App_Stock] WITH CHECK
    ADD CONSTRAINT [FK_App_Stock_App_StockGroup]
    FOREIGN KEY([StockGroupId]) REFERENCES [dbo].[App_StockGroup]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_Stock_App_StockUnit')
BEGIN
    ALTER TABLE [dbo].[App_Stock] WITH CHECK
    ADD CONSTRAINT [FK_App_Stock_App_StockUnit]
    FOREIGN KEY([StockUnitId]) REFERENCES [dbo].[App_StockUnit]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_Stock_PurchaseVat')
BEGIN
    ALTER TABLE [dbo].[App_Stock] WITH CHECK
    ADD CONSTRAINT [FK_App_Stock_PurchaseVat]
    FOREIGN KEY([PurchaseVatId]) REFERENCES [dbo].[App_VatDefinition]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_VatDefinition', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_Stock_SalesVat')
BEGIN
    ALTER TABLE [dbo].[App_Stock] WITH CHECK
    ADD CONSTRAINT [FK_App_Stock_SalesVat]
    FOREIGN KEY([SalesVatId]) REFERENCES [dbo].[App_VatDefinition]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_Stock_StockCode' AND object_id = OBJECT_ID(N'dbo.App_Stock'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_Stock_StockCode]
    ON [dbo].[App_Stock]([StockCode]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 11
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (11, N'YamanCam.Core', N'App_Stock stoklar tablosu', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 12 - App_JournalVoucher / App_JournalVoucherLine             */
/* (Mahsup Fisi basligi + Borc/Alacak satirlari, master-detail)       */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_JournalVoucher](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [VoucherNo]    [nvarchar](50) NOT NULL,
        [VoucherDate]  [datetime2](0) NOT NULL,
        [CompanyId]    [int] NULL,
        [WorkPlaceId]  [int] NULL,
        [Description]  [nvarchar](500) NULL,
        [TotalDebit]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_JournalVoucher_TotalDebit] DEFAULT (0),
        [TotalCredit]  [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_JournalVoucher_TotalCredit] DEFAULT (0),
        [CurrencyCode] [nvarchar](3) NOT NULL CONSTRAINT [DF_App_JournalVoucher_CurrencyCode] DEFAULT (N'TRY'),
        [IsActive]     [bit] NULL CONSTRAINT [DF_App_JournalVoucher_IsActive] DEFAULT (1),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_JournalVoucher_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_JournalVoucher] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'VoucherNo') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [VoucherNo] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_JournalVoucher_VoucherNo] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'VoucherDate') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [VoucherDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_JournalVoucher_VoucherDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'CompanyId') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [CompanyId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'Description') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [Description] [nvarchar](500) NULL;
GO
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'TotalDebit') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [TotalDebit] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_JournalVoucher_TotalDebit_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'TotalCredit') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [TotalCredit] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_JournalVoucher_TotalCredit_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'CurrencyCode') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [CurrencyCode] [nvarchar](3) NOT NULL CONSTRAINT [DF_App_JournalVoucher_CurrencyCode_Alt] DEFAULT (N'TRY');
GO
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_JournalVoucher_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_JournalVoucher_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_JournalVoucher_App_Company')
BEGIN
    ALTER TABLE [dbo].[App_JournalVoucher] WITH CHECK
    ADD CONSTRAINT [FK_App_JournalVoucher_App_Company]
    FOREIGN KEY([CompanyId]) REFERENCES [dbo].[App_Company]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_JournalVoucher_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_JournalVoucher] WITH CHECK
    ADD CONSTRAINT [FK_App_JournalVoucher_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_JournalVoucher_VoucherNo' AND object_id = OBJECT_ID(N'dbo.App_JournalVoucher'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_JournalVoucher_VoucherNo]
    ON [dbo].[App_JournalVoucher]([VoucherNo]);
END
GO

/* ------------------------------------------------------------------ */
/* App_JournalVoucherLine (Mahsup Fisi satirlari - Borc/Alacak)       */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_JournalVoucherLine](
        [RecId]         [int] IDENTITY(1,1) NOT NULL,
        [VoucherId]     [int] NOT NULL,
        [LineNo]        [int] NOT NULL CONSTRAINT [DF_App_JournalVoucherLine_LineNo] DEFAULT (1),
        [AccountId]     [int] NOT NULL,
        [Description]   [nvarchar](500) NULL,
        [DebitAmount]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_JournalVoucherLine_DebitAmount] DEFAULT (0),
        [CreditAmount]  [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_JournalVoucherLine_CreditAmount] DEFAULT (0),
        [CreatedDate]   [datetime2](0) NULL CONSTRAINT [DF_App_JournalVoucherLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_JournalVoucherLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucherLine', 'VoucherId') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucherLine] ADD [VoucherId] [int] NOT NULL CONSTRAINT [DF_App_JournalVoucherLine_VoucherId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucherLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucherLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_JournalVoucherLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucherLine', 'AccountId') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucherLine] ADD [AccountId] [int] NOT NULL CONSTRAINT [DF_App_JournalVoucherLine_AccountId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucherLine', 'Description') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucherLine] ADD [Description] [nvarchar](500) NULL;
GO
IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucherLine', 'DebitAmount') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucherLine] ADD [DebitAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_JournalVoucherLine_DebitAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucherLine', 'CreditAmount') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucherLine] ADD [CreditAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_JournalVoucherLine_CreditAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucherLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucherLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_JournalVoucherLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_JournalVoucherLine_App_JournalVoucher')
BEGIN
    ALTER TABLE [dbo].[App_JournalVoucherLine] WITH CHECK
    ADD CONSTRAINT [FK_App_JournalVoucherLine_App_JournalVoucher]
    FOREIGN KEY([VoucherId]) REFERENCES [dbo].[App_JournalVoucher]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_JournalVoucherLine_App_AccountPlan')
BEGIN
    ALTER TABLE [dbo].[App_JournalVoucherLine] WITH CHECK
    ADD CONSTRAINT [FK_App_JournalVoucherLine_App_AccountPlan]
    FOREIGN KEY([AccountId]) REFERENCES [dbo].[App_AccountPlan]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_JournalVoucherLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_JournalVoucherLine_VoucherId' AND object_id = OBJECT_ID(N'dbo.App_JournalVoucherLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_JournalVoucherLine_VoucherId]
    ON [dbo].[App_JournalVoucherLine]([VoucherId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 12
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (12, N'YamanCam.Core', N'App_JournalVoucher / App_JournalVoucherLine mahsup fisi master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 13 - App_JournalVoucher.VoucherType                          */
/* (Mahsup / Tediye / Tahsilat fisleri ayni tabloyu paylasir)         */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_JournalVoucher', 'VoucherType') IS NULL
    ALTER TABLE [dbo].[App_JournalVoucher] ADD [VoucherType] [nvarchar](20) NOT NULL CONSTRAINT [DF_App_JournalVoucher_VoucherType] DEFAULT (N'Mahsup');
GO

IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_JournalVoucher_VoucherNo' AND object_id = OBJECT_ID(N'dbo.App_JournalVoucher'))
BEGIN
    DROP INDEX [UX_App_JournalVoucher_VoucherNo] ON [dbo].[App_JournalVoucher];
END
GO

IF OBJECT_ID(N'dbo.App_JournalVoucher', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_JournalVoucher_Type_VoucherNo' AND object_id = OBJECT_ID(N'dbo.App_JournalVoucher'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_JournalVoucher_Type_VoucherNo]
    ON [dbo].[App_JournalVoucher]([VoucherType], [VoucherNo]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 13
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (13, N'YamanCam.Core', N'App_JournalVoucher.VoucherType (Mahsup/Tediye/Tahsilat ayrimi)', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 14 - App_PurchaseInvoice / App_PurchaseInvoiceLine           */
/* (Alis Faturasi basligi + malzeme satirlari, doviz/TL cift alanli)  */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_PurchaseInvoice](
        [RecId]          [int] IDENTITY(1,1) NOT NULL,
        [InvoiceNo]      [nvarchar](50) NOT NULL,
        [InvoiceDate]    [datetime2](0) NOT NULL,
        [CompanyId]      [int] NULL,
        [WorkPlaceId]    [int] NULL,
        [AccountId]      [int] NOT NULL,
        [Description]    [nvarchar](500) NULL,
        [CurrencyCode]   [nvarchar](3) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_CurrencyCode] DEFAULT (N'TRY'),
        [ExchangeRate]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_ExchangeRate] DEFAULT (1),
        [NetAmount]      [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_NetAmount] DEFAULT (0),
        [VatAmount]      [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_VatAmount] DEFAULT (0),
        [TotalAmount]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_TotalAmount] DEFAULT (0),
        [NetAmountTRY]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_NetAmountTRY] DEFAULT (0),
        [VatAmountTRY]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_VatAmountTRY] DEFAULT (0),
        [TotalAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_TotalAmountTRY] DEFAULT (0),
        [IsReturn]       [bit] NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_IsReturn] DEFAULT (0),
        [IsActive]       [bit] NULL CONSTRAINT [DF_App_PurchaseInvoice_IsActive] DEFAULT (1),
        [CreatedDate]    [datetime2](0) NULL CONSTRAINT [DF_App_PurchaseInvoice_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_PurchaseInvoice] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'InvoiceNo') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [InvoiceNo] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_InvoiceNo] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'InvoiceDate') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [InvoiceDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_InvoiceDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'CompanyId') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [CompanyId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'AccountId') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [AccountId] [int] NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_AccountId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'Description') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [Description] [nvarchar](500) NULL;
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'CurrencyCode') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [CurrencyCode] [nvarchar](3) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_CurrencyCode_Alt] DEFAULT (N'TRY');
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'ExchangeRate') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [ExchangeRate] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_ExchangeRate_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'NetAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [NetAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_NetAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'VatAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [VatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_VatAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'NetAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [NetAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_NetAmountTRY_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'VatAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [VatAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_VatAmountTRY_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'TotalAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [TotalAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_TotalAmountTRY_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'IsReturn') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [IsReturn] [bit] NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_IsReturn_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_PurchaseInvoice_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_PurchaseInvoice_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_PurchaseInvoice_App_Company')
BEGIN
    ALTER TABLE [dbo].[App_PurchaseInvoice] WITH CHECK
    ADD CONSTRAINT [FK_App_PurchaseInvoice_App_Company]
    FOREIGN KEY([CompanyId]) REFERENCES [dbo].[App_Company]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_PurchaseInvoice_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_PurchaseInvoice] WITH CHECK
    ADD CONSTRAINT [FK_App_PurchaseInvoice_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_PurchaseInvoice_App_AccountPlan')
BEGIN
    ALTER TABLE [dbo].[App_PurchaseInvoice] WITH CHECK
    ADD CONSTRAINT [FK_App_PurchaseInvoice_App_AccountPlan]
    FOREIGN KEY([AccountId]) REFERENCES [dbo].[App_AccountPlan]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.App_PurchaseInvoice', 'IsReturn') IS NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_PurchaseInvoice_InvoiceNo' AND object_id = OBJECT_ID(N'dbo.App_PurchaseInvoice'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_PurchaseInvoice_InvoiceNo]
    ON [dbo].[App_PurchaseInvoice]([InvoiceNo]);
END
GO

/* ------------------------------------------------------------------ */
/* App_PurchaseInvoiceLine (Alis Faturasi malzeme satirlari)          */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_PurchaseInvoiceLine](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [InvoiceId]    [int] NOT NULL,
        [LineNo]       [int] NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_LineNo] DEFAULT (1),
        [StockId]      [int] NOT NULL,
        [Description]  [nvarchar](500) NULL,
        [Quantity]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_Quantity] DEFAULT (0),
        [UnitPrice]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_UnitPrice] DEFAULT (0),
        [VatRate]      [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_VatRate] DEFAULT (0),
        [NetAmount]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_NetAmount] DEFAULT (0),
        [VatAmount]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_VatAmount] DEFAULT (0),
        [TotalAmount]  [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_TotalAmount] DEFAULT (0),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_PurchaseInvoiceLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'InvoiceId') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [InvoiceId] [int] NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_InvoiceId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'StockId') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [StockId] [int] NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_StockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'Description') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [Description] [nvarchar](500) NULL;
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'Quantity') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [Quantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_Quantity_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'UnitPrice') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [UnitPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_UnitPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'VatRate') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [VatRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_VatRate_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'NetAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [NetAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_NetAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'VatAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [VatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_VatAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_PurchaseInvoiceLine_App_PurchaseInvoice')
BEGIN
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] WITH CHECK
    ADD CONSTRAINT [FK_App_PurchaseInvoiceLine_App_PurchaseInvoice]
    FOREIGN KEY([InvoiceId]) REFERENCES [dbo].[App_PurchaseInvoice]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_PurchaseInvoiceLine_App_Stock')
BEGIN
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] WITH CHECK
    ADD CONSTRAINT [FK_App_PurchaseInvoiceLine_App_Stock]
    FOREIGN KEY([StockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_PurchaseInvoiceLine_InvoiceId' AND object_id = OBJECT_ID(N'dbo.App_PurchaseInvoiceLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_PurchaseInvoiceLine_InvoiceId]
    ON [dbo].[App_PurchaseInvoiceLine]([InvoiceId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 14
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (14, N'YamanCam.Core', N'App_PurchaseInvoice / App_PurchaseInvoiceLine alis faturasi master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 15 - App_SalesInvoice / App_SalesInvoiceLine                 */
/* (Satis Faturasi basligi + malzeme satirlari, doviz/TL cift alanli) */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_SalesInvoice](
        [RecId]          [int] IDENTITY(1,1) NOT NULL,
        [InvoiceNo]      [nvarchar](50) NOT NULL,
        [InvoiceDate]    [datetime2](0) NOT NULL,
        [CompanyId]      [int] NULL,
        [WorkPlaceId]    [int] NULL,
        [AccountId]      [int] NOT NULL,
        [Description]    [nvarchar](500) NULL,
        [CurrencyCode]   [nvarchar](3) NOT NULL CONSTRAINT [DF_App_SalesInvoice_CurrencyCode] DEFAULT (N'TRY'),
        [ExchangeRate]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_ExchangeRate] DEFAULT (1),
        [NetAmount]      [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_NetAmount] DEFAULT (0),
        [VatAmount]      [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_VatAmount] DEFAULT (0),
        [TotalAmount]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_TotalAmount] DEFAULT (0),
        [NetAmountTRY]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_NetAmountTRY] DEFAULT (0),
        [VatAmountTRY]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_VatAmountTRY] DEFAULT (0),
        [TotalAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_TotalAmountTRY] DEFAULT (0),
        [IsReturn]       [bit] NOT NULL CONSTRAINT [DF_App_SalesInvoice_IsReturn] DEFAULT (0),
        [IsActive]       [bit] NULL CONSTRAINT [DF_App_SalesInvoice_IsActive] DEFAULT (1),
        [CreatedDate]    [datetime2](0) NULL CONSTRAINT [DF_App_SalesInvoice_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_SalesInvoice] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'InvoiceNo') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [InvoiceNo] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_SalesInvoice_InvoiceNo] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'InvoiceDate') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [InvoiceDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_SalesInvoice_InvoiceDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'CompanyId') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [CompanyId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'AccountId') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [AccountId] [int] NOT NULL CONSTRAINT [DF_App_SalesInvoice_AccountId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'Description') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [Description] [nvarchar](500) NULL;
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'CurrencyCode') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [CurrencyCode] [nvarchar](3) NOT NULL CONSTRAINT [DF_App_SalesInvoice_CurrencyCode_Alt] DEFAULT (N'TRY');
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'ExchangeRate') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [ExchangeRate] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_ExchangeRate_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'NetAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [NetAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_NetAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'VatAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [VatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_VatAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'NetAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [NetAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_NetAmountTRY_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'VatAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [VatAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_VatAmountTRY_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'TotalAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [TotalAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_TotalAmountTRY_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'IsReturn') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [IsReturn] [bit] NOT NULL CONSTRAINT [DF_App_SalesInvoice_IsReturn_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_SalesInvoice_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_SalesInvoice_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Company', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_SalesInvoice_App_Company')
BEGIN
    ALTER TABLE [dbo].[App_SalesInvoice] WITH CHECK
    ADD CONSTRAINT [FK_App_SalesInvoice_App_Company]
    FOREIGN KEY([CompanyId]) REFERENCES [dbo].[App_Company]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_SalesInvoice_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_SalesInvoice] WITH CHECK
    ADD CONSTRAINT [FK_App_SalesInvoice_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_SalesInvoice_App_AccountPlan')
BEGIN
    ALTER TABLE [dbo].[App_SalesInvoice] WITH CHECK
    ADD CONSTRAINT [FK_App_SalesInvoice_App_AccountPlan]
    FOREIGN KEY([AccountId]) REFERENCES [dbo].[App_AccountPlan]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL
   AND COL_LENGTH('dbo.App_SalesInvoice', 'IsReturn') IS NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_SalesInvoice_InvoiceNo' AND object_id = OBJECT_ID(N'dbo.App_SalesInvoice'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_SalesInvoice_InvoiceNo]
    ON [dbo].[App_SalesInvoice]([InvoiceNo]);
END
GO

/* ------------------------------------------------------------------ */
/* App_SalesInvoiceLine (Satis Faturasi malzeme satirlari)            */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_SalesInvoiceLine](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [InvoiceId]    [int] NOT NULL,
        [LineNo]       [int] NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_LineNo] DEFAULT (1),
        [StockId]      [int] NOT NULL,
        [Description]  [nvarchar](500) NULL,
        [Quantity]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_Quantity] DEFAULT (0),
        [UnitPrice]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_UnitPrice] DEFAULT (0),
        [VatRate]      [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_VatRate] DEFAULT (0),
        [NetAmount]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_NetAmount] DEFAULT (0),
        [VatAmount]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_VatAmount] DEFAULT (0),
        [TotalAmount]  [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_TotalAmount] DEFAULT (0),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_SalesInvoiceLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_SalesInvoiceLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'InvoiceId') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [InvoiceId] [int] NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_InvoiceId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'StockId') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [StockId] [int] NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_StockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'Description') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [Description] [nvarchar](500) NULL;
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'Quantity') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [Quantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_Quantity_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'UnitPrice') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [UnitPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_UnitPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'VatRate') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [VatRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_VatRate_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'NetAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [NetAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_NetAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'VatAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [VatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_VatAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_SalesInvoiceLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_SalesInvoiceLine_App_SalesInvoice')
BEGIN
    ALTER TABLE [dbo].[App_SalesInvoiceLine] WITH CHECK
    ADD CONSTRAINT [FK_App_SalesInvoiceLine_App_SalesInvoice]
    FOREIGN KEY([InvoiceId]) REFERENCES [dbo].[App_SalesInvoice]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_SalesInvoiceLine_App_Stock')
BEGIN
    ALTER TABLE [dbo].[App_SalesInvoiceLine] WITH CHECK
    ADD CONSTRAINT [FK_App_SalesInvoiceLine_App_Stock]
    FOREIGN KEY([StockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_SalesInvoiceLine_InvoiceId' AND object_id = OBJECT_ID(N'dbo.App_SalesInvoiceLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_SalesInvoiceLine_InvoiceId]
    ON [dbo].[App_SalesInvoiceLine]([InvoiceId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 15
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (15, N'YamanCam.Core', N'App_SalesInvoice / App_SalesInvoiceLine satis faturasi master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 16 - App_CustomsFreightInvoice / App_CustomsFreightInvoiceLine */
/* (Gumruk Nakliye Faturasi basligi + gider hesap satirlari; Alis      */
/*  Faturasi ile "Stok Fat No" alaniyla isteğe bagli iliskilendirilir) */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_CustomsFreightInvoice](
        [RecId]                    [int] IDENTITY(1,1) NOT NULL,
        [InvoiceDate]              [datetime2](0) NOT NULL,
        [WorkPlaceId]              [int] NULL,
        [InvoiceKind]              [nvarchar](30) NULL,
        [AccountId]                [int] NOT NULL,
        [InvoiceNo]                [nvarchar](50) NOT NULL,
        [SpecialCode]              [nvarchar](50) NULL,
        [LinkedPurchaseInvoiceId]  [int] NULL,
        [NetAmount]                [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_NetAmount] DEFAULT (0),
        [VatAmount]                [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_VatAmount] DEFAULT (0),
        [WithholdingAmount]        [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_WithholdingAmount] DEFAULT (0),
        [NetVatAmount]             [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_NetVatAmount] DEFAULT (0),
        [TotalAmount]              [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_TotalAmount] DEFAULT (0),
        [CurrencyAmount]           [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_CurrencyAmount] DEFAULT (0),
        [ExchangeRate]             [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_ExchangeRate] DEFAULT (1),
        [CurrencyCode]             [nvarchar](3) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_CurrencyCode] DEFAULT (N'TRY'),
        [IsActive]                 [bit] NULL CONSTRAINT [DF_App_CustomsFreightInvoice_IsActive] DEFAULT (1),
        [CreatedDate]              [datetime2](0) NULL CONSTRAINT [DF_App_CustomsFreightInvoice_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_CustomsFreightInvoice] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'InvoiceDate') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [InvoiceDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_InvoiceDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'InvoiceKind') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [InvoiceKind] [nvarchar](30) NULL;
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'AccountId') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [AccountId] [int] NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_AccountId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'InvoiceNo') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [InvoiceNo] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_InvoiceNo] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'SpecialCode') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [SpecialCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'LinkedPurchaseInvoiceId') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [LinkedPurchaseInvoiceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'NetAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [NetAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_NetAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'VatAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [VatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_VatAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'WithholdingAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [WithholdingAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_WithholdingAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'NetVatAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [NetVatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_NetVatAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'CurrencyAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [CurrencyAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_CurrencyAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'ExchangeRate') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [ExchangeRate] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_ExchangeRate_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'CurrencyCode') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [CurrencyCode] [nvarchar](3) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_CurrencyCode_Alt] DEFAULT (N'TRY');
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_CustomsFreightInvoice_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_CustomsFreightInvoice_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_CustomsFreightInvoice_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] WITH CHECK
    ADD CONSTRAINT [FK_App_CustomsFreightInvoice_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_CustomsFreightInvoice_App_AccountPlan')
BEGIN
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] WITH CHECK
    ADD CONSTRAINT [FK_App_CustomsFreightInvoice_App_AccountPlan]
    FOREIGN KEY([AccountId]) REFERENCES [dbo].[App_AccountPlan]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_CustomsFreightInvoice_App_PurchaseInvoice')
BEGIN
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] WITH CHECK
    ADD CONSTRAINT [FK_App_CustomsFreightInvoice_App_PurchaseInvoice]
    FOREIGN KEY([LinkedPurchaseInvoiceId]) REFERENCES [dbo].[App_PurchaseInvoice]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_CustomsFreightInvoice_InvoiceNo' AND object_id = OBJECT_ID(N'dbo.App_CustomsFreightInvoice'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_CustomsFreightInvoice_InvoiceNo]
    ON [dbo].[App_CustomsFreightInvoice]([InvoiceNo]);
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_CustomsFreightInvoice_LinkedPurchaseInvoiceId' AND object_id = OBJECT_ID(N'dbo.App_CustomsFreightInvoice'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_CustomsFreightInvoice_LinkedPurchaseInvoiceId]
    ON [dbo].[App_CustomsFreightInvoice]([LinkedPurchaseInvoiceId]);
END
GO

/* ------------------------------------------------------------------ */
/* App_CustomsFreightInvoiceLine (Gumruk Nakliye Faturasi gider        */
/* hesap satirlari - Matrah/KDV/Tevkifat)                              */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_CustomsFreightInvoiceLine](
        [RecId]              [int] IDENTITY(1,1) NOT NULL,
        [InvoiceId]          [int] NOT NULL,
        [LineNo]             [int] NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_LineNo] DEFAULT (1),
        [AccountId]          [int] NOT NULL,
        [Amount]             [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_Amount] DEFAULT (0),
        [VatRate]            [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_VatRate] DEFAULT (0),
        [WithholdingRate]    [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_WithholdingRate] DEFAULT (0),
        [VatAmount]          [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_VatAmount] DEFAULT (0),
        [WithholdingAmount]  [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_WithholdingAmount] DEFAULT (0),
        [NetVatAmount]       [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_NetVatAmount] DEFAULT (0),
        [TotalAmount]        [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_TotalAmount] DEFAULT (0),
        [CreatedDate]        [datetime2](0) NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_CustomsFreightInvoiceLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'InvoiceId') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [InvoiceId] [int] NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_InvoiceId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'AccountId') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [AccountId] [int] NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_AccountId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'Amount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [Amount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_Amount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'VatRate') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [VatRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_VatRate_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'WithholdingRate') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [WithholdingRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_WithholdingRate_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'VatAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [VatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_VatAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'WithholdingAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [WithholdingAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_WithholdingAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'NetVatAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [NetVatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_NetVatAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_CustomsFreightInvoiceLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_CustomsFreightInvoiceLine_App_CustomsFreightInvoice')
BEGIN
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] WITH CHECK
    ADD CONSTRAINT [FK_App_CustomsFreightInvoiceLine_App_CustomsFreightInvoice]
    FOREIGN KEY([InvoiceId]) REFERENCES [dbo].[App_CustomsFreightInvoice]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_AccountPlan', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_CustomsFreightInvoiceLine_App_AccountPlan')
BEGIN
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] WITH CHECK
    ADD CONSTRAINT [FK_App_CustomsFreightInvoiceLine_App_AccountPlan]
    FOREIGN KEY([AccountId]) REFERENCES [dbo].[App_AccountPlan]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_CustomsFreightInvoiceLine_InvoiceId' AND object_id = OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_CustomsFreightInvoiceLine_InvoiceId]
    ON [dbo].[App_CustomsFreightInvoiceLine]([InvoiceId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 16
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (16, N'YamanCam.Core', N'App_CustomsFreightInvoice / App_CustomsFreightInvoiceLine gumruk nakliye faturasi master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 17 - App_VatWithholdingDefinition (Tevkifat Kdv Tanimlari)   */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_VatWithholdingDefinition](
        [RecId]              [int] IDENTITY(1,1) NOT NULL,
        [WithholdingCode]    [nvarchar](20) NOT NULL,
        [WithholdingName]    [nvarchar](100) NOT NULL,
        [VatRate]            [decimal](13, 10) NOT NULL,
        [WithholdingRate]    [decimal](13, 10) NOT NULL,
        [AccountCode]        [nvarchar](50) NULL,
        [IsActive]           [bit] NULL CONSTRAINT [DF_App_VatWithholdingDefinition_IsActive] DEFAULT (1),
        [CreatedDate]        [datetime2](0) NULL CONSTRAINT [DF_App_VatWithholdingDefinition_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_VatWithholdingDefinition] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatWithholdingDefinition', 'WithholdingCode') IS NULL
    ALTER TABLE [dbo].[App_VatWithholdingDefinition] ADD [WithholdingCode] [nvarchar](20) NOT NULL CONSTRAINT [DF_App_VatWithholdingDefinition_WithholdingCode] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatWithholdingDefinition', 'WithholdingName') IS NULL
    ALTER TABLE [dbo].[App_VatWithholdingDefinition] ADD [WithholdingName] [nvarchar](100) NOT NULL CONSTRAINT [DF_App_VatWithholdingDefinition_WithholdingName] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatWithholdingDefinition', 'VatRate') IS NULL
    ALTER TABLE [dbo].[App_VatWithholdingDefinition] ADD [VatRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_VatWithholdingDefinition_VatRate] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatWithholdingDefinition', 'WithholdingRate') IS NULL
    ALTER TABLE [dbo].[App_VatWithholdingDefinition] ADD [WithholdingRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_VatWithholdingDefinition_WithholdingRate] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatWithholdingDefinition', 'AccountCode') IS NULL
    ALTER TABLE [dbo].[App_VatWithholdingDefinition] ADD [AccountCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatWithholdingDefinition', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_VatWithholdingDefinition] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_VatWithholdingDefinition_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_VatWithholdingDefinition', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_VatWithholdingDefinition] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_VatWithholdingDefinition_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_VatWithholdingDefinition_WithholdingCode' AND object_id = OBJECT_ID(N'dbo.App_VatWithholdingDefinition'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_VatWithholdingDefinition_WithholdingCode]
    ON [dbo].[App_VatWithholdingDefinition]([WithholdingCode]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 17
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (17, N'YamanCam.Core', N'App_VatWithholdingDefinition tevkifat kdv tanimlari tablosu', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 18 - Alis/Satis/Gumruk Nakliye faturalarina tevkifat kdv     */
/* secimi ve hesaplama alanlari eklendi.                              */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'WithholdingDefinitionId') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [WithholdingDefinitionId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'WithholdingRate') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [WithholdingRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_WithholdingRate] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'WithholdingAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [WithholdingAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_WithholdingAmount] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'NetVatAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] ADD [NetVatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoiceLine_NetVatAmount] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoiceLine', 'NetVatAmount') IS NOT NULL
    UPDATE [dbo].[App_PurchaseInvoiceLine] SET [NetVatAmount] = [VatAmount] WHERE [WithholdingRate] = 0;
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'WithholdingAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [WithholdingAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_WithholdingAmount] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'NetVatAmount') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [NetVatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_NetVatAmount] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'WithholdingAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [WithholdingAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_WithholdingAmountTRY] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'NetVatAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [NetVatAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_NetVatAmountTRY] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'NetVatAmountTRY') IS NOT NULL
    UPDATE [dbo].[App_PurchaseInvoice] SET [NetVatAmount] = [VatAmount], [NetVatAmountTRY] = [VatAmountTRY] WHERE [WithholdingAmount] = 0;
GO

IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'WithholdingDefinitionId') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [WithholdingDefinitionId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'WithholdingRate') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [WithholdingRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_WithholdingRate] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'WithholdingAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [WithholdingAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_WithholdingAmount] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'NetVatAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoiceLine] ADD [NetVatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoiceLine_NetVatAmount] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoiceLine', 'NetVatAmount') IS NOT NULL
    UPDATE [dbo].[App_SalesInvoiceLine] SET [NetVatAmount] = [VatAmount] WHERE [WithholdingRate] = 0;
GO

IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'WithholdingAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [WithholdingAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_WithholdingAmount] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'NetVatAmount') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [NetVatAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_NetVatAmount] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'WithholdingAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [WithholdingAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_WithholdingAmountTRY] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'NetVatAmountTRY') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [NetVatAmountTRY] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_SalesInvoice_NetVatAmountTRY] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'NetVatAmountTRY') IS NOT NULL
    UPDATE [dbo].[App_SalesInvoice] SET [NetVatAmount] = [VatAmount], [NetVatAmountTRY] = [VatAmountTRY] WHERE [WithholdingAmount] = 0;
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceLine', 'WithholdingDefinitionId') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] ADD [WithholdingDefinitionId] [int] NULL;
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoiceLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_PurchaseInvoiceLine_App_VatWithholdingDefinition')
BEGIN
    ALTER TABLE [dbo].[App_PurchaseInvoiceLine] WITH CHECK
    ADD CONSTRAINT [FK_App_PurchaseInvoiceLine_App_VatWithholdingDefinition]
    FOREIGN KEY([WithholdingDefinitionId]) REFERENCES [dbo].[App_VatWithholdingDefinition]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_SalesInvoiceLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_SalesInvoiceLine_App_VatWithholdingDefinition')
BEGIN
    ALTER TABLE [dbo].[App_SalesInvoiceLine] WITH CHECK
    ADD CONSTRAINT [FK_App_SalesInvoiceLine_App_VatWithholdingDefinition]
    FOREIGN KEY([WithholdingDefinitionId]) REFERENCES [dbo].[App_VatWithholdingDefinition]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_VatWithholdingDefinition', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_CustomsFreightInvoiceLine_App_VatWithholdingDefinition')
BEGIN
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceLine] WITH CHECK
    ADD CONSTRAINT [FK_App_CustomsFreightInvoiceLine_App_VatWithholdingDefinition]
    FOREIGN KEY([WithholdingDefinitionId]) REFERENCES [dbo].[App_VatWithholdingDefinition]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 18
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (18, N'YamanCam.Core', N'Alis/Satis/Gumruk Nakliye faturalarina tevkifat kdv secimi ve hesaplama alanlari eklendi', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 19 - App_StockOpening / App_StockOpeningLine                 */
/* (Stok Acilis Fisi basligi + stok satirlari, doviz/KDV yok)          */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockOpening](
        [RecId]           [int] IDENTITY(1,1) NOT NULL,
        [VoucherNo]       [nvarchar](50) NOT NULL,
        [VoucherDate]     [datetime2](0) NOT NULL,
        [WorkPlaceId]     [int] NULL,
        [TransactionType] [nvarchar](30) NOT NULL CONSTRAINT [DF_App_StockOpening_TransactionType] DEFAULT (N'İlk Giriş'),
        [SpecialCode]     [nvarchar](50) NULL,
        [TotalAmount]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockOpening_TotalAmount] DEFAULT (0),
        [IsActive]        [bit] NULL CONSTRAINT [DF_App_StockOpening_IsActive] DEFAULT (1),
        [CreatedDate]     [datetime2](0) NULL CONSTRAINT [DF_App_StockOpening_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockOpening] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpening', 'VoucherNo') IS NULL
    ALTER TABLE [dbo].[App_StockOpening] ADD [VoucherNo] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_StockOpening_VoucherNo] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpening', 'VoucherDate') IS NULL
    ALTER TABLE [dbo].[App_StockOpening] ADD [VoucherDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_StockOpening_VoucherDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpening', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_StockOpening] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpening', 'TransactionType') IS NULL
    ALTER TABLE [dbo].[App_StockOpening] ADD [TransactionType] [nvarchar](30) NOT NULL CONSTRAINT [DF_App_StockOpening_TransactionType_Alt] DEFAULT (N'İlk Giriş');
GO
IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpening', 'SpecialCode') IS NULL
    ALTER TABLE [dbo].[App_StockOpening] ADD [SpecialCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpening', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_StockOpening] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockOpening_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpening', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_StockOpening] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_StockOpening_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpening', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockOpening] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockOpening_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockOpening_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_StockOpening] WITH CHECK
    ADD CONSTRAINT [FK_App_StockOpening_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_StockOpening_VoucherNo' AND object_id = OBJECT_ID(N'dbo.App_StockOpening'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_StockOpening_VoucherNo]
    ON [dbo].[App_StockOpening]([VoucherNo]);
END
GO

/* ------------------------------------------------------------------ */
/* App_StockOpeningLine (Stok Acilis Fisi stok satirlari)              */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockOpeningLine](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [OpeningId]    [int] NOT NULL,
        [LineNo]       [int] NOT NULL CONSTRAINT [DF_App_StockOpeningLine_LineNo] DEFAULT (1),
        [StockId]      [int] NOT NULL,
        [StockUnitId]  [int] NULL,
        [Quantity]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockOpeningLine_Quantity] DEFAULT (0),
        [UnitPrice]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockOpeningLine_UnitPrice] DEFAULT (0),
        [TotalPrice]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockOpeningLine_TotalPrice] DEFAULT (0),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_StockOpeningLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockOpeningLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpeningLine', 'OpeningId') IS NULL
    ALTER TABLE [dbo].[App_StockOpeningLine] ADD [OpeningId] [int] NOT NULL CONSTRAINT [DF_App_StockOpeningLine_OpeningId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpeningLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_StockOpeningLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_StockOpeningLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpeningLine', 'StockId') IS NULL
    ALTER TABLE [dbo].[App_StockOpeningLine] ADD [StockId] [int] NOT NULL CONSTRAINT [DF_App_StockOpeningLine_StockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpeningLine', 'StockUnitId') IS NULL
    ALTER TABLE [dbo].[App_StockOpeningLine] ADD [StockUnitId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpeningLine', 'Quantity') IS NULL
    ALTER TABLE [dbo].[App_StockOpeningLine] ADD [Quantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockOpeningLine_Quantity_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpeningLine', 'UnitPrice') IS NULL
    ALTER TABLE [dbo].[App_StockOpeningLine] ADD [UnitPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockOpeningLine_UnitPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpeningLine', 'TotalPrice') IS NULL
    ALTER TABLE [dbo].[App_StockOpeningLine] ADD [TotalPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockOpeningLine_TotalPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockOpeningLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockOpeningLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockOpeningLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockOpening', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockOpeningLine_App_StockOpening')
BEGIN
    ALTER TABLE [dbo].[App_StockOpeningLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockOpeningLine_App_StockOpening]
    FOREIGN KEY([OpeningId]) REFERENCES [dbo].[App_StockOpening]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockOpeningLine_App_Stock')
BEGIN
    ALTER TABLE [dbo].[App_StockOpeningLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockOpeningLine_App_Stock]
    FOREIGN KEY([StockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockOpeningLine_App_StockUnit')
BEGIN
    ALTER TABLE [dbo].[App_StockOpeningLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockOpeningLine_App_StockUnit]
    FOREIGN KEY([StockUnitId]) REFERENCES [dbo].[App_StockUnit]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockOpeningLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_StockOpeningLine_OpeningId' AND object_id = OBJECT_ID(N'dbo.App_StockOpeningLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_StockOpeningLine_OpeningId]
    ON [dbo].[App_StockOpeningLine]([OpeningId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 19
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (19, N'YamanCam.Core', N'App_StockOpening / App_StockOpeningLine stok acilis fisi master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 20 - App_ProductionDefinition (Uretim Tanimlari)              */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_ProductionDefinition](
        [RecId]              [int] IDENTITY(1,1) NOT NULL,
        [ProductStockId]     [int] NOT NULL,
        [RawMaterialStockId] [int] NOT NULL,
        [ProductionQuantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionDefinition_ProductionQuantity] DEFAULT (1),
        [WorkPlaceId]        [int] NOT NULL,
        [IsActive]           [bit] NULL CONSTRAINT [DF_App_ProductionDefinition_IsActive] DEFAULT (1),
        [CreatedDate]        [datetime2](0) NULL CONSTRAINT [DF_App_ProductionDefinition_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_ProductionDefinition] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionDefinition', 'ProductStockId') IS NULL
    ALTER TABLE [dbo].[App_ProductionDefinition] ADD [ProductStockId] [int] NOT NULL CONSTRAINT [DF_App_ProductionDefinition_ProductStockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionDefinition', 'RawMaterialStockId') IS NULL
    ALTER TABLE [dbo].[App_ProductionDefinition] ADD [RawMaterialStockId] [int] NOT NULL CONSTRAINT [DF_App_ProductionDefinition_RawMaterialStockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionDefinition', 'ProductionQuantity') IS NULL
    ALTER TABLE [dbo].[App_ProductionDefinition] ADD [ProductionQuantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionDefinition_ProductionQuantity_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionDefinition', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_ProductionDefinition] ADD [WorkPlaceId] [int] NOT NULL CONSTRAINT [DF_App_ProductionDefinition_WorkPlaceId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionDefinition', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_ProductionDefinition] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_ProductionDefinition_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionDefinition', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_ProductionDefinition] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_ProductionDefinition_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductionDefinition_ProductStock')
BEGIN
    ALTER TABLE [dbo].[App_ProductionDefinition] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductionDefinition_ProductStock]
    FOREIGN KEY([ProductStockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductionDefinition_RawMaterialStock')
BEGIN
    ALTER TABLE [dbo].[App_ProductionDefinition] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductionDefinition_RawMaterialStock]
    FOREIGN KEY([RawMaterialStockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductionDefinition_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_ProductionDefinition] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductionDefinition_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductionDefinition', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_ProductionDefinition_ProductStockId' AND object_id = OBJECT_ID(N'dbo.App_ProductionDefinition'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_ProductionDefinition_ProductStockId]
    ON [dbo].[App_ProductionDefinition]([ProductStockId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 20
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (20, N'YamanCam.Core', N'App_ProductionDefinition uretim tanimlari tablosu', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 21 - App_StockTransfer / App_StockTransferLine                */
/* (Subeler Arasi Transfer Fisi basligi + stok satirlari, doviz/KDV yok)*/
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockTransfer](
        [RecId]           [int] IDENTITY(1,1) NOT NULL,
        [VoucherNo]       [nvarchar](50) NOT NULL,
        [VoucherDate]     [datetime2](0) NOT NULL,
        [InWorkPlaceId]   [int] NULL,
        [OutWorkPlaceId]  [int] NULL,
        [TransactionType] [nvarchar](30) NOT NULL CONSTRAINT [DF_App_StockTransfer_TransactionType] DEFAULT (N'Transfer'),
        [SpecialCode]     [nvarchar](50) NULL,
        [TotalAmount]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockTransfer_TotalAmount] DEFAULT (0),
        [IsActive]        [bit] NULL CONSTRAINT [DF_App_StockTransfer_IsActive] DEFAULT (1),
        [CreatedDate]     [datetime2](0) NULL CONSTRAINT [DF_App_StockTransfer_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockTransfer] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransfer', 'VoucherNo') IS NULL
    ALTER TABLE [dbo].[App_StockTransfer] ADD [VoucherNo] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_StockTransfer_VoucherNo] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransfer', 'VoucherDate') IS NULL
    ALTER TABLE [dbo].[App_StockTransfer] ADD [VoucherDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_StockTransfer_VoucherDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransfer', 'InWorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_StockTransfer] ADD [InWorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransfer', 'OutWorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_StockTransfer] ADD [OutWorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransfer', 'TransactionType') IS NULL
    ALTER TABLE [dbo].[App_StockTransfer] ADD [TransactionType] [nvarchar](30) NOT NULL CONSTRAINT [DF_App_StockTransfer_TransactionType_Alt] DEFAULT (N'Transfer');
GO
IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransfer', 'SpecialCode') IS NULL
    ALTER TABLE [dbo].[App_StockTransfer] ADD [SpecialCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransfer', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_StockTransfer] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockTransfer_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransfer', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_StockTransfer] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_StockTransfer_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransfer', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockTransfer] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockTransfer_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockTransfer_InWorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_StockTransfer] WITH CHECK
    ADD CONSTRAINT [FK_App_StockTransfer_InWorkPlace]
    FOREIGN KEY([InWorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockTransfer_OutWorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_StockTransfer] WITH CHECK
    ADD CONSTRAINT [FK_App_StockTransfer_OutWorkPlace]
    FOREIGN KEY([OutWorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_StockTransfer_VoucherNo' AND object_id = OBJECT_ID(N'dbo.App_StockTransfer'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_StockTransfer_VoucherNo]
    ON [dbo].[App_StockTransfer]([VoucherNo]);
END
GO

/* ------------------------------------------------------------------ */
/* App_StockTransferLine (Subeler Arasi Transfer Fisi stok satirlari)  */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockTransferLine](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [TransferId]   [int] NOT NULL,
        [LineNo]       [int] NOT NULL CONSTRAINT [DF_App_StockTransferLine_LineNo] DEFAULT (1),
        [StockId]      [int] NOT NULL,
        [StockUnitId]  [int] NULL,
        [Quantity]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockTransferLine_Quantity] DEFAULT (0),
        [UnitPrice]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockTransferLine_UnitPrice] DEFAULT (0),
        [TotalPrice]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockTransferLine_TotalPrice] DEFAULT (0),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_StockTransferLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockTransferLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransferLine', 'TransferId') IS NULL
    ALTER TABLE [dbo].[App_StockTransferLine] ADD [TransferId] [int] NOT NULL CONSTRAINT [DF_App_StockTransferLine_TransferId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransferLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_StockTransferLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_StockTransferLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransferLine', 'StockId') IS NULL
    ALTER TABLE [dbo].[App_StockTransferLine] ADD [StockId] [int] NOT NULL CONSTRAINT [DF_App_StockTransferLine_StockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransferLine', 'StockUnitId') IS NULL
    ALTER TABLE [dbo].[App_StockTransferLine] ADD [StockUnitId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransferLine', 'Quantity') IS NULL
    ALTER TABLE [dbo].[App_StockTransferLine] ADD [Quantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockTransferLine_Quantity_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransferLine', 'UnitPrice') IS NULL
    ALTER TABLE [dbo].[App_StockTransferLine] ADD [UnitPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockTransferLine_UnitPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransferLine', 'TotalPrice') IS NULL
    ALTER TABLE [dbo].[App_StockTransferLine] ADD [TotalPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockTransferLine_TotalPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockTransferLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockTransferLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockTransferLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockTransfer', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockTransferLine_App_StockTransfer')
BEGIN
    ALTER TABLE [dbo].[App_StockTransferLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockTransferLine_App_StockTransfer]
    FOREIGN KEY([TransferId]) REFERENCES [dbo].[App_StockTransfer]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockTransferLine_App_Stock')
BEGIN
    ALTER TABLE [dbo].[App_StockTransferLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockTransferLine_App_Stock]
    FOREIGN KEY([StockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockTransferLine_App_StockUnit')
BEGIN
    ALTER TABLE [dbo].[App_StockTransferLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockTransferLine_App_StockUnit]
    FOREIGN KEY([StockUnitId]) REFERENCES [dbo].[App_StockUnit]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockTransferLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_StockTransferLine_TransferId' AND object_id = OBJECT_ID(N'dbo.App_StockTransferLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_StockTransferLine_TransferId]
    ON [dbo].[App_StockTransferLine]([TransferId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 21
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (21, N'YamanCam.Core', N'App_StockTransfer / App_StockTransferLine subeler arasi transfer fisi master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 22 - App_StockMerge (Stoklari Birlestir islemi)              */
/* (Donem tarihi + sube + islemin fiilen yapildigi tarih)             */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockMerge', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockMerge](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [PeriodDate]   [datetime2](0) NOT NULL,
        [WorkPlaceId]  [int] NULL,
        [ProcessDate]  [datetime2](0) NOT NULL CONSTRAINT [DF_App_StockMerge_ProcessDate] DEFAULT (SYSDATETIME()),
        [IsActive]     [bit] NULL CONSTRAINT [DF_App_StockMerge_IsActive] DEFAULT (1),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_StockMerge_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockMerge] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockMerge', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockMerge', 'PeriodDate') IS NULL
    ALTER TABLE [dbo].[App_StockMerge] ADD [PeriodDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_StockMerge_PeriodDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_StockMerge', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockMerge', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_StockMerge] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_StockMerge', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockMerge', 'ProcessDate') IS NULL
    ALTER TABLE [dbo].[App_StockMerge] ADD [ProcessDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_StockMerge_ProcessDate_Alt] DEFAULT (SYSDATETIME());
GO
IF OBJECT_ID(N'dbo.App_StockMerge', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockMerge', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_StockMerge] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_StockMerge_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockMerge', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockMerge', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockMerge] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockMerge_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockMerge', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockMerge_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_StockMerge] WITH CHECK
    ADD CONSTRAINT [FK_App_StockMerge_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 22
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (22, N'YamanCam.Core', N'App_StockMerge stoklari birlestir islemi tablosu', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 23 - App_ProductTransfer / App_ProductTransferLine            */
/* (Subeler Arasi Transfer Farkli Urun fisi basligi + satirlari.       */
/*  Her satirda cikan stok + giren/aktarilan stok ayri secilir.        */
/*  Doviz/KDV yok; miktar x birim fiyat = toplam fiyat)                */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_ProductTransfer](
        [RecId]           [int] IDENTITY(1,1) NOT NULL,
        [VoucherNo]       [nvarchar](50) NOT NULL,
        [VoucherDate]     [datetime2](0) NOT NULL,
        [InWorkPlaceId]   [int] NULL,
        [OutWorkPlaceId]  [int] NULL,
        [TransactionType] [nvarchar](40) NOT NULL CONSTRAINT [DF_App_ProductTransfer_TransactionType] DEFAULT (N'Farklı Ürün Transfer'),
        [SpecialCode]     [nvarchar](50) NULL,
        [TotalAmount]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductTransfer_TotalAmount] DEFAULT (0),
        [IsActive]        [bit] NULL CONSTRAINT [DF_App_ProductTransfer_IsActive] DEFAULT (1),
        [CreatedDate]     [datetime2](0) NULL CONSTRAINT [DF_App_ProductTransfer_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_ProductTransfer] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransfer', 'VoucherNo') IS NULL
    ALTER TABLE [dbo].[App_ProductTransfer] ADD [VoucherNo] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_ProductTransfer_VoucherNo] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransfer', 'VoucherDate') IS NULL
    ALTER TABLE [dbo].[App_ProductTransfer] ADD [VoucherDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_ProductTransfer_VoucherDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransfer', 'InWorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_ProductTransfer] ADD [InWorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransfer', 'OutWorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_ProductTransfer] ADD [OutWorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransfer', 'TransactionType') IS NULL
    ALTER TABLE [dbo].[App_ProductTransfer] ADD [TransactionType] [nvarchar](40) NOT NULL CONSTRAINT [DF_App_ProductTransfer_TransactionType_Alt] DEFAULT (N'Farklı Ürün Transfer');
GO
IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransfer', 'SpecialCode') IS NULL
    ALTER TABLE [dbo].[App_ProductTransfer] ADD [SpecialCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransfer', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_ProductTransfer] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductTransfer_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransfer', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_ProductTransfer] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_ProductTransfer_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransfer', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_ProductTransfer] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_ProductTransfer_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductTransfer_InWorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_ProductTransfer] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductTransfer_InWorkPlace]
    FOREIGN KEY([InWorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductTransfer_OutWorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_ProductTransfer] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductTransfer_OutWorkPlace]
    FOREIGN KEY([OutWorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_ProductTransfer_VoucherNo' AND object_id = OBJECT_ID(N'dbo.App_ProductTransfer'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_ProductTransfer_VoucherNo]
    ON [dbo].[App_ProductTransfer]([VoucherNo]);
END
GO

/* ------------------------------------------------------------------ */
/* App_ProductTransferLine (Farkli Urun Transfer fisi stok satirlari)  */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_ProductTransferLine](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [TransferId]   [int] NOT NULL,
        [LineNo]       [int] NOT NULL CONSTRAINT [DF_App_ProductTransferLine_LineNo] DEFAULT (1),
        [OutStockId]   [int] NOT NULL,
        [InStockId]    [int] NOT NULL,
        [StockUnitId]  [int] NULL,
        [Quantity]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductTransferLine_Quantity] DEFAULT (0),
        [UnitPrice]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductTransferLine_UnitPrice] DEFAULT (0),
        [TotalPrice]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductTransferLine_TotalPrice] DEFAULT (0),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_ProductTransferLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_ProductTransferLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransferLine', 'TransferId') IS NULL
    ALTER TABLE [dbo].[App_ProductTransferLine] ADD [TransferId] [int] NOT NULL CONSTRAINT [DF_App_ProductTransferLine_TransferId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransferLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_ProductTransferLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_ProductTransferLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransferLine', 'OutStockId') IS NULL
    ALTER TABLE [dbo].[App_ProductTransferLine] ADD [OutStockId] [int] NOT NULL CONSTRAINT [DF_App_ProductTransferLine_OutStockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransferLine', 'InStockId') IS NULL
    ALTER TABLE [dbo].[App_ProductTransferLine] ADD [InStockId] [int] NOT NULL CONSTRAINT [DF_App_ProductTransferLine_InStockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransferLine', 'StockUnitId') IS NULL
    ALTER TABLE [dbo].[App_ProductTransferLine] ADD [StockUnitId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransferLine', 'Quantity') IS NULL
    ALTER TABLE [dbo].[App_ProductTransferLine] ADD [Quantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductTransferLine_Quantity_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransferLine', 'UnitPrice') IS NULL
    ALTER TABLE [dbo].[App_ProductTransferLine] ADD [UnitPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductTransferLine_UnitPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransferLine', 'TotalPrice') IS NULL
    ALTER TABLE [dbo].[App_ProductTransferLine] ADD [TotalPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductTransferLine_TotalPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductTransferLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_ProductTransferLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_ProductTransferLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_ProductTransfer', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductTransferLine_App_ProductTransfer')
BEGIN
    ALTER TABLE [dbo].[App_ProductTransferLine] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductTransferLine_App_ProductTransfer]
    FOREIGN KEY([TransferId]) REFERENCES [dbo].[App_ProductTransfer]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductTransferLine_OutStock')
BEGIN
    ALTER TABLE [dbo].[App_ProductTransferLine] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductTransferLine_OutStock]
    FOREIGN KEY([OutStockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductTransferLine_InStock')
BEGIN
    ALTER TABLE [dbo].[App_ProductTransferLine] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductTransferLine_InStock]
    FOREIGN KEY([InStockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductTransferLine_App_StockUnit')
BEGIN
    ALTER TABLE [dbo].[App_ProductTransferLine] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductTransferLine_App_StockUnit]
    FOREIGN KEY([StockUnitId]) REFERENCES [dbo].[App_StockUnit]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductTransferLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_ProductTransferLine_TransferId' AND object_id = OBJECT_ID(N'dbo.App_ProductTransferLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_ProductTransferLine_TransferId]
    ON [dbo].[App_ProductTransferLine]([TransferId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 23
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (23, N'YamanCam.Core', N'App_ProductTransfer / App_ProductTransferLine subeler arasi transfer farkli urun master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 24 - App_StockIssue / App_StockIssueLine                      */
/* (Stok Cikis Fisi basligi + stok satirlari, doviz/KDV yok)           */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockIssue](
        [RecId]           [int] IDENTITY(1,1) NOT NULL,
        [VoucherNo]       [nvarchar](50) NOT NULL,
        [VoucherDate]     [datetime2](0) NOT NULL,
        [WorkPlaceId]     [int] NULL,
        [TransactionType] [nvarchar](30) NOT NULL CONSTRAINT [DF_App_StockIssue_TransactionType] DEFAULT (N'Sarf Çıkışı'),
        [SpecialCode]     [nvarchar](50) NULL,
        [TotalAmount]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockIssue_TotalAmount] DEFAULT (0),
        [IsActive]        [bit] NULL CONSTRAINT [DF_App_StockIssue_IsActive] DEFAULT (1),
        [CreatedDate]     [datetime2](0) NULL CONSTRAINT [DF_App_StockIssue_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockIssue] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssue', 'VoucherNo') IS NULL
    ALTER TABLE [dbo].[App_StockIssue] ADD [VoucherNo] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_StockIssue_VoucherNo] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssue', 'VoucherDate') IS NULL
    ALTER TABLE [dbo].[App_StockIssue] ADD [VoucherDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_StockIssue_VoucherDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssue', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_StockIssue] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssue', 'TransactionType') IS NULL
    ALTER TABLE [dbo].[App_StockIssue] ADD [TransactionType] [nvarchar](30) NOT NULL CONSTRAINT [DF_App_StockIssue_TransactionType_Alt] DEFAULT (N'Sarf Çıkışı');
GO
IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssue', 'SpecialCode') IS NULL
    ALTER TABLE [dbo].[App_StockIssue] ADD [SpecialCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssue', 'TotalAmount') IS NULL
    ALTER TABLE [dbo].[App_StockIssue] ADD [TotalAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockIssue_TotalAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssue', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_StockIssue] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_StockIssue_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssue', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockIssue] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockIssue_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockIssue_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_StockIssue] WITH CHECK
    ADD CONSTRAINT [FK_App_StockIssue_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_StockIssue_VoucherNo' AND object_id = OBJECT_ID(N'dbo.App_StockIssue'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_StockIssue_VoucherNo]
    ON [dbo].[App_StockIssue]([VoucherNo]);
END
GO

/* ------------------------------------------------------------------ */
/* App_StockIssueLine (Stok Cikis Fisi stok satirlari)                 */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockIssueLine](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [IssueId]      [int] NOT NULL,
        [LineNo]       [int] NOT NULL CONSTRAINT [DF_App_StockIssueLine_LineNo] DEFAULT (1),
        [StockId]      [int] NOT NULL,
        [StockUnitId]  [int] NULL,
        [Quantity]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockIssueLine_Quantity] DEFAULT (0),
        [UnitPrice]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockIssueLine_UnitPrice] DEFAULT (0),
        [TotalPrice]   [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockIssueLine_TotalPrice] DEFAULT (0),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_StockIssueLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockIssueLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssueLine', 'IssueId') IS NULL
    ALTER TABLE [dbo].[App_StockIssueLine] ADD [IssueId] [int] NOT NULL CONSTRAINT [DF_App_StockIssueLine_IssueId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssueLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_StockIssueLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_StockIssueLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssueLine', 'StockId') IS NULL
    ALTER TABLE [dbo].[App_StockIssueLine] ADD [StockId] [int] NOT NULL CONSTRAINT [DF_App_StockIssueLine_StockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssueLine', 'StockUnitId') IS NULL
    ALTER TABLE [dbo].[App_StockIssueLine] ADD [StockUnitId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssueLine', 'Quantity') IS NULL
    ALTER TABLE [dbo].[App_StockIssueLine] ADD [Quantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockIssueLine_Quantity_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssueLine', 'UnitPrice') IS NULL
    ALTER TABLE [dbo].[App_StockIssueLine] ADD [UnitPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockIssueLine_UnitPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssueLine', 'TotalPrice') IS NULL
    ALTER TABLE [dbo].[App_StockIssueLine] ADD [TotalPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockIssueLine_TotalPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockIssueLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockIssueLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockIssueLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockIssue', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockIssueLine_App_StockIssue')
BEGIN
    ALTER TABLE [dbo].[App_StockIssueLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockIssueLine_App_StockIssue]
    FOREIGN KEY([IssueId]) REFERENCES [dbo].[App_StockIssue]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockIssueLine_App_Stock')
BEGIN
    ALTER TABLE [dbo].[App_StockIssueLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockIssueLine_App_Stock]
    FOREIGN KEY([StockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockUnit', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockIssueLine_App_StockUnit')
BEGIN
    ALTER TABLE [dbo].[App_StockIssueLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockIssueLine_App_StockUnit]
    FOREIGN KEY([StockUnitId]) REFERENCES [dbo].[App_StockUnit]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockIssueLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_StockIssueLine_IssueId' AND object_id = OBJECT_ID(N'dbo.App_StockIssueLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_StockIssueLine_IssueId]
    ON [dbo].[App_StockIssueLine]([IssueId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 24
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (24, N'YamanCam.Core', N'App_StockIssue / App_StockIssueLine stok cikis fisi master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 25 - App_ProductionVoucher / App_ProductionVoucherLine        */
/* (Stok Urun Uretim Fisi basligi + hammadde/mamul satirlari.          */
/*  Fis tarihi + uretim tarihi + sube. Doviz/KDV/tutar yok)            */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_ProductionVoucher](
        [RecId]           [int] IDENTITY(1,1) NOT NULL,
        [VoucherNo]       [nvarchar](50) NOT NULL,
        [VoucherDate]     [datetime2](0) NOT NULL,
        [ProductionDate]  [datetime2](0) NOT NULL,
        [WorkPlaceId]     [int] NULL,
        [TransactionType] [nvarchar](30) NOT NULL CONSTRAINT [DF_App_ProductionVoucher_TransactionType] DEFAULT (N'Üretim'),
        [SpecialCode]     [nvarchar](50) NULL,
        [IsActive]        [bit] NULL CONSTRAINT [DF_App_ProductionVoucher_IsActive] DEFAULT (1),
        [CreatedDate]     [datetime2](0) NULL CONSTRAINT [DF_App_ProductionVoucher_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_ProductionVoucher] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucher', 'VoucherNo') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucher] ADD [VoucherNo] [nvarchar](50) NOT NULL CONSTRAINT [DF_App_ProductionVoucher_VoucherNo] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucher', 'VoucherDate') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucher] ADD [VoucherDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_ProductionVoucher_VoucherDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucher', 'ProductionDate') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucher] ADD [ProductionDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_ProductionVoucher_ProductionDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucher', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucher] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucher', 'TransactionType') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucher] ADD [TransactionType] [nvarchar](30) NOT NULL CONSTRAINT [DF_App_ProductionVoucher_TransactionType_Alt] DEFAULT (N'Üretim');
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucher', 'SpecialCode') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucher] ADD [SpecialCode] [nvarchar](50) NULL;
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucher', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucher] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_ProductionVoucher_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucher', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucher] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_ProductionVoucher_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductionVoucher_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_ProductionVoucher] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductionVoucher_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_ProductionVoucher_VoucherNo' AND object_id = OBJECT_ID(N'dbo.App_ProductionVoucher'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_ProductionVoucher_VoucherNo]
    ON [dbo].[App_ProductionVoucher]([VoucherNo]);
END
GO

/* ------------------------------------------------------------------ */
/* App_ProductionVoucherLine (Uretim Fisi hammadde/mamul satirlari)    */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_ProductionVoucherLine](
        [RecId]               [int] IDENTITY(1,1) NOT NULL,
        [VoucherId]           [int] NOT NULL,
        [LineNo]              [int] NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_LineNo] DEFAULT (1),
        [RawMaterialStockId]  [int] NOT NULL,
        [RawMaterialQuantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_RawQty] DEFAULT (0),
        [RawMaterialUnitPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_RawUnitPrice] DEFAULT (0),
        [RawMaterialNetAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_RawNetAmount] DEFAULT (0),
        [WasteRate]           [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_WasteRate] DEFAULT (0),
        [ProductQuantity]     [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_ProdQty] DEFAULT (0),
        [ProductUnitPrice]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_ProdUnitPrice] DEFAULT (0),
        [ProductNetAmount]    [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_ProdNetAmount] DEFAULT (0),
        [ProductStockId]      [int] NOT NULL,
        [CreatedDate]         [datetime2](0) NULL CONSTRAINT [DF_App_ProductionVoucherLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_ProductionVoucherLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'VoucherId') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [VoucherId] [int] NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_VoucherId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'RawMaterialStockId') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [RawMaterialStockId] [int] NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_RawStockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'RawMaterialQuantity') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [RawMaterialQuantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_RawQty_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'WasteRate') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [WasteRate] [decimal](13, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_WasteRate_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'RawMaterialUnitPrice') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [RawMaterialUnitPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_RawUnitPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'RawMaterialNetAmount') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [RawMaterialNetAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_RawNetAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'ProductQuantity') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [ProductQuantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_ProdQty_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'ProductUnitPrice') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [ProductUnitPrice] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_ProdUnitPrice_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'ProductNetAmount') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [ProductNetAmount] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_ProdNetAmount_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'ProductStockId') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [ProductStockId] [int] NOT NULL CONSTRAINT [DF_App_ProductionVoucherLine_ProdStockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_ProductionVoucherLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_ProductionVoucherLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_ProductionVoucherLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_ProductionVoucher', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductionVoucherLine_App_ProductionVoucher')
BEGIN
    ALTER TABLE [dbo].[App_ProductionVoucherLine] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductionVoucherLine_App_ProductionVoucher]
    FOREIGN KEY([VoucherId]) REFERENCES [dbo].[App_ProductionVoucher]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductionVoucherLine_RawStock')
BEGIN
    ALTER TABLE [dbo].[App_ProductionVoucherLine] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductionVoucherLine_RawStock]
    FOREIGN KEY([RawMaterialStockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_ProductionVoucherLine_ProductStock')
BEGIN
    ALTER TABLE [dbo].[App_ProductionVoucherLine] WITH CHECK
    ADD CONSTRAINT [FK_App_ProductionVoucherLine_ProductStock]
    FOREIGN KEY([ProductStockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_ProductionVoucherLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_ProductionVoucherLine_VoucherId' AND object_id = OBJECT_ID(N'dbo.App_ProductionVoucherLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_ProductionVoucherLine_VoucherId]
    ON [dbo].[App_ProductionVoucherLine]([VoucherId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 25
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (25, N'YamanCam.Core', N'App_ProductionVoucher / App_ProductionVoucherLine stok urun uretim fisi master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 26 - App_StockCount / App_StockCountLine                      */
/* (Sayim Kaydi basligi: baslangic/bitis tarihi + sube;               */
/*  satirlar: stok + sayim miktari. Doviz/KDV/tutar yok)               */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockCount', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockCount](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [StartDate]    [datetime2](0) NOT NULL,
        [EndDate]      [datetime2](0) NOT NULL,
        [WorkPlaceId]  [int] NULL,
        [IsActive]     [bit] NULL CONSTRAINT [DF_App_StockCount_IsActive] DEFAULT (1),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_StockCount_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockCount] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockCount', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCount', 'StartDate') IS NULL
    ALTER TABLE [dbo].[App_StockCount] ADD [StartDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_StockCount_StartDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_StockCount', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCount', 'EndDate') IS NULL
    ALTER TABLE [dbo].[App_StockCount] ADD [EndDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_StockCount_EndDate] DEFAULT (SYSUTCDATETIME());
GO
IF OBJECT_ID(N'dbo.App_StockCount', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCount', 'WorkPlaceId') IS NULL
    ALTER TABLE [dbo].[App_StockCount] ADD [WorkPlaceId] [int] NULL;
GO
IF OBJECT_ID(N'dbo.App_StockCount', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCount', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_StockCount] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_StockCount_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockCount', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCount', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockCount] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockCount_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockCount', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_WorkPlace', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockCount_App_WorkPlace')
BEGIN
    ALTER TABLE [dbo].[App_StockCount] WITH CHECK
    ADD CONSTRAINT [FK_App_StockCount_App_WorkPlace]
    FOREIGN KEY([WorkPlaceId]) REFERENCES [dbo].[App_WorkPlace]([RecId]);
END
GO

/* ------------------------------------------------------------------ */
/* App_StockCountLine (Sayim Kaydi stok satirlari)                     */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_StockCountLine', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_StockCountLine](
        [RecId]         [int] IDENTITY(1,1) NOT NULL,
        [CountId]       [int] NOT NULL,
        [LineNo]        [int] NOT NULL CONSTRAINT [DF_App_StockCountLine_LineNo] DEFAULT (1),
        [StockId]       [int] NOT NULL,
        [CountQuantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockCountLine_CountQuantity] DEFAULT (0),
        [CreatedDate]   [datetime2](0) NULL CONSTRAINT [DF_App_StockCountLine_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_StockCountLine] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_StockCountLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCountLine', 'CountId') IS NULL
    ALTER TABLE [dbo].[App_StockCountLine] ADD [CountId] [int] NOT NULL CONSTRAINT [DF_App_StockCountLine_CountId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockCountLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCountLine', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_StockCountLine] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_StockCountLine_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_StockCountLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCountLine', 'StockId') IS NULL
    ALTER TABLE [dbo].[App_StockCountLine] ADD [StockId] [int] NOT NULL CONSTRAINT [DF_App_StockCountLine_StockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockCountLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCountLine', 'CountQuantity') IS NULL
    ALTER TABLE [dbo].[App_StockCountLine] ADD [CountQuantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_StockCountLine_CountQuantity_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_StockCountLine', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_StockCountLine', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_StockCountLine] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_StockCountLine_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_StockCountLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_StockCount', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockCountLine_App_StockCount')
BEGIN
    ALTER TABLE [dbo].[App_StockCountLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockCountLine_App_StockCount]
    FOREIGN KEY([CountId]) REFERENCES [dbo].[App_StockCount]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_StockCountLine', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_StockCountLine_App_Stock')
BEGIN
    ALTER TABLE [dbo].[App_StockCountLine] WITH CHECK
    ADD CONSTRAINT [FK_App_StockCountLine_App_Stock]
    FOREIGN KEY([StockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_StockCountLine', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_StockCountLine_CountId' AND object_id = OBJECT_ID(N'dbo.App_StockCountLine'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_StockCountLine_CountId]
    ON [dbo].[App_StockCountLine]([CountId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 26
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (26, N'YamanCam.Core', N'App_StockCount / App_StockCountLine sayim kaydi master-detail', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 27 - App_Setting (Ayarlar / Kusurat Haneleri)                 */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_Setting', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_Setting](
        [RecId]        [int] IDENTITY(1,1) NOT NULL,
        [SettingGroup] [nvarchar](100) NOT NULL CONSTRAINT [DF_App_Setting_SettingGroup] DEFAULT (N''),
        [Explanation]  [nvarchar](200) NOT NULL CONSTRAINT [DF_App_Setting_Explanation] DEFAULT (N''),
        [Value]        [nvarchar](200) NOT NULL CONSTRAINT [DF_App_Setting_Value] DEFAULT (N''),
        [IsActive]     [bit] NULL CONSTRAINT [DF_App_Setting_IsActive] DEFAULT (1),
        [CreatedDate]  [datetime2](0) NULL CONSTRAINT [DF_App_Setting_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_Setting] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_Setting', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Setting', 'SettingGroup') IS NULL
    ALTER TABLE [dbo].[App_Setting] ADD [SettingGroup] [nvarchar](100) NOT NULL CONSTRAINT [DF_App_Setting_SettingGroup_Alt] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_Setting', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Setting', 'Explanation') IS NULL
    ALTER TABLE [dbo].[App_Setting] ADD [Explanation] [nvarchar](200) NOT NULL CONSTRAINT [DF_App_Setting_Explanation_Alt] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_Setting', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Setting', 'Value') IS NULL
    ALTER TABLE [dbo].[App_Setting] ADD [Value] [nvarchar](200) NOT NULL CONSTRAINT [DF_App_Setting_Value_Alt] DEFAULT (N'');
GO
IF OBJECT_ID(N'dbo.App_Setting', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Setting', 'IsActive') IS NULL
    ALTER TABLE [dbo].[App_Setting] ADD [IsActive] [bit] NULL CONSTRAINT [DF_App_Setting_IsActive_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_Setting', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Setting', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_Setting] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_Setting_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 27
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (27, N'YamanCam.Core', N'App_Setting ayarlar (kusurat haneleri) tablosu', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 28 - App_Setting.Value alanini metin (nvarchar) yap           */
/*            (sayisal ve metinsel deger girilebilsin)                 */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_Setting', N'U') IS NOT NULL
   AND EXISTS (
       SELECT 1
       FROM sys.columns c
       JOIN sys.types t ON t.user_type_id = c.user_type_id
       WHERE c.object_id = OBJECT_ID(N'dbo.App_Setting') AND c.name = N'Value' AND t.name <> N'nvarchar'
   )
BEGIN
    DECLARE @ValueDefaultConstraint sysname;
    SELECT @ValueDefaultConstraint = dc.name
    FROM sys.default_constraints dc
    JOIN sys.columns c ON c.object_id = dc.parent_object_id AND c.column_id = dc.parent_column_id
    WHERE dc.parent_object_id = OBJECT_ID(N'dbo.App_Setting') AND c.name = N'Value';

    IF @ValueDefaultConstraint IS NOT NULL
        EXEC(N'ALTER TABLE [dbo].[App_Setting] DROP CONSTRAINT [' + @ValueDefaultConstraint + N']');

    ALTER TABLE [dbo].[App_Setting] ALTER COLUMN [Value] [nvarchar](200) NOT NULL;

    EXEC(N'ALTER TABLE [dbo].[App_Setting] ADD CONSTRAINT [DF_App_Setting_Value] DEFAULT (N''0'') FOR [Value]');
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 28
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (28, N'YamanCam.Core', N'App_Setting.Value alani nvarchar yapildi (metin+sayi destegi)', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 29 - Tum miktar/tutar/oran decimal kolonlarinin olcegi        */
/*            (scale) 10 haneye genisletildi. Kusurat ayarlari artik   */
/*            yalnizca ekranda kac hane gosterilecegini belirler;      */
/*            veritabaninda deger her zaman tam hassasiyetle saklanir. */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL
BEGIN
    DECLARE @DecimalMigrations TABLE (
        TableName sysname,
        ColumnName sysname,
        TargetPrecision int,
        TargetScale int
    );

    INSERT INTO @DecimalMigrations (TableName, ColumnName, TargetPrecision, TargetScale)
    VALUES
        (N'App_CustomsFreightInvoiceLine', N'Amount', 28, 10),
        (N'App_CustomsFreightInvoiceLine', N'VatRate', 13, 10),
        (N'App_CustomsFreightInvoiceLine', N'WithholdingRate', 13, 10),
        (N'App_CustomsFreightInvoiceLine', N'VatAmount', 28, 10),
        (N'App_CustomsFreightInvoiceLine', N'WithholdingAmount', 28, 10),
        (N'App_CustomsFreightInvoiceLine', N'NetVatAmount', 28, 10),
        (N'App_CustomsFreightInvoiceLine', N'TotalAmount', 28, 10),
        (N'App_CustomsFreightInvoice', N'NetAmount', 28, 10),
        (N'App_CustomsFreightInvoice', N'VatAmount', 28, 10),
        (N'App_CustomsFreightInvoice', N'WithholdingAmount', 28, 10),
        (N'App_CustomsFreightInvoice', N'NetVatAmount', 28, 10),
        (N'App_CustomsFreightInvoice', N'TotalAmount', 28, 10),
        (N'App_CustomsFreightInvoice', N'CurrencyAmount', 28, 10),
        (N'App_CustomsFreightInvoice', N'ExchangeRate', 28, 10),
        (N'App_JournalVoucherLine', N'DebitAmount', 28, 10),
        (N'App_JournalVoucherLine', N'CreditAmount', 28, 10),
        (N'App_JournalVoucher', N'TotalDebit', 28, 10),
        (N'App_JournalVoucher', N'TotalCredit', 28, 10),
        (N'App_ProductionDefinition', N'ProductionQuantity', 28, 10),
        (N'App_ProductionVoucherLine', N'RawMaterialQuantity', 28, 10),
        (N'App_ProductionVoucherLine', N'WasteRate', 13, 10),
        (N'App_ProductionVoucherLine', N'ProductQuantity', 28, 10),
        (N'App_ProductTransferLine', N'Quantity', 28, 10),
        (N'App_ProductTransferLine', N'UnitPrice', 28, 10),
        (N'App_ProductTransferLine', N'TotalPrice', 28, 10),
        (N'App_ProductTransfer', N'TotalAmount', 28, 10),
        (N'App_PurchaseInvoiceLine', N'Quantity', 28, 10),
        (N'App_PurchaseInvoiceLine', N'UnitPrice', 28, 10),
        (N'App_PurchaseInvoiceLine', N'VatRate', 13, 10),
        (N'App_PurchaseInvoiceLine', N'WithholdingRate', 13, 10),
        (N'App_PurchaseInvoiceLine', N'NetAmount', 28, 10),
        (N'App_PurchaseInvoiceLine', N'VatAmount', 28, 10),
        (N'App_PurchaseInvoiceLine', N'WithholdingAmount', 28, 10),
        (N'App_PurchaseInvoiceLine', N'NetVatAmount', 28, 10),
        (N'App_PurchaseInvoiceLine', N'TotalAmount', 28, 10),
        (N'App_PurchaseInvoice', N'ExchangeRate', 28, 10),
        (N'App_PurchaseInvoice', N'NetAmount', 28, 10),
        (N'App_PurchaseInvoice', N'VatAmount', 28, 10),
        (N'App_PurchaseInvoice', N'WithholdingAmount', 28, 10),
        (N'App_PurchaseInvoice', N'NetVatAmount', 28, 10),
        (N'App_PurchaseInvoice', N'TotalAmount', 28, 10),
        (N'App_PurchaseInvoice', N'NetAmountTRY', 28, 10),
        (N'App_PurchaseInvoice', N'VatAmountTRY', 28, 10),
        (N'App_PurchaseInvoice', N'WithholdingAmountTRY', 28, 10),
        (N'App_PurchaseInvoice', N'NetVatAmountTRY', 28, 10),
        (N'App_PurchaseInvoice', N'TotalAmountTRY', 28, 10),
        (N'App_SalesInvoiceLine', N'Quantity', 28, 10),
        (N'App_SalesInvoiceLine', N'UnitPrice', 28, 10),
        (N'App_SalesInvoiceLine', N'VatRate', 13, 10),
        (N'App_SalesInvoiceLine', N'WithholdingRate', 13, 10),
        (N'App_SalesInvoiceLine', N'NetAmount', 28, 10),
        (N'App_SalesInvoiceLine', N'VatAmount', 28, 10),
        (N'App_SalesInvoiceLine', N'WithholdingAmount', 28, 10),
        (N'App_SalesInvoiceLine', N'NetVatAmount', 28, 10),
        (N'App_SalesInvoiceLine', N'TotalAmount', 28, 10),
        (N'App_SalesInvoice', N'ExchangeRate', 28, 10),
        (N'App_SalesInvoice', N'NetAmount', 28, 10),
        (N'App_SalesInvoice', N'VatAmount', 28, 10),
        (N'App_SalesInvoice', N'WithholdingAmount', 28, 10),
        (N'App_SalesInvoice', N'NetVatAmount', 28, 10),
        (N'App_SalesInvoice', N'TotalAmount', 28, 10),
        (N'App_SalesInvoice', N'NetAmountTRY', 28, 10),
        (N'App_SalesInvoice', N'VatAmountTRY', 28, 10),
        (N'App_SalesInvoice', N'WithholdingAmountTRY', 28, 10),
        (N'App_SalesInvoice', N'NetVatAmountTRY', 28, 10),
        (N'App_SalesInvoice', N'TotalAmountTRY', 28, 10),
        (N'App_StockIssue', N'TotalAmount', 28, 10),
        (N'App_StockCountLine', N'CountQuantity', 28, 10),
        (N'App_Stock', N'ProductionWeight', 28, 10),
        (N'App_StockOpeningLine', N'Quantity', 28, 10),
        (N'App_StockOpeningLine', N'UnitPrice', 28, 10),
        (N'App_StockOpeningLine', N'TotalPrice', 28, 10),
        (N'App_StockIssueLine', N'Quantity', 28, 10),
        (N'App_StockIssueLine', N'UnitPrice', 28, 10),
        (N'App_StockIssueLine', N'TotalPrice', 28, 10),
        (N'App_StockTransferLine', N'Quantity', 28, 10),
        (N'App_StockTransferLine', N'UnitPrice', 28, 10),
        (N'App_StockTransferLine', N'TotalPrice', 28, 10),
        (N'App_VatWithholdingDefinition', N'VatRate', 13, 10),
        (N'App_VatWithholdingDefinition', N'WithholdingRate', 13, 10),
        (N'App_StockOpening', N'TotalAmount', 28, 10),
        (N'App_StockTransfer', N'TotalAmount', 28, 10),
        (N'App_VatDefinition', N'VatRate', 13, 10);

    DECLARE @MigTableName sysname, @MigColumnName sysname, @MigTargetPrecision int, @MigTargetScale int;
    DECLARE @MigIsNullable bit, @MigSql nvarchar(max);

    DECLARE decimalMigrationCursor CURSOR LOCAL FAST_FORWARD FOR
        SELECT TableName, ColumnName, TargetPrecision, TargetScale FROM @DecimalMigrations;

    OPEN decimalMigrationCursor;
    FETCH NEXT FROM decimalMigrationCursor INTO @MigTableName, @MigColumnName, @MigTargetPrecision, @MigTargetScale;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        IF EXISTS (
            SELECT 1
            FROM sys.columns c
            JOIN sys.types ty ON ty.user_type_id = c.user_type_id
            WHERE c.object_id = OBJECT_ID(N'dbo.' + @MigTableName)
              AND c.name = @MigColumnName
              AND ty.name = N'decimal'
              AND (c.precision <> @MigTargetPrecision OR c.scale <> @MigTargetScale)
        )
        BEGIN
            SELECT @MigIsNullable = c.is_nullable
            FROM sys.columns c
            WHERE c.object_id = OBJECT_ID(N'dbo.' + @MigTableName) AND c.name = @MigColumnName;

            SET @MigSql = N'ALTER TABLE [dbo].[' + @MigTableName + N'] ALTER COLUMN [' + @MigColumnName + N'] [decimal](' +
                CAST(@MigTargetPrecision AS nvarchar(10)) + N', ' + CAST(@MigTargetScale AS nvarchar(10)) + N') ' +
                CASE WHEN @MigIsNullable = 1 THEN N'NULL' ELSE N'NOT NULL' END;
            EXEC sp_executesql @MigSql;
        END

        FETCH NEXT FROM decimalMigrationCursor INTO @MigTableName, @MigColumnName, @MigTargetPrecision, @MigTargetScale;
    END

    CLOSE decimalMigrationCursor;
    DEALLOCATE decimalMigrationCursor;
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 29
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (29, N'YamanCam.Core', N'Decimal kolonlarin olcegi (scale) 10 haneye genisletildi', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 30 - App_SalesInvoice.IsReturn alani (Satis Iade Faturasi)    */
/*            InvoiceNo benzersizligi artık (InvoiceNo, IsReturn)     */
/*            ikilisine gore kontrol edilir; boylece Satis Faturasi    */
/*            ve Satis Iade Faturasi kendi ayri numara serilerini      */
/*            kullanabilir.                                            */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_SalesInvoice_InvoiceNo' AND object_id = OBJECT_ID(N'dbo.App_SalesInvoice'))
BEGIN
    DROP INDEX [UX_App_SalesInvoice_InvoiceNo] ON [dbo].[App_SalesInvoice];
END
GO

IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_SalesInvoice_InvoiceNo_IsReturn' AND object_id = OBJECT_ID(N'dbo.App_SalesInvoice'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_SalesInvoice_InvoiceNo_IsReturn]
    ON [dbo].[App_SalesInvoice]([InvoiceNo], [IsReturn]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 30
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (30, N'YamanCam.Core', N'App_SalesInvoice.IsReturn alani eklendi (Satis Iade Faturasi)', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 31 - App_PurchaseInvoice.IsReturn alani (Alis Iade Faturasi)  */
/*            InvoiceNo benzersizligi artık (InvoiceNo, IsReturn)     */
/*            ikilisine gore kontrol edilir; boylece Alis Faturasi    */
/*            ve Alis Iade Faturasi kendi ayri numara serilerini       */
/*            kullanabilir.                                            */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL
   AND EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_PurchaseInvoice_InvoiceNo' AND object_id = OBJECT_ID(N'dbo.App_PurchaseInvoice'))
BEGIN
    DROP INDEX [UX_App_PurchaseInvoice_InvoiceNo] ON [dbo].[App_PurchaseInvoice];
END
GO

IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'UX_App_PurchaseInvoice_InvoiceNo_IsReturn' AND object_id = OBJECT_ID(N'dbo.App_PurchaseInvoice'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX [UX_App_PurchaseInvoice_InvoiceNo_IsReturn]
    ON [dbo].[App_PurchaseInvoice]([InvoiceNo], [IsReturn]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 31
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (31, N'YamanCam.Core', N'App_PurchaseInvoice.IsReturn alani eklendi (Alis Iade Faturasi)', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 32 - App_ProductionVoucherLine hammadde/mamul birim fiyat ve  */
/*            tutar alanlari eklendi (otomatik uretim satiri hesabi)   */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 32
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (32, N'YamanCam.Core', N'App_ProductionVoucherLine.RawMaterialUnitPrice/NetAmount, ProductUnitPrice/NetAmount alanlari eklendi', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 33 - App_CustomsFreightInvoiceMaterial (Gumruk Nakliye        */
/*            Faturasina birden fazla malzeme + miktar secimi;        */
/*            "Stok Fat No" (LinkedPurchaseInvoiceId) alaninin yerini  */
/*            alir; o kolon geriye donuk uyumluluk icin DB'de kalir    */
/*            ama artik kullanilmaz)                                   */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[App_CustomsFreightInvoiceMaterial](
        [RecId]              [int] IDENTITY(1,1) NOT NULL,
        [InvoiceId]          [int] NOT NULL,
        [LineNo]             [int] NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceMaterial_LineNo] DEFAULT (1),
        [StockId]            [int] NOT NULL,
        [Quantity]           [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceMaterial_Quantity] DEFAULT (0),
        [CreatedDate]        [datetime2](0) NULL CONSTRAINT [DF_App_CustomsFreightInvoiceMaterial_CreatedDate] DEFAULT (SYSUTCDATETIME()),
        CONSTRAINT [PK_App_CustomsFreightInvoiceMaterial] PRIMARY KEY CLUSTERED ([RecId] ASC)
    ) ON [PRIMARY];
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceMaterial', 'InvoiceId') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceMaterial] ADD [InvoiceId] [int] NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceMaterial_InvoiceId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceMaterial', 'LineNo') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceMaterial] ADD [LineNo] [int] NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceMaterial_LineNo_Alt] DEFAULT (1);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceMaterial', 'StockId') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceMaterial] ADD [StockId] [int] NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceMaterial_StockId] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceMaterial', 'Quantity') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceMaterial] ADD [Quantity] [decimal](28, 10) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoiceMaterial_Quantity_Alt] DEFAULT (0);
GO
IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoiceMaterial', 'CreatedDate') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceMaterial] ADD [CreatedDate] [datetime2](0) NULL CONSTRAINT [DF_App_CustomsFreightInvoiceMaterial_CreatedDate_Alt] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_CustomsFreightInvoiceMaterial_App_CustomsFreightInvoice')
BEGIN
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceMaterial] WITH CHECK
    ADD CONSTRAINT [FK_App_CustomsFreightInvoiceMaterial_App_CustomsFreightInvoice]
    FOREIGN KEY([InvoiceId]) REFERENCES [dbo].[App_CustomsFreightInvoice]([RecId])
    ON DELETE CASCADE;
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_CustomsFreightInvoiceMaterial_App_Stock')
BEGIN
    ALTER TABLE [dbo].[App_CustomsFreightInvoiceMaterial] WITH CHECK
    ADD CONSTRAINT [FK_App_CustomsFreightInvoiceMaterial_App_Stock]
    FOREIGN KEY([StockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_CustomsFreightInvoiceMaterial_InvoiceId' AND object_id = OBJECT_ID(N'dbo.App_CustomsFreightInvoiceMaterial'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_CustomsFreightInvoiceMaterial_InvoiceId]
    ON [dbo].[App_CustomsFreightInvoiceMaterial]([InvoiceId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 33
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (33, N'YamanCam.Core', N'App_CustomsFreightInvoiceMaterial eklendi (coklu malzeme + miktar secimi, Stok Fat No baglantisinin yerine)', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 34 - Tum fatura tablolarina (Alis, Satis, Gumruk Nakliye)     */
/*            "Islem Tarihi" (TransactionDate) alani eklendi. Fatura   */
/*            uzerinde yazan tarihten (InvoiceDate) ayri, isleme       */
/*            ozgu ikinci bir tarih alanidir. Alis Iade ve Satis Iade  */
/*            ekranlari sirasiyla App_PurchaseInvoice / App_SalesInvoice*/
/*            tablolarini paylastigindan ayrica bir islem gerekmez.    */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_PurchaseInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_PurchaseInvoice', 'TransactionDate') IS NULL
    ALTER TABLE [dbo].[App_PurchaseInvoice] ADD [TransactionDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_PurchaseInvoice_TransactionDate] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_SalesInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_SalesInvoice', 'TransactionDate') IS NULL
    ALTER TABLE [dbo].[App_SalesInvoice] ADD [TransactionDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_SalesInvoice_TransactionDate] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_CustomsFreightInvoice', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_CustomsFreightInvoice', 'TransactionDate') IS NULL
    ALTER TABLE [dbo].[App_CustomsFreightInvoice] ADD [TransactionDate] [datetime2](0) NOT NULL CONSTRAINT [DF_App_CustomsFreightInvoice_TransactionDate] DEFAULT (SYSUTCDATETIME());
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 34
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (34, N'YamanCam.Core', N'App_PurchaseInvoice / App_SalesInvoice / App_CustomsFreightInvoice tablolarina TransactionDate (Islem Tarihi) alani eklendi', SYSUTCDATETIME());
END
GO

/* ------------------------------------------------------------------ */
/* Surum 35 - App_Stock.MergeStockId (Birlestirme Kodu) alani eklendi  */
/*            Bir stok kartinin birlestirilecegi baska bir stok        */
/*            kartina isaret eden kendine referansli (self-referencing)*/
/*            isteğe bagli alandir.                                    */
/* ------------------------------------------------------------------ */
IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL AND COL_LENGTH('dbo.App_Stock', 'MergeStockId') IS NULL
    ALTER TABLE [dbo].[App_Stock] ADD [MergeStockId] [int] NULL;
GO

IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_App_Stock_MergeStock')
BEGIN
    ALTER TABLE [dbo].[App_Stock] WITH CHECK
    ADD CONSTRAINT [FK_App_Stock_MergeStock]
    FOREIGN KEY([MergeStockId]) REFERENCES [dbo].[App_Stock]([RecId]);
END
GO

IF OBJECT_ID(N'dbo.App_Stock', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_App_Stock_MergeStockId' AND object_id = OBJECT_ID(N'dbo.App_Stock'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_App_Stock_MergeStockId]
    ON [dbo].[App_Stock]([MergeStockId]);
END
GO

IF OBJECT_ID(N'dbo.App_SchemaVersion', N'U') IS NOT NULL
   AND NOT EXISTS (
       SELECT 1 FROM [dbo].[App_SchemaVersion]
       WHERE [ScriptName] = N'YamanCam.Core' AND [VersionNo] = 35
   )
BEGIN
    INSERT INTO [dbo].[App_SchemaVersion] ([VersionNo], [ScriptName], [Description], [AppliedUtc])
    VALUES (35, N'YamanCam.Core', N'App_Stock.MergeStockId (Birlestirme Kodu) alani eklendi', SYSUTCDATETIME());
END
GO
