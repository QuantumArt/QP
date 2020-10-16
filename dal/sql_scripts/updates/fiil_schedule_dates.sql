update CONTENT_ITEM_SCHEDULE
SET
    START_DATE = dbo.get_schedule_date(isnull(active_start_date, 17530101), active_start_time),
    END_DATE = dbo.get_schedule_date(active_end_date, active_end_time)

GO
