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