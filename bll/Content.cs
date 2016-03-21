using QA.Validation.Xaml;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Validators;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Quantumart.QP8.BLL
{
    public class Content : EntityObject
    {
        #region Classes
        /// <summary>
        /// Элемент иерархии виртуальных полей для виртуального контента
        /// </summary>
        public class VirtualFieldNode
        {
            public VirtualFieldNode()
            {
                Children = Enumerable.Empty<Content.VirtualFieldNode>();
            }
            /// <summary>
            /// Id виртуального поля
            /// </summary>
            public int Id { get; set; }
            /// <summary>
            /// Подчиненные поля
            /// </summary>
            public IEnumerable<VirtualFieldNode> Children { get; set; }

            public string TreeId { get; set; }

            public string Name { get; set; }

            /// <summary>
            /// Разворачивает иерархию в плоскую структуру
            /// </summary>
            /// <returns></returns>
            public static IEnumerable<VirtualFieldNode> Linearize(IEnumerable<VirtualFieldNode> nodes)
            {
                List<VirtualFieldNode> result = new List<VirtualFieldNode>();

                Action<IEnumerable<VirtualFieldNode>> toggle = null;
                toggle = (parentNodes) =>
                    {
                        result.AddRange(parentNodes);
                        foreach (var childNode in parentNodes)
                        {
                            toggle(childNode.Children);
                        }
                    };

                toggle(nodes);
                return result.ToArray();
            }

            /// <summary>
            /// Сворачивает иерархию на основе строк с иерархическими ключами
            /// </summary>
            /// <param name="nodeIDsString"></param>
            /// <returns></returns>
            public static IEnumerable<VirtualFieldNode> Parse(IEnumerable<string> nodeIDsString)
            {
                if (nodeIDsString == null)
                    return new VirtualFieldNode[0];
                else
                {

                    IEnumerable<string> normilizedNodeIdSeq = nodeIDsString;

                    Func<string, int, IEnumerable<VirtualFieldNode>> getNodes = null;
                    getNodes = (parentNodeId, level) =>
                        {
                            return normilizedNodeIdSeq
                                    .Where(s => s.Where(c => c.Equals('.')).Count() == level) // количество символов '.' в ключе = его zero-based уровню в иерархии
                                    .Where(s => String.IsNullOrWhiteSpace(parentNodeId) || s.IndexOf(parentNodeId.TrimEnd(']')) >= 0) // только наследники
                                    .Select(id => new VirtualFieldNode
                                    {
                                        TreeId = id,
                                        Id = VirtualFieldNode.ParseFieldTreeId(id),
                                        Children = getNodes(id, level + 1)
                                    })
                                    .ToArray();
                        };


                    return getNodes(null, 0);
                }
            }

            /// <summary>
            /// Возвращает иерархию полей Join-контента
            /// </summary>
            /// <param name="virtualContentId"></param>
            /// <returns></returns>
            public static IEnumerable<VirtualFieldNode> GetVirtualJoinFieldNodes(int virtualContentId)
            {
                if (virtualContentId == 0)
                    return new Content.VirtualFieldNode[0];
                else
                {
                    Content content = ContentRepository.GetById(virtualContentId);
                    IEnumerable<Field> fields = content.Fields;

                    Func<int?, string, IEnumerable<Content.VirtualFieldNode>> addChildFieldNodes = null;
                    addChildFieldNodes = (parentFieldId, parentFieldTreeId) =>
                    {
                        var childFieldNodes = fields
                                            .Where(f => f.JoinId == parentFieldId)
                                            .Select(f => new Content.VirtualFieldNode { Id = f.Id })
                                            .ToArray();
                        foreach (var childFieldNode in childFieldNodes)
                        {
                            string childTreeId = GetFieldTreeId(childFieldNode.Id, parentFieldTreeId);
                            childFieldNode.TreeId = childTreeId;
                            childFieldNode.Children = addChildFieldNodes(childFieldNode.Id, childTreeId);
                        }
                        return childFieldNodes;
                    };

                    return addChildFieldNodes(null, null);
                }
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
                if (String.IsNullOrWhiteSpace(parentFieldTreeId) || parentFieldTreeId.Equals(rootLevelTreeId))
                    return String.Format("[{0}]", fieldId);
                else
                    return String.Format(String.Concat(parentFieldTreeId.Trim(']'), ".{0}]"), fieldId);
            }

            /// <summary>
            /// Парсит id поля в дереве и возвращает id поля в БД
            /// </summary>
            /// <param name="fieldTreeId">id поля в дереве</param>
            /// <returns>id поля в БД</returns>
            internal static int ParseFieldTreeId(string fieldTreeId)
            {
                if (String.IsNullOrWhiteSpace(fieldTreeId))
                    return 0;

                if (fieldTreeId.StartsWith("[") && fieldTreeId.EndsWith("]"))
                {
                    var fidStr = fieldTreeId.Trim('[', ']').Split('.').LastOrDefault();
                    return Int32.Parse(fidStr);
                }
                else
                    throw new FormatException("Field Id format string");

            }

            /// <summary>
            /// Возвращает id родительского поля в дереве
            /// </summary>
            /// <param name="fieldTreeId"></param>
            internal static string GetParentFieldTreeId(string fieldTreeId)
            {
                int lastPointIndex = fieldTreeId.LastIndexOf('.');
                if (lastPointIndex < 0)
                    return null;
                else
                    return String.Concat(fieldTreeId.Substring(0, lastPointIndex), ']');
            }

            /// <summary>
            /// Добавляет отсутствующие id полей в дереве для верхнего уровня иерархии
            /// так, что бы у каждого id (кроме рутовых) был предок
            /// </summary>
            /// <param name="argument"></param>
            /// <returns></returns>
            public static IEnumerable<string> NormalizeFieldTreeIdSeq(IEnumerable<string> fieldTreeIDs)
            {
                List<string> result = new List<string>(fieldTreeIDs);

                Action<IEnumerable<string>> round = null;
                round = (seq) =>
                {
                    bool added = false;
                    foreach (var id in seq.OrderByDescending(i => i.Length))
                    {
                        string parentId = GetParentFieldTreeId(id);
                        if (!String.IsNullOrEmpty(parentId) && !result.Contains(parentId))
                        {
                            added = true;
                            result.Add(parentId);
                        }
                    }
                    if (added)
                        round(seq);
                };

                round(result);
                return result;
            }

        }


        public class TreeItem
        {
            public int ContentId { get; set; }
            public int Level { get; set; }
        }

        #endregion

        #region constants

        public const int MinPageSize = 5;

        public const int MaxPageSize = 100;

        public const int DefaultPageSize = 20;

        public const int MinLimitOfStoredVersions = 1;

        public const int MaxLimitOfStoredVersions = 30;

        public const int DefaultLimitOfStoredVersions = 10;

        internal const string ContentItemIdPropertyName = "CONTENT_ITEM_ID";
        internal const string StatusTypeIdPropertyName = "STATUS_TYPE_ID";

        /// <summary>
        /// Обязательные поля контента
        /// </summary>
        internal static readonly ReadOnlyCollection<UserQueryColumn> SystemMandatoryColumns = new ReadOnlyCollection<UserQueryColumn>(new List<UserQueryColumn>
        {
            new UserQueryColumn {ColumnName = "CONTENT_ITEM_ID", DbType = "numeric", NumericScale = 0},
            new UserQueryColumn {ColumnName = "STATUS_TYPE_ID", DbType = "numeric", NumericScale = 0},
            new UserQueryColumn {ColumnName = "VISIBLE", DbType = "numeric", NumericScale = 0},
            new UserQueryColumn {ColumnName = "ARCHIVE", DbType = "numeric", NumericScale = 0},
            new UserQueryColumn {ColumnName = "CREATED", DbType = "datetime"},
            new UserQueryColumn {ColumnName = "MODIFIED", DbType = "datetime"},
            new UserQueryColumn {ColumnName = "LAST_MODIFIED_BY", DbType = "numeric", NumericScale = 0},
        });

        #endregion

        #region private fields

        private ContentWorkflowBind _WorkflowBinding;

        private Site _Site;

        private IEnumerable<Field> _Fields;

        private IEnumerable<ContentConstraint> _Constraints;

        private Lazy<IEnumerable<Content>> aggregatedContents = null;

        private InitPropertyValue<Field> treeField;

        private InitPropertyValue<Field> variationField;

        private InitPropertyValue<IEnumerable<int>> unionContentIDs = null;

        private Lazy<IEnumerable<Content>> virtualSubContents = null;

        private Lazy<IEnumerable<UserQueryColumn>> userQueryContentViewSchema = null;

        private InitPropertyValue<IEnumerable<VirtualFieldNode>> virtualJoinFieldNodes;

        private InitPropertyValue<Content> parentContent;

        private InitPropertyValue<IEnumerable<Content>> childContents;

        private InitPropertyValue<ContentGroup> contentGroup;

        #endregion

        #region creation

        public Content()
        {
            PageSize = DefaultPageSize;
            AllowItemsPermission = false;
            AutoArchive = false;
            IsShared = false;
            UseVersionControl = true;
            MaxNumOfStoredVersions = DefaultLimitOfStoredVersions;
            UseDefaultFiltration = true;

            virtualJoinFieldNodes = new InitPropertyValue<IEnumerable<VirtualFieldNode>>(() =>
            {
                if (!IsNew && VirtualType == Constants.VirtualType.Join)
                    return VirtualFieldNode.GetVirtualJoinFieldNodes(Id);
                else
                    return new VirtualFieldNode[0];
            });

            treeField = new InitPropertyValue<Field>(() =>
            {
                int id = ContentRepository.GetTreeFieldId(Id);
                return (id == 0) ? null : FieldRepository.GetById(id);
            });

            variationField = new InitPropertyValue<Field>(() =>
            {
                int id = ContentRepository.GetVariationFieldId(Id);
                return (id == 0) ? null : FieldRepository.GetById(id);
            });

            unionContentIDs = new InitPropertyValue<IEnumerable<int>>(() => VirtualContentRepository.GetUnionSourceContents(this.Id));

            virtualSubContents = new Lazy<IEnumerable<Content>>(() => ContentRepository.GetVirtualSubContents(this.Id));

            userQueryContentViewSchema = new Lazy<IEnumerable<UserQueryColumn>>(() => GetUserQueryContentViewSchema());

            aggregatedContents = new Lazy<IEnumerable<Content>>(() => ContentRepository.GetAggregatedContents(this.Id));

            parentContent = new InitPropertyValue<Content>(() => (this.ParentContentId.HasValue) ? ContentRepository.GetById(this.ParentContentId.Value) : null);

            childContents = new InitPropertyValue<IEnumerable<Content>>(() => ContentRepository.GetChildList(this.Id));

            contentGroup = new InitPropertyValue<ContentGroup>(() => ContentRepository.GetGroupById(this.GroupId));

        }


        public Content(Site site)
            : this()
        {
            Site = site;
            SiteId = site.Id;
            GroupId = ContentGroup.GetDefaultGroup(SiteId).Id;
        }

        #endregion

        #region properties

        #region simple read-write

        public int[] ForceFieldIds { get; set; }

        public int[] ForceLinkIds { get; set; }

        [RequiredValidator(MessageTemplateResourceName = "NameNotEntered", MessageTemplateResourceType = typeof(ContentStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NameMaxLengthExceeded", MessageTemplateResourceType = typeof(ContentStrings))]
        [FormatValidator(Constants.RegularExpressions.InvalidEntityName, Negated = true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(ContentStrings))]
        [LocalizedDisplayName("Name", NameResourceType = typeof(ContentStrings))]
        public override string Name
        {
            get;
            set;
        }

        [MaxLengthValidator(512, MessageTemplateResourceName = "DescriptionMaxLengthExceeded", MessageTemplateResourceType = typeof(ContentStrings))]
        [LocalizedDisplayName("Description", NameResourceType = typeof(ContentStrings))]
        public override string Description
        {
            get;
            set;
        }

        [MaxLengthValidator(255, MessageTemplateResourceName = "FriendlyPluralMaxLengthExceeded", MessageTemplateResourceType = typeof(ContentStrings))]
        [LocalizedDisplayName("Plural", NameResourceType = typeof(ContentStrings))]
        public string FriendlyPluralName
        {
            get;
            set;
        }

        [MaxLengthValidator(255, MessageTemplateResourceName = "FriendlySingularMaxLengthExceeded", MessageTemplateResourceType = typeof(ContentStrings))]
        [LocalizedDisplayName("Singular", NameResourceType = typeof(ContentStrings))]
        public string FriendlySingularName
        {
            get;
            set;
        }

        [LocalizedDisplayName("EnableArticlesPermissions", NameResourceType = typeof(ContentStrings))]
        public bool AllowItemsPermission
        {
            get;
            set;
        }

        [LocalizedDisplayName("Group", NameResourceType = typeof(ContentStrings))]
        public int GroupId
        {
            get;
            set;
        }

        [LocalizedDisplayName("EnableSiteSharing", NameResourceType = typeof(ContentStrings))]
        public bool IsShared
        {
            get;
            set;
        }

        [LocalizedDisplayName("ArchiveOnRemoval", NameResourceType = typeof(ContentStrings))]
        public bool AutoArchive
        {
            get;
            set;
        }


        [LocalizedDisplayName("CreateVersions", NameResourceType = typeof(ContentStrings))]
        public bool UseVersionControl
        {
            get;
            set;
        }

        [LocalizedDisplayName("MaximumNumberOfStoredVersions", NameResourceType = typeof(ContentStrings))]
        public int MaxNumOfStoredVersions
        {
            get;
            set;
        }

        [LocalizedDisplayName("ArticlesNumberPerPage", NameResourceType = typeof(ContentStrings))]
        public int PageSize
        {
            get;
            set;
        }

        [LocalizedDisplayName("MapAsClass", NameResourceType = typeof(ContentStrings))]
        public bool MapAsClass
        {
            get;
            set;
        }

        [MaxLengthValidator(255, MessageTemplateResourceName = "NameSingularMaxLengthExceeded", MessageTemplateResourceType = typeof(ContentStrings))]
        [FormatValidator(Constants.RegularExpressions.NetName, MessageTemplateResourceName = "NameSingularInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("NameSingular", NameResourceType = typeof(ContentStrings))]
        public string NetName
        {
            get;
            set;
        }

        [MaxLengthValidator(255, MessageTemplateResourceName = "NamePluralMaxLengthExceeded", MessageTemplateResourceType = typeof(ContentStrings))]
        [FormatValidator(Constants.RegularExpressions.NetName, MessageTemplateResourceName = "NamePluralInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("NamePlural", NameResourceType = typeof(ContentStrings))]
        public string NetPluralName
        {
            get;
            set;
        }

        [LocalizedDisplayName("UseDefaultFiltration", NameResourceType = typeof(ContentStrings))]
        public bool UseDefaultFiltration
        {
            get;
            set;
        }

        [MaxLengthValidator(255, MessageTemplateResourceName = "AddContextClassNameLengthExceeded", MessageTemplateResourceType = typeof(ContentStrings))]
        [FormatValidator(Constants.RegularExpressions.FullQualifiedNetName, MessageTemplateResourceName = "AddContextClassNameInvalidFormat", MessageTemplateResourceType = typeof(SiteStrings))]
        [LocalizedDisplayName("AdditionalContextClassName", NameResourceType = typeof(ContentStrings))]
        public string AdditionalContextClassName
        {
            get;
            set;
        }

        [LocalizedDisplayName("VirtualType", NameResourceType = typeof(ContentStrings))]
        public int VirtualType
        {
            get;
            set;
        }

        /// <summary>
        /// Тип виртуального контента в БД
        /// (не изменяеться на основании данных из формы)
        /// </summary>
        public int StoredVirtualType { get; set; }

        [LocalizedDisplayName("JoinRoot", NameResourceType = typeof(ContentStrings))]
        public int? JoinRootId
        {
            get;
            set;
        }

        public string Query
        {
            get;
            set;
        }

        public string AltQuery
        {
            get;
            set;
        }

        public int SiteId
        {
            get;
            set;
        }

        [LocalizedDisplayName("XamlValidation", NameResourceType = typeof(ContentStrings))]
        public string XamlValidation { get; set; }

        [LocalizedDisplayName("DisableXamlValidation", NameResourceType = typeof(ContentStrings))]
        public bool DisableXamlValidation { get; set; }

        [LocalizedDisplayName("CreateDefaultXAMLValidation", NameResourceType = typeof(ContentStrings))]
        public bool CreateDefaultXamlValidation { get; set; }

        [LocalizedDisplayName("ParentContent", NameResourceType = typeof(ContentStrings))]
        public int? ParentContentId { get; set; }

        [LocalizedDisplayName("UseForContext", NameResourceType = typeof(ContentStrings))]
        public bool UseForContext { get; set; }

        [LocalizedDisplayName("DisableChangingActions", NameResourceType = typeof(ContentStrings))]
        public bool DisableChangingActions { get; set; }

        [LocalizedDisplayName("FormScript", NameResourceType = typeof(ContentStrings))]
        public string FormScript { get; set; }

        public int[] NewVirtualFieldIds { get; set; }

        public int[] ForceVirtualFieldIds { get; set; }

        #endregion

        #region simple read-only

        public bool IsVirtual
        {
            get
            {
                return (VirtualType != 0);
            }
        }

        public string VirtualTypeString
        {
            get
            {
                return GetVirtualTypeString(VirtualType);

            }

        }

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
                    return String.Empty;
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
                    return NotificationStrings.RadioNone;
                default:
                    return String.Empty;
            }
        }

        public string GroupName
        {
            get
            {
                return (Group == null) ? String.Empty : Group.Name;
            }
        }

        public string SelfRelationFieldFilter
        {
            get
            {
                Field field = TreeField;
                return (field == null) ? String.Empty : field.RelationFilter;
            }
        }

        public override string EntityTypeCode
        {
            get
            {
                return IsVirtual ? Constants.EntityTypeCode.VirtualContent : Constants.EntityTypeCode.Content;
            }
        }

        public override int ParentEntityId
        {
            get
            {
                return SiteId;
            }
        }

        public override string CannotAddBecauseOfSecurityMessage
        {
            get
            {
                return ContentStrings.CannotAddBecauseOfSecurity;
            }
        }

        public override string CannotUpdateBecauseOfSecurityMessage
        {
            get
            {
                return ContentStrings.CannotUpdateBecauseOfSecurity;
            }
        }

        public override string PropertyIsNotUniqueMessage
        {
            get
            {
                return ContentStrings.NameNonUnique;
            }
        }

        public bool HasTreeField
        {
            get
            {
                return ContentRepository.HasContentTreeField(Id);
            }
        }

        public bool HasAnyNotification
        {
            get
            {
                return ContentRepository.HasAnyNotifications(Id);
            }
        }

        public bool HasAggregatedFields { get { return Fields.Where(f => f.Aggregated).Any(); } }

        public IEnumerable<Content> AggregatedContents { get { return aggregatedContents.Value; } }

        public Field TreeField
        {
            get
            {
                return treeField.Value;
            }
        }

        public Field VariationField
        {
            get
            {
                return variationField.Value;
            }
        }

        internal IEnumerable<Field> FieldsForCreateLike
        {
            get
            {
                HashSet<int> currentFieldIds = new HashSet<int>(Fields.Select(n => n.Id));
                return Fields
                    .Where(n => !n.BackRelationId.HasValue || currentFieldIds.Contains(n.BackRelationId.Value))
                    .Where(f => !f.IsClassifier)
                    .OrderBy(n => n.Order);
            }
        }

        #endregion

        #region references

        public IEnumerable<ContentConstraint> Constraints
        {
            get
            {
                if (_Constraints == null)
                {
                    if (this.IsNew)
                        _Constraints = Enumerable.Empty<ContentConstraint>();
                    else
                        _Constraints = ContentConstraintRepository.GetConstraintsByContentId(this.Id);
                }
                return _Constraints;
            }
        }

        public IEnumerable<Field> Fields
        {

            get
            {
                if (_Fields == null)
                {
                    if (this.IsNew)
                        _Fields = Enumerable.Empty<Field>();
                    else
                        _Fields = FieldRepository.GetFullList(this.Id);

                    foreach (Field current in _Fields)
                    {
                        current.Content = this;
                    }
                }
                return _Fields;
            }
        }

        /// <summary>
        /// Поля на которые могу ссылаться другие поля через O2M связь
        /// </summary>
        public IEnumerable<Field> RelateableFields
        {
            get
            {
                return Fields
                    .Where(f => f.IsRelateable)
                    .Select(f => f);
            }
        }


        public ContentGroup Group
        {
            get
            {
                return contentGroup.Value;
            }
        }

        public ContentWorkflowBind WorkflowBinding
        {
            get
            {
                if (_WorkflowBinding == null)
                {
                    LoadWorkflowBinding();
                }
                return _WorkflowBinding;
            }
            set
            {
                _WorkflowBinding = value;
            }
        }

        public Site Site
        {
            get
            {
                if (_Site == null)
                {
                    _Site = SiteRepository.GetById(SiteId);
                }
                return _Site;
            }

            set
            {
                _Site = value;
            }
        }

        public PathInfo RootVersionsLibrary
        {
            get
            {
                return PathInfo.GetSubPathInfo(ArticleVersion.RootFolder);
            }
        }

        public PathInfo RootContentsPathInfo
        {
            get
            {
                return Site.BasePathInfo.GetSubPathInfo("contents");
            }
        }

        public override PathInfo PathInfo
        {
            get
            {
                return RootContentsPathInfo.GetSubPathInfo(Id.ToString());
            }
        }

        public override EntityObject Parent
        {
            get
            {
                return Site;
            }
        }

        public Content ParentContent
        {
            get
            {
                return parentContent.Value;
            }
        }

        public IEnumerable<Content> ChildContents
        {
            get
            {
                return childContents.Value;
            }
        }

        #region Related to Virtual Content
        /// <summary>
        /// Виртуальные поля контента
        /// </summary>
        public IEnumerable<VirtualFieldNode> VirtualJoinFieldNodes
        {
            get
            {
                return virtualJoinFieldNodes.Value;
            }
            set
            {
                virtualJoinFieldNodes.Value = value;
            }
        }

        /// <summary>
        /// Дочерние виртуальные контенты
        /// </summary>
        public IEnumerable<Content> VirtualSubContents
        {
            get
            {
                return virtualSubContents.Value;
            }
        }

        /// <summary>
        /// Схема view виртуального контента типа User Query
        /// </summary>
        internal IEnumerable<UserQueryColumn> UserQueryContentViewSchema
        {
            get
            {
                return userQueryContentViewSchema.Value;
            }
        }

        /// <summary>
        /// ID базовых контентов для построения Union-контента
        /// </summary>
        [LocalizedDisplayName("UnionSourceContents", NameResourceType = typeof(ContentStrings))]
        public IEnumerable<int> UnionSourceContentIDs
        {
            get
            {
                return unionContentIDs.Value;
            }
            set
            {
                unionContentIDs.Value = value;
            }
        }

        /// <summary>
        /// Текст запроста для User Query контента
        /// </summary>
        [LocalizedDisplayName("UserQuery", NameResourceType = typeof(ContentStrings))]
        public string UserQuery { get; set; }

        /// <summary>
        /// Текст альтернативного запроста для united view из User Query контента
        /// </summary>
        [LocalizedDisplayName("UserQueryAlternative", NameResourceType = typeof(ContentStrings))]
        public string UserQueryAlternative { get; set; }

        #endregion

        #endregion

        #endregion

        #region methods

        #region public

        public void DoCustomBinding()
        {
            if (!MapAsClass)
            {
                NetName = null;
                NetPluralName = null;
                UseDefaultFiltration = true;
                AdditionalContextClassName = null;
            }
            if (CreateDefaultXamlValidation)
                XamlValidation = GenerateDefaultValidationXaml();
            else
                XamlValidation = String.IsNullOrWhiteSpace(XamlValidation) ? null : XamlValidation;
        }

        public override void Validate()
        {
            RulesException<Content> errors = new RulesException<Content>();

            base.Validate(errors);

            ValidateAggregated(errors);

            ValidateConditionalRequirements(errors);

            if (this.VirtualType != Constants.VirtualType.None)
            {
                ValidateVirtualContent(errors);
            }
            if (this.VirtualType == Constants.VirtualType.Join)
                ValidateJoinVirtualContent(errors);
            else if (this.VirtualType == Constants.VirtualType.Union)
                ValidateUnionVirtualContent(errors);
            else if (this.VirtualType == Constants.VirtualType.UserQuery)
                ValidateUserQueryVirtualContent(errors);

            if (!PageSize.IsInRange(MinPageSize, MaxPageSize))
                errors.ErrorFor(c => c.PageSize, String.Format(ContentStrings.ArticlesNumberPerPageNotInRange, MinPageSize, MaxPageSize));

            if (this.VirtualType == Constants.VirtualType.None)
                ValidateXaml(errors);

            if (!errors.IsEmpty)
                throw errors;
        }

        /// <summary>
        /// Проверка XAML
        /// </summary>
        /// <param name="errors"></param>
        private void ValidateXaml(RulesException<Content> errors)
        {
            if (!DisableXamlValidation && !CreateDefaultXamlValidation && !String.IsNullOrWhiteSpace(XamlValidation))
            {
                Dictionary<string, string> data = Fields.ToDictionary(f => f.FormName, f => "");
                // add system properties
                data[ContentItemIdPropertyName] = "";
                data[StatusTypeIdPropertyName] = "";
                try
                {
                    ValidationServices.TestValidator(data, XamlValidation, Site.XamlDictionaries);
                }
                catch (Exception exp)
                {
                    errors.ErrorFor(f => f.XamlValidation, String.Format("{0}: {1}", ContentStrings.XamlValidation, exp.Message));
                }
            }
        }

        /// <summary>
        /// Валидация контента содержащего агрегированные поля
        /// </summary>
        /// <param name="errors"></param>
        private void ValidateAggregated(RulesException<Content> errors)
        {
            // изменение некоторых свойств запрещено
            if (HasAggregatedFields && !IsNew)
            {
                Content dbContent = ContentRepository.GetById(Id);
                if (dbContent == null)
                    throw new Exception(String.Format(ContentStrings.ContentNotFound, Id));
                if (AutoArchive != dbContent.AutoArchive) // Архивировать при удалении
                    errors.ErrorFor(f => AutoArchive, ContentStrings.CannotChangeAutoArchive);
                if (WorkflowBinding.WorkflowId != dbContent.WorkflowBinding.WorkflowId) // Тип Workflow
                    errors.ErrorFor(f => WorkflowBinding.WorkflowId, ContentStrings.CannotChangeWorkflow);
                if (WorkflowBinding.IsAsync != dbContent.WorkflowBinding.IsAsync) // Расщеплять ли статьи по Workflow
                    errors.ErrorFor(f => WorkflowBinding.IsAsync, ContentStrings.CannotChangeIsAsync);
            }
        }

        /// <summary>
        /// Валидация при удалении
        /// </summary>
        /// <param name="violationMessages"></param>
        public void ValidateForRemove(IList<string> violationMessages)
        {
            // нельзя удалять если контент агрегированный и есть хотя бы одна статья
            if (HasAggregatedFields && ContentRepository.IsAnyArticle(Id))
                violationMessages.Add(String.Format(ContentStrings.ContentIsAggregated,
                    Environment.NewLine + String.Join(Environment.NewLine,
                        Fields.Where(f => f.Aggregated)
                        .Select(c => c.Name)
                        .ToArray()
                    )
                ));

            var contentIDComparer = new LambdaEqualityComparer<Content>((c1, c2) => c1.Id == c2.Id, c => c.Id);

            // Получить контенты у которых хотя бы одно поле ссылается на данные контент
            IEnumerable<Content> relatedO2MContents = ContentRepository.GetRelatedO2MContents(this);
            IEnumerable<Content> relatedM2MContents = ContentRepository.GetRelatedM2MContents(this);
            IEnumerable<Content> relatedContents = relatedM2MContents.Union(relatedO2MContents);
            if (relatedContents.Any())
                violationMessages.Add(String.Format(ContentStrings.RelatedContentsExist,
                    Environment.NewLine + String.Join(Environment.NewLine,
                        relatedContents
                        .Distinct(contentIDComparer)
                        .Select(c => c.Name)
                        .ToArray()
                    )
                ));
            // ---------------------

            // проверка на существование подчиненных виртуальных контентов
            IEnumerable<Content> inheritedContents = this.VirtualSubContents
                .Where(c => c.VirtualType == Constants.VirtualType.UserQuery) // UQ контенты
                .Concat(VirtualContentRepository.GetJoinRelatedContents(this)) // JOIN контенты
                .Concat(this.VirtualSubContents
                    .Where(c => c.VirtualType == Constants.VirtualType.Union && c.UnionSourceContentIDs.Count() < 2)
                ); // Проверка Union-Контентов (есть ли у union еще контенты)

            if (inheritedContents.Any())
                violationMessages.Add(String.Format(ContentStrings.VirtualSubContentsExist,
                    Environment.NewLine + String.Join(Environment.NewLine,
                        inheritedContents
                        .Distinct(contentIDComparer)
                        .Select(c => c.Name)
                        .ToArray()
                    )
                ));

            if (!IsContentChangingActionsAllowed)
                violationMessages.Add(ContentStrings.ContentChangingIsProhibited);
        }

        public void LoadWorkflowBinding()
        {
            _WorkflowBinding = WorkflowRepository.GetContentWorkflow(this);
        }

        public int GetMaxFieldsOrder()
        {
            return (Fields.Any()) ? Fields.OrderByDescending(f => f.Order).First().Order : 0;
        }

        public int GetMinFieldsOrder()
        {
            return (Fields.Any()) ? Fields.OrderBy(f => f.Order).First().Order : 0;
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
                    Content contentToCopy = ContentRepository.GetById(ParentContentId.Value);

                    contentToCopy.Name = Name;
                    contentToCopy.ParentContentId = ParentContentId.Value;
                    contentToCopy.Description = Description;

                    result = ContentRepository.Copy(contentToCopy, ForceId, ForceFieldIds, ForceLinkIds, true);
                }
                result.CreateContentFolders();
            }
            else
            {
                result = ContentRepository.Update(this);
                if (!result.UseVersionControl)
                {
                    Folder.ForceDelete(result.RootVersionsLibrary.Path);
                }
            }
            return result;

        }

        public IEnumerable<string> Die()
        {
            using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(Id))
            {

                List<string> violationMessages = new List<string>();
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
                .Select(n => new ArticleContextSearchBlockItem()
                {
                    ContentName = n.Content.Name,
                    ContentId = n.Content.Id,
                    FieldId = n.FieldId
                });
        }

        #endregion

        #region internal

        internal Article CreateArticle()
        {
            if (IsVirtual)
                throw new Exception(String.Format(ArticleStrings.AppendToVirtual, Id));

            return new Article(this);
        }


        internal Article CreateArticle(Dictionary<string, string> predefinedValues)
        {
            if (IsVirtual)
                throw new Exception(String.Format(ArticleStrings.AppendToVirtual, Id));

            return new Article(this, predefinedValues);
        }

        internal PathInfo GetVersionPathInfo(int versionId)
        {
            return RootVersionsLibrary.GetSubPathInfo((versionId == ArticleVersion.CurrentVersionId) ? "current" : versionId.ToString());
        }

        internal bool HasNotifications(string code)
        {
            foreach (string currentCode in code.Split(';'))
                if (ContentRepository.HasNotifications(Id, currentCode))
                    return true;
            return false;
        }

        internal void CreateContentFolders()
        {
            Directory.CreateDirectory(PathInfo.Path);
        }

        internal void DeleteContentFolders()
        {
            Folder.ForceDelete(PathInfo.Path);
        }

        internal void MutateNames()
        {
            string name = Name;
            int index = 0;
            do
            {
                index++;
                Name = MutateHelper.MutateString(name, index);
            }
            while (ContentRepository.NameExists(this));

            string netName = NetName;
            index = 0;
            do
            {
                index++;
                NetName = MutateHelper.MutateNetName(netName, index);
            }
            while (ContentRepository.NetNameExists(this));

            string netPluralName = NetPluralName;
            index = 0;
            do
            {
                index++;
                NetPluralName = MutateHelper.MutateNetName(netPluralName, index);
            }
            while (ContentRepository.NetPluralNameExists(this));

        }

        internal void DeleteFields()
        {
            foreach (Field field in Fields)
            {
                FieldRepository.Delete(field.Id);
            }
        }

        internal bool AssignedAsyncWorkflow
        {
            get
            {
                return WorkflowBinding.IsAssigned && WorkflowBinding.IsAsync;
            }
        }

        /// <summary>
        /// Проверяет можно ли для статей контента выполнять изменяющие операции
        /// </summary>
        /// <param name="content"></param>
        /// <param name="boundToExternal"></param>
        /// <returns></returns>
        internal bool IsArticleChangingActionsAllowed(bool? boundToExternal)
        {
            if (DisableChangingActions && (!boundToExternal.HasValue || !boundToExternal.Value))
                return false;
            else
                return true;
        }

        public bool IsContentChangingActionsAllowed
        {
            get
            {
                return !(DisableChangingActions && !QPContext.IsAdmin);
            }
        }

        #endregion

        #region private

        private void ValidateConditionalRequirements(RulesException<Content> errors)
        {
            if (MapAsClass)
            {
                if (String.IsNullOrEmpty(NetName))
                    errors.ErrorFor(n => n.NetName, ContentStrings.NameSingularNotEntered);
                else
                    if (ContentRepository.NetNameExists(this))
                    errors.ErrorFor(n => n.NetName, ContentStrings.NameSingularNonUnique);

                if (String.IsNullOrEmpty(NetPluralName))
                    errors.ErrorFor(n => n.NetPluralName, ContentStrings.NamePluralNotEntered);
                else
                    if (ContentRepository.NetPluralNameExists(this))
                    errors.ErrorFor(n => n.NetPluralName, ContentStrings.NamePluralNonUnique);

                if (!String.IsNullOrEmpty(AdditionalContextClassName))
                    if (ContentRepository.ContextClassNameExists(this))
                        errors.ErrorFor(n => n.AdditionalContextClassName, ContentStrings.AddContextClassNameNonUnique);
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
                    errors.ErrorFor(c => c.VirtualType, ContentStrings.VirtualTypeCantBeChanged);
            }
        }

        /// <summary>
        /// Валидация виртуального контента типа Join
        /// </summary>
        /// <param name="errors"></param>
        private void ValidateJoinVirtualContent(RulesException<Content> errors)
        {
            if (this.VirtualType == Constants.VirtualType.Join)
            {
                if (!JoinRootId.HasValue)
                    errors.ErrorFor(c => c.JoinRootId, ContentStrings.SourceContentNotEntered);

                if (IsNew && !VirtualJoinFieldNodes.Any())
                    errors.ErrorFor(c => c.VirtualJoinFieldNodes, ContentStrings.JoinFieldsNotEntered);
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
            if (this.VirtualType == Constants.VirtualType.Union)
            {
                if (!UnionSourceContentIDs.Any())
                    errors.ErrorFor(c => c.UnionSourceContentIDs, ContentStrings.UnionBaseContentNotEntered);
                else
                {
                    // проверить соответствие полей базовых контентов
                    var violations = new FieldsConflictValidator().UnionFieldsMatchCheck(UnionSourceContentIDs);
                    foreach (var violation in violations)
                    {
                        errors.ErrorFor(c => c.UnionSourceContentIDs, violation.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Валидация виртуального контента типа User Query
        /// </summary>
        /// <param name="errors"></param>
        internal void ValidateUserQueryVirtualContent(RulesException<Content> errors)
        {
            if (this.VirtualType == Constants.VirtualType.UserQuery)
            {
                if (String.IsNullOrWhiteSpace(UserQuery))
                {
                    errors.ErrorFor(c => c.UserQuery, ContentStrings.UserQueryNotEntered);
                }
                else
                {
                    // проверить sql запросы на возможность выполнения
                    bool areQueriesCorrect = true;
                    string userQueryErrorMsg;
                    if (!VirtualContentRepository.IsQueryQueryCorrect(UserQuery, out userQueryErrorMsg))
                    {
                        areQueriesCorrect = false;
                        errors.ErrorFor(c => c.UserQuery, String.Format(ContentStrings.UserQueryIsInvalid, userQueryErrorMsg));
                    }
                    if (!String.IsNullOrWhiteSpace(UserQueryAlternative) && !VirtualContentRepository.IsQueryQueryCorrect(UserQueryAlternative, out userQueryErrorMsg))
                    {
                        areQueriesCorrect = false;
                        errors.ErrorFor(c => c.UserQueryAlternative, String.Format(ContentStrings.UserQueryAltIsInvalid, userQueryErrorMsg));
                    }

                    if (areQueriesCorrect)
                    {
                        // проверить запрос на наличие обязательных полей
                        IEnumerable<UserQueryColumn> userQueryViewAllColumns = VirtualContentRepository.GetQuerySchema(UserQuery);

                        // проверить на наличие обязательных полей
                        IEnumerable<UserQueryColumn> userQueryViewUniqColumns = userQueryViewAllColumns.Distinct(UserQueryColumn.TableNameIgnoreEqualityComparer);
                        IEnumerable<UserQueryColumn> expectedMandatoryColumns = SystemMandatoryColumns.Except(userQueryViewUniqColumns, UserQueryColumn.TableNameIgnoreEqualityComparer);
                        if (expectedMandatoryColumns.Any())
                            errors.ErrorFor(c => c.UserQuery, String.Format(ContentStrings.NotAllMandatoryColumnsInUserQuery, String.Join(", ", expectedMandatoryColumns.Select(c => c.ColumnName + " (" + c.DbType + ")"))));

                        // проверить запрос на наличие недопустимых DB-типов колонок
                        IEnumerable<UserQueryColumn> invalidTypeColumns = userQueryViewAllColumns.Where(c => !Field.ValidFieldColumnDbTypeCollection.Contains(c.DbType));
                        if (invalidTypeColumns.Any())
                            errors.ErrorFor(c => c.UserQuery, String.Format(ContentStrings.InvalidColumnsInUserQuery, String.Join(", ", invalidTypeColumns.Select(c => c.ColumnName + " (" + c.DbType + ")"))));

                        // проверить что типы полей во view не изменены относительно соответствующих полей в реальных таблицах
                        Func<IEnumerable<UserQueryColumn>, IEnumerable<string>> getChangedTypeColumnMessage = (columns =>
                                columns
                                    .Where(c => !c.DbType.Equals(c.TableDbType, StringComparison.InvariantCultureIgnoreCase))
                                    .Select(c => String.Format(ContentStrings.UserQueryColumnTypeChanged, c.ColumnName, c.TableName, c.TableDbType, c.DbType))
                            );
                        var changedTypeColumnMessages = getChangedTypeColumnMessage(userQueryViewAllColumns);
                        if (changedTypeColumnMessages.Any())
                            errors.ErrorFor(c => c.UserQuery, ContentStrings.UserQuery + ": " + String.Join("; ", changedTypeColumnMessages));

                        // проверить что поля с одинаковыми именами имеют один и тот же тип в таблицах
                        Func<IEnumerable<UserQueryColumn>, IEnumerable<string>> getDiffTypeColumnMessages = (columns =>
                            columns
                                .GroupBy(c => c.ColumnName, StringComparer.InvariantCultureIgnoreCase)
                                .Where(g => g.Select(c => c.TableDbType)
                                    .Distinct(StringComparer.InvariantCultureIgnoreCase)
                                    .Count() > 1)
                                .Select(g => String.Format(ContentStrings.UserQuerySameNameColumsHaveDiffTypes, g.Key, String.Join(", ", g.Select(c => String.Format("[{0}]", c.TableName)))))
                            );
                        var diffTypeColumnMessage = getDiffTypeColumnMessages(userQueryViewAllColumns);
                        if (diffTypeColumnMessage.Any())
                            errors.ErrorFor(c => c.UserQuery, ContentStrings.UserQuery + ": " + String.Join("; ", diffTypeColumnMessage));


                        // если UserQueryAlternative задан
                        if (!String.IsNullOrWhiteSpace(UserQueryAlternative))
                        {
                            IEnumerable<UserQueryColumn> userAltQueryViewAllColumns = VirtualContentRepository.GetQuerySchema(UserQueryAlternative);

                            // проверить запросы на соответсвие полей
                            IEnumerable<UserQueryColumn> userAltQueryViewUniqColumns = userAltQueryViewAllColumns.Distinct(UserQueryColumn.TableNameIgnoreEqualityComparer);
                            if (userQueryViewUniqColumns.Count() != userAltQueryViewUniqColumns.Count() ||
                                userQueryViewUniqColumns.Except(userAltQueryViewUniqColumns, UserQueryColumn.TableNameIgnoreEqualityComparer).Any())
                                errors.ErrorFor(c => c.UserQueryAlternative, ContentStrings.UserQueryAndAltColumnsMismatch);

                            // проверить что поля с одинаковыми именами имеют один и тот же тип в таблицах
                            changedTypeColumnMessages = getChangedTypeColumnMessage(userAltQueryViewAllColumns);
                            if (changedTypeColumnMessages.Any())
                                errors.ErrorFor(c => c.UserQuery, ContentStrings.UserQueryAlternative + ": " + String.Join("; ", changedTypeColumnMessages));

                            // проверить что поля с одинаковыми именами имеют один и тот же тип в таблицах
                            diffTypeColumnMessage = getDiffTypeColumnMessages(userAltQueryViewAllColumns);
                            if (diffTypeColumnMessage.Any())
                                errors.ErrorFor(c => c.UserQueryAlternative, ContentStrings.UserQueryAlternative + ": " + String.Join("; ", diffTypeColumnMessage));
                        }
                    }

                }
            }

        }

        /// <summary>
        /// получить схему view UQ-контента (кроме системных полей)
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        internal IEnumerable<UserQueryColumn> GetUserQueryContentViewSchema()
        {
            if (VirtualType == Constants.VirtualType.UserQuery)
            {
                // получить схему view (кроме системных полей)
                string viewName = String.Format("content_{0}", Id);
                IEnumerable<UserQueryColumn> viewColumns = VirtualContentRepository.GetViewSchema(viewName)
                    .Where(c => !Content.SystemMandatoryColumns.Contains(c, UserQueryColumn.TableNameIgnoreEqualityComparer));

                // спец. обработка колонки content_id из union-контентов
                foreach (var c in viewColumns)
                {
                    if (c.ContentId.HasValue && Field.NameComparerPredicate("content_id", c.ColumnName))
                    {
                        Content cnt = ContentRepository.GetById(c.ContentId.Value);
                        if (cnt.VirtualType == (int)Constants.VirtualType.Union)
                            c.ResetContentBased();
                    }

                }

                return viewColumns;
            }
            else
                return Enumerable.Empty<UserQueryColumn>();
        }

        /// <summary>
        /// Создает XAML для валидации по умолчанию
        /// </summary>
        /// <returns></returns>
        private string GenerateDefaultValidationXaml()
        {
            List<PropertyDefinition> propDefs = new List<PropertyDefinition>
            {
                new PropertyDefinition
                {
                    PropertyName = ContentItemIdPropertyName,
                    Alias = "Id",
                    Description = "An indentifier of the entity.",
                    PropertyType = typeof(Int32)
                },
                new PropertyDefinition
                {
                    PropertyName = StatusTypeIdPropertyName,
                    Alias = "StatusTypeId",
                    Description = "StatusTypeId",
                    PropertyType = typeof(Int32)
                }

            };

            propDefs.AddRange(Fields.Select(f =>
                new PropertyDefinition
                {
                    PropertyName = f.FormName,
                    Alias = f.FormName,
                    PropertyType = f.XamlType,
                    Description = String.IsNullOrWhiteSpace(f.FriendlyName) ? f.Name : f.FriendlyName
                }
            ));
            return ValidationServices.GenerateXamlValidatorText(propDefs);
        }
        #endregion

        #endregion

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
