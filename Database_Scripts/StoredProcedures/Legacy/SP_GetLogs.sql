CREATE OR ALTER PROCEDURE [dbo].[SP_GetLogs]
    @PageNumber INT = 1,
    @PageSize INT = 50,
    @Level NVARCHAR(50) = NULL,
    @SearchTerm NVARCHAR(500) = NULL,
    @StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @TotalCount INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    -- Ensure valid page number and size
    IF @PageNumber < 1 SET @PageNumber = 1;
    IF @PageSize < 1 SET @PageSize = 50;
    IF @PageSize > 100 SET @PageSize = 100;

    DECLARE @Offset INT = (@PageNumber - 1) * @PageSize;

    -- Get total count
    SELECT @TotalCount = COUNT(*)
    FROM Logs
    WHERE 
        (@Level IS NULL OR Level = @Level)
        AND (@SearchTerm IS NULL OR Message LIKE '%' + @SearchTerm + '%')
        AND (@StartDate IS NULL OR TimeStamp >= @StartDate)
        AND (@EndDate IS NULL OR TimeStamp <= @EndDate);

    -- Get paginated logs
    SELECT 
        Id,
        TimeStamp,
        ISNULL(Level, 'Information') as Level,
        ISNULL(Message, 'No message') as Message,
        ISNULL(Exception, '') as Exception,
        ISNULL(Properties, '') as Properties,
        ISNULL(UserId, '') as UserId,
        ISNULL(RequestId, '') as RequestId,
        ISNULL(ActionName, '') as ActionName
    FROM Logs
    WHERE 
        (@Level IS NULL OR Level = @Level)
        AND (@SearchTerm IS NULL OR Message LIKE '%' + @SearchTerm + '%')
        AND (@StartDate IS NULL OR TimeStamp >= @StartDate)
        AND (@EndDate IS NULL OR TimeStamp <= @EndDate)
    ORDER BY TimeStamp DESC
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END