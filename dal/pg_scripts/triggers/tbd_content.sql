create trigger tbd_content
    before delete
    on content
    for each row
execute procedure process_before_content_delete();