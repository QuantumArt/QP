CREATE PROCEDURE SyncProductVersions
AS
BEGIN
	print('Start update ProductVersions...')

	WHILE exists(
		select null
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
	)
	BEGIN
		insert into ProductVersions(Deleted, Modification, DpcId, Version, IsLive, Language, Format, Data, Alias, Created, Updated, Hash, MarketingProductId, Title, UserUpdated, UserUpdatedId, ProductType)
		select top 1000
			0 Deleted,
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
	END

	print('End update ProductVersions')
END
GO