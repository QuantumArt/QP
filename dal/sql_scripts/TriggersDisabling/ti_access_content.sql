ALTER TRIGGER [dbo].[ti_access_content] ON [dbo].[CONTENT] FOR INSERT
AS
  if object_id('tempdb..#disable_ti_access_content') is null
  begin
	  INSERT INTO content_access
		(content_id, user_id, permission_level_id, last_modified_by, propagate_to_items)
	  SELECT
		content_id, last_modified_by, 1, 1,
		propagate_to_items =
			case virtual_type
				when 0 then 1
				else 0
			end
	  FROM inserted

	  INSERT INTO content_access
		(content_id, user_id, group_id, permission_level_id, last_modified_by, propagate_to_items)
	  SELECT
		i.content_id,ca.user_id, ca.group_id, ca.permission_level_id , 1,
		propagate_to_items =
			case virtual_type
				when 0 then 1
				else 0
			end
	  FROM site_access ca, inserted i
	  WHERE ca.site_id = i.site_id
		AND (ca.user_id <> i.last_modified_by OR ca.user_id IS NULL)
		AND ca.propagate_to_contents = 1

	  INSERT INTO content_access
		(content_id, group_id, permission_level_id, last_modified_by, propagate_to_items)
	  SELECT
		c.content_id, 1, 1,	1,
		propagate_to_items =
			case virtual_type
				when 0 then 1
				else 0
			end
	  FROM inserted AS c
	  WHERE c.content_id NOT IN (
		SELECT ca.content_id FROM content_access AS ca
		WHERE ca.content_id = c.content_id AND ca.group_id = 1
	  )
  end
GO