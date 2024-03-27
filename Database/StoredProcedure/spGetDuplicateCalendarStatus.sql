IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[spGetDuplicateCalendarStatus]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[spGetDuplicateCalendarStatus]
GO
-- spGetDuplicateCalendarStatus 17,'05-25-2023','05-28-2023',17,'05-25-2023','05-25-2023'
CREATE PROCEDURE spGetDuplicateCalendarStatus(
@CalendarDuplicateCheck CalendarDuplicateCheck readonly,@CalendarId Int,@StartDate datetime,@EndDate datetime
) AS
BEGIN	

DECLARE @tblCSR CalendarDuplicateCheck
INSERT INTO @tblCSR
SELECT RowId,CalendarId,StartDate,EndDate FROM 
(SELECT RowId,CalendarId,StartDate,EndDate
FROM @CalendarDuplicateCheck) rslt

Create Table #Calendar1 (Dates date,Day_Name nvarchar(50),Start_time time,End_time time);
Create Table #Calendar2 (Dates date,Day_Name nvarchar(50),Start_time time,End_time time);

INSERT INTO #Calendar1 
select * from dbo.fnGetCalendarTimeDetails(@StartDate,@EndDate,@CalendarId)

-- New changes
DECLARE @RowId Int,@RowCalendarId int, @RowStartDate nvarchar(20),@RowEndDate nvarchar(20);  
  
DECLARE contact_cursor CURSOR FOR  
SELECT	RowId,CalendarId, StartDate,  EndDate FROM @tblCSR
ORDER BY RowId;  
  
OPEN contact_cursor;  
    
FETCH NEXT FROM contact_cursor  
INTO @RowId,@RowCalendarId, @RowStartDate,@RowEndDate;
  
-- Check @@FETCH_STATUS to see if there are any more rows to fetch.  
WHILE @@FETCH_STATUS = 0  
BEGIN  

		INSERT INTO #Calendar2 
		select * from dbo.fnGetCalendarTimeDetails(@RowStartDate,@RowEndDate,@RowCalendarId)

   -- This is executed as long as the previous fetch succeeds.  
   FETCH NEXT FROM contact_cursor  
   INTO @RowId,@RowCalendarId, @RowStartDate,  @RowEndDate;  
END  
  
CLOSE contact_cursor;  
DEALLOCATE contact_cursor;  

SELECT * FROM #Calendar1 c1
INNER JOIN #Calendar2 c2 ON C1.Day_Name = c2.Day_Name
WHERE (c1.Start_time BETWEEN c2.Start_time AND c2.End_time) OR (c2.Start_time BETWEEN c1.Start_time AND c1.End_time)
DROP TABLE #Calendar1
DROP TABLE #Calendar2

END