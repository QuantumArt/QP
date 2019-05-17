-- auto-generated definition
create trigger tiu_update_hash
    before insert or update
    on users
    for each row
execute procedure update_hash();

