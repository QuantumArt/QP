CREATE FUNCTION GetProducts(@date datetime)
RETURNS TABLE
AS
RETURN
(
	select *
	from
		ProductVersions v with (nolock)
	where
		not exists (
			select null
			from ProductVersions v2 with (nolock)
			where
				v2.[Id] > v.[Id]
				and v2.[DpcId] = v.[DpcId]
				and v2.[IsLive] = v.[IsLive]
				and v2.[Language] = v.[Language]
				and v2.[Format] = v.[Format]
				and v2.[Modification] <= @date)
		and v.[Deleted] = 0
		and v.[Modification] <= @date
)
GO