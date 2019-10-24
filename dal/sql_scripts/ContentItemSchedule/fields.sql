IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT_ITEM_SCHEDULE]') AND name = 'START_DATE')
  ALTER TABLE [dbo].[CONTENT_ITEM_SCHEDULE]
  ADD START_DATE DATETIME NULL
GO

IF NOT EXISTS (SELECT * FROM sys.columns WHERE  object_id = OBJECT_ID(N'[dbo].[CONTENT_ITEM_SCHEDULE]') AND name = 'END_DATE')
  ALTER TABLE [dbo].[CONTENT_ITEM_SCHEDULE]
  ADD END_DATE DATETIME NULL
GO

exec qp_drop_existing 'get_schedule_date', 'IsScalarFunction'
GO

CREATE FUNCTION dbo.get_schedule_date(@dt int, @tm int) returns datetime
AS BEGIN
    declare @result datetime;
    declare @year int, @month int, @day int;
    declare @hour int, @minute int, @second int;

    set @year = @dt / 100;
    set @day = @dt % 100;
    set @month = @year % 100;
    set @year = @year / 100;

    set @hour = @tm / 100;
    set @second = @tm % 100;
    set @minute = @hour % 100;
    set @hour = @hour / 100;

    set @result = DATETIMEFROMPARTS(@year, @month, @day, @hour, @minute, @second, 0);

    return @result;
end
GO

update CONTENT_ITEM_SCHEDULE
SET
    START_DATE = dbo.get_schedule_date(isnull(active_start_date, 0), active_start_time),
    END_DATE = dbo.get_schedule_date(active_end_date, active_end_time)

GO

