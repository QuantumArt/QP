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

ALTER TRIGGER [dbo].[ti_content_item_schedule_add_job] ON [dbo].[CONTENT_ITEM_SCHEDULE] FOR INSERT AS BEGIN
  DECLARE @current_db SYSNAME, @item_id NUMERIC, @qp_job_name SYSNAME, @sql NVARCHAR(1024)
  DECLARE @freq_type INT, @freq_interval INT, @freq_relative_interval INT, @freq_recurrence_factor INT
  DECLARE @active_start_date INT, @active_end_date INT, @active_start_time INT, @active_end_time INT
  DECLARE @use_duration INT, @deactivate BIT
  DECLARE @pre_sql NVARCHAR(1024)
  declare @str_set_params nvarchar(255)
  SELECT @current_db = DB_NAME()

  UPDATE CONTENT_ITEM_SCHEDULE
  set
    START_DATE = COALESCE(START_DATE, dbo.get_schedule_date(active_start_date, active_start_time)),
    END_DATE = COALESCE(END_DATE, dbo.get_schedule_date(active_end_date, active_end_time))
  where CONTENT_ITEM_ID in (select CONTENT_ITEM_ID from inserted)

  DECLARE items CURSOR FOR
    SELECT content_item_id, freq_type, freq_interval, freq_relative_interval, freq_recurrence_factor,
      active_start_date, active_end_date, active_start_time, active_end_time, use_duration, deactivate
    FROM inserted where use_service = 0
  OPEN items

  FETCH NEXT FROM items
  INTO @item_id, @freq_type, @freq_interval, @freq_relative_interval, @freq_recurrence_factor,
    @active_start_date, @active_end_date, @active_start_time, @active_end_time, @use_duration, @deactivate
  WHILE @@FETCH_STATUS = 0 BEGIN

    DECLARE @delete_level INT

    IF @freq_type = 1 OR @freq_type = 2 BEGIN
      DECLARE @now_date DATETIME
      DECLARE @now_date_int BIGINT, @start_date_int BIGINT, @end_date_int BIGINT

      SET @now_date = DATEADD(mi, 1, GETDATE())
      SET @now_date_int =  DATEPART(ss, @now_date) + (100 * DATEPART(mi, @now_date)) + (10000 * DATEPART(hh, @now_date)) + (1000000 * DAY(@now_date)) + (100000000 * MONTH(@now_date)) + (10000000000 * YEAR(@now_date))
      SET @start_date_int = CAST(@active_start_time AS BIGINT) + CAST(@active_start_date AS BIGINT) * 1000000
      SET @end_date_int   = CAST(@active_end_time AS BIGINT) + CAST(@active_end_date AS BIGINT) * 1000000

      IF @now_date_int > @start_date_int BEGIN
        SET @active_start_date = @now_date_int / 1000000
        SET @active_start_time = @now_date_int % 1000000

        UPDATE content_item_schedule
        SET active_start_date = @active_start_date, active_start_time = @active_start_time
        WHERE CONTENT_ITEM_ID = @item_id
      END

      SET @now_date = DATEADD(ss, 10, @now_date)
      SET @now_date_int =  DATEPART(ss, @now_date) + (100 * DATEPART(mi, @now_date)) + (10000 * DATEPART(hh, @now_date)) + (1000000 * DAY(@now_date)) + (100000000 * MONTH(@now_date)) + (10000000000 * YEAR(@now_date))

      IF @now_date_int > @end_date_int BEGIN
        SET @active_end_date = @now_date_int / 1000000
        SET @active_end_time = @now_date_int % 1000000

        UPDATE content_item_schedule
        SET active_end_date = @active_end_date, active_end_time = @active_end_time
        WHERE CONTENT_ITEM_ID = @item_id
      END

      SET @delete_level  = 1

    END ELSE BEGIN
      SET @delete_level  = 0
    END

    SET @qp_job_name = 'Q-Publishing Schedule for ' + @current_db + ' item '
      + CAST(@item_id AS NVARCHAR) + ' on'
    IF EXISTS(SELECT * FROM msdb.dbo.sysjobs_view WHERE name = @qp_job_name) BEGIN
      EXEC msdb.dbo.sp_delete_job @job_name = @qp_job_name
    END

	IF @deactivate = 0 BEGIN	--if schedule is deactivated then don't create job
		if dbo.qp_is_sql_2000() = 1
			set @str_set_params =  '@activation_start_dt=[STRTDT], @activation_start_tm=[STRTTM]'
		else if dbo.qp_is_early_sql_2005() = 1
			set @str_set_params =  '@activation_start_dt=$(STRTDT), @activation_start_tm=$(STRTTM)'
		else
			set @str_set_params =  '@activation_start_dt=$(ESCAPE_NONE(STRTDT)), @activation_start_tm=$(ESCAPE_NONE(STRTTM))'
		if @freq_type <> 2
			SET @sql = 'UPDATE content_item with(rowlock) SET visible = 1 WHERE content_item_id = '
			  + CAST(@item_id AS NVARCHAR)
			  + '
				EXECUTE qp_create_deactivation_job @item_id=' + CAST(@item_id AS NVARCHAR) + ', ' + @str_set_params
		else begin	--scheduleNewVersionPublication
			set @sql  = 'exec qp_merge_article ' + CAST(@item_id AS NVARCHAR)
			set @freq_type = 1
		end

		SET @pre_sql = 'Q-Publishing Schedule for ' + @current_db + ' item ' + CAST(@item_id AS NVARCHAR) + ' off'
		SET @pre_sql = 'IF EXISTS(SELECT * FROM msdb.dbo.sysjobs_view WHERE name = ''' + @pre_sql + ''') EXEC msdb.dbo.sp_delete_job @job_name = ''' + @pre_sql + ''' '

		EXEC msdb.dbo.sp_add_job @job_name = @qp_job_name, @delete_level  =  @delete_level
		EXEC msdb.dbo.sp_add_jobstep @job_name = @qp_job_name, @step_name = 'Remove old deactivation job',
		  @command = @pre_sql, @database_name = @current_db,
		  @retry_attempts = 1,
		  @on_success_action  = 3, @on_fail_action = 3
		EXEC msdb.dbo.sp_add_jobstep @job_name = @qp_job_name, @step_name = 'Activate article',
		  @command = @sql, @database_name = @current_db,
		  @retry_attempts = 1
		EXEC msdb.dbo.sp_add_jobschedule @job_name = @qp_job_name, @name = 'Activate Schedule',
		  @enabled = 1, @freq_type = @freq_type, @freq_interval = @freq_interval,
		  @freq_relative_interval = @freq_relative_interval,
		  @freq_recurrence_factor = @freq_recurrence_factor,
		  @freq_subday_type = 0x1, @freq_subday_interval = 0,
		  @active_start_date = @active_start_date, @active_end_date = @active_end_date,
		  @active_start_time = @active_start_time, @active_end_time = @active_end_time
		EXEC msdb.dbo.sp_add_jobserver @job_name = @qp_job_name, @server_name = '(LOCAL)'
	END

    FETCH NEXT FROM items
    INTO @item_id, @freq_type, @freq_interval, @freq_relative_interval, @freq_recurrence_factor,
      @active_start_date, @active_end_date, @active_start_time, @active_end_time, @use_duration, @deactivate
  END
  CLOSE items
  DEALLOCATE items
END
go

