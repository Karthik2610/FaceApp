IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spProgramsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spProgramsData]
GO
--spProgramsData 'UserName',0,10,1
CREATE PROCEDURE spProgramsData(
@ProgramName nvarchar(max),@SortColumn nvarchar(255),@SortOrder int,@PageSize [int] = 10,@Start [int] = 1
) AS
BEGIN	

	WITH Filters as
	(	
			SELECT ProgramId,ProgramName,IsActive,
			CASE 
			WHEN @SortColumn='ProgramId' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY ProgramId) ELSE ROW_NUMBER() OVER (ORDER BY ProgramId DESC) END
			WHEN @SortColumn='ProgramName' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY ProgramName) ELSE ROW_NUMBER() OVER (ORDER BY ProgramName DESC) END
			WHEN @SortColumn='IsActive' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY IsActive) ELSE ROW_NUMBER() OVER (ORDER BY IsActive DESC) END
			ELSE
				CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY ProgramName) ELSE ROW_NUMBER() OVER (ORDER BY ProgramName DESC)END
			END RowNumber
			FROM(
					SELECT ProgramId,ProgramName,IsActive
					from Programs
					WHERE ProgramName LIKE '%'+ISNULL(@ProgramName,ProgramName)+'%' 
			)x
	)	
	
		SELECT (SELECT COUNT(1) FROM Filters) TotalCount,* FROM Filters WHERE RowNumber BETWEEN @Start AND @PageSize order by RowNumber

END

