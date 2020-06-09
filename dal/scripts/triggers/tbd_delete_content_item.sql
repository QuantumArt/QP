ALTER TRIGGER [dbo].[tbd_delete_content_item] ON [dbo].[CONTENT_ITEM] INSTEAD OF DELETE
AS
BEGIN

delete waiting_for_approval from waiting_for_approval wa inner join deleted d on wa.content_item_id = d.content_item_id

delete child_delays from child_delays cd inner join deleted d on cd.child_id = d.content_item_id

IF dbo.qp_get_version_control() IS NOT NULL BEGIN
  delete content_item_version from content_item_version civ inner join deleted d on civ.content_item_id = d.content_item_id

  delete item_to_item_version from item_to_item_version iiv
  inner join content_item_version civ on civ.content_item_version_id = iiv.content_item_version_id
  inner join deleted d on d.content_item_id = civ.content_item_id

  delete item_to_item_version from item_to_item_version iiv
  inner join deleted d on d.content_item_id = iiv.linked_item_id
END

delete item_link_united_full from item_link_united_full ii where ii.item_id in (select content_item_id from deleted)

-- delete asymettric links
delete item_link_united_full from item_link_united_full ii where ii.linked_item_id in (select content_item_id from deleted)

DELETE from FIELD_ARTICLE_BIND where [ARTICLE_ID] in (select content_item_id from deleted)

delete content_data from content_data cd inner join deleted d on cd.content_item_id = d.content_item_id

delete content_item from content_item ci inner join deleted d on ci.content_item_id = d.content_item_id

END
GO
