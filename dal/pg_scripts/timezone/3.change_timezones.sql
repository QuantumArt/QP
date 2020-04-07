DO $$
	declare
		columns column_to_process[];
		item column_to_process;
		new_item text;
		sql text;
	begin

	    columns := array_agg(row(c.table_name, c.column_name)) from information_schema.columns c where table_schema = 'public' and data_type = 'timestamp without time zone'
        and not table_name in (
	        select table_name from information_schema.views where table_schema = 'public'
        )
	    and (table_name like '%content_%'
        or table_name in (
                          'access_token',
                          'action_access',
                          'backend_action_log',
                          'button_trace',
                          'cdclastexecutedlsn',
                          'code_snippet',
                          'container',
                          'content',
                          'custom_action',
                          'dangerous_actions',
                          'db',
                          'entity_type_access',
                          'external_notification_queue',
                          'folder',
                          'folder_access',
                          'messages',
                          'notifications',
                          'notifications_sent',
                          'object',
                          'object_format',
                          'object_format_version',
                          'page',
                          'page_template',
                          'page_trace',
                          'product_relevance',
                          'products',
                          'product_versions',
                          'region_updates',
                          'removed_entities',
                          'removed_files',
                          'sessions_log',
                          'site',
                          'site_access',
                          'status_type',
                          'style',
                          'system_notification_queue',
                          'tasks',
                          'user_group',
                          'users',
                          've_command',
                          've_plugin',
                          've_style',
                          'workflow',
                          'workflow_access',
                          'xml_db_update',
                          'xml_db_update_actions'
                    ));

		if columns is not null then
			foreach item in array columns loop
			    RAISE NOTICE 'Table %, column %', item.table_name, item.column_name;
			    CALL qp_change_timestamp_zone_time(item.table_name, item.column_name);
			end loop;
		end if;
	end;
$$ LANGUAGE plpgsql;