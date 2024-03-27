IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spDailyReportData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spDailyReportData]
GO
-- spDailyReportData 17,9,'2023/10/25','Name',0,10,1
CREATE PROCEDURE spDailyReportData(@ProgramId int,@LocationId int,@Date Datetime,
@SortColumn nvarchar(255),@SortOrder int,@PageSize [int] = 10,@Start [int] = 1
) AS
BEGIN	

	WITH Filters as
	(	
			SELECT x.Name,CAST(max(x.StatusIn) AS BIT) StatusIn,CAST(max(x.StatusOut) AS BIT) StatusOut,
			CASE WHEN @SortColumn='Name' THEN 
					CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY x.Name) ELSE ROW_NUMBER() OVER (ORDER BY x.Name DESC) END
			 WHEN @SortColumn='ProgramName' THEN 
			 CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY x.ProgramName) ELSE ROW_NUMBER() OVER (ORDER BY x.ProgramName DESC)END
			  WHEN @SortColumn='LocationName' THEN 
			 CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY x.LocationName) ELSE ROW_NUMBER() OVER (ORDER BY x.LocationName DESC)END
			 WHEN @SortColumn='PersonId1' THEN 
			 CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY MAX(PersonId1)) ELSE ROW_NUMBER() OVER (ORDER BY MAX(PersonId1) DESC)END
				ELSE
					CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY x.Name) ELSE ROW_NUMBER() OVER (ORDER BY x.Name DESC)
				END
			END RowNumber,PersonId1,LocationName,ProgramName
			FROM(
				SELECT p.LastName +' '+ p.FirstName Name,
				CASE WHEN a.AttendanceType = 'CheckIn' OR a.AttendanceType = 'CheckOut' THEN 1 ELSE 0 END StatusIn,
				CASE WHEN a.AttendanceType = 'CheckOut' THEN 1 ELSE 0 END StatusOut,				
				pg.ProgramId,l.LocationId,pd.StartDate,pd.EndDate,P.PersonId1,l.LocationName,pg.ProgramName,
				ROW_NUMBER() OVER(PARTITION BY p.PersonId,CONVERT(DATETIME,CONVERT(Varchar,a.AttendanceDate,101)),l.LocationId,pg.ProgramId ORDER BY p.PersonId,a.AttendanceDate,l.LocationId,pg.ProgramId,a.AttendanceType Desc) RowNo
				FROM Persons p
				LEFT JOIN PersonsDetails pd on pd.PersonId = p.PersonId
				LEFT JOIN Programs pg on pg.ProgramId = pd.ProgramId
				LEFT JOIN Locations l on l.LocationId = pd.LocationId
				LEFT JOIN (
					SELECT * FROM
					(
						SELECT ROW_NUMBER() OVER(PARTITION BY CONVERT(DATETIME,CONVERT(Varchar,AttendanceDate,101)),LocationId,ProgramId,PersonId 
						ORDER BY AttendanceId Desc) RowNo,*
						from Attendance  
						WHERE convert(date,AttendanceDate,101) = convert(date,@Date,101)
						and ProgramId=@ProgramId AND LocationId=@LocationId 
					)x
					WHERE x.RowNo=1
				) a on p.PersonId =a.PersonId 
				AND  pg.ProgramId = a.ProgramId AND a.LocationId = l.LocationId  AND convert(date,a.AttendanceDate,101) = ISNULL(convert(date,@Date,101),GETDATE())


			)x
			WHERE x.ProgramId=ISNULL(@ProgramId,x.ProgramId)
			AND x.LocationId=ISNULL(@LocationId,x.LocationId)
			AND  ISNULL(convert(date,@Date,101),GETDATE()) BETWEEN 
			convert(date,x.StartDate,101) AND convert(date,x.EndDate,101) 
			GROUP BY Name,PersonId1,LocationName,ProgramName
	)	
	
		SELECT (SELECT COUNT(1) FROM Filters) TotalCount,
		(SELECT SUM(CAST(StatusOut as INT)) FROM Filters) CheckOutCount ,(SELECT SUM(CAST(StatusIn as INT)) FROM Filters) CheckInCount ,* ,	CASE WHEN StatusIn = 1 THEN 'Yes' ELSE 'No' END StatusInExcel,CASE WHEN StatusOut = 1 THEN 'Yes' ELSE 'No' END StatusOutExcel	
		FROM Filters WHERE RowNumber BETWEEN @Start AND @PageSize order by RowNumber

END
