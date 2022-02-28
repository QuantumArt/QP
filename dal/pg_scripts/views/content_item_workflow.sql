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



