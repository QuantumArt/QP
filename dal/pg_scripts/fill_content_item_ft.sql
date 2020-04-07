INSERT INTO content_item_ft (content_item_id, ft_data)
SELECT ci.content_item_id, qp_get_article_tsvector(ci.content_item_id::int) from content_item ci
ON CONFLICT(content_item_id)
DO UPDATE SET ft_data = qp_get_article_tsvector(EXCLUDED.content_item_id::int);