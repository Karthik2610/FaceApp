IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spCalendarsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spCalendarsData]
GO
--spCalendarsData 'UserName',0,10,1
CREATE PROCEDURE spCalendarsData(
@CalendarName nvarchar(max),@SortColumn nvarchar(255),@SortOrder int,@PageSize [int] = 10,@Start [int] = 1
) AS
BEGIN	

	WITH Filters as
	(	
			SELECT CalendarId,CalendarName,IsActive,
			CASE 
			WHEN @SortColumn='CalendarId' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY CalendarId) ELSE ROW_NUMBER() OVER (ORDER BY CalendarId DESC) END
			WHEN @SortColumn='CalendarName' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY CalendarName) ELSE ROW_NUMBER() OVER (ORDER BY CalendarName DESC) END
			WHEN @SortColumn='IsActive' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY IsActive) ELSE ROW_NUMBER() OVER (ORDER BY IsActive DESC) END
			ELSE
				CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY CalendarName) ELSE ROW_NUMBER() OVER (ORDER BY CalendarName DESC)END
			END RowNumber
			FROM(
					SELECT CalendarId,CalendarName,IsActive
					from CalendarSetting
					WHERE CalendarName LIKE '%'+ISNULL(@CalendarName,CalendarName)+'%' 
			)x
	)	
	
		SELECT (SELECT COUNT(1) FROM Filters) TotalCount,* FROM Filters WHERE RowNumber BETWEEN @Start AND @PageSize order by RowNumber

END

