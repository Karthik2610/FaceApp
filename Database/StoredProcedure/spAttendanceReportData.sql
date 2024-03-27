IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spAttendanceReportData]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spAttendanceReportData]
GO
-- spAttendanceReportData 17,'10/20/2023','LastName',0,10,1

CREATE PROCEDURE spAttendanceReportData(
@LocationId int,@Date Datetime,
@SortColumn nvarchar(255),@SortOrder int,@PageSize [int] = 10,@Start [int] = 1
) AS
BEGIN	
create table #AttendanceList (PersonId int,AttendanceId int,LocationId int,ProgramId int,RawAttendanceCheckInDate datetime2(7),RawAttendanceCheckOutDate datetime2(7),AttendanceDate  datetime2(7),CheckInTime nvarchar(100), CheckOutTime nvarchar(100),Duration nvarchar(100),ModeofTransportation nvarchar(max))  
create table #FinalList (PersonId int,AttendanceId int ,LocationId int,ProgramId int,PersonID1 nvarchar(max),FirstName nvarchar(max),Lastname nvarchar(max),AttendanceDate  datetime2(7) ,ModeofTransportation nvarchar(max) ,CheckInTime nvarchar(100), CheckOutTime nvarchar(100),Duration nvarchar(100))  


;			
				with cte_test as
				(

				select PersonId,AttendanceId,LocationId,ProgramId,AttendanceDate,AttendanceType,ModeofTransportation,ROW_NUMBER() over (partition by PersonId,AttendanceType order by PersonId,AttendanceType)
				 as rn from Attendance A  WHERE convert(date,AttendanceDate,101) = ISNULL(convert(date,@Date,101),AttendanceDate) 
				 )

				 --select * from cte_test
				 insert into #AttendanceList
				select a.PersonId,a.AttendanceId,a.LocationId,a.ProgramId,a.AttendanceDate as RawAttendanceState,b.AttendanceDate ,convert(char(10), a.AttendanceDate, 111)as dated, convert(varchar,a.AttendanceDate, 108), convert(varchar,b.AttendanceDate, 108),  convert(varchar,DATEDIFF(second, a.AttendanceDate, b.AttendanceDate)/ 3600)  + ' Hrs ' + convert(varchar,((DATEDIFF(second, a.AttendanceDate, b.AttendanceDate) % 3600)/ 60)) + ' Mins ' --+ convert(varchar,DATEDIFF(second, a.AttendanceDate, b.AttendanceDate)%60) +' Seconds'
				AS TotalTime, a.ModeofTransportation

				 from cte_test a

				inner join cte_test b on a.PersonId=b.PersonId 
				--and a.name=b.name 
				and convert(char(10), a.AttendanceDate, 111)=convert(char(10), b.AttendanceDate, 111) and a.rn=b.rn

				and a.AttendanceType='CheckIn' and b.AttendanceType='CheckOut'
				group by a.PersonId, a.AttendanceDate,b.AttendanceDate,a.ModeofTransportation,a.AttendanceId,a.LocationId,a.ProgramId--convert(char(10), a.AttendanceDate, 111)

				insert into #FinalList
				select P.PersonId,A.AttendanceId,a.LocationId,a.ProgramId,P.PersonId1, P.FirstName,P.LastName, A.AttendanceDate,A.ModeofTransportation,A.CheckInTime,A.CheckOutTime,A.Duration from #AttendanceList A inner join Persons P on P.PersonId=A.PersonId  								
				WHERE convert(date,a.RawAttendanceCheckInDate,101) = ISNULL(convert(date,@Date,101),a.RawAttendanceCheckInDate) 
				union
				select   P.PersonId,A.AttendanceId,a.LocationId,a.ProgramId,P.PersonId1,P.FirstName,P.LastName,A.AttendanceDate,A.ModeofTransportation,convert(varchar,a.AttendanceDate, 108),null,null from Attendance a inner join Persons P on P.PersonId=a.PersonId 			
				where a.AttendanceDate not in ( select RawAttendanceCheckInDate from #AttendanceList) and a.AttendanceType='CheckIn'   and convert(date,a.AttendanceDate,101) = ISNULL(convert(date,@Date,101),a.AttendanceDate) 
				--union
				--select  P.PersonId,A.AttendanceId,a.LocationId,a.ProgramId,P.PersonId1, P.FirstName,P.LastName,A.AttendanceDate,A.ModeofTransportation,null,convert(varchar,a.AttendanceDate, 108),null from Attendance a inner join Persons P on P.PersonId=a.PersonId 				
				--where a.AttendanceDate not in ( select RawAttendanceCheckOutDate from #AttendanceList) and a.AttendanceType='CheckOut'  and convert(date,a.AttendanceDate,101) = ISNULL(convert(date,@Date,101),a.AttendanceDate) 

;
	WITH Filters as
	(	
			SELECT x.FirstName,x.LastName,x.LocationName,x.ProgramName,x.PersonID1,convert(nvarchar, convert(date,AttendanceDate,108)) as AttendanceDate,x.ModeofTransportation,x.CheckInTime,x.CheckOutTime,x.Duration,x.CalendarName,
			CASE 
			WHEN @SortColumn='FirstName' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY FirstName) ELSE ROW_NUMBER() OVER (ORDER BY FirstName DESC) END
			WHEN @SortColumn='LastName' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY LastName) ELSE ROW_NUMBER() OVER (ORDER BY LastName DESC) END
			WHEN @SortColumn='AttendanceDate' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY AttendanceDate) ELSE ROW_NUMBER() OVER (ORDER BY AttendanceDate DESC) END
			WHEN @SortColumn='ModeofTransportation' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY ModeofTransportation) ELSE ROW_NUMBER() OVER (ORDER BY ModeofTransportation DESC) END
			WHEN @SortColumn='LocationName' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY LocationName) ELSE ROW_NUMBER() OVER (ORDER BY LocationName DESC) END
			WHEN @SortColumn='ProgramName' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY ProgramName) ELSE ROW_NUMBER() OVER (ORDER BY ProgramName DESC) END
			WHEN @SortColumn='PersonID1' THEN  CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY PersonID1) ELSE ROW_NUMBER() OVER (ORDER BY PersonID1 DESC) END
			ELSE
				CASE @SortOrder WHEN 0 THEN ROW_NUMBER() OVER (ORDER BY FirstName) ELSE ROW_NUMBER() OVER (ORDER BY FirstName DESC)END
			END RowNumber
			
			FROM(			
				SELECT * FROM
				(
					select P.PersonId,P.PersonID1,P.AttendanceDate,P.Lastname,P.FirstName,P.AttendanceId,P.ModeofTransportation,P.CheckInTime,P.CheckOutTime,P.Duration ,L.LocationId,L.LocationName,PR.ProgramName  ,Pr.ProgramId,
					(select * from fnGetCalendarId(L.LocationId,PR.ProgramId,P.PersonId,P.AttendanceId)) CalendarName
					from #FinalList P				
					INNER JOIN Locations L ON P.LocationId=L.LocationId
					INNER JOIN Programs PR ON P.ProgramId=PR.ProgramId
				)a

			)x
			
	
	)	
	
		SELECT (SELECT COUNT(1) FROM Filters) TotalCount,* 
		FROM Filters P		
		WHERE RowNumber BETWEEN @Start AND @PageSize order by RowNumber

		drop table #AttendanceList
		drop table #FinalList
END
