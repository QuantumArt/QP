using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using NLog.Fluent;
using QA.Validation.Xaml;
using QP8.Infrastructure.Web.Helpers;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class Site : LockableEntityObject
    {
        public static readonly string DefaultContextClassName = "QPDataContext";

        public static readonly string DefaultConnectionStringName = "qp_database";

        public const string DependPropertyName = "ExternalDevelopment";

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
            ReplaceUrlsInDB = true;
        }

        /// <summary>
        /// Название сущности
        /// </summary>
        [Required(ErrorMessageResourceName = "NameNotEntered", ErrorMessageResourceType = typeof(SiteStrings))]
        [StringLength(255, ErrorMessageResourceName = "NameMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.EntityName, ErrorMessageResourceName = "NameInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Display(Name = "Name", ResourceType = typeof(EntityObjectStrings))]
        public override string Name { get; set; }

        /// <summary>
        /// Описание сущности
        /// </summary>
        [StringLength(512, ErrorMessageResourceName = "DescriptionMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [Display(Name = "Description", ResourceType = typeof(SiteStrings))]
        public override string Description { get; set; }

        /// <summary>
        /// Тип сборки
        /// </summary>
        [Display(Name = "AssemblingType", ResourceType = typeof(SiteStrings))]
        public string AssemblingType { get; set; }

        /// <summary>
        /// Признак, разрешающий использование пользовательских сессий
        /// </summary>
        [Display(Name = "AllowUserSessions", ResourceType = typeof(SiteStrings))]
        public bool AllowUserSessions { get; set; }

        /// <summary>
        /// Признак, разрешающий собирать страницы для предварительного просмотра и уведомлений в Основном режиме
        /// </summary>
        [Display(Name = "AssemblePreviewAndNotificationsInLiveModeOnly", ResourceType = typeof(SiteStrings))]
        public bool AssembleFormatsInLive { get; set; }

        /// <summary>
        /// признак работы сайта в основном режиме
        /// </summary>
        [Display(Name = "SiteMode", ResourceType = typeof(SiteStrings))]
        public bool IsLive { get; set; }

        /// <summary>
        /// DNS
        /// </summary>
        [Required(ErrorMessageResourceName = "DnsNotEntered", ErrorMessageResourceType = typeof(SiteStrings))]
        [StringLength(255, ErrorMessageResourceName = "DnsMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.DomainName, ErrorMessageResourceName = "DnsInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Display(Name = "Dns", ResourceType = typeof(SiteStrings))]
        [Example("localhost")]
        public string Dns { get; set; }

        /// <summary>
        /// Признак, разрещающий замену Url-Placeholder
        /// </summary>
        [Display(Name = "ReplaceUrlsInDB", ResourceType = typeof(SiteStrings))]
        public bool ReplaceUrlsInDB { get; set; }


        /// <summary>
        /// Признак использования отдельного DNS
        /// </summary>
        [Display(Name = "UseSeparateDnsForStageMode", ResourceType = typeof(SiteStrings))]
        public bool SeparateDns { get; set; }

        /// <summary>
        /// DNS для тестового режима
        /// </summary>
        [StringLength(255, ErrorMessageResourceName = "StageDnsMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.DomainName, ErrorMessageResourceName = "StageDnsInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Display(Name = "StageDns", ResourceType = typeof(SiteStrings))]
        public string StageDns { get; set; }

        /// <summary>
        /// URL загрузки
        /// </summary>
        [Required(ErrorMessageResourceName = "UploadUrlNotEntered", ErrorMessageResourceType = typeof(SiteStrings))]
        [StringLength(255, ErrorMessageResourceName = "UploadUrlMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.RelativeWebFolderUrl, ErrorMessageResourceName = "UploadUrlInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Example("/qp_demo_net/upload/")]
        [Display(Name = "UploadUrl", ResourceType = typeof(SiteStrings))]
        public string UploadUrl { get; set; }

        /// <summary>
        /// Признак, разрешающий использовать абсолютный URL закачки
        /// </summary>
        [Display(Name = "UseAbsoluteUploadUrl", ResourceType = typeof(SiteStrings))]
        public bool UseAbsoluteUploadUrl { get; set; }

        /// <summary>
        /// префикс URL загрузки
        /// </summary>
        [StringLength(255, ErrorMessageResourceName = "UploadUrlPrefixMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.AbsoluteWebFolderUrl, ErrorMessageResourceName = "UploadUrlPrefixInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Display(Name = "UploadUrlPrefix", ResourceType = typeof(SiteStrings))]
        public string UploadUrlPrefix { get; set; }

        /// <summary>
        /// Путь загрузки
        /// </summary>
        [Required(ErrorMessageResourceName = "UploadDirNotEntered", ErrorMessageResourceType = typeof(SiteStrings))]
        [StringLength(255, ErrorMessageResourceName = "UploadDirMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net\upload")]
        [Display(Name = "UploadPath", ResourceType = typeof(SiteStrings))]
        public string UploadDir { get; set; }

        /// <summary>
        /// Виртуальный путь расположения страниц в Основном режиме
        /// </summary>
        [StringLength(255, ErrorMessageResourceName = "StageDirectoryMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.RelativeWebFolderUrl, ErrorMessageResourceName = "LiveVirtualRootInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Example("/qp_demo_net/")]
        [Display(Name = "LiveVirtualRoot", ResourceType = typeof(SiteStrings))]
        public string LiveVirtualRoot { get; set; }

        /// <summary>
        /// Путь расположения страниц в Основном режиме
        /// </summary>
        [StringLength(255, ErrorMessageResourceName = "LiveDirectoryMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.AbsoluteWindowsFolderPath, ErrorMessageResourceName = "LiveDirectoryInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net")]
        [Display(Name = "LiveDirectory", ResourceType = typeof(SiteStrings))]
        public string LiveDirectory { get; set; }

        /// <summary>
        /// Признак, разрешающий использовать тестовую папку
        /// </summary>
        [Display(Name = "ForceTestDirectory", ResourceType = typeof(SiteStrings))]
        public bool ForceTestDirectory { get; set; }

        /// <summary>
        /// Тестовая папка
        /// </summary>
        [StringLength(255, ErrorMessageResourceName = "TestDirectoryMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.AbsoluteWindowsFolderPath, ErrorMessageResourceName = "TestDirectoryInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Display(Name = "TestDirectory", ResourceType = typeof(SiteStrings))]
        public string TestDirectory { get; set; }

        /// <summary>
        /// Виртуальный путь расположения страниц в Тестовом режиме
        /// </summary>
        [StringLength(255, ErrorMessageResourceName = "StageVirtualRootMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.RelativeWebFolderUrl, ErrorMessageResourceName = "StageVirtualRootInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Example("/qp_demo_net/stage/")]
        [Display(Name = "StageVirtualRoot", ResourceType = typeof(SiteStrings))]
        public string StageVirtualRoot { get; set; }

        /// <summary>
        /// Путь расположения страниц в Тестовом режиме
        /// </summary>
        [StringLength(255, ErrorMessageResourceName = "StageDirectoryMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.AbsoluteWindowsFolderPath, ErrorMessageResourceName = "StageDirectoryInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net\stage")]
        [Display(Name = "StageDirectory", ResourceType = typeof(SiteStrings))]
        public string StageDirectory { get; set; }

        /// <summary>
        /// Путь расположения файлов сборок для Основного режима
        /// </summary>
        [StringLength(255, ErrorMessageResourceName = "AssemblyPathMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.AbsoluteWindowsFolderPath, ErrorMessageResourceName = "AssemblyPathInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net\bin")]
        [Display(Name = "LiveAssemblyPath", ResourceType = typeof(SiteStrings))]
        public string AssemblyPath { get; set; }

        /// <summary>
        /// Путь расположения файлов сборок для Тестового режима
        /// </summary>
        [StringLength(255, ErrorMessageResourceName = "StageAssemblyPathMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.AbsoluteWindowsFolderPath, ErrorMessageResourceName = "StageAssemblyPathInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        [Example(@"C:\Inetpub\wwwroot\qp_demo_net\stage\bin")]
        [Display(Name = "StageAssemblyPath", ResourceType = typeof(SiteStrings))]
        public string StageAssemblyPath { get; set; }

        [Display(Name = "ForceAssemble", ResourceType = typeof(SiteStrings))]
        public bool ForceAssemble { get; set; }

        /// <summary>
        /// Отображение рамки объекта в режиме OnScreen
        /// </summary>
        [Display(Name = "OnScreenObjectBorder", ResourceType = typeof(SiteStrings))]
        public int OnScreenObjectBorder { get; set; }

        /// <summary>
        /// Отображение рамки поля в режиме OnScreen
        /// </summary>
        [Display(Name = "OnScreenFieldBorder", ResourceType = typeof(SiteStrings))]
        public int OnScreenFieldBorder { get; set; }

        /// <summary>
        /// Маска разрешений для объектов в режиме OnScreen
        /// </summary>
        public int OnScreenObjectTypeMask { get; set; }

        /// <summary>
        /// Разрешает импортировать файл отображения в базу данных
        /// </summary>
        [Display(Name = "ImportMappingToDb", ResourceType = typeof(SiteStrings))]
        public bool ImportMappingToDb { get; set; }

        /// <summary>
        /// Разрешает использовать прямое отображение из базы данных
        /// </summary>
        [Display(Name = "ProceedMappingWithDb", ResourceType = typeof(SiteStrings))]
        public bool ProceedMappingWithDb { get; set; }

        [Display(Name = "ConnectionStringName", ResourceType = typeof(SiteStrings))]
        [StringLength(255, ErrorMessageResourceName = "ConnectionStringNameMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.NetName, ErrorMessageResourceName = "ConnectionStringNameInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        public string ConnectionStringName { get; set; }

        [Display(Name = "ReplaceUrls", ResourceType = typeof(SiteStrings))]
        public bool ReplaceUrls { get; set; }

        [Display(Name = "UseLongUrls", ResourceType = typeof(SiteStrings))]
        public bool UseLongUrls { get; set; }

        [Display(Name = "Namespace", ResourceType = typeof(SiteStrings))]
        [StringLength(255, ErrorMessageResourceName = "NamespaceMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.FullQualifiedNetName, ErrorMessageResourceName = "NamespaceInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        public string Namespace { get; set; }

        [Display(Name = "ContextClassName", ResourceType = typeof(SiteStrings))]
        [StringLength(255, ErrorMessageResourceName = "ContextClassNameMaxLengthExceeded", ErrorMessageResourceType = typeof(SiteStrings))]
        [RegularExpression(RegularExpressions.NetName, ErrorMessageResourceName = "ContextClassNameInvalidFormat", ErrorMessageResourceType = typeof(SiteStrings))]
        public string ContextClassName { get; set; }

        [Display(Name = "SendNotifications", ResourceType = typeof(SiteStrings))]
        public bool SendNotifications { get; set; }

        [Display(Name = "PEnterMode", ResourceType = typeof(SiteStrings))]
        public bool PEnterMode { get; set; }

        [Display(Name = "UseEnglishQuotes", ResourceType = typeof(SiteStrings))]
        public bool UseEnglishQuotes { get; set; }

        [Display(Name = "DisableListAutoWrap", ResourceType = typeof(SiteStrings))]
        public bool DisableListAutoWrap { get; set; }

        [Display(Name = "DbIndependent", ResourceType = typeof(SiteStrings))]
        public bool ProceedDbIndependentGeneration { get; set; }

        [Display(Name = "MapFileOnly", ResourceType = typeof(SiteStrings))]
        public bool GenerateMapFileOnly { get; set; }

        [Display(Name = "EnableOnScreen", ResourceType = typeof(SiteStrings))]
        public bool EnableOnScreen { get; set; }

        [Display(Name = "ExternalUrl", ResourceType = typeof(SiteStrings))]
        public string ExternalUrl { get; set; }

        public string ExternalCss { get; set; }

        private readonly InitPropertyValue<IEnumerable<ExternalCss>> _externalCssItems;

        [Display(Name = "ExternalCss", ResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<ExternalCss> ExternalCssItems
        {
            get => _externalCssItems.Value;
            set => _externalCssItems.Value = value;
        }

        [Display(Name = "RootElementClass", ResourceType = typeof(VisualEditorStrings))]
        public string RootElementClass { get; set; }

        [Display(Name = "XamlDictionaries", ResourceType = typeof(SiteStrings))]
        public string XamlDictionaries { get; set; }

        [Display(Name = "ContentFormScript", ResourceType = typeof(SiteStrings))]
        public string ContentFormScript { get; set; }

        [Display(Name = "CreateDefaultXamlDictionary", ResourceType = typeof(SiteStrings))]
        public bool CreateDefaultXamlDictionary { get; set; }

        [Display(Name = "DownloadEfSource", ResourceType = typeof(SiteStrings))]
        public bool DownloadEfSource { get; set; }

        [Display(Name = "ExternalDevelopment", ResourceType = typeof(SiteStrings))]
        public bool ExternalDevelopment { get; set; }

        private IEnumerable<QpPluginFieldValue> _qpPluginFieldValues;
        public IEnumerable<QpPluginFieldValue> QpPluginFieldValues
        {
            get => _qpPluginFieldValues = _qpPluginFieldValues ?? SiteRepository.GetPluginValues(Id);
            set => _qpPluginFieldValues = value;
        }

        public IEnumerable<QpPluginFieldValueGroup> QpPluginFieldValueGroups => QpPluginFieldValues.ToFieldGroups();

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

        [ValidateNever]
        [BindNever]
        public string TestBinDirectory => PathUtility.Combine(TestDirectory, SitePathRepository.RELATIVE_BIN_PATH);

        [ValidateNever]
        [BindNever]
        public string AppDataDirectory => AssemblyPath.Replace(SitePathRepository.RELATIVE_BIN_PATH, "") + SitePathRepository.RELATIVE_APP_DATA_PATH;

        [ValidateNever]
        [BindNever]
        public string AppCodeDirectory => AssemblyPath.Replace(SitePathRepository.RELATIVE_BIN_PATH, "") + SitePathRepository.RELATIVE_APP_CODE_PATH;

        [ValidateNever]
        [BindNever]
        public string AppDataStageDirectory => StageAssemblyPath.Replace(SitePathRepository.RELATIVE_BIN_PATH, "") + SitePathRepository.RELATIVE_APP_DATA_PATH;

        [ValidateNever]
        [BindNever]
        public string AppCodeStageDirectory => StageAssemblyPath.Replace(SitePathRepository.RELATIVE_BIN_PATH, "") + SitePathRepository.RELATIVE_APP_CODE_PATH;

        public bool IsDotNet => AssemblingType == Constants.AssemblingType.AspDotNet;

        public string TempDirectoryForClasses => $@"{QPConfiguration.TempDirectory}{Path.DirectorySeparatorChar}{QPContext.CurrentCustomerCode}{Path.DirectorySeparatorChar}{Id}";

        public string TempArchiveForClasses => TempDirectoryForClasses + ".zip";

        public PathInfo BasePathInfo => new PathInfo { Path = UploadDir, Url = LongUploadUrl };

        public override PathInfo PathInfo => BasePathInfo.GetSubPathInfo("images");

        public string FullyQualifiedContextClassName => GetFullyQualifiedName(Namespace, ContextClassName);


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
                    var str = $"{SiteStrings.XamlDictionaries}: {exp.Message}";
                    CurrentLogger.Error().Exception(exp).Message(str).Write();
                    errors.ErrorFor(f => f.XamlDictionaries, str);
                }
            }
        }

        /// <summary>
        /// Выполняет нестандартный биндинг (после стандартного)
        /// </summary>
        public override void DoCustomBinding()
        {
            const CorrectSlashMode simpleDirMode = CorrectSlashMode.RemoveLastSlash;
            const CorrectSlashMode dirMode = CorrectSlashMode.RemoveLastSlash | CorrectSlashMode.ConvertSlashesToBackSlashes;
            const CorrectSlashMode urlMode = CorrectSlashMode.ReplaceDoubleSlashes | CorrectSlashMode.ConvertBackSlashesToSlashes | CorrectSlashMode.WrapToSlashes;
            const CorrectSlashMode absUrlMode = CorrectSlashMode.ConvertBackSlashesToSlashes | CorrectSlashMode.RemoveLastSlash;

            UploadDir = PathUtility.CorrectSlashes(UploadDir, simpleDirMode);
            LiveDirectory = PathUtility.CorrectSlashes(LiveDirectory, dirMode);
            StageDirectory = PathUtility.CorrectSlashes(StageDirectory, dirMode);
            TestDirectory = PathUtility.CorrectSlashes(TestDirectory, dirMode);
            AssemblyPath = PathUtility.CorrectSlashes(AssemblyPath, dirMode);
            StageAssemblyPath = PathUtility.CorrectSlashes(StageAssemblyPath, dirMode);

            UploadUrl = PathUtility.CorrectSlashes(UploadUrl, urlMode);
            LiveVirtualRoot = PathUtility.CorrectSlashes(LiveVirtualRoot, urlMode);
            StageVirtualRoot = PathUtility.CorrectSlashes(StageVirtualRoot, urlMode);

            UploadUrlPrefix = PathUtility.CorrectSlashes(UploadUrlPrefix, absUrlMode);

            if (ExternalDevelopment)
            {
                LiveDirectory = "";
                StageDirectory = "";
                TestDirectory = "";
                AssemblyPath = "";
                StageAssemblyPath = "";
                LiveVirtualRoot = "";
                StageVirtualRoot = "";
            }

            if (CreateDefaultXamlDictionary)
            {
                XamlDictionaries = GenerateDefaultXamlDictionary();
            }
            else
            {
                XamlDictionaries = string.IsNullOrWhiteSpace(XamlDictionaries) ? null : XamlDictionaries;
            }

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

            if (!SeparateDns)
            {
                StageDns = null;
            }

            if (!UseAbsoluteUploadUrl)
            {
                UploadUrlPrefix = null;
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

        public static string GetFullyQualifiedName(string nameSpace, string className) => string.IsNullOrEmpty(nameSpace) ? className : $"{nameSpace}.{className}";

        public void SaveVisualEditorCommands(int[] activeCommandIds)
        {
            var oldCommands = VisualEditorRepository.GetResultCommands(Id)
               .ToDictionary(s => s.Id, s => s.On);

            var activeCommandIdsSet = new HashSet<int>(activeCommandIds);

            var newCommands = oldCommands.Keys
                .Union(activeCommandIds)
                .ToDictionary(id => id, id => activeCommandIdsSet.Contains(id));

            var changedCommands = newCommands.Keys
                .Where(id => !oldCommands.ContainsKey(id) || oldCommands[id] != newCommands[id])
                .ToDictionary(id => id, id => newCommands[id]);

            var defaultCommands = VisualEditorRepository.GetDefaultCommands()
                .ToDictionary(s => s.Id, s => s.On);

            VisualEditorRepository.SetSiteCommands(Id, changedCommands, defaultCommands);
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

        internal void NullifyField()
        {
            LiveVirtualRoot = null;
            LiveDirectory = null;
            StageVirtualRoot = null;
            StageDirectory = null;
            AssemblyPath = null;
            StageAssemblyPath = null;
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
            if (!ExternalDevelopment)
            {
                if (isLive)
                {
                    SitePathRepository.CreateDirectories(LiveDirectory);
                }

                if (isStage)
                {
                    SitePathRepository.CreateDirectories(StageDirectory);
                }
            }

            SitePathRepository.CreateUploadDirectories(UploadDir);

            if (!ExternalDevelopment && IsDotNet)
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
            var winRe = new Regex(RegularExpressions.AbsoluteWindowsFolderPath);
            var linuxRe = new Regex(RegularExpressions.RelativeWebFolderUrl);

            if (!winRe.IsMatch(UploadDir) && !linuxRe.IsMatch(UploadDir))
            {
                errors.ErrorFor(s => s.UploadDir, SiteStrings.UploadDirInvalidFormat);
            }

            if (!ExternalDevelopment)
            {
                if (string.IsNullOrEmpty(LiveVirtualRoot))
                {
                    errors.ErrorFor(s => s.LiveVirtualRoot, SiteStrings.LiveVirtualRootNotEntered);
                }
                if (string.IsNullOrEmpty(StageVirtualRoot))
                {
                    errors.ErrorFor(s => s.StageVirtualRoot, SiteStrings.StageVirtualRootNotEntered);
                }
                if (string.IsNullOrEmpty(LiveDirectory))
                {
                    errors.ErrorFor(s => s.LiveDirectory, SiteStrings.LiveDirectoryNotEntered);
                }
                if (string.IsNullOrEmpty(StageDirectory))
                {
                    errors.ErrorFor(s => s.StageDirectory, SiteStrings.StageDirectoryNotEntered);
                }
            }

            if (SeparateDns && string.IsNullOrEmpty(StageDns))
            {
                errors.ErrorFor(s => s.StageDns, SiteStrings.StageDnsNotEntered);
            }

            if (ForceTestDirectory && string.IsNullOrEmpty(TestDirectory) && !ExternalDevelopment)
            {
                errors.ErrorFor(s => s.TestDirectory, SiteStrings.TestDirectoryNotEntered);
            }

            if (UseAbsoluteUploadUrl && string.IsNullOrEmpty(UploadUrlPrefix))
            {
                errors.ErrorFor(s => s.UploadUrlPrefix, SiteStrings.UploadUrlPrefixNotEntered);
            }

            if (IsDotNet)
            {
                if (string.IsNullOrEmpty(AssemblyPath) && !ExternalDevelopment)
                {
                    errors.ErrorFor(s => s.AssemblyPath, SiteStrings.AssemblyPathNotEntered);
                }

                if (string.IsNullOrEmpty(StageAssemblyPath) && !ExternalDevelopment)
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
            var oldStyles = VisualEditorRepository.GetResultStyles(Id)
                .ToDictionary(s => s.Id, s => s.On);

            var activeStyleIdsSet = new HashSet<int>(activeStyleIds);

            var newStyles = oldStyles.Keys
                .Union(activeStyleIds)
                .ToDictionary(id => id, id => activeStyleIdsSet.Contains(id));

            var changedStyles = newStyles.Keys
                .Where(id => !oldStyles.ContainsKey(id) || oldStyles[id] != newStyles[id])
                .ToDictionary(id => id, id => newStyles[id]);

            var defaultStyles = VisualEditorRepository.GetAllStyles()
                .ToDictionary(s => s.Id, s => s.On);

            VisualEditorRepository.SetSiteStyles(Id, changedStyles, defaultStyles);
        }
    }
}
