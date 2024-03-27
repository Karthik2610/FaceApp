IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spAttendanceDetailsReportData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spAttendanceDetailsReportData]
GO
-- spAttendanceDetailsReportData 17,'2023-10-20','',0,10,1
CREATE PROCEDURE spAttendanceDetailsReportData(
	@LocationId int,@Date Datetime,@SortColumn nvarchar(255),@SortOrder int,@PageSize [int] = 10,@Start [int] = 1
) AS
BEGIN	


		WITH Filters as
		(	
			SELECT Distinct PersonId,PersonName,PersonId1,
			 [1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[24],[25],
			CASE 
			WHEN @SortColumn='PersonName' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY PersonName) ELSE ROW_NUMBER() OVER (ORDER BY PersonName DESC) END
			ELSE
				CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY PersonName) ELSE ROW_NUMBER() OVER (ORDER BY PersonName DESC)END
			END RowNumber
			FROM  
			(
			  SELECT distinct P.PersonId,L.LocationName,P.LastName +','+P.FirstName PersonName, Rank() OVER (ORDER BY PR.ProgramName) RankNo, 
			  CASE ISNULL(A.AttendanceType,'') WHEN 'CheckIn' THEN 1 WHEN 'CheckOut' THEN 2 ELSE 3 END Attendance,P.PersonId1
			FROM Persons P 
			INNER JOIN PersonsDetails PD ON P.PersonId=PD.PersonId
			INNER JOIN Locations L ON PD.LocationId=L.LocationId
			INNER JOIN Programs PR ON PD.ProgramId=PR.ProgramId
			INNER JOIN CalendarSetting C ON PD.CalendarId=C.CalendarId
			LEFT OUTER JOIN (
					SELECT PersonId,AttendanceDate,AttendanceType,LocationId,ProgramId FROM  
					(
						SELECT PersonId,CONVERT(Varchar,AttendanceDate,101) AttendanceDate,AttendanceType,LocationId,ProgramId, 
						ROW_NUMBER() OVER(PARTITION BY PersonId,CONVERT(DATETIME,CONVERT(Varchar,AttendanceDate,101)),LocationId,ProgramId ORDER BY PersonId,AttendanceDate Desc) RowNo
						from Attendance
					) Rslt WHERE RowNo=1) A ON PD.PersonId=A.PersonId AND PD.LocationId=A.LocationId AND PD.ProgramId=A.ProgramId 
				AND CONVERT(datetime,CONVERT(varchar,A.AttendanceDate,101))=@Date 
			WHERE L.LocationId=@LocationId AND CONVERT(datetime,@Date) BETWEEN PD.StartDate AND PD.EndDate

			) AS SourceTable  
			PIVOT  
			(  
			  sum(Attendance)  
			  FOR RankNo IN ([1],[2],[3],[4],[5],[6],[7],[8],[9],[10],[11],[12],[13],[14],[15],[16],[17],[18],[19],[20],[21],[22],[23],[24],[25]) 
			) AS PivotTable
		
		)

		SELECT (SELECT COUNT(1) FROM Filters) TotalCount,* 
		FROM Filters WHERE RowNumber BETWEEN @Start AND @PageSize order by RowNumber

END
