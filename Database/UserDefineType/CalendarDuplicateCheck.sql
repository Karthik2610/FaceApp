CREATE TYPE [CalendarDuplicateCheck] AS TABLE(
	RowId int NULL,
	CalendarId int NULL,
	StartDate datetime,
	EndDate datetime
)
