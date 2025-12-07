-- نسخ احتياطي لقاعدة البيانات
DECLARE @backupPath NVARCHAR(500);
DECLARE @databaseName NVARCHAR(100) = 'YemenChatDB';
DECLARE @dateTime NVARCHAR(20) = CONVERT(NVARCHAR(20), GETDATE(), 112) + '_' + 
                                  REPLACE(CONVERT(NVARCHAR(20), GETDATE(), 108), ':', '');

-- المسار الافتراضي للنسخ الاحتياطي
SET @backupPath = 'C:\Backups\YemenChatDB_' + @dateTime + '.bak';

-- إنشاء المجلد إذا لم يكن موجوداً
DECLARE @folderPath NVARCHAR(500) = LEFT(@backupPath, LEN(@backupPath) - CHARINDEX('\', REVERSE(@backupPath)) + 1);
EXEC xp_create_subdir @folderPath;

-- تنفيذ النسخ الاحتياطي
BACKUP DATABASE @databaseName 
TO DISK = @backupPath
WITH FORMAT,
     MEDIANAME = 'YemenChatBackup',
     NAME = 'Full Backup of YemenChatDB';

PRINT '✅ تم إنشاء نسخة احتياطية في: ' + @backupPath;