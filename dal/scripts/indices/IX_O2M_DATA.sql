if not exists(select * from sys.indexes where name = 'IX_O2M_DATA' and [object_id] = object_id('CONTENT_DATA'))
begin
    create index IX_O2M_DATA on CONTENT_DATA(O2M_DATA) WHERE O2M_DATA IS NOT NULL
end

if not exists(select * from sys.indexes where name = 'IX_O2M_DATA' and [object_id] = object_id('VERSION_CONTENT_DATA'))
begin
    create index IX_O2M_DATA on VERSION_CONTENT_DATA(O2M_DATA) WHERE O2M_DATA IS NOT NULL
end
go