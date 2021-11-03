exec qp_drop_existing 'tbd_delete_content', 'IsTrigger'
GO

CREATE TRIGGER [dbo].[tbd_delete_content] ON [dbo].[CONTENT] INSTEAD OF DELETE
AS
BEGIN
	create table #disable_td_delete_item(id numeric)

	UPDATE content_attribute SET related_attribute_id = NULL
	where related_attribute_id in (
		select attribute_id from content_attribute ca
		inner join deleted d on ca.content_id = d.content_id
	)

	UPDATE content_attribute SET CLASSIFIER_ATTRIBUTE_ID = NULL, AGGREGATED = 0
	where CLASSIFIER_ATTRIBUTE_ID in (
		select attribute_id from content_attribute ca
		inner join deleted d on ca.content_id = d.content_id
	)
	UPDATE content_attribute SET TREE_ORDER_FIELD = NULL
	where TREE_ORDER_FIELD in (
		select attribute_id from content_attribute ca
		inner join deleted d on ca.content_id = d.content_id
	)
	update content_attribute set link_id = null where link_id in (select link_id from content_link cl
	inner join deleted d on cl.content_id = d.content_id)

	delete USER_DEFAULT_FILTER from USER_DEFAULT_FILTER f
	inner join deleted d on d.content_id = f.CONTENT_ID

	delete content_to_content from content_to_content cc
	inner join deleted d on d.content_id = cc.r_content_id or d.content_id = cc.l_content_id

	delete container from container c
	inner join deleted d on d.content_id = c.content_id

	delete content_form from content_form cf
	inner join deleted d on d.content_id = cf.content_id

	delete content_item from content_item ci
	inner join deleted d on d.content_id = ci.content_id

	delete content_tab_bind from content_tab_bind ctb
	inner join deleted d on d.content_id = ctb.content_id

	delete [ACTION_CONTENT_BIND] from [ACTION_CONTENT_BIND] acb
	inner join deleted d on d.content_id = acb.content_id

	delete ca from CONTENT_ATTRIBUTE ca
	inner join CONTENT_ATTRIBUTE cad on ca.BACK_RELATED_ATTRIBUTE_ID = cad.ATTRIBUTE_ID
	inner join deleted c on cad.CONTENT_ID = c.CONTENT_ID

    delete plugin_field_value from plugin_field_value pv
	inner join deleted d on d.content_id = pv.content_id

	delete content from content c inner join deleted d on c.content_id = d.content_id

	drop table #disable_td_delete_item
END

GO
