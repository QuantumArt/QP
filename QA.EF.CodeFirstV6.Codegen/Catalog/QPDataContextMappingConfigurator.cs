using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Threading;


namespace QA.EF.CodeFirstV6.Codegen.Catalog
{
    public class QPDataContextMappingConfigurator : IMappingConfigurator
    {
	    public static ContentAccess DefaultContentAccess = ContentAccess.Live;
        private ContentAccess _contentAccess;
		private static ConcurrentDictionary<ContentAccess, Lazy<DbCompiledModel>> _cache = new ConcurrentDictionary<ContentAccess, Lazy<DbCompiledModel>>();

        public QPDataContextMappingConfigurator()
            : this(DefaultContentAccess)
        { }


        public QPDataContextMappingConfigurator(ContentAccess contentAccess)
        {
            _contentAccess = contentAccess;
        }

        public virtual DbCompiledModel GetBuiltModel(DbConnection connection)
        {           
            return _cache.GetOrAdd(_contentAccess, a =>
            {
                var builder = new DbModelBuilder();
                OnModelCreating(builder);
                var builtModel = builder.Build(connection);

                return new Lazy<DbCompiledModel>( 
                    () => builtModel.Compile(), 
                    LazyThreadSafetyMode.ExecutionAndPublication);
            }).Value;
        }


        public virtual void OnModelCreating(DbModelBuilder modelBuilder)
        {

            #region StatusType
            modelBuilder.Entity<StatusType>()
                .ToTable("STATUS_TYPE_NEW")
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("id");

            modelBuilder.Entity<StatusType>().Property(e => e.SiteId).HasColumnName("site_id");
            modelBuilder.Entity<StatusType>().Property(e => e.StatusTypeName).HasColumnName("name");
            modelBuilder.Entity<StatusType>().Property(e => e.Weight).HasColumnName("weight");
            #endregion

            #region User
            modelBuilder.Entity<User>()
                .ToTable("USER_NEW")
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("id");

            modelBuilder.Entity<User>().Property(e => e.FirstName).HasColumnName("first_name");
            modelBuilder.Entity<User>().Property(e => e.LastName).HasColumnName("last_name");
            modelBuilder.Entity<User>().Property(e => e.NTLogin).HasColumnName("nt_login");
			modelBuilder.Entity<User>().Property(e => e.ISOCode).HasColumnName("iso_code");
            #endregion

            #region UserGroup
            modelBuilder.Entity<UserGroup>()
                .ToTable("USER_GROUP_NEW")
                .Property(e => e.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("id");


            modelBuilder.Entity<UserGroup>()
                .HasMany(e => e.Users)
                .WithMany(e => e.UserGroups)
                .Map(m => m.ToTable("USER_GROUP_BIND_NEW").MapLeftKey("GROUP_ID").MapRightKey("USER_ID"));

            #endregion

            #region GeoRegion mappings
            modelBuilder.Entity<GeoRegion >()
                .ToTable(GetTableName("288"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<GeoRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<GeoRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<GeoRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<GeoRegion>()
                .HasOptional<GeoRegion>(mp => mp.Parent)
                .WithMany(mp => mp.Children)
                .HasForeignKey(fp => fp.Parent_ID);

            modelBuilder.Entity<GeoRegion>()
                .Property(x => x.Parent_ID)
                .HasColumnName("Parent");
            modelBuilder.Entity<GeoRegion>()
                .HasOptional<MarketingRegion>(mp => mp.MarketingRegion)
                .WithMany(mp => mp.GeoRegions)
                .HasForeignKey(fp => fp.MarketingRegion_ID);

            modelBuilder.Entity<GeoRegion>()
                .Property(x => x.MarketingRegion_ID)
                .HasColumnName("MarketingRegion");

 
            #endregion

            #region MarketingRegion mappings
            modelBuilder.Entity<MarketingRegion >()
                .ToTable(GetTableName("289"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingRegion>()
                .HasOptional<MarketingRegion>(mp => mp.Parent)
                .WithMany(mp => mp.Children)
                .HasForeignKey(fp => fp.Parent_ID);

            modelBuilder.Entity<MarketingRegion>()
                .Property(x => x.Parent_ID)
                .HasColumnName("Parent");

 
            #endregion

            #region GeoRegionsIPAddress mappings
            modelBuilder.Entity<GeoRegionsIPAddress >()
                .ToTable(GetTableName("290"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<GeoRegionsIPAddress>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<GeoRegionsIPAddress>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<GeoRegionsIPAddress>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<GeoRegionsIPAddress>()
                .HasOptional<GeoRegion>(mp => mp.GeoRegion)
                .WithMany(mp => mp.GeoRegionIpAddresses)
                .HasForeignKey(fp => fp.GeoRegion_ID);

            modelBuilder.Entity<GeoRegionsIPAddress>()
                .Property(x => x.GeoRegion_ID)
                .HasColumnName("GeoRegion");

 
            #endregion

            #region SiteProduct mappings
            modelBuilder.Entity<SiteProduct >()
                .ToTable(GetTableName("291"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SiteProduct>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SiteProduct>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SiteProduct>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SiteProduct>()
                .HasOptional<MarketingRegion>(mp => mp.DefaultRegion)
                .WithMany(mp => mp.SiteProductDefaults)
                .HasForeignKey(fp => fp.DefaultRegion_ID);

            modelBuilder.Entity<SiteProduct>()
                .Property(x => x.DefaultRegion_ID)
                .HasColumnName("DefaultRegion");
            modelBuilder.Entity<SiteProduct>()
                .HasOptional<DeviceType>(mp => mp.DefaultDeviceType)
                .WithMany(mp => mp.DefaultSiteProducts)
                .HasForeignKey(fp => fp.DefaultDeviceType_ID);

            modelBuilder.Entity<SiteProduct>()
                .Property(x => x.DefaultDeviceType_ID)
                .HasColumnName("DefaultDeviceType");
            modelBuilder.Entity<SiteProduct>()
                .HasOptional<FeedbackTheme>(mp => mp.FeedbackDefaultTheme)
                .WithMany(mp => mp.SiteProducts)
                .HasForeignKey(fp => fp.FeedbackDefaultTheme_ID);

            modelBuilder.Entity<SiteProduct>()
                .Property(x => x.FeedbackDefaultTheme_ID)
                .HasColumnName("FeedbackDefaultTheme");

            modelBuilder.Entity<SiteProduct>().HasMany<MarketingRegion>(p => p.MarketingRegions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("12"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<SiteProduct>(p => p.SiteProducts).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("12"));
                });


            modelBuilder.Entity<SiteProduct>().HasMany<DeviceType>(p => p.DeviceTypes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("47"));
                });

            modelBuilder.Entity<DeviceType>().HasMany<SiteProduct>(p => p.SiteProducts).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("47"));
                });


 
            #endregion

            #region QPAbstractItem mappings
            modelBuilder.Entity<QPAbstractItem >()
                .ToTable(GetTableName("293"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QPAbstractItem>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QPAbstractItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QPAbstractItem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<QPAbstractItem>()
                .HasOptional<QPAbstractItem>(mp => mp.Parent)
                .WithMany(mp => mp.Children)
                .HasForeignKey(fp => fp.Parent_ID);

            modelBuilder.Entity<QPAbstractItem>()
                .Property(x => x.Parent_ID)
                .HasColumnName("Parent");
            modelBuilder.Entity<QPAbstractItem>()
                .HasOptional<QPDiscriminator>(mp => mp.Discriminator)
                .WithMany(mp => mp.Items)
                .HasForeignKey(fp => fp.Discriminator_ID);

            modelBuilder.Entity<QPAbstractItem>()
                .Property(x => x.Discriminator_ID)
                .HasColumnName("Discriminator");
            modelBuilder.Entity<QPAbstractItem>()
                .HasOptional<QPAbstractItem>(mp => mp.VersionOf)
                .WithMany(mp => mp.Versions)
                .HasForeignKey(fp => fp.VersionOf_ID);

            modelBuilder.Entity<QPAbstractItem>()
                .Property(x => x.VersionOf_ID)
                .HasColumnName("VersionOf");
            modelBuilder.Entity<QPAbstractItem>()
                .HasOptional<QPCulture>(mp => mp.Culture)
                .WithMany(mp => mp.AbstractItems)
                .HasForeignKey(fp => fp.Culture_ID);

            modelBuilder.Entity<QPAbstractItem>()
                .Property(x => x.Culture_ID)
                .HasColumnName("Culture");
            modelBuilder.Entity<QPAbstractItem>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.Item)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<QPAbstractItem>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");

            modelBuilder.Entity<QPAbstractItem>().HasMany<QPRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("13"));
                });

            modelBuilder.Entity<QPRegion>().HasMany<QPAbstractItem>(p => p.BackwardForRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("13"));
                });


 
            #endregion

            #region QPDiscriminator mappings
            modelBuilder.Entity<QPDiscriminator >()
                .ToTable(GetTableName("294"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QPDiscriminator>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QPDiscriminator>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QPDiscriminator>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<QPDiscriminator>().HasMany<QPDiscriminator>(p => p.AllowedItemDefinitions1).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("42"));
                });

            modelBuilder.Entity<QPDiscriminator>().HasMany<QPDiscriminator>(p => p.BackwardForAllowedItemDefinitions1).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("linked_id"); // ===
                    rp.MapRightKey("id");
                    rp.ToTable(GetReversedLinkTableName("42"));
                });


            modelBuilder.Entity<QPDiscriminator>().Ignore(p => p.IconUrlUrl);
            modelBuilder.Entity<QPDiscriminator>().Ignore(p => p.IconUrlUploadPath);
 
            #endregion

            #region QPCulture mappings
            modelBuilder.Entity<QPCulture >()
                .ToTable(GetTableName("295"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QPCulture>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QPCulture>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QPCulture>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<QPCulture>().Ignore(p => p.IconUrl);
            modelBuilder.Entity<QPCulture>().Ignore(p => p.IconUploadPath);
 
            #endregion

            #region ItemTitleFormat mappings
            modelBuilder.Entity<ItemTitleFormat >()
                .ToTable(GetTableName("296"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ItemTitleFormat>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ItemTitleFormat>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ItemTitleFormat>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region QPRegion mappings
            modelBuilder.Entity<QPRegion >()
                .ToTable(GetTableName("300"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QPRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QPRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QPRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region Poll mappings
            modelBuilder.Entity<Poll >()
                .ToTable(GetTableName("301"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Poll>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Poll>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Poll>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<Poll>()
                .Property(x => x.Title)
                .HasColumnName("Название");
            modelBuilder.Entity<Poll>()
                .Property(x => x.MarketCodes)
                .HasColumnName("Маркетинговые коды");
            modelBuilder.Entity<Poll>()
                .Property(x => x.StartDate)
                .HasColumnName("Начало проведения опроса");
            modelBuilder.Entity<Poll>()
                .Property(x => x.EndDate)
                .HasColumnName("Окончание проведения опроса");

 
            #endregion

            #region PollQuestion mappings
            modelBuilder.Entity<PollQuestion >()
                .ToTable(GetTableName("302"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PollQuestion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PollQuestion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PollQuestion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<PollQuestion>()
                .HasOptional<Poll>(mp => mp.Poll)
                .WithMany(mp => mp.Questions)
                .HasForeignKey(fp => fp.Poll_ID);

            modelBuilder.Entity<PollQuestion>()
                .Property(x => x.Poll_ID)
                .HasColumnName("Poll");

 
            #endregion

            #region PollAnswer mappings
            modelBuilder.Entity<PollAnswer >()
                .ToTable(GetTableName("303"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PollAnswer>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PollAnswer>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PollAnswer>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<PollAnswer>()
                .HasOptional<PollQuestion>(mp => mp.Question)
                .WithMany(mp => mp.Answers)
                .HasForeignKey(fp => fp.Question_ID);

            modelBuilder.Entity<PollAnswer>()
                .Property(x => x.Question_ID)
                .HasColumnName("Question");

 
            #endregion

            #region UserPollAnswer mappings
            modelBuilder.Entity<UserPollAnswer >()
                .ToTable(GetTableName("304"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<UserPollAnswer>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<UserPollAnswer>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<UserPollAnswer>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<UserPollAnswer>()
                .HasOptional<PollAnswer>(mp => mp.Answer)
                .WithMany(mp => mp.UserAnswers)
                .HasForeignKey(fp => fp.Answer_ID);

            modelBuilder.Entity<UserPollAnswer>()
                .Property(x => x.Answer_ID)
                .HasColumnName("Answer");
            modelBuilder.Entity<UserPollAnswer>()
                .HasOptional<PollQuestion>(mp => mp.Question)
                .WithMany(mp => mp.UsersAnswers)
                .HasForeignKey(fp => fp.Question_ID);

            modelBuilder.Entity<UserPollAnswer>()
                .Property(x => x.Question_ID)
                .HasColumnName("Question");
            modelBuilder.Entity<UserPollAnswer>()
                .HasOptional<Poll>(mp => mp.Poll)
                .WithMany(mp => mp.UserAnswers)
                .HasForeignKey(fp => fp.Poll_ID);

            modelBuilder.Entity<UserPollAnswer>()
                .Property(x => x.Poll_ID)
                .HasColumnName("Poll");

 
            #endregion

            #region TrailedAbstractItem mappings
            modelBuilder.Entity<TrailedAbstractItem >()
                .ToTable(GetTableName("305"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TrailedAbstractItem>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TrailedAbstractItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TrailedAbstractItem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region QPObsoleteUrl mappings
            modelBuilder.Entity<QPObsoleteUrl >()
                .ToTable(GetTableName("307"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QPObsoleteUrl>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QPObsoleteUrl>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QPObsoleteUrl>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<QPObsoleteUrl>()
                .Property(x => x.Url)
                .HasColumnName("Ссылка");
            modelBuilder.Entity<QPObsoleteUrl>()
                .HasOptional<QPAbstractItem>(mp => mp.AbstractItem)
                .WithMany(mp => mp.ObsoleteUrls)
                .HasForeignKey(fp => fp.AbstractItem_ID);

            modelBuilder.Entity<QPObsoleteUrl>()
                .Property(x => x.AbstractItem_ID)
                .HasColumnName("AbstractItem");

 
            #endregion

            #region RegionTag mappings
            modelBuilder.Entity<RegionTag >()
                .ToTable(GetTableName("308"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<RegionTag>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<RegionTag>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<RegionTag>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region RegionTagValue mappings
            modelBuilder.Entity<RegionTagValue >()
                .ToTable(GetTableName("309"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<RegionTagValue>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<RegionTagValue>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<RegionTagValue>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<RegionTagValue>()
                .HasOptional<RegionTag>(mp => mp.RegionTag)
                .WithMany(mp => mp.RegionTagValues)
                .HasForeignKey(fp => fp.RegionTag_ID);

            modelBuilder.Entity<RegionTagValue>()
                .Property(x => x.RegionTag_ID)
                .HasColumnName("RegionTag");

            modelBuilder.Entity<RegionTagValue>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("17"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<RegionTagValue>(p => p.BackwardForRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("17"));
                });


 
            #endregion

            #region SearchSuggestion mappings
            modelBuilder.Entity<SearchSuggestion >()
                .ToTable(GetTableName("311"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SearchSuggestion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SearchSuggestion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SearchSuggestion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<SearchSuggestion>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("139"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<SearchSuggestion>(p => p.BackwardForRegions1).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("139"));
                });


 
            #endregion

            #region SearchResult mappings
            modelBuilder.Entity<SearchResult >()
                .ToTable(GetTableName("312"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SearchResult>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SearchResult>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SearchResult>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region SiteSection mappings
            modelBuilder.Entity<SiteSection >()
                .ToTable(GetTableName("313"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SiteSection>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SiteSection>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SiteSection>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region NewsCategory mappings
            modelBuilder.Entity<NewsCategory >()
                .ToTable(GetTableName("317"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<NewsCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<NewsCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<NewsCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region NewsArticle mappings
            modelBuilder.Entity<NewsArticle >()
                .ToTable(GetTableName("318"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<NewsArticle>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<NewsArticle>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<NewsArticle>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<NewsArticle>()
                .HasOptional<NewsCategory>(mp => mp.Category)
                .WithMany(mp => mp.NewsArticles)
                .HasForeignKey(fp => fp.Category_ID);

            modelBuilder.Entity<NewsArticle>()
                .Property(x => x.Category_ID)
                .HasColumnName("Category");
            modelBuilder.Entity<NewsArticle>()
                .HasOptional<SiteProduct>(mp => mp.SiteProduct)
                .WithMany(mp => mp.NewsArticles)
                .HasForeignKey(fp => fp.SiteProduct_ID);

            modelBuilder.Entity<NewsArticle>()
                .Property(x => x.SiteProduct_ID)
                .HasColumnName("SiteProduct");
            modelBuilder.Entity<NewsArticle>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.NewArticles)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<NewsArticle>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");

            modelBuilder.Entity<NewsArticle>().HasMany<MarketingRegion>(p => p.MarketRegions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("18"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<NewsArticle>(p => p.BackwardForMarketRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("18"));
                });


            modelBuilder.Entity<NewsArticle>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<NewsArticle>().Ignore(p => p.TileImageUrl);
            modelBuilder.Entity<NewsArticle>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<NewsArticle>().Ignore(p => p.TileImageUploadPath);
 
            #endregion

            #region NotificationTemplate mappings
            modelBuilder.Entity<NotificationTemplate >()
                .ToTable(GetTableName("319"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<NotificationTemplate>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<NotificationTemplate>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<NotificationTemplate>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region RoamingTariffZone mappings
            modelBuilder.Entity<RoamingTariffZone >()
                .ToTable(GetTableName("327"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<RoamingTariffZone>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<RoamingTariffZone>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<RoamingTariffZone>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<RoamingTariffZone>()
                .HasOptional<MarketingRegion>(mp => mp.InterCityRegion)
                .WithMany(mp => mp.InterCityTariffs)
                .HasForeignKey(fp => fp.InterCityRegion_ID);

            modelBuilder.Entity<RoamingTariffZone>()
                .Property(x => x.InterCityRegion_ID)
                .HasColumnName("InterCityRegion");

            modelBuilder.Entity<RoamingTariffZone>().HasMany<MarketingMobileService>(p => p.RecomendedServices).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("120"));
                });

            modelBuilder.Entity<MarketingMobileService>().HasMany<RoamingTariffZone>(p => p.BackwardForRecomendedServices).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("120"));
                });


 
            #endregion

            #region RoamingCountryZone mappings
            modelBuilder.Entity<RoamingCountryZone >()
                .ToTable(GetTableName("328"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<RoamingCountryZone>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<RoamingCountryZone>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<RoamingCountryZone>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<RoamingCountryZone>()
                .Property(x => x.Country_ID)
                .HasColumnName("Country");
            modelBuilder.Entity<RoamingCountryZone>()
                .Property(x => x.RoamingOperator_ID)
                .HasColumnName("RoamingOperator");
            modelBuilder.Entity<RoamingCountryZone>()
                .Property(x => x.LocalRoamingRegion_ID)
                .HasColumnName("LocalRoamingRegion");
            modelBuilder.Entity<RoamingCountryZone>()
                .Property(x => x.Country_ID)
                .HasColumnName("Country");
            modelBuilder.Entity<RoamingCountryZone>()
                .Property(x => x.RoamingOperator_ID)
                .HasColumnName("RoamingOperator");
            modelBuilder.Entity<RoamingCountryZone>()
                .Property(x => x.LocalRoamingRegion_ID)
                .HasColumnName("LocalRoamingRegion");
            modelBuilder.Entity<RoamingCountryZone>()
                .HasOptional<RoamingTariffZone>(mp => mp.TariffZone)
                .WithMany(mp => mp.CountriesInZone)
                .HasForeignKey(fp => fp.TariffZone_ID);

            modelBuilder.Entity<RoamingCountryZone>()
                .Property(x => x.TariffZone_ID)
                .HasColumnName("TariffZone");
            modelBuilder.Entity<RoamingCountryZone>()
                .HasOptional<LocalRoamingOperator>(mp => mp.LocalRoamingOperator)
                .WithMany(mp => mp.LocalRoamingOperatorInTarifZones)
                .HasForeignKey(fp => fp.LocalRoamingOperator_ID);

            modelBuilder.Entity<RoamingCountryZone>()
                .Property(x => x.LocalRoamingOperator_ID)
                .HasColumnName("LocalRoamingOperator");

 
            #endregion

            #region UserSubscription mappings
            modelBuilder.Entity<UserSubscription >()
                .ToTable(GetTableName("330"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<UserSubscription>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<UserSubscription>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<UserSubscription>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<UserSubscription>()
                .HasOptional<MarketingRegion>(mp => mp.MarketRegion)
                .WithMany(mp => mp.NewsSubscribers)
                .HasForeignKey(fp => fp.MarketRegion_ID);

            modelBuilder.Entity<UserSubscription>()
                .Property(x => x.MarketRegion_ID)
                .HasColumnName("MarketRegion");
            modelBuilder.Entity<UserSubscription>()
                .HasOptional<MarketingRegion>(mp => mp.NewRegion)
                .WithMany(mp => mp.UpdatedNewsSubscribers)
                .HasForeignKey(fp => fp.NewRegion_ID);

            modelBuilder.Entity<UserSubscription>()
                .Property(x => x.NewRegion_ID)
                .HasColumnName("NewRegion");

 
            #endregion

            #region SubscriptionCategory mappings
            modelBuilder.Entity<SubscriptionCategory >()
                .ToTable(GetTableName("331"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SubscriptionCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SubscriptionCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SubscriptionCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SubscriptionCategory>()
                .HasOptional<NewsCategory>(mp => mp.NewsCategory)
                .WithMany(mp => mp.SubscribedCategories)
                .HasForeignKey(fp => fp.NewsCategory_ID);

            modelBuilder.Entity<SubscriptionCategory>()
                .Property(x => x.NewsCategory_ID)
                .HasColumnName("NewsCategory");
            modelBuilder.Entity<SubscriptionCategory>()
                .HasOptional<UserSubscription>(mp => mp.Subscriber)
                .WithMany(mp => mp.SubscribedCategories)
                .HasForeignKey(fp => fp.Subscriber_ID);

            modelBuilder.Entity<SubscriptionCategory>()
                .Property(x => x.Subscriber_ID)
                .HasColumnName("Subscriber");

 
            #endregion

            #region ConfirmationRequest mappings
            modelBuilder.Entity<ConfirmationRequest >()
                .ToTable(GetTableName("332"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ConfirmationRequest>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ConfirmationRequest>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ConfirmationRequest>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ConfirmationRequest>()
                .HasOptional<UserSubscription>(mp => mp.Subscription)
                .WithMany(mp => mp.ConfirmationRequests)
                .HasForeignKey(fp => fp.Subscription_ID);

            modelBuilder.Entity<ConfirmationRequest>()
                .Property(x => x.Subscription_ID)
                .HasColumnName("Subscription");

 
            #endregion

            #region FeedbackTheme mappings
            modelBuilder.Entity<FeedbackTheme >()
                .ToTable(GetTableName("333"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FeedbackTheme>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FeedbackTheme>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FeedbackTheme>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region FeedbackType mappings
            modelBuilder.Entity<FeedbackType >()
                .ToTable(GetTableName("334"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FeedbackType>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FeedbackType>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FeedbackType>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region FeedbackSubtheme mappings
            modelBuilder.Entity<FeedbackSubtheme >()
                .ToTable(GetTableName("335"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FeedbackSubtheme>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FeedbackSubtheme>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FeedbackSubtheme>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<FeedbackSubtheme>()
                .HasOptional<FeedbackType>(mp => mp.FeedbackType)
                .WithMany(mp => mp.Subthemes)
                .HasForeignKey(fp => fp.FeedbackType_ID);

            modelBuilder.Entity<FeedbackSubtheme>()
                .Property(x => x.FeedbackType_ID)
                .HasColumnName("FeedbackType");
            modelBuilder.Entity<FeedbackSubtheme>()
                .HasOptional<FeedbackTheme>(mp => mp.Theme)
                .WithMany(mp => mp.Subthemes)
                .HasForeignKey(fp => fp.Theme_ID);

            modelBuilder.Entity<FeedbackSubtheme>()
                .Property(x => x.Theme_ID)
                .HasColumnName("Theme");
            modelBuilder.Entity<FeedbackSubtheme>()
                .HasOptional<FeedbackSubthemeGroup>(mp => mp.FeedbackSubthemeGroup)
                .WithMany(mp => mp.FeedbackSubthemes)
                .HasForeignKey(fp => fp.FeedbackSubthemeGroup_ID);

            modelBuilder.Entity<FeedbackSubtheme>()
                .Property(x => x.FeedbackSubthemeGroup_ID)
                .HasColumnName("FeedbackSubthemeGroup");

 
            #endregion

            #region FeedbackQueue mappings
            modelBuilder.Entity<FeedbackQueue >()
                .ToTable(GetTableName("336"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FeedbackQueue>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FeedbackQueue>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FeedbackQueue>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<FeedbackQueue>()
                .HasOptional<FeedbackTheme>(mp => mp.Theme)
                .WithMany(mp => mp.Queues)
                .HasForeignKey(fp => fp.Theme_ID);

            modelBuilder.Entity<FeedbackQueue>()
                .Property(x => x.Theme_ID)
                .HasColumnName("Theme");

            modelBuilder.Entity<FeedbackQueue>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("21"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<FeedbackQueue>(p => p.BackwardForRegions2).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("21"));
                });


 
            #endregion

            #region TVChannel mappings
            modelBuilder.Entity<TVChannel >()
                .ToTable(GetTableName("338"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TVChannel>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TVChannel>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TVChannel>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<TVChannel>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("52"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<TVChannel>(p => p.BackwardForRegions3).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("52"));
                });


            modelBuilder.Entity<TVChannel>().HasMany<TVChannelTheme>(p => p.Themes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("51"));
                });

            modelBuilder.Entity<TVChannelTheme>().HasMany<TVChannel>(p => p.Channels).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("51"));
                });


            modelBuilder.Entity<TVChannel>().Ignore(p => p.LogoImageUrl);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.GrayLogoUrl);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.GrayLogoSmallUrl);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.SmallLogoUrl);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.TinyLogoUrl);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.VideoPreviewImageUrl);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.PromoVideoUrl);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.LogoImageUploadPath);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.GrayLogoUploadPath);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.GrayLogoSmallUploadPath);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.VideoPreviewImageUploadPath);
            modelBuilder.Entity<TVChannel>().Ignore(p => p.PromoVideoUploadPath);
 
            #endregion

            #region RegionFeedbackGroup mappings
            modelBuilder.Entity<RegionFeedbackGroup >()
                .ToTable(GetTableName("344"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<RegionFeedbackGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<RegionFeedbackGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<RegionFeedbackGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<RegionFeedbackGroup>()
                .HasOptional<FeedbackType>(mp => mp.FeedbackType)
                .WithMany(mp => mp.RegionFeedbackGroups)
                .HasForeignKey(fp => fp.FeedbackType_ID);

            modelBuilder.Entity<RegionFeedbackGroup>()
                .Property(x => x.FeedbackType_ID)
                .HasColumnName("FeedbackType");

            modelBuilder.Entity<RegionFeedbackGroup>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("23"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<RegionFeedbackGroup>(p => p.BackwardForRegions4).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("23"));
                });


 
            #endregion

            #region DeviceType mappings
            modelBuilder.Entity<DeviceType >()
                .ToTable(GetTableName("345"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<DeviceType>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<DeviceType>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<DeviceType>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<DeviceType>().HasMany<MarketingMobileService>(p => p.BackwardForDeviceTypes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("48"));
                });

            modelBuilder.Entity<MarketingMobileService>().HasMany<DeviceType>(p => p.DeviceTypes).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("48"));
                });


            modelBuilder.Entity<DeviceType>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<DeviceType>().Ignore(p => p.IconForTariffUrl);
            modelBuilder.Entity<DeviceType>().Ignore(p => p.DefaultTariffTileIconUrl);
            modelBuilder.Entity<DeviceType>().Ignore(p => p.DefaultServiceTileIconUrl);
            modelBuilder.Entity<DeviceType>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<DeviceType>().Ignore(p => p.IconForTariffUploadPath);
            modelBuilder.Entity<DeviceType>().Ignore(p => p.DefaultTariffTileIconUploadPath);
            modelBuilder.Entity<DeviceType>().Ignore(p => p.DefaultServiceTileIconUploadPath);
 
            #endregion

            #region Action mappings
            modelBuilder.Entity<Action >()
                .ToTable(GetTableName("346"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Action>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Action>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Action>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<Action>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.Actions)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<Action>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");

            modelBuilder.Entity<Action>().HasMany<SiteProduct>(p => p.Products).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("24"));
                });

            modelBuilder.Entity<SiteProduct>().HasMany<Action>(p => p.BackwardForProducts).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("24"));
                });


            modelBuilder.Entity<Action>().HasMany<DeviceType>(p => p.DeviceTypes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("25"));
                });

            modelBuilder.Entity<DeviceType>().HasMany<Action>(p => p.BackwardForDeviceTypes1).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("25"));
                });


            modelBuilder.Entity<Action>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("26"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<Action>(p => p.BackwardForRegions5).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("26"));
                });


            modelBuilder.Entity<Action>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<Action>().Ignore(p => p.TileImageUrl);
            modelBuilder.Entity<Action>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<Action>().Ignore(p => p.TileImageUploadPath);
 
            #endregion

            #region TVPackage mappings
            modelBuilder.Entity<TVPackage >()
                .ToTable(GetTableName("348"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TVPackage>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TVPackage>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TVPackage>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<TVPackage>()
                .HasOptional<MarketingTVPackage>(mp => mp.MarketingPackage)
                .WithMany(mp => mp.TVPackages)
                .HasForeignKey(fp => fp.MarketingPackage_ID);

            modelBuilder.Entity<TVPackage>()
                .Property(x => x.MarketingPackage_ID)
                .HasColumnName("MarketingPackage");
            modelBuilder.Entity<TVPackage>()
                .HasOptional<SubscriptionFeeType>(mp => mp.SubscriptionFeeType)
                .WithMany(mp => mp.TVPackages)
                .HasForeignKey(fp => fp.SubscriptionFeeType_ID);

            modelBuilder.Entity<TVPackage>()
                .Property(x => x.SubscriptionFeeType_ID)
                .HasColumnName("SubscriptionFeeType");

            modelBuilder.Entity<TVPackage>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("29"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<TVPackage>(p => p.BackwardForRegions6).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("29"));
                });


            modelBuilder.Entity<TVPackage>().HasMany<MarketingTVPackage>(p => p.IncludedDiscountTvPackages).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("174"));
                });

            modelBuilder.Entity<MarketingTVPackage>().HasMany<TVPackage>(p => p.BackwardForIncludedDiscountTvPackages).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("174"));
                });


            modelBuilder.Entity<TVPackage>().HasMany<MarketingTVPackage>(p => p.ActivatedDiscountTvPackages).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("175"));
                });

            modelBuilder.Entity<MarketingTVPackage>().HasMany<TVPackage>(p => p.BackwardForActivatedDiscountTvPackages).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("175"));
                });


            modelBuilder.Entity<TVPackage>().Ignore(p => p.PDFUrl);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.PDFEnglUrl);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.PDFTatUrl);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.PDFUploadPath);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.PDFEnglUploadPath);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.PDFTatUploadPath);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<TVPackage>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region ExternalRegionMapping mappings
            modelBuilder.Entity<ExternalRegionMapping >()
                .ToTable(GetTableName("351"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ExternalRegionMapping>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ExternalRegionMapping>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ExternalRegionMapping>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ExternalRegionMapping>()
                .HasOptional<ExternalRegionSystem>(mp => mp.ExternalSystem)
                .WithMany(mp => mp.ExternalSystemMappings)
                .HasForeignKey(fp => fp.ExternalSystem_ID);

            modelBuilder.Entity<ExternalRegionMapping>()
                .Property(x => x.ExternalSystem_ID)
                .HasColumnName("ExternalSystem");

            modelBuilder.Entity<ExternalRegionMapping>().HasMany<MarketingRegion>(p => p.MarketingRegion).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("28"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<ExternalRegionMapping>(p => p.ExternalRegionMapping).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("28"));
                });


 
            #endregion

            #region ExternalRegionSystem mappings
            modelBuilder.Entity<ExternalRegionSystem >()
                .ToTable(GetTableName("352"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ExternalRegionSystem>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ExternalRegionSystem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ExternalRegionSystem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region SiteSetting mappings
            modelBuilder.Entity<SiteSetting >()
                .ToTable(GetTableName("353"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SiteSetting>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SiteSetting>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SiteSetting>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region PhoneCode mappings
            modelBuilder.Entity<PhoneCode >()
                .ToTable(GetTableName("357"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PhoneCode>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PhoneCode>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PhoneCode>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region QPItemDefinitionConstraint mappings
            modelBuilder.Entity<QPItemDefinitionConstraint >()
                .ToTable(GetTableName("359"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QPItemDefinitionConstraint>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QPItemDefinitionConstraint>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QPItemDefinitionConstraint>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<QPItemDefinitionConstraint>()
                .HasOptional<QPDiscriminator>(mp => mp.Source)
                .WithMany(mp => mp.AllowedItemDefinitions)
                .HasForeignKey(fp => fp.Source_ID);

            modelBuilder.Entity<QPItemDefinitionConstraint>()
                .Property(x => x.Source_ID)
                .HasColumnName("Source");
            modelBuilder.Entity<QPItemDefinitionConstraint>()
                .HasOptional<QPDiscriminator>(mp => mp.Target)
                .WithMany(mp => mp.AllowDefinition)
                .HasForeignKey(fp => fp.Target_ID);

            modelBuilder.Entity<QPItemDefinitionConstraint>()
                .Property(x => x.Target_ID)
                .HasColumnName("Target");

 
            #endregion

            #region MarketingMobileTariff mappings
            modelBuilder.Entity<MarketingMobileTariff >()
                .ToTable(GetTableName("360"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingMobileTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingMobileTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingMobileTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingMobileTariff>()
                .HasOptional<MarketingMobileTariff>(mp => mp.OtherPaymentSystemTariff)
                .WithMany(mp => mp.OtherPaymentSystemTariffs)
                .HasForeignKey(fp => fp.OtherPaymentSystemTariff_ID);

            modelBuilder.Entity<MarketingMobileTariff>()
                .Property(x => x.OtherPaymentSystemTariff_ID)
                .HasColumnName("OtherPaymentSystemTariff");
            modelBuilder.Entity<MarketingMobileTariff>()
                .HasOptional<MobileTariffFamily>(mp => mp.TariffFamily)
                .WithMany(mp => mp.MobileTariffs)
                .HasForeignKey(fp => fp.TariffFamily_ID);

            modelBuilder.Entity<MarketingMobileTariff>()
                .Property(x => x.TariffFamily_ID)
                .HasColumnName("TariffFamily");
            modelBuilder.Entity<MarketingMobileTariff>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.MarketingMobileTariffs)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<MarketingMobileTariff>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");

            modelBuilder.Entity<MarketingMobileTariff>().HasMany<DeviceType>(p => p.DeviceTypes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("31"));
                });

            modelBuilder.Entity<DeviceType>().HasMany<MarketingMobileTariff>(p => p.BackwardForDeviceTypes2).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("31"));
                });


            modelBuilder.Entity<MarketingMobileTariff>().HasMany<MobileTariffCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("33"));
                });

            modelBuilder.Entity<MobileTariffCategory>().HasMany<MarketingMobileTariff>(p => p.MarketingTariffs).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("33"));
                });


            modelBuilder.Entity<MarketingMobileTariff>().HasMany<MobileTariffParameterGroup>(p => p.PreOpenParamGroups).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("178"));
                });

            modelBuilder.Entity<MobileTariffParameterGroup>().HasMany<MarketingMobileTariff>(p => p.BackwardForPreOpenParamGroups).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("178"));
                });


            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.IconUrl);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.IconUploadPath);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconHoverTatUploadPath);
            modelBuilder.Entity<MarketingMobileTariff>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
 
            #endregion

            #region TariffGuideQuestion mappings
            modelBuilder.Entity<TariffGuideQuestion >()
                .ToTable(GetTableName("361"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TariffGuideQuestion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TariffGuideQuestion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TariffGuideQuestion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<TariffGuideQuestion>()
                .HasOptional<TariffGuideAnswer>(mp => mp.ParentAnswer)
                .WithMany(mp => mp.ChildQuestions)
                .HasForeignKey(fp => fp.ParentAnswer_ID);

            modelBuilder.Entity<TariffGuideQuestion>()
                .Property(x => x.ParentAnswer_ID)
                .HasColumnName("ParentAnswer");

            modelBuilder.Entity<TariffGuideQuestion>().HasMany<DeviceType>(p => p.DeviceTypes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("30"));
                });

            modelBuilder.Entity<DeviceType>().HasMany<TariffGuideQuestion>(p => p.BackwardForDeviceTypes3).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("30"));
                });


 
            #endregion

            #region TariffGuideAnswer mappings
            modelBuilder.Entity<TariffGuideAnswer >()
                .ToTable(GetTableName("362"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TariffGuideAnswer>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TariffGuideAnswer>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TariffGuideAnswer>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<TariffGuideAnswer>()
                .HasOptional<TariffGuideQuestion>(mp => mp.Question)
                .WithMany(mp => mp.Answers)
                .HasForeignKey(fp => fp.Question_ID);

            modelBuilder.Entity<TariffGuideAnswer>()
                .Property(x => x.Question_ID)
                .HasColumnName("Question");

 
            #endregion

            #region TariffGuideResult mappings
            modelBuilder.Entity<TariffGuideResult >()
                .ToTable(GetTableName("363"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TariffGuideResult>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TariffGuideResult>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TariffGuideResult>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<TariffGuideResult>()
                .HasOptional<DeviceType>(mp => mp.DeviceType)
                .WithMany(mp => mp.TariffGuideResults)
                .HasForeignKey(fp => fp.DeviceType_ID);

            modelBuilder.Entity<TariffGuideResult>()
                .Property(x => x.DeviceType_ID)
                .HasColumnName("DeviceType");

 
            #endregion

            #region MobileTariffParameterGroup mappings
            modelBuilder.Entity<MobileTariffParameterGroup >()
                .ToTable(GetTableName("364"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MobileTariffParameterGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MobileTariffParameterGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MobileTariffParameterGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<MobileTariffParameterGroup>().Ignore(p => p.IconUrl);
            modelBuilder.Entity<MobileTariffParameterGroup>().Ignore(p => p.IconUploadPath);
 
            #endregion

            #region MarketingSign mappings
            modelBuilder.Entity<MarketingSign >()
                .ToTable(GetTableName("365"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingSign>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingSign>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingSign>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region MarketingMobileService mappings
            modelBuilder.Entity<MarketingMobileService >()
                .ToTable(GetTableName("366"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingMobileService>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingMobileService>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingMobileService>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingMobileService>()
                .HasOptional<MobileServiceFamily>(mp => mp.ServiceFamily)
                .WithMany(mp => mp.MobileServices)
                .HasForeignKey(fp => fp.ServiceFamily_ID);

            modelBuilder.Entity<MarketingMobileService>()
                .Property(x => x.ServiceFamily_ID)
                .HasColumnName("ServiceFamily");
            modelBuilder.Entity<MarketingMobileService>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.MarketingMobileServices)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<MarketingMobileService>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");
            modelBuilder.Entity<MarketingMobileService>()
                .HasOptional<ServicePrefix>(mp => mp.Prefix)
                .WithMany(mp => mp.MobileMarketingServices)
                .HasForeignKey(fp => fp.Prefix_ID);

            modelBuilder.Entity<MarketingMobileService>()
                .Property(x => x.Prefix_ID)
                .HasColumnName("Prefix");

            modelBuilder.Entity<MarketingMobileService>().HasMany<MobileServiceCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("35"));
                });

            modelBuilder.Entity<MobileServiceCategory>().HasMany<MarketingMobileService>(p => p.MobileServices).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("35"));
                });


            modelBuilder.Entity<MarketingMobileService>().HasMany<MobileTariffParameterGroup>(p => p.TariffParameterGroups).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("37"));
                });

            modelBuilder.Entity<MobileTariffParameterGroup>().HasMany<MarketingMobileService>(p => p.BackwardForTariffParameterGroups).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("37"));
                });


            modelBuilder.Entity<MarketingMobileService>().HasMany<MobileServiceParameterGroup>(p => p.PreOpenParamGroups).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("179"));
                });

            modelBuilder.Entity<MobileServiceParameterGroup>().HasMany<MarketingMobileService>(p => p.BackwardForPreOpenParamGroups).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("179"));
                });


            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.IconUrl);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.IconUploadPath);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconHoverTatUploadPath);
            modelBuilder.Entity<MarketingMobileService>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
 
            #endregion

            #region SubscriptionFeeType mappings
            modelBuilder.Entity<SubscriptionFeeType >()
                .ToTable(GetTableName("367"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SubscriptionFeeType>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SubscriptionFeeType>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SubscriptionFeeType>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region MobileServiceParameterGroup mappings
            modelBuilder.Entity<MobileServiceParameterGroup >()
                .ToTable(GetTableName("368"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MobileServiceParameterGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MobileServiceParameterGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MobileServiceParameterGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region MobileTariffFamily mappings
            modelBuilder.Entity<MobileTariffFamily >()
                .ToTable(GetTableName("369"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MobileTariffFamily>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MobileTariffFamily>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MobileTariffFamily>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MobileTariffFamily>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.MobileTariffFamilies)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<MobileTariffFamily>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");

            modelBuilder.Entity<MobileTariffFamily>().HasMany<MobileTariffCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("34"));
                });

            modelBuilder.Entity<MobileTariffCategory>().HasMany<MobileTariffFamily>(p => p.TariffFamilies).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("34"));
                });


 
            #endregion

            #region MutualMobileServiceGroup mappings
            modelBuilder.Entity<MutualMobileServiceGroup >()
                .ToTable(GetTableName("370"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MutualMobileServiceGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MutualMobileServiceGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MutualMobileServiceGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<MutualMobileServiceGroup>().HasMany<MarketingMobileService>(p => p.MobileServices).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("32"));
                });

            modelBuilder.Entity<MarketingMobileService>().HasMany<MutualMobileServiceGroup>(p => p.BackwardForMobileServices).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("32"));
                });


 
            #endregion

            #region MobileServiceFamily mappings
            modelBuilder.Entity<MobileServiceFamily >()
                .ToTable(GetTableName("371"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MobileServiceFamily>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MobileServiceFamily>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MobileServiceFamily>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MobileServiceFamily>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.MobileServiceFamilies)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<MobileServiceFamily>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");

            modelBuilder.Entity<MobileServiceFamily>().HasMany<MobileServiceCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("36"));
                });

            modelBuilder.Entity<MobileServiceCategory>().HasMany<MobileServiceFamily>(p => p.ServiceFamilies).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("36"));
                });


 
            #endregion

            #region MobileTariffCategory mappings
            modelBuilder.Entity<MobileTariffCategory >()
                .ToTable(GetTableName("372"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MobileTariffCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MobileTariffCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MobileTariffCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region MobileServiceCategory mappings
            modelBuilder.Entity<MobileServiceCategory >()
                .ToTable(GetTableName("373"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MobileServiceCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MobileServiceCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MobileServiceCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ArchiveTariff mappings
            modelBuilder.Entity<ArchiveTariff >()
                .ToTable(GetTableName("374"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ArchiveTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ArchiveTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ArchiveTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ArchiveTariff>()
                .HasOptional<MarketingRegion>(mp => mp.Region)
                .WithMany(mp => mp.ArchiveTariffs)
                .HasForeignKey(fp => fp.Region_ID);

            modelBuilder.Entity<ArchiveTariff>()
                .Property(x => x.Region_ID)
                .HasColumnName("Region");
            modelBuilder.Entity<ArchiveTariff>()
                .HasOptional<DeviceType>(mp => mp.Product)
                .WithMany(mp => mp.ArchiveTariffs)
                .HasForeignKey(fp => fp.Product_ID);

            modelBuilder.Entity<ArchiveTariff>()
                .Property(x => x.Product_ID)
                .HasColumnName("Product");

 
            #endregion

            #region ArchiveTariffTab mappings
            modelBuilder.Entity<ArchiveTariffTab >()
                .ToTable(GetTableName("375"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ArchiveTariffTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ArchiveTariffTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ArchiveTariffTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ArchiveTariffTab>()
                .HasOptional<ArchiveTariff>(mp => mp.Tariff)
                .WithMany(mp => mp.Tabs)
                .HasForeignKey(fp => fp.Tariff_ID);

            modelBuilder.Entity<ArchiveTariffTab>()
                .Property(x => x.Tariff_ID)
                .HasColumnName("Tariff");

 
            #endregion

            #region MobileServiceTab mappings
            modelBuilder.Entity<MobileServiceTab >()
                .ToTable(GetTableName("376"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MobileServiceTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MobileServiceTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MobileServiceTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region MobileParamsGroupTab mappings
            modelBuilder.Entity<MobileParamsGroupTab >()
                .ToTable(GetTableName("377"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MobileParamsGroupTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MobileParamsGroupTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MobileParamsGroupTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MobileParamsGroupTab>()
                .HasOptional<MobileServiceTab>(mp => mp.Tab)
                .WithMany(mp => mp.ServiceParamGroups)
                .HasForeignKey(fp => fp.Tab_ID);

            modelBuilder.Entity<MobileParamsGroupTab>()
                .Property(x => x.Tab_ID)
                .HasColumnName("Tab");
            modelBuilder.Entity<MobileParamsGroupTab>()
                .HasOptional<MarketingMobileService>(mp => mp.MarketingService)
                .WithMany(mp => mp.MobileParamsGroupTabs)
                .HasForeignKey(fp => fp.MarketingService_ID);

            modelBuilder.Entity<MobileParamsGroupTab>()
                .Property(x => x.MarketingService_ID)
                .HasColumnName("MarketingService");

            modelBuilder.Entity<MobileParamsGroupTab>().HasMany<MobileServiceParameterGroup>(p => p.Groups).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("38"));
                });

            modelBuilder.Entity<MobileServiceParameterGroup>().HasMany<MobileParamsGroupTab>(p => p.BackwardForGroups).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("38"));
                });


 
            #endregion

            #region TariffParamGroupPriority mappings
            modelBuilder.Entity<TariffParamGroupPriority >()
                .ToTable(GetTableName("378"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TariffParamGroupPriority>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TariffParamGroupPriority>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TariffParamGroupPriority>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<TariffParamGroupPriority>()
                .HasOptional<MarketingMobileTariff>(mp => mp.Tariff)
                .WithMany(mp => mp.GroupPriorities)
                .HasForeignKey(fp => fp.Tariff_ID);

            modelBuilder.Entity<TariffParamGroupPriority>()
                .Property(x => x.Tariff_ID)
                .HasColumnName("Tariff");
            modelBuilder.Entity<TariffParamGroupPriority>()
                .HasOptional<MobileTariffParameterGroup>(mp => mp.ParamGroup)
                .WithMany(mp => mp.ParamGroupPriorities)
                .HasForeignKey(fp => fp.ParamGroup_ID);

            modelBuilder.Entity<TariffParamGroupPriority>()
                .Property(x => x.ParamGroup_ID)
                .HasColumnName("ParamGroup");

 
            #endregion

            #region ArchiveService mappings
            modelBuilder.Entity<ArchiveService >()
                .ToTable(GetTableName("379"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ArchiveService>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ArchiveService>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ArchiveService>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ArchiveService>()
                .Property(x => x.IsAction)
                .HasColumnName("Признак акции");
            modelBuilder.Entity<ArchiveService>()
                .Property(x => x.Note)
                .HasColumnName("Подсказка");
            modelBuilder.Entity<ArchiveService>()
                .Property(x => x.Price)
                .HasColumnName("Цена");
            modelBuilder.Entity<ArchiveService>()
                .HasOptional<MarketingRegion>(mp => mp.Region)
                .WithMany(mp => mp.OldMobileServices)
                .HasForeignKey(fp => fp.Region_ID);

            modelBuilder.Entity<ArchiveService>()
                .Property(x => x.Region_ID)
                .HasColumnName("Region");

 
            #endregion

            #region ArchiveMobileServiceBookmark mappings
            modelBuilder.Entity<ArchiveMobileServiceBookmark >()
                .ToTable(GetTableName("380"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ArchiveMobileServiceBookmark>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ArchiveMobileServiceBookmark>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ArchiveMobileServiceBookmark>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ArchiveMobileServiceBookmark>()
                .Property(x => x.Order)
                .HasColumnName("Порядок");
            modelBuilder.Entity<ArchiveMobileServiceBookmark>()
                .Property(x => x.FullDescription)
                .HasColumnName("Полное описание");
            modelBuilder.Entity<ArchiveMobileServiceBookmark>()
                .Property(x => x.Description)
                .HasColumnName("Описание");
            modelBuilder.Entity<ArchiveMobileServiceBookmark>()
                .HasOptional<ArchiveService>(mp => mp.Service)
                .WithMany(mp => mp.Bookmarks)
                .HasForeignKey(fp => fp.Service_ID);

            modelBuilder.Entity<ArchiveMobileServiceBookmark>()
                .Property(x => x.Service_ID)
                .HasColumnName("Service");

 
            #endregion

            #region PrivelegeAndBonus mappings
            modelBuilder.Entity<PrivelegeAndBonus >()
                .ToTable(GetTableName("381"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PrivelegeAndBonus>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PrivelegeAndBonus>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PrivelegeAndBonus>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<PrivelegeAndBonus>()
                .Property(x => x.IsBlank)
                .HasColumnName("DisplayEmpty");
            modelBuilder.Entity<PrivelegeAndBonus>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.PrivelegeAndBonuses)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<PrivelegeAndBonus>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");

            modelBuilder.Entity<PrivelegeAndBonus>().HasMany<SiteProduct>(p => p.Products).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("39"));
                });

            modelBuilder.Entity<SiteProduct>().HasMany<PrivelegeAndBonus>(p => p.BackwardForProducts1).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("39"));
                });


            modelBuilder.Entity<PrivelegeAndBonus>().HasMany<DeviceType>(p => p.DeviceTypes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("40"));
                });

            modelBuilder.Entity<DeviceType>().HasMany<PrivelegeAndBonus>(p => p.BackwardForDeviceTypes4).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("40"));
                });


            modelBuilder.Entity<PrivelegeAndBonus>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("41"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<PrivelegeAndBonus>(p => p.BackwardForRegions7).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("41"));
                });


            modelBuilder.Entity<PrivelegeAndBonus>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<PrivelegeAndBonus>().Ignore(p => p.TileImageUrl);
            modelBuilder.Entity<PrivelegeAndBonus>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<PrivelegeAndBonus>().Ignore(p => p.TileImageUploadPath);
 
            #endregion

            #region FaqContent mappings
            modelBuilder.Entity<FaqContent >()
                .ToTable(GetTableName("384"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FaqContent>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FaqContent>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FaqContent>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<FaqContent>()
                .HasOptional<FaqGroup>(mp => mp.Group)
                .WithMany(mp => mp.Questions)
                .HasForeignKey(fp => fp.Group_ID);

            modelBuilder.Entity<FaqContent>()
                .Property(x => x.Group_ID)
                .HasColumnName("Group");

            modelBuilder.Entity<FaqContent>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("59"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<FaqContent>(p => p.BackwardForRegions8).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("59"));
                });


            modelBuilder.Entity<FaqContent>().HasMany<FeedbackSubtheme>(p => p.FeedbackSubThemes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("159"));
                });

            modelBuilder.Entity<FeedbackSubtheme>().HasMany<FaqContent>(p => p.FaqContents).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("159"));
                });


            modelBuilder.Entity<FaqContent>().HasMany<FeedbackTheme>(p => p.FeedbackThemes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("158"));
                });

            modelBuilder.Entity<FeedbackTheme>().HasMany<FaqContent>(p => p.FaqContents).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("158"));
                });


 
            #endregion

            #region FaqGroup mappings
            modelBuilder.Entity<FaqGroup >()
                .ToTable(GetTableName("385"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FaqGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FaqGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FaqGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region SiteText mappings
            modelBuilder.Entity<SiteText >()
                .ToTable(GetTableName("397"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SiteText>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SiteText>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SiteText>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region TVChannelTheme mappings
            modelBuilder.Entity<TVChannelTheme >()
                .ToTable(GetTableName("399"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TVChannelTheme>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TVChannelTheme>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TVChannelTheme>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<TVChannelTheme>().Ignore(p => p.IconUrl);
            modelBuilder.Entity<TVChannelTheme>().Ignore(p => p.IconUploadPath);
 
            #endregion

            #region MarketingTVPackage mappings
            modelBuilder.Entity<MarketingTVPackage >()
                .ToTable(GetTableName("400"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingTVPackage>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingTVPackage>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingTVPackage>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingTVPackage>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.MarketingTVPackages)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<MarketingTVPackage>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");
            modelBuilder.Entity<MarketingTVPackage>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.MarketingTVPackages)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<MarketingTVPackage>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");
            modelBuilder.Entity<MarketingTVPackage>()
                .HasOptional<MarketingTVPackage>(mp => mp.RecomendedTariff)
                .WithMany(mp => mp.RecomendedPackages)
                .HasForeignKey(fp => fp.RecomendedTariff_ID);

            modelBuilder.Entity<MarketingTVPackage>()
                .Property(x => x.RecomendedTariff_ID)
                .HasColumnName("RecomendedTariff");
            modelBuilder.Entity<MarketingTVPackage>()
                .HasOptional<TVPackageFamily>(mp => mp.Family)
                .WithMany(mp => mp.MarketingTVPackages)
                .HasForeignKey(fp => fp.Family_ID);

            modelBuilder.Entity<MarketingTVPackage>()
                .Property(x => x.Family_ID)
                .HasColumnName("Family");
            modelBuilder.Entity<MarketingTVPackage>()
                .HasOptional<INACParamType>(mp => mp.InacParamType)
                .WithMany(mp => mp.MarketingTVPackages)
                .HasForeignKey(fp => fp.InacParamType_ID);

            modelBuilder.Entity<MarketingTVPackage>()
                .Property(x => x.InacParamType_ID)
                .HasColumnName("InacParamType");
            modelBuilder.Entity<MarketingTVPackage>()
                .HasOptional<TVPackageCategory>(mp => mp.Category)
                .WithMany(mp => mp.MarketingTVPackage)
                .HasForeignKey(fp => fp.Category_ID);

            modelBuilder.Entity<MarketingTVPackage>()
                .Property(x => x.Category_ID)
                .HasColumnName("Category");

            modelBuilder.Entity<MarketingTVPackage>().HasMany<TVChannelTheme>(p => p.Themes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("57"));
                });

            modelBuilder.Entity<TVChannelTheme>().HasMany<MarketingTVPackage>(p => p.MarketingTVPackages).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("57"));
                });


            modelBuilder.Entity<MarketingTVPackage>().HasMany<TVChannel>(p => p.Channels).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("58"));
                });

            modelBuilder.Entity<TVChannel>().HasMany<MarketingTVPackage>(p => p.MarketingTVPackages).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("58"));
                });


            modelBuilder.Entity<MarketingTVPackage>().HasMany<MutualTVPackageGroup>(p => p.MutualGroups).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("117"));
                });

            modelBuilder.Entity<MutualTVPackageGroup>().HasMany<MarketingTVPackage>(p => p.BackwardForMutualGroups).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("117"));
                });


            modelBuilder.Entity<MarketingTVPackage>().HasMany<MarketingProvodService>(p => p.UnavailableServices).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("124"));
                });

            modelBuilder.Entity<MarketingProvodService>().HasMany<MarketingTVPackage>(p => p.BackwardForUnavailableServices).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("124"));
                });


            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.TileIconUrl);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.TileIconUploadPath);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<MarketingTVPackage>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region QuickLinksTitle mappings
            modelBuilder.Entity<QuickLinksTitle >()
                .ToTable(GetTableName("401"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QuickLinksTitle>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QuickLinksTitle>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QuickLinksTitle>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region QuickLinksGroup mappings
            modelBuilder.Entity<QuickLinksGroup >()
                .ToTable(GetTableName("402"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QuickLinksGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QuickLinksGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QuickLinksGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<QuickLinksGroup>()
                .HasOptional<QuickLinksTitle>(mp => mp.KeyPhrase)
                .WithMany(mp => mp.Groups)
                .HasForeignKey(fp => fp.KeyPhrase_ID);

            modelBuilder.Entity<QuickLinksGroup>()
                .Property(x => x.KeyPhrase_ID)
                .HasColumnName("KeyPhrase");

            modelBuilder.Entity<QuickLinksGroup>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("173"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<QuickLinksGroup>(p => p.BackwardForRegions9).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("173"));
                });


            modelBuilder.Entity<QuickLinksGroup>().Ignore(p => p.IconUrl);
            modelBuilder.Entity<QuickLinksGroup>().Ignore(p => p.IconUploadPath);
 
            #endregion

            #region ProvodTariffParameterGroup mappings
            modelBuilder.Entity<ProvodTariffParameterGroup >()
                .ToTable(GetTableName("404"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodTariffParameterGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodTariffParameterGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodTariffParameterGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<ProvodTariffParameterGroup>().HasMany<DeviceType>(p => p.Product).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("138"));
                });

            modelBuilder.Entity<DeviceType>().HasMany<ProvodTariffParameterGroup>(p => p.BackwardForProduct).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("138"));
                });


 
            #endregion

            #region InternetTariffFamily mappings
            modelBuilder.Entity<InternetTariffFamily >()
                .ToTable(GetTableName("405"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<InternetTariffFamily>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<InternetTariffFamily>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<InternetTariffFamily>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<InternetTariffFamily>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.HomeInternetTariffFamilies)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<InternetTariffFamily>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");

            modelBuilder.Entity<InternetTariffFamily>().HasMany<InternetTariffCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("63"));
                });

            modelBuilder.Entity<InternetTariffCategory>().HasMany<InternetTariffFamily>(p => p.Families).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("63"));
                });


 
            #endregion

            #region PhoneTariffFamily mappings
            modelBuilder.Entity<PhoneTariffFamily >()
                .ToTable(GetTableName("406"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PhoneTariffFamily>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PhoneTariffFamily>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PhoneTariffFamily>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<PhoneTariffFamily>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.HomePhoneFamilies)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<PhoneTariffFamily>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");

            modelBuilder.Entity<PhoneTariffFamily>().HasMany<PhoneTariffCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("70"));
                });

            modelBuilder.Entity<PhoneTariffCategory>().HasMany<PhoneTariffFamily>(p => p.Families).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("70"));
                });


 
            #endregion

            #region ProvodServiceFamily mappings
            modelBuilder.Entity<ProvodServiceFamily >()
                .ToTable(GetTableName("407"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodServiceFamily>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodServiceFamily>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodServiceFamily>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ProvodServiceFamily>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.HomeServiceFamilies)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<ProvodServiceFamily>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");

            modelBuilder.Entity<ProvodServiceFamily>().HasMany<ProvodServiceCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("74"));
                });

            modelBuilder.Entity<ProvodServiceCategory>().HasMany<ProvodServiceFamily>(p => p.ProvodServiceFamilies).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("74"));
                });


 
            #endregion

            #region ProvodServiceParameterGroup mappings
            modelBuilder.Entity<ProvodServiceParameterGroup >()
                .ToTable(GetTableName("408"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodServiceParameterGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodServiceParameterGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodServiceParameterGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region InternetTariffCategory mappings
            modelBuilder.Entity<InternetTariffCategory >()
                .ToTable(GetTableName("409"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<InternetTariffCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<InternetTariffCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<InternetTariffCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region PhoneTariffCategory mappings
            modelBuilder.Entity<PhoneTariffCategory >()
                .ToTable(GetTableName("410"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PhoneTariffCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PhoneTariffCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PhoneTariffCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ProvodServiceCategory mappings
            modelBuilder.Entity<ProvodServiceCategory >()
                .ToTable(GetTableName("411"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodServiceCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodServiceCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodServiceCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region MarketingInternetTariff mappings
            modelBuilder.Entity<MarketingInternetTariff >()
                .ToTable(GetTableName("412"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingInternetTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingInternetTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingInternetTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingInternetTariff>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.MarketingInternetTariffs)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<MarketingInternetTariff>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");
            modelBuilder.Entity<MarketingInternetTariff>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.MarketingInternetTariffs)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<MarketingInternetTariff>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");
            modelBuilder.Entity<MarketingInternetTariff>()
                .HasOptional<InternetTariffFamily>(mp => mp.Family)
                .WithMany(mp => mp.MarketingInternetTariffs)
                .HasForeignKey(fp => fp.Family_ID);

            modelBuilder.Entity<MarketingInternetTariff>()
                .Property(x => x.Family_ID)
                .HasColumnName("Family");
            modelBuilder.Entity<MarketingInternetTariff>()
                .HasOptional<INACParamType>(mp => mp.InacParamType)
                .WithMany(mp => mp.MarketingInternetTariffs)
                .HasForeignKey(fp => fp.InacParamType_ID);

            modelBuilder.Entity<MarketingInternetTariff>()
                .Property(x => x.InacParamType_ID)
                .HasColumnName("InacParamType");

            modelBuilder.Entity<MarketingInternetTariff>().HasMany<InternetTariffCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("69"));
                });

            modelBuilder.Entity<InternetTariffCategory>().HasMany<MarketingInternetTariff>(p => p.MarketingTariffs).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("69"));
                });


            modelBuilder.Entity<MarketingInternetTariff>().HasMany<MarketingProvodService>(p => p.UnavailableServices).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("123"));
                });

            modelBuilder.Entity<MarketingProvodService>().HasMany<MarketingInternetTariff>(p => p.BackwardForUnavailableServices1).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("123"));
                });


            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.TileIconUrl);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.TileIconUploadPath);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<MarketingInternetTariff>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region MarketingPhoneTariff mappings
            modelBuilder.Entity<MarketingPhoneTariff >()
                .ToTable(GetTableName("413"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingPhoneTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingPhoneTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingPhoneTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingPhoneTariff>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.MarketingPhoneTariffs)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<MarketingPhoneTariff>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");
            modelBuilder.Entity<MarketingPhoneTariff>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.MarketingPhoneTariffs)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<MarketingPhoneTariff>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");
            modelBuilder.Entity<MarketingPhoneTariff>()
                .HasOptional<PhoneTariffFamily>(mp => mp.Family)
                .WithMany(mp => mp.MarketingPhoneTariffs)
                .HasForeignKey(fp => fp.Family_ID);

            modelBuilder.Entity<MarketingPhoneTariff>()
                .Property(x => x.Family_ID)
                .HasColumnName("Family");
            modelBuilder.Entity<MarketingPhoneTariff>()
                .HasOptional<INACParamType>(mp => mp.InacParamType)
                .WithMany(mp => mp.MarketingPhoneTariffs)
                .HasForeignKey(fp => fp.InacParamType_ID);

            modelBuilder.Entity<MarketingPhoneTariff>()
                .Property(x => x.InacParamType_ID)
                .HasColumnName("InacParamType");

            modelBuilder.Entity<MarketingPhoneTariff>().HasMany<PhoneTariffCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("71"));
                });

            modelBuilder.Entity<PhoneTariffCategory>().HasMany<MarketingPhoneTariff>(p => p.MarketingPhoneTariffs).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("71"));
                });


            modelBuilder.Entity<MarketingPhoneTariff>().HasMany<MarketingProvodService>(p => p.UnavailableServices).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("122"));
                });

            modelBuilder.Entity<MarketingProvodService>().HasMany<MarketingPhoneTariff>(p => p.BackwardForUnavailableServices2).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("122"));
                });


            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.TileIconUrl);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.TileIconUploadPath);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<MarketingPhoneTariff>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region MarketingProvodService mappings
            modelBuilder.Entity<MarketingProvodService >()
                .ToTable(GetTableName("414"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingProvodService>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingProvodService>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingProvodService>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingProvodService>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.MarketingTariffs)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<MarketingProvodService>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");
            modelBuilder.Entity<MarketingProvodService>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.MarketingProvodServices)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<MarketingProvodService>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");
            modelBuilder.Entity<MarketingProvodService>()
                .HasOptional<ProvodServiceFamily>(mp => mp.Family)
                .WithMany(mp => mp.MarketingProvodServices)
                .HasForeignKey(fp => fp.Family_ID);

            modelBuilder.Entity<MarketingProvodService>()
                .Property(x => x.Family_ID)
                .HasColumnName("Family");
            modelBuilder.Entity<MarketingProvodService>()
                .HasOptional<MarketingEquipment>(mp => mp.MarketingEquipment)
                .WithMany(mp => mp.MarketingProvodServices)
                .HasForeignKey(fp => fp.MarketingEquipment_ID);

            modelBuilder.Entity<MarketingProvodService>()
                .Property(x => x.MarketingEquipment_ID)
                .HasColumnName("MarketingEquipment");
            modelBuilder.Entity<MarketingProvodService>()
                .HasOptional<INACParamType>(mp => mp.InacParamType)
                .WithMany(mp => mp.MarketingProvodServices)
                .HasForeignKey(fp => fp.InacParamType_ID);

            modelBuilder.Entity<MarketingProvodService>()
                .Property(x => x.InacParamType_ID)
                .HasColumnName("InacParamType");
            modelBuilder.Entity<MarketingProvodService>()
                .HasOptional<ServicePrefix>(mp => mp.Prefix)
                .WithMany(mp => mp.HomeMarketingServices)
                .HasForeignKey(fp => fp.Prefix_ID);

            modelBuilder.Entity<MarketingProvodService>()
                .Property(x => x.Prefix_ID)
                .HasColumnName("Prefix");

            modelBuilder.Entity<MarketingProvodService>().HasMany<DeviceType>(p => p.Products).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("72"));
                });

            modelBuilder.Entity<DeviceType>().HasMany<MarketingProvodService>(p => p.BackwardForProducts).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("72"));
                });


            modelBuilder.Entity<MarketingProvodService>().HasMany<ProvodServiceCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("73"));
                });

            modelBuilder.Entity<ProvodServiceCategory>().HasMany<MarketingProvodService>(p => p.MarketingProvodServices).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("73"));
                });


            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.TileIconUrl);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.TileIconUploadPath);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<MarketingProvodService>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region ServiceForIternetTariff mappings
            modelBuilder.Entity<ServiceForIternetTariff >()
                .ToTable(GetTableName("415"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ServiceForIternetTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ServiceForIternetTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ServiceForIternetTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ServiceForIternetTariff>()
                .HasOptional<MarketingInternetTariff>(mp => mp.Tariff)
                .WithMany(mp => mp.Services)
                .HasForeignKey(fp => fp.Tariff_ID);

            modelBuilder.Entity<ServiceForIternetTariff>()
                .Property(x => x.Tariff_ID)
                .HasColumnName("Tariff");
            modelBuilder.Entity<ServiceForIternetTariff>()
                .HasOptional<MarketingProvodService>(mp => mp.Service)
                .WithMany(mp => mp.InternetTariffs)
                .HasForeignKey(fp => fp.Service_ID);

            modelBuilder.Entity<ServiceForIternetTariff>()
                .Property(x => x.Service_ID)
                .HasColumnName("Service");
            modelBuilder.Entity<ServiceForIternetTariff>()
                .HasOptional<ProvodTariffParameterGroup>(mp => mp.ParameterGroup)
                .WithMany(mp => mp.ServicesInInternetTariffs)
                .HasForeignKey(fp => fp.ParameterGroup_ID);

            modelBuilder.Entity<ServiceForIternetTariff>()
                .Property(x => x.ParameterGroup_ID)
                .HasColumnName("ParameterGroup");

 
            #endregion

            #region ServiceForPhoneTariff mappings
            modelBuilder.Entity<ServiceForPhoneTariff >()
                .ToTable(GetTableName("416"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ServiceForPhoneTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ServiceForPhoneTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ServiceForPhoneTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ServiceForPhoneTariff>()
                .HasOptional<MarketingPhoneTariff>(mp => mp.Tariff)
                .WithMany(mp => mp.Services)
                .HasForeignKey(fp => fp.Tariff_ID);

            modelBuilder.Entity<ServiceForPhoneTariff>()
                .Property(x => x.Tariff_ID)
                .HasColumnName("Tariff");
            modelBuilder.Entity<ServiceForPhoneTariff>()
                .HasOptional<MarketingProvodService>(mp => mp.Service)
                .WithMany(mp => mp.PhoneTariffs)
                .HasForeignKey(fp => fp.Service_ID);

            modelBuilder.Entity<ServiceForPhoneTariff>()
                .Property(x => x.Service_ID)
                .HasColumnName("Service");
            modelBuilder.Entity<ServiceForPhoneTariff>()
                .HasOptional<ProvodTariffParameterGroup>(mp => mp.ParameterGroup)
                .WithMany(mp => mp.ServicesInPhoneTariffs)
                .HasForeignKey(fp => fp.ParameterGroup_ID);

            modelBuilder.Entity<ServiceForPhoneTariff>()
                .Property(x => x.ParameterGroup_ID)
                .HasColumnName("ParameterGroup");

 
            #endregion

            #region ProvodKitFamily mappings
            modelBuilder.Entity<ProvodKitFamily >()
                .ToTable(GetTableName("417"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodKitFamily>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodKitFamily>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodKitFamily>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ProvodKitFamily>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.HomeKitFamilies)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<ProvodKitFamily>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");

            modelBuilder.Entity<ProvodKitFamily>().HasMany<ProvodKitCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("77"));
                });

            modelBuilder.Entity<ProvodKitCategory>().HasMany<ProvodKitFamily>(p => p.Families).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("77"));
                });


 
            #endregion

            #region ProvodKitCategory mappings
            modelBuilder.Entity<ProvodKitCategory >()
                .ToTable(GetTableName("418"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodKitCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodKitCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodKitCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region MarketingProvodKit mappings
            modelBuilder.Entity<MarketingProvodKit >()
                .ToTable(GetTableName("419"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingProvodKit>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingProvodKit>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingProvodKit>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingProvodKit>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.MarketingProvodKits)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<MarketingProvodKit>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");
            modelBuilder.Entity<MarketingProvodKit>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.MarketingProvodKits)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<MarketingProvodKit>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");
            modelBuilder.Entity<MarketingProvodKit>()
                .HasOptional<ProvodKitFamily>(mp => mp.Family)
                .WithMany(mp => mp.MarketingProvodKits)
                .HasForeignKey(fp => fp.Family_ID);

            modelBuilder.Entity<MarketingProvodKit>()
                .Property(x => x.Family_ID)
                .HasColumnName("Family");
            modelBuilder.Entity<MarketingProvodKit>()
                .HasOptional<MarketingInternetTariff>(mp => mp.InternetTariff)
                .WithMany(mp => mp.MarketingProvodKits)
                .HasForeignKey(fp => fp.InternetTariff_ID);

            modelBuilder.Entity<MarketingProvodKit>()
                .Property(x => x.InternetTariff_ID)
                .HasColumnName("InternetTariff");
            modelBuilder.Entity<MarketingProvodKit>()
                .HasOptional<MarketingPhoneTariff>(mp => mp.PhoneTariff)
                .WithMany(mp => mp.MarketingProvodKits)
                .HasForeignKey(fp => fp.PhoneTariff_ID);

            modelBuilder.Entity<MarketingProvodKit>()
                .Property(x => x.PhoneTariff_ID)
                .HasColumnName("PhoneTariff");
            modelBuilder.Entity<MarketingProvodKit>()
                .HasOptional<INACParamType>(mp => mp.InacParamType)
                .WithMany(mp => mp.MarketingProvodKits)
                .HasForeignKey(fp => fp.InacParamType_ID);

            modelBuilder.Entity<MarketingProvodKit>()
                .Property(x => x.InacParamType_ID)
                .HasColumnName("InacParamType");

            modelBuilder.Entity<MarketingProvodKit>().HasMany<ProvodKitCategory>(p => p.Categories).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("80"));
                });

            modelBuilder.Entity<ProvodKitCategory>().HasMany<MarketingProvodKit>(p => p.MarketingProvodKits).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("80"));
                });


            modelBuilder.Entity<MarketingProvodKit>().HasMany<MarketingTVPackage>(p => p.TVPackages).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("81"));
                });

            modelBuilder.Entity<MarketingTVPackage>().HasMany<MarketingProvodKit>(p => p.BackwardForTVPackages).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("81"));
                });


            modelBuilder.Entity<MarketingProvodKit>().HasMany<MarketingProvodService>(p => p.MarketingProvodServices).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("82"));
                });

            modelBuilder.Entity<MarketingProvodService>().HasMany<MarketingProvodKit>(p => p.BackwardForMarketingProvodServices).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("82"));
                });


            modelBuilder.Entity<MarketingProvodKit>().HasMany<MarketingProvodService>(p => p.UnavailableServices).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("121"));
                });

            modelBuilder.Entity<MarketingProvodService>().HasMany<MarketingProvodKit>(p => p.BackwardForUnavailableServices3).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("121"));
                });


            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.TileIconUrl);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.TileIconUploadPath);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<MarketingProvodKit>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region ProvodKit mappings
            modelBuilder.Entity<ProvodKit >()
                .ToTable(GetTableName("420"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodKit>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodKit>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodKit>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ProvodKit>()
                .HasOptional<MarketingProvodKit>(mp => mp.MarketKit)
                .WithMany(mp => mp.Kits)
                .HasForeignKey(fp => fp.MarketKit_ID);

            modelBuilder.Entity<ProvodKit>()
                .Property(x => x.MarketKit_ID)
                .HasColumnName("MarketKit");
            modelBuilder.Entity<ProvodKit>()
                .HasOptional<SubscriptionFeeType>(mp => mp.SubscriptionFeeType)
                .WithMany(mp => mp.ProvodKits)
                .HasForeignKey(fp => fp.SubscriptionFeeType_ID);

            modelBuilder.Entity<ProvodKit>()
                .Property(x => x.SubscriptionFeeType_ID)
                .HasColumnName("SubscriptionFeeType");

            modelBuilder.Entity<ProvodKit>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("83"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<ProvodKit>(p => p.BackwardForRegions10).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("83"));
                });


            modelBuilder.Entity<ProvodKit>().Ignore(p => p.PDFUrl);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.PDFEnglUrl);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.PDFTatUrl);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.PDFUploadPath);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.PDFEnglUploadPath);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.PDFTatUploadPath);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<ProvodKit>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region LocalRoamingOperator mappings
            modelBuilder.Entity<LocalRoamingOperator >()
                .ToTable(GetTableName("421"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<LocalRoamingOperator>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<LocalRoamingOperator>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<LocalRoamingOperator>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region RoamingTariffParam mappings
            modelBuilder.Entity<RoamingTariffParam >()
                .ToTable(GetTableName("422"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<RoamingTariffParam>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<RoamingTariffParam>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<RoamingTariffParam>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<RoamingTariffParam>()
                .HasOptional<RoamingTariffZone>(mp => mp.TariffZone)
                .WithMany(mp => mp.TariffParams)
                .HasForeignKey(fp => fp.TariffZone_ID);

            modelBuilder.Entity<RoamingTariffParam>()
                .Property(x => x.TariffZone_ID)
                .HasColumnName("TariffZone");

            modelBuilder.Entity<RoamingTariffParam>().HasMany<RoamingCountryGroup>(p => p.CountryGroups).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("168"));
                });

            modelBuilder.Entity<RoamingCountryGroup>().HasMany<RoamingTariffParam>(p => p.BackwardForCountryGroups).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("168"));
                });


 
            #endregion

            #region TVPackageFamily mappings
            modelBuilder.Entity<TVPackageFamily >()
                .ToTable(GetTableName("424"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TVPackageFamily>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TVPackageFamily>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TVPackageFamily>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<TVPackageFamily>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.TVPackageFamilies)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<TVPackageFamily>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");

            modelBuilder.Entity<TVPackageFamily>().HasMany<TVPackageCategory>(p => p.BackwardForFamilies).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("116"));
                });

            modelBuilder.Entity<TVPackageCategory>().HasMany<TVPackageFamily>(p => p.Families).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("116"));
                });


 
            #endregion

            #region SocialNetwork mappings
            modelBuilder.Entity<SocialNetwork >()
                .ToTable(GetTableName("430"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SocialNetwork>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SocialNetwork>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SocialNetwork>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<SocialNetwork>().Ignore(p => p.IconUrl);
            modelBuilder.Entity<SocialNetwork>().Ignore(p => p.IconHoverUrl);
            modelBuilder.Entity<SocialNetwork>().Ignore(p => p.IconUploadPath);
            modelBuilder.Entity<SocialNetwork>().Ignore(p => p.IconHoverUploadPath);
 
            #endregion

            #region HelpDeviceType mappings
            modelBuilder.Entity<HelpDeviceType >()
                .ToTable(GetTableName("434"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<HelpDeviceType>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<HelpDeviceType>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<HelpDeviceType>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<HelpDeviceType>()
                .HasOptional<SiteProduct>(mp => mp.SiteProduct)
                .WithMany(mp => mp.HelpDeviceTypes)
                .HasForeignKey(fp => fp.SiteProduct_ID);

            modelBuilder.Entity<HelpDeviceType>()
                .Property(x => x.SiteProduct_ID)
                .HasColumnName("SiteProduct");

            modelBuilder.Entity<HelpDeviceType>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<HelpDeviceType>().Ignore(p => p.ImageUploadPath);
 
            #endregion

            #region HelpCenterParam mappings
            modelBuilder.Entity<HelpCenterParam >()
                .ToTable(GetTableName("435"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<HelpCenterParam>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<HelpCenterParam>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<HelpCenterParam>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<HelpCenterParam>().HasMany<HelpDeviceType>(p => p.DeviceTypes).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("100"));
                });

            modelBuilder.Entity<HelpDeviceType>().HasMany<HelpCenterParam>(p => p.BackwardForDeviceTypes).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("100"));
                });


 
            #endregion

            #region TVChannelRegion mappings
            modelBuilder.Entity<TVChannelRegion >()
                .ToTable(GetTableName("436"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TVChannelRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TVChannelRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TVChannelRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<TVChannelRegion>()
                .Property(x => x.TVProgramChannel_ID)
                .HasColumnName("TVProgramChannel");
            modelBuilder.Entity<TVChannelRegion>()
                .Property(x => x.TVProgramChannel_ID)
                .HasColumnName("TVProgramChannel");
            modelBuilder.Entity<TVChannelRegion>()
                .HasOptional<TVChannel>(mp => mp.TVChannel)
                .WithMany(mp => mp.TVProgramChannelsTVChannels)
                .HasForeignKey(fp => fp.TVChannel_ID);

            modelBuilder.Entity<TVChannelRegion>()
                .Property(x => x.TVChannel_ID)
                .HasColumnName("TVChannel");

            modelBuilder.Entity<TVChannelRegion>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("101"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<TVChannelRegion>(p => p.BackwardForRegions11).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("101"));
                });


 
            #endregion

            #region PhoneAsModemTab mappings
            modelBuilder.Entity<PhoneAsModemTab >()
                .ToTable(GetTableName("437"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PhoneAsModemTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PhoneAsModemTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PhoneAsModemTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region PhoneAsModemInterface mappings
            modelBuilder.Entity<PhoneAsModemInterface >()
                .ToTable(GetTableName("438"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PhoneAsModemInterface>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PhoneAsModemInterface>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PhoneAsModemInterface>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region OperatingSystem mappings
            modelBuilder.Entity<OperatingSystem >()
                .ToTable(GetTableName("439"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<OperatingSystem>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<OperatingSystem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<OperatingSystem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region CpaPartner mappings
            modelBuilder.Entity<CpaPartner >()
                .ToTable(GetTableName("440"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<CpaPartner>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<CpaPartner>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<CpaPartner>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<CpaPartner>()
                .Property(x => x.Title)
                .HasColumnName("Название");
            modelBuilder.Entity<CpaPartner>()
                .Property(x => x.TitleEng)
                .HasColumnName("Название eng");
            modelBuilder.Entity<CpaPartner>()
                .Property(x => x.TitleTat)
                .HasColumnName("Название tat");
            modelBuilder.Entity<CpaPartner>()
                .Property(x => x.Phones)
                .HasColumnName("Телефоны");
            modelBuilder.Entity<CpaPartner>()
                .Property(x => x.Email)
                .HasColumnName("E-mail");

 
            #endregion

            #region PhoneAsModemInstruction mappings
            modelBuilder.Entity<PhoneAsModemInstruction >()
                .ToTable(GetTableName("441"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PhoneAsModemInstruction>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PhoneAsModemInstruction>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PhoneAsModemInstruction>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<PhoneAsModemInstruction>()
                .Property(x => x.Text)
                .HasColumnName("Текст");
            modelBuilder.Entity<PhoneAsModemInstruction>()
                .HasOptional<PhoneAsModemTab>(mp => mp.Tab)
                .WithMany(mp => mp.Instructions)
                .HasForeignKey(fp => fp.Tab_ID);

            modelBuilder.Entity<PhoneAsModemInstruction>()
                .Property(x => x.Tab_ID)
                .HasColumnName("Tab");
            modelBuilder.Entity<PhoneAsModemInstruction>()
                .HasOptional<OperatingSystem>(mp => mp.OperatingSystem)
                .WithMany(mp => mp.Instructions)
                .HasForeignKey(fp => fp.OperatingSystem_ID);

            modelBuilder.Entity<PhoneAsModemInstruction>()
                .Property(x => x.OperatingSystem_ID)
                .HasColumnName("OperatingSystem");
            modelBuilder.Entity<PhoneAsModemInstruction>()
                .HasOptional<PhoneAsModemInterface>(mp => mp.Interface)
                .WithMany(mp => mp.Instructions)
                .HasForeignKey(fp => fp.Interface_ID);

            modelBuilder.Entity<PhoneAsModemInstruction>()
                .Property(x => x.Interface_ID)
                .HasColumnName("Interface");

 
            #endregion

            #region CpaShortNumber mappings
            modelBuilder.Entity<CpaShortNumber >()
                .ToTable(GetTableName("442"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<CpaShortNumber>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<CpaShortNumber>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.Number)
                .HasColumnName("Номер");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.Type)
                .HasColumnName("Тип");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.Tariff)
                .HasColumnName("Тариф");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.TariffEng)
                .HasColumnName("Тариф eng");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.TariffTat)
                .HasColumnName("Тариф tat");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.Disconnect)
                .HasColumnName("Отключение");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.DisconnectEng)
                .HasColumnName("Отключение eng");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.DisconnectTat)
                .HasColumnName("Отключение tat");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.Service)
                .HasColumnName("Сервис");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.ServiceEng)
                .HasColumnName("Сервис eng");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.ServiceTat)
                .HasColumnName("Сервис tat");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.AreaOfService)
                .HasColumnName("Территория оказания услуг");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.AreaOfServiceEng)
                .HasColumnName("Территория оказания услуг eng");
            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.AreaOfServiceTat)
                .HasColumnName("Территория оказания услуг tat");
            modelBuilder.Entity<CpaShortNumber>()
                .HasOptional<CpaPartner>(mp => mp.Partner)
                .WithMany(mp => mp.Numbers)
                .HasForeignKey(fp => fp.Partner_ID);

            modelBuilder.Entity<CpaShortNumber>()
                .Property(x => x.Partner_ID)
                .HasColumnName("Partner");

 
            #endregion

            #region ChangeNumberErrorText mappings
            modelBuilder.Entity<ChangeNumberErrorText >()
                .ToTable(GetTableName("444"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ChangeNumberErrorText>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ChangeNumberErrorText>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ChangeNumberErrorText>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region EquipmentParamsTab mappings
            modelBuilder.Entity<EquipmentParamsTab >()
                .ToTable(GetTableName("446"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<EquipmentParamsTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<EquipmentParamsTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<EquipmentParamsTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region EquipmentTab mappings
            modelBuilder.Entity<EquipmentTab >()
                .ToTable(GetTableName("447"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<EquipmentTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<EquipmentTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<EquipmentTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<EquipmentTab>()
                .HasOptional<EquipmentParamsTab>(mp => mp.ParamsTab)
                .WithMany(mp => mp.EquipmentTabs)
                .HasForeignKey(fp => fp.ParamsTab_ID);

            modelBuilder.Entity<EquipmentTab>()
                .Property(x => x.ParamsTab_ID)
                .HasColumnName("ParamsTab");
            modelBuilder.Entity<EquipmentTab>()
                .HasOptional<MarketingEquipment>(mp => mp.MarketingEquipment)
                .WithMany(mp => mp.EquipmentTabs)
                .HasForeignKey(fp => fp.MarketingEquipment_ID);

            modelBuilder.Entity<EquipmentTab>()
                .Property(x => x.MarketingEquipment_ID)
                .HasColumnName("MarketingEquipment");

            modelBuilder.Entity<EquipmentTab>().HasMany<EquipmentParamsGroup>(p => p.ParamsGroups).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("102"));
                });

            modelBuilder.Entity<EquipmentParamsGroup>().HasMany<EquipmentTab>(p => p.BackwardForParamsGroups).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("102"));
                });


 
            #endregion

            #region EquipmentType mappings
            modelBuilder.Entity<EquipmentType >()
                .ToTable(GetTableName("448"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<EquipmentType>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<EquipmentType>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<EquipmentType>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<EquipmentType>()
                .HasOptional<SiteProduct>(mp => mp.Product)
                .WithMany(mp => mp.EquipmentTypes)
                .HasForeignKey(fp => fp.Product_ID);

            modelBuilder.Entity<EquipmentType>()
                .Property(x => x.Product_ID)
                .HasColumnName("Product");

            modelBuilder.Entity<EquipmentType>().Ignore(p => p.ImageUrl);
            modelBuilder.Entity<EquipmentType>().Ignore(p => p.DefaultImageUrl);
            modelBuilder.Entity<EquipmentType>().Ignore(p => p.ImageUploadPath);
            modelBuilder.Entity<EquipmentType>().Ignore(p => p.DefaultImageUploadPath);
 
            #endregion

            #region EquipmentCategory mappings
            modelBuilder.Entity<EquipmentCategory >()
                .ToTable(GetTableName("449"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<EquipmentCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<EquipmentCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<EquipmentCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<EquipmentCategory>()
                .HasOptional<EquipmentType>(mp => mp.Type)
                .WithMany(mp => mp.Categories)
                .HasForeignKey(fp => fp.Type_ID);

            modelBuilder.Entity<EquipmentCategory>()
                .Property(x => x.Type_ID)
                .HasColumnName("Type");

 
            #endregion

            #region EquipmentParam mappings
            modelBuilder.Entity<EquipmentParam >()
                .ToTable(GetTableName("450"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<EquipmentParam>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<EquipmentParam>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<EquipmentParam>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<EquipmentParam>()
                .Property(x => x.ShowInTiles)
                .HasColumnName("Показывать в плитке");
            modelBuilder.Entity<EquipmentParam>()
                .HasOptional<MarketingEquipment>(mp => mp.MarketingEquipment)
                .WithMany(mp => mp.Params)
                .HasForeignKey(fp => fp.MarketingEquipment_ID);

            modelBuilder.Entity<EquipmentParam>()
                .Property(x => x.MarketingEquipment_ID)
                .HasColumnName("MarketingEquipment");
            modelBuilder.Entity<EquipmentParam>()
                .HasOptional<EquipmentParamsGroup>(mp => mp.Group)
                .WithMany(mp => mp.Params)
                .HasForeignKey(fp => fp.Group_ID);

            modelBuilder.Entity<EquipmentParam>()
                .Property(x => x.Group_ID)
                .HasColumnName("Group");

 
            #endregion

            #region EquipmentParamsGroup mappings
            modelBuilder.Entity<EquipmentParamsGroup >()
                .ToTable(GetTableName("451"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<EquipmentParamsGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<EquipmentParamsGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<EquipmentParamsGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<EquipmentParamsGroup>()
                .HasOptional<EquipmentType>(mp => mp.Type)
                .WithMany(mp => mp.Groups)
                .HasForeignKey(fp => fp.Type_ID);

            modelBuilder.Entity<EquipmentParamsGroup>()
                .Property(x => x.Type_ID)
                .HasColumnName("Type");

 
            #endregion

            #region Equipment mappings
            modelBuilder.Entity<Equipment >()
                .ToTable(GetTableName("452"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Equipment>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Equipment>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Equipment>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<Equipment>()
                .HasOptional<MarketingEquipment>(mp => mp.MarketingEquipment)
                .WithMany(mp => mp.Equipments)
                .HasForeignKey(fp => fp.MarketingEquipment_ID);

            modelBuilder.Entity<Equipment>()
                .Property(x => x.MarketingEquipment_ID)
                .HasColumnName("MarketingEquipment");
            modelBuilder.Entity<Equipment>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.Equipments)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<Equipment>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");

            modelBuilder.Entity<Equipment>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("103"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<Equipment>(p => p.BackwardForRegions12).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("103"));
                });


            modelBuilder.Entity<Equipment>().Ignore(p => p.PdfFileUrl);
            modelBuilder.Entity<Equipment>().Ignore(p => p.PdfFileEnglUrl);
            modelBuilder.Entity<Equipment>().Ignore(p => p.PdfFileTatUrl);
            modelBuilder.Entity<Equipment>().Ignore(p => p.PdfFileUploadPath);
            modelBuilder.Entity<Equipment>().Ignore(p => p.PdfFileEnglUploadPath);
            modelBuilder.Entity<Equipment>().Ignore(p => p.PdfFileTatUploadPath);
 
            #endregion

            #region MarketingEquipment mappings
            modelBuilder.Entity<MarketingEquipment >()
                .ToTable(GetTableName("453"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingEquipment>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingEquipment>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingEquipment>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingEquipment>()
                .HasOptional<EquipmentCategory>(mp => mp.Category)
                .WithMany(mp => mp.MarketingEquipments)
                .HasForeignKey(fp => fp.Category_ID);

            modelBuilder.Entity<MarketingEquipment>()
                .Property(x => x.Category_ID)
                .HasColumnName("Category");
            modelBuilder.Entity<MarketingEquipment>()
                .HasOptional<MarketingSign>(mp => mp.MarketingSign)
                .WithMany(mp => mp.MarketingEquipments)
                .HasForeignKey(fp => fp.MarketingSign_ID);

            modelBuilder.Entity<MarketingEquipment>()
                .Property(x => x.MarketingSign_ID)
                .HasColumnName("MarketingSign");
            modelBuilder.Entity<MarketingEquipment>()
                .HasOptional<ItemTitleFormat>(mp => mp.TitleFormat)
                .WithMany(mp => mp.MarketingEquipments)
                .HasForeignKey(fp => fp.TitleFormat_ID);

            modelBuilder.Entity<MarketingEquipment>()
                .Property(x => x.TitleFormat_ID)
                .HasColumnName("TitleFormat");

            modelBuilder.Entity<MarketingEquipment>().HasMany<EquipmentImage>(p => p.Images).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("105"));
                });

            modelBuilder.Entity<EquipmentImage>().HasMany<MarketingEquipment>(p => p.BackwardForImages).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("105"));
                });


            modelBuilder.Entity<MarketingEquipment>().HasMany<MarketingEquipment>(p => p.InstalledApplications).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("137"));
                });

            modelBuilder.Entity<MarketingEquipment>().HasMany<MarketingEquipment>(p => p.BackwardForInstalledApplications).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("linked_id"); // ===
                    rp.MapRightKey("id");
                    rp.ToTable(GetReversedLinkTableName("137"));
                });


            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.TileImageUrl);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.ApplicationIconUrl);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.FileToDownloadUrl);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.PdfFileUrl);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.PdfFileEnglUrl);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.PdfFileTatUrl);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.TileImageUploadPath);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.ApplicationIconUploadPath);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.FileToDownloadUploadPath);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.PdfFileUploadPath);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.PdfFileEnglUploadPath);
            modelBuilder.Entity<MarketingEquipment>().Ignore(p => p.PdfFileTatUploadPath);
 
            #endregion

            #region EquipmentImage mappings
            modelBuilder.Entity<EquipmentImage >()
                .ToTable(GetTableName("454"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<EquipmentImage>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<EquipmentImage>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<EquipmentImage>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<EquipmentImage>().Ignore(p => p.ImageFileUrl);
            modelBuilder.Entity<EquipmentImage>().Ignore(p => p.PreviewFileUrl);
            modelBuilder.Entity<EquipmentImage>().Ignore(p => p.ImageFileUploadPath);
 
            #endregion

            #region PaymentServiceFilter mappings
            modelBuilder.Entity<PaymentServiceFilter >()
                .ToTable(GetTableName("458"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PaymentServiceFilter>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PaymentServiceFilter>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PaymentServiceFilter>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region INACParamType mappings
            modelBuilder.Entity<INACParamType >()
                .ToTable(GetTableName("461"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<INACParamType>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<INACParamType>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<INACParamType>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region InternetTariff mappings
            modelBuilder.Entity<InternetTariff >()
                .ToTable(GetTableName("462"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<InternetTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<InternetTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<InternetTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<InternetTariff>()
                .HasOptional<MarketingInternetTariff>(mp => mp.MarketingTariff)
                .WithMany(mp => mp.Tariffs)
                .HasForeignKey(fp => fp.MarketingTariff_ID);

            modelBuilder.Entity<InternetTariff>()
                .Property(x => x.MarketingTariff_ID)
                .HasColumnName("MarketingTariff");
            modelBuilder.Entity<InternetTariff>()
                .HasOptional<SubscriptionFeeType>(mp => mp.SubscriptionFeeType)
                .WithMany(mp => mp.InternetTariffs)
                .HasForeignKey(fp => fp.SubscriptionFeeType_ID);

            modelBuilder.Entity<InternetTariff>()
                .Property(x => x.SubscriptionFeeType_ID)
                .HasColumnName("SubscriptionFeeType");

            modelBuilder.Entity<InternetTariff>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("110"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<InternetTariff>(p => p.BackwardForRegions13).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("110"));
                });


            modelBuilder.Entity<InternetTariff>().Ignore(p => p.PDFUrl);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.PDFEnglUrl);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.PDFTatUrl);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.PDFUploadPath);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.PDFEnglUploadPath);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.PDFTatUploadPath);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<InternetTariff>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region InternetTariffParam mappings
            modelBuilder.Entity<InternetTariffParam >()
                .ToTable(GetTableName("463"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<InternetTariffParam>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<InternetTariffParam>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<InternetTariffParam>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<InternetTariffParam>()
                .Property(x => x.TextEngl)
                .HasColumnName("TextEng");
            modelBuilder.Entity<InternetTariffParam>()
                .HasOptional<InternetTariff>(mp => mp.Tariff)
                .WithMany(mp => mp.Params)
                .HasForeignKey(fp => fp.Tariff_ID);

            modelBuilder.Entity<InternetTariffParam>()
                .Property(x => x.Tariff_ID)
                .HasColumnName("Tariff");
            modelBuilder.Entity<InternetTariffParam>()
                .HasOptional<ProvodTariffParameterGroup>(mp => mp.Group)
                .WithMany(mp => mp.InternetTariffParams)
                .HasForeignKey(fp => fp.Group_ID);

            modelBuilder.Entity<InternetTariffParam>()
                .Property(x => x.Group_ID)
                .HasColumnName("Group");

 
            #endregion

            #region PhoneTariff mappings
            modelBuilder.Entity<PhoneTariff >()
                .ToTable(GetTableName("464"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PhoneTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PhoneTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PhoneTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<PhoneTariff>()
                .HasOptional<MarketingPhoneTariff>(mp => mp.MarketingTariff)
                .WithMany(mp => mp.Tariffs)
                .HasForeignKey(fp => fp.MarketingTariff_ID);

            modelBuilder.Entity<PhoneTariff>()
                .Property(x => x.MarketingTariff_ID)
                .HasColumnName("MarketingTariff");
            modelBuilder.Entity<PhoneTariff>()
                .HasOptional<SubscriptionFeeType>(mp => mp.SubscriptionFeeType)
                .WithMany(mp => mp.PhoneTariffs)
                .HasForeignKey(fp => fp.SubscriptionFeeType_ID);

            modelBuilder.Entity<PhoneTariff>()
                .Property(x => x.SubscriptionFeeType_ID)
                .HasColumnName("SubscriptionFeeType");

            modelBuilder.Entity<PhoneTariff>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("112"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<PhoneTariff>(p => p.BackwardForRegions14).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("112"));
                });


            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.PDFUrl);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.PDFEnglUrl);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.PDFTatUrl);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.PDFUploadPath);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.PDFEnglUploadPath);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.PDFTatUploadPath);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<PhoneTariff>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region PhoneTariffParam mappings
            modelBuilder.Entity<PhoneTariffParam >()
                .ToTable(GetTableName("465"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<PhoneTariffParam>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<PhoneTariffParam>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<PhoneTariffParam>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<PhoneTariffParam>()
                .Property(x => x.TextEngl)
                .HasColumnName("TextEng");
            modelBuilder.Entity<PhoneTariffParam>()
                .HasOptional<PhoneTariff>(mp => mp.Tariff)
                .WithMany(mp => mp.Params)
                .HasForeignKey(fp => fp.Tariff_ID);

            modelBuilder.Entity<PhoneTariffParam>()
                .Property(x => x.Tariff_ID)
                .HasColumnName("Tariff");
            modelBuilder.Entity<PhoneTariffParam>()
                .HasOptional<ProvodTariffParameterGroup>(mp => mp.Group)
                .WithMany(mp => mp.PhoneTariffParams)
                .HasForeignKey(fp => fp.Group_ID);

            modelBuilder.Entity<PhoneTariffParam>()
                .Property(x => x.Group_ID)
                .HasColumnName("Group");

 
            #endregion

            #region DiagnoseItem mappings
            modelBuilder.Entity<DiagnoseItem >()
                .ToTable(GetTableName("466"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<DiagnoseItem>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<DiagnoseItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<DiagnoseItem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<DiagnoseItem>()
                .HasOptional<DiagnoseItem>(mp => mp.LinkedItems)
                .WithMany(mp => mp.ItemsByLink)
                .HasForeignKey(fp => fp.LinkedItems_ID);

            modelBuilder.Entity<DiagnoseItem>()
                .Property(x => x.LinkedItems_ID)
                .HasColumnName("LinkedItems");

            modelBuilder.Entity<DiagnoseItem>().Ignore(p => p.PdfInstructionUrl);
            modelBuilder.Entity<DiagnoseItem>().Ignore(p => p.PdfInstructionEngUrl);
            modelBuilder.Entity<DiagnoseItem>().Ignore(p => p.PdfInstructionTatUrl);
            modelBuilder.Entity<DiagnoseItem>().Ignore(p => p.PdfInstructionUploadPath);
            modelBuilder.Entity<DiagnoseItem>().Ignore(p => p.PdfInstructionEngUploadPath);
            modelBuilder.Entity<DiagnoseItem>().Ignore(p => p.PdfInstructionTatUploadPath);
 
            #endregion

            #region MutualTVPackageGroup mappings
            modelBuilder.Entity<MutualTVPackageGroup >()
                .ToTable(GetTableName("468"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MutualTVPackageGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MutualTVPackageGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MutualTVPackageGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region TVPackageCategory mappings
            modelBuilder.Entity<TVPackageCategory >()
                .ToTable(GetTableName("469"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TVPackageCategory>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TVPackageCategory>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TVPackageCategory>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ProvodServiceParamTab mappings
            modelBuilder.Entity<ProvodServiceParamTab >()
                .ToTable(GetTableName("470"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodServiceParamTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodServiceParamTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodServiceParamTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ParamTabInProvodService mappings
            modelBuilder.Entity<ParamTabInProvodService >()
                .ToTable(GetTableName("471"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ParamTabInProvodService>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ParamTabInProvodService>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ParamTabInProvodService>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ParamTabInProvodService>()
                .HasOptional<MarketingProvodService>(mp => mp.MarketingService)
                .WithMany(mp => mp.ParamTabs)
                .HasForeignKey(fp => fp.MarketingService_ID);

            modelBuilder.Entity<ParamTabInProvodService>()
                .Property(x => x.MarketingService_ID)
                .HasColumnName("MarketingService");
            modelBuilder.Entity<ParamTabInProvodService>()
                .HasOptional<ProvodServiceParameterGroup>(mp => mp.ParamGroup)
                .WithMany(mp => mp.ParamTabs)
                .HasForeignKey(fp => fp.ParamGroup_ID);

            modelBuilder.Entity<ParamTabInProvodService>()
                .Property(x => x.ParamGroup_ID)
                .HasColumnName("ParamGroup");
            modelBuilder.Entity<ParamTabInProvodService>()
                .HasOptional<ProvodServiceParamTab>(mp => mp.Tab)
                .WithMany(mp => mp.TabServices)
                .HasForeignKey(fp => fp.Tab_ID);

            modelBuilder.Entity<ParamTabInProvodService>()
                .Property(x => x.Tab_ID)
                .HasColumnName("Tab");

 
            #endregion

            #region ProvodServiceParam mappings
            modelBuilder.Entity<ProvodServiceParam >()
                .ToTable(GetTableName("472"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodServiceParam>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodServiceParam>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodServiceParam>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ProvodServiceParam>()
                .Property(x => x.TextEngl)
                .HasColumnName("TextEng");
            modelBuilder.Entity<ProvodServiceParam>()
                .HasOptional<MarketingProvodService>(mp => mp.ProvodService)
                .WithMany(mp => mp.Params)
                .HasForeignKey(fp => fp.ProvodService_ID);

            modelBuilder.Entity<ProvodServiceParam>()
                .Property(x => x.ProvodService_ID)
                .HasColumnName("ProvodService");
            modelBuilder.Entity<ProvodServiceParam>()
                .HasOptional<ProvodServiceParameterGroup>(mp => mp.ParamGroup)
                .WithMany(mp => mp.Params)
                .HasForeignKey(fp => fp.ParamGroup_ID);

            modelBuilder.Entity<ProvodServiceParam>()
                .Property(x => x.ParamGroup_ID)
                .HasColumnName("ParamGroup");
            modelBuilder.Entity<ProvodServiceParam>()
                .HasOptional<ProvodServiceParam>(mp => mp.Parent)
                .WithMany(mp => mp.RegionChildren)
                .HasForeignKey(fp => fp.Parent_ID);

            modelBuilder.Entity<ProvodServiceParam>()
                .Property(x => x.Parent_ID)
                .HasColumnName("Parent");

            modelBuilder.Entity<ProvodServiceParam>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("118"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<ProvodServiceParam>(p => p.BackwardForRegions15).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("118"));
                });


 
            #endregion

            #region DiagnoseStatisticItem mappings
            modelBuilder.Entity<DiagnoseStatisticItem >()
                .ToTable(GetTableName("474"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<DiagnoseStatisticItem>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<DiagnoseStatisticItem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<DiagnoseStatisticItem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region RoamingCountryGroup mappings
            modelBuilder.Entity<RoamingCountryGroup >()
                .ToTable(GetTableName("476"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<RoamingCountryGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<RoamingCountryGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<RoamingCountryGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region RoamingRegionFriendGroup mappings
            modelBuilder.Entity<RoamingRegionFriendGroup >()
                .ToTable(GetTableName("477"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<RoamingRegionFriendGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<RoamingRegionFriendGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<RoamingRegionFriendGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region SukkRegionFriend mappings
            modelBuilder.Entity<SukkRegionFriend >()
                .ToTable(GetTableName("478"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SukkRegionFriend>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SukkRegionFriend>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SukkRegionFriend>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SukkRegionFriend>()
                .Property(x => x.Region_ID)
                .HasColumnName("Region");
            modelBuilder.Entity<SukkRegionFriend>()
                .Property(x => x.Region_ID)
                .HasColumnName("Region");
            modelBuilder.Entity<SukkRegionFriend>()
                .HasOptional<RoamingRegionFriendGroup>(mp => mp.Group)
                .WithMany(mp => mp.Regions)
                .HasForeignKey(fp => fp.Group_ID);

            modelBuilder.Entity<SukkRegionFriend>()
                .Property(x => x.Group_ID)
                .HasColumnName("Group");

 
            #endregion

            #region SukkCountryInGroup mappings
            modelBuilder.Entity<SukkCountryInGroup >()
                .ToTable(GetTableName("479"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SukkCountryInGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SukkCountryInGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SukkCountryInGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SukkCountryInGroup>()
                .Property(x => x.Country_ID)
                .HasColumnName("Country");
            modelBuilder.Entity<SukkCountryInGroup>()
                .Property(x => x.Country_ID)
                .HasColumnName("Country");
            modelBuilder.Entity<SukkCountryInGroup>()
                .HasOptional<RoamingCountryGroup>(mp => mp.Groups)
                .WithMany(mp => mp.SukkCountries)
                .HasForeignKey(fp => fp.Groups_ID);

            modelBuilder.Entity<SukkCountryInGroup>()
                .Property(x => x.Groups_ID)
                .HasColumnName("Groups");

 
            #endregion

            #region SukkToMarketingRegion mappings
            modelBuilder.Entity<SukkToMarketingRegion >()
                .ToTable(GetTableName("480"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SukkToMarketingRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SukkToMarketingRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SukkToMarketingRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SukkToMarketingRegion>()
                .Property(x => x.SukkRegion_ID)
                .HasColumnName("SukkRegion");
            modelBuilder.Entity<SukkToMarketingRegion>()
                .Property(x => x.SukkRegion_ID)
                .HasColumnName("SukkRegion");
            modelBuilder.Entity<SukkToMarketingRegion>()
                .HasOptional<MarketingRegion>(mp => mp.MarketingRegion)
                .WithMany(mp => mp.MarketingRegionsInSukk)
                .HasForeignKey(fp => fp.MarketingRegion_ID);

            modelBuilder.Entity<SukkToMarketingRegion>()
                .Property(x => x.MarketingRegion_ID)
                .HasColumnName("MarketingRegion");

 
            #endregion

            #region SetupConnectionTab mappings
            modelBuilder.Entity<SetupConnectionTab >()
                .ToTable(GetTableName("482"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SetupConnectionTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SetupConnectionTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SetupConnectionTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SetupConnectionTab>()
                .Property(x => x.Title)
                .HasColumnName("Название");
            modelBuilder.Entity<SetupConnectionTab>()
                .Property(x => x.TitleEngl)
                .HasColumnName("Название eng");
            modelBuilder.Entity<SetupConnectionTab>()
                .Property(x => x.TitleTat)
                .HasColumnName("Название tat");
            modelBuilder.Entity<SetupConnectionTab>()
                .Property(x => x.Description)
                .HasColumnName("Текст под вкладкой");
            modelBuilder.Entity<SetupConnectionTab>()
                .Property(x => x.DescriptionEngl)
                .HasColumnName("Текст под вкладкой eng");
            modelBuilder.Entity<SetupConnectionTab>()
                .Property(x => x.DescriptionTat)
                .HasColumnName("Текст под вкладкой tat");
            modelBuilder.Entity<SetupConnectionTab>()
                .Property(x => x.Index)
                .HasColumnName("Порядок");

 
            #endregion

            #region SetupConnectionGoal mappings
            modelBuilder.Entity<SetupConnectionGoal >()
                .ToTable(GetTableName("483"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SetupConnectionGoal>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SetupConnectionGoal>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SetupConnectionGoal>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SetupConnectionGoal>()
                .Property(x => x.Title)
                .HasColumnName("Название");
            modelBuilder.Entity<SetupConnectionGoal>()
                .Property(x => x.Index)
                .HasColumnName("Порядок");
            modelBuilder.Entity<SetupConnectionGoal>()
                .Property(x => x.TitleEngl)
                .HasColumnName("Название eng");
            modelBuilder.Entity<SetupConnectionGoal>()
                .Property(x => x.TitleTat)
                .HasColumnName("Название tat");

 
            #endregion

            #region SetupConnectionInstruction mappings
            modelBuilder.Entity<SetupConnectionInstruction >()
                .ToTable(GetTableName("484"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SetupConnectionInstruction>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SetupConnectionInstruction>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.Title)
                .HasColumnName("Название");
            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.File)
                .HasColumnName("Pdf файл инструкции");
            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.Content)
                .HasColumnName("Визуальный редактор");
            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.Wizard)
                .HasColumnName("Мастер");
            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.FileEngl)
                .HasColumnName("Pdf файл инструкции eng");
            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.FileTat)
                .HasColumnName("Pdf файл инструкции tat");
            modelBuilder.Entity<SetupConnectionInstruction>()
                .HasOptional<OperatingSystem>(mp => mp.System)
                .WithMany(mp => mp.SetUpConnectionInstructions)
                .HasForeignKey(fp => fp.System_ID);

            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.System_ID)
                .HasColumnName("System");
            modelBuilder.Entity<SetupConnectionInstruction>()
                .HasOptional<SetupConnectionGoal>(mp => mp.Goal)
                .WithMany(mp => mp.Instructions)
                .HasForeignKey(fp => fp.Goal_ID);

            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.Goal_ID)
                .HasColumnName("Goal");
            modelBuilder.Entity<SetupConnectionInstruction>()
                .HasOptional<SetupConnectionTab>(mp => mp.Tab)
                .WithMany(mp => mp.Instructions)
                .HasForeignKey(fp => fp.Tab_ID);

            modelBuilder.Entity<SetupConnectionInstruction>()
                .Property(x => x.Tab_ID)
                .HasColumnName("Tab");

            modelBuilder.Entity<SetupConnectionInstruction>().Ignore(p => p.FileUrl);
            modelBuilder.Entity<SetupConnectionInstruction>().Ignore(p => p.WizardUrl);
            modelBuilder.Entity<SetupConnectionInstruction>().Ignore(p => p.FileEnglUrl);
            modelBuilder.Entity<SetupConnectionInstruction>().Ignore(p => p.FileTatUrl);
            modelBuilder.Entity<SetupConnectionInstruction>().Ignore(p => p.FileUploadPath);
            modelBuilder.Entity<SetupConnectionInstruction>().Ignore(p => p.WizardUploadPath);
            modelBuilder.Entity<SetupConnectionInstruction>().Ignore(p => p.FileEnglUploadPath);
            modelBuilder.Entity<SetupConnectionInstruction>().Ignore(p => p.FileTatUploadPath);
 
            #endregion

            #region ProvodService mappings
            modelBuilder.Entity<ProvodService >()
                .ToTable(GetTableName("488"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ProvodService>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ProvodService>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ProvodService>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ProvodService>()
                .HasOptional<MarketingProvodService>(mp => mp.MarketingService)
                .WithMany(mp => mp.ProvodServices)
                .HasForeignKey(fp => fp.MarketingService_ID);

            modelBuilder.Entity<ProvodService>()
                .Property(x => x.MarketingService_ID)
                .HasColumnName("MarketingService");
            modelBuilder.Entity<ProvodService>()
                .HasOptional<SubscriptionFeeType>(mp => mp.SubscriptionFeeType)
                .WithMany(mp => mp.ProvodServices)
                .HasForeignKey(fp => fp.SubscriptionFeeType_ID);

            modelBuilder.Entity<ProvodService>()
                .Property(x => x.SubscriptionFeeType_ID)
                .HasColumnName("SubscriptionFeeType");

            modelBuilder.Entity<ProvodService>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("126"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<ProvodService>(p => p.BackwardForRegions16).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("126"));
                });


            modelBuilder.Entity<ProvodService>().Ignore(p => p.PDFUrl);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.PDFEnglUrl);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.PDFTatUrl);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconUrl);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconEnglUrl);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconTatUrl);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconHoverUrl);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconHoverEnglUrl);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconHoverTatUrl);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.PDFUploadPath);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.PDFEnglUploadPath);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.PDFTatUploadPath);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconUploadPath);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconEnglUploadPath);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconTatUploadPath);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconHoverUploadPath);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconHoverEnglUploadPath);
            modelBuilder.Entity<ProvodService>().Ignore(p => p.FamilyIconHoverTatUploadPath);
 
            #endregion

            #region WordForm mappings
            modelBuilder.Entity<WordForm >()
                .ToTable(GetTableName("494"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<WordForm>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<WordForm>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<WordForm>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ServiceForTVPackage mappings
            modelBuilder.Entity<ServiceForTVPackage >()
                .ToTable(GetTableName("495"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ServiceForTVPackage>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ServiceForTVPackage>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ServiceForTVPackage>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ServiceForTVPackage>()
                .HasOptional<MarketingTVPackage>(mp => mp.TVPackage)
                .WithMany(mp => mp.Services)
                .HasForeignKey(fp => fp.TVPackage_ID);

            modelBuilder.Entity<ServiceForTVPackage>()
                .Property(x => x.TVPackage_ID)
                .HasColumnName("TVPackage");
            modelBuilder.Entity<ServiceForTVPackage>()
                .HasOptional<MarketingProvodService>(mp => mp.Service)
                .WithMany(mp => mp.PhoneTariffs1)
                .HasForeignKey(fp => fp.Service_ID);

            modelBuilder.Entity<ServiceForTVPackage>()
                .Property(x => x.Service_ID)
                .HasColumnName("Service");

 
            #endregion

            #region QP_TrusteePayment mappings
            modelBuilder.Entity<QP_TrusteePayment >()
                .ToTable(GetTableName("497"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QP_TrusteePayment>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QP_TrusteePayment>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QP_TrusteePayment>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<QP_TrusteePayment>()
                .HasOptional<MarketingRegion>(mp => mp.Regions)
                .WithMany(mp => mp.FromRegions)
                .HasForeignKey(fp => fp.Regions_ID);

            modelBuilder.Entity<QP_TrusteePayment>()
                .Property(x => x.Regions_ID)
                .HasColumnName("Regions");
            modelBuilder.Entity<QP_TrusteePayment>()
                .HasOptional<QP_TrusteePaymentTab>(mp => mp.InnerTab)
                .WithMany(mp => mp.LinkedTab)
                .HasForeignKey(fp => fp.InnerTab_ID);

            modelBuilder.Entity<QP_TrusteePayment>()
                .Property(x => x.InnerTab_ID)
                .HasColumnName("InnerTab");

 
            #endregion

            #region QP_TrusteePaymentTab mappings
            modelBuilder.Entity<QP_TrusteePaymentTab >()
                .ToTable(GetTableName("498"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<QP_TrusteePaymentTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<QP_TrusteePaymentTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<QP_TrusteePaymentTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region SearchAnnouncement mappings
            modelBuilder.Entity<SearchAnnouncement >()
                .ToTable(GetTableName("499"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SearchAnnouncement>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SearchAnnouncement>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SearchAnnouncement>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<SearchAnnouncement>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("140"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<SearchAnnouncement>(p => p.BackwardForRegions17).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("140"));
                });


 
            #endregion

            #region WarrantyService mappings
            modelBuilder.Entity<WarrantyService >()
                .ToTable(GetTableName("501"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<WarrantyService>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<WarrantyService>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<WarrantyService>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<WarrantyService>().HasMany<DeviceType>(p => p.Products).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("142"));
                });

            modelBuilder.Entity<DeviceType>().HasMany<WarrantyService>(p => p.BackwardForProducts1).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("142"));
                });


            modelBuilder.Entity<WarrantyService>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("149"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<WarrantyService>(p => p.BackwardForRegions21).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("149"));
                });


 
            #endregion

            #region SKADType mappings
            modelBuilder.Entity<SKADType >()
                .ToTable(GetTableName("502"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SKADType>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SKADType>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SKADType>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region SKADService mappings
            modelBuilder.Entity<SKADService >()
                .ToTable(GetTableName("503"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SKADService>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SKADService>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SKADService>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region SKADSpeciality mappings
            modelBuilder.Entity<SKADSpeciality >()
                .ToTable(GetTableName("504"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SKADSpeciality>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SKADSpeciality>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SKADSpeciality>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region SalesPointParameter mappings
            modelBuilder.Entity<SalesPointParameter >()
                .ToTable(GetTableName("505"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SalesPointParameter>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SalesPointParameter>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SalesPointParameter>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SalesPointParameter>()
                .Property(x => x.Services)
                .HasColumnName("Блок услуг");

            modelBuilder.Entity<SalesPointParameter>().HasMany<SalesPointType>(p => p.SalesPointType).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("145"));
                });

            modelBuilder.Entity<SalesPointType>().HasMany<SalesPointParameter>(p => p.BackwardForSalesPointType).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("145"));
                });


            modelBuilder.Entity<SalesPointParameter>().HasMany<SalesPointSpeciality>(p => p.SalesPointSpeciality).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("146"));
                });

            modelBuilder.Entity<SalesPointSpeciality>().HasMany<SalesPointParameter>(p => p.BackwardForSalesPointSpeciality).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("146"));
                });


            modelBuilder.Entity<SalesPointParameter>().HasMany<SalesPointService>(p => p.SalesPointServices).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("147"));
                });

            modelBuilder.Entity<SalesPointService>().HasMany<SalesPointParameter>(p => p.BackwardForSalesPointServices).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("147"));
                });


 
            #endregion

            #region SalesPointType mappings
            modelBuilder.Entity<SalesPointType >()
                .ToTable(GetTableName("506"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SalesPointType>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SalesPointType>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SalesPointType>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SalesPointType>()
                .Property(x => x.Order)
                .HasColumnName("Порядок");
            modelBuilder.Entity<SalesPointType>()
                .HasOptional<SKADType>(mp => mp.SKADType)
                .WithMany(mp => mp.SalesPointType)
                .HasForeignKey(fp => fp.SKADType_ID);

            modelBuilder.Entity<SalesPointType>()
                .Property(x => x.SKADType_ID)
                .HasColumnName("SKADType");

 
            #endregion

            #region SalesPointSpeciality mappings
            modelBuilder.Entity<SalesPointSpeciality >()
                .ToTable(GetTableName("507"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SalesPointSpeciality>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SalesPointSpeciality>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SalesPointSpeciality>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SalesPointSpeciality>()
                .Property(x => x.Order)
                .HasColumnName("Порядок");
            modelBuilder.Entity<SalesPointSpeciality>()
                .HasOptional<SKADSpeciality>(mp => mp.SKADSpeciality)
                .WithMany(mp => mp.SalesPointSpeciality)
                .HasForeignKey(fp => fp.SKADSpeciality_ID);

            modelBuilder.Entity<SalesPointSpeciality>()
                .Property(x => x.SKADSpeciality_ID)
                .HasColumnName("SKADSpeciality");

 
            #endregion

            #region SalesPointService mappings
            modelBuilder.Entity<SalesPointService >()
                .ToTable(GetTableName("508"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SalesPointService>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SalesPointService>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SalesPointService>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SalesPointService>()
                .Property(x => x.Order)
                .HasColumnName("Порядок");
            modelBuilder.Entity<SalesPointService>()
                .HasOptional<SKADService>(mp => mp.SKADService)
                .WithMany(mp => mp.ServiceType)
                .HasForeignKey(fp => fp.SKADService_ID);

            modelBuilder.Entity<SalesPointService>()
                .Property(x => x.SKADService_ID)
                .HasColumnName("SKADService");

 
            #endregion

            #region TargetUser mappings
            modelBuilder.Entity<TargetUser >()
                .ToTable(GetTableName("509"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<TargetUser>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<TargetUser>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<TargetUser>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region FeedbackSubthemeGroup mappings
            modelBuilder.Entity<FeedbackSubthemeGroup >()
                .ToTable(GetTableName("512"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FeedbackSubthemeGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FeedbackSubthemeGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FeedbackSubthemeGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ArchiveTvTariff mappings
            modelBuilder.Entity<ArchiveTvTariff >()
                .ToTable(GetTableName("513"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ArchiveTvTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ArchiveTvTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ArchiveTvTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ArchiveTvTariff>()
                .Property(x => x.IsTariff)
                .HasColumnName("Тариф или пакет");
            modelBuilder.Entity<ArchiveTvTariff>()
                .Property(x => x.MonthPrice)
                .HasColumnName("Стоимость в месяц");
            modelBuilder.Entity<ArchiveTvTariff>()
                .Property(x => x.Description)
                .HasColumnName("Описание");
            modelBuilder.Entity<ArchiveTvTariff>()
                .Property(x => x.IdINAC)
                .HasColumnName("ID INAC");

            modelBuilder.Entity<ArchiveTvTariff>().HasMany<TVChannel>(p => p.Channels).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("160"));
                });

            modelBuilder.Entity<TVChannel>().HasMany<ArchiveTvTariff>(p => p.BackwardForChannels).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("160"));
                });


            modelBuilder.Entity<ArchiveTvTariff>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("161"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<ArchiveTvTariff>(p => p.BackwardForRegions18).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("161"));
                });


 
            #endregion

            #region ObsoleteUrlRedirect mappings
            modelBuilder.Entity<ObsoleteUrlRedirect >()
                .ToTable(GetTableName("514"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ObsoleteUrlRedirect>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ObsoleteUrlRedirect>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ObsoleteUrlRedirect>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ArchiveViewTariff mappings
            modelBuilder.Entity<ArchiveViewTariff >()
                .ToTable(GetTableName("518"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ArchiveViewTariff>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ArchiveViewTariff>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ArchiveViewTariff>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ArchiveViewTariff>()
                .Property(x => x.IsTv)
                .HasColumnName("isTv");

 
            #endregion

            #region MarketingRegionNoCallback mappings
            modelBuilder.Entity<MarketingRegionNoCallback >()
                .ToTable(GetTableName("519"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MarketingRegionNoCallback>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MarketingRegionNoCallback>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MarketingRegionNoCallback>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MarketingRegionNoCallback>()
                .HasOptional<MarketingRegion>(mp => mp.MarketingRegion)
                .WithMany(mp => mp.MarketingRegionNoCallback)
                .HasForeignKey(fp => fp.MarketingRegion_ID);

            modelBuilder.Entity<MarketingRegionNoCallback>()
                .Property(x => x.MarketingRegion_ID)
                .HasColumnName("MarketingRegion");

 
            #endregion

            #region FeedbackCallbackTime mappings
            modelBuilder.Entity<FeedbackCallbackTime >()
                .ToTable(GetTableName("520"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FeedbackCallbackTime>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FeedbackCallbackTime>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FeedbackCallbackTime>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region FeedbackSwindleProblem mappings
            modelBuilder.Entity<FeedbackSwindleProblem >()
                .ToTable(GetTableName("528"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<FeedbackSwindleProblem>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<FeedbackSwindleProblem>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<FeedbackSwindleProblem>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region Contact mappings
            modelBuilder.Entity<Contact >()
                .ToTable(GetTableName("531"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Contact>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Contact>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Contact>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<Contact>()
                .HasOptional<ContactsTab>(mp => mp.ContactsTab)
                .WithMany(mp => mp.ContactsTabs)
                .HasForeignKey(fp => fp.ContactsTab_ID);

            modelBuilder.Entity<Contact>()
                .Property(x => x.ContactsTab_ID)
                .HasColumnName("ContactsTab");

            modelBuilder.Entity<Contact>().HasMany<MarketingRegion>(p => p.MarketingRegions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("169"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<Contact>(p => p.BackwardForMarketingRegions).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("169"));
                });


            modelBuilder.Entity<Contact>().HasMany<ContactsGroup>(p => p.ContactsGroups).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("170"));
                });

            modelBuilder.Entity<ContactsGroup>().HasMany<Contact>(p => p.BackwardForContactsGroups).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("170"));
                });


 
            #endregion

            #region ContactsTab mappings
            modelBuilder.Entity<ContactsTab >()
                .ToTable(GetTableName("532"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ContactsTab>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ContactsTab>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ContactsTab>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ContactsGroup mappings
            modelBuilder.Entity<ContactsGroup >()
                .ToTable(GetTableName("533"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ContactsGroup>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ContactsGroup>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ContactsGroup>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region CampaignIdToRegion mappings
            modelBuilder.Entity<CampaignIdToRegion >()
                .ToTable(GetTableName("535"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<CampaignIdToRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<CampaignIdToRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<CampaignIdToRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<CampaignIdToRegion>()
                .HasOptional<FeedbackCallbackTime>(mp => mp.FeedbackCallbackTime)
                .WithMany(mp => mp.CampaignIdToRegion)
                .HasForeignKey(fp => fp.FeedbackCallbackTime_ID);

            modelBuilder.Entity<CampaignIdToRegion>()
                .Property(x => x.FeedbackCallbackTime_ID)
                .HasColumnName("FeedbackCallbackTime");
            modelBuilder.Entity<CampaignIdToRegion>()
                .HasOptional<MarketingRegion>(mp => mp.Region)
                .WithMany(mp => mp.CampaignIds)
                .HasForeignKey(fp => fp.Region_ID);

            modelBuilder.Entity<CampaignIdToRegion>()
                .Property(x => x.Region_ID)
                .HasColumnName("Region");

 
            #endregion

            #region SitemapXml mappings
            modelBuilder.Entity<SitemapXml >()
                .ToTable(GetTableName("536"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SitemapXml>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SitemapXml>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SitemapXml>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SitemapXml>()
                .HasOptional<MarketingRegion>(mp => mp.Region)
                .WithMany(mp => mp.SitemapXmls)
                .HasForeignKey(fp => fp.Region_ID);

            modelBuilder.Entity<SitemapXml>()
                .Property(x => x.Region_ID)
                .HasColumnName("Region");

 
            #endregion

            #region SiteConfig mappings
            modelBuilder.Entity<SiteConfig >()
                .ToTable(GetTableName("537"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SiteConfig>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SiteConfig>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SiteConfig>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<SiteConfig>()
                .HasOptional<SiteConfig>(mp => mp.Parent)
                .WithMany(mp => mp.Children)
                .HasForeignKey(fp => fp.Parent_ID);

            modelBuilder.Entity<SiteConfig>()
                .Property(x => x.Parent_ID)
                .HasColumnName("Parent");

 
            #endregion

            #region RobotsTxt mappings
            modelBuilder.Entity<RobotsTxt >()
                .ToTable(GetTableName("540"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<RobotsTxt>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<RobotsTxt>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<RobotsTxt>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<RobotsTxt>()
                .HasOptional<MarketingRegion>(mp => mp.MarketingRegion)
                .WithMany(mp => mp.RobotsTxt)
                .HasForeignKey(fp => fp.MarketingRegion_ID);

            modelBuilder.Entity<RobotsTxt>()
                .Property(x => x.MarketingRegion_ID)
                .HasColumnName("MarketingRegion");

 
            #endregion

            #region MetroLine mappings
            modelBuilder.Entity<MetroLine >()
                .ToTable(GetTableName("542"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MetroLine>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MetroLine>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MetroLine>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MetroLine>()
                .Property(x => x.Line_ID)
                .HasColumnName("Line");
            modelBuilder.Entity<MetroLine>()
                .Property(x => x.ColorName)
                .HasColumnName("Цвет линии");
            modelBuilder.Entity<MetroLine>()
                .Property(x => x.Line_ID)
                .HasColumnName("Line");

            modelBuilder.Entity<MetroLine>().Ignore(p => p.IconUrl);
            modelBuilder.Entity<MetroLine>().Ignore(p => p.IconUploadPath);
 
            #endregion

            #region AnnualContractSetting mappings
            modelBuilder.Entity<AnnualContractSetting >()
                .ToTable(GetTableName("544"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<AnnualContractSetting>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<AnnualContractSetting>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<AnnualContractSetting>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<AnnualContractSetting>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("177"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<AnnualContractSetting>(p => p.BackwardForRegions19).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("177"));
                });


 
            #endregion

            #region MnpCrmSetting mappings
            modelBuilder.Entity<MnpCrmSetting >()
                .ToTable(GetTableName("545"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MnpCrmSetting>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MnpCrmSetting>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MnpCrmSetting>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<MnpCrmSetting>().HasMany<MarketingRegion>(p => p.Regions).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("176"));
                });

            modelBuilder.Entity<MarketingRegion>().HasMany<MnpCrmSetting>(p => p.BackwardForRegions20).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("176"));
                });


 
            #endregion

            #region MNPRequestOfficeInRegion mappings
            modelBuilder.Entity<MNPRequestOfficeInRegion >()
                .ToTable(GetTableName("547"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MNPRequestOfficeInRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MNPRequestOfficeInRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MNPRequestOfficeInRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MNPRequestOfficeInRegion>()
                .HasOptional<MarketingRegion>(mp => mp.Region)
                .WithMany(mp => mp.MNPRequestInOffice)
                .HasForeignKey(fp => fp.Region_ID);

            modelBuilder.Entity<MNPRequestOfficeInRegion>()
                .Property(x => x.Region_ID)
                .HasColumnName("Region");

 
            #endregion

            #region MNPRequesCourierInRegion mappings
            modelBuilder.Entity<MNPRequesCourierInRegion >()
                .ToTable(GetTableName("548"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MNPRequesCourierInRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MNPRequesCourierInRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MNPRequesCourierInRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MNPRequesCourierInRegion>()
                .HasOptional<MarketingRegion>(mp => mp.Region)
                .WithMany(mp => mp.MNPRequesCourier)
                .HasForeignKey(fp => fp.Region_ID);

            modelBuilder.Entity<MNPRequesCourierInRegion>()
                .Property(x => x.Region_ID)
                .HasColumnName("Region");

 
            #endregion

            #region MNPRequestPageInRegion mappings
            modelBuilder.Entity<MNPRequestPageInRegion >()
                .ToTable(GetTableName("549"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<MNPRequestPageInRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<MNPRequestPageInRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<MNPRequestPageInRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<MNPRequestPageInRegion>()
                .HasOptional<MarketingRegion>(mp => mp.Region)
                .WithMany(mp => mp.MNPRequestPage)
                .HasForeignKey(fp => fp.Region_ID);

            modelBuilder.Entity<MNPRequestPageInRegion>()
                .Property(x => x.Region_ID)
                .HasColumnName("Region");

 
            #endregion

            #region SkadToFederalRegion mappings
            modelBuilder.Entity<SkadToFederalRegion >()
                .ToTable(GetTableName("550"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<SkadToFederalRegion>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<SkadToFederalRegion>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<SkadToFederalRegion>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ServicePrefix mappings
            modelBuilder.Entity<ServicePrefix >()
                .ToTable(GetTableName("551"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ServicePrefix>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ServicePrefix>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ServicePrefix>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region Widget mappings
            modelBuilder.Entity<Widget >()
                .ToTable(GetTableName("553"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Widget>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Widget>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Widget>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


            modelBuilder.Entity<Widget>().HasMany<ContainerEvent>(p => p.Fires).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("181"));
                });

            modelBuilder.Entity<ContainerEvent>().HasMany<Widget>(p => p.WhoFires).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("181"));
                });


            modelBuilder.Entity<Widget>().HasMany<ContainerEvent>(p => p.AcceptEvents).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("182"));
                });

            modelBuilder.Entity<ContainerEvent>().HasMany<Widget>(p => p.WhoAccepts).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("182"));
                });


            modelBuilder.Entity<Widget>().HasMany<WidgetDataSource>(p => p.DataSources).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("183"));
                });

            modelBuilder.Entity<WidgetDataSource>().HasMany<Widget>(p => p.BackwardForDataSources).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("183"));
                });


 
            #endregion

            #region Classifier mappings
            modelBuilder.Entity<Classifier >()
                .ToTable(GetTableName("554"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Classifier>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Classifier>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Classifier>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region IframeWidget mappings
            modelBuilder.Entity<IframeWidget >()
                .ToTable(GetTableName("555"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<IframeWidget>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<IframeWidget>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<IframeWidget>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<IframeWidget>()
                .HasOptional<Widget>(mp => mp.ItemId)
                .WithMany(mp => mp.IframeExtension)
                .HasForeignKey(fp => fp.ItemId_ID);

            modelBuilder.Entity<IframeWidget>()
                .Property(x => x.ItemId_ID)
                .HasColumnName("ItemId");

 
            #endregion

            #region HtmlWidget mappings
            modelBuilder.Entity<HtmlWidget >()
                .ToTable(GetTableName("556"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<HtmlWidget>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<HtmlWidget>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<HtmlWidget>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<HtmlWidget>()
                .HasOptional<Widget>(mp => mp.ItemId)
                .WithMany(mp => mp.HtmlJsExtension)
                .HasForeignKey(fp => fp.ItemId_ID);

            modelBuilder.Entity<HtmlWidget>()
                .Property(x => x.ItemId_ID)
                .HasColumnName("ItemId");

            modelBuilder.Entity<HtmlWidget>().HasMany<Library>(p => p.Requirements).WithMany()
                .Map(rp =>
                {
                    rp.MapLeftKey("id");
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetLinkTableName("180"));
                });

            modelBuilder.Entity<Library>().HasMany<HtmlWidget>(p => p.BackwardForRequirements).WithMany()
                .Map(rp =>
                { 
                    rp.MapLeftKey("id"); // !+
                    rp.MapRightKey("linked_id");
                    rp.ToTable(GetReversedLinkTableName("180"));
                });


 
            #endregion

            #region Library mappings
            modelBuilder.Entity<Library >()
                .ToTable(GetTableName("558"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<Library>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<Library>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<Library>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion

            #region ContainerEvent mappings
            modelBuilder.Entity<ContainerEvent >()
                .ToTable(GetTableName("559"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<ContainerEvent>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<ContainerEvent>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<ContainerEvent>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 

            modelBuilder.Entity<ContainerEvent>()
                .Property(x => x.UniqueEventName)
                .HasColumnName("EventName");

 
            #endregion

            #region WidgetDataSource mappings
            modelBuilder.Entity<WidgetDataSource >()
                .ToTable(GetTableName("560"))
                .Property(x => x.Id)
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity)
                .HasColumnName("CONTENT_ITEM_ID");
           
		    modelBuilder.Entity<WidgetDataSource>()                
                .Property(x => x.LastModifiedBy)
                .HasColumnName("LAST_MODIFIED_BY");
            
            modelBuilder.Entity<WidgetDataSource>()
                .Property(x => x.StatusTypeId)
                .HasColumnName("STATUS_TYPE_ID");

			modelBuilder.Entity<WidgetDataSource>()
                .HasRequired<StatusType>(x => x.StatusType)
                .WithMany()
                .HasForeignKey(x => x.StatusTypeId); 


 
            #endregion
        }

        #region Private members
        private string GetTableName(string contentId)
        {
            switch (_contentAccess)
            {
                case ContentAccess.Live:
                    return string.Format("content_{0}_live_new", contentId);
                case ContentAccess.Stage:
                    return string.Format("content_{0}_stage_new", contentId);
                case ContentAccess.InvisibleOrArchived:
                    return string.Format("content_{0}_united_new", contentId);
            }

            throw new InvalidOperationException(_contentAccess + " is not supported.");
        }

        private string GetLinkTableName(string linkId)
        {
            switch (_contentAccess)
            {
                case ContentAccess.Live:
                    return string.Format("item_link_{0}", linkId);
                case ContentAccess.Stage:
                    return string.Format("item_link_{0}_united", linkId);
                case ContentAccess.InvisibleOrArchived:
                    return string.Format("item_link_{0}_united", linkId);
            }

            throw new InvalidOperationException(_contentAccess + " is not supported.");
        }

        private string GetReversedLinkTableName(string linkId)
        {
            switch (_contentAccess)
            {
                case ContentAccess.Live:
                    return string.Format("item_link_{0}_rev", linkId);
                case ContentAccess.Stage:
                    return string.Format("item_link_{0}_united_rev", linkId);
                case ContentAccess.InvisibleOrArchived:
                    return string.Format("item_link_{0}_united_rev", linkId);
            }

            throw new InvalidOperationException(_contentAccess + " is not supported.");
        }
        #endregion

        #region Nested type
        public enum ContentAccess
        {
            /// <summary>
            /// Published articles
            /// </summary>
            Live = 0,
            /// <summary>
            /// Splitted versions of articles
            /// </summary>
            Stage = 1,
            /// <summary>
            /// Splitted versions of articles including invisible and archived
            /// </summary>
            InvisibleOrArchived = 2
        } 
        #endregion
    }
}

