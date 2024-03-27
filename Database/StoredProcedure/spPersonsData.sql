IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spPersonsData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spPersonsData]
GO
--spPersonsData 'DOB',1,10,1
CREATE PROCEDURE spPersonsData(
@PersonName nvarchar(max),@SortColumn nvarchar(255),@SortOrder int,@PageSize [int] = 10,@Start [int] = 1
) AS
BEGIN	

	WITH Filters as
	(	
			SELECT PersonId,Name,ISNULL(PersonId1,'') PersonId1,IsActive,DOB,
			CASE 
			WHEN @SortColumn='PersonId' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY PersonId) ELSE ROW_NUMBER() OVER (ORDER BY PersonId DESC) END
			WHEN @SortColumn='Name' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY Name) ELSE ROW_NUMBER() OVER (ORDER BY Name DESC) END
			WHEN @SortColumn='PersonId1' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY PersonId1) ELSE ROW_NUMBER() OVER (ORDER BY PersonId1 DESC) END
			WHEN @SortColumn='DOB' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY dateDOB) ELSE ROW_NUMBER() OVER (ORDER BY dateDOB DESC) END
			WHEN @SortColumn='IsActive' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY IsActive) ELSE ROW_NUMBER() OVER (ORDER BY IsActive DESC) END
			ELSE
				CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY Name) ELSE ROW_NUMBER() OVER (ORDER BY Name DESC)END
			END RowNumber
			FROM(
					SELECT PersonId,LastName + ' ' +FirstName Name,PersonId1,IsActive,CAST(FORMAT (DOB, 'MM/dd/yyyy') AS varchar(255)) DOB,DOB as dateDOB
					from Persons
					WHERE 
					FirstName LIKE '%'+ISNULL(@PersonName,FirstName)+'%' 
					OR LastName LIKE '%'+ISNULL(@PersonName,LastName)+'%'
					OR (FirstName + ' ' + LastName) LIKE '%'+ ISNULL(@PersonName,(FirstName + ' ' + LastName))  +'%'
					OR (LastName + ' ' + FirstName) LIKE '%'+ ISNULL(@PersonName,(LastName + ' ' + FirstName))  +'%'
			)x
	)	
	
		SELECT (SELECT COUNT(1) FROM Filters) TotalCount,* FROM Filters WHERE RowNumber BETWEEN @Start AND @PageSize order by RowNumber

END

