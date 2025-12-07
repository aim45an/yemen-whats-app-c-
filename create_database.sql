-- إنشاء قاعدة البيانات
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'YemenChatDB')
BEGIN
    CREATE DATABASE YemenChatDB
    COLLATE Arabic_CI_AS;
    PRINT '✅ تم إنشاء قاعدة البيانات YemenChatDB';
END
GO

USE YemenChatDB;
GO

-- إنشاء جدول المستخدمين
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Username] [nvarchar](100) NOT NULL,
        [ConnectionId] [nvarchar](500) NULL,
        [IsOnline] [bit] NOT NULL DEFAULT 1,
        [LastSeen] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Status] [nvarchar](100) NULL DEFAULT N'متصل',
        [Color] [nvarchar](50) NULL DEFAULT N'#0078D7',
        [Avatar] [nvarchar](500) NULL DEFAULT N'👤',
        [Bio] [nvarchar](500) NULL,
        [Name] [nvarchar](100) NULL,
        [Email] [nvarchar](100) NULL,
        [PhoneNumber] [nvarchar](20) NULL,
        [IsActive] [bit] NOT NULL DEFAULT 1,
        [LastLogin] [datetime2](7) NULL,
        [LoginCount] [int] NOT NULL DEFAULT 0,
        [NotificationsEnabled] [bit] NOT NULL DEFAULT 1,
        [SoundEnabled] [bit] NOT NULL DEFAULT 1,
        [Theme] [nvarchar](50) NULL DEFAULT N'light',
        [TotalMessagesSent] [int] NOT NULL DEFAULT 0,
        [TotalMessagesReceived] [int] NOT NULL DEFAULT 0,
        [TotalFilesSent] [int] NOT NULL DEFAULT 0,
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
    
    CREATE UNIQUE INDEX [IX_Users_Username] 
    ON [dbo].[Users] ([Username]);
    
    CREATE INDEX [IX_Users_IsOnline] 
    ON [dbo].[Users] ([IsOnline]);
    
    CREATE INDEX [IX_Users_LastSeen] 
    ON [dbo].[Users] ([LastSeen]);
    
    PRINT '✅ تم إنشاء جدول المستخدمين';
END
GO

-- إنشاء جدول الرسائل
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Messages' AND xtype='U')
BEGIN
    CREATE TABLE [dbo].[Messages] (
        [Id] [int] IDENTITY(1,1) NOT NULL,
        [Sender] [nvarchar](100) NOT NULL,
        [Receiver] [nvarchar](100) NOT NULL DEFAULT N'الجميع',
        [Content] [nvarchar](MAX) NOT NULL,
        [MessageType] [nvarchar](50) NULL DEFAULT N'text',
        [FilePath] [nvarchar](500) NULL,
        [FileName] [nvarchar](500) NULL,
        [FileSize] [bigint] NULL,
        [IsPrivate] [bit] NOT NULL DEFAULT 0,
        [IsRead] [bit] NOT NULL DEFAULT 0,
        [Timestamp] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [Status] [nvarchar](50) NULL DEFAULT N'sent',
        [CreatedAt] [datetime2](7) NOT NULL DEFAULT GETDATE(),
        [SenderId] [int] NULL,
        [ReceiverId] [int] NULL,
        CONSTRAINT [PK_Messages] PRIMARY KEY ([Id])
    );
    
    CREATE INDEX [IX_Messages_Sender] 
    ON [dbo].[Messages] ([Sender]);
    
    CREATE INDEX [IX_Messages_Receiver] 
    ON [dbo].[Messages] ([Receiver]);
    
    CREATE INDEX [IX_Messages_Timestamp] 
    ON [dbo].[Messages] ([Timestamp]);
    
    CREATE INDEX [IX_Messages_IsPrivate] 
    ON [dbo].[Messages] ([IsPrivate]);
    
    CREATE INDEX [IX_Messages_Sender_Receiver_Timestamp] 
    ON [dbo].[Messages] ([Sender], [Receiver], [Timestamp]);
    
    CREATE INDEX [IX_Messages_Receiver_Sender_Timestamp] 
    ON [dbo].[Messages] ([Receiver], [Sender], [Timestamp]);
    
    PRINT '✅ تم إنشاء جدول الرسائل';
END
GO

-- إضافة البيانات الأولية
IF NOT EXISTS (SELECT * FROM [Users] WHERE Username = N'النظام')
BEGIN
    INSERT INTO [Users] (Username, Status, IsOnline, Color, Avatar, Bio)
    VALUES (N'النظام', N'نظام', 0, N'#808080', N'🤖', N'حساب النظام - Yemen WhatsApp');
    PRINT '✅ تم إضافة المستخدم النظامي';
END
GO

IF NOT EXISTS (SELECT * FROM [Messages])
BEGIN
    INSERT INTO [Messages] (Sender, Receiver, Content, IsPrivate, Status, Timestamp)
    VALUES (N'النظام', N'الجميع', 
            N'🎉 مرحباً بكم في Yemen WhatsApp Desktop! 💬' + CHAR(13) + CHAR(10) + 
            CHAR(13) + CHAR(10) + 'يمكنكم الآن الدردشة مع الأصدقاء والزملاء بشكل آمن وسريع.' + CHAR(13) + CHAR(10) + 
            CHAR(13) + CHAR(10) + '🇾🇪 تطوير يمني ١٠٠٪', 
            0, N'sent', DATEADD(minute, -5, GETDATE()));
    PRINT '✅ تم إضافة الرسالة الترحيبية';
END
GO

-- إضافة مستخدمين تجريبيين
IF NOT EXISTS (SELECT * FROM [Users] WHERE Username = N'أحمد')
BEGIN
    INSERT INTO [Users] (Username, Status, IsOnline, Color, Avatar, Bio)
    VALUES (N'أحمد', N'متصل', 1, N'#0078D7', N'👤', N'مبرمج ومطور برمجيات');
END

IF NOT EXISTS (SELECT * FROM [Users] WHERE Username = N'محمد')
BEGIN
    INSERT INTO [Users] (Username, Status, IsOnline, Color, Avatar, Bio)
    VALUES (N'محمد', N'متصل', 1, N'#107C10', N'👤', N'مهندس حاسوب');
END

IF NOT EXISTS (SELECT * FROM [Users] WHERE Username = N'فاطمة')
BEGIN
    INSERT INTO [Users] (Username, Status, IsOnline, Color, Avatar, Bio)
    VALUES (N'فاطمة', N'متصل', 1, N'#5C2D91', N'👤', N'مصممة جرافيك');
END
GO

PRINT '🎉 تم تهيئة قاعدة البيانات بنجاح!';