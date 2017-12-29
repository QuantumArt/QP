CREATE PROCEDURE SyncProductVersions
AS
BEGIN
	insert into ProductVersions(Deleted, Modification, DpcId, Version, IsLive, Language, Format, Data, Alias, Created, Updated, Hash, MarketingProductId, Title, UserUpdated, UserUpdatedId, ProductType)
	select
	 1 Deleted,
	 p.Updated Modification,
	 p.DpcId,
	 p.Version,
	 p.IsLive,
	 p.Language,
	 p.Format,
	 p.Data,
	 p.Alias,
	 p.Created,
	 p.Updated,
	 p.Hash,
	 p.MarketingProductId,
	 p.Title,
	 p.UserUpdated,
	 p.UserUpdatedId,
	 p.ProductType
	from Products p
	where not exists (
		select null
		from ProductVersions v with (nolock)
		where
			p.[Updated] = v.[Modification] and
			p.[DpcId] = v.[DpcId] and
			p.[IsLive] = v.[IsLive] and
			p.[Language] = v.[Language]
			and p.[Format] = v.[Format]
		)


	insert ProductRegionVersions([ProductVersionId], [RegionId])
	select v.[Id], r.[RegionId]
	from
		Products p
		join ProductVersions v on
			p.[Updated] = v.[Modification] and
			p.[DpcId] = v.[DpcId] and
			p.[IsLive] = v.[IsLive] and
			p.[Language] = v.[Language]
			and p.[Format] = v.[Format]
		join ProductRegions r on
			p.Id = r.ProductId
	where
		not exists (select null from ProductRegionVersions rv with (nolock) where rv.[ProductVersionId] = v.[Id])
END
GO