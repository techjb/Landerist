USE [Landerist]
GO

SET XACT_ABORT ON
GO

BEGIN TRANSACTION
GO

/* PAGES: ResponseBodyTextHash -> ListingParserInputHash */
IF COL_LENGTH('dbo.PAGES', 'ResponseBodyTextHash') IS NOT NULL
   AND COL_LENGTH('dbo.PAGES', 'ListingParserInputHash') IS NULL
BEGIN
    EXEC sp_rename 'dbo.PAGES.ResponseBodyTextHash', 'ListingParserInputHash', 'COLUMN'
END
GO

IF COL_LENGTH('dbo.PAGES', 'ResponseBodyTextHash') IS NULL
   AND COL_LENGTH('dbo.PAGES', 'ListingParserInputHash') IS NULL
BEGIN
    ALTER TABLE [dbo].[PAGES]
    ADD [ListingParserInputHash] [char](64) NULL
END
GO

IF COL_LENGTH('dbo.PAGES', 'ResponseBodyTextHash') IS NOT NULL
   AND COL_LENGTH('dbo.PAGES', 'ListingParserInputHash') IS NOT NULL
BEGIN
    THROW 51000, 'Both PAGES.ResponseBodyTextHash and PAGES.ListingParserInputHash exist. Resolve this manually before running the rename migration.', 1
END
GO

/* PAGES: ResponseBodyTextNotChangedCounter -> ListingParserInputNotChangedCounter */
IF COL_LENGTH('dbo.PAGES', 'ResponseBodyTextNotChangedCounter') IS NOT NULL
   AND COL_LENGTH('dbo.PAGES', 'ListingParserInputNotChangedCounter') IS NULL
BEGIN
    EXEC sp_rename 'dbo.PAGES.ResponseBodyTextNotChangedCounter', 'ListingParserInputNotChangedCounter', 'COLUMN'
END
GO

IF COL_LENGTH('dbo.PAGES', 'ResponseBodyTextNotChangedCounter') IS NULL
   AND COL_LENGTH('dbo.PAGES', 'ListingParserInputNotChangedCounter') IS NULL
BEGIN
    ALTER TABLE [dbo].[PAGES]
    ADD [ListingParserInputNotChangedCounter] [smallint] NULL
END
GO

IF COL_LENGTH('dbo.PAGES', 'ResponseBodyTextNotChangedCounter') IS NOT NULL
   AND COL_LENGTH('dbo.PAGES', 'ListingParserInputNotChangedCounter') IS NOT NULL
BEGIN
    THROW 51001, 'Both PAGES.ResponseBodyTextNotChangedCounter and PAGES.ListingParserInputNotChangedCounter exist. Resolve this manually before running the rename migration.', 1
END
GO

