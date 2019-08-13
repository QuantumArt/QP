ALTER FUNCTION [dbo].[qp_correct_data] (
@data nvarchar(max),
@type_id numeric,
@length numeric,
@default_value nvarchar(255)
) RETURNS nvarchar(max)
AS
BEGIN
	declare @num numeric, @err numeric
	declare @return_data nvarchar(max)
	if @type_id in (1, 7, 8, 12) begin
		set @return_data = left(@data, @length)
	end
	else if @type_id in (2, 3, 11) begin
		if isnumeric(@data) = 1 or @data is null
			set @return_data = @data
		else if isnumeric(@default_value) = 1
			set @return_data = @default_value
		else
			set @return_data = null
	end
	else if @type_id in (4, 5, 6) begin
		if isdate(@data) = 1 or @data is null
			set @return_data = @data
		else if isdate(@default_value) = 1
			set @return_data = @default_value
		else
			set @return_data = null
	end
	else begin
		set @return_data = @data
	end
	RETURN @return_data
END
GO