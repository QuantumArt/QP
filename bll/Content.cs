using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Newtonsoft.Json;
using NLog;
using QA.Validation.Xaml;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.BLL.Validators;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Utils.Binders;

namespace Quantumart.QP8.BLL
{
    public class Content : EntityObject
    {
        public class VirtualFieldNode
        {
            public VirtualFieldNode()
            {
                Children = Enumerable.Empty<VirtualFieldNode>();
            }

            public int Id { get; set; }

            public IEnumerable<VirtualFieldNode> Children { get; set; }

            public string TreeId { get; set; }

            public string Name { get; set; }

            /// <summary>
            /// Разворачивает иерархию в плоскую структуру
            /// </summary>
            public static IEnumerable<VirtualFieldNode> Linearize(IEnumerable<VirtualFieldNode> nodes)
            {
                var result = new List<VirtualFieldNode>();

                void Toggle(IReadOnlyCollection<VirtualFieldNode> parentNodes)
                {
                    result.AddRange(parentNodes);
                    foreach (var childNode in parentNodes)
                    {
                        Toggle(childNode.Children.ToList());
                    }
                }

                Toggle(nodes.ToList());
                return result.ToArray();
            }

            /// <summary>
            /// Сворачивает иерархию на основе строк с иерархическими ключами
            /// </summary>
            public static IEnumerable<VirtualFieldNode> Parse(IEnumerable<string> nodeIDsString)
            {
                if (nodeIDsString == null)
                {
                    return new VirtualFieldNode[0];
                }

                var normilizedNodeIdSeq = nodeIDsString;

                IEnumerable<VirtualFieldNode> GetNodes(string parentNodeId, int level)
                {
                    return normilizedNodeIdSeq.Where(s => s.Count(c => c.Equals('.')) == level) // количество символов '.' в ключе = его zero-based уровню в иерархии
                        .Where(s => string.IsNullOrWhiteSpace(parentNodeId) || s.IndexOf(parentNodeId.TrimEnd(']'), StringComparison.Ordinal) >= 0) // только наследники
                        .Select(id => new VirtualFieldNode
                        {
                            TreeId = id,
                            Id = ParseFieldTreeId(id),
                            Children = GetNodes(id, level + 1)
                        }).ToArray();
                }

                return GetNodes(null, 0);
            }

            /// <summary>
            /// Возвращает иерархию полей Join-контента
            /// </summary>
            /// <param name="virtualContentId"></param>
            /// <returns></returns>
            public static IEnumerable<VirtualFieldNode> GetVirtualJoinFieldNodes(int virtualContentId)
            {
                if (virtualContentId == 0)
                {
                    return new VirtualFieldNode[0];
                }

                var content = ContentRepository.GetById(virtualContentId);
                var fields = content.Fields;

                IEnumerable<VirtualFieldNode> AddChildFieldNodes(int? parentFieldId, string parentFieldTreeId)
                {
                    // ReSharper disable once AccessToModifiedClosure
                    var childFieldNodes = fields.Where(f => f.JoinId == parentFieldId).Select(f => new VirtualFieldNode { Id = f.Id }).ToArray();
                    foreach (var childFieldNode in childFieldNodes)
                    {
                        var childTreeId = GetFieldTreeId(childFieldNode.Id, parentFieldTreeId);
                        childFieldNode.TreeId = childTreeId;
                        childFieldNode.Children = AddChildFieldNodes(childFieldNode.Id, childTreeId);
                    }

                    return childFieldNodes;
                }

                return AddChildFieldNodes(null, null);
            }

            /// <summary>
            /// Формирует id поля в дереве
            /// </summary>
            /// <param name="fieldId">id поля в БД</param>
            /// <param name="parentFieldTreeId">id родительского поля в дереве</param>
            /// <returns>id поля в дереве</returns>
            internal static string GetFieldTreeId(int fieldId, string parentFieldTreeId = null)
            {
                const string rootLevelTreeId = "[]";
                if (string.IsNullOrWhiteSpace(parentFieldTreeId) || parentFieldTreeId.Equals(rootLevelTreeId))
                {
                    return $"[{fieldId}]";
                }

                return string.Format(string.Concat(parentFieldTreeId.Trim(']'), ".{0}]"), fieldId);
            }

            /// <summary>
            /// Парсит id поля в дереве и возвращает id поля в БД
            /// </summary>
            /// <param name="fieldTreeId">id поля в дереве</param>
            /// <returns>id поля в БД</returns>
            internal static int ParseFieldTreeId(string fieldTreeId)
            {
                if (string.IsNullOrWhiteSpace(fieldTreeId))
                {
                    return 0;
                }

                if (fieldTreeId.StartsWith("[") && fieldTreeId.EndsWith("]"))
                {
                    var fidStr = fieldTreeId.Trim('[', ']').Split('.').LastOrDefault();
                    return int.Parse(fidStr ?? throw new InvalidOperationException());
                }

                throw new FormatException("Field Id format string");
            }

            /// <summary>
            /// Возвращает id родительского поля в дереве
            /// </summary>
            /// <param name="fieldTreeId"></param>
            internal static string GetParentFieldTreeId(string fieldTreeId)
            {
                var lastPointIndex = fieldTreeId.LastIndexOf('.');
                return lastPointIndex < 0 ? null : string.Concat(fieldTreeId.Substring(0, lastPointIndex), ']');
            }

            /// <summary>
            /// Добавляет отсутствующие id полей в дереве для верхнего уровня иерархии
            /// так, что бы у каждого id (кроме рутовых) был предок
            /// </summary>
            public static IEnumerable<string> NormalizeFieldTreeIdSeq(IEnumerable<string> fieldTreeIDs)
            {
                var result = new List<string>(fieldTreeIDs);

                void Round(List<string> seq)
                {
                    var added = false;
                    foreach (var id in seq.OrderByDescending(i => i.Length))
                    {
                        var parentId = GetParentFieldTreeId(id);
                        if (!string.IsNullOrEmpty(parentId) && !result.Contains(parentId))
                        {
                            added = true;
                            result.Add(parentId);
                        }
                    }

                    if (added)
                    {
                        Round(seq);
                    }
                }

                Round(result);
                return result;
            }
        }

        public class TreeItem
        {
            public int ContentId { get; set; }

            public int Level { get; set; }
        }

        public const int MinPageSize = 5;
        public const int MaxPageSize = 100;
        public const int DefaultPageSize = 20;
        public const int MinLimitOfStoredVersions = 1;
        public const int MaxLimitOfStoredVersions = 30;
        public const int DefaultLimitOfStoredVersions = 10;

        internal static ReadOnlyCollection<UserQueryColumn> SystemMandatoryColumns = new(new List<UserQueryColumn>
        {
            new() { ColumnName = FieldName.ContentItemId, DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.StatusTypeId, DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.Visible, DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.Archive, DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.Created, DbType = "datetime" },
            new() { ColumnName = FieldName.Modified, DbType = "datetime" },
            new() { ColumnName = FieldName.LastModifiedBy, DbType = "numeric", NumericScale = 0 }
        });

        internal static ReadOnlyCollection<UserQueryColumn> PgSystemMandatoryColumns = new(new List<UserQueryColumn>
        {
            new() { ColumnName = FieldName.ContentItemId.ToLowerInvariant(), DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.StatusTypeId.ToLowerInvariant(), DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.Visible.ToLowerInvariant(), DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.Archive.ToLowerInvariant(), DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.Created.ToLowerInvariant(), DbType = "timestamp with time zone" },
            new() { ColumnName = FieldName.Modified.ToLowerInvariant(), DbType = "timestamp with time zone" },
            new() { ColumnName = FieldName.LastModifiedBy.ToLowerInvariant(), DbType = "numeric", NumericScale = 0 }
        });

        internal static ReadOnlyCollection<UserQueryColumn> NativePgSystemMandatoryColumns = new(new List<UserQueryColumn>
        {
            new() { ColumnName = FieldName.ContentItemId.ToLowerInvariant(), DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.StatusTypeId.ToLowerInvariant(), DbType = "numeric", NumericScale = 0 },
            new() { ColumnName = FieldName.Visible.ToLowerInvariant(), DbType = "boolean" },
            new() { ColumnName = FieldName.Archive.ToLowerInvariant(), DbType = "boolean" },
            new() { ColumnName = FieldName.Created.ToLowerInvariant(), DbType = "timestamp with time zone" },
            new() { ColumnName = FieldName.Modified.ToLowerInvariant(), DbType = "timestamp with time zone" },
            new() { ColumnName = FieldName.LastModifiedBy.ToLowerInvariant(), DbType = "numeric", NumericScale = 0 }
        });

        internal static ReadOnlyCollection<UserQueryColumn> GetUserQueryMandatoryColumns(bool useNativeEfTypes) => QPContext.DatabaseType == DatabaseType.Postgres
                ? (useNativeEfTypes ? NativePgSystemMandatoryColumns : PgSystemMandatoryColumns)
                : SystemMandatoryColumns;

        private Site _site;
        private IEnumerable<Field> _fields;
        private ContentWorkflowBind _workflowBinding;
        private IEnumerable<QpPluginFieldValue> _qpPluginFieldValues;
        private IEnumerable<ContentConstraint> _constraints;
        private readonly Lazy<IEnumerable<Content>> _aggregatedContents;
        private readonly InitPropertyValue<Content> _baseAggregationContent;
        private readonly InitPropertyValue<Field> _treeField;
        private readonly InitPropertyValue<Field> _variationField;
        private readonly InitPropertyValue<IEnumerable<int>> _unionContentIDs;
        private readonly Lazy<IEnumerable<Content>> _virtualSubContents;
        private readonly Lazy<IEnumerable<UserQueryColumn>> _userQueryContentViewSchema;
        private readonly InitPropertyValue<IEnumerable<VirtualFieldNode>> _virtualJoinFieldNodes;
        private readonly InitPropertyValue<Content> _parentContent;
        private readonly InitPropertyValue<IEnumerable<Content>> _childContents;
        private readonly InitPropertyValue<ContentGroup> _contentGroup;

        public Content()
        {
            PageSize = DefaultPageSize;
            AllowItemsPermission = false;
            AutoArchive = false;
            IsShared = false;
            UseVersionControl = true;
            MaxNumOfStoredVersions = DefaultLimitOfStoredVersions;
            UseDefaultFiltration = true;
            ForReplication = true;
            PathHelper = new PathHelper(new DbService(new S3Options()));
            _virtualJoinFieldNodes = new InitPropertyValue<IEnumerable<VirtualFieldNode>>(() =>
            {
                if (!IsNew && VirtualType == Constants.VirtualType.Join)
                {
                    return VirtualFieldNode.GetVirtualJoinFieldNodes(Id);
                }

                return new VirtualFieldNode[0];
            });

            _treeField = new InitPropertyValue<Field>(() =>
            {
                var id = ContentRepository.GetTreeFieldId(Id);
                return id == 0 ? null : FieldRepository.GetById(id);
            });

            _variationField = new InitPropertyValue<Field>(() =>
            {
                var id = ContentRepository.GetVariationFieldId(Id);
                return id == 0 ? null : FieldRepository.GetById(id);
            });

            _unionContentIDs = new InitPropertyValue<IEnumerable<int>>(() => VirtualContentRepository.GetUnionSourceContents(Id));
            _virtualSubContents = new Lazy<IEnumerable<Content>>(() => ContentRepository.GetVirtualSubContents(Id));
            _userQueryContentViewSchema = new Lazy<IEnumerable<UserQueryColumn>>(GetUserQueryContentViewSchema);
            _baseAggregationContent = new InitPropertyValue<Content>(() => ContentRepository.GetBaseAggregationContent((Id)));
            _aggregatedContents = new Lazy<IEnumerable<Content>>(() => ContentRepository.GetAggregatedContents(Id));
            _parentContent = new InitPropertyValue<Content>(() => ParentContentId.HasValue ? ContentRepository.GetById(ParentContentId.Value) : null);
            _childContents = new InitPropertyValue<IEnumerable<Content>>(() => ContentRepository.GetChildList(Id));
            _contentGroup = new InitPropertyValue<ContentGroup>(() => ContentRepository.GetGroupById(GroupId));
        }

        public static Content Create(Site site)
        {
            Content result = new Content();
            result.Site = site;
            result.SiteId = site.Id;
            result.UseNativeEfTypes = site.UseNativeEfTypes;
            result.GroupId = ContentGroup.GetDefaultGroup(site.Id).Id;
            return result;
        }

        public int[] ForceFieldIds { get; set; }

        public int[] ForceLinkIds { get; set; }

        [Required(ErrorMessageResourceName = "NameNotEntered", ErrorMessageResourceType = typeof(ContentStrings))]
        [StringLength(255, ErrorMessageResourceName = "NameMaxLengthExceeded", ErrorMessageResourceType = typeof(ContentStrings))]
        [RegularExpression(RegularExpressions.EntityName, ErrorMessageResourceName = "NameInvalidFormat", ErrorMessageResourceType = typeof(ContentStrings))]
        [Display(Name = "Name", ResourceType = typeof(ContentStrings))]
        public override string Name { get; set; }

        [StringLength(512, ErrorMessageResourceName = "DescriptionMaxLengthExceeded", ErrorMessageResourceType = typeof(ContentStrings))]
        [Display(Name = "Description", ResourceType = typeof(ContentStrings))]
        public override string Description { get; set; }

        [StringLength(255, ErrorMessageResourceName = "FriendlyPluralMaxLengthExceeded", ErrorMessageResourceType = typeof(ContentStrings))]
        [Display(Name = "Plural", ResourceType = typeof(ContentStrings))]
        public string FriendlyPluralName { get; set; }

        [StringLength(255, ErrorMessageResourceName = "FriendlySingularMaxLengthExceeded", ErrorMessageResourceType = typeof(ContentStrings))]
        [Display(Name = "Singular", ResourceType = typeof(ContentStrings))]
        public string FriendlySingularName { get; set; }

        [Display(Name = "EnableArticlesPermissions", ResourceType = typeof(ContentStrings))]
        public bool AllowItemsPermission { get; set; }

        [Display(Name = "Group", ResourceType = typeof(ContentStrings))]
        public int GroupId { get; set; }

        [Display(Name = "EnableSiteSharing", ResourceType = typeof(ContentStrings))]
        public bool IsShared { get; set; }

        [Display(Name = "ArchiveOnRemoval", ResourceType = typeof(ContentStrings))]
        public bool AutoArchive { get; set; }

        [Display(Name = "CreateVersions", ResourceType = typeof(ContentStrings))]
        public bool UseVersionControl { get; set; }

        [Display(Name = "MaximumNumberOfStoredVersions", ResourceType = typeof(ContentStrings))]
        public int MaxNumOfStoredVersions { get; set; }

        [Display(Name = "ArticlesNumberPerPage", ResourceType = typeof(ContentStrings))]
        public int PageSize { get; set; }

        [Display(Name = "MapAsClass", ResourceType = typeof(ContentStrings))]
        public bool MapAsClass { get; set; }

        [StringLength(255, ErrorMessageResourceName = "NameSingularMaxLengthExceeded", ErrorMessageResourceType = typeof(ContentStrings))]
        [RegularExpression(RegularExpressions.NetName, ErrorMessageResourceName = "NameSingularInvalidFormat", ErrorMessageResourceType = typeof(ContentStrings))]
        [Display(Name = "NameSingular", ResourceType = typeof(ContentStrings))]
        public string NetName { get; set; }

        [StringLength(255, ErrorMessageResourceName = "NamePluralMaxLengthExceeded", ErrorMessageResourceType = typeof(ContentStrings))]
        [RegularExpression(RegularExpressions.NetName, ErrorMessageResourceName = "NamePluralInvalidFormat", ErrorMessageResourceType = typeof(ContentStrings))]
        [Display(Name = "NamePlural", ResourceType = typeof(ContentStrings))]
        public string NetPluralName { get; set; }

        [Display(Name = "UseDefaultFiltration", ResourceType = typeof(ContentStrings))]
        public bool UseDefaultFiltration { get; set; }

        [StringLength(255, ErrorMessageResourceName = "AddContextClassNameLengthExceeded", ErrorMessageResourceType = typeof(ContentStrings))]
        [RegularExpression(RegularExpressions.FullQualifiedNetName, ErrorMessageResourceName = "AddContextClassNameInvalidFormat", ErrorMessageResourceType = typeof(ContentStrings))]
        [Display(Name = "AdditionalContextClassName", ResourceType = typeof(ContentStrings))]
        public string AdditionalContextClassName { get; set; }

        [Display(Name = "VirtualType", ResourceType = typeof(ContentStrings))]
        public int VirtualType { get; set; }

        /// <summary>
        /// Тип виртуального контента в БД
        /// (не изменяеться на основании данных из формы)
        /// </summary>
        public int StoredVirtualType { get; set; }

        [Display(Name = "JoinRoot", ResourceType = typeof(ContentStrings))]
        public int? JoinRootId { get; set; }

        public string Query { get; set; }

        public string AltQuery { get; set; }

        [Display(Name = "TraceImportScript", ResourceType = typeof(ContentStrings))]
        public string TraceImportScript { get; set; }

        public int SiteId { get; set; }

        [Display(Name = "XamlValidation", ResourceType = typeof(ContentStrings))]
        public string XamlValidation { get; set; }

        [Display(Name = "DisableXamlValidation", ResourceType = typeof(ContentStrings))]
        public bool DisableXamlValidation { get; set; }

        [Display(Name = "CreateDefaultXAMLValidation", ResourceType = typeof(ContentStrings))]
        public bool CreateDefaultXamlValidation { get; set; }

        [Display(Name = "ParentContent", ResourceType = typeof(ContentStrings))]
        public int? ParentContentId { get; set; }

        [Display(Name = "UseForContext", ResourceType = typeof(ContentStrings))]
        public bool UseForContext { get; set; }

        [Display(Name = "ForReplication", ResourceType = typeof(ContentStrings))]
        public bool ForReplication { get; set; }

        [Display(Name = "DisableChangingActions", ResourceType = typeof(ContentStrings))]
        public bool DisableChangingActions { get; set; }

        [Display(Name = "UseNativeEfTypes", ResourceType = typeof(ContentStrings))]
        public bool UseNativeEfTypes { get; set; }

        [Display(Name = "FormScript", ResourceType = typeof(ContentStrings))]
        public string FormScript { get; set; }

        public int[] NewVirtualFieldIds { get; set; }

        public int[] ForceVirtualFieldIds { get; set; }

        public bool IsVirtual => VirtualType != 0;

        public string VirtualTypeString => GetVirtualTypeString(VirtualType);

        public static string GetVirtualTypeString(int virtualTypeCode)
        {
            switch (virtualTypeCode)
            {
                case 1:
                    return "JOIN";
                case 2:
                    return "UNION";
                case 3:
                    return "USER QUERY";
                default:
                    return string.Empty;
            }
        }

        public static string GetReceiverTypeString(int receiverTypeCode)
        {
            switch (receiverTypeCode)
            {
                case 0:
                    return NotificationStrings.RadioUser;
                case 1:
                    return NotificationStrings.RadioUserGroup;
                case 2:
                    return NotificationStrings.RadioEveryone;
                case 3:
                    return NotificationStrings.RadioFromArticle;
                case 4:
                    return NotificationStrings.RadioFromContent;
                case 5:
                    return NotificationStrings.RadioNone;
                default:
                    return string.Empty;
            }
        }

        public string GroupName => Group == null ? string.Empty : Group.Name;

        public string SelfRelationFieldFilter
        {
            get
            {
                var field = TreeField;
                return field == null ? string.Empty :
                    field.Content.UseNativeEfTypes ? Field.DefaultRelationNativeFilter : Field.DefaultRelationFilter;
            }
        }

        public override string EntityTypeCode => IsVirtual ? Constants.EntityTypeCode.VirtualContent : Constants.EntityTypeCode.Content;

        public override int ParentEntityId => SiteId;

        public override string CannotAddBecauseOfSecurityMessage => ContentStrings.CannotAddBecauseOfSecurity;

        public override string CannotUpdateBecauseOfSecurityMessage => ContentStrings.CannotUpdateBecauseOfSecurity;

        public override string PropertyIsNotUniqueMessage => $"{ContentStrings.NameNonUnique}: {Name}({Id})";

        public bool HasTreeField => ContentRepository.HasContentTreeField(Id);

        public bool HasAnyNotification => ContentRepository.HasAnyNotifications(Id);

        [BindNever]
        [ValidateNever]

        public bool HasAggregatedFields
        {
            get { return Fields.Any(f => f.Aggregated); }
        }

        [BindNever]
        [ValidateNever]
        [JsonIgnore]
        public Content BaseAggregationContent => _baseAggregationContent.Value;

        [BindNever]
        [ValidateNever]
        public IEnumerable<Content> AggregatedContents => _aggregatedContents.Value;

        [BindNever]
        [ValidateNever]
        public Field TreeField => _treeField.Value;

        [BindNever]
        [ValidateNever]
        public Field VariationField => _variationField.Value;

        [BindNever]
        [ValidateNever]

        internal IEnumerable<Field> FieldsForCreateLike
        {
            get
            {
                var currentFieldIds = new HashSet<int>(Fields.Select(n => n.Id));
                return Fields
                    .Where(n => !n.BackRelationId.HasValue || currentFieldIds.Contains(n.BackRelationId.Value))
                    .Where(f => !f.IsClassifier)
                    .OrderBy(n => n.Order);
            }
        }

        [BindNever]
        [ValidateNever]
        public IEnumerable<ContentConstraint> Constraints => _constraints ?? (_constraints = IsNew ? Enumerable.Empty<ContentConstraint>() : ContentConstraintRepository.GetConstraintsByContentId(Id));

        [BindNever]
        [ValidateNever]
        public IEnumerable<Field> Fields
        {
            get
            {
                if (_fields == null)
                {
                    _fields = (IsNew ? Enumerable.Empty<Field>() : FieldRepository.GetFullList(Id)).ToList();
                    foreach (var current in _fields)
                    {
                        current.Content = this;
                    }
                }

                return _fields;
            }
        }

        [BindNever]
        [ValidateNever]
        public IEnumerable<Field> RelateableFields
        {
            get { return Fields.Where(f => f.IsRelateable).Select(f => f); }
        }

        [BindNever]
        [ValidateNever]
        public ContentGroup Group => _contentGroup.Value;

        [JsonIgnore]
        public ContentWorkflowBind WorkflowBinding
        {
            get
            {
                if (_workflowBinding == null)
                {
                    LoadWorkflowBinding();
                }

                return _workflowBinding;
            }
            set => _workflowBinding = value;
        }
        public IEnumerable<QpPluginFieldValue> QpPluginFieldValues
        {
            get => _qpPluginFieldValues = _qpPluginFieldValues ?? ContentRepository.GetPluginValues(Id);
            set => _qpPluginFieldValues = value;
        }

        public IEnumerable<QpPluginFieldValueGroup> QpPluginFieldValueGroups => QpPluginFieldValues.ToFieldGroups();

        [BindNever]
        [ValidateNever]
        public Site Site
        {
            get => _site ?? (_site = SiteRepository.GetById(SiteId));
            set => _site = value;
        }

        [BindNever]
        [ValidateNever]
        public PathInfo RootVersionsLibrary => PathInfo.GetSubPathInfo(ArticleVersion.RootFolder);

        [BindNever]
        [ValidateNever]
        public PathInfo RootContentsPathInfo => Site.BasePathInfo.GetSubPathInfo("contents");

        [BindNever]
        [ValidateNever]
        public override PathInfo PathInfo => RootContentsPathInfo.GetSubPathInfo(Id.ToString());


        [BindNever]
        [ValidateNever]
        public override EntityObject Parent => Site;


        [BindNever]
        [ValidateNever]
        public Content ParentContent => _parentContent.Value;


        [BindNever]
        [ValidateNever]
        public IEnumerable<Content> ChildContents => _childContents.Value;

        [BindNever]
        [ValidateNever]
        public IEnumerable<VirtualFieldNode> VirtualJoinFieldNodes
        {
            get => _virtualJoinFieldNodes.Value;
            set => _virtualJoinFieldNodes.Value = value;
        }

        [BindNever]
        [ValidateNever]
        public IEnumerable<Content> VirtualSubContents => _virtualSubContents.Value;

        [BindNever]
        [ValidateNever]
        internal IEnumerable<UserQueryColumn> UserQueryContentViewSchema => _userQueryContentViewSchema.Value;

        [Display(Name = "UnionSourceContents", ResourceType = typeof(ContentStrings))]
        [ModelBinder(BinderType = typeof(IdArrayBinder))]
        public IEnumerable<int> UnionSourceContentIDs
        {
            get => _unionContentIDs.Value;
            set => _unionContentIDs.Value = value;
        }

        /// <summary>
        /// Текст запроста для User Query контента
        /// </summary>
        [Display(Name = "UserQuery", ResourceType = typeof(ContentStrings))]
        public string UserQuery { get; set; }

        /// <summary>
        /// Текст альтернативного запроста для united view из User Query контента
        /// </summary>
        [Display(Name = "UserQueryAlternative", ResourceType = typeof(ContentStrings))]
        public string UserQueryAlternative { get; set; }

        [JsonIgnore]
        [BindNever]
        [ValidateNever]
        public PathHelper PathHelper { get; set; }

        public override void DoCustomBinding()
        {

            if (!UseVersionControl)
            {
                MaxNumOfStoredVersions = 0;
            }

            if (!MapAsClass)
            {
                NetName = null;
                NetPluralName = null;
                UseDefaultFiltration = true;
                AdditionalContextClassName = null;
            }

            if (CreateDefaultXamlValidation)
            {
                XamlValidation = GenerateDefaultValidationXaml();
            }
            else
            {
                XamlValidation = string.IsNullOrWhiteSpace(XamlValidation) ? null : XamlValidation;
            }
        }

        public override void Validate()
        {
            var errors = new RulesException<Content>();
            base.Validate(errors);

            ValidateAggregated(errors);
            ValidateConditionalRequirements(errors);

            if (VirtualType != Constants.VirtualType.None)
            {
                ValidateVirtualContent(errors);
            }

            switch (VirtualType)
            {
                case Constants.VirtualType.Join:
                    ValidateJoinVirtualContent(errors);
                    break;
                case Constants.VirtualType.Union:
                    ValidateUnionVirtualContent(errors);
                    break;
                case Constants.VirtualType.UserQuery:
                    ValidateUserQueryVirtualContent(errors);
                    break;
            }

            if (!PageSize.IsInRange(MinPageSize, MaxPageSize))
            {
                errors.ErrorFor(c => c.PageSize, string.Format(ContentStrings.ArticlesNumberPerPageNotInRange, MinPageSize, MaxPageSize));
            }

            if (VirtualType == Constants.VirtualType.None)
            {
                ValidateXaml(errors);
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        private void ValidateXaml(RulesException<Content> errors)
        {
            if (!DisableXamlValidation && !CreateDefaultXamlValidation && !string.IsNullOrWhiteSpace(XamlValidation))
            {
                var data = Fields.ToDictionary(f => f.FormName, f => string.Empty);
                string baseXamlValidation = null;
                if (BaseAggregationContent != null)
                {
                    baseXamlValidation = BaseAggregationContent.XamlValidation;
                    var baseData = BaseAggregationContent.Fields.ToDictionary(f => f.FormName, f => string.Empty);
                    baseData.ToList().ForEach(x => data[x.Key] = x.Value);

                }

                data[FieldName.ContentItemId] = string.Empty;
                data[FieldName.StatusTypeId] = string.Empty;

                try
                {
                    ValidationServices.TestValidator(data, XamlValidation, Site.XamlDictionaries, baseXamlValidation);
                }
                catch (Exception exp)
                {
                    var message = exp.InnerException != null ? exp.InnerException.Message : exp.Message;
                    CurrentLogger.ForErrorEvent()
                        .Exception(exp)
                        .Message($"Testing XAML validator for content {Id} failed")
                        .Log();
                    errors.ErrorFor(f => f.XamlValidation, $"{ContentStrings.XamlValidation}: {message}");

                }
            }
        }

        private void ValidateAggregated(RulesException<Content> errors)
        {
            // изменение некоторых свойств запрещено
            if (HasAggregatedFields && !IsNew)
            {
                var dbContent = ContentRepository.GetById(Id);
                if (dbContent == null)
                {
                    throw new Exception(string.Format(ContentStrings.ContentNotFound, Id));
                }

                if (AutoArchive != dbContent.AutoArchive) // Архивировать при удалении
                {
                    errors.ErrorFor(f => AutoArchive, ContentStrings.CannotChangeAutoArchive);
                }
            }
        }

        public void ValidateForRemove(IList<string> violationMessages)
        {
            // нельзя удалять если контент агрегированный и есть хотя бы одна статья
            if (HasAggregatedFields && ((IContentRepository)new ContentRepository()).IsAnyArticle(Id))
            {
                violationMessages.Add(string.Format(ContentStrings.ContentIsAggregated, Environment.NewLine + string.Join(Environment.NewLine, Fields.Where(f => f.Aggregated).Select(c => c.Name).ToArray())));
            }

            var contentIdComparer = new LambdaEqualityComparer<Content>((c1, c2) => c1.Id == c2.Id, c => c.Id);

            // Получить контенты у которых хотя бы одно поле ссылается на данные контент
            var relatedO2MContents = ContentRepository.GetRelatedO2MContents(this);
            var relatedM2MContents = ContentRepository.GetRelatedM2MContents(this);
            var relatedContents = relatedM2MContents.Union(relatedO2MContents).ToList();
            if (relatedContents.Any())
            {
                violationMessages.Add(string.Format(ContentStrings.RelatedContentsExist, Environment.NewLine + string.Join(Environment.NewLine, relatedContents.Distinct(contentIdComparer).Select(c => c.Name).ToArray())));
            }

            // проверка на существование подчиненных виртуальных контентов
            var inheritedContents = VirtualSubContents
                .Where(c => c.VirtualType == Constants.VirtualType.UserQuery) // UQ контенты
                .Concat(VirtualContentRepository.GetJoinRelatedContents(this)) // JOIN контенты
                .Concat(VirtualSubContents.Where(c => c.VirtualType == Constants.VirtualType.Union && c.UnionSourceContentIDs.Count() < 2))
                .ToList();

            if (inheritedContents.Any())
            {
                violationMessages.Add(string.Format(ContentStrings.VirtualSubContentsExist, Environment.NewLine + string.Join(Environment.NewLine, inheritedContents.Distinct(contentIdComparer).Select(c => c.Name).ToArray())));
            }

            if (!IsContentChangingActionsAllowed)
            {
                violationMessages.Add(ContentStrings.ContentChangingIsProhibited);
            }
        }

        public void LoadWorkflowBinding()
        {
            _workflowBinding = IsNew ? WorkflowRepository.GetDefaultWorkflow(this) : WorkflowRepository.GetContentWorkflow(this);
        }

        public int GetMaxFieldsOrder()
        {
            return Fields.Any() ? Fields.OrderByDescending(f => f.Order).First().Order : 0;
        }

        public int GetMinFieldsOrder()
        {
            return Fields.Any() ? Fields.OrderBy(f => f.Order).First().Order : 0;
        }

        public Content Persist()
        {
            Content result;
            if (IsNew)
            {
                if (!ParentContentId.HasValue)
                {
                    result = ContentRepository.Save(this, true);
                }
                else
                {
                    var contentToCopy = ContentRepository.GetById(ParentContentId.Value);
                    contentToCopy.Name = Name;
                    contentToCopy.ParentContentId = ParentContentId.Value;
                    contentToCopy.Description = Description;
                    contentToCopy.FriendlySingularName = FriendlySingularName;
                    contentToCopy.FriendlyPluralName = FriendlyPluralName;
                    contentToCopy.MapAsClass = MapAsClass;
                    if (contentToCopy.MapAsClass)
                    {
                        contentToCopy.NetName = NetName;
                        contentToCopy.NetPluralName = NetPluralName;
                    }
                    result = ContentRepository.Copy(contentToCopy, ForceId, ForceFieldIds, ForceLinkIds, true);
                }

                result.CreateContentFolders();
            }
            else
            {
                result = ContentRepository.Update(this);
                if (!result.UseVersionControl)
                {
                    PathHelper.RemoveFolder(result.RootVersionsLibrary.Path);
                }
            }

            return result;
        }

        public IEnumerable<string> Die()
        {
            using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(Id))
            {
                var violationMessages = new List<string>();
                ValidateForRemove(violationMessages);
                if (!violationMessages.Any())
                {
                    DieWithoutValidation();
                }

                return violationMessages;
            }
        }

        public void DieWithoutValidation()
        {
            var helper = new VirtualContentHelper();
            using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(Id))
            {
                helper.RemoveSourceContentFromUnions(this);
                helper.RemoveContentData(this);
                ContentRepository.Delete(Id);
                DeleteContentFolders();
            }
        }

        public IEnumerable<ArticleContextSearchBlockItem> GetContextSearchBlockItems()
        {
            return Fields
                .Where(n => n.UseForContext)
                .OrderBy(n => n.Order)
                .Select(n => new { Content = n.RelatedToContent, FieldId = n.Id })
                .Select(n => new ArticleContextSearchBlockItem
                {
                    ContentName = n.Content.Name,
                    ContentId = n.Content.Id,
                    FieldId = n.FieldId
                });
        }

        internal Article CreateArticle()
        {
            if (IsVirtual)
            {
                throw new Exception(string.Format(ArticleStrings.AppendToVirtual, Id));
            }

            return new Article(this);
        }

        internal Article CreateArticle(Dictionary<string, string> predefinedValues)
        {
            if (IsVirtual)
            {
                throw new Exception(string.Format(ArticleStrings.AppendToVirtual, Id));
            }

            return new Article(this, predefinedValues);
        }

        internal PathInfo GetVersionPathInfo(int versionId) => RootVersionsLibrary.GetSubPathInfo(versionId == ArticleVersion.CurrentVersionId ? "current" : versionId.ToString());

        internal bool HasNotifications(string code)
        {
            return code.Split(';').Any(currentCode => ContentRepository.HasNotifications(Id, currentCode));
        }

        internal void CreateContentFolders()
        {
            if (!PathHelper.UseS3)
            {
                Directory.CreateDirectory(PathInfo.Path);
            }
        }

        internal void DeleteContentFolders()
        {
            PathHelper.RemoveFolder(PathInfo.Path);
        }

        internal void MutateNames()
        {
            var name = Name;
            var index = 0;
            do
            {
                index++;
                Name = MutateHelper.MutateString(name, index);
            } while (ContentRepository.NameExists(this));

            var netName = NetName;
            if (!String.IsNullOrEmpty(netName))
            {
                index = 0;
                do
                {
                    index++;
                    NetName = MutateHelper.MutateNetName(netName, index);
                } while (ContentRepository.NetNameExists(this));
            }

            var netPluralName = NetPluralName;
            if (!String.IsNullOrEmpty(netPluralName))
            {
                index = 0;
                do
                {
                    index++;
                    NetPluralName = MutateHelper.MutateNetName(netPluralName, index);
                } while (ContentRepository.NetPluralNameExists(this));
            }
        }

        internal void DeleteFields()
        {
            foreach (var field in Fields)
            {
                ((IFieldRepository)new FieldRepository()).Delete(field.Id);
            }
        }

        internal bool AssignedAsyncWorkflow => WorkflowBinding.IsAssigned && WorkflowBinding.IsAsync;

        /// <summary>
        /// Проверяет можно ли для статей контента выполнять изменяющие операции
        /// </summary>
        internal bool IsArticleChangingActionsAllowed(bool? boundToExternal) => !DisableChangingActions || boundToExternal.HasValue && boundToExternal.Value;

        public bool IsContentChangingActionsAllowed => !(DisableChangingActions && !QPContext.IsAdmin);

        private void ValidateConditionalRequirements(RulesException<Content> errors)
        {
            if (MapAsClass)
            {
                if (string.IsNullOrEmpty(NetName))
                {
                    errors.ErrorFor(n => n.NetName, ContentStrings.NameSingularNotEntered);
                }
                else if (ContentRepository.NetNameExists(this))
                {
                    errors.ErrorFor(n => n.NetName, ContentStrings.NameSingularNonUnique);
                }

                if (string.IsNullOrEmpty(NetPluralName))
                {
                    errors.ErrorFor(n => n.NetPluralName, ContentStrings.NamePluralNotEntered);
                }
                else if (ContentRepository.NetPluralNameExists(this))
                {
                    errors.ErrorFor(n => n.NetPluralName, ContentStrings.NamePluralNonUnique);
                }

                if (!string.IsNullOrEmpty(AdditionalContextClassName))
                {
                    if (ContentRepository.ContextClassNameExists(this))
                    {
                        errors.ErrorFor(n => n.AdditionalContextClassName, ContentStrings.AddContextClassNameNonUnique);
                    }
                }
            }
        }

        /// <summary>
        /// Валидация виртуального контента
        /// </summary>
        /// <param name="errors"></param>
        private void ValidateVirtualContent(RulesException<Content> errors)
        {
            if (!IsNew)
            {
                // Нельзя менять тип виртуального контента если на нем построены другие виртуальные контенты
                if (VirtualType != StoredVirtualType && VirtualSubContents.Any())
                {
                    errors.ErrorFor(c => c.VirtualType, ContentStrings.VirtualTypeCantBeChanged);
                }
            }
        }

        /// <summary>
        /// Валидация виртуального контента типа Join
        /// </summary>
        /// <param name="errors"></param>
        private void ValidateJoinVirtualContent(RulesException<Content> errors)
        {
            if (VirtualType == Constants.VirtualType.Join)
            {
                if (!JoinRootId.HasValue)
                {
                    errors.ErrorFor(c => c.JoinRootId, ContentStrings.SourceContentNotEntered);
                }

                if (IsNew && !VirtualJoinFieldNodes.Any())
                {
                    errors.ErrorFor(c => c.VirtualJoinFieldNodes, ContentStrings.JoinFieldsNotEntered);
                }
                else if (!IsNew && VirtualJoinFieldNodes.Any())
                {
                    var violations = new FieldsConflictValidator().SubContentsCheck(this, VirtualJoinFieldNodes);
                    foreach (var violation in violations)
                    {
                        errors.ErrorFor(c => c.VirtualJoinFieldNodes, violation.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Валидация виртуального контента типа Union
        /// </summary>
        /// <param name="errors"></param>
        private void ValidateUnionVirtualContent(RulesException<Content> errors)
        {
            if (VirtualType == Constants.VirtualType.Union)
            {
                if (!UnionSourceContentIDs.Any())
                {
                    errors.ErrorFor(c => c.UnionSourceContentIDs, ContentStrings.UnionBaseContentNotEntered);
                }
                else
                {
                    // проверить соответствие полей базовых контентов
                    var violations = new FieldsConflictValidator().UnionFieldsMatchCheck(UnionSourceContentIDs.ToList());
                    foreach (var violation in violations)
                    {
                        errors.ErrorFor(c => c.UnionSourceContentIDs, violation.Message);
                    }
                }
            }
        }

        private static ReadOnlyCollection<string> UserQueryValidColumnTypes => QPContext.DatabaseType == DatabaseType.SqlServer ?
            Field.ValidFieldColumnDbTypeCollection : Field.PgValidFieldColumnDbTypeCollection;

        /// <summary>
        /// Валидация виртуального контента типа User Query
        /// </summary>
        /// <param name="errors"></param>
        internal void ValidateUserQueryVirtualContent(RulesException<Content> errors)
        {
            if (VirtualType == Constants.VirtualType.UserQuery)
            {
                if (string.IsNullOrWhiteSpace(UserQuery))
                {
                    errors.ErrorFor(c => c.UserQuery, ContentStrings.UserQueryNotEntered);
                }
                else
                {
                    // проверить sql запросы на возможность выполнения
                    var areQueriesCorrect = true;
                    if (!VirtualContentRepository.IsQueryQueryCorrect(UserQuery, out var userQueryErrorMsg))
                    {
                        areQueriesCorrect = false;
                        errors.ErrorFor(c => c.UserQuery, string.Format(ContentStrings.UserQueryIsInvalid, userQueryErrorMsg));
                    }

                    if (!string.IsNullOrWhiteSpace(UserQueryAlternative) && !VirtualContentRepository.IsQueryQueryCorrect(UserQueryAlternative, out userQueryErrorMsg))
                    {
                        areQueriesCorrect = false;
                        errors.ErrorFor(c => c.UserQueryAlternative, string.Format(ContentStrings.UserQueryAltIsInvalid, userQueryErrorMsg));
                    }

                    if (areQueriesCorrect)
                    {
                        // проверить запрос на наличие обязательных полей
                        var userQueryViewAllColumns = VirtualContentRepository.GetQuerySchema(UserQuery).ToList();

                        // проверить на наличие обязательных полей
                        var userQueryViewUniqColumns = userQueryViewAllColumns.Distinct(UserQueryColumn.TableNameIgnoreEqualityComparer).ToList();
                        var expectedMandatoryColumns = GetUserQueryMandatoryColumns(UseNativeEfTypes).Except(userQueryViewUniqColumns, UserQueryColumn.TableNameIgnoreEqualityComparer).ToList();
                        if (expectedMandatoryColumns.Any())
                        {
                            errors.ErrorFor(c => c.UserQuery, string.Format(ContentStrings.NotAllMandatoryColumnsInUserQuery, string.Join(", ", expectedMandatoryColumns.Select(c => c.ColumnName + " (" + c.DbType + ")"))));
                        }

                        // проверить запрос на наличие недопустимых DB-типов колонок
                        var invalidTypeColumns = userQueryViewAllColumns.Where(c => !UserQueryValidColumnTypes.Contains(c.DbType)).ToList();
                        if (invalidTypeColumns.Any())
                        {
                            errors.ErrorFor(c => c.UserQuery, string.Format(ContentStrings.InvalidColumnsInUserQuery, string.Join(", ", invalidTypeColumns.Select(c => c.ColumnName + " (" + c.DbType + ")"))));
                        }

                        // проверить что типы полей во view не изменены относительно соответствующих полей в реальных таблицах
                        List<string> GetChangedTypeColumnMessage(IEnumerable<UserQueryColumn> columns)
                            => columns
                                .Where(c => !c.DbType.Equals(c.TableDbType, StringComparison.InvariantCultureIgnoreCase))
                                .Select(c => string.Format(ContentStrings.UserQueryColumnTypeChanged, c.ColumnName, c.TableName, c.TableDbType, c.DbType))
                                .ToList();

                        var customUserQueryColumns = userQueryViewAllColumns.Except(GetUserQueryMandatoryColumns(UseNativeEfTypes), UserQueryColumn.TableNameIgnoreEqualityComparer).ToList();
                        var changedTypeColumnMessages = GetChangedTypeColumnMessage(customUserQueryColumns).ToList();
                        if (changedTypeColumnMessages.Any())
                        {
                            errors.ErrorFor(c => c.UserQuery, ContentStrings.UserQuery + ": " + string.Join("; ", changedTypeColumnMessages));
                        }

                        // проверить что поля с одинаковыми именами имеют один и тот же тип в таблицах
                        List<string> GetDiffTypeColumnMessages(IEnumerable<UserQueryColumn> columns)
                            => columns
                                .GroupBy(c => c.ColumnName, StringComparer.InvariantCultureIgnoreCase)
                                .Where(g => g.Select(c => c.TableDbType).Distinct(StringComparer.InvariantCultureIgnoreCase).Count() > 1)
                                .Select(g => string.Format(ContentStrings.UserQuerySameNameColumsHaveDiffTypes, g.Key, string.Join(", ", g.Select(c => $"{SqlQuerySyntaxHelper.EscapeEntityName(QPContext.DatabaseType, c.TableName)}"))))
                                .ToList();

                        var diffTypeColumnMessage = GetDiffTypeColumnMessages(customUserQueryColumns).ToList();
                        if (diffTypeColumnMessage.Any())
                        {
                            errors.ErrorFor(c => c.UserQuery, ContentStrings.UserQuery + ": " + string.Join("; ", diffTypeColumnMessage));
                        }

                        // привести пользовательские поля в запросе к нижнему регистру
                        if (customUserQueryColumns.Any())
                        {
                            UserQuery = ConvertQueryColumnsToLowercase(UserQuery, customUserQueryColumns);
                        }

                        // если UserQueryAlternative задан
                        if (!string.IsNullOrWhiteSpace(UserQueryAlternative))
                        {
                            var userAltQueryViewAllColumns = VirtualContentRepository.GetQuerySchema(UserQueryAlternative).ToList();

                            // проверить запросы на соответствие полей
                            var userAltQueryViewUniqColumns = userAltQueryViewAllColumns.Distinct(UserQueryColumn.TableNameIgnoreEqualityComparer).ToList();
                            if (userQueryViewUniqColumns.Count != userAltQueryViewUniqColumns.Count || userQueryViewUniqColumns.Except(userAltQueryViewUniqColumns, UserQueryColumn.TableNameIgnoreEqualityComparer).Any())
                            {
                                errors.ErrorFor(c => c.UserQueryAlternative, ContentStrings.UserQueryAndAltColumnsMismatch);
                            }

                            // проверить что поля с одинаковыми именами имеют один и тот же тип в таблицах
                            changedTypeColumnMessages = GetChangedTypeColumnMessage(userAltQueryViewAllColumns);
                            if (changedTypeColumnMessages.Any())
                            {
                                errors.ErrorFor(c => c.UserQuery, ContentStrings.UserQueryAlternative + ": " + string.Join("; ", changedTypeColumnMessages));
                            }

                            // проверить что поля с одинаковыми именами имеют один и тот же тип в таблицах
                            diffTypeColumnMessage = GetDiffTypeColumnMessages(userAltQueryViewAllColumns);
                            if (diffTypeColumnMessage.Any())
                            {
                                errors.ErrorFor(c => c.UserQueryAlternative, ContentStrings.UserQueryAlternative + ": " + string.Join("; ", diffTypeColumnMessage));
                            }

                            // привести пользовательские поля в запросе к нижнему регистру
                            if (customUserQueryColumns.Any())
                            {
                                UserQueryAlternative = ConvertQueryColumnsToLowercase(UserQueryAlternative, customUserQueryColumns);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Привести указанные поля в запросе к нижнему регистру
        /// </summary>
        /// <param name="query">Строка запроса</param>
        /// <param name="columns">Поля запроса</param>
        /// <returns>Возвращает строку запроса с измененными полями</returns>
        private static string ConvertQueryColumnsToLowercase(string query, List<UserQueryColumn> columns)
        {
            foreach (UserQueryColumn column in columns)
            {
                query = query.Replace(column.ColumnName, column.ColumnName.ToLower());
            }
            return query;
        }

        /// <summary>
        /// Получить схему view UQ-контента (кроме системных полей)
        /// </summary>
        internal IEnumerable<UserQueryColumn> GetUserQueryContentViewSchema()
        {
            if (VirtualType == Constants.VirtualType.UserQuery)
            {
                // получить схему view (кроме системных полей)
                var viewName = $"content_{Id}";
                var viewColumns = VirtualContentRepository.GetViewSchema(viewName).Where(c => !GetUserQueryMandatoryColumns(UseNativeEfTypes).Contains(c, UserQueryColumn.TableNameIgnoreEqualityComparer)).ToList();

                // спец. обработка колонки content_id из union-контентов
                foreach (var c in viewColumns)
                {
                    if (c.ContentId.HasValue && Field.NameComparerPredicate("content_id", c.ColumnName))
                    {
                        var cnt = ContentRepository.GetById(c.ContentId.Value);
                        if (cnt.VirtualType == Constants.VirtualType.Union)
                        {
                            c.ResetContentBased();
                        }
                    }
                }

                return viewColumns;
            }

            return Enumerable.Empty<UserQueryColumn>();
        }

        /// <summary>
        /// Создает XAML для валидации по умолчанию
        /// </summary>
        private string GenerateDefaultValidationXaml()
        {
            var propDefs = new List<PropertyDefinition>
            {
                new PropertyDefinition
                {
                    PropertyName = FieldName.ContentItemId,
                    Alias = "Id",
                    Description = "An indentifier of the entity.",
                    PropertyType = typeof(int)
                },
                new PropertyDefinition
                {
                    PropertyName = FieldName.StatusTypeId,
                    Alias = "StatusTypeId",
                    Description = "StatusTypeId",
                    PropertyType = typeof(int)
                }
            };

            propDefs.AddRange(Fields.Select(f => new PropertyDefinition
            {
                PropertyName = f.FormName,
                Alias = f.FormName,
                PropertyType = f.XamlType,
                Description = string.IsNullOrWhiteSpace(f.FriendlyName) ? f.Name : f.FriendlyName
            }));

            return CleanPropertyDefinitions(ValidationServices.GenerateXamlValidatorText(propDefs));
        }

        public static string CleanPropertyDefinitions(string propDefs)
        {
            var re = new Regex("(<PropertyDefinition[^>]+)>.*?</PropertyDefinition>");
            return re.Replace(propDefs, "$1 />");
        }

        public int[] GetFieldIds()
        {
            return Fields.Select(n => n.Id).ToArray();
        }

        public int[] GetLinkIds()
        {
            return ContentRepository.GetContentLinks(Id).Select(n => n.LinkId).ToArray();
        }
    }
}
