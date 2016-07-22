namespace QA.EF.CodeFirstV6.Codegen.Catalog
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
	False
	***||||

    public partial class QPDataContext : DbContext
    {
        partial void OnContextCreated();

        static QPDataContext()
        {
            Database.SetInitializer<QPDataContext>(new NullDatabaseInitializer<QPDataContext>());
        }

        public QPDataContext()
            : base("name=qp_database")
        {
            this.Configuration.LazyLoadingEnabled = false;

			OnContextCreated();
        }

        public virtual DbSet<StatusType> StatusTypes { get; set; }
        public virtual DbSet<User> Users { get; set; }
        public virtual DbSet<UserGroup> UserGroups { get; set; }		
        public virtual DbSet<GeoRegion> GeoRegions { get; set; }
        public virtual DbSet<MarketingRegion> MarketingRegions { get; set; }
        public virtual DbSet<GeoRegionsIPAddress> GeoRegionsIPAddresses { get; set; }
        public virtual DbSet<SiteProduct> SiteProducts { get; set; }
        public virtual DbSet<QPAbstractItem> QPAbstractItems { get; set; }
        public virtual DbSet<QPDiscriminator> QPDiscriminators { get; set; }
        public virtual DbSet<QPCulture> QPCultures { get; set; }
        public virtual DbSet<ItemTitleFormat> ItemTitleFormats { get; set; }
        public virtual DbSet<QPRegion> QPRegions { get; set; }
        public virtual DbSet<Poll> Polls { get; set; }
        public virtual DbSet<PollQuestion> PollQuestions { get; set; }
        public virtual DbSet<PollAnswer> PollAnswers { get; set; }
        public virtual DbSet<UserPollAnswer> UserPollAnswers { get; set; }
        public virtual DbSet<TrailedAbstractItem> TrailedAbstractItems { get; set; }
        public virtual DbSet<QPObsoleteUrl> QPObsoleteUrls { get; set; }
        public virtual DbSet<RegionTag> RegionTags { get; set; }
        public virtual DbSet<RegionTagValue> RegionTagValues { get; set; }
        public virtual DbSet<SearchSuggestion> SearchSuggestions { get; set; }
        public virtual DbSet<SearchResult> SearchResults { get; set; }
        public virtual DbSet<SiteSection> SiteSections { get; set; }
        public virtual DbSet<NewsCategory> NewsCategories { get; set; }
        public virtual DbSet<NewsArticle> NewsArticles { get; set; }
        public virtual DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public virtual DbSet<RoamingTariffZone> RoamingTariffZones { get; set; }
        public virtual DbSet<RoamingCountryZone> RoamingCountryZones { get; set; }
        public virtual DbSet<UserSubscription> UserSubscriptions { get; set; }
        public virtual DbSet<SubscriptionCategory> SubscriptionCategories { get; set; }
        public virtual DbSet<ConfirmationRequest> ConfirmationRequests { get; set; }
        public virtual DbSet<FeedbackTheme> FeedbackThemes { get; set; }
        public virtual DbSet<FeedbackType> FeedbackTypes { get; set; }
        public virtual DbSet<FeedbackSubtheme> FeedbackSubthemes { get; set; }
        public virtual DbSet<FeedbackQueue> FeedbackQueues { get; set; }
        public virtual DbSet<TVChannel> TVChannels { get; set; }
        public virtual DbSet<RegionFeedbackGroup> RegionFeedbackGroups { get; set; }
        public virtual DbSet<DeviceType> DeviceTypes { get; set; }
        public virtual DbSet<Action> Actions { get; set; }
        public virtual DbSet<TVPackage> TVPackages { get; set; }
        public virtual DbSet<ExternalRegionMapping> ExternalRegionMappings { get; set; }
        public virtual DbSet<ExternalRegionSystem> ExternalRegionSystems { get; set; }
        public virtual DbSet<SiteSetting> SiteSettings { get; set; }
        public virtual DbSet<PhoneCode> PhoneCodes { get; set; }
        public virtual DbSet<QPItemDefinitionConstraint> QPItemDefinitionConstraints { get; set; }
        public virtual DbSet<MarketingMobileTariff> MarketingMobileTariffs { get; set; }
        public virtual DbSet<TariffGuideQuestion> TariffGuideQuestions { get; set; }
        public virtual DbSet<TariffGuideAnswer> TariffGuideAnswers { get; set; }
        public virtual DbSet<TariffGuideResult> TariffGuideResults { get; set; }
        public virtual DbSet<MobileTariffParameterGroup> MobileTariffParameterGroups { get; set; }
        public virtual DbSet<MarketingSign> MarketingSigns { get; set; }
        public virtual DbSet<MarketingMobileService> MarketingMobileServices { get; set; }
        public virtual DbSet<SubscriptionFeeType> SubscriptionFeeTypes { get; set; }
        public virtual DbSet<MobileServiceParameterGroup> MobileServiceParameterGroups { get; set; }
        public virtual DbSet<MobileTariffFamily> MobileTariffFamilies { get; set; }
        public virtual DbSet<MutualMobileServiceGroup> MutualMobileServiceGroups { get; set; }
        public virtual DbSet<MobileServiceFamily> MobileServiceFamilies { get; set; }
        public virtual DbSet<MobileTariffCategory> MobileTariffCategories { get; set; }
        public virtual DbSet<MobileServiceCategory> MobileServiceCategories { get; set; }
        public virtual DbSet<ArchiveTariff> ArchiveTariffs { get; set; }
        public virtual DbSet<ArchiveTariffTab> ArchiveTariffTabs { get; set; }
        public virtual DbSet<MobileServiceTab> MobileServiceTabs { get; set; }
        public virtual DbSet<MobileParamsGroupTab> MobileParamsGroupTabs { get; set; }
        public virtual DbSet<TariffParamGroupPriority> TariffParamGroupPriorities { get; set; }
        public virtual DbSet<ArchiveService> ArchiveServices { get; set; }
        public virtual DbSet<ArchiveMobileServiceBookmark> ArchiveMobileServiceBookmarks { get; set; }
        public virtual DbSet<PrivelegeAndBonus> PrivelegeAndBonuses { get; set; }
        public virtual DbSet<FaqContent> FaqContents { get; set; }
        public virtual DbSet<FaqGroup> FaqGroups { get; set; }
        public virtual DbSet<SiteText> SiteTexts { get; set; }
        public virtual DbSet<TVChannelTheme> TVChannelThemes { get; set; }
        public virtual DbSet<MarketingTVPackage> MarketingTVPackages { get; set; }
        public virtual DbSet<QuickLinksTitle> QuickLinksTitle { get; set; }
        public virtual DbSet<QuickLinksGroup> QuickLinksGroups { get; set; }
        public virtual DbSet<ProvodTariffParameterGroup> ProdovTariffParameterGroups { get; set; }
        public virtual DbSet<InternetTariffFamily> InternetTariffFamilies { get; set; }
        public virtual DbSet<PhoneTariffFamily> PhoneTariffFamilies { get; set; }
        public virtual DbSet<ProvodServiceFamily> ProvodServiceFamilies { get; set; }
        public virtual DbSet<ProvodServiceParameterGroup> ProdovServiceParameterGroups { get; set; }
        public virtual DbSet<InternetTariffCategory> InternetTariffCategories { get; set; }
        public virtual DbSet<PhoneTariffCategory> PhoneTariffCategories { get; set; }
        public virtual DbSet<ProvodServiceCategory> ProvodServiceCategories { get; set; }
        public virtual DbSet<MarketingInternetTariff> MarketingInternetTariffs { get; set; }
        public virtual DbSet<MarketingPhoneTariff> MarketingPhoneTariffs { get; set; }
        public virtual DbSet<MarketingProvodService> MarketingProvodServices { get; set; }
        public virtual DbSet<ServiceForIternetTariff> ServiceForIternetTariffs { get; set; }
        public virtual DbSet<ServiceForPhoneTariff> ServiceForPhoneTariffs { get; set; }
        public virtual DbSet<ProvodKitFamily> ProvodKitFamilies { get; set; }
        public virtual DbSet<ProvodKitCategory> ProvodKitCategories { get; set; }
        public virtual DbSet<MarketingProvodKit> MarketingProvodKits { get; set; }
        public virtual DbSet<ProvodKit> ProvodKits { get; set; }
        public virtual DbSet<LocalRoamingOperator> LocalRoamingOperators { get; set; }
        public virtual DbSet<RoamingTariffParam> RoamingTariffParams { get; set; }
        public virtual DbSet<TVPackageFamily> TVPackageFamilies { get; set; }
        public virtual DbSet<SocialNetwork> SocialNetworks { get; set; }
        public virtual DbSet<HelpDeviceType> HelpDeviceTypes { get; set; }
        public virtual DbSet<HelpCenterParam> HelpCenterParams { get; set; }
        public virtual DbSet<TVChannelRegion> TVChannelRegions { get; set; }
        public virtual DbSet<PhoneAsModemTab> PhoneAsModemTabs { get; set; }
        public virtual DbSet<PhoneAsModemInterface> PhoneAsModemInterfaces { get; set; }
        public virtual DbSet<OperatingSystem> OperatingSystems { get; set; }
        public virtual DbSet<CpaPartner> CpaPartners { get; set; }
        public virtual DbSet<PhoneAsModemInstruction> PhoneAsModemInstructions { get; set; }
        public virtual DbSet<CpaShortNumber> CpaShortNumbers { get; set; }
        public virtual DbSet<ChangeNumberErrorText> ChangeNumberErrorTexts { get; set; }
        public virtual DbSet<EquipmentParamsTab> EquipmentParamsTabs { get; set; }
        public virtual DbSet<EquipmentTab> EquipmentTab { get; set; }
        public virtual DbSet<EquipmentType> EquipmentTypes { get; set; }
        public virtual DbSet<EquipmentCategory> EquipmentCategories { get; set; }
        public virtual DbSet<EquipmentParam> EquipmentParam { get; set; }
        public virtual DbSet<EquipmentParamsGroup> EquipmentParamsGroups { get; set; }
        public virtual DbSet<Equipment> Equipments { get; set; }
        public virtual DbSet<MarketingEquipment> MarketingEquipments { get; set; }
        public virtual DbSet<EquipmentImage> EquipmentImages { get; set; }
        public virtual DbSet<PaymentServiceFilter> PaymentServiceFilters { get; set; }
        public virtual DbSet<INACParamType> INACParamTypes { get; set; }
        public virtual DbSet<InternetTariff> InternetTariffs { get; set; }
        public virtual DbSet<InternetTariffParam> InternetTariffParams { get; set; }
        public virtual DbSet<PhoneTariff> PhoneTariffs { get; set; }
        public virtual DbSet<PhoneTariffParam> PhoneTariffParams { get; set; }
        public virtual DbSet<DiagnoseItem> DiagnoseItems { get; set; }
        public virtual DbSet<MutualTVPackageGroup> MutualTVPackageGroups { get; set; }
        public virtual DbSet<TVPackageCategory> TVPackageCategories { get; set; }
        public virtual DbSet<ProvodServiceParamTab> ProvodServiceParamTabs { get; set; }
        public virtual DbSet<ParamTabInProvodService> ParamTabInProvodServices { get; set; }
        public virtual DbSet<ProvodServiceParam> ProvodServiceParams { get; set; }
        public virtual DbSet<DiagnoseStatisticItem> DiagnoseStatisticItems { get; set; }
        public virtual DbSet<RoamingCountryGroup> RoamingCountryGroups { get; set; }
        public virtual DbSet<RoamingRegionFriendGroup> RoamingRegionFriendGroups { get; set; }
        public virtual DbSet<SukkRegionFriend> SukkRegionFriends { get; set; }
        public virtual DbSet<SukkCountryInGroup> SukkCountryInGroups { get; set; }
        public virtual DbSet<SukkToMarketingRegion> SukkToMarketingRegions { get; set; }
        public virtual DbSet<SetupConnectionTab> SetupConnectionTab { get; set; }
        public virtual DbSet<SetupConnectionGoal> SetupConnectionGoals { get; set; }
        public virtual DbSet<SetupConnectionInstruction> SetupConnectionInstructions { get; set; }
        public virtual DbSet<ProvodService> ProvodServices { get; set; }
        public virtual DbSet<WordForm> WordForms { get; set; }
        public virtual DbSet<ServiceForTVPackage> ServiceForTVPackages { get; set; }
        public virtual DbSet<QP_TrusteePayment> QP_TrusteePayments { get; set; }
        public virtual DbSet<QP_TrusteePaymentTab> QP_TrusteePaymentTabs { get; set; }
        public virtual DbSet<SearchAnnouncement> SearchAnnouncements { get; set; }
        public virtual DbSet<WarrantyService> WarrantyServices { get; set; }
        public virtual DbSet<SKADType> SKADTypes { get; set; }
        public virtual DbSet<SKADService> SKADServices { get; set; }
        public virtual DbSet<SKADSpeciality> SKADSpecialities { get; set; }
        public virtual DbSet<SalesPointParameter> SalesPointParameters { get; set; }
        public virtual DbSet<SalesPointType> SalesPointTypes { get; set; }
        public virtual DbSet<SalesPointSpeciality> SalesPointSpecialities { get; set; }
        public virtual DbSet<SalesPointService> SalesPointServices { get; set; }
        public virtual DbSet<TargetUser> TargetUsers { get; set; }
        public virtual DbSet<FeedbackSubthemeGroup> FeedbackSubthemeGroups { get; set; }
        public virtual DbSet<ArchiveTvTariff> ArchiveTvTariffs { get; set; }
        public virtual DbSet<ObsoleteUrlRedirect> ObsoleteUrlRedirects { get; set; }
        public virtual DbSet<ArchiveViewTariff> ArchiveViewTariffs { get; set; }
        public virtual DbSet<MarketingRegionNoCallback> MarketingRegionsNoCallbacks { get; set; }
        public virtual DbSet<FeedbackCallbackTime> FeedbackCallbackTimes { get; set; }
        public virtual DbSet<FeedbackSwindleProblem> FeedbackSwindleProblems { get; set; }
        public virtual DbSet<Contact> Contacts { get; set; }
        public virtual DbSet<ContactsTab> ContactsTabs { get; set; }
        public virtual DbSet<ContactsGroup> ContactsGroups { get; set; }
        public virtual DbSet<CampaignIdToRegion> CampaignIdsToRegions { get; set; }
        public virtual DbSet<SitemapXml> SitemapXmls { get; set; }
        public virtual DbSet<SiteConfig> SiteConfigs { get; set; }
        public virtual DbSet<RobotsTxt> RobotsTxts { get; set; }
        public virtual DbSet<MetroLine> MetroLine { get; set; }
        public virtual DbSet<AnnualContractSetting> AnnualContractSettings { get; set; }
        public virtual DbSet<MnpCrmSetting> MnpCrmSettings { get; set; }
        public virtual DbSet<MNPRequestOfficeInRegion> MNPRequestOfficeInRegions { get; set; }
        public virtual DbSet<MNPRequesCourierInRegion> MNPRequesCourierInRegions { get; set; }
        public virtual DbSet<MNPRequestPageInRegion> MNPRequestPageInRegions { get; set; }
        public virtual DbSet<SkadToFederalRegion> SkadToFederalRegions { get; set; }
        public virtual DbSet<ServicePrefix> ServicePrefixes { get; set; }
        public virtual DbSet<Widget> Widgets { get; set; }
        public virtual DbSet<Classifier> Classifiers { get; set; }
        public virtual DbSet<IframeWidget> IframeWidgets { get; set; }
        public virtual DbSet<HtmlWidget> HtmlWidgets { get; set; }
        public virtual DbSet<Library> Libraries { get; set; }
        public virtual DbSet<ContainerEvent> ContainerEvents { get; set; }
        public virtual DbSet<WidgetDataSource> WidgetDataSources { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            var mapping = new QPDataContextMappingConfigurator();
            mapping.OnModelCreating(modelBuilder);
        }
    }
}
