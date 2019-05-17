-- DROP TRIGGER td_item_to_item ON item_to_item;

create trigger td_item_to_item
    after delete
    on item_to_item
    referencing OLD TABLE as old_table
    for each statement
execute procedure process_item_to_item_delete();

