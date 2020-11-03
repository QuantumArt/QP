create extension IF NOT EXISTS hstore;
create extension IF NOT EXISTS intarray;
create extension IF NOT EXISTS pgcrypto;
create extension IF NOT EXISTS tablefunc;
ALTER TABLE content_data ALTER COLUMN data TYPE text;

ALTER TABLE content_data ADD COLUMN IF NOT EXISTS o2m_data numeric(18,0) NULL;

ALTER TABLE content_data ADD COLUMN IF NOT EXISTS ft_data tsvector NULL;


update content_data cd set o2m_data = data::numeric from content_attribute ca
where ca.attribute_id = cd.attribute_id and ca.attribute_type_id = 11 and ca.link_id is null
and cd.o2m_data is null and cd.data is not null;

-- DROP INDEX public.ix_o2m_data;

CREATE INDEX IF NOT EXISTS ix_o2m_data
    ON public.content_data USING btree
    (o2m_data)
    TABLESPACE pg_default;

update content_data cd
set ft_data = to_tsvector('russian', coalesce(cd.data, cd.blob_data))
from content_attribute ca
where ca.attribute_id = cd.attribute_id
and cd.ft_data is null and coalesce(cd.data, cd.blob_data) is not null;

CREATE INDEX IF NOT EXISTS ix_ft_data ON content_data USING gist(ft_data);


DO $$
BEGIN

  BEGIN

    ALTER TABLE public.content_data
        ADD CONSTRAINT fk_content_data_content_attribute FOREIGN KEY (attribute_id)
        REFERENCES public.content_attribute (attribute_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE;


  EXCEPTION
    WHEN duplicate_object THEN RAISE NOTICE 'Table constraint fk_content_data_content_attribute already exists';
  END;

END $$;

DO $$
BEGIN
  BEGIN

    ALTER TABLE public.content_data
        ADD CONSTRAINT fk_content_data_content_item FOREIGN KEY (content_item_id)
        REFERENCES public.content_item (content_item_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE NO ACTION;

  EXCEPTION
    WHEN duplicate_object THEN RAISE NOTICE 'Table constraint fk_content_data_content_item already exists';
  END;

END $$;


-- Table: public.content_data

-- DROP TABLE public.content_data;

CREATE TABLE IF NOT EXISTS public.content_item_ft
(
    content_item_id numeric(18,0) NOT NULL,
    ft_data tsvector,
    PRIMARY KEY (content_item_id),
    CONSTRAINT fk_content_item_ft_content_item FOREIGN KEY (content_item_id)
        REFERENCES public.content_item (content_item_id) MATCH SIMPLE
        ON UPDATE NO ACTION
        ON DELETE CASCADE
);

ALTER TABLE public.content_item_ft
    OWNER to postgres;


CREATE INDEX IF NOT EXISTS ix_content_item_ft_data
    ON public.content_item_ft USING gin
    (ft_data)
    TABLESPACE pg_default;






ALTER TABLE version_content_data ALTER COLUMN data TYPE text;

ALTER TABLE version_content_data ADD COLUMN IF NOT EXISTS o2m_data numeric(18,0) NULL;

update version_content_data cd set o2m_data = data::numeric from content_attribute ca
where ca.attribute_id = cd.attribute_id and ca.attribute_type_id = 11 and ca.link_id is null
and cd.o2m_data is null and cd.data is not null;

-- DROP INDEX public.ix_o2m_data;

CREATE INDEX IF NOT EXISTS ix_version_o2m_data
    ON public.version_content_data USING btree
    (o2m_data)
    TABLESPACE pg_default;
DO $$ BEGIN
    create type content_link as
    (
        id numeric(18),
        is_symmetric boolean,
        l_content_id numeric(18),
        r_content_id numeric(18)
    );

    alter type content_link owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;



DO $$ BEGIN
    create type link as
    (
        id numeric(18),
        linked_id numeric(18)
    );

    alter type link owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    create type link_data as
    (
        id numeric(18),
        attribute_id numeric(18),
        has_data boolean,
        splitted boolean,
        has_async boolean
    );

    alter type link_data owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
	CREATE TYPE public.link_multiple AS
	(
		id numeric(18,0),
		link_id numeric(18,0),
		linked_id numeric(18,0)
	);

	ALTER TYPE public.link_multiple
		OWNER TO postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    create type link_multiple_splitted as
    (
        id numeric(18),
        link_id numeric(18),
        linked_id numeric(18),
        splitted boolean
    );

    alter type link_multiple_splitted owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    create type value as
    (
        id numeric,
        field_id numeric,
        data text
    );

    alter type value owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

CREATE OR REPLACE PROCEDURE public.qp_change_timestamp_zone_time(
	table_name text,
	column_name text,
	use_timezone boolean default true
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    time_sql text;
	    sql text;
	BEGIN
	    time_sql := case when use_timezone then 'with' else 'without' end;
        sql := 'alter table %s alter column "%s" type timestamp %s time zone';
	    sql := format(sql, table_name, lower(column_name), time_sql);
	    execute sql;
	END;
$BODY$;

alter procedure qp_change_timestamp_zone_time(text, text, boolean) owner to postgres;

DO $$ BEGIN
    create type column_to_process as
    (
        table_name text,
        column_name text
    );

    alter type column_to_process owner to postgres;

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;
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
create or replace view backend_action_access_permlevel(action_access_id, user_id, group_id, permission_level, backend_action_id) as
SELECT c.action_access_id,
       c.user_id,
       c.group_id,
       pl.permission_level,
       x.id AS backend_action_id
FROM ((action_access c
    JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)))
         JOIN backend_action x ON ((c.action_id = x.id)));

alter table backend_action_access_permlevel
    owner to postgres;


create or replace view content_access_permlevel(content_id, user_id, group_id, permission_level_id, created, modified,
                                      last_modified_by, propagate_to_items, content_access_id, hide,
                                      permission_level) as
SELECT c.content_id,
       c.user_id,
       c.group_id,
       c.permission_level_id,
       c.created,
       c.modified,
       c.last_modified_by,
       c.propagate_to_items,
       c.content_access_id,
       c.hide,
       pl.permission_level
FROM (content_access c
         JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)));

alter table content_access_permlevel
    owner to postgres;


create or replace view content_access_permlevel_site(content_id, user_id, group_id, permission_level_id, created, modified,
                                           last_modified_by, propagate_to_items, content_access_id, hide,
                                           permission_level, site_id) as
SELECT c.content_id,
       c.user_id,
       c.group_id,
       c.permission_level_id,
       c.created,
       c.modified,
       c.last_modified_by,
       c.propagate_to_items,
       c.content_access_id,
       c.hide,
       pl.permission_level,
       x.site_id
FROM ((content_access c
    JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)))
         JOIN content x ON ((c.content_id = x.content_id)));

alter table content_access_permlevel_site
    owner to postgres;


create or replace view content_attribute_type(database_type, input_type, icon, type_name, attribute_id, content_id, attribute_name,
                                   format_mask, input_mask, attribute_size, default_value, attribute_type_id,
                                   related_attribute_id, index_flag, description, modified, created, last_modified_by,
                                   attribute_order, required, permanent_flag, primary_flag, relation_condition,
                                   display_as_radio_button, view_in_list, readonly_flag, allow_stage_edit,
                                   attribute_configuration, related_image_attribute_id, persistent_attr_id,
                                   join_attr_id, link_id, default_blob_value, auto_load, friendly_name,
                                   use_site_library, use_archive_articles, auto_expand, relation_page_size, doctype,
                                   full_page, rename_matched, subfolder, disable_version_control, map_as_property,
                                   net_attribute_name, net_back_attribute_name, p_enter_mode, use_english_quotes,
                                   back_related_attribute_id, is_long, external_css, root_element_class, use_for_tree,
                                   auto_check_children, aggregated, classifier_attribute_id, is_classifier, changeable,
                                   use_relation_security, copy_permissions_to_children, enum_values,
                                   show_as_radio_button, use_for_default_filtration, tree_order_field,
                                   parent_attribute_id, hide, override, use_for_context, use_for_variations,
                                   order_by_title, field_title_count, include_relations_in_title,
                                   use_in_child_content_filter, optimize_for_hierarchy, is_localization,
                                   use_separate_reverse_views, disable_list_auto_wrap, ta_highlight_type,
                                   max_data_list_item_count) as
SELECT at.database_type,
       at.input_type,
       at.icon,
       at.type_name,
       ca.attribute_id,
       ca.content_id,
       ca.attribute_name,
       ca.format_mask,
       ca.input_mask,
       ca.attribute_size,
       ca.default_value,
       ca.attribute_type_id,
       ca.related_attribute_id,
       ca.index_flag,
       ca.description,
       ca.modified,
       ca.created,
       ca.last_modified_by,
       ca.attribute_order,
       ca.required,
       ca.permanent_flag,
       ca.primary_flag,
       ca.relation_condition,
       ca.display_as_radio_button,
       ca.view_in_list,
       ca.readonly_flag,
       ca.allow_stage_edit,
       ca.attribute_configuration,
       ca.related_image_attribute_id,
       ca.persistent_attr_id,
       ca.join_attr_id,
       ca.link_id,
       ca.default_blob_value,
       ca.auto_load,
       ca.friendly_name,
       ca.use_site_library,
       ca.use_archive_articles,
       ca.auto_expand,
       ca.relation_page_size,
       ca.doctype,
       ca.full_page,
       ca.rename_matched,
       ca.subfolder,
       ca.disable_version_control,
       ca.map_as_property,
       ca.net_attribute_name,
       ca.net_back_attribute_name,
       ca.p_enter_mode,
       ca.use_english_quotes,
       ca.back_related_attribute_id,
       ca.is_long,
       ca.external_css,
       ca.root_element_class,
       ca.use_for_tree,
       ca.auto_check_children,
       ca.aggregated,
       ca.classifier_attribute_id,
       ca.is_classifier,
       ca.changeable,
       ca.use_relation_security,
       ca.copy_permissions_to_children,
       ca.enum_values,
       ca.show_as_radio_button,
       ca.use_for_default_filtration,
       ca.tree_order_field,
       ca.parent_attribute_id,
       ca.hide,
       ca.override,
       ca.use_for_context,
       ca.use_for_variations,
       ca.order_by_title,
       ca.field_title_count,
       ca.include_relations_in_title,
       ca.use_in_child_content_filter,
       ca.optimize_for_hierarchy,
       ca.is_localization,
       ca.use_separate_reverse_views,
       ca.disable_list_auto_wrap,
       ca.ta_highlight_type,
       ca.max_data_list_item_count
FROM (content_attribute ca
         JOIN attribute_type at ON ((ca.attribute_type_id = at.attribute_type_id)));

alter table content_attribute_type
    owner to postgres;


create or replace view content_item_access_permlevel(content_item_id, user_id, group_id, permission_level_id, created, modified,
                                          last_modified_by, content_item_access_id, permission_level) as
SELECT c.content_item_id,
       c.user_id,
       c.group_id,
       c.permission_level_id,
       c.created,
       c.modified,
       c.last_modified_by,
       c.content_item_access_id,
       pl.permission_level
FROM (content_item_access c
         JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)));

alter table content_item_access_permlevel
    owner to postgres;


create or replace view content_item_access_permlevel_content(content_item_id, user_id, group_id, permission_level_id, created,
                                                  modified, last_modified_by, content_item_access_id, permission_level,
                                                  content_id) as
SELECT c.content_item_id,
       c.user_id,
       c.group_id,
       c.permission_level_id,
       c.created,
       c.modified,
       c.last_modified_by,
       c.content_item_access_id,
       pl.permission_level,
       x.content_id
FROM ((content_item_access c
    JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)))
         JOIN content_item x ON ((c.content_item_id = x.content_item_id)));

alter table content_item_access_permlevel_content
    owner to postgres;


create or replace view content_item_workflow(content_item_id, content_id, workflow_id, is_async, article_workflow) as
SELECT ci.content_item_id,
       ci.content_id,
       CASE
           WHEN awb.content_item_id IS NOT NULL THEN awb.workflow_id
           ELSE cwb.workflow_id
       END
       AS workflow_id,
       COALESCE(awb.is_async, cwb.is_async) AS is_async,
       CASE
           WHEN (awb.content_item_id IS NULL) THEN 0
           ELSE 1
       END
       AS article_workflow
FROM content_item ci
    LEFT JOIN article_workflow_bind awb ON ci.content_item_id = awb.content_item_id
         LEFT JOIN content_workflow_bind cwb ON ci.content_id = cwb.content_id;

alter table content_item_workflow
    owner to postgres;


create or replace view entity_type_access_permlevel(entity_type_access_id, user_id, group_id, permission_level, entity_type_id) as
SELECT c.entity_type_access_id,
       c.user_id,
       c.group_id,
       pl.permission_level,
       x.id AS entity_type_id
FROM ((entity_type_access c
    JOIN permission_level pl ON ((c.permission_level_id = pl.permission_level_id)))
         JOIN entity_type x ON ((c.entity_type_id = x.id)));

alter table entity_type_access_permlevel
    owner to postgres;


create or replace view folder_access_permlevel as
    SELECT c.*, pl.permission_level from FOLDER_ACCESS as c
    INNER JOIN Permission_Level as pl ON c.permission_level_id = pl.permission_level_id;

alter table folder_access_permlevel
    owner to postgres;


create or replace view folder_access_permlevel_site as
    SELECT c.*, pl.permission_level, x.site_id from FOLDER_ACCESS as c
    INNER JOIN Permission_Level as pl ON c.permission_level_id = pl.permission_level_id
    INNER JOIN folder as x ON c.folder_id = x.folder_id;

alter table folder_access_permlevel_site
    owner to postgres;


create or replace view full_workflow_rules(workflow_rule_id, user_id, group_id, rule_order, predecessor_permission_id,
                                successor_permission_id, successor_status_id, comment, workflow_id) as
    SELECT workflow_rules.workflow_rule_id,
           workflow_rules.user_id,
           workflow_rules.group_id,
           workflow_rules.rule_order,
           workflow_rules.predecessor_permission_id,
           workflow_rules.successor_permission_id,
           workflow_rules.successor_status_id,
           workflow_rules.comment,
           workflow_rules.workflow_id
    FROM workflow_rules
    UNION ALL
    SELECT 0                                  AS workflow_rule_id,
           1                                  AS user_id,
           NULL::numeric                      AS group_id,
           0                                  AS rule_order,
           NULL::numeric                      AS predecessor_permission_id,
           NULL::numeric                      AS successor_permission_id,
           st.status_type_id                  AS successor_status_id,
           '(no comments)'::character varying AS comment,
           w.workflow_id
    FROM (workflow w
             JOIN status_type st ON ((w.site_id = st.site_id)))
    WHERE ((st.status_type_name)::text = 'None'::text);

alter table full_workflow_rules
    owner to postgres;


create or replace view item_link(link_id, item_id, linked_item_id, is_rev, is_self) as
SELECT ii.link_id,
       ii.l_item_id AS item_id,
       ii.r_item_id AS linked_item_id,
       ii.is_rev,
       ii.is_self
FROM item_to_item ii;

alter table item_link
    owner to postgres;


create or replace view item_link_united(link_id, item_id, linked_item_id, is_rev, is_self) as
    SELECT il.link_id,
           il.item_id,
           il.linked_item_id,
           il.is_rev,
           il.is_self
    FROM item_link il
    WHERE (NOT (EXISTS(SELECT cis.content_item_id
                       FROM content_item_splitted cis
                       WHERE ((il.item_id)::numeric = cis.content_item_id))))
    UNION ALL
    SELECT ila.link_id,
           ila.item_id,
           ila.linked_item_id,
           ila.is_rev,
           ila.is_self
    FROM item_link_async ila;

alter table item_link_united
    owner to postgres;


-- View: public.site_access_permlevel

-- DROP VIEW public.site_access_permlevel;

CREATE OR REPLACE VIEW public.site_access_permlevel AS
 SELECT c.site_id,
    c.user_id,
    c.group_id,
    c.permission_level_id,
    c.created,
    c.modified,
    c.last_modified_by,
    c.propagate_to_contents,
    c.site_access_id,
    pl.permission_level
   FROM site_access c
     JOIN permission_level pl ON c.permission_level_id = pl.permission_level_id;

ALTER TABLE public.site_access_permlevel
    OWNER TO postgres;


CREATE OR REPLACE VIEW public.status_type_new AS
 SELECT cast(site_id as int) as site_id, cast(status_type_id as int) as id, status_type_name as name, cast(weight as int) as weight from STATUS_TYPE;

ALTER TABLE public.status_type_new
    OWNER TO postgres;


create or replace view template_object(object_id, parent_object_id, page_template_id, page_id, object_name, object_format_id,
                            description, object_type_id, use_default_values, created, modified, last_modified_by,
                            allow_stage_edit, global, net_object_name, locked, locked_by, enable_viewstate,
                            control_custom_class, disable_databind, permanent_lock, icon) as
SELECT o.object_id,
       o.parent_object_id,
       o.page_template_id,
       o.page_id,
       o.object_name,
       o.object_format_id,
       o.description,
       o.object_type_id,
       o.use_default_values,
       o.created,
       o.modified,
       o.last_modified_by,
       o.allow_stage_edit,
       o.global,
       o.net_object_name,
       o.locked,
       o.locked_by,
       o.enable_viewstate,
       o.control_custom_class,
       o.disable_databind,
       o.permanent_lock,
       ot.icon
FROM (object o
         JOIN object_type ot ON ((o.object_type_id = ot.object_type_id)))
WHERE (o.page_id IS NULL);

alter table template_object
    owner to postgres;


CREATE OR REPLACE VIEW public.user_group_bind_new AS
 SELECT cast(group_id as int) as group_id, cast(user_id as int) as user_id from user_group_bind;

ALTER TABLE public.user_group_bind_new
    OWNER TO postgres;

CREATE OR REPLACE VIEW public.user_group_new AS
 SELECT cast(group_id as int) as id, group_name as name from user_group;

ALTER TABLE public.user_group_new
    OWNER TO postgres;

create or replace view user_group_tree(group_id, group_name, description, created, modified, last_modified_by,
                            last_modified_by_login, shared_content_items, nt_group, ad_sid, built_in, readonly,
                            use_parallel_workflow, can_unlock_items, parent_group_id) as
SELECT ug.group_id,
       ug.group_name,
       ug.description,
       ug.created,
       ug.modified,
       ug.last_modified_by,
       u.login AS last_modified_by_login,
       ug.shared_content_items,
       ug.nt_group,
       ug.ad_sid,
       ug.built_in,
       ug.readonly,
       ug.use_parallel_workflow,
       ug.can_unlock_items,
       gtg.parent_group_id
FROM ((user_group ug
    LEFT JOIN group_to_group gtg ON ((ug.group_id = gtg.child_group_id)))
         JOIN users u ON ((u.user_id = ug.last_modified_by)));

alter table user_group_tree
    owner to postgres;


CREATE OR REPLACE VIEW public.user_new AS
 SELECT cast(user_id as int) as id, login ,nt_login, l.iso_code, first_name, last_name, email  from users u
     inner join LANGUAGES l on l.language_id = u.language_id;

ALTER TABLE public.user_new
    OWNER TO postgres;

create or replace view VIRTUAL_ATTR_BASE_ATTR_RELATION AS
	WITH V2BREL AS
		(SELECT USER_QUERY_ATTR_ID as VIRTUAL_ATTR_ID
			  ,BASE_ATTR_ID as BASE_ATTR_ID
		FROM V_USER_QUERY_ATTRS
		UNION ALL
		SELECT virtual_attr_id as VIRTUAL_ATTR_ID
			  ,union_attr_id as BASE_ATTR_ID
		FROM union_attrs
		UNION ALL
		select ATTRIBUTE_ID as VIRTUAL_ATTR_ID,
		persistent_attr_id as BASE_ATTR_ID
		from CONTENT_ATTRIBUTE
		where persistent_attr_id is not null)
	select AR.BASE_ATTR_ID, BC.CONTENT_ID BASE_CNT_ID, BC.VIRTUAL_TYPE BASE_CNT_VTYPE,
	AR.VIRTUAL_ATTR_ID, VC.CONTENT_ID VIRTUAL_CNT_ID, VC.VIRTUAL_TYPE VIRTUAL_CNT_VTYPE
	from V2BREL AR
	JOIN CONTENT_ATTRIBUTE BA ON BA.ATTRIBUTE_ID = AR.BASE_ATTR_ID
	JOIN CONTENT_ATTRIBUTE VA ON VA.ATTRIBUTE_ID = AR.VIRTUAL_ATTR_ID
	JOIN CONTENT BC ON BA.CONTENT_ID = BC.CONTENT_ID
	JOIN CONTENT VC ON VA.CONTENT_ID = VC.CONTENT_ID;

alter table v_user_query_attrs
    owner to postgres;

CREATE OR REPLACE VIEW VIRTUAL_CONTENT_RELATION AS
	select  DISTINCT
			PA.CONTENT_ID AS BASE_CONTENT_ID,		
			A.CONTENT_ID AS VIRTUAL_CONTENT_ID
			from CONTENT_ATTRIBUTE A
			JOIN CONTENT_ATTRIBUTE PA ON A.persistent_attr_id = PA.ATTRIBUTE_ID		
	UNION ALL
	SELECT union_content_id AS BASE_CONTENT_ID,
		   virtual_content_id AS VIRTUAL_CONTENT_ID
	FROM union_contents
	UNION ALL
	SELECT real_content_id AS BASE_CONTENT_ID,
		   virtual_content_id AS VIRTUAL_CONTENT_ID
	FROM user_query_contents;


alter table VIRTUAL_CONTENT_RELATION
    owner to postgres;
create or replace view v_user_query_attrs AS
	select vca.ATTRIBUTE_ID as USER_QUERY_ATTR_ID, ca.ATTRIBUTE_ID BASE_ATTR_ID
	from user_query_attrs uqa
	join CONTENT_ATTRIBUTE vca on uqa.virtual_content_id = vca.CONTENT_ID
	join CONTENT_ATTRIBUTE ca on uqa.user_query_attr_id = ca.ATTRIBUTE_ID
	where vca.ATTRIBUTE_NAME = ca.ATTRIBUTE_NAME;

alter table v_user_query_attrs
    owner to postgres;

create or replace view workflow_access_permlevel as
SELECT c.*, pl.permission_level from workflow_access as c INNER JOIN permission_level as pl ON c.permission_level_id = pl.permission_level_id;

alter table workflow_access_permlevel
    owner to postgres;


CREATE OR REPLACE VIEW workflow_access_permlevel_site AS
  SELECT c.*, pl.permission_level, x.site_id from workflow_access as c
  INNER JOIN permission_level as pl ON c.permission_level_id = pl.permission_level_id
  INNER JOIN workflow as x ON c.workflow_id = x.workflow_id;

alter table workflow_access_permlevel_site
    owner to postgres;
create or replace view workflow_max_statuses AS

select workflow_id, STATUS_TYPE_ID as max_status_type_id from
(
	select wr.workflow_id, st.status_type_id, ROW_NUMBER() OVER (PARTITION BY wr.WORKFLOW_ID ORDER BY wr.RULE_ORDER DESC ) AS order
	from workflow_rules wr
	inner join status_type st on wr.successor_status_id = st.status_type_id
) as v
where v.order = 1;

alter table workflow_max_statuses
    owner to postgres;

create or replace function qp_aggregated_and_self(ids integer[]) returns integer[]
    stable
    language plpgsql
as
$$
DECLARE
    classifier_ids int[];
    item_ids int[];
    agg_ids int[];
BEGIN
    item_ids := ids;
    classifier_ids := array_agg(distinct(ca.ATTRIBUTE_ID)) from content_attribute ca
        inner join content_item ci on ca.CONTENT_ID = ci.CONTENT_ID
        where ca.IS_CLASSIFIER and ci.content_item_id = ANY(ids);

    if classifier_ids is not null then
        agg_ids := array_agg(cd.CONTENT_ITEM_ID) FROM content_data cd
        inner join content_attribute ca on ca.attribute_id = cd.attribute_id
	    where ca.AGGREGATED and ca.CLASSIFIER_ATTRIBUTE_ID = ANY(classifier_ids)
        and cd.o2m_data = ANY(ids);
        if agg_ids is not null then
            item_ids := array_cat(item_ids, agg_ids);
        end if;
    end if;
	return item_ids;
END;
$$;

create or replace function qp_aggregates_to_remove(ids integer[]) returns integer[]
    stable
    language plpgsql
as
$$
DECLARE
    agg_ids int[];
BEGIN
    agg_ids := array_agg(cd2.CONTENT_ITEM_ID) FROM content_item ci
    inner join content_attribute ca on ca.CONTENT_ID = ci.CONTENT_ID and ca.is_classifier
    inner join content_data cd on cd.attribute_id = ca.attribute_id and cd.content_item_id = ci.content_item_id
    inner join content_attribute ca2 on ca2.classifier_attribute_id = ca.attribute_id and ca2.aggregated
    inner join content_data cd2 on cd2.o2m_data = ci.content_item_id and cd2.attribute_id = ca2.attribute_id
    where ci.content_item_id = ANY(ids) and cd.data <> ca2.content_id::text;

	return coalesce(agg_ids, ARRAY[]::int[]);
END;
$$;

create or replace function public.qp_batch_insert(input xml, visible int, user_id int)
    returns table("OriginalArticleId" int, "CreatedArticleId" int, "ContentId" int)
    volatile
    language plpgsql
as
$$
DECLARE
    result link_multiple[];
    ids int[];
    articles link[];
    statuses link[];
BEGIN
        articles := array_agg(row(a.ArticleId, a.ContentId) order by ArticleId desc)
        from (
            select distinct xml.ArticleId, xml.ContentId
            FROM XMLTABLE('ITEMS/ITEM' passing input COLUMNS
                ArticleId int PATH '@id',
                FieldId int PATH '@fieldId',
                ContentId int PATH '@contentId',
                Value text PATH 'DATA'
            ) xml
            EXCEPT SELECT CONTENT_ITEM_ID, CONTENT_ID FROM CONTENT_ITEM
        ) a;


        raise notice '%', articles;

        statuses := array_agg(row(s.ContentId, s.StatusId)) from
        (
            SELECT a.ContentId,
            CASE WHEN w.WORKFLOW_ID IS NULL THEN
                ( SELECT STATUS_TYPE_ID FROM STATUS_TYPE t WHERE t.STATUS_TYPE_NAME = 'Published' AND t.SITE_ID = c.SITE_ID)
            ELSE
                ( SELECT STATUS_TYPE_ID FROM STATUS_TYPE t WHERE t.STATUS_TYPE_NAME = 'None' AND t.SITE_ID = c.SITE_ID)
            END StatusId

            FROM unnest(articles) a(ArticleId, ContentId) INNER JOIN CONTENT c ON a.ContentId = c.CONTENT_ID
            LEFT JOIN CONTENT_WORKFLOW_BIND w ON a.ContentId = w.CONTENT_ID
            GROUP BY a.ContentId, w.WORKFLOW_ID, c.SITE_ID
        ) s;

        RAISE NOTICE '%', statuses;

        WITH inserted(id) AS (
            INSERT INTO CONTENT_ITEM ( CONTENT_ID, VISIBLE, STATUS_TYPE_ID, LAST_MODIFIED_BY, NOT_FOR_REPLICATION)
            SELECT a.ContentId, $2, s.StatusId, $3, true
            from unnest(articles) a(ArticleId, ContentId) inner join unnest(statuses) s(ContentId, StatusId) on a.ContentId = s.ContentId
            order by a.ArticleId desc
            RETURNING content_item_id
        ) select array_agg(id) from inserted into ids;

        return query select old_id::int, new_id::int, content_id::int from unnest(ids, articles) x(new_id, old_id, content_id);

END;
$$;
CREATE OR REPLACE PROCEDURE public.qp_before_content_item_version_delete(
	version_ids integer[])
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	BEGIN
	    IF version_ids IS NOT NULL THEN
			delete from content_item_status_history where content_item_version_id = ANY(version_ids)
			and system_status_type_id = 2;
			delete from item_to_item_version where content_item_version_id = ANY(version_ids);
			delete from version_content_data where content_item_version_id = ANY(version_ids);
        END IF;
	END;
$BODY$;

ALTER PROCEDURE public.qp_before_content_item_version_delete(integer[])
    OWNER TO postgres;



CREATE OR REPLACE PROCEDURE public.qp_content_frontend_views_create(
	cid numeric,
	is_new boolean DEFAULT false
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    sql text;
	    new text;
	    cond text;
	BEGIN
        new := case when is_new then '_new' else '' end;
        cond := case when is_new then 'visible and not archive' else 'visible = 1 and archive = 0' end;
        sql := 'create or replace view content_%s_live%s as
               select * from content_%s%s where %s
               and status_type_id in ( select status_type_id from status_type where status_type_name = ''Published'')';
	     sql := format(sql, cid, new, cid, new, cond);
	    execute sql;

         sql := 'create or replace view content_%s_stage%s as
               select * from content_%s_united%s where %s';
	     sql := format(sql, cid, new, cid, new, cond);
	     execute sql;
	END;
$BODY$;

alter procedure qp_content_frontend_views_create(numeric, boolean) owner to postgres;


CREATE OR REPLACE PROCEDURE public.qp_content_frontend_views_drop(
	cid numeric,
	is_new boolean DEFAULT false
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    sql text;
	    new text;
	BEGIN
        new := case when is_new then '_new' else '' end;
        sql := 'drop view if exists content_%s_live%s';
	    sql := format(sql, cid, new);
	    execute sql;

        sql := 'drop view if exists content_%s_stage%s';
	    sql := format(sql, cid, new);
	    execute sql;
	END;
$BODY$;

alter procedure qp_content_frontend_views_drop(numeric, boolean) owner to postgres;

CREATE OR REPLACE PROCEDURE public.qp_content_new_views_create(
    cid numeric
)

LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    sql text;
	    attrs content_attribute[];
	    attr content_attribute;
	    field text;
	    fields text[];
	    do_cast boolean;
	    field_template text;
	    type_name text;
	    is_user_query boolean;
	BEGIN
	    fields := ARRAY[
	        'coalesce(CONTENT_ITEM_ID::int, 0) as CONTENT_ITEM_ID',
	        'coalesce(STATUS_TYPE_ID::int, 0) as STATUS_TYPE_ID',
	        'coalesce(VISIBLE::int::boolean, false) as VISIBLE',
	        'coalesce(ARCHIVE::int::boolean, false) as ARCHIVE',
	        'CREATED',
	        'MODIFIED',
	        'coalesce(LAST_MODIFIED_BY::int, 0) as LAST_MODIFIED_BY'
	    ];
        is_user_query := virtual_type = 3 from content where content_id = cid;
        attrs := array_agg(ca.* order by attribute_order) from content_attribute ca where content_id = cid;
	    if attrs is not null THEN
            foreach attr in ARRAY attrs
            loop
                do_cast := true;
                field_template = 'cast(%s as %s) as %s';
                field := '"' || lower(attr.attribute_name) || '"';
                if attr.attribute_type_id = 2 and attr.attribute_size = 0 and attr.is_long then
                    type_name := 'bigint';
                elseif attr.attribute_type_id = 11 or attr.attribute_type_id = 13
                    or attr.attribute_type_id = 2 and attr.attribute_size = 0 and not attr.is_long then
                    type_name := 'int';
                elseif attr.attribute_type_id = 2 and attr.attribute_size <> 0 and not attr.is_long then
                    type_name := 'float';
                elseif attr.attribute_type_id = 2 and attr.attribute_size <> 0 and attr.is_long then
                    type_name := format('decimal(18, %s)', attr.attribute_size);
                elseif attr.attribute_type_id = 3 then
                    type_name := 'boolean';
                    field_template := 'cast(coalesce(%s::int, 0) as %s) as %s';
                elseif attr.attribute_type_id = 4 then
                    type_name := 'date';
                elseif attr.attribute_type_id = 5 then
                    type_name := 'time';
                elseif attr.attribute_type_id in (9,10) then
                    type_name := 'text';
                else
                    do_cast := false;
                end if;

                if do_cast then
                    field := format(field_template, field, type_name, field);
                end if;
                fields := array_append(fields, field);
            end loop;
        end if;


        sql := 'create or replace view content_%s_new as select %s from content_%s';
	    sql := format(sql, cid, array_to_string(fields, ', '), cid);
	    raise notice '%', sql;
	    execute sql;

	    if is_user_query then
            sql := 'create or replace view content_%s_united_new as select %s from content_%s_united';
            sql := format(sql, cid, array_to_string(fields, ', '), cid);
            raise notice '%', sql;
            execute sql;
        else
            sql := 'create or replace view content_%s_async_new as select %s from content_%s_async';
            sql := format(sql, cid, array_to_string(fields, ', '), cid);
            raise notice '%', sql;
            execute sql;

            call qp_content_united_view_create(cid, true);
        end if;

	    call qp_content_frontend_views_create(cid, true);

	END;
$BODY$;

alter procedure qp_content_new_views_create(numeric) owner to postgres;



CREATE OR REPLACE PROCEDURE public.qp_content_new_views_drop(
    cid numeric
)

LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    sql text;
	    is_user_query boolean;
	BEGIN
	    call qp_content_frontend_views_drop(cid, true);

        is_user_query := virtual_type = 3 from content where content_id = cid;
	    if is_user_query then
            sql := format('drop view if exists content_%s_united_new', cid);
            raise notice '%', sql;
            execute sql;
        else
            call qp_content_united_view_drop(cid, true);

            sql := format('drop view if exists content_%s_async_new', cid);
            raise notice '%', sql;
            execute sql;
        end if;

	    sql := format('drop view if exists content_%s_new', cid);
	    raise notice '%', sql;
	    execute sql;


	END;
$BODY$;

alter procedure qp_content_new_views_drop(numeric) owner to postgres;



CREATE OR REPLACE PROCEDURE public.qp_content_united_view_create(
	cid numeric,
	is_new boolean DEFAULT false
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  new text;
	  sql text;
	BEGIN
        new := case when is_new then '_new' else '' end;
        sql := 'create or replace view content_%s_united%s as
	           select c1.* from content_%s%s c1
	           left join content_%s_async%s c2 on c1.content_item_id = c2.content_item_id
	           where c2.content_item_id is null
	           union all
	           select * from content_%s_async%s';
	    sql := format(sql, cid, new, cid, new, cid, new, cid, new);
	    execute sql;
	END;
$BODY$;

alter procedure qp_content_united_view_create(numeric, boolean) owner to postgres;



CREATE OR REPLACE PROCEDURE public.qp_content_united_view_drop(
	cid numeric,
	is_new boolean DEFAULT false
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  new text;
	  sql text;
	BEGIN
        new := case when is_new then '_new' else '' end;
        sql := 'drop view if exists content_%s_united%s';
	    sql := format(sql, cid, new);
	    execute sql;
	END;
$BODY$;

alter procedure qp_content_united_view_drop(numeric, boolean) owner to postgres;



CREATE OR REPLACE PROCEDURE public.qp_content_user_query_view_recreate(
    cid integer
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    view_sql text;
	    alt_view_sql text;
	    sql text;

	BEGIN
	    select query, coalesce(alt_query, query)
	    into view_sql, alt_view_sql from content where content_id = cid;

	    sql := format('drop view if exists content_%s', cid);
	    execute sql;
	    sql := format('create view content_%s as %s', cid, view_sql);
	    execute sql;

	    sql := format('drop view if exists content_%s_united', cid);
	    execute sql;
	    sql := format('create view content_%s_united as %s', cid, alt_view_sql);
	    execute sql;
    END;
$BODY$;

alter procedure qp_content_user_query_view_recreate(integer) owner to postgres;
CREATE OR REPLACE function public.qp_count_duplicates(content_id int, field_ids int[], ids int[] default null,
                                                       include_archive boolean default false) returns int
LANGUAGE 'plpgsql'

AS $BODY$
    DECLARE
        attr_names text[];
        attrs text;
        condition text;
        sql text;
        result int;
    BEGIN
        attr_names := array_agg('"' || lower(attribute_name) || '"') from content_attribute where attribute_id = ANY(field_ids);
        raise notice '%', attr_names;

        if ids is not null and array_length(ids, 1) > 0 then
            condition := 'and c.content_item_id = ANY($1)';
        else
            condition := '';
        end if;

        if not include_archive then
            condition := condition || ' and c.archive = 0';
        end if;

        if attr_names is not null then
            attrs := array_to_string(attr_names, ',');
            sql := 'select coalesce(sum(c0.cnt), 0) from (select COUNT(*) as cnt
                  from content_%s_united c where 1=1 %s group by %s having COUNT(*) > 1) as c0';
            sql := format(sql, content_id, condition, attrs);

        else
            sql := 'select 0 as cnt';
        end if;
        raise notice '%', sql;
        execute sql using ids into result;
        return result;

    END;
$BODY$;


create or replace function qp_default_link_ids(field_id numeric) returns text
    stable
    language plpgsql
as
$$
DECLARE
    result int[];
BEGIN
    result := array_agg(a.article_id) from field_article_bind a where a.field_id = $1;

    if result is null then
        return '';
    else
        return array_to_string(result, ',');
    end if;
END;
$$;

alter function qp_default_link_ids(numeric) owner to postgres;
create or replace procedure qp_delete_items(content_id integer, ids integer[], is_async boolean)
    language plpgsql
as
$$
DECLARE
	  	table_name text;	  
		sql text;
	BEGIN

		table_name := 'content_' || content_id;
		IF is_async THEN
			table_name := table_name || '_async';
		END IF;
		
		sql := FORMAT('delete from %s where content_item_id = ANY($1)', table_name);
		RAISE NOTICE '%', sql;
		execute sql using ids;
	END;
$$;

alter procedure qp_delete_items(integer, integer[], boolean) owner to postgres;


-- PROCEDURE: public.qp_delete_link_table_item(numeric, numeric, link[], boolean, boolean, boolean)

-- DROP PROCEDURE public.qp_delete_link_table_item(numeric, numeric, link[], boolean, boolean, boolean);

CREATE OR REPLACE PROCEDURE public.qp_delete_link_table_item(
	link_id numeric,
	content_id numeric,
	links link[],
	is_async boolean,
	use_reverse_table boolean,
	reverse_fields boolean)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  link_table_name text;
	  condition text;
	  sql text;
	  fsql text;
	BEGIN
		link_table_name := 'item_link_' || $1::text;
		condition := CASE WHEN reverse_fields THEN 'src.id = il.linked_id and src.linked_id = il.id' ELSE 'src.id = il.id and src.linked_id = il.linked_id' END;		
		
		IF is_async THEN
			link_table_name := link_table_name || '_async';
		END IF;
		
		IF use_reverse_table THEN
			link_table_name := link_table_name || '_rev';
		END IF;
		
		sql := 'delete from %s src using unnest($1) il where %s';
		fsql := format(sql, link_table_name, condition);
		EXECUTE fsql USING links;
		RAISE NOTICE 'Query: %', fsql;

	END;
$BODY$;

alter procedure qp_delete_link_table_item(numeric, numeric, link[], boolean, boolean, boolean) owner to postgres;


create or replace function qp_get_aggregated_ids(id integer, classifier_ids integer[], content_ids integer[], is_live boolean)
returns integer[]
    stable
    language plpgsql
as
$$
DECLARE
    result int[];
    agg_attrs content_attribute[];
    attr content_attribute;
    queries text[];
    sql text;
    live_suffix text;
BEGIN

    sql := '';
    live_suffix := case when is_live then '' else '_united' end;

    agg_attrs := array_agg(ca.*) from content_attribute ca
    where ca.classifier_attribute_id = ANY($2) and ca.content_id = ANY($3);

    if agg_attrs is not null then
        foreach attr in array agg_attrs
        loop
          sql := 'select content_item_id from content_%s%s where "%s" = $1';
          sql := format(sql, attr.content_id, live_suffix, lower(attr.attribute_name));
          queries := array_append(queries, sql);
        end loop;
        sql := array_to_string(queries, ' union all ');
        sql := 'select array_agg(u.content_item_id) from (' || sql || ') u';
        raise notice '%', sql;
        execute sql using $1 into result;
    end if;

    return coalesce(result, ARRAY[]::int[]);
END;
$$;

alter function qp_get_aggregated_ids(int, int[], int[], boolean) owner to postgres;
-- FUNCTION: public.qp_get_article_title_func(numeric, numeric)

-- DROP FUNCTION public.qp_get_article_title_func(numeric, numeric);

CREATE OR REPLACE FUNCTION public.qp_get_article_title_func(
	content_item_id numeric,
	content_id numeric)
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    STABLE
AS $BODY$
DECLARE
    result text;
BEGIN
    result := null;

	select data into result
	from content_data cd
	where
	cd.content_item_id = $1
	and attribute_id in (
		select ca.attribute_id from content_attribute ca
		where ca.content_id = $2 and attribute_name = public.qp_get_display_field($2, true)
	);

	return result;
END;
$BODY$;

ALTER FUNCTION public.qp_get_article_title_func(numeric, numeric)
    OWNER TO postgres;

create or replace function qp_get_base_field(field_id integer, article_id integer)
returns integer
    stable
    language plpgsql
as
$$
DECLARE
    virtual_type int;
    result int;
    attr content_attribute;
BEGIN

    attr := row(ca.*) from content_attribute ca where ca.attribute_id = $1;
    virtual_type := c.virtual_type from content c where c.content_id = attr.content_id;

    if virtual_type = 1 then
		result := ca.persistent_attr_id from content_attribute ca where attribute_id = attr.attribute_id;
    elseif virtual_type = 2 then
		result := ua.union_attr_id from union_attrs ua inner join content_attribute ca on ua.union_attr_id = ca.attribute_id where virtual_attr_id = attr.attribute_id and ca.content_id in (select content_id from content_item where content_item_id = $2);
    elseif virtual_type = 3 then
	    result := ca.attribute_id from content_attribute ca where attribute_name = attr.attribute_name and content_id in (select real_content_id from user_query_contents where virtual_content_id = attr.content_id);
    else
        result := attr.attribute_id;
    end if;

    return coalesce(result, attr.attribute_id);


END;
$$;

alter function qp_get_base_field(int, int) owner to postgres;
-- FUNCTION: public.qp_get_display_field(numeric, boolean)

-- DROP FUNCTION public.qp_get_display_field(numeric, boolean);

CREATE OR REPLACE FUNCTION public.qp_get_display_field(
	content_id numeric,
	with_relation_field boolean DEFAULT false)
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    STABLE
AS $BODY$
DECLARE
    result text;
BEGIN
    SELECT attribute_name into result
FROM (
         select attribute_name,
                CASE
                    WHEN attribute_type_id in (9, 10)
                        THEN CASE WHEN with_relation_field THEN 1 ELSE 0 END
                    WHEN attribute_type_id = 13 or IS_CLASSIFIER or
                         attribute_type_id = 11 AND NOT with_relation_field
                        THEN -1
                    ELSE 1
                    END AS attribute_priority
         from unnest(public.qp_get_display_fields(content_id, with_relation_field))
         order by view_in_list desc, attribute_priority desc, attribute_order asc
     ) a
LIMIT 1;

if result is null then
	return 'content_item_id';
else
	return result;
end if;

END;
$BODY$;

ALTER FUNCTION public.qp_get_display_field(numeric, boolean)
    OWNER TO postgres;

-- FUNCTION: public.qp_get_display_fields(numeric, boolean)

-- DROP FUNCTION public.qp_get_display_fields(numeric, boolean);

CREATE OR REPLACE FUNCTION public.qp_get_display_fields(
	content_id numeric,
	with_relation_field boolean DEFAULT false)
    RETURNS content_attribute[]
    LANGUAGE 'plpgsql'

    COST 100
    STABLE 
AS $BODY$
DECLARE
    result content_attribute[];
BEGIN
    result := ARRAY(
            SELECT ca
            FROM (
                     select ca,
                            CASE
                                WHEN attribute_type_id in (9, 10)
                                    THEN CASE WHEN with_relation_field THEN 1 ELSE 0 END
                                WHEN attribute_type_id = 13 or IS_CLASSIFIER or
                                     attribute_type_id = 11 AND NOT with_relation_field
                                    THEN -1
                                ELSE 1
                                END AS attribute_priority
                     from content_attribute ca
                     where ca.content_id = $1
                 ) i
            where attribute_priority >= 0
        );
    return result;
END;
$BODY$;

ALTER FUNCTION public.qp_get_display_fields(numeric, boolean)
    OWNER TO postgres;

-- FUNCTION: public.qp_get_hash(text, bigint)

-- DROP FUNCTION public.qp_get_hash(text, bigint);

CREATE OR REPLACE FUNCTION public.qp_get_hash(
	text,
	bigint)
    RETURNS bytea
    LANGUAGE 'sql'

    COST 100
    IMMUTABLE STRICT 
AS $BODY$
    SELECT digest($1 || $2::text, 'sha1')
$BODY$;

ALTER FUNCTION public.qp_get_hash(text, bigint)
    OWNER TO postgres;

create or replace function qp_get_user_weight(user_id int, workflow_id int)
returns int
    stable
    language plpgsql
as
$$
DECLARE
    result int;
BEGIN

    result := max(st.weight) FROM workflow_rules wr
		INNER JOIN status_type st ON wr.successor_status_id = st.status_type_id
		WHERE wr.workflow_id = $2 and wr.user_id = $1;

    if result is null then
		WITH RECURSIVE groups(group_id) AS
		(
		    select ug.group_id from user_group_bind ug where ug.user_id = $1
		    UNION ALL
		    select gg.parent_group_id from group_to_group gg inner join groups g on g.group_id = gg.child_group_id
		 )
		select max(st.weight) into result FROM workflow_rules wr
		INNER JOIN status_type st ON wr.successor_status_id = st.status_type_id
		WHERE wr.workflow_id = $2 and wr.group_id in (select group_id from groups);

	end if;

	return result;

END;
$$;


ALTER FUNCTION public.qp_get_user_weight(int, int)
    OWNER TO postgres;

-- FUNCTION: public.qp_get_version_data(numeric, numeric)

-- DROP FUNCTION public.qp_get_version_data(numeric, numeric);

CREATE OR REPLACE FUNCTION public.qp_get_version_data(
	attribute_id numeric,
	version_id numeric)
    RETURNS text
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE
AS $BODY$
declare result text;
begin
	result := coalesce(vcd.blob_data, vcd.data) from version_content_data vcd where vcd.attribute_id = $1 and vcd.content_item_version_id = $2;
	return result;
end;
$BODY$;

ALTER FUNCTION public.qp_get_version_data(numeric, numeric)
    OWNER TO postgres;

-- PROCEDURE: public.qp_insert_link_table_item(numeric, numeric, link[], boolean, boolean, boolean)

-- DROP PROCEDURE public.qp_insert_link_table_item(numeric, numeric, link[], boolean, boolean, boolean);

CREATE OR REPLACE PROCEDURE public.qp_insert_link_table_item(
	link_id numeric,
	content_id numeric,
	links link[],
	is_async boolean,
	use_reverse_table boolean,
	reverse_fields boolean)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  link_table_name text;
	  source_name text;
	  is_self boolean;
	  rev_fields text;
	  condition text;
	  sql text;
	  fsql text;
	  links2 link[];
	  t link;
	BEGIN
		link_table_name := 'item_link_' || $1::text;
		is_self := l_content_id = r_content_id from content_to_content cc where cc.link_id = $1;
		source_name := CASE WHEN is_async THEN 'item_link_async' ELSE 'item_link' END;
		rev_fields := CASE WHEN reverse_fields THEN 'il.linked_id, il.id' ELSE 'il.id, il.linked_id' END;
		condition := CASE WHEN reverse_fields THEN 'il2.id = il.linked_id and il2.linked_id = il.id' ELSE 'il2.id = il.id and il2.linked_id = il.linked_id' END;		
		
		IF is_async THEN
			link_table_name := link_table_name || '_async';
		END IF;
		
		IF use_reverse_table THEN
			link_table_name := link_table_name || '_rev';
		END IF;
		
		links2 := array(
			select il from unnest(links) il inner join content_item ci on il.id = ci.CONTENT_ITEM_ID where ci.CONTENT_ID = $2
		);
		
		foreach t in array links2
		loop
			raise notice '%', t;																  
		end loop;	
		
		sql := 'insert into %s select %s from unnest($1) il where not exists(select * from %s il2 where %s)';
		fsql := format(sql, link_table_name, rev_fields, link_table_name, condition);
		EXECUTE fsql USING links2;
		RAISE NOTICE 'Query: %', fsql;
		
		
		IF is_self THEN
	    	sql := 'update %s i set is_self = true from unnest($1) i2 where i.link_id = $2 and i.item_id = i2.id and i.linked_item_id = i2.linked_id';
			fsql := format(sql, source_name, source_name);			
			EXECUTE fsql USING links2, link_id;
			RAISE NOTICE 'Query: %', fsql;
		END IF;
		
		IF use_reverse_table and not is_self THEN
	    	sql := 'update %s i set is_rev = true from unnest($1) i2 where i.link_id = $2 and i.item_id = i2.id and i.linked_item_id = i2.linked_id';
			fsql := format(sql, source_name, source_name);			
			EXECUTE fsql USING links2, link_id;
			RAISE NOTICE 'Query: %', fsql;
		END IF;

	END;
$BODY$;

alter procedure qp_insert_link_table_item(numeric, numeric, link[], boolean, boolean, boolean) owner to postgres;

create or replace function qp_is_date(s character varying) returns boolean
    language plpgsql
as
$$
begin
  perform s::date;
  return true;
exception when others then
  return false;
end;
$$;

alter function qp_is_date(varchar) owner to postgres;


create or replace function qp_is_numeric(text) returns boolean
    immutable
    strict
    language plpgsql
as
$$
DECLARE x NUMERIC;
BEGIN
    x = $1::NUMERIC;
    RETURN TRUE;
EXCEPTION WHEN others THEN
    RETURN FALSE;
END;
$$;

alter function qp_is_numeric(text) owner to postgres;


create or replace function qp_link_ids(link_id integer, id integer, is_stage boolean) returns text
    stable
    language plpgsql
as
$$
DECLARE
    result int[];
BEGIN
    if is_stage then
        result := array_agg(i.linked_item_id) from item_link_united i where i.link_id = $1 and i.item_id = $2;
    else
        result := array_agg(i.linked_item_id) from item_link i where i.link_id = $1 and i.item_id = $2;
    end if;

    if result is null then
        return '';
    else
        return array_to_string(result, ',');
    end if;
END;
$$;

alter function qp_link_ids(int, int, boolean) owner to postgres;

create or replace function qp_link_titles(
    link_id integer, id integer, display_attribute_id integer, maxlength integer
) returns text
    stable
    language plpgsql
as
$$
DECLARE
    result text[];
BEGIN

    	result := array_agg(
    	    case when char_length(d.title) > maxlength then left(d.title, maxlength) || '...'
    	    else d.title end
    	) from
        (select coalesce(data, blob_data, '') as title from content_data
    	where attribute_id = $3 and content_item_id in (
    	    select linked_item_id from item_link i where i.link_id = $1 and i.item_id = $2
    	)) d;

    if result is null then
        return '';
    else
        return array_to_string(result, ',');
    end if;
END;
$$;

alter function qp_link_titles(int, int, int, int) owner to postgres;


CREATE OR REPLACE PROCEDURE public.qp_link_view_create(
	id int
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    link content_to_content;
        view_name text;
	    view_name2 text;
	    view_name3 text;
	    view_name4 text;
        link_table text;
	    link_table_async text;
	    link_table_rev text;
	    link_table_async_rev text;
	    sql text;
	    sql2 text;

	BEGIN
	    select cc.* into link from content_to_content cc where cc.link_id = id;
	    view_name := 'link_' || id;
	    view_name2 := 'link_' || id || '_united';
	    view_name3 := 'item_link_' || id || '_united';
	    view_name4 := 'item_link_' || id || '_united_rev';

	    link_table := 'item_link_' || id;
	    link_table_async := 'item_link_' || id || '_async';
	    link_table_rev := 'item_link_' || id || '_rev';
	    link_table_async_rev := 'item_link_' || id || '_async_rev';

        sql2 := 'CREATE OR REPLACE VIEW %s AS select id, linked_id from %s il
             where not exists (select * from content_item_splitted cis where il.id = cis.CONTENT_ITEM_ID)
             union all SELECT id, linked_id from %s ila';

        sql := 'CREATE OR REPLACE VIEW %s AS select il.item_id, il.linked_item_id from %s il
               inner join content_item ci on il.item_id = ci.CONTENT_ITEM_ID
               where CONTENT_ID = %s  and link_id = %s';

	    execute format(sql, view_name, 'item_link', link.l_content_id, link.link_id);
	    execute format(sql, view_name2, 'item_link_united', link.l_content_id, link.link_id);
	    execute format(sql2, view_name3, link_table, link_table_async);
	    execute format(sql2, view_name4, link_table_rev, link_table_async_rev);

	END;
$BODY$;

alter procedure qp_link_view_create(int) owner to postgres;

CREATE OR REPLACE PROCEDURE public.qp_link_view_drop(
	id int
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
        view_name text;
	    view_name2 text;
	    view_name3 text;
	    view_name4 text;
	    sql text;

	BEGIN
	    view_name := 'link_' || id;
	    view_name2 := 'link_' || id || '_united';
	    view_name3 := 'item_link_' || id || '_united';
	    view_name4 := 'item_link_' || id || '_united_rev';

        sql := 'drop VIEW IF EXISTS %s ';

	    execute format(sql, view_name);
	    execute format(sql, view_name2);
	    execute format(sql, view_name3);
	    execute format(sql, view_name4);

	END;
$BODY$;

alter procedure qp_link_view_drop(int) owner to postgres;
create or replace function qp_m2o_titles(
    id integer, field_related_id integer, related_attribute_id integer, maxlength integer
) returns text
    stable
    language plpgsql
as
$$
DECLARE
    result text[];
BEGIN

    	result := array_agg(
    	    case when char_length(d.title) > maxlength then left(d.title, maxlength) || '...'
    	    else d.title end
    	) from
        (
            select coalesce(data, blob_data, '') as title from CONTENT_DATA where attribute_id = $2
	        and content_item_id in (select content_item_id from content_data where ATTRIBUTE_ID = $3 and o2m_data = $1)
    	) d;

    if result is null then
        return '';
    else
        return array_to_string(result, ',');
    end if;
END;
$$;

alter function qp_m2o_titles(int, int, int, int) owner to postgres;

-- PROCEDURE: public.qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[])

-- DROP PROCEDURE public.qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[]);

CREATE OR REPLACE PROCEDURE public.qp_update_items_flags(
	content_id integer,
	ids integer[],
	is_async boolean)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  	table_name text;	  
		sql text;
	BEGIN

		table_name := 'content_' || content_id;
		IF is_async THEN
			table_name := table_name || '_async';
		END IF;
		
	    sql := 'update %s base set visible = ci.visible, archive = ci.archive from content_item ci
		 where base.content_item_id = ci.content_item_id and ci.content_item_id = ANY($1)';
		
		sql := FORMAT(sql, table_name);
		RAISE NOTICE '%', sql;
		execute sql using ids;
	END;
$BODY$;

alter procedure qp_update_items_flags(integer, integer[], boolean) owner to postgres;
-- PROCEDURE: public.qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[])

-- DROP PROCEDURE public.qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[]);

CREATE OR REPLACE PROCEDURE public.qp_update_items_with_content_data_pivot(
	content_id integer,
	ids integer[],
	is_async boolean,
	attr_ids integer[])
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  table_name text;
	  attributes content_attribute[];
	  attr_names text[];
	  cross_tab text;
	  attrs_result text[];
	  res text;
	  attrs_select text[];
	  sel text;
	  attrs_update text[];
	  upd text;
	BEGIN

		table_name := 'content_' || content_id;
		IF is_async THEN
			table_name := table_name || '_async';
		END IF;

		IF attr_ids IS NULL THEN
			attributes := array_agg(ca.* order by ca.attribute_name) from CONTENT_ATTRIBUTE ca
				where ca.content_id = $1;
		ELSE
			attributes := array_agg(ca.* order by ca.attribute_name) from CONTENT_ATTRIBUTE ca
				where ca.content_id = $1 AND attribute_id = ANY(attr_ids);
		END IF;
		attr_ids := array_agg(attribute_id) from unnest(attributes) a;

		IF array_length(attributes, 1) > 0 THEN

			attr_names := array_agg(lower(a.attribute_name)) from unnest(attributes) a;

			attrs_update := array_agg(FORMAT('"%s" = pt."%s"', unnest, unnest)) from unnest(attr_names);
			upd := array_to_string(attrs_update, ', ');

			attrs_result := array_agg(FORMAT('"%s" TEXT', unnest)) from unnest(attr_names);
			attrs_result := array_prepend('content_item_id NUMERIC', attrs_result);
			res := array_to_string(attrs_result, ', ');

			attrs_select := array_agg(FORMAT('"%s"::%s', b.name, b.type)) from (
				select lower(a.attribute_name) as name, CASE WHEN a.attribute_type_id in (2,3,11,13) THEN
					'numeric'
				WHEN a.attribute_type_id in (4,5,6) THEN
					'timestamp with time zone'
				ELSE
					'text'
				END AS type from unnest(attributes) a
			) b;
			attrs_select := array_prepend('content_item_id', attrs_select);
			sel := array_to_string(attrs_select, ', ');

			cross_tab := 'update %s base set %s from (
			SELECT %s FROM crosstab(''
			select content_item_id, lower(ca.attribute_name),
			case when ca.attribute_type_id in (9, 10) then coalesce(cd.data, cd.blob_data)
			else qp_correct_data(cd.data::text, ca.attribute_type_id, ca.attribute_size, ca.default_value)::text
			end as value from content_data cd
			inner join content_attribute ca on cd.attribute_id = ca.attribute_id
			where content_item_id in (%s) and cd.attribute_id in (%s)
			order by 1,2
			'') AS final_result(%s)) pt where pt.content_item_id = base.content_item_id;';

			cross_tab := FORMAT(cross_tab, table_name, upd, sel, array_to_string(ids, ', '), array_to_string(attr_ids, ', '), res);
			RAISE NOTICE '%', cross_tab;
			execute cross_tab;
		END IF;
	END;
$BODY$;

alter procedure qp_update_items_with_content_data_pivot(integer, integer[], boolean, integer[]) owner to postgres;

create or replace procedure qp_upsert_items(content_id integer, ids integer[], delayed_ids integer[], none_id integer, is_async boolean)
    language plpgsql
as
$$
DECLARE
	  	table_name text;	  
		sql text;
	BEGIN

		table_name := 'content_' || content_id;
		IF is_async THEN
			table_name := table_name || '_async';
		END IF;
		
	    sql := 'update %s base set visible = ci.visible, archive = ci.archive,
    			modified = ci.modified, last_modified_by = ci.last_modified_by, status_type_id = ci.status_type_id
			 	from content_item ci
		 		where base.content_item_id = ci.content_item_id and ci.content_item_id = ANY($1)';
		
		sql := FORMAT(sql, table_name);
		RAISE NOTICE '%', sql;
		execute sql using ids;
		
		sql := 'insert into %s (content_item_id, created, modified, last_modified_by, status_type_id, visible, archive)
    			select ci.content_item_id, ci.created, ci.modified, ci.last_modified_by, 
    			case when i2.id is not null then $3 else ci.status_type_id end as status_type_id, 
    			ci.visible, ci.archive 
				from content_item ci left join %s base on ci.content_item_id = base.content_item_id 
    			inner join unnest($1) i(id) on ci.content_item_id = i.id
				left join unnest($2) i2(id) on ci.content_item_id = i2.id
    			where base.content_item_id is null';
		sql := FORMAT(sql, table_name, table_name);
		RAISE NOTICE '%', sql;
		execute sql using ids, delayed_ids, none_id;				
	END;
$$;

alter procedure qp_upsert_items(integer, integer[], integer[], integer, boolean) owner to postgres;



CREATE OR REPLACE PROCEDURE public.qp_archive(
	ids int[],
	flag boolean,
	last_modified_by int,
	with_aggregated boolean default true)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
    BEGIN
        if with_aggregated then
            ids := qp_aggregated_and_self(ids);
        end if;

        
        update content_item set archive = $2::int, modified = now(), last_modified_by = $3 where content_item_id = ANY(ids);
        update content_item set locked_by = null, locked = null where content_item_id = ANY(ids);
	END;
$BODY$;

alter procedure qp_archive(int[], boolean, int, boolean) owner to postgres;






-- FUNCTION: public.qp_authenticate(text, text, boolean, boolean)

-- DROP FUNCTION public.qp_authenticate(text, text, boolean, boolean);

CREATE OR REPLACE FUNCTION public.qp_authenticate(
	login text,
	password text,
	use_nt_login boolean DEFAULT false,
	check_admin_access boolean DEFAULT false)
    RETURNS users
    LANGUAGE 'plpgsql'

    COST 100
    VOLATILE
AS $BODY$
	DECLARE
		user_row users%ROWTYPE;
	BEGIN
		IF use_nt_login THEN
			select * from users where nt_login = $1 into user_row;
			IF user_row is null THEN
				RAISE EXCEPTION 'Your Windows account is not mapped to any QP user' using errcode = 'AUTH5';
			ELSEIF user_row.disabled = 1 THEN
				RAISE EXCEPTION 'Account is disabled. Contact <@MailForErrors@>' using errcode = 'AUTH2';
			ELSEIF user_row.auto_login = 0 THEN
				RAISE EXCEPTION 'Auto login option is switched off for your account. Contact <@MailForErrors@>' using errcode = 'AUTH6';
			END IF;
		ELSE
			select * from users u where u.login = $1 into user_row;
			IF user_row is null THEN
				RAISE EXCEPTION 'Login doesn''t exist' using errcode = 'AUTH1';
			ELSEIF user_row.disabled = 1 THEN
				RAISE EXCEPTION 'Account is disabled. Contact <@MailForErrors@>' using errcode = 'AUTH2';
			ELSEIF qp_get_hash(password, user_row.salt) <> user_row.hash THEN
				RAISE EXCEPTION 'Password is incorrect' using errcode = 'AUTH3';
			ELSEIF check_admin_access THEN
				IF NOT EXISTS(select * from USER_GROUP_BIND where GROUP_ID = 1 and USER_ID = user_row.user_id) THEN
					RAISE EXCEPTION 'Account is not a member of Administrators group' using errcode = 'AUTH4';
				END IF;
			END IF;
		END IF;
		RETURN user_row;
	END;

$BODY$;

ALTER FUNCTION public.qp_authenticate(text, text, boolean, boolean)
    OWNER TO postgres;


CREATE OR REPLACE PROCEDURE public.qp_batch_delete_contents(site_id int, count_to_del int DEFAULT 20)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    ids int[];
	BEGIN
        ids := array_agg(c.content_id) from content c where c.site_id = $1 limit $2;
        if ids is not null then

	        delete from union_contents
	        where virtual_content_id = ANY(ids) or union_content_id = ANY(ids) or master_content_id = ANY(ids);

	        delete from union_attrs where virtual_attr_id in (
	            select attribute_id from content_attribute where content_id = ANY(ids)
	        );

	        delete from user_query_contents
	        where virtual_content_id = ANY(ids) or real_content_id = ANY(ids);

	        delete from user_query_attrs
	        where virtual_content_id = ANY(ids);

	        delete from user_query_attrs where user_query_attr_id in (
	            select attribute_id from content_attribute where content_id = ANY(ids)
	        );

	        delete from content where content_id = ANY(ids);
	    end if;

    END;
$BODY$;

CREATE OR REPLACE PROCEDURE public.qp_before_content_delete(
	ids integer[])
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	    item_ids integer[];
	    attr_ids integer[];
	    link_ids integer[];
	BEGIN
	    IF ids IS NOT NULL THEN

	        CREATE TEMP TABLE disable_td_delete_item(id numeric);

	        item_ids := array_agg(ci.content_item_id) from content_item ci where content_id = ANY(ids);
	        call qp_before_content_item_delete(item_ids);

	        delete from content_item where content_id = ANY(ids);

	        DROP TABLE disable_td_delete_item;

	        attr_ids := array_agg(attribute_id) from content_attribute ca where content_id = ANY(ids);
	        IF attr_ids IS NOT NULL THEN
                update content_attribute set related_attribute_id = NULL where related_attribute_id = ANY(attr_ids);

                update content_attribute set CLASSIFIER_ATTRIBUTE_ID = NULL, AGGREGATED = false
                where CLASSIFIER_ATTRIBUTE_ID = ANY(attr_ids);

                delete from content_constraint_rule where attribute_id = any(attr_ids);

                delete from content_attribute where BACK_RELATED_ATTRIBUTE_ID = any(attr_ids);

                update content_attribute set TREE_ORDER_FIELD = NULL where TREE_ORDER_FIELD = ANY(attr_ids);
            END IF;

	        link_ids := array_agg(link_id) from content_to_content cc
	            where cc.l_content_id = ANY(ids) or cc.r_content_id = ANY(ids);

	        if link_ids IS NOT NULL THEN
    	        call qp_before_content_to_content_delete(link_ids);

            end if;
            update content_attribute set link_id = null where link_id = ANY(link_ids);

            delete from content_to_content where l_content_id = ANY(ids) or r_content_id = ANY(ids);

            delete from container where content_id = ANY(ids);
            delete from content_form where content_id = ANY(ids);
            delete from user_default_filter where content_id = ANY(ids);
            delete from content_tab_bind where content_id = ANY(ids);
            delete from action_content_bind where content_id = ANY(ids);

        END IF;
	END;
$BODY$;

ALTER PROCEDURE public.qp_before_content_delete(integer[])
    OWNER TO postgres;

CREATE OR REPLACE PROCEDURE public.qp_before_content_item_delete(
	ids integer[])
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	    version_ids integer[];
	BEGIN
	    IF ids IS NOT NULL THEN
	        version_ids := array_agg(civ.content_item_version_id)
	            from content_item_version civ where content_item_id = ANY(ids);
	        call qp_before_content_item_version_delete(version_ids);

            delete from waiting_for_approval where content_item_id = ANY(ids);
            delete from child_delays where child_id = ANY(ids);
            delete from content_item_version where content_item_id = ANY(ids);
            delete from item_to_item_version where linked_item_id = ANY(ids);
            delete from item_link where item_id = ANY(ids) or linked_item_id = ANY(ids);
            delete from item_link_async where item_id = ANY(ids) or linked_item_id = ANY(ids);
            delete from field_article_bind where article_id = ANY(ids);
            delete from content_data where content_item_id = ANY(ids);
        END IF;
	END;
$BODY$;

ALTER PROCEDURE public.qp_before_content_item_delete(integer[])
    OWNER TO postgres;

CREATE OR REPLACE PROCEDURE public.qp_before_content_to_content_delete(
	ids integer[])
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	BEGIN
	    IF ids IS NOT NULL THEN
            delete from content_attribute ca where link_id = ANY(ids);
            delete from item_to_item ii where link_id = ANY(ids);
            delete from item_link_async ila where link_id = ANY(ids);
        END IF;
	END;
$BODY$;

ALTER PROCEDURE public.qp_before_content_to_content_delete(integer[])
    OWNER TO postgres;


CREATE OR REPLACE PROCEDURE public.qp_content_united_views_recreate()
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  content_ids int[];
	  cid int;
	BEGIN
	    content_ids := array_agg(content_id) from content where virtual_type = 0;
	    foreach cid in array content_ids
	    loop
            call qp_content_united_view_drop(cid);
            call qp_content_united_view_create(cid);
        end loop;
    END;
$BODY$;

alter procedure qp_content_united_views_recreate() owner to postgres;


CREATE OR REPLACE PROCEDURE public.qp_copy_schedule_to_child_delays(id int)
LANGUAGE 'plpgsql'
AS $BODY$
	DECLARE
	BEGIN
        if exists(select * from content_item_schedule where content_item_id = $1 and freq_type = 2) then
            update content_item_schedule set delete_job = true where not use_service and content_item_id in (
                select child_id from child_delays cd where cd.id = $1
            );
            delete from content_item_schedule where content_item_id in (
                select child_id from child_delays cd where cd.id = $1
            );
            insert into content_item_schedule (
                CONTENT_ITEM_ID, MAXIMUM_OCCURENCES, CREATED, MODIFIED, LAST_MODIFIED_BY, freq_type,
                freq_interval, freq_subday_type, freq_subday_interval, freq_relative_interval, freq_recurrence_factor,
                active_start_date, active_end_date, active_start_time, active_end_time,
                occurences, use_duration, duration, duration_units, DEACTIVATE, DELETE_JOB, USE_SERVICE, start_date, end_date
            )
            select child_id, MAXIMUM_OCCURENCES, now(), now(), LAST_MODIFIED_BY, freq_type,
                freq_interval, freq_subday_type, freq_subday_interval, freq_relative_interval, freq_recurrence_factor,
                active_start_date, active_end_date, active_start_time, active_end_time,
                occurences, use_duration, duration, duration_units, DEACTIVATE, DELETE_JOB, USE_SERVICE, start_date, end_date
            from content_item_schedule cis inner join child_delays cd on cis.content_item_id = cd.id
            where content_item_id = $1;
        end if;
    END;
$BODY$;

ALTER PROCEDURE public.qp_copy_schedule_to_child_delays(int)
    OWNER TO postgres;


create or replace function qp_correct_data(value text, type_id numeric, length numeric, default_value text) returns text
    immutable
    called on null input
    language plpgsql
as
$$
DECLARE
	num numeric(18, 0);
BEGIN
	IF type_id in (1, 7, 8, 12) THEN
		RETURN left(value, length::int);
	ELSEIF type_id in (2, 3, 11) THEN
		IF qp_is_numeric(value) or value is null THEN
			RETURN value;
		ELSEIF qp_is_numeric(default_value) THEN
			RETURN default_value;
		ELSE
			RETURN NULL;
		END IF;			
	ELSEIF type_id in (4, 5, 6) THEN
		IF qp_is_date(value) or value is null THEN
			RETURN value;
		ELSEIF qp_is_date(default_value) THEN
			RETURN default_value;
		ELSE 
			RETURN NULL;
		END IF;			
	ELSE	
		RETURN value;
	END IF;
END;
$$;


alter function qp_correct_data(text, numeric, numeric, text) owner to postgres;


CREATE OR REPLACE PROCEDURE public.qp_create_content_item_access(
	ids numeric[]
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    items content_item[];
	BEGIN
        items := array_agg(row(ci.*)) from content_item ci inner join content c on ci.content_id = c.content_id
        where content_item_id = ANY(ids) and c.allow_items_permission = 1;

        if items is null then
            return;
        end if;

        INSERT INTO content_item_access (content_item_id, user_id, permission_level_id, last_modified_by)
	    SELECT i.content_item_id, i.last_modified_by, 1, 1 FROM unnest(items) i WHERE i.last_modified_by <> 1;

        INSERT INTO content_item_access (content_item_id, user_id, group_id, permission_level_id, last_modified_by)
	    SELECT i.content_item_id, ca.user_id, ca.group_id, ca.permission_level_id, 1
	    FROM content_access AS ca INNER JOIN unnest(items) i ON ca.content_id = i.content_id
		LEFT OUTER JOIN user_group AS g ON g.group_id = ca.group_id
	    WHERE (ca.user_id <> i.last_modified_by or ca.user_id IS NULL)
		AND ((g.shared_content_items = 0 and g.GROUP_ID <> 1) OR g.group_id IS NULL) AND ca.propagate_to_items = 1;


	  INSERT INTO content_item_access
		(content_item_id, group_id, permission_level_id, last_modified_by)
	  SELECT DISTINCT
		i.content_item_id, g.group_id, 1, 1
	  FROM unnest(items) i
		LEFT OUTER JOIN user_group_bind AS gb ON gb.user_id = i.last_modified_by
		LEFT OUTER JOIN user_group AS g ON g.group_id = gb.group_id
	  WHERE
		g.shared_content_items = 1 and g.GROUP_ID <> 1;

    END;
$BODY$;

ALTER PROCEDURE qp_create_content_item_access(numeric[]) owner to postgres;
CREATE OR REPLACE PROCEDURE public.qp_create_content_item_versions(
	ids numeric[],
	last_modified_by numeric
)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
        delete_ids numeric[];
	    ext_ids numeric[];
	    agg_ids numeric[];
	    attr content_attribute;
	    exts link_multiple[];
	    main link_multiple[];
	    main2 link_multiple[];
	    m2o_attrs content_attribute[];
	    current_ids numeric[];
	    current_m2o link[];

    BEGIN
	    create temp table version_items(
	        id numeric primary key,
	        cnt numeric,
	        last_version_id numeric,
	        new_version_id numeric,
	        content_id numeric,
	        max_num numeric
	    );

        insert into version_items (id, cnt)
        select i.id, count(civ.content_item_version_id) from unnest(ids) i(id)
        left join content_item_version civ on civ.content_item_id = i.id
        group by i.id;

        RAISE NOTICE 'Init versions completed: %',  clock_timestamp();

        update version_items set content_id = ci.content_id, max_num = c.max_num_of_stored_versions
        from version_items items
        inner join content_item ci on items.id = ci.CONTENT_ITEM_ID
        inner join content c on c.CONTENT_ID = ci.CONTENT_ID;

        RAISE NOTICE 'max_num updated: %',  clock_timestamp();

        delete_ids := array_agg(content_item_version_id)
        from (
            select content_item_id, content_item_version_id,
            row_number() over (partition by civ.content_item_id order by civ.content_item_version_id desc) as num
            from content_item_version civ
            where content_item_id = ANY($1)
        ) c inner join version_items items
        on items.id = c.content_item_id and c.num >= items.max_num;

        if delete_ids is not null then
            call qp_before_content_item_version_delete(delete_ids::int[]);
            DELETE from content_item_version WHERE content_item_version_id = ANY(delete_ids);
        end if;

        RAISE NOTICE 'Exceeded deleted: %',  clock_timestamp();

        WITH inserted(id, content_item_id) AS (
            INSERT INTO content_item_version (version, version_label, content_version_id, content_item_id, created_by, modified, last_modified_by)
            SELECT now(), 'backup', NULL, ci.content_item_id, $2, ci.modified, ci.last_modified_by
            FROM content_item ci WHERE CONTENT_ITEM_ID = ANY(ids)
            RETURNING content_item_version_id, content_item_id
        )
        update version_items vi set new_version_id = i.id from inserted i where vi.id = i.content_item_id;

        RAISE NOTICE 'New versions updated: %',  clock_timestamp();

        select array_agg(a.data::numeric) into ext_ids from
        (
            select distinct DATA from content_data
            where CONTENT_ITEM_ID = ANY(ids)
            and DATA is not null
            and ATTRIBUTE_ID in (
                select attribute_id from CONTENT_ATTRIBUTE where content_id in (select distinct content_id from version_items) and IS_CLASSIFIER
            )
	    ) as a;

	    RAISE NOTICE 'Extensions defined: %',  clock_timestamp();

	    agg_ids := array_agg(ca.attribute_id) from CONTENT_ATTRIBUTE ca
	    where ca.aggregated and ca.CONTENT_ID = ANY(ext_ids);

	    exts := array_agg(row(cd.o2m_data, ca.content_id, cd.content_item_id))
	    from content_data cd inner join CONTENT_ATTRIBUTE ca on ca.attribute_id = cd.attribute_id
	    where cd.o2m_data = ANY(ids) and cd.attribute_id = ANY(agg_ids);

	    RAISE NOTICE 'Extensions received: %',  clock_timestamp();
        RAISE NOTICE 'exts: %', exts;

        main := array_agg(row(i.new_version_id, e.link_id, e.linked_id))
        from unnest(exts) e inner join version_items i on e.id = i.id;
        RAISE NOTICE 'main: %', main;

        main2 := array_agg(row(i.new_version_id, i.content_id, i.id)) from version_items i;
        RAISE NOTICE 'main2: %', main2;

        main := array_cat(main, main2);


        RAISE NOTICE 'Main defined: %',  clock_timestamp();

        INSERT INTO version_content_data (attribute_id, content_item_version_id, data, blob_data, o2m_data, created)
        SELECT attribute_id, m.id, data, blob_data, o2m_data, now()
        FROM content_data cd inner join unnest(main) m on cd.CONTENT_ITEM_ID = m.linked_id;

	    RAISE NOTICE 'Data saved: %',  clock_timestamp();

        INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
        SELECT m.id, ca.attribute_id, linked_item_id
        FROM item_link_united AS il
        INNER JOIN content_attribute AS ca ON ca.link_id = il.link_id
        INNER JOIN content_item AS ci ON ci.content_id =  ca.content_id AND ci.content_item_id = il.item_id
        inner join unnest(main) m  on il.item_id = m.linked_id;

	    RAISE NOTICE 'M2M saved: %',  clock_timestamp();

	    INSERT INTO item_to_item_version (content_item_version_id, attribute_id, linked_item_id)
        SELECT m.id, ca.attribute_id, cd.content_item_id
        FROM content_data AS cd
        INNER JOIN content_attribute AS ca ON ca.BACK_RELATED_ATTRIBUTE_ID = cd.ATTRIBUTE_ID
        inner join unnest(main) m on cd.O2M_DATA = m.linked_id and ca.CONTENT_ID = m.link_id;

	    RAISE NOTICE 'M2O saved: %',  clock_timestamp();

        INSERT INTO content_item_status_history
        (content_item_id, user_id, description, created, content_item_version_id,system_status_type_id)
        select v.id, $2, 'Record version backup has been created', now(), v.new_version_id, 2
        from version_items v;

        drop table version_items;
    END;
$BODY$;

ALTER PROCEDURE qp_create_content_item_versions(numeric[], numeric) owner to postgres;

CREATE OR REPLACE FUNCTION public.qp_get_article_tsvector(id int)
RETURNS tsvector
    STABLE
    LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    result tsvector;
	    data content_data[];
	    data_item content_data;
    BEGIN
	    result := '' as tsvector;
	    data = array_agg(cd.* order by attribute_id asc) from content_data cd
	    where cd.content_item_id = $1 and cd.ft_data is not null;

	    data = coalesce(data, ARRAY[]::content_data[]);

	    foreach data_item in array data
	    loop
            result := result || data_item.ft_data;
        end loop;

	    return result;

	END;
$BODY$;



alter function qp_get_article_tsvector(int) owner to postgres;






create or replace function qp_get_m2o_ids(content_id integer, field_name text, id integer default NULL)
    returns int[]
    called on null input
    language plpgsql
as
$$
    DECLARE
        condition text;
        sql text;
        result int[];
    BEGIN
        condition := case when id is null THEN ' is null' ELSE ' = ' || id::text end;
        sql := 'select array_agg(content_item_id) from content_%s where "%s" %s';
        sql := format(sql, content_id, lower(field_name), condition);
        execute sql into result;
        return result;
    END;
$$;


alter function qp_get_m2o_ids(integer, text, integer) owner to postgres;


create or replace function qp_get_m2o_ids_multiple(content_id integer, field_name text, ids integer[])
    returns link[]
    language plpgsql
as
$$
    DECLARE
        sql text;
        result link[];
    BEGIN
        sql := 'select array_agg(row("%s", content_item_id)) from content_%s where "%s" = ANY($1)';
        sql := format(sql, lower(field_name), content_id, lower(field_name));
        execute sql into result using ids;
        return result;
    END;
$$;


alter function qp_get_m2o_ids_multiple(integer, text, integer[]) owner to postgres;


CREATE OR REPLACE FUNCTION public.qp_mass_update_content_item(input xml, content_id int, last_modified_by int, not_for_replication int, create_versions bool, import_only bool DEFAULT false)
RETURNS TABLE(id numeric)
LANGUAGE 'plpgsql'
as
$$
DECLARE
        new_ids int[];
        old_ids int[];
        old_non_splitted_ids int[];
        old_splitted_ids int[];
        new_non_splitted_ids int[];
        new_splitted_ids int[];
    BEGIN

        create temp table articles as
        select x.* from XMLTABLE('/ITEMS/ITEM' PASSING input COLUMNS
			content_item_id numeric PATH 'CONTENT_ITEM_ID',
			status_type_id numeric PATH 'STATUS_TYPE_ID',
			archive numeric PATH 'ARCHIVE',
			visible numeric PATH 'VISIBLE'
		) x;

        with result as (
            insert into content_item (
                content_id, status_type_id, not_for_replication, archive, visible,last_modified_by
            )
            (
                select $2, a.status_type_id, $4 = 1, a.archive, a.visible, $3 from articles a
                where a.content_item_id = 0
            )
            returning content_item.content_item_id
        )
        select array_agg(result.content_item_id::int) from result into new_ids;
		new_ids := coalesce(new_ids, ARRAY[]::int[]);

        RAISE NOTICE 'New Ids: %', new_ids;

        if not import_only then

            perform ci.content_item_id from content_item ci
            where ci.content_item_id in (select a.content_item_id from articles a)
            for update;

            old_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select a.content_item_id from articles a);

            RAISE NOTICE 'Old Ids: %', old_ids;

            if create_versions then
                call qp_create_content_item_versions(old_ids, $3);
            end if;

            old_splitted_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select i.id from unnest(old_ids) i(id)) and splitted;

            RAISE NOTICE 'Old Splitted Ids: %', old_splitted_ids;

            old_non_splitted_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select i.id from unnest(old_ids) i(id)) and not splitted;

            RAISE NOTICE 'Old Non-Splitted Ids: %', old_non_splitted_ids;

        end if;

        update content_item ci
            set modified = now(),
                last_modified_by = $3,
                status_type_id = coalesce(a.status_type_id, ci.status_type_id),
                archive = coalesce(a.archive, ci.archive),
                visible = coalesce(a.visible, ci.visible)
        from articles a where a.content_item_id = ci.content_item_id;

        drop table articles;

        if not import_only then

            new_splitted_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select i.id from unnest(old_non_splitted_ids) i(id)) and splitted;

            RAISE NOTICE 'New Splitted Ids: %', new_splitted_ids;

            new_non_splitted_ids := array_agg(ci.content_item_id) from content_item ci
            where ci.content_item_id in (select i.id from unnest(old_splitted_ids) i(id)) and not splitted;

            RAISE NOTICE 'New Non-Splitted Ids: %', new_non_splitted_ids;

            if new_splitted_ids is not null then
                call qp_split_articles(new_splitted_ids, $3);
            end if;

            if new_non_splitted_ids is not null then
                call qp_merge_articles(new_non_splitted_ids, $3, true);
            end if;
        end if;

        return query select unnest::numeric from unnest(new_ids);

    END;

$$;

alter FUNCTION qp_mass_update_content_item(xml, int, int, int, bool, bool) owner to postgres;




create or replace procedure qp_merge_articles(ids integer[], last_modified_by integer DEFAULT 1, force_merge boolean DEFAULT false)
    language plpgsql
as
$$
DECLARE
		ids2 int[];
    BEGIN
		ids2 := array_agg(ci.content_item_id) from content_item ci where ci.content_item_id = ANY(ids) 
			and (ci.SCHEDULE_NEW_VERSION_PUBLICATION or force_merge);
			
		IF ids2 is not null THEN
			call qp_merge_links_multiple(ids2, force_merge);
			
    		UPDATE content_item set not_for_replication = true WHERE content_item_id = ANY(ids2);
			
    		UPDATE content_item set SCHEDULE_NEW_VERSION_PUBLICATION = false, MODIFIED = now(), 
			LAST_MODIFIED_BY = $2, CANCEL_SPLIT = false
			where CONTENT_ITEM_ID = ANY(ids2);
			
			call qp_replicate_items(ids2);
			
    		UPDATE content_item_schedule set delete_job = false WHERE content_item_id = ANY(ids2);
			DELETE FROM content_item_schedule WHERE content_item_id = ANY(ids2);
    		DELETE FROM CHILD_DELAYS WHERE id = ANY(ids2);
    		DELETE FROM CHILD_DELAYS WHERE child_id = ANY(ids2);	
		
		END IF;
	END;
$$;

alter procedure qp_merge_articles(integer[], integer, boolean) owner to postgres;


CREATE OR REPLACE PROCEDURE qp_merge_delays(ids integer[], last_modified_by integer DEFAULT 1)
    language plpgsql
as
$$
DECLARE
		ids2 int[];
		ids3 int[];
    BEGIN
		ids2 := array_agg(cd.child_id) from child_delays cd where cd.id = ANY(ids);
		IF ids2 is not null THEN
			ids3 := array_agg(cd.child_id) from child_delays cd where cd.id = ANY(ids)
			and not exists (select * from CHILD_DELAYS cd2 where cd2.child_id = cd.child_id and cd2.id <> cd.id);

			IF ids3 is not null THEN
				call qp_merge_articles(ids3, last_modified_by);
			END IF;

			DELETE FROM child_delays WHERE id = ANY(ids2);

		END IF;
	END;
$$;

alter procedure qp_merge_delays(integer[], integer) owner to postgres;


-- PROCEDURE: public.qp_merge_links_multiple(integer[], boolean)

-- DROP PROCEDURE public.qp_merge_links_multiple(integer[], boolean);

CREATE OR REPLACE PROCEDURE public.qp_merge_links_multiple(
	ids integer[],
	force_merge boolean)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
		ids_with_links link[];
		new_ids item_link[];
		old_ids item_link[];
		cross_ids item_link[];
	BEGIN
	    ids := coalesce(ids, ARRAY[]::int[]);
		IF array_length(ids, 1) = 0 THEN
			RETURN;
		END IF;
						
		ids_with_links := array_agg(row(i.id, ca.link_id)) from (select unnest(ids) as id) i
  		inner join content_item ci on ci.CONTENT_ITEM_ID = i.id and (ci.SPLITTED or force_merge)
  		inner join content c on ci.CONTENT_ID = c.CONTENT_ID
  		inner join CONTENT_ATTRIBUTE ca on ca.CONTENT_ID = c.CONTENT_ID and link_id is not null;
		ids_with_links := coalesce(ids_with_links, ARRAY[]::link[]);
		IF array_length(ids_with_links, 1) = 0 THEN
			RETURN;
		END IF;								   
				
		new_ids := array_agg(ila.*)
		from item_link_async ila inner join unnest(ids_with_links) i 
		on ila.item_id = i.id and ila.link_id = i.linked_id;
		new_ids := coalesce(new_ids, ARRAY[]::item_link[]);
							
		old_ids := array_agg(il.*)
		from item_link il inner join unnest(ids_with_links) i 
		on il.item_id = i.id and il.link_id = i.linked_id;
		old_ids := coalesce(old_ids, ARRAY[]::item_link[]);
		
		cross_ids := array_agg(t1.*)
		from unnest(new_ids) t1 inner join unnest(old_ids) t2 
		on t1.item_id = t2.item_id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id;
		cross_ids := coalesce(cross_ids, ARRAY[]::item_link[]);

		old_ids := array_agg(t1.*)
		from unnest(old_ids) t1 left join unnest(cross_ids) t2 							  
		on t1.item_id = t2.item_id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
		where t2.item_id is null;
		old_ids := coalesce(old_ids, ARRAY[]::item_link[]);
							
		new_ids := array_agg(t1.*)
		from unnest(new_ids) t1 left join unnest(cross_ids) t2 							  
		on t1.item_id = t2.item_id and t1.link_id = t2.link_id and t1.linked_item_id = t2.linked_item_id
		where t2.item_id is null;
		new_ids := coalesce(new_ids, ARRAY[]::item_link[]);
							
  		delete from item_link il using unnest(old_ids) i
  		where il.item_id = i.item_id and il.link_id = i.link_id and il.linked_item_id = i.linked_item_id;
							
  		delete from item_link il using unnest(old_ids) i, content_to_content c
		where il.linked_item_id = i.item_id and il.link_id = i.link_id and il.item_id = i.linked_item_id
		and i.link_id = c.link_id and c.symmetric;

  		insert into item_link (link_id, item_id, linked_item_id)
  		select link_id, item_id, linked_item_id from unnest(new_ids) i;
							  
  		insert into item_link (link_id, item_id, linked_item_id)
  		select i.link_id, i.linked_item_id, i.item_id
		from unnest(new_ids) i 
		inner join content_to_content c on i.link_id = c.link_id 
		left join item_link il on i.link_id = il.link_id and i.item_id = il.linked_item_id and i.linked_item_id = il.item_id
		where c.symmetric and il.item_id is null;
							
		IF (array_length(new_ids, 1) > 0) THEN
			create temp table multiple_data as select 
				n.item_id, n.link_id, n.linked_item_id,
				ca.attribute_id as linked_attribute_id,
    			(cd.content_item_id is not null) as linked_has_data,
    			(ila.link_id is not null) as linked_has_async
  			from unnest(new_ids) n
			inner join content_to_content cc on n.link_id = cc.link_id and cc.symmetric
    		inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_item_id
    		inner join content c on ci.content_id = c.content_id
    		inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    		left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
    		left join item_link_async ila on n.link_id = ila.link_id and n.linked_item_id = ila.item_id and n.item_id = ila.linked_item_id;
							
  			update content_data cd set data = n.link_id from multiple_data n
  			where cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_item_id
  			and n.linked_has_data;
							
  			insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  			select distinct n.linked_item_id, n.linked_attribute_id, n.link_id from multiple_data n
  			where not n.linked_has_data and n.linked_attribute_id is not null;

			drop table multiple_data;
		END IF;

  		delete from item_link_async ila using unnest(ids_with_links) i
		where ila.item_id = i.id and ila.link_id = i.linked_id;

	END;
	
$BODY$;

alter procedure qp_merge_links_multiple(integer[], boolean) owner to postgres;

DROP FUNCTION IF EXISTS qp_persist_article;

CREATE OR REPLACE FUNCTION public.qp_persist_article(input xml)
RETURNS TABLE(id int, modified timestamp with time zone)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    main_id int;
	    ids int[];
	    cid int;
	    splitted boolean;
        modified timestamp with time zone;
	    data_items value[];
	    m2m_data value[];
	    m2o_data value[];
	    m2m_xml xml;
	    val value;

    BEGIN
        create temp table item as
        select x.* from XMLTABLE('/items/item' PASSING input COLUMNS
			id int PATH '@id',
			content_id int PATH '@content_id',
			status_type_id int PATH '@status_type_id',
			archive int PATH '@archive',
			visible int PATH '@visible',
			last_modified_by int PATH '@last_modified_by',
		    delayed boolean PATH '@delayed',
		    cancel_split boolean PATH '@cancel_split',
		    permanent_lock boolean PATH '@permanent_lock',
		    unique_id uuid PATH '@unique_id'
		) x;

        main_id := i.id from item i;

        if main_id = 0 then
            with result as (
                insert into content_item (
                    content_id, status_type_id, not_for_replication, archive, visible,
                    last_modified_by, schedule_new_version_publication, cancel_split, permanent_lock, unique_id
                )
                (
                    select i.content_id, i.status_type_id, true, i.archive, i.visible, i.last_modified_by,
                    coalesce(i.delayed, false), coalesce(i.cancel_split, false), coalesce(i.permanent_lock, false),
                    coalesce(i.unique_id, md5(random()::text || clock_timestamp()::text)::uuid)
                    from item i
                )
                returning content_item.content_item_id, content_item.modified, content_item.splitted
            )
            select result.content_item_id, result.splitted, result.modified
            into main_id, splitted, modified from result;


        else

            update content_item set not_for_replication = true where not not_for_replication
            and content_item_id in (select item.id from item) ;

            with result as (
                update content_item ci
                    set modified = now(), last_modified_by = i.last_modified_by,
                        status_type_id = coalesce(i.status_type_id, ci.status_type_id),
                        archive = coalesce(i.archive, ci.archive),
                        visible = coalesce(i.visible, ci.visible),
                        schedule_new_version_publication = coalesce(i.delayed, ci.schedule_new_version_publication),
                        permanent_lock = coalesce(i.permanent_lock, ci.permanent_lock),
                        unique_id = coalesce(i.unique_id, ci.unique_id)
                from item i where i.id = ci.content_item_id
                returning content_item_id, ci.modified, ci.splitted
            )
            select result.content_item_id, result.splitted, result.modified
            into main_id, splitted, modified from result;
        end if;

        drop table item;

        data_items := array_agg(row(
            case when x.id <> 0 then x.id else main_id end,
            x.field_id,
            case when x.value <> '' then x.value else null end))
        from XMLTABLE('/items/item/data' PASSING input COLUMNS
			id int PATH '../@id',
            field_id int PATH '@field_id',
            value text PATH '.'
		) x;

        select
            array_agg(d.*) filter(where ca.attribute_type_id = 13),
            array_agg(row(d.id, ca.link_id, d.data)) filter(where ca.attribute_type_id = 11 and ca.link_id is not null),
            array_agg(row(d.id, d.field_id,
            case
                when ca.attribute_type_id = 13 then ca.back_related_attribute_id::text
                when ca.attribute_type_id = 11 and ca.link_id is not null then ca.link_id::text
                else d.data end))
        into m2o_data, m2m_data, data_items
        from unnest(data_items) d inner join content_attribute ca on d.field_id = ca.attribute_id;

        raise notice 'M2M %', m2m_data;
        raise notice 'M2O %', m2o_data;
        raise notice 'Data %', data_items;

        update content_data cd set data = d.data, blob_data = null, not_for_replication = true
        from unnest(data_items) d
        where cd.content_item_id = d.id and cd.attribute_id = d.field_id;

        ids := ARRAY[main_id];

        if m2m_data is not null then

            m2m_xml := xmlelement(name items, xmlagg(x.m2m)) from
            (
                select xmlelement(name item, xmlattributes(
                    d.id, d.field_id as "linkId", d.data as value)
                )
                as m2m from unnest(m2m_data) d
            ) x;

            raise notice 'M2M xml %', m2m_xml;

            call qp_update_m2m_values(m2m_xml);
        end if;

        if m2o_data is not null then
            foreach val in array m2o_data
            loop
                call qp_update_m2o(val.id, val.field_id, val.data);
            end loop;
        end if;

        call qp_replicate_items(ids);

        if m2o_data is not null then
            call qp_update_m2o_final(main_id);
        end if;

        call qp_remove_old_aggregates(ids);

        return query select main_id, modified;

	END;
$BODY$;



alter function qp_persist_article(xml) owner to postgres;







CREATE OR REPLACE PROCEDURE public.qp_publish(
	ids int[],
	last_modified_by int,
	with_aggregated boolean default true)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
        status numeric;
	    ids2 int[];
    BEGIN
        if with_aggregated then
            ids := qp_aggregated_and_self(ids);
        end if;

        status := status_type_id from status_type
        where status_type_name = 'Published' and site_id in (
            select site_id from content c inner join content_item ci on c.content_id = ci.content_id
            where content_item_id = ANY(ids)
        );

        update content_item set status_type_id = status, modified = now(), last_modified_by = $2
        where content_item_id = ANY(ids) and status_type_id <> status and not splitted;

        update content_item set status_type_id = status, modified = now(), last_modified_by = $2 ,
                                schedule_new_version_publication = true
        where content_item_id = ANY(ids) and status_type_id <> status and splitted;

        call qp_merge_delays(ids, last_modified_by);

        ids2 := array_agg(ci.content_item_id) from content_item ci
        where ci.content_item_id = ANY(ids) and not ci.splitted and not ci.schedule_new_version_publication;

        if ids2 is not null then
            ids := ids - ids2;
        end if;

        if array_length(ids, 1) > 0 then
            call qp_merge_articles(ids, last_modified_by);
        end if;


	END;
$BODY$;

alter procedure qp_publish(int[], int, boolean) owner to postgres;






create or replace procedure qp_remove_old_aggregates(ids integer[])
    language plpgsql
as
$$
    BEGIN
        delete from content_item where content_item_id = ANY(qp_aggregates_to_remove(ids));
	END;
$$;

alter procedure qp_remove_old_aggregates(integer[]) owner to postgres;
-- PROCEDURE: public.qp_replicate_items(integer[], integer[], integer)

-- DROP PROCEDURE public.qp_replicate_items(integer[], integer[], integer);

CREATE OR REPLACE PROCEDURE public.qp_replicate_items(
	ids integer[],
	attr_ids integer[] DEFAULT NULL::integer[],
	modification_update_interval integer DEFAULT '-1'::integer)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
		setting_value text;
		default_modification_update_interval int = 30;
		modification_update_interval int;
		content_ids int[];
		id int;
		site_id int;
		none_id int;
		published_id int;
		articles content_item[];
		live_expired boolean = true;
		stage_expired boolean = true;
		live_modified timestamp with time zone;
		stage_modified timestamp with time zone;
		sync_ids int[];
		async_ids int[];
		sync_ids_delayed int[];

		table_name text;
		sql text;
    BEGIN
		setting_value := VALUE from APP_SETTINGS where key = 'CONTENT_MODIFICATION_UPDATE_INTERVAL';
		IF setting_value is not null and qp_is_numeric(setting_value) THEN
			default_modification_update_interval := setting_value::numeric::int;
		END IF;

		IF modification_update_interval < 0 THEN
			modification_update_interval := default_modification_update_interval;
		END IF;

		articles := array_agg(ci.*) from content_item ci where ci.content_item_id = ANY(ids);
		content_ids := array_agg(distinct(a.content_id)) from unnest(articles) a;


		FOREACH id in array content_ids
		LOOP
			select st1.status_type_id, st2.status_type_id into none_id, published_id
			from status_type st1 inner join status_type st2
			on st1.site_id = st2.site_id and st1.status_type_name = 'None' and st2.status_type_name = 'Published'
			where st1.site_id in (select c.site_id from content c where c.content_id = id);

			IF modification_update_interval > 0 THEN
            	select cm.live_modified, cm.stage_modified into live_modified, stage_modified
									   from CONTENT_MODIFICATION cm where cm.content_id = id;
				live_expired := extract(epoch from now() - live_modified) >= modification_update_interval;
				stage_expired := extract(epoch from now() - stage_modified) >= modification_update_interval;
			END IF;

			sync_ids := array_agg(a.content_item_id) from unnest(articles) a where a.content_id = id and not a.splitted;
			async_ids := array_agg(a.content_item_id) from unnest(articles) a where a.content_id = id and a.splitted;
			sync_ids_delayed := array_agg(a.content_item_id) from unnest(articles) a where a.content_id = id and not a.splitted and a.schedule_new_version_publication;
			sync_ids_delayed := coalesce(sync_ids_delayed, ARRAY[]::int[]);

			IF sync_ids is not null THEN
				call qp_upsert_items(id, sync_ids, sync_ids_delayed, none_id, false);
				call qp_delete_items(id, sync_ids, true);
				call qp_update_items_with_content_data_pivot(id, sync_ids, false, attr_ids);
			END IF;

			IF async_ids is not null THEN
				call qp_upsert_items(id, async_ids, ARRAY[]::int[], none_id, true);
				call qp_update_items_flags(id, async_ids, false);
				call qp_update_items_with_content_data_pivot(id, async_ids, true, attr_ids);
			END IF;

			IF EXISTS (
				select * from unnest(articles) a where a.content_id = id and (
					a.cancel_split or (not a.splitted and a.status_type_id = published_id)
				)
			) THEN
				IF (live_expired or stage_expired) THEN
                	update content_modification cm set live_modified = now(), stage_modified = now() where cm.content_id = id;
				END IF;
			ELSE
				IF (stage_expired) THEN
                	update content_modification cm set stage_modified = now() where cm.content_id = id;
				END IF;
			END IF;

		END LOOP;

		update content_data cd2 set o2m_data = a.data::numeric, ft_data = a.ft_data from
		(
		    select to_tsvector('russian', cd.data) ft_data, cd.data, cd.attribute_id, cd.content_item_id
		    from content_data cd inner join content_attribute ca on ca.attribute_id = cd.attribute_id
		    and ca.attribute_type_id = 11 and ca.link_id is null
		    WHERE cd.content_item_id = ANY(ids)
		) a where a.attribute_id = cd2.attribute_id and a.content_item_id = cd2.content_item_id
		      and coalesce(cd2.ft_data, to_tsvector('russian', '')) <> coalesce(a.ft_data, to_tsvector('russian', ''));

		update content_data cd2 set ft_data = a.ft_data from
	    (
		    select to_tsvector('russian', cd.data) ft_data, cd.attribute_id, cd.content_item_id
		    from content_data cd inner join content_attribute ca on ca.attribute_id = cd.attribute_id
		    and (ca.attribute_type_id <> 11 or ca.link_id is not null)
		    WHERE cd.content_item_id = ANY(ids)
		) a where a.attribute_id = cd2.attribute_id and a.content_item_id = cd2.content_item_id
		      and coalesce(cd2.ft_data, to_tsvector('russian', '')) <> coalesce(a.ft_data, to_tsvector('russian', ''));

		update content_item_ft ci set ft_data = a.ft_data from
        (
		    select qp_get_article_tsvector(i.id) ft_data, i.id from unnest(ids) i(id)
		) a where ci.content_item_id = a.id and ci.ft_data <> a.ft_data;

   		update content_item set not_for_replication = false, CANCEL_SPLIT = false where content_item_id = ANY(ids)
   		    and (not_for_replication or cancel_split);

		update content_data set not_for_replication = false where not_for_replication and content_item_id = ANY(ids);

	END;
$BODY$;

alter procedure qp_replicate_items(integer[], integer[], integer) owner to postgres;

create or replace procedure qp_split_articles(ids integer[], last_modified_by integer DEFAULT 1)
    language plpgsql
as
$$
DECLARE
		content_ids int[];
		cid int;
		items link[];
		ids2 int[];
		table_name text;
		sql text;
    BEGIN
		items := array_agg(row(ci.content_id, ci.content_item_id)) from content_item ci where ci.content_item_id = ANY(ids);
        items = coalesce(items, ARRAY[]::link[]);

		content_ids := array_agg(distinct(i.id)) from unnest(items) i;
		content_ids = coalesce(content_ids, ARRAY[]::int[]);
		
		FOREACH cid in array content_ids
		LOOP
			ids2 := array_agg(i.linked_id) from unnest(items) i where i.id = cid;
			ids2 = coalesce(ids2, ARRAY[]::int[]);
	    	sql := '
					insert into content_%s_async 
					select * from content_%s_async c where content_item_id = ANY($1) and not exists(
						select * from content_%s_async a where a.content_item_id = c.content_item_id
					)';
			sql := FORMAT(sql, cid, cid, cid);
			RAISE NOTICE '%', sql;
			execute sql using ids2;							  
								  
		END LOOP;
										  
  		insert into item_link_async select * from item_to_item ii where l_item_id = ANY(ids)
  		and link_id in (select link_id from content_attribute ca where ca.content_id = ANY(content_ids))
  		and not exists (select * from item_link_async ila where ila.item_id = ii.l_item_id);										  
										  
	END
$$;

alter procedure qp_split_articles(integer[], integer) owner to postgres;


-- PROCEDURE: public.qp_update_m2m(numeric, numeric, text, boolean, boolean)

-- DROP PROCEDURE public.qp_update_m2m(numeric, numeric, text, boolean, boolean);

CREATE OR REPLACE PROCEDURE public.qp_update_m2m(
	id numeric,
	link_id numeric,
	value text,
	splitted boolean,
	update_archive boolean)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	  new_ids int[];
	  old_ids int[];
	  cross_ids int[];
	  archive_ids int[];
	  is_symmetric boolean;
	  data_items link_data[];
	BEGIN
		RAISE NOTICE 'Start: %', clock_timestamp();			
		is_symmetric := "symmetric" from content_to_content cc where cc.link_id = $2;
		IF value is null OR value = '' THEN
			new_ids = ARRAY[]::int[];
		ELSE
			new_ids := regexp_split_to_array(value, E',\\s*')::int[];
		END IF;
		
		IF splitted THEN
			old_ids := array_agg(linked_item_id) from item_link_async ila where ila.link_id = $2 and item_id = $1;
		ELSE
			old_ids := array_agg(linked_item_id) from item_link il where il.link_id = $2 and item_id = $1;		
		END IF;
		old_ids := coalesce(old_ids, ARRAY[]::int[]);
		
		cross_ids := new_ids & old_ids;
		old_ids := old_ids - cross_ids;
		new_ids := new_ids - cross_ids;
							
		RAISE NOTICE 'Arrays calculated: %',  clock_timestamp();								
		
		IF not update_archive and array_length(old_ids, 1) > 1 THEN
			archive_ids := array_agg(content_item_id) from content_item where content_item_id = ANY(old_ids) AND archive = 1;
			archive_ids = coalesce(archive_ids, ARRAY[]::int[]);			
			old_ids := old_ids - archive_ids;
		END IF;
								   
		RAISE NOTICE 'Archive calculated: %',  clock_timestamp();								
	   
		IF splitted THEN
			DELETE FROM item_link_async ila WHERE ila.link_id = $2 AND item_id = $1 and linked_item_id = ANY(old_ids);			
		ELSE
			DELETE FROM item_link_async ila WHERE ila.link_id = $2 AND item_id = $1;
			DELETE FROM item_to_item ii WHERE ii.link_id = $2 AND l_item_id = $1 and r_item_id = ANY(old_ids);
			IF is_symmetric THEN
				DELETE FROM item_link_async ila WHERE ila.link_id = $2 AND linked_item_id = $1 and item_id = ANY(old_ids);
				DELETE FROM item_to_item ii WHERE ii.link_id = $2 AND r_item_id = $1 and l_item_id = ANY(old_ids);			
			END IF;
		END IF;
								   
		RAISE NOTICE 'Deleted: %',  clock_timestamp();		

		IF splitted THEN
        	INSERT INTO item_link_async SELECT $2, $1, unnest from unnest(new_ids);
    	ELSE
        	INSERT INTO item_link SELECT $2, $1, unnest from unnest(new_ids);
			IF is_symmetric THEN
 				INSERT INTO item_link SELECT $2, unnest, $1 from unnest(new_ids);			
			END IF;
		END IF;
								   
		RAISE NOTICE 'Inserted: %',  clock_timestamp();								   
		
		IF is_symmetric and not splitted and array_length(new_ids, 1) > 0 THEN
			data_items := array_agg(
					row(n.id, ca.attribute_id, cd.attribute_id is not null, ci.splitted, ila.link_id is not null)
				)
				from (select unnest(new_ids) as id) n
            	inner join content_item ci on ci.CONTENT_ITEM_ID = n.id
            	inner join content c on ci.content_id = c.content_id
            	inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = $2
            	left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
            	left join item_link_async ila on $2 = ila.link_id and n.id = ila.item_id and ila.linked_item_id = $1;
								   
			data_items := COALESCE(data_items, ARRAY[]::link_data[]);
								   
								   
			RAISE NOTICE 'Data items received: %',  clock_timestamp();
				
			IF array_length(data_items, 1) > 0 THEN	

				update content_data cd set data = $2 from unnest(data_items) n
				where cd.ATTRIBUTE_ID = n.attribute_id and cd.CONTENT_ITEM_ID = n.id
				and n.has_data;
								   
				RAISE NOTICE 'content_data updated:%',  clock_timestamp();								   

				insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
				select n.id, n.attribute_id, $2								  
				from unnest(data_items) n
				where not n.has_data and n.attribute_id is not null;

				RAISE NOTICE 'content_data inserted:%',  clock_timestamp();								   

				insert into item_link_async(link_id, item_id, linked_item_id)
				select $2, n.id, $1
				from unnest(data_items) n
				where n.splitted and not n.has_async and n.attribute_id is not null;

				RAISE NOTICE 'item_link_async inserted: %',  clock_timestamp();								   
								   
			END IF;
								  
		END IF;
	END;
$BODY$;

alter procedure qp_update_m2m(numeric, numeric, text, boolean, boolean) owner to postgres;

-- PROCEDURE: public.qp_update_m2m_values(xml)

-- DROP PROCEDURE public.qp_update_m2m_values(xml);

CREATE OR REPLACE PROCEDURE public.qp_update_m2m_values(
	xml_parameter xml)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
		new_ids link_multiple_splitted[];
		old_ids link_multiple_splitted[];
		cross_ids link_multiple_splitted[];
	BEGIN
		create temp table field_values as	
		select x.*, ci.splitted from XMLTABLE(
		'/items/item'  
		PASSING XMLPARSE(DOCUMENT xml_parameter) 
		COLUMNS
			id int PATH '@id',
			link_id int PATH '@linkId',
			value text PATH '@value'
		) x inner join content_item ci on x.id = ci.content_item_id;
		
		new_ids := array_agg(row(a.id, a.link_id, unnest, a.splitted)) 
		from field_values a, unnest(
		    case when a.value = '' then ARRAY[]::int[] else regexp_split_to_array(a.value, E',\\s*')::int[] end
		);
		new_ids := coalesce(new_ids, ARRAY[]::link_multiple_splitted[]);
		
		RAISE NOTICE 'New ids: %', new_ids;
							
		old_ids := array_agg(row(c.*)) from
		(					
		  select ila.item_id, ila.link_id, ila.linked_item_id, f.splitted
		  from item_link_async ila inner join field_values f
		  on ila.link_id = f.link_id and ila.item_id = f.id
		  where f.splitted
		  union all
		  select il.item_id, il.link_id, il.linked_item_id, f.splitted
		  from item_link il inner join field_values f
		  on il.link_id = f.link_id and il.item_id = f.id
		  where not f.splitted
		) c;
		old_ids := coalesce(old_ids, ARRAY[]::link_multiple_splitted[]);
		
		RAISE NOTICE 'Old ids: %', old_ids;		
		
		cross_ids := array_agg(row(t1.id, t1.link_id, t1.linked_id, t1.splitted))
		from unnest(new_ids) t1 inner join unnest(old_ids) t2 
		on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_id = t2.linked_id;
		cross_ids := coalesce(cross_ids, ARRAY[]::link_multiple_splitted[]);
		
		RAISE NOTICE 'Cross ids: %', cross_ids;	
		
		old_ids := array_agg(row(t1.id, t1.link_id, t1.linked_id, t1.splitted))							  
		from unnest(old_ids) t1 left join unnest(cross_ids) t2 							  
		on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_id = t2.linked_id
		where t2.id is null;
		old_ids := coalesce(old_ids, ARRAY[]::link_multiple_splitted[]);
							
		new_ids := array_agg(row(t1.id, t1.link_id, t1.linked_id, t1.splitted))							  
		from unnest(new_ids) t1 left join unnest(cross_ids) t2 							  
		on t1.id = t2.id and t1.link_id = t2.link_id and t1.linked_id = t2.linked_id
		where t2.id is null;
		new_ids := coalesce(new_ids, ARRAY[]::link_multiple_splitted[]);
							  
  		delete from item_link_async il using field_values f
		where il.item_id = f.id and il.link_id = f.link_id
		and not f.splitted;
							
  		delete from item_link_async il using unnest(old_ids) i
  		where il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_id
  		and i.splitted;
							
  		delete from item_link il using unnest(old_ids) i
  		where il.item_id = i.id and il.link_id = i.link_id and il.linked_item_id = i.linked_id
  		and not i.splitted;							

  		delete from item_link_async il using unnest(old_ids) i, content_to_content c
  		where il.linked_item_id = i.id and il.link_id = i.link_id and il.item_id = i.linked_id 
		and c.link_id = i.link_id
  		and not i.splitted and c.symmetric;
							
  		delete from item_link il using unnest(old_ids) i, content_to_content c
  		where il.linked_item_id = i.id and il.link_id = i.link_id and il.item_id = i.linked_id
		and c.link_id = i.link_id							
  		and not i.splitted and c.symmetric;
							
  		insert into item_link_async (link_id, item_id, linked_item_id)
  		select link_id, id, linked_id from unnest(new_ids)
  		where splitted;

  		insert into item_link (link_id, item_id, linked_item_id)
  		select link_id, id, linked_id from unnest(new_ids)
  		where not splitted;
							
  		insert into item_link (link_id, item_id, linked_item_id)
  		select i.link_id, i.linked_id, i.id
		from unnest(new_ids) i, content_to_content c
  		where i.link_id = c.link_id and i.id <> i.linked_id
		and not i.splitted and c.symmetric;							
							
		IF (array_length(new_ids, 1) > 0) THEN
			create temp table multiple_data as select 
				n.id, n.link_id, n.linked_id, n.splitted,
				ca.attribute_id as linked_attribute_id,
    			(cd.content_item_id is not null) as linked_has_data,
    			ci.splitted as linked_splitted,
    			(ila.link_id is not null) as linked_has_async
  			from unnest(new_ids) n
			inner join content_to_content cc on n.link_id = cc.link_id and cc.symmetric
    		inner join content_item ci on ci.CONTENT_ITEM_ID = n.linked_id
    		inner join content c on ci.content_id = c.content_id
    		inner join content_attribute ca on ca.content_id = c.content_id and ca.link_id = n.link_id
    		left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = ci.content_item_id
    		left join item_link_async ila on n.link_id = ila.link_id and n.linked_id = ila.item_id and n.id = ila.linked_item_id;						
							
  			update content_data cd set data = n.link_id from multiple_data n
  			where cd.ATTRIBUTE_ID = n.linked_attribute_id and cd.CONTENT_ITEM_ID = n.linked_id
  			and not n.splitted and n.linked_has_data;
							
  			insert into content_data(CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA)
  			select distinct n.linked_id, n.linked_attribute_id, n.link_id from multiple_data n
  			where not n.splitted and not n.linked_has_data and n.linked_attribute_id is not null;

  			insert into item_link_async(link_id, item_id, linked_item_id)
  			select n.link_id, n.linked_id, n.id from multiple_data n
  			where not n.splitted and n.linked_splitted and not n.linked_has_async and n.linked_attribute_id is not null	;
							
			drop table multiple_data;
		END IF;
		drop table field_values;
	END;
	
$BODY$;

alter procedure qp_update_m2m_values(xml) owner to postgres;
-- PROCEDURE: public.qp_update_m2o(numeric, numeric, text, boolean)

-- DROP PROCEDURE public.qp_update_m2o(numeric, numeric, text, boolean);

CREATE OR REPLACE PROCEDURE public.qp_update_m2o(
	id numeric,
	field_id numeric,
	ids text,
	update_archive boolean default false)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    attr content_attribute;
	    new_ids int[];
	    old_ids int[];
	    cross_ids int[];
	    archive_ids int[];
	BEGIN
	    attr := row(ca.*) from content_attribute ca where ca.attribute_id = field_id;

	    old_ids := array_agg(cd.content_item_id)
	    from content_data cd where cd.attribute_id = attr.back_related_attribute_id and cd.o2m_data = id;
	    old_ids = coalesce(old_ids, ARRAY[]::int[]);


		RAISE NOTICE 'Start: %', clock_timestamp();
		IF ids is null OR ids = '' THEN
			new_ids = ARRAY[]::int[];
		ELSE
			new_ids := regexp_split_to_array(ids, E',\\s*')::int[];
		END IF;

		cross_ids := new_ids & old_ids;
		old_ids := old_ids - cross_ids;
		new_ids := new_ids - cross_ids;

		RAISE NOTICE 'Arrays calculated: %, to add: %', clock_timestamp(), new_ids;

		IF not update_archive and array_length(old_ids, 1) > 1 THEN
			archive_ids := array_agg(content_item_id) from content_item where content_item_id = ANY(old_ids) AND archive = 1;
			archive_ids = coalesce(archive_ids, ARRAY[]::int[]);
			old_ids := old_ids - archive_ids;
		END IF;

		RAISE NOTICE 'Archive calculated: %, to remove: %, ', clock_timestamp(), old_ids;

		create temp table if not exists o2m_result_ids
		(
		    id numeric, attribute_id numeric, to_remove boolean, remove_delays boolean
		);

		insert into o2m_result_ids
        select unnest, attr.back_related_attribute_id, true, false from unnest(old_ids)
        union all
        select unnest, attr.back_related_attribute_id, false, false from unnest(new_ids);

		RAISE NOTICE 'Result returned: %',  clock_timestamp();


	END;
$BODY$;

alter procedure qp_update_m2o(numeric, numeric, text, boolean) owner to postgres;

CREATE OR REPLACE PROCEDURE public.qp_update_m2o_final(
	id numeric)
LANGUAGE 'plpgsql'

AS $BODY$
	DECLARE
	    item content_item;
	    max_status numeric;
	    ids int[];
	    ids_to_split int[];
	BEGIN
    	if exists(select * from information_schema.tables where table_name = 'o2m_result_ids') then
            if exists(select * from o2m_result_ids) or exists(select * from CHILD_DELAYS cd where cd.id = $1) then
                item := row(ci.*) from content_item ci where ci.content_item_id = $1;

                max_status := max_status_type_id from content_item_workflow ciw
                    left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
                    where ciw.content_item_id = $1;

                if max_status is null then
                    max_status :=  st.status_type_id from content_item
                    inner join content c on content_item.content_id = c.content_id
                    inner join site s on c.site_id = s.site_id
                    inner join status_type st on s.site_id = st.site_id and status_type_name = 'Published'
                    where content_item_id = $1;
                end if;

                if item.status_type_id = max_status and not item.splitted then
                    call qp_merge_delays(ARRAY[id]::int[], item.last_modified_by::int);
                    RAISE NOTICE 'Delays merged: %', clock_timestamp();
                end if;

                ids := array_agg(o.id) from o2m_result_ids o;

                if ids is null then
                    return;
                end if;

                update o2m_result_ids o set remove_delays = true
                from child_delays cd where o.id = cd.child_id and cd.id = $1;

                update content_item
                set modified = now(), last_modified_by = item.last_modified_by, not_for_replication = true
                where content_item_id = ANY(ids);

                RAISE NOTICE 'Not for replication: %', ids;

                update content_data cd
                set data = $1, blob_data = null, modified = now()
                from o2m_result_ids r
                where cd.attribute_id = r.attribute_id and cd.content_item_id = r.id and not r.to_remove;

                insert into content_data (CONTENT_ITEM_ID, ATTRIBUTE_ID, DATA, BLOB_DATA, MODIFIED)
                select r.id, r.attribute_id, $1, NULL, now()
                from o2m_result_ids r
                left join content_data cd on cd.attribute_id = r.attribute_id and cd.content_item_id = r.id
                where not r.to_remove and cd.CONTENT_DATA_ID is null;

                update content_data cd
                set data = null, blob_data = null, modified = now()
                from o2m_result_ids r
                where cd.attribute_id = r.attribute_id and cd.content_item_id = r.id and r.to_remove;

                RAISE NOTICE 'content_data updated: %', clock_timestamp();

		        delete from CHILD_DELAYS cd where cd.id = $1 and child_id in (
		            select o.id from o2m_result_ids o where remove_delays
		        );

                RAISE NOTICE 'Child delays removed: %', clock_timestamp();

		        if (item.status_type_id <> max_status or item.splitted) then
                    insert into child_delays (id, child_id)
                    select $1, r.id from o2m_result_ids r
                    inner join content_item ci on r.id = ci.content_item_id
                    left join child_delays ex on ex.child_id = ci.content_item_id and ex.id = $1
                    left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id
                    left join workflow_max_statuses wms on ciw.workflow_id = wms.workflow_id
                    where ex.child_id is null and ci.status_type_id = wms.max_status_type_id
                        and (not ci.splitted or ci.splitted and exists(
                            select * from CHILD_DELAYS where child_id = ci.CONTENT_ITEM_ID and r.id <> $1)
                        )
                        and not r.remove_delays;

                    RAISE NOTICE 'Child delays inserted: %', clock_timestamp();

                    ids_to_split := array_agg(content_item_id) from content_item
                    where content_item_id in (select child_id from child_delays cd where cd.id = $1) and not splitted;

                    if ids_to_split is not null then
                        call qp_split_articles(ids_to_split);
                        RAISE NOTICE 'Articles splitted: %', ids_to_split;
                    end if;

                    update content_item set schedule_new_version_publication = true
                    where content_item_id in (select child_id from child_delays cd where cd.id = $1);

                end if;

                call qp_replicate_items(ids);

                RAISE NOTICE 'Articles replicated: % %', ids, clock_timestamp();

            end if;

            drop table o2m_result_ids;

        end if;
	END;
$BODY$;

alter procedure qp_update_m2o_final(numeric) owner to postgres;


create or replace function process_before_content_delete() returns trigger
    language plpgsql
as
$$
    DECLARE
        ids integer[];
    BEGIN
        ids := ARRAY[old.CONTENT_ID]::int[];
        call qp_before_content_delete(ids);
		RETURN OLD;
	END;
$$;

alter function process_before_content_delete() owner to postgres;

create or replace function process_before_content_item_delete() returns trigger
    language plpgsql
as
$$
    DECLARE
        ids integer[];
    BEGIN
        ids := ARRAY[old.CONTENT_ITEM_ID]::int[];
        call qp_before_content_item_delete(ids);
		RETURN OLD;
	END;
$$;

alter function process_before_content_item_delete() owner to postgres;

create or replace function process_before_content_item_version_delete() returns trigger
    language plpgsql
as
$$
DECLARE
		ids int[];
    BEGIN
		ids := ARRAY[OLD.content_item_version_id]::int[];
		call qp_before_content_item_version_delete(ids);
		RETURN OLD;
	END;
$$;

alter function process_before_content_item_version_delete() owner to postgres;


create or replace function process_before_content_to_content_delete() returns trigger
    language plpgsql
as
$$
    DECLARE
        ids integer[];
    BEGIN
        ids := ARRAY[old.LINK_ID]::int[];
        call qp_before_content_to_content_delete(ids);
		RETURN OLD;
	END;
$$;

alter function process_before_content_to_content_delete() owner to postgres;

-- FUNCTION: public.process_content_data_upsert()

-- DROP FUNCTION public.process_content_data_upsert();

CREATE OR REPLACE FUNCTION public.process_content_data_upsert()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF
AS $BODY$
	DECLARE
		ids int[];
		async_ids int[];
	    o2m_ids int[];
	    ft_ids int[];
		attribute_ids int[];
		attr_id int;
		attr content_attribute;
		source text;
		column_type text;
		sql text;
    BEGIN

		IF NOT EXISTS(select content_data_id from new_table where not not_for_replication) THEN
			RETURN NULL;
		END IF;

		IF TG_OP = 'UPDATE' THEN
            select
                array_agg(i.content_data_id) filter (where ca.attribute_type_id = 11 and ca.link_id is null ) ,
		        array_agg(i.content_data_id) filter (where ca.attribute_id <> 11 or ca.link_id is not null)
                into o2m_ids, ft_ids
            from new_table i
                inner join old_table o on o.content_data_id = i.content_data_id
                inner join content_attribute ca on ca.attribute_id = i.attribute_id
                where coalesce(i.data, '') <> coalesce(o.data, '')
                and not i.not_for_replication;
		ELSE
            select
                array_agg(i.content_data_id) filter (where ca.attribute_type_id = 11 and ca.link_id is null),
  		        array_agg(i.content_data_id) filter (where ca.attribute_id <> 11 or ca.link_id is not null)
                into o2m_ids, ft_ids
            from new_table i
                inner join content_attribute ca on ca.attribute_id = i.attribute_id
		        where i.data is not null
		        and not i.not_for_replication;
        END IF;

		Raise notice 'O2M to sync: %', o2m_ids;
		Raise notice 'FT to sync: %', ft_ids;



        IF o2m_ids is not null THEN
            update content_data set o2m_data = data::numeric where content_data_id = ANY(o2m_ids);
        END IF;

		IF ft_ids is not null THEN
            update content_data set ft_data = to_tsvector('russian', data) where content_data_id = ANY(ft_ids);

            update content_item_ft set ft_data = qp_get_article_tsvector(i.id) from (
                select distinct cd.content_item_id::int as id from content_data cd where cd.content_data_id = ANY(ft_ids)
            ) i;
        END IF;

		IF TG_OP = 'UPDATE' THEN
			IF EXISTS (
				select * from new_table i inner join old_table o on i.content_data_id = o.content_data_id
				where i.splitted <> o.splitted
				or i.not_for_replication <> o.not_for_replication
				or coalesce(i.ft_data, '') <> coalesce(o.ft_data, '')
				or coalesce(i.o2m_data, 0) <> coalesce(o.o2m_data, 0)) THEN
					RETURN NULL;
			END IF;
		END IF;

		attribute_ids := array_agg(distinct(attribute_id)) from new_table;
		attribute_ids := COALESCE(attribute_ids, ARRAY[]::int[]);
		FOREACH attr_id in array attribute_ids
		LOOP
			attr := row(ca.*) from content_attribute ca where ca.attribute_id = attr_id;

			ids := array_agg(i.content_item_id) from new_table i
                inner join content_item ci on ci.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID
                inner join content c on ci.CONTENT_ID = c.CONTENT_ID
                where ATTRIBUTE_ID = attr.attribute_id and not ci.not_for_replication and c.virtual_type = 0
				and not ci.splitted;
			ids := COALESCE(ids, ARRAY[]::int[]);

			async_ids := array_agg(i.content_item_id) from new_table i
                inner join content_item ci on ci.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID
                inner join content c on ci.CONTENT_ID = c.CONTENT_ID
                where ATTRIBUTE_ID = attr.attribute_id and not ci.not_for_replication and c.virtual_type = 0
				and ci.splitted;
			async_ids := COALESCE(async_ids, ARRAY[]::int[]);

			IF attr.attribute_type_id in (2,3,11,13) THEN
				column_type := 'numeric';
			ELSEIF attr.attribute_type_id in (4,5,6) THEN
				column_type := 'timestamp with time zone';
			ELSE
				column_type := 'text';
			END IF;

	   		IF attr.attribute_type_id in (9,10) THEN
				source := 'coalesce(cd.data, cd.blob_data)';
			ELSE
				source := 'qp_correct_data(cd.data::text, %s, %s, ''%s'')';
				source := FORMAT(source,
					attr.attribute_type_id, attr.attribute_size, coalesce(attr.default_value, '')
				);
			END IF;

			sql :=
				'update %s d set "%s" = %s::%s from content_data cd, unnest($1) where d.content_item_id = unnest' ||
				' and cd.attribute_id = %s and cd.content_item_id = d.content_item_id';



			IF array_length(ids, 1) > 0 THEN

				sql := FORMAT(sql, 'content_' || attr.content_id, lower(attr.attribute_name), source, column_type, attr.attribute_id);
				RAISE NOTICE '%', sql;
				execute sql using ids;

			END IF;

			IF array_length(async_ids, 1) > 0 THEN
				sql := FORMAT(sql, 'content_' || attr.content_id || '_async', lower(attr.attribute_name), source, column_type, attr.attribute_id);
				RAISE NOTICE '%', sql;
				execute sql using async_ids;
			END IF;

		END LOOP;
		RETURN NULL;
	END
$BODY$;

ALTER FUNCTION public.process_content_data_upsert()
    OWNER TO postgres;

-- FUNCTION: public.process_content_item_insert()

-- DROP FUNCTION public.process_content_item_insert();

CREATE OR REPLACE FUNCTION public.process_content_item_delete()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$
	DECLARE
		ids int[];
		content_ids int[];
		cid int;
		published_id int;
		is_virtual boolean;
		char_ids text[];
		o2m_ids int[];
    BEGIN
	
		IF NOT EXISTS(SELECT * FROM information_schema.tables where table_name = 'disable_td_delete_item') THEN
			content_ids := array_agg(distinct(content_id)) from OLD_TABLE;
			content_ids := COALESCE(content_ids, ARRAY[]::int[]);		
			
		FOREACH cid in array content_ids
			LOOP
				select st.status_type_id, c.virtual_type <> 0 into published_id, is_virtual from STATUS_TYPE st
				inner join content c on st.site_id = c.site_id and st.status_type_name = 'Published'
				where c.content_id = cid;

				ids := array_agg(n.content_item_id) from OLD_TABLE n where n.content_id = cid;
									
				IF EXISTS (select * from OLD_TABLE where status_type_id = published_id and not splitted) THEN
					update content_modification set live_modified = now(), stage_modified = now() where content_id = cid;
				ELSE
					update content_modification set stage_modified = now() where content_id = cid;
				END IF;									
								 
            	o2m_ids := array_agg(ca1.attribute_id) from CONTENT_ATTRIBUTE ca1
            		inner join content_attribute ca2 on ca1.RELATED_ATTRIBUTE_ID = ca2.ATTRIBUTE_ID
            		where ca2.CONTENT_ID = cid;
									
				IF o2m_ids is not null THEN
					char_ids := array_agg(unnest::text) from unnest(ids);
									
	                UPDATE content_attribute SET default_value = null
                    	WHERE attribute_id = ANY(o2m_ids)
                    	AND default_value = ANY(char_ids);

					UPDATE content_data SET data = NULL, blob_data = NULL
						WHERE attribute_id = ANY(o2m_ids)
						AND o2m_data = ANY(ids);

					DELETE from VERSION_CONTENT_DATA
						where ATTRIBUTE_ID = ANY(o2m_ids)
						AND o2m_data = ANY(ids);
									
				END IF;
				
								 
				IF NOT is_virtual THEN
					call qp_delete_items(cid, ids, false);
					call qp_delete_items(cid, ids, true);
				END IF;
								 

			END LOOP;
		END IF;
							 
		RETURN NULL;
	END
$BODY$;

ALTER FUNCTION public.process_content_item_delete()
    OWNER TO postgres;

-- FUNCTION: public.process_content_item_insert()

-- DROP FUNCTION public.process_content_item_insert();

CREATE OR REPLACE FUNCTION public.process_content_item_insert()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$
	DECLARE
	    all_ids int[];
		ids int[];
		ids2 int[];
		content_ids int[];
		cid int;
		none_id int;
    BEGIN

	    all_ids := array_agg(content_item_id) from NEW_TABLE;
		all_ids := COALESCE(all_ids, ARRAY[]::int[]);
	    call qp_create_content_item_access(all_ids);

		insert into content_data (content_item_id, attribute_id, not_for_replication)
		select i.content_item_id, ca.attribute_id, i.not_for_replication
		from new_table i inner join content_attribute ca on i.content_id = ca.content_id;
	
		content_ids := array_agg(distinct(content_id)) from NEW_TABLE;
		content_ids := COALESCE(content_ids, ARRAY[]::int[]);
		FOREACH cid in array content_ids
		LOOP
			none_id := st.status_type_id from STATUS_TYPE st
			inner join content c on st.site_id = c.site_id and st.status_type_name = 'None'
			where c.content_id = cid;
								
			ids := array_agg(n.content_item_id) from new_table n
						where n.content_id = cid and not n.not_for_replication;
			ids := COALESCE(ids, ARRAY[]::int[]);
							 
			ids2 := array_agg(n.content_item_id) from new_table n
						where n.content_id = cid and not n.not_for_replication and n.schedule_new_version_publication;
			ids2 := COALESCE(ids2, ARRAY[]::int[]);
			
			call qp_upsert_items(cid, ids, ids2, none_id, false);
							 
		END LOOP;
		RETURN NULL;
	END
$BODY$;

ALTER FUNCTION public.process_content_item_insert()
    OWNER TO postgres;

-- FUNCTION: public.process_content_item_update()

-- DROP FUNCTION public.process_content_item_update();

CREATE OR REPLACE FUNCTION public.process_content_item_update()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$
	DECLARE
		splitted_ids int[];
		not_for_replication_ids int[];
		locked_by_ids int[];
		modified_ids int[];
		ids int[];
		content_ids int[];
		cid int;
		items content_item[];
		ids_to_set int[];
		ids_to_reset int[];
		sync_ids int[];
		async_ids int[];
		none_id int;
		published_id int;
		sql text;
    BEGIN
		select 
			array_agg(i.content_item_id) filter (where i.splitted <> o.splitted), 
			array_agg(i.content_item_id) filter (where i.not_for_replication <> o.not_for_replication),
			array_agg(i.content_item_id) filter (where i.locked_by <> o.locked_by),
			array_agg(i.content_item_id) filter (where i.modified <> o.modified)			
		into splitted_ids, not_for_replication_ids, locked_by_ids, modified_ids
		from new_table i inner join old_table o on i.content_item_id = o.content_item_id;

		RAISE NOTICE 'Splitted ids: %', splitted_ids;
		
		IF splitted_ids is not null THEN
			update content_data set splitted = i.splitted
			from new_table i where content_data.content_item_id = i.content_item_id
			and content_data.content_item_id = ANY(splitted_ids);
			
			RETURN NULL;		
		END IF;

		RAISE NOTICE 'Not for Replication ids: %', not_for_replication_ids;
		
		IF not_for_replication_ids is not null THEN
			update content_data set not_for_replication = i.not_for_replication
			from new_table i where content_data.content_item_id = i.content_item_id
			and content_data.content_item_id = ANY(not_for_replication_ids);
		
			RETURN NULL;		
		END IF;
		
		IF locked_by_ids is not null THEN
			RETURN NULL;
		END IF;
		
		insert into content_data (content_item_id, attribute_id, not_for_replication)
		select i.content_item_id, ca.attribute_id, i.not_for_replication
		from new_table i inner join content_attribute ca on i.content_id = ca.content_id
		left join content_data cd on cd.ATTRIBUTE_ID = ca.ATTRIBUTE_ID and cd.CONTENT_ITEM_ID = i.CONTENT_ITEM_ID 
		where cd.CONTENT_DATA_ID is null;		
	
		content_ids := array_agg(distinct(content_id)) from new_table 
		where content_id in (select content_id from content where virtual_type = 0);
		content_ids := COALESCE(content_ids, ARRAY[]::int[]);		
			
		FOREACH cid in array content_ids
			LOOP
				
				select st1.status_type_id, st2.status_type_id into none_id, published_id
				from status_type st1 inner join status_type st2
				on st1.site_id = st2.site_id and st1.status_type_name = 'None' and st2.status_type_name = 'Published'
				where st1.site_id in (select c.site_id from content c where c.content_id = cid);
								
				items := array_agg(n.*) from new_table n where n.content_id = cid;
				ids := array_agg(i.content_item_id) from unnest(items) i;
				async_ids := array_agg(i.content_item_id) from unnest(items) i where i.cancel_split;

				sql := '
					create temp table ids_with_splitted as 
						select content_item_id, (curr_weight < front_weight and is_workflow_async) or 
            				(curr_weight = workflow_max_weight and delayed) as splitted, not_for_replication
						from (
            				select distinct ci.content_item_id, st1.WEIGHT as curr_weight, st2.WEIGHT as front_weight, 
            				max(st3.WEIGHT) over (partition by ci.content_item_id) as workflow_max_weight, 
							case when i2.id is not null then false else ciw.is_async end as is_workflow_async, 
            				ci.SCHEDULE_NEW_VERSION_PUBLICATION as delayed, ci.not_for_replication 
            				from content_item ci inner join UNNEST($1) i(id) on i.id = ci.content_item_id
            				left join UNNEST($2) i2(id) on i2.id = ci.content_item_id
            				inner join content_%s c on ci.CONTENT_ITEM_ID = c.CONTENT_ITEM_ID
            				inner join STATUS_TYPE st1 on ci.STATUS_TYPE_ID = st1.STATUS_TYPE_ID
            				inner join STATUS_TYPE st2 on c.STATUS_TYPE_ID = st2.STATUS_TYPE_ID
            				left join content_item_workflow ciw on ci.content_item_id = ciw.content_item_id
            				left join workflow_rules wr on ciw.WORKFLOW_ID = wr.WORKFLOW_ID
            				left join STATUS_TYPE st3 on st3.STATUS_TYPE_ID = wr.SUCCESSOR_STATUS_ID
            			) as main';
				
				sql := FORMAT(sql, cid);
				RAISE NOTICE 'IDS with splitted %', sql;
				EXECUTE sql using ids, async_ids;

				select 
					array_agg(i2.content_item_id) filter (where not i2.splitted and i.splitted), 
					array_agg(i2.content_item_id) filter (where i2.splitted and not i.splitted),
					array_agg(i2.content_item_id) filter (where not i2.splitted and not i2.not_for_replication),
					array_agg(i2.content_item_id) filter (where i2.splitted and not i2.not_for_replication)
				into ids_to_reset, ids_to_set, sync_ids, async_ids
				from ids_with_splitted i2 inner join (select * from unnest(items)) i on i2.content_item_id = i.content_item_id;

				drop table ids_with_splitted;
				
				IF ids_to_set is not null THEN
				    RAISE NOTICE 'ids to set splitted: %', ids_to_set;				    
            		insert into content_item_splitted(content_item_id)
					select id from (select unnest(ids_to_set) as id) base
            		where not exists (select * from content_item_splitted cis where cis.content_item_id = base.id);
																		   
					update content_item set splitted = true where content_item_id = ANY(ids_to_set);
				END IF;
																		   
				IF ids_to_reset is not null THEN
				    RAISE NOTICE 'ids to reset splitted: %', ids_to_reset;					    
					delete from content_item_splitted where content_item_id = ANY(ids_to_reset);
					update content_item set splitted = false where content_item_id = ANY(ids_to_reset);
				END IF;

				IF sync_ids is not null THEN
				    RAISE NOTICE 'ids to update sync: %', sync_ids;					    
					call qp_upsert_items(cid, sync_ids, ARRAY[]::int[], none_id, false);
					call qp_delete_items(cid, sync_ids, true);
				END IF;
																		   
				IF async_ids is not null THEN
				    RAISE NOTICE 'ids to update async: %', async_ids;					    
					call qp_upsert_items(cid, async_ids, ARRAY[]::int[], none_id, true);
					call qp_update_items_flags(cid, async_ids, false);
				END IF;																				   

			END LOOP;

		IF modified_ids is not null THEN
			insert into content_item_status_history
			(content_item_id, status_type_id, archive, visible, user_id, description, created)
			select i.content_item_id, i.status_type_id, i.archive = 1, i.visible = 1, i.last_modified_by, st.description, now()
			from new_table i INNER JOIN status_type st ON i.status_type_id = st.status_type_id
			where i.content_item_id = ANY(modified_ids);
		END IF;
																		   
		RETURN NULL;
	END
$BODY$;

ALTER FUNCTION public.process_content_item_update()
    OWNER TO postgres;

-- FUNCTION: public.process_item_to_item_delete()

-- DROP FUNCTION public.process_item_to_item_delete();

CREATE OR REPLACE FUNCTION public.process_item_to_item_delete()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$
	DECLARE
		content_links content_link[];
		item content_link;
		link_items link[];
		self_related boolean;
		is_async boolean;
    BEGIN
	  	content_links := array_agg(
			distinct row(i.link_id, c2c.SYMMETRIC, c2c.l_content_id, c2c.r_content_id)
		) from old_table i inner join content_to_content c2c on i.link_id = c2c.link_id;
		IF array_length(content_links, 1) > 0 THEN
			FOREACH item in array content_links
			LOOP
			    IF TG_TABLE_NAME = 'item_to_item' THEN
				    link_items := array_agg(distinct row(l_item_id, r_item_id)) from old_table where link_id = item.id;
				ELSE
				    link_items := array_agg(distinct row(item_id, linked_item_id)) from old_table where link_id = item.id;
				END IF;
				self_related := item.l_content_id = item.r_content_id;
				is_async := TG_TABLE_NAME = 'item_link_async';												 
				CALL qp_delete_link_table_item(item.id, item.l_content_id, link_items, is_async, false, false);
				CALL qp_delete_link_table_item(item.id, item.r_content_id, link_items, is_async, true, self_related);
			END LOOP;
		END IF;
		RETURN NULL;												   	
    END;
$BODY$;

ALTER FUNCTION public.process_item_to_item_delete()
    OWNER TO postgres;

-- FUNCTION: public.process_item_to_item_insert()

-- DROP FUNCTION public.process_item_to_item_insert();

CREATE OR REPLACE FUNCTION public.process_item_to_item_insert()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    COST 100
    VOLATILE NOT LEAKPROOF 
AS $BODY$
	DECLARE
		content_links content_link[];
		item content_link;
		link_items link[];
		self_related boolean;
		is_async boolean;
    BEGIN
	  	content_links := array_agg(
			distinct row(i.link_id, c2c.SYMMETRIC, c2c.l_content_id, c2c.r_content_id)
		) from new_table i inner join content_to_content c2c on i.link_id = c2c.link_id;
		
		IF array_length(content_links, 1) > 0 THEN
			FOREACH item in array content_links
			LOOP
			    IF TG_TABLE_NAME = 'item_to_item' THEN
				    link_items := array_agg(distinct row(l_item_id, r_item_id)) from new_table where link_id = item.id;
				ELSE
				    link_items := array_agg(distinct row(item_id, linked_item_id)) from new_table where link_id = item.id;
				END IF;
				self_related := item.l_content_id = item.r_content_id;
				is_async := TG_TABLE_NAME = 'item_link_async';														   
				CALL qp_insert_link_table_item(item.id, item.l_content_id, link_items, is_async, false, false);
				CALL qp_insert_link_table_item(item.id, item.r_content_id, link_items, is_async, true, self_related);

			END LOOP;
		END IF;
		RETURN NULL;												   	
    END;
$BODY$;

ALTER FUNCTION public.process_item_to_item_insert()
    OWNER TO postgres;

CREATE OR REPLACE FUNCTION update_hash() RETURNS TRIGGER AS $tiu_update_hash$
	DECLARE 
		salt bigint;
	 	old_hash bytea;
	BEGIN
		IF (TG_OP = 'INSERT') THEN
			IF NEW.PASSWORD IS NULL OR NEW.PASSWORD = '' THEN
                RAISE EXCEPTION 'Cannot create user with empty password';
			END IF;
			old_hash = null;
		ELSEIF (TG_OP = 'UPDATE') THEN
			IF NEW.PASSWORD IS NULL OR NEW.PASSWORD = '' OR OLD.PASSWORD = NEW.PASSWORD THEN
				RAISE NOTICE 'No changes in password detected';
				RETURN NEW;
			END IF;
			old_hash = OLD.hash;
		END IF;

		salt = coalesce(salt, floor(random() * 1000000000 + 1)::bigint);
		NEW.salt = salt;
		NEW.hash = qp_get_hash(NEW.password, salt);
		NEW.password = '';
		IF NEW.hash <> old_hash THEN
			NEW.password_modified = NOW();
		END IF;

		RETURN NEW;	
	END
$tiu_update_hash$ LANGUAGE plpgsql;

alter function update_hash() owner to postgres;


DO $$ BEGIN
    create trigger tbd_content
        before delete
        on content
        for each row
    execute procedure process_before_content_delete();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    create trigger tbd_delete_item_version
        before delete
        on content_item_version
        for each row
    execute procedure process_before_content_item_version_delete();
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    create trigger tbd_content_to_content
        before delete
        on content_to_content
        for each row
    execute procedure process_before_content_to_content_delete();
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;

DO $$ BEGIN
    create trigger tbd_delete_item
        before delete
        on content_item
        for each row
    execute procedure process_before_content_item_delete();
EXCEPTION
    WHEN duplicate_object THEN null;
END $$;


DO $$ BEGIN
    create trigger td_delete_item
        after delete
        on content_item
        REFERENCING OLD TABLE AS old_table
        FOR EACH STATEMENT
    execute procedure process_content_item_delete();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;


DO $$ BEGIN
    create trigger td_item_link_async
        after delete
        on item_link_async
        referencing OLD TABLE as old_table
        for each statement
    execute procedure process_item_to_item_delete();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;


DO $$ BEGIN
    create trigger td_item_to_item
        after delete
        on item_to_item
        referencing OLD TABLE as old_table
        for each statement
    execute procedure process_item_to_item_delete();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;



DO $$ BEGIN
    create trigger tiu_update_hash
        before insert or update
        on users
        for each row
    execute procedure update_hash();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;



DO $$ BEGIN
    create trigger ti_content_data_fill
        after insert
        on content_data
        REFERENCING NEW TABLE AS new_table
        FOR EACH STATEMENT
    execute procedure process_content_data_upsert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;



DO $$ BEGIN
    create trigger ti_insert_item
        after insert
        on content_item
        REFERENCING NEW TABLE AS new_table
        FOR EACH STATEMENT
    execute procedure process_content_item_insert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;



DO $$ BEGIN
    create trigger ti_item_link_async
        after insert
        on item_link_async
        referencing NEW TABLE as new_table
        for each statement
    execute procedure process_item_to_item_insert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;




DO $$ BEGIN
    create trigger ti_item_to_item
        after insert
        on item_to_item
        referencing NEW TABLE as new_table
        for each statement
    execute procedure process_item_to_item_insert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;




DO $$ BEGIN
    create trigger tu_content_data_fill
        after update
        on content_data
        REFERENCING NEW TABLE AS new_table OLD table as old_table
        FOR EACH STATEMENT
    execute procedure process_content_data_upsert();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;


DO $$ BEGIN
    create trigger tu_update_item
        after update
        on content_item
        REFERENCING NEW TABLE AS new_table OLD TABLE AS old_table
        FOR EACH STATEMENT
    execute procedure process_content_item_update();

EXCEPTION
    WHEN duplicate_object THEN null;
END $$;



DO LANGUAGE plpgsql $$
DECLARE
	cids int[];
	cid int;
BEGIN
	cids := array_agg(content_id) from content where virtual_type = 0;
	foreach cid in array cids
	loop
		call qp_content_united_view_create(cid);
	end loop;
END;
$$;
DO LANGUAGE plpgsql $$
DECLARE
	lids int[];
	lid int;
BEGIN
	lids := array_agg(link_id) from content_to_content where link_id in (select link_id from content_attribute);
	foreach lid in array lids
	loop
		call qp_link_view_create(lid);
	end loop;
END;
$$;
DO LANGUAGE plpgsql $$
DECLARE
	cids int[];
	cid int;
BEGIN
	cids := array_agg(content_id) from content where virtual_type = 0;
	foreach cid in array cids
	loop
		call qp_content_new_views_create(cid);
	end loop;
END;
$$;
ALTER TABLE public.site ADD COLUMN IF NOT EXISTS replace_urls_in_db boolean NOT NULL DEFAULT false;
ALTER TABLE public.content_item_schedule 
    ADD COLUMN IF NOT EXISTS start_date timestamp NULL;

ALTER TABLE public.content_item_schedule 
    ADD COLUMN IF NOT EXISTS end_date timestamp NULL;


ALTER TABLE CONTENT_ATTRIBUTE ADD COLUMN IF NOT EXISTS TRACE_IMPORT boolean NOT NULL DEFAULT false;
ALTER TABLE CONTENT ADD COLUMN IF NOT EXISTS TRACE_IMPORT_SCRIPT text NULL;
ALTER TABLE CONTENT_ATTRIBUTE ADD COLUMN IF NOT EXISTS DENY_PAST_DATES boolean NOT NULL DEFAULT false;

ALTER TABLE public.workflow ADD COLUMN IF NOT EXISTS is_default boolean NOT NULL DEFAULT false;

