EXEC qp_drop_existing 'qp_merge_delays', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_merge_delays]
@ids [Ids] READONLY,
@lastModifiedBy int
AS
BEGIN
    if exists(select * from CHILD_DELAYS cd inner join @ids i on cd.id = i.ID)
    BEGIN
        declare @ids_to_merge [Ids]

        insert into @ids_to_merge
        select child_id from CHILD_DELAYS cd1 where id in (select id from @ids)
        and not exists(select * from CHILD_DELAYS cd2 where cd2.child_id = cd1.child_id and cd2.id <> cd1.id)

        exec qp_merge_articles @ids_to_merge, @lastModifiedBy

        delete from child_delays where id in (select id from @ids)
    END
END
GO
