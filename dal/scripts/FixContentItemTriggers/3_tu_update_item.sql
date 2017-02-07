ALTER TRIGGER [dbo].[tu_update_item] ON [dbo].[CONTENT_ITEM] FOR UPDATE
AS
begin
    if not update(locked_by) and not update(splitted) and not UPDATE(not_for_replication)
    begin
        declare @content_id numeric
        declare @sql nvarchar(max), @table_name varchar(50), @async_table_name varchar(50)
        declare @items_list nvarchar(max), @async_ids_list nvarchar(max), @sync_ids_list nvarchar(max)

        declare @contents table
        (
            id numeric primary key
        )

        insert into @contents
        select distinct content_id from inserted
        where CONTENT_ID in (select CONTENT_ID from content where virtual_type = 0)

        create table #ids_with_splitted
        (
            id numeric primary key,
            new_splitted bit
        )

        declare @items table
        (
            id numeric primary key,
            splitted bit,
            not_for_replication bit,
            cancel_split bit
        )

        declare @ids [Ids], @ids2 [Ids]

        while exists (select id from @contents)
        begin
            select @content_id = id from @contents

            insert into @items
            select i.content_item_id, i.SPLITTED, i.not_for_replication, i.cancel_split from inserted i
            inner join content_item ci on i.content_item_id = ci.content_item_id
            where ci.CONTENT_ID = @content_id

            insert into @ids
            select id from @items

            insert into @ids2
            select id from @items where cancel_split = 1

            set @items_list = null
            select @items_list = coalesce(@items_list + ',', '') + convert(nvarchar, id) from @items

            set @sql = 'insert into #ids_with_splitted '
            set @sql = @sql + ' select content_item_id,'
            set @sql = @sql + ' case'
            set @sql = @sql + ' when curr_weight < front_weight and is_workflow_async = 1 then 1'
            set @sql = @sql + ' when curr_weight = workflow_max_weight and delayed = 1 then 1'
            set @sql = @sql + ' else 0'
            set @sql = @sql + ' end'
            set @sql = @sql + ' as new_splitted from ('
            set @sql = @sql + ' select distinct ci.content_item_id, st1.WEIGHT as curr_weight, st2.WEIGHT as front_weight, '
            set @sql = @sql + ' max(st3.WEIGHT) over (partition by ci.content_item_id) as workflow_max_weight, case when i2.id is not null then 0 else ciw.is_async end as is_workflow_async, '
            set @sql = @sql + ' ci.SCHEDULE_NEW_VERSION_PUBLICATION as delayed '
            set @sql = @sql + ' from content_item ci'
            set @sql = @sql + ' inner join @ids i on i.id = ci.content_item_id'
            set @sql = @sql + ' left join @ids2 i2 on i.id = ci.content_item_id'
            set @sql = @sql + ' inner join content_' + CONVERT(nvarchar, @content_id) + ' c on ci.CONTENT_ITEM_ID = c.CONTENT_ITEM_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st1 on ci.STATUS_TYPE_ID = st1.STATUS_TYPE_ID'
            set @sql = @sql + ' inner join STATUS_TYPE st2 on c.STATUS_TYPE_ID = st2.STATUS_TYPE_ID'
            set @sql = @sql + ' left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id'
            set @sql = @sql + ' left join workflow_rules wr on ciw.WORKFLOW_ID = wr.WORKFLOW_ID'
            set @sql = @sql + ' left join STATUS_TYPE st3 on st3.STATUS_TYPE_ID = wr.SUCCESSOR_STATUS_ID'
            set @sql = @sql + ' ) as main'
            print @sql
            exec sp_executesql @sql, N'@ids [Ids] READONLY, @ids2 [Ids] READONLY', @ids = @ids, @ids2 = @ids2

            update base set base.splitted = i.new_splitted from @items base inner join #ids_with_splitted i on base.id = i.id
            update base set base.splitted = i.splitted from content_item base inner join @items i on base.CONTENT_ITEM_ID = i.id

            insert into content_item_splitted(content_item_id)
            select id from @items base where splitted = 1 and not exists (select * from content_item_splitted cis where cis.content_item_id = base.id)

            delete from content_item_splitted where content_item_id in (
                select id from @items base where splitted = 0
            )

            set @sync_ids_list = null
            select @sync_ids_list = coalesce(@sync_ids_list + ',', '') + convert(nvarchar, id) from @items where splitted = 0 and not_for_replication = 0
            set @async_ids_list = null
            select @async_ids_list = coalesce(@async_ids_list + ',', '') + convert(nvarchar, id) from @items where splitted = 1 and not_for_replication = 0

            set @table_name = 'content_' + CONVERT(nvarchar, @content_id)
            set @async_table_name = @table_name + '_async'

            if @sync_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @table_name, @sync_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql

                exec qp_get_delete_items_sql @content_id, @sync_ids_list, 1, @sql = @sql out
                print @sql
                exec sp_executesql @sql
            end

            if @async_ids_list <> ''
            begin
                exec qp_get_upsert_items_sql @async_table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql

                exec qp_get_update_items_flags_sql @table_name, @async_ids_list, @sql = @sql out
                print @sql
                exec sp_executesql @sql
            end

            delete from #ids_with_splitted

            delete from @contents where id = @content_id

            delete from @items
            delete from @ids
            delete from @ids2
        end

        drop table #ids_with_splitted

    end
end
GO