/* NOT_LISTINGS_CACHE: ResponseBodyTextHash -> ListingParserInputHash */
IF OBJECT_ID('dbo.NOT_LISTINGS_CACHE', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.NOT_LISTINGS_CACHE', 'ResponseBodyTextHash') IS NOT NULL
   AND COL_LENGTH('dbo.NOT_LISTINGS_CACHE', 'ListingParserInputHash') IS NULL
BEGIN
    EXEC sp_rename 'dbo.NOT_LISTINGS_CACHE.ResponseBodyTextHash', 'ListingParserInputHash', 'COLUMN'
END
GO

IF OBJECT_ID('dbo.NOT_LISTINGS_CACHE', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.NOT_LISTINGS_CACHE', 'ResponseBodyTextHash') IS NULL
   AND COL_LENGTH('dbo.NOT_LISTINGS_CACHE', 'ListingParserInputHash') IS NULL
BEGIN
    ALTER TABLE [dbo].[NOT_LISTINGS_CACHE]
    ADD [ListingParserInputHash] [char](64) NULL
END
GO

IF OBJECT_ID('dbo.NOT_LISTINGS_CACHE', 'U') IS NOT NULL
   AND COL_LENGTH('dbo.NOT_LISTINGS_CACHE', 'ResponseBodyTextHash') IS NOT NULL
   AND COL_LENGTH('dbo.NOT_LISTINGS_CACHE', 'ListingParserInputHash') IS NOT NULL
BEGIN
    THROW 51002, 'Both NOT_LISTINGS_CACHE.ResponseBodyTextHash and NOT_LISTINGS_CACHE.ListingParserInputHash exist. Resolve this manually before running the rename migration.', 1
END
GO

/* Existing statistics keys */
IF OBJECT_ID('dbo.GLOBAL_STATISTICS', 'U') IS NOT NULL
BEGIN
    UPDATE target
    SET [Counter] = target.[Counter] + source.[Counter]
    FROM [dbo].[GLOBAL_STATISTICS] target
    INNER JOIN (
        SELECT [Date], SUM([Counter]) AS [Counter]
        FROM [dbo].[GLOBAL_STATISTICS]
        WHERE [Key] = 'ResponseBodyTextAlreadyParsed'
        GROUP BY [Date]
    ) source ON source.[Date] = target.[Date]
    WHERE target.[Key] = 'ListingParserInputAlreadyParsed'

    DELETE old
    FROM [dbo].[GLOBAL_STATISTICS] old
    WHERE old.[Key] = 'ResponseBodyTextAlreadyParsed'
      AND EXISTS (
          SELECT 1
          FROM [dbo].[GLOBAL_STATISTICS] newer
          WHERE newer.[Date] = old.[Date]
            AND newer.[Key] = 'ListingParserInputAlreadyParsed'
      )

    UPDATE [dbo].[GLOBAL_STATISTICS]
    SET [Key] = 'ListingParserInputAlreadyParsed'
    WHERE [Key] = 'ResponseBodyTextAlreadyParsed'

    UPDATE target
    SET [Counter] = target.[Counter] + source.[Counter]
    FROM [dbo].[GLOBAL_STATISTICS] target
    INNER JOIN (
        SELECT [Date], SUM([Counter]) AS [Counter]
        FROM [dbo].[GLOBAL_STATISTICS]
        WHERE [Key] = 'ReponseBodyTextIsAnotherListingInHost'
        GROUP BY [Date]
    ) source ON source.[Date] = target.[Date]
    WHERE target.[Key] = 'ListingParserInputIsAnotherListingInHost'

    DELETE old
    FROM [dbo].[GLOBAL_STATISTICS] old
    WHERE old.[Key] = 'ReponseBodyTextIsAnotherListingInHost'
      AND EXISTS (
          SELECT 1
          FROM [dbo].[GLOBAL_STATISTICS] newer
          WHERE newer.[Date] = old.[Date]
            AND newer.[Key] = 'ListingParserInputIsAnotherListingInHost'
      )

    UPDATE [dbo].[GLOBAL_STATISTICS]
    SET [Key] = 'ListingParserInputIsAnotherListingInHost'
    WHERE [Key] = 'ReponseBodyTextIsAnotherListingInHost'
END
GO

IF OBJECT_ID('dbo.HOST_STATISTICS', 'U') IS NOT NULL
BEGIN
    UPDATE target
    SET [Counter] = target.[Counter] + source.[Counter]
    FROM [dbo].[HOST_STATISTICS] target
    INNER JOIN (
        SELECT [Date], [Host], SUM([Counter]) AS [Counter]
        FROM [dbo].[HOST_STATISTICS]
        WHERE [Key] = 'ResponseBodyTextAlreadyParsed'
        GROUP BY [Date], [Host]
    ) source ON source.[Date] = target.[Date] AND source.[Host] = target.[Host]
    WHERE target.[Key] = 'ListingParserInputAlreadyParsed'

    DELETE old
    FROM [dbo].[HOST_STATISTICS] old
    WHERE old.[Key] = 'ResponseBodyTextAlreadyParsed'
      AND EXISTS (
          SELECT 1
          FROM [dbo].[HOST_STATISTICS] newer
          WHERE newer.[Date] = old.[Date]
            AND newer.[Host] = old.[Host]
            AND newer.[Key] = 'ListingParserInputAlreadyParsed'
      )

    UPDATE [dbo].[HOST_STATISTICS]
    SET [Key] = 'ListingParserInputAlreadyParsed'
    WHERE [Key] = 'ResponseBodyTextAlreadyParsed'

    UPDATE target
    SET [Counter] = target.[Counter] + source.[Counter]
    FROM [dbo].[HOST_STATISTICS] target
    INNER JOIN (
        SELECT [Date], [Host], SUM([Counter]) AS [Counter]
        FROM [dbo].[HOST_STATISTICS]
        WHERE [Key] = 'ReponseBodyTextIsAnotherListingInHost'
        GROUP BY [Date], [Host]
    ) source ON source.[Date] = target.[Date] AND source.[Host] = target.[Host]
    WHERE target.[Key] = 'ListingParserInputIsAnotherListingInHost'

    DELETE old
    FROM [dbo].[HOST_STATISTICS] old
    WHERE old.[Key] = 'ReponseBodyTextIsAnotherListingInHost'
      AND EXISTS (
          SELECT 1
          FROM [dbo].[HOST_STATISTICS] newer
          WHERE newer.[Date] = old.[Date]
            AND newer.[Host] = old.[Host]
            AND newer.[Key] = 'ListingParserInputIsAnotherListingInHost'
      )

    UPDATE [dbo].[HOST_STATISTICS]
    SET [Key] = 'ListingParserInputIsAnotherListingInHost'
    WHERE [Key] = 'ReponseBodyTextIsAnotherListingInHost'
END
GO

IF OBJECT_ID('dbo.HOST_STATISTICS_SNAPSHOT', 'U') IS NOT NULL
BEGIN
    UPDATE target
    SET [Counter] = target.[Counter] + source.[Counter]
    FROM [dbo].[HOST_STATISTICS_SNAPSHOT] target
    INNER JOIN (
        SELECT [Date], [Host], SUM([Counter]) AS [Counter]
        FROM [dbo].[HOST_STATISTICS_SNAPSHOT]
        WHERE [Key] = 'ResponseBodyTextAlreadyParsed'
        GROUP BY [Date], [Host]
    ) source ON source.[Date] = target.[Date] AND source.[Host] = target.[Host]
    WHERE target.[Key] = 'ListingParserInputAlreadyParsed'

    DELETE old
    FROM [dbo].[HOST_STATISTICS_SNAPSHOT] old
    WHERE old.[Key] = 'ResponseBodyTextAlreadyParsed'
      AND EXISTS (
          SELECT 1
          FROM [dbo].[HOST_STATISTICS_SNAPSHOT] newer
          WHERE newer.[Date] = old.[Date]
            AND newer.[Host] = old.[Host]
            AND newer.[Key] = 'ListingParserInputAlreadyParsed'
      )

    UPDATE [dbo].[HOST_STATISTICS_SNAPSHOT]
    SET [Key] = 'ListingParserInputAlreadyParsed'
    WHERE [Key] = 'ResponseBodyTextAlreadyParsed'

    UPDATE target
    SET [Counter] = target.[Counter] + source.[Counter]
    FROM [dbo].[HOST_STATISTICS_SNAPSHOT] target
    INNER JOIN (
        SELECT [Date], [Host], SUM([Counter]) AS [Counter]
        FROM [dbo].[HOST_STATISTICS_SNAPSHOT]
        WHERE [Key] = 'ReponseBodyTextIsAnotherListingInHost'
        GROUP BY [Date], [Host]
    ) source ON source.[Date] = target.[Date] AND source.[Host] = target.[Host]
    WHERE target.[Key] = 'ListingParserInputIsAnotherListingInHost'

    DELETE old
    FROM [dbo].[HOST_STATISTICS_SNAPSHOT] old
    WHERE old.[Key] = 'ReponseBodyTextIsAnotherListingInHost'
      AND EXISTS (
          SELECT 1
          FROM [dbo].[HOST_STATISTICS_SNAPSHOT] newer
          WHERE newer.[Date] = old.[Date]
            AND newer.[Host] = old.[Host]
            AND newer.[Key] = 'ListingParserInputIsAnotherListingInHost'
      )

    UPDATE [dbo].[HOST_STATISTICS_SNAPSHOT]
    SET [Key] = 'ListingParserInputIsAnotherListingInHost'
    WHERE [Key] = 'ReponseBodyTextIsAnotherListingInHost'
END
GO

COMMIT TRANSACTION
GO
