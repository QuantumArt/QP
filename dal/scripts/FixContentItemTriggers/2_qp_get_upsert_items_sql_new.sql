exec qp_drop_existing 'qp_get_upsert_items_sql_new', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_get_upsert_items_sql_new]
@table_name nvarchar(25),
@sql nvarchar(max) output
as
BEGIN
    set @sql = 'update base set '
    set @sql = @sql + ' base.modified = ci.modified, base.last_modified_by = ci.last_modified_by, base.status_type_id = ci.status_type_id, '
    set @sql = @sql + ' base.visible = ci.visible, base.archive = ci.archive '
    set @sql = @sql + ' from ' + @table_name + ' base with(rowlock) '
    set @sql = @sql + ' inner join content_item ci with(rowlock) on base.content_item_id = ci.content_item_id '
    set @sql = @sql + ' inner join @ids i on ci.content_item_id = i.id'
    set @sql = @sql + ';' + CHAR(13) + CHAR(10)

    set @sql = @sql + 'insert into ' + @table_name + ' (content_item_id, created, modified, last_modified_by, status_type_id, visible, archive)'
    set @sql = @sql + ' select ci.content_item_id, ci.created, ci.modified, ci.last_modified_by, '
    set @sql = @sql + ' case when i2.id is not null then @noneId else ci.status_type_id end as status_type_id, '
    set @sql = @sql + ' ci.visible, ci.archive '
    set @sql = @sql + ' from content_item ci left join ' + @table_name + ' base on ci.content_item_id = base.content_item_id '
    set @sql = @sql + ' inner join @ids i on ci.content_item_id = i.id '
    set @sql = @sql + ' left join @ids2 i2 on ci.content_item_id = i2.id '
    set @sql = @sql + ' where base.content_item_id is null'
END
GO
