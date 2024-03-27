-- DROP FUNCTIOn fnGetCalendarId
-- select * from fnGetCalendarId (17,17,2020,9243)
CREATE Function fnGetCalendarId(@LocationId int,@ProgramId int,@PersonId int,@AttendanceId int) 
returns @tbl TABLE(CalendarName nvarchar(max))
BEGIN

	INSERT @tbl 

	SELECT TOP 1 Z.CalendarName FROM Attendance A
	LEFT JOIN(
			SELECT * FROM
			(
				SELECT pd.PersonId,pd.LocationId,pd.ProgramId,C.CalendarName, CASE 
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Sunday' THEN CAST(SundayStartTime as time) 
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Monday' THEN CAST(MondayStartTime as time) 
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Tuesday' THEN CAST(TuesdayStartTime as time) 
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Wednesday' THEN CAST(WednesdayStartTime as time)  
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Thursday' THEN CAST(ThursdayStartTime as time)  
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Friday' THEN CAST(FridayStartTime as time)  
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Saturday' THEN CAST(SaturdayStartTime as time) 
				END StartTime,
				CASE 
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Sunday' THEN CAST(SundayEndTime as time)  
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Monday' THEN CAST(MondayEndTime as time) 
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Tuesday' THEN CAST(TuesdayEndTime as time) 
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Wednesday' THEN CAST(WednesdayEndTime as time) 
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Thursday' THEN CAST(ThursdayEndTime as time)  
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Friday' THEN CAST(FridayEndTime as time)  
					WHEN FORMAT(CAST(a.AttendanceDate AS DATE), 'ddddddddd') = 'Saturday' THEN CAST(SaturdayEndTime as time) 
				END EndTime
				FROM PersonsDetails pd
				INNER JOIN Attendance a ON a.LocationId=pd.LocationId AND a.ProgramId=pd.ProgramId ANd a.PersonId=pd.PersonId
				INNER JOIN CalendarSetting C ON C.CalendarId=pd.CalendarId
				WHERE a.AttendanceId =@AttendanceId AND pd.PersonId=@PersonId 
				AND pd.LocationId =ISNULL(@LocationId,pd.LocationId)
				AND pd.ProgramId =ISNULL(@ProgramId,pd.ProgramId)
				AND CONVERT(DATETIME,CONVERT(varchar,a.AttendanceDate,101)) BETWEEN CONVERT(DATETIME,CONVERT(varchar,pd.StartDate,101)) AND CONVERT(DATETIME,CONVERT(varchar,pd.EndDate,101))
			)x
			WHERE StartTime IS NOT NULL AND EndTime IS NOT NULL
	)z  ON Z.LocationId =A.LocationId
	AND Z.ProgramId =A.ProgramId
	AND Z.PersonId =A.PersonId
	WHERE A.AttendanceId= @AttendanceId AND CAST(A.AttendanceDate as time)  BETWEEN Z.StartTime AND Z.EndTime
return
END
