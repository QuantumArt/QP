-- DROP TRIGGER ti_item_to_item ON item_to_item;

create trigger ti_item_to_item
    after insert
    on item_to_item
    referencing NEW TABLE as new_table
    for each statement
execute procedure process_item_to_item_insert();


