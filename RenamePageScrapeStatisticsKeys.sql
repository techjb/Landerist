UPDATE [dbo].[GLOBAL_STATISTICS]
SET [Key] = 'LastScrapePages'
WHERE [Key] = 'UpdatedPages';
GO

UPDATE [dbo].[HOST_STATISTICS]
SET [Key] = 'LastScrape'
WHERE [Key] = 'Updated';
GO
