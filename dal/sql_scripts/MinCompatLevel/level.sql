declare @level int
declare @sql nvarchar(max)
select @level = compatibility_level from sys.databases where name = db_name()
if @level < 110
begin
    print 'Changing compatibility level for ' + db_name() + ' from ' + cast(@level as nvarchar(3)) + ' to 110'
    set @sql = N'ALTER DATABASE ' + db_name() + ' SET compatibility_level = 110 '
    exec sp_executesql @sql
end
GO
