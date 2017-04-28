using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.Validation.Validators;
using QA.Validation.Xaml;
using QP8.Infrastructure.Web.Helpers;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    [HasSelfValidation]
    public class Site : LockableEntityObject
    {
        public static readonly string DefaultContextClassName = "QPDataContext";

        public static readonly string DefaultConnectionStringName = "qp_database";

        internal Site()
        {
            AllowUserSessions = true;
            AssemblingType = Constants.AssemblingType.AspDotNet;
            IsLive = true;
            OnScreenFieldBorder = OnScreenBorderMode.OnMouseOver;
            OnScreenObjectBorder = OnScreenBorderMode.Never;
            ProceedMappingWithDb = true;
            ProceedDbIndependentGeneration = true;
            ContextClassName = DefaultContextClassName;
            ConnectionStringName = DefaultConnectionStringName;
            _externalCssItems = new InitPropertyValue<IEnumerable<ExternalCss>>(() => ExternalCssHelper.GenerateExternalCss(ExternalCss));
        }

        /// <summary>
        /// Название сущности
        /// </summary>
        [RequiredValidator(MessageTemplateResourceName = "NameNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NameMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.InvalidEntityName, Negated = true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("Name", NameResourceType = typeof(EntityObjectStrings))]
        public override string Name { get; set; }

        /// <summary>
        /// Описание сущности
        /// </summary>
        [MaxLengthValidator(512, MessageTemplateResourceName = "DescriptionMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("Description", NameResourceType = typeof(SiteStrings))]
        public override string Description { get; set; }

        /// <summary>
        /// Тип сборки
        /// </summary>
        [LocalizedDisplayName("AssemblingType", NameResourceType = typeof(SiteStrings))]
        public string AssemblingType { get; set; }

        /// <summary>
        /// Признак, разрешающий использование пользовательских сессий
        /// </summary>
        [LocalizedDisplayName("AllowUserSessions", NameResourceType = typeof(SiteStrings))]
        public bool AllowUserSessions { get; set; }

        /// <summary>
        /// Признак, разрешающий собирать страницы для предварительного просмотра и уведомлений в Основном режиме
        /// </summary>
        [LocalizedDisplayName("AssemblePreviewAndNotificationsInLiveModeOnly", NameResourceType = typeof(SiteStrings))]
        public bool AssembleFormatsInLive { get; set; }

        /// <summary>
        /// признак работы сайта в основном режиме
        /// </summary>
        [LocalizedDisplayName("SiteMode", NameResourceType = typeof(SiteStrings))]
        public bool IsLive { get; set; }

        /// <summary>
        /// DNS
        /// </summary>
        [RequiredValidator(MessageTemplateResourceName = "DnsNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "DnsMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.DomainName, MessageTemplateResourceName = "DnsInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("Dns", NameResourceType = typeof(SiteStrings))]
        [Example("localhost")]
        public string Dns { get; set; }

        /// <summary>
        /// Признак использования отдельного DNS
        /// </summary>
        [LocalizedDisplayName("UseSeparateDnsForStageMode", NameResourceType = typeof(SiteStrings))]
        public bool SeparateDns { get; set; }

        /// <summary>
        /// DNS для тестового режима
        /// </summary>
        [MaxLengthValidator(255, MessageTemplateResourceName = "StageDnsMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.DomainName, MessageTemplateResourceName = "StageDnsInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("StageDns", NameResourceType = typeof(SiteStrings))]
        public string StageDns { get; set; }

        /// <summary>
        /// URL загрузки
        /// </summary>
        [RequiredValidator(MessageTemplateResourceName = "UploadUrlNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "UploadUrlMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.RelativeWebFolderUrl, MessageTemplateResourceName = "UploadUrlInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [Example("/qp_demo_net/upload/")]
        [LocalizedDisplayName("UploadUrl", NameResourceType = typeof(SiteStrings))]
        public string UploadUrl { get; set; }

        /// <summary>
        /// Признак, разрешающий использовать абсолютный URL закачки
        /// </summary>
        [LocalizedDisplayName("UseAbsoluteUploadUrl", NameResourceType = typeof(SiteStrings))]
        public bool UseAbsoluteUploadUrl { get; set; }

        /// <summary>
        /// префикс URL загрузки
        /// </summary>
        [MaxLengthValidator(255, MessageTemplateResourceName = "UploadUrlPrefixMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.AbsoluteWebFolderUrl, MessageTemplateResourceName = "UploadUrlPrefixInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("UploadUrlPrefix", NameResourceType = typeof(SiteStrings))]
        public string UploadUrlPrefix { get; set; }

        /// <summary>
        /// Путь загрузки
        /// </summary>
        [RequiredValidator(MessageTemplateResourceName = "UploadDirNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "UploadDirMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "UploadDirInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net\upload")]
        [LocalizedDisplayName("UploadPath", NameResourceType = typeof(SiteStrings))]
        public string UploadDir { get; set; }

        /// <summary>
        /// Виртуальный путь расположения страниц в Основном режиме
        /// </summary>
        [RequiredValidator(MessageTemplateResourceName = "LiveVirtualRootNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "LiveVirtualRootMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.RelativeWebFolderUrl, MessageTemplateResourceName = "LiveVirtualRootInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [Example("/qp_demo_net/")]
        [LocalizedDisplayName("LiveVirtualRoot", NameResourceType = typeof(SiteStrings))]
        public string LiveVirtualRoot { get; set; }

        /// <summary>
        /// Путь расположения страниц в Основном режиме
        /// </summary>
        [RequiredValidator(MessageTemplateResourceName = "LiveDirectoryNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "LiveDirectoryMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "LiveDirectoryInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net")]
        [LocalizedDisplayName("LiveDirectory", NameResourceType = typeof(SiteStrings))]
        public string LiveDirectory { get; set; }

        /// <summary>
        /// Признак, разрешающий использовать тестовую папку
        /// </summary>
        [LocalizedDisplayName("ForceTestDirectory", NameResourceType = typeof(SiteStrings))]
        public bool ForceTestDirectory { get; set; }

        /// <summary>
        /// Тестовая папка
        /// </summary>
        [MaxLengthValidator(255, MessageTemplateResourceName = "TestDirectoryMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "TestDirectoryInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("TestDirectory", NameResourceType = typeof(SiteStrings))]
        public string TestDirectory { get; set; }

        /// <summary>
        /// Виртуальный путь расположения страниц в Тестовом режиме
        /// </summary>
        [RequiredValidator(MessageTemplateResourceName = "StageVirtualRootNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "StageVirtualRootMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.RelativeWebFolderUrl, MessageTemplateResourceName = "StageVirtualRootInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [Example("/qp_demo_net/stage/")]
        [LocalizedDisplayName("StageVirtualRoot", NameResourceType = typeof(SiteStrings))]
        public string StageVirtualRoot { get; set; }

        /// <summary>
        /// Путь расположения страниц в Тестовом режиме
        /// </summary>
        [RequiredValidator(MessageTemplateResourceName = "StageDirectoryNotEntered", MessageTemplateResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "StageDirectoryMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "StageDirectoryInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net\stage")]
        [LocalizedDisplayName("StageDirectory", NameResourceType = typeof(SiteStrings))]
        public string StageDirectory { get; set; }

        /// <summary>
        /// Путь расположения файлов сборок для Основного режима
        /// </summary>
        [MaxLengthValidator(255, MessageTemplateResourceName = "AssemblyPathMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "AssemblyPathInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net\bin")]
        [LocalizedDisplayName("LiveAssemblyPath", NameResourceType = typeof(SiteStrings))]
        public string AssemblyPath { get; set; }

        /// <summary>
        /// Путь расположения файлов сборок для Тестового режима
        /// </summary>
        [MaxLengthValidator(255, MessageTemplateResourceName = "StageAssemblyPathMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.AbsoluteWindowsFolderPath, MessageTemplateResourceName = "StageAssemblyPathInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net\stage\bin")]
        [LocalizedDisplayName("StageAssemblyPath", NameResourceType = typeof(SiteStrings))]
        public string StageAssemblyPath { get; set; }

        [LocalizedDisplayName("ForceAssemble", NameResourceType = typeof(SiteStrings))]
        public bool ForceAssemble { get; set; }

        /// <summary>
        /// Отображение рамки объекта в режиме OnScreen
        /// </summary>
        [LocalizedDisplayName("OnScreenObjectBorder", NameResourceType = typeof(SiteStrings))]
        public int OnScreenObjectBorder { get; set; }

        /// <summary>
        /// Отображение рамки поля в режиме OnScreen
        /// </summary>
        [LocalizedDisplayName("OnScreenFieldBorder", NameResourceType = typeof(SiteStrings))]
        public int OnScreenFieldBorder { get; set; }

        /// <summary>
        /// Маска разрешений для объектов в режиме OnScreen
        /// </summary>
        public int OnScreenObjectTypeMask { get; set; }

        /// <summary>
        /// Разрешает импортировать файл отображения в базу данных
        /// </summary>
        [LocalizedDisplayName("ImportMappingToDb", NameResourceType = typeof(SiteStrings))]
        public bool ImportMappingToDb { get; set; }

        /// <summary>
        /// Разрешает использовать прямое отображение из базы данных
        /// </summary>
        [LocalizedDisplayName("ProceedMappingWithDb", NameResourceType = typeof(SiteStrings))]
        public bool ProceedMappingWithDb { get; set; }

        [LocalizedDisplayName("ConnectionStringName", NameResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "ConnectionStringNameMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.NetName, MessageTemplateResourceName = "ConnectionStringNameInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        public string ConnectionStringName { get; set; }

        [LocalizedDisplayName("ReplaceUrls", NameResourceType = typeof(SiteStrings))]
        public bool ReplaceUrls { get; set; }

        [LocalizedDisplayName("UseLongUrls", NameResourceType = typeof(SiteStrings))]
        public bool UseLongUrls { get; set; }

        [LocalizedDisplayName("Namespace", NameResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NamespaceMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.FullQualifiedNetName, MessageTemplateResourceName = "NamespaceInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        public string Namespace { get; set; }

        [LocalizedDisplayName("ContextClassName", NameResourceType = typeof(SiteStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "ContextClassNameMaxLengthExceeded", MessageTemplateResourceType = typeof(SiteStrings))]
        [FormatValidator(RegularExpressions.NetName, MessageTemplateResourceName = "ContextClassNameInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        public string ContextClassName { get; set; }

        [LocalizedDisplayName("SendNotifications", NameResourceType = typeof(SiteStrings))]
        public bool SendNotifications { get; set; }

        [LocalizedDisplayName("PEnterMode", NameResourceType = typeof(SiteStrings))]
        public bool PEnterMode { get; set; }

        [LocalizedDisplayName("UseEnglishQuotes", NameResourceType = typeof(SiteStrings))]
        public bool UseEnglishQuotes { get; set; }

        [LocalizedDisplayName("DisableListAutoWrap", NameResourceType = typeof(SiteStrings))]
        public bool DisableListAutoWrap { get; set; }

        [LocalizedDisplayName("DbIndependent", NameResourceType = typeof(SiteStrings))]
        public bool ProceedDbIndependentGeneration { get; set; }

        [LocalizedDisplayName("MapFileOnly", NameResourceType = typeof(SiteStrings))]
        public bool GenerateMapFileOnly { get; set; }

        [LocalizedDisplayName("EnableOnScreen", NameResourceType = typeof(SiteStrings))]
        public bool EnableOnScreen { get; set; }

        [LocalizedDisplayName("ExternalUrl", NameResourceType = typeof(SiteStrings))]
        public string ExternalUrl { get; set; }

        public string ExternalCss { get; set; }

        private readonly InitPropertyValue<IEnumerable<ExternalCss>> _externalCssItems;

        [LocalizedDisplayName("ExternalCss", NameResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<ExternalCss> ExternalCssItems
        {
            get { return _externalCssItems.Value; }
            set { _externalCssItems.Value = value; }
        }

        [LocalizedDisplayName("RootElementClass", NameResourceType = typeof(VisualEditorStrings))]
        public string RootElementClass { get; set; }

        [LocalizedDisplayName("XamlDictionaries", NameResourceType = typeof(SiteStrings))]
        public string XamlDictionaries { get; set; }

        [LocalizedDisplayName("ContentFormScript", NameResourceType = typeof(SiteStrings))]
        public string ContentFormScript { get; set; }

        [LocalizedDisplayName("CreateDefaultXamlDictionary", NameResourceType = typeof(SiteStrings))]
        public bool CreateDefaultXamlDictionary { get; set; }

        [LocalizedDisplayName("DownloadEfSource", NameResourceType = typeof(SiteStrings))]
        public bool DownloadEfSource { get; set; }

        [LocalizedDisplayName("ExternalDevelopment", NameResourceType = typeof(SiteStrings))]
        public bool ExternalDevelopment { get; set; }
        public override string EntityTypeCode => Constants.EntityTypeCode.Site;

        public override string LockedByAnyoneElseMessage => SiteStrings.LockedByAnyoneElse;

        public override string CannotUpdateBecauseOfSecurityMessage => SiteStrings.CannotUpdateBecauseOfSecurity;

        public override string PropertyIsNotUniqueMessage => SiteStrings.NameNonUnique;

        public string IsLiveIcon => $"site{(IsLive ? 1 : 0)}.gif";

        public string ActualDnsForPages => !IsLive && SeparateDns ? StageDns : Dns;

        public string LongUploadUrl
        {
            get
            {
                var prefix = UseAbsoluteUploadUrl ? UploadUrlPrefix : "http://" + Dns;
                return prefix + UploadUrl;
            }
        }

        public string ImagesLongUploadUrl => LongUploadUrl + "images";

        public string LiveUrl => "http://" + Dns + LiveVirtualRoot;

        public string StageUrl => "http://" + (SeparateDns ? StageDns : Dns) + StageVirtualRoot;

        public string CurrentUrl => IsLive ? LiveUrl : StageUrl;

        public string TestBinDirectory => PathUtility.Combine(TestDirectory, SitePathRepository.RELATIVE_BIN_PATH);

        public string AppDataDirectory => AssemblyPath.Replace(SitePathRepository.RELATIVE_BIN_PATH, "") + SitePathRepository.RELATIVE_APP_DATA_PATH;

        public string AppCodeDirectory => AssemblyPath.Replace(SitePathRepository.RELATIVE_BIN_PATH, "") + SitePathRepository.RELATIVE_APP_CODE_PATH;

        public string AppDataStageDirectory => StageAssemblyPath.Replace(SitePathRepository.RELATIVE_BIN_PATH, "") + SitePathRepository.RELATIVE_APP_DATA_PATH;

        public string AppCodeStageDirectory => StageAssemblyPath.Replace(SitePathRepository.RELATIVE_BIN_PATH, "") + SitePathRepository.RELATIVE_APP_CODE_PATH;

        public bool IsDotNet => AssemblingType == Constants.AssemblingType.AspDotNet;

        public string TempDirectoryForClasses => $@"{QPConfiguration.TempDirectory}\{QPContext.CurrentCustomerCode}\{Id}";

        public string TempArchiveForClasses => TempDirectoryForClasses + ".zip";

        public PathInfo BasePathInfo => new PathInfo { Path = UploadDir, Url = LongUploadUrl };

        public override PathInfo PathInfo => BasePathInfo.GetSubPathInfo("images");

        public string FullyQualifiedContextClassName => GetFullyQualifiedName(Namespace, ContextClassName);

        public IEnumerable<ContentGroup> ContentGroups => ContentRepository.GetSiteContentGroups(Id);

        public IEnumerable<Workflow> Workflows => WorkflowRepository.GetSiteWorkflows(Id);

        public override void Validate()
        {
            var errors = new RulesException<Site>();

            base.Validate(errors);
            ValidateConditionalRequirements(errors);
            ValidateExternalCss(errors);
            ValidateXaml(errors);

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        private void ValidateExternalCss(RulesException errors)
        {
            ExternalCssHelper.ValidateExternalCss(ExternalCssItems, errors);
        }

        /// <summary>
        /// Проверка XAML
        /// </summary>
        /// <param name="errors"></param>
        private void ValidateXaml(RulesException<Site> errors)
        {
            if (!string.IsNullOrWhiteSpace(XamlDictionaries))
            {
                try
                {
                    ValidationServices.TestResourceDictionary(XamlDictionaries);
                }
                catch (Exception exp)
                {
                    errors.ErrorFor(f => f.XamlDictionaries, $"{SiteStrings.XamlDictionaries}: {exp.Message}");
                }
            }
        }

        /// <summary>
        /// Выполняет нестандартный биндинг (после стандартного)
        /// </summary>
        public void DoCustomBinding()
        {
            const CorrectSlashMode dirMode =
                CorrectSlashMode.RemoveLastSlash | CorrectSlashMode.ReplaceDoubleSlashes |
                CorrectSlashMode.ConvertSlashesToBackSlashes;
            const CorrectSlashMode urlMode =
                CorrectSlashMode.ReplaceDoubleSlashes | CorrectSlashMode.ConvertBackSlashesToSlashes |
                CorrectSlashMode.WrapToSlashes;
            const CorrectSlashMode absUrlMode =
                CorrectSlashMode.ConvertBackSlashesToSlashes | CorrectSlashMode.RemoveLastSlash;

            UploadDir = PathUtility.CorrectSlashes(UploadDir, dirMode);
            LiveDirectory = PathUtility.CorrectSlashes(LiveDirectory, dirMode);
            StageDirectory = PathUtility.CorrectSlashes(StageDirectory, dirMode);
            TestDirectory = PathUtility.CorrectSlashes(TestDirectory, dirMode);
            AssemblyPath = PathUtility.CorrectSlashes(AssemblyPath, dirMode);
            StageAssemblyPath = PathUtility.CorrectSlashes(StageAssemblyPath, dirMode);

            UploadUrl = PathUtility.CorrectSlashes(UploadUrl, urlMode);
            LiveVirtualRoot = PathUtility.CorrectSlashes(LiveVirtualRoot, urlMode);
            StageVirtualRoot = PathUtility.CorrectSlashes(StageVirtualRoot, urlMode);

            UploadUrlPrefix = PathUtility.CorrectSlashes(UploadUrlPrefix, absUrlMode);

            if (!IsDotNet)
            {
                ForceTestDirectory = false;
                AssembleFormatsInLive = false;
                ForceAssemble = false;
                AssemblyPath = null;
                StageAssemblyPath = null;
                ImportMappingToDb = false;
                ProceedMappingWithDb = false;
            }

            if (!ProceedMappingWithDb)
            {
                ConnectionStringName = null;
                ProceedDbIndependentGeneration = false;
                ReplaceUrls = false;
                UseLongUrls = false;
                Namespace = null;
                ContextClassName = null;
                SendNotifications = false;
            }

            if (!ProceedDbIndependentGeneration)
            {
                GenerateMapFileOnly = false;

                if (AssemblingType != Constants.AssemblingType.Asp)
                {
                    OnScreenObjectBorder = OnScreenBorderMode.Never;
                }

                if (!EnableOnScreen)
                {
                    OnScreenObjectBorder = OnScreenBorderMode.Never;
                    OnScreenFieldBorder = OnScreenBorderMode.Never;
                }

                if (!ForceTestDirectory)
                {
                    TestDirectory = null;
                }

                if (!SeparateDns)
                {
                    StageDns = null;
                }

                if (!UseAbsoluteUploadUrl)
                {
                    UploadUrlPrefix = null;
                }

                if (CreateDefaultXamlDictionary)
                {
                    XamlDictionaries = GenerateDefaultXamlDictionary();
                }
                else
                {
                    XamlDictionaries = string.IsNullOrWhiteSpace(XamlDictionaries) ? null : XamlDictionaries;
                }
            }
        }

        /// <summary>
        /// Генерирует XAML словарь по умолчанию
        /// </summary>
        private string GenerateDefaultXamlDictionary()
        {
            var container = new DynamicResourceDictionaryContainer();
            var dict = new DynamicResourceDictionary { Name = "Site" };
            dict.Resources.Add("Name", Name);
            container.ResourceDictionaries.Add("Site", dict);

            return ValidationServices.GenerateDynamicResourceText(container);
        }

        public static string GetFullyQualifiedName(string nameSpace, string className)
        {
            return string.IsNullOrEmpty(nameSpace) ? className : $"{nameSpace}.{className}";
        }

        public void SaveVisualEditorCommands(int[] activeVeCommands)
        {
            var defaultCommands = VisualEditorRepository.GetDefaultCommands().ToList();//все возможные команды
            var offVeCommands = VisualEditorHelpers.Subtract(defaultCommands, activeVeCommands).Select(c => c.Id).ToArray();//opposite to activeVecommands
            var oldSiteCommandIdsOn = new HashSet<int>(VisualEditorRepository.GetResultCommands(Id).Where(n => n.On).Select(n => n.Id)); // с этим нужно сравнивать на предмет измененеий

            var defaultCommandsDictionary = defaultCommands.ToDictionary(c => c.Id, c => c.On);
            var changedCommands = activeVeCommands.Where(cId => !oldSiteCommandIdsOn.Contains(cId)).ToDictionary(cId => cId, cId => true);
            foreach (var cId in offVeCommands.Where(cId => oldSiteCommandIdsOn.Contains(cId)))
            {
                changedCommands.Add(cId, false);
            }

            VisualEditorRepository.SetSiteCommands(Id, changedCommands, defaultCommandsDictionary);
        }

        /// <summary>
        /// Создает директории внутри сайта
        /// </summary>
        internal void CreateSiteFolders()
        {
            CreateSiteFolders(true, true);
        }

        /// <summary>
        /// Создает директории внутри live-сайта
        /// </summary>
        internal void CreateLiveSiteFolders()
        {
            CreateSiteFolders(true, false);
        }

        /// <summary>
        /// Создает директории внутри сайта
        /// </summary>
        internal void CreateStageSiteFolders()
        {
            CreateSiteFolders(false, true);
        }

        /// <summary>
        /// Создает директории внутри сайта
        /// </summary>
        private void CreateSiteFolders(bool isLive, bool isStage)
        {
            if (isLive)
            {
                SitePathRepository.CreateDirectories(LiveDirectory);
            }

            if (isStage)
            {
                SitePathRepository.CreateDirectories(StageDirectory);
            }

            SitePathRepository.CreateUploadDirectories(UploadDir);
            if (IsDotNet)
            {
                if (isLive)
                {
                    SitePathRepository.CreateBinDirectory(AssemblyPath);
                }

                if (isStage)
                {
                    SitePathRepository.CreateBinDirectory(StageAssemblyPath);
                }

                if (!string.IsNullOrWhiteSpace(TestDirectory))
                {
                    Directory.CreateDirectory(TestDirectory);
                    SitePathRepository.CreateBinDirectory(TestBinDirectory);
                }

            }
        }

        private void ValidateConditionalRequirements(RulesException<Site> errors)
        {
            if (SeparateDns && string.IsNullOrEmpty(StageDns))
            {
                errors.ErrorFor(s => s.StageDns, SiteStrings.StageDnsNotEntered);
            }

            if (ForceTestDirectory && string.IsNullOrEmpty(TestDirectory))
            {
                errors.ErrorFor(s => s.TestDirectory, SiteStrings.TestDirectoryNotEntered);
            }

            if (UseAbsoluteUploadUrl && string.IsNullOrEmpty(UploadUrlPrefix))
            {
                errors.ErrorFor(s => s.UploadUrlPrefix, SiteStrings.UploadUrlPrefixNotEntered);
            }

            if (IsDotNet)
            {
                if (string.IsNullOrEmpty(AssemblyPath))
                {
                    errors.ErrorFor(s => s.AssemblyPath, SiteStrings.AssemblyPathNotEntered);
                }

                if (string.IsNullOrEmpty(StageAssemblyPath))
                {
                    errors.ErrorFor(s => s.StageAssemblyPath, SiteStrings.StageAssemblyPathNotEntered);
                }

                if (!string.IsNullOrEmpty(ContextClassName))
                {
                    if (SiteRepository.ContextClassNameExists(this))
                    {
                        errors.ErrorFor(s => s.ContextClassName, SiteStrings.ContextClassNameNonUnique);
                    }
                }
            }

            if (!string.IsNullOrEmpty(ExternalUrl) && !UrlHelpers.IsAbsoluteWebFolderUrl(ExternalUrl))
            {
                errors.ErrorFor(s => s.ExternalUrl, SiteStrings.ExternalUrlNotValid);
            }
        }

        internal void SaveVisualEditorStyles(int[] activeStyleIds)
        {
            var defaultStyles = VisualEditorRepository.GetAllStyles().ToList();
            var offVeStyles = VisualEditorHelpers.Subtract(defaultStyles, activeStyleIds).Select(c => c.Id).ToArray();
            var oldSiteStylesIdsOn = new HashSet<int>(VisualEditorRepository.GetResultStyles(Id).Where(n => n.On).Select(n => n.Id));
            var defaultStyleDictionary = defaultStyles.ToDictionary(s => s.Id, s => s.On);
            var changedStyles = activeStyleIds.Where(cId => !oldSiteStylesIdsOn.Contains(cId)).ToDictionary(cId => cId, cId => true);
            foreach (var cId in offVeStyles.Where(cId => !oldSiteStylesIdsOn.Contains(cId)))
            {
                changedStyles.Add(cId, false);
            }

            VisualEditorRepository.SetSiteStyles(Id, changedStyles, defaultStyleDictionary);
        }
    }
}
