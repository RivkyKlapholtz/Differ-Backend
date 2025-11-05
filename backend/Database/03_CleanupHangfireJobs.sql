-- ניקוי Jobs ישנים מ-Hangfire
-- הרץ את הסקריפט הזה אם אתה רואה שגיאות של "target method was not found"

USE [DiffSpectrumView]
GO

PRINT 'Starting Hangfire cleanup...'
GO

-- מחיקת כל ה-Jobs הישנים
DELETE FROM [HangFire].[State]
PRINT 'Deleted job states'

DELETE FROM [HangFire].[JobParameter]
PRINT 'Deleted job parameters'

DELETE FROM [HangFire].[Job]
PRINT 'Deleted jobs'

-- מחיקת Recurring Jobs ישנים
DELETE FROM [HangFire].[Set] WHERE [Key] LIKE 'recurring-jobs%'
DELETE FROM [HangFire].[Hash] WHERE [Key] LIKE 'recurring-job:%'
PRINT 'Deleted recurring jobs'

-- איפוס Counters
DELETE FROM [HangFire].[Counter]
DELETE FROM [HangFire].[AggregatedCounter]
PRINT 'Reset counters'

PRINT ''
PRINT '✅ Hangfire cleanup completed successfully!'
PRINT 'You can now restart your application.'
PRINT ''
GO
