ALTER  PROCEDURE [dbo].[restore_content_item_version]
  @uid NUMERIC,
  @version_id NUMERIC
AS
  DECLARE @id NUMERIC, @tm DATETIME
  DECLARE @content_id numeric, @splitted bit
  SET @tm = GETDATE()
  SELECT @id = content_item_id FROM content_item_version WHERE content_item_version_id = @version_id
  IF @id IS NOT NULL BEGIN
    select @content_id = content_id, @splitted = splitted from content_item where content_item_id = @id

    -- Restore common data
    DELETE FROM content_data WHERE content_item_id = @id
    INSERT INTO content_data (attribute_id, content_item_id, data, blob_data)
    SELECT attribute_id, @id, data, blob_data
    FROM version_content_data
    WHERE content_item_version_id = @version_id

    -- Restore many-to-many data
    IF @splitted = 1
    begin
		DELETE FROM item_link_async where item_id = @id and link_id in (select link_id from content_attribute where content_id = @content_id)

		INSERT INTO item_link_async (link_id, item_id, linked_item_id)
		SELECT link_id, @id, linked_item_id FROM item_to_item_version AS iv
		INNER JOIN content_attribute ca on iv.attribute_id = ca.attribute_id
		WHERE iv.content_item_version_id = @version_id and link_id is not null
    end else
    begin
		DELETE FROM item_link_united_full where item_id = @id and link_id in (select link_id from content_attribute where content_id = @content_id)

		INSERT INTO item_to_item (link_id, l_item_id, r_item_id)
		SELECT link_id, @id, linked_item_id FROM item_to_item_version AS iv
		INNER JOIN content_attribute ca on iv.attribute_id = ca.attribute_id
		WHERE iv.content_item_version_id = @version_id and link_id is not null
    end

    -- Restore many-to-one data
    create table #resultIds (id numeric, attribute_id numeric not null, to_remove bit not null default 0)

    declare @fieldIds table (id numeric, back_id numeric)

    insert into @fieldIds
    select ATTRIBUTE_ID, BACK_RELATED_ATTRIBUTE_ID From CONTENT_ATTRIBUTE where BACK_RELATED_ATTRIBUTE_ID is not null and CONTENT_ID = @content_id

    while exists(select * from @fieldIds)
    begin
    	declare @currentFieldId numeric, @currentBackFieldId numeric
    	select @currentFieldId = id, @currentBackFieldId = back_id from @fieldIds

    	declare @ids table (id numeric)
    	insert into @ids
    	select linked_item_id from item_to_item_version where attribute_id = @currentFieldId and content_item_version_id = @version_id

    	declare @value nvarchar(max)
    	set @value = ''
    	while exists(select * from @ids)
    	begin
    		declare @currentId numeric
    		select @currentId = id from @ids
    		if @value <> ''
    			set @value = @value + ','
    		set @value = @value + CAST(@currentId as nvarchar)

    		delete from @ids where id = @currentId

    	end

    	exec qp_update_m2o @id, @currentBackFieldId, @value


		delete from @fieldIds where id = @currentFieldId
    end

    exec qp_update_m2o_final @id

    drop table #resultIds

    update content_item set MODIFIED = GETDATE(), LAST_MODIFIED_BY = @uid where CONTENT_ITEM_ID = @id

    -- Write status history log
    INSERT INTO content_item_status_history
      (content_item_id, user_id, description, created,
      system_status_type_id, content_item_version_id)
    VALUES
      (@id, @uid, 'Record has been restored from version backup', @tm,
      4, @version_id)
  END
GO
