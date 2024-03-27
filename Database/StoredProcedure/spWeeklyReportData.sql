IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spWeeklyReportData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spWeeklyReportData]
GO
-- spWeeklyReportData 14,17,'10/19/2023','10/21/2023','',1,10,1
CREATE PROCEDURE spWeeklyReportData(@ProgramId int,@LocationId int,@FromDate Datetime,@ToDate Datetime,
@SortColumn nvarchar(255),@SortOrder int,@PageSize [int] = 10,@Start [int] = 1
) AS
BEGIN	

	WITH Filters as
	(		
			SELECT  Name,PersonId1,AttendanceDate,ProgramName,
			CONCAT(( ((SUM(ISNULL(Hrs,0)) * 60) + SUM(ISNULL(Mins,0))) /60), ' Hrs ',
			( ((SUM(ISNULL(Hrs,0))*60)+SUM(ISNULL(Mins,0))) %60), ' Mins')
			as 'Duration',
			 CASE WHEN @SortColumn='Name' THEN 
					 CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY Name) ELSE ROW_NUMBER() OVER (ORDER BY Name DESC)END
					 WHEN @SortColumn='ProgramName' THEN 
					 CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY ProgramName) ELSE ROW_NUMBER() OVER (ORDER BY ProgramName DESC)END
					 -- WHEN @SortColumn='LocationName' THEN 
					 --CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY LocationName) ELSE ROW_NUMBER() OVER (ORDER BY LocationName DESC)END
					  WHEN @SortColumn='Date' THEN 
					 CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY MAX(AttendanceDate)) ELSE ROW_NUMBER() OVER (ORDER BY MIN(AttendanceDate) DESC)END
					 --WHEN @SortColumn='Duration' THEN 
					 --CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY MAX(Duration)) ELSE ROW_NUMBER() OVER (ORDER BY MAX(Duration) DESC)END
					 WHEN @SortColumn='PersonId1' THEN 
					 CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY MAX(PersonId1)) ELSE ROW_NUMBER() OVER (ORDER BY MAX(PersonId1) DESC)END
					 ELSE
							CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY Name) ELSE ROW_NUMBER() OVER (ORDER BY Name DESC)
					 END
					END RowNumber
			FROM(
					SELECT x.Name,x.PersonId1, /*CheckIn AttendanceDate,Duration*/ CheckIn as AttendanceDate,ProgramName,
					  Hrs,Mins
					  --SUM(Hrs) Duration,
					 --ROW_NUMBER() OVER (ORDER BY x.Name) RowNumber
					
					FROM(
						SELECT pr.LastName + ' ' + pr.FirstName Name,pg.ProgramId,pg.ProgramName,l.LocationId,l.LocationName, 
						CAST(FORMAT (taskStart.[AttendanceDate], 'MM/dd/yyyy') AS varchar(255)) AS 'CheckIn',
						taskEnd.[AttendanceDate] AS 'CheckOut',
						Pd.StartDate,pd.EndDate,
						(DATEDIFF(MINUTE, taskStart.[AttendanceDate],  ISNULL(taskEnd.[AttendanceDate],GETDATE()))/60) Hrs,
							(DATEDIFF(MINUTE, taskStart.[AttendanceDate],  ISNULL(taskEnd.[AttendanceDate],GETDATE()))%60) Mins,
						--CONCAT((DATEDIFF(MINUTE, taskStart.[AttendanceDate],  ISNULL(taskEnd.[AttendanceDate],GETDATE()))/60), ' Hrs ',
						--(DATEDIFF(MINUTE, taskStart.[AttendanceDate],  ISNULL(taskEnd.[AttendanceDate],GETDATE()))%60), ' Mins')
						--as 'Duration'
						pr.PersonId1
						FROM Persons pr 
						INNER JOIN PersonsDetails Pd On Pd.PersonId =pr.PersonId
						LEFT JOIN Programs pg on pg.ProgramId = Pd.ProgramId
						LEFT JOIN Locations l on l.LocationId = Pd.LocationId
						LEFT JOIN Attendance taskStart on Pd.PersonId = taskStart.PersonId AND taskStart.LocationId = l.LocationId AND taskStart.ProgramId =pg.ProgramId 
						OUTER APPLY (
							SELECT TOP(1)
								ISNULL(taskEnd.[AttendanceDate],GETDATE()) AttendanceDate
							FROM Attendance taskEnd
							WHERE 
								taskEnd.[AttendanceDate] > taskStart.[AttendanceDate]
								AND taskEnd.[AttendanceType] = 'CheckOut'
								AND taskEnd.PersonId = pr.PersonId 
								AND taskEnd.ProgramId = pg.ProgramId
								AND taskEnd.LocationId = l.LocationId
								ORDER BY [AttendanceDate]
							) taskEnd
						WHERE taskStart.AttendanceType = 'CheckIn'

					)x
					WHERE x.ProgramId=ISNULL(@ProgramId,x.ProgramId)
					AND x.LocationId=ISNULL(@LocationId,x.LocationId)
					AND (ISNULL(convert(date,x.CheckIn,101),GETDATE()) >= ISNULL(convert(date,@FromDate,101),x.CheckIn) 
					AND ISNULL(convert(date,x.CheckOut,101),GETDATE()) <= ISNULL(convert(date,@ToDate,101),x.CheckOut))
					GROUP BY Name,ProgramName,LocationName,CheckIn,PersonId1, Hrs,Mins
			)z
			GROUP BY Name,PersonId1,AttendanceDate,ProgramName
	)	
	
			SELECT (SELECT COUNT(1) FROM Filters) TotalCount,* FROM Filters WHERE RowNumber BETWEEN @Start AND @PageSize order by RowNumber

END
