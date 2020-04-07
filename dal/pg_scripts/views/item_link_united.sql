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

