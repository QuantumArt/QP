-- auto-generated definition
create trigger tbd_delete_item
    before delete
    on content_item
    for each row
execute procedure process_before_content_item_delete();

