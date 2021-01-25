INSERT INTO content_item_ft (content_item_id, ft_data)
SELECT ci.content_item_id, qp_get_article_tsvector(ci.content_item_id::int) from content_item ci
where not exists(
    select * from content_item_ft cif where cif.content_item_id = ci.content_item_id
);