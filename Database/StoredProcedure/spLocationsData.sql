IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spLocationsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spLocationsData]
GO
--spLocationsData 'UserName',0,10,1
CREATE PROCEDURE spLocationsData(@LocationName nvarchar(max),
@SortColumn nvarchar(255),@SortOrder int,@PageSize [int] = 10,@Start [int] = 1
) AS
BEGIN	

	WITH Filters as
	(	
			SELECT LocationId,LocationName,IsActive,
			CASE 
			WHEN @SortColumn='LocationId' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY LocationId) ELSE ROW_NUMBER() OVER (ORDER BY LocationId DESC) END
			WHEN @SortColumn='LocationName' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY LocationName) ELSE ROW_NUMBER() OVER (ORDER BY LocationName DESC) END
			WHEN @SortColumn='IsActive' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY IsActive) ELSE ROW_NUMBER() OVER (ORDER BY IsActive DESC) END
			ELSE
				CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY LocationName) ELSE ROW_NUMBER() OVER (ORDER BY LocationName DESC)END
			END RowNumber
			FROM(
					SELECT LocationId,LocationName,IsActive
					from Locations
					WHERE 
					LocationName LIKE '%'+ISNULL(@LocationName,LocationName)+'%' 
			)x
	)	
	
		SELECT (SELECT COUNT(1) FROM Filters) TotalCount,* FROM Filters WHERE RowNumber BETWEEN @Start AND @PageSize order by RowNumber

END

