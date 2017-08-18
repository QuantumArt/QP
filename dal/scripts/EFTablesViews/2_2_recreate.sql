if not exists(
	select * From INFORMATION_SCHEMA.VIEWS where table_name like 'content%' and TABLE_NAME like '%new'
) or object_id('tempdb..#qp_rebuild_all_new_views') is not null
begin
	exec qp_rebuild_all_new_views
end
GO

if not exists(
	select * From INFORMATION_SCHEMA.VIEWS where table_name like 'item_link%' and TABLE_NAME like '%rev'
) or object_id('tempdb..#qp_rebuild_all_link_views') is not null
begin
	exec qp_rebuild_all_link_views
end
GO

if not exists(
	select * From INFORMATION_SCHEMA.TABLES where table_name like 'item_link%' and TABLE_NAME like '%rev' and TABLE_TYPE = 'BASE TABLE'
) or object_id('tempdb..#qp_recreate_link_tables') is not null
begin
	exec qp_recreate_link_tables
end
GO
