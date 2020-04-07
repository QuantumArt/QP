ALTER TRIGGER [dbo].[tu_content_indexes] ON [dbo].[content_constraint] 
FOR UPDATE 
AS
BEGIN
	if object_id('tempdb..#disable_tu_content_indexes') is null
	BEGIN
        DECLARE @constraint_id numeric
        DECLARE @content_id numeric
        DECLARE @attribute_id numeric
        DECLARE @count numeric
        SELECT @constraint_id = COUNT(constraint_id) FROM deleted
        IF @constraint_id <> 0 
        BEGIN
            DECLARE Constraints CURSOR FOR SELECT constraint_id, content_id FROM deleted
            OPEN Constraints
            FETCH NEXT FROM Constraints INTO @constraint_id, @content_id
            WHILE @@fetch_status = 0 BEGIN
                print @constraint_id
                print @content_id
                exec qp_drop_complex_index @constraint_id, 1, @content_id
                exec qp_drop_complex_index @constraint_id, 0, @content_id
                FETCH NEXT FROM Constraints INTO @constraint_id, @content_id
            END
            CLOSE Constraints
            DEALLOCATE Constraints
        END
        DECLARE Constraints CURSOR FOR SELECT constraint_id FROM inserted
        OPEN Constraints
        FETCH NEXT FROM Constraints INTO @constraint_id
        WHILE @@fetch_status = 0 BEGIN
            SELECT @count = count(constraint_id),@attribute_id = min(attribute_id) FROM content_constraint_rule WHERE constraint_id = @constraint_id
            IF (@count = 1)
                UPDATE content_attribute SET index_flag = 1 WHERE attribute_id = @attribute_id
            ElSE
                IF (@count > 1)
                BEGIN
                    exec qp_create_complex_index @constraint_id, 1
                    exec qp_create_complex_index @constraint_id, 0
                END
            FETCH NEXT FROM Constraints INTO @constraint_id
        END
        CLOSE Constraints
        DEALLOCATE Constraints
    END
END
GO