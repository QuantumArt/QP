exec qp_drop_existing 'qp_assert_num_equal', 'IsProcedure'
GO

CREATE PROCEDURE [dbo].[qp_assert_num_equal]
@id1 int,
@id2 int,
@msg nvarchar(50)
AS
BEGIN
  declare @text nvarchar(max)
  set @text = @msg + ': '
  if @id1 = @id2
  begin
    set @text = @text + 'OK'
    print @text
  end
  else
  begin
    set @text = @text + 'Failed - %d/%d'
    raiserror(@text, 11, 1, @id1, @id2)
  end
END
GO
