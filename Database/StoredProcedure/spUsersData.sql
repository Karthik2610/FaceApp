IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spUsersData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spUsersData]
GO
--spUsersData 'UserName',0,10,1
CREATE PROCEDURE spUsersData(
@UserName nvarchar(max),@Email nvarchar(max),@SortColumn nvarchar(255),@SortOrder int,@PageSize [int] = 10,@Start [int] = 1
) AS
BEGIN	

	WITH Filters as
	(	
			SELECT UserId,UserName,ISNULL(UserEmpId,'') UserEmpId,ISNULL(SecurityGroup,'') SecurityGroup,Email,IsAdmin,
			CASE 
			WHEN @SortColumn='UserId' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY UserId) ELSE ROW_NUMBER() OVER (ORDER BY UserId DESC) END
			WHEN @SortColumn='UserName' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY UserName) ELSE ROW_NUMBER() OVER (ORDER BY UserName DESC) END
			WHEN @SortColumn='UserEmpId' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY UserEmpId) ELSE ROW_NUMBER() OVER (ORDER BY UserEmpId DESC) END
			WHEN @SortColumn='SecurityGroup' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY SecurityGroup) ELSE ROW_NUMBER() OVER (ORDER BY SecurityGroup DESC) END
			WHEN @SortColumn='Email' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY Email) ELSE ROW_NUMBER() OVER (ORDER BY Email DESC) END
			WHEN @SortColumn='IsAdmin' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY IsAdmin) ELSE ROW_NUMBER() OVER (ORDER BY IsAdmin DESC) END
			ELSE
				CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY UserName) ELSE ROW_NUMBER() OVER (ORDER BY UserName DESC)END
			END RowNumber
			FROM(
					SELECT UserId,LastName + ' ' +FirstName UserName,UserEmpId,SecurityGroup,Email,IsAdmin
					FROM Users
					WHERE (FirstName LIKE '%'+ISNULL(@UserName,FirstName)+'%' 
					OR LastName LIKE '%'+ISNULL(@UserName,LastName)+'%'
					OR (FirstName + ' ' + LastName) LIKE '%'+ ISNULL(@UserName,(FirstName + ' ' + LastName))  +'%'
					OR (LastName + ' ' + FirstName) LIKE '%'+ ISNULL(@UserName,(LastName + ' ' + FirstName))  +'%')
					AND Email LIKE '%'+ISNULL(@Email,Email)+'%' 
			)x
	)	
	
		SELECT (SELECT COUNT(1) FROM Filters) TotalCount,* FROM Filters WHERE RowNumber BETWEEN @Start AND @PageSize order by RowNumber

END

