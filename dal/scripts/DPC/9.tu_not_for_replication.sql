ALTER TRIGGER [dbo].[tu_not_for_replication] ON [dbo].[CONTENT_ITEM] FOR UPDATE
AS
BEGIN
	if update(not_for_replication)
	begin
		update base set base.not_for_replication = i.not_for_replication
		from content_data base inner join inserted i on i.content_item_id = base.CONTENT_ITEM_ID
	end

    if update(splitted)
	begin
		update base set base.splitted = i.splitted
		from content_data base inner join inserted i on i.content_item_id = base.CONTENT_ITEM_ID
	end
END
GO