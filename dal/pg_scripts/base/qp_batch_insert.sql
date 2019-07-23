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