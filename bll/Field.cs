using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using QA.Validation.Xaml.ListTypes;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.ListItems;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ArticleRepositories;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.BLL.Services.VisualEditor;
using Quantumart.QP8.BLL.Validators;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QPublishing.Info;

namespace Quantumart.QP8.BLL
{
    public class Field : EntityObject
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IContentRepository _contentRepository;

        /// <summary>
        /// Траслирует SortExpression из Presentation в BLL
        /// </summary>
        /// <param name="sortExpression">SortExpression</param>
        /// <returns>SortExpression</returns>
        public new static string TranslateSortExpression(string sortExpression)
        {
            var result = EntityObject.TranslateSortExpression(sortExpression);
            var replaces = new Dictionary<string, string>
            {
                { "Type", "Type.Name" },
                { "Indexed", "IndexFlag" }
            };

            return TranslateHelper.TranslateSortExpression(result, replaces);
        }

        public static IEqualityComparer<Field> IdComparer
        {
            get { return new LambdaEqualityComparer<Field>((f1, f2) => f1.Id == f2.Id, f => f.Id); }
        }

        public static IEqualityComparer<Field> NameComparer
        {
            get { return new LambdaEqualityComparer<Field>(NameComparerPredicate, f => GetNameHashCode(f.Name)); }
        }

        public static bool NameComparerPredicate(Field f1, Field f2) => NameComparerPredicate(f1.Name, f2.Name);

        public static bool NameComparerPredicate(string n1, string n2)
        {
            if (n1 == null && n2 == null)
            {
                return true;
            }

            if (n1 == null || n2 == null)
            {
                return false;
            }

            return NameStringComparer.Equals(n1, n2);
        }

        public static IEqualityComparer<string> NameStringComparer => StringComparer.InvariantCultureIgnoreCase;

        public static int GetNameHashCode(string name) => name?.ToLowerInvariant().GetHashCode() ?? 0;

        public const int TextBoxRowsDefaultValue = 5;
        public const int VisualEditorHeightDefaultValue = 450;
        public const int DecimalPlacesDefaultValue = 2;
        public const int StringSizeDefaultValue = 255;
        public static readonly string Prefix = "field_";

        public static readonly ReadOnlyCollection<FieldExactTypes> UniqueFieldTypes = new ReadOnlyCollection<FieldExactTypes>(new List<FieldExactTypes>
        {
            FieldExactTypes.String,
            FieldExactTypes.Numeric,
            FieldExactTypes.Boolean,
            FieldExactTypes.File,
            FieldExactTypes.Image,
            FieldExactTypes.O2MRelation,
            FieldExactTypes.StringEnum,
            FieldExactTypes.Classifier
        });

        public static readonly string DefaultRelationFilter = "c.archive = 0";
        public static readonly string ArchiveFilter = "c.archive = 1";

        /// <summary>
        /// Разрешенные DataBase типы колонок полей
        /// </summary>
        public static readonly ReadOnlyCollection<string> ValidFieldColumnDbTypeCollection = new ReadOnlyCollection<string>(new List<string>
        {
            ValidFieldColumnDbTypes.Numeric,
            ValidFieldColumnDbTypes.Int,
            ValidFieldColumnDbTypes.BigInt,
            ValidFieldColumnDbTypes.Nvarchar,
            ValidFieldColumnDbTypes.Datetime,
            ValidFieldColumnDbTypes.Ntext,
            ValidFieldColumnDbTypes.SmallInt,
            ValidFieldColumnDbTypes.TinyInt,
            ValidFieldColumnDbTypes.Bit
        });

        public static readonly ReadOnlyCollection<string> PgValidFieldColumnDbTypeCollection = new ReadOnlyCollection<string>(new List<string>
        {
            ValidFieldColumnDbTypes.Numeric,
            ValidFieldColumnDbTypes.Int,
            ValidFieldColumnDbTypes.BigInt,
            ValidFieldColumnDbTypes.CharVarying,
            ValidFieldColumnDbTypes.TimeStampWithoutTimeZone,
            ValidFieldColumnDbTypes.Text,
            ValidFieldColumnDbTypes.SmallInt,
            ValidFieldColumnDbTypes.TinyInt,
            ValidFieldColumnDbTypes.Bit
        });

        public Field()
            : this(new FieldRepository(), new ContentRepository())
        {
            // TODO: REMOVE AND FIX AUTOMAPPER
        }

        public Field(IFieldRepository fieldRepository, IContentRepository contentRepository)
        {
            _fieldRepository = fieldRepository;
            _contentRepository = contentRepository;

            RebuildVirtualContents = true;
            FieldTitleCount = 1;
            StringEnumItems = Enumerable.Empty<StringEnumItem>();
        }

        public static Field Create(Content content, IFieldRepository fieldRepository, IContentRepository contentRepository)
        {
            var result = new Field(fieldRepository, contentRepository) { Content = content, ContentId = content.Id };
            result.Init();
            return result;
        }

        private InitPropertyValue<Field> _relation;

        private InitPropertyValue<Field> _backRelation;

        private InitPropertyValue<VisualEditFieldParams> _visualEditFieldParams;

        private InitPropertyValue<IEnumerable<ExternalCss>> _externalCssItems;

        private InitPropertyValue<Field> _o2MBackwardField;

        private InitPropertyValue<Field> _m2MBackwardField;

        private InitPropertyValue<ContentLink> _contentLink;

        private InitPropertyValue<ContentConstraint> _constraint;

        private InitPropertyValue<DynamicImage> _dynamicImage;

        private InitPropertyValue<VisualEditorConfig> _visualEditor;

        private InitPropertyValue<bool> _hasAnyAggregators;

        private InitPropertyValue<bool> _isUnique;

        private InitPropertyValue<int?> _relateToContentId;

        private InitPropertyValue<FieldExactTypes> _exactType;

        private InitPropertyValue<PathInfo> _pathInfo;

        private InitPropertyValue<Field> _parentField;

        private InitPropertyValue<IEnumerable<Field>> _childFields;

        private InitPropertyValue<int[]> _activeVeCommandIds;

        private InitPropertyValue<int[]> _activeVeStyleIds;

        private InitPropertyValue<int[]> _defaultArticleIds;

        [LocalizedDisplayName("Name", NameResourceType = typeof(EntityObjectStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "NameMaxLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        [RequiredValidator(MessageTemplateResourceName = "NameNotEntered", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        [FormatValidator(RegularExpressions.InvalidFieldName, Negated = true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        public override string Name { get; set; }

        [LocalizedDisplayName("Description", NameResourceType = typeof(EntityObjectStrings))]
        [MaxLengthValidator(512, MessageTemplateResourceName = "DescriptionMaxLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
        public override string Description { get; set; }

        public override string EntityTypeCode => Constants.EntityTypeCode.Field;

        public override bool IsReadOnly => ReadOnly;

        public override PathInfo PathInfo => _pathInfo.Value;

        public override int ParentEntityId => ContentId;

        public int? OrderFieldId { get; set; }

        [LocalizedDisplayName("TreeSortingField", NameResourceType = typeof(FieldStrings))]
        public int? TreeOrderFieldId { get; set; }

        [LocalizedDisplayName("ListSortingField", NameResourceType = typeof(FieldStrings))]
        public int? ListOrderFieldId { get; set; }

        public int FieldTitleCount { get; set; }

        [LocalizedDisplayName("TreeFieldTitleCount", NameResourceType = typeof(FieldStrings))]
        public int TreeFieldTitleCount { get; set; }

        [LocalizedDisplayName("ListFieldTitleCount", NameResourceType = typeof(FieldStrings))]
        public int ListFieldTitleCount { get; set; }

        [LocalizedDisplayName("ListFieldTitleCount", NameResourceType = typeof(FieldStrings))]
        public int ListO2MFieldTitleCount { get; set; }

        public bool IncludeRelationsInTitle { get; set; }

        [LocalizedDisplayName("ListIncludeRelationsInTitle", NameResourceType = typeof(FieldStrings))]
        public bool ListO2MIncludeRelationsInTitle { get; set; }

        [LocalizedDisplayName("ListIncludeRelationsInTitle", NameResourceType = typeof(FieldStrings))]
        public bool ListIncludeRelationsInTitle { get; set; }

        [LocalizedDisplayName("TreeIncludeRelationsInTitle", NameResourceType = typeof(FieldStrings))]
        public bool TreeIncludeRelationsInTitle { get; set; }

        public bool OrderByTitle { get; set; }

        [LocalizedDisplayName("TreeOrderByTitle", NameResourceType = typeof(FieldStrings))]
        public bool TreeOrderByTitle { get; set; }

        [LocalizedDisplayName("ListOrderByTitle", NameResourceType = typeof(FieldStrings))]
        public bool ListOrderByTitle { get; set; }

        public int ContentId { get; set; }

        public int ForceBackwardId { get; set; }

        public int[] ForceChildFieldIds { get; set; }

        public int[] ResultChildFieldIds { get; set; }

        public int[] ForceChildLinkIds { get; set; }

        public int[] ResultChildLinkIds { get; set; }

        public int[] ForceVirtualFieldIds { get; set; }

        public int[] NewVirtualFieldIds { get; set; }

        public int[] ActiveVeCommandIds
        {
            get => _activeVeCommandIds.Value;
            set => _activeVeCommandIds.Value = value;
        }

        public int[] ActiveVeStyleIds
        {
            get => _activeVeStyleIds.Value;
            set => _activeVeStyleIds.Value = value;
        }

        public string FormatMask { get; set; }

        [LocalizedDisplayName("InputMask", NameResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "InputMaskMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        public string InputMask { get; set; }

        public string DefaultValue { get; set; }

        public int TypeId { get; set; }

        public FieldType Type { get; set; }

        [LocalizedDisplayName("RelationId", NameResourceType = typeof(FieldStrings))]
        public int? RelationId { get; set; }

        [LocalizedDisplayName("BackRelationId", NameResourceType = typeof(FieldStrings))]
        public int? BackRelationId { get; set; }

        [LocalizedDisplayName("Indexed", NameResourceType = typeof(FieldStrings))]
        public bool Indexed { get; set; }

        [LocalizedDisplayName("Order", NameResourceType = typeof(FieldStrings))]
        public int Order { get; set; }

        [LocalizedDisplayName("Required", NameResourceType = typeof(FieldStrings))]
        public bool Required { get; set; }

        [LocalizedDisplayName("RelationCondition", NameResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "RelationConditionMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        public string RelationCondition { get; set; }

        [LocalizedDisplayName("ViewInList", NameResourceType = typeof(FieldStrings))]
        public bool ViewInList { get; set; }

        [LocalizedDisplayName("RenameMatched", NameResourceType = typeof(FieldStrings))]
        public bool RenameMatched { get; set; }

        [LocalizedDisplayName("PEnterMode", NameResourceType = typeof(FieldStrings))]
        public bool PEnterMode
        {
            get => VisualEditFieldParams.PEnterMode;
            set => VisualEditFieldParams.PEnterMode = value;
        }

        [LocalizedDisplayName("UseEnglishQuotes", NameResourceType = typeof(FieldStrings))]
        public bool UseEnglishQuotes
        {
            get => VisualEditFieldParams.UseEnglishQuotes;
            set => VisualEditFieldParams.UseEnglishQuotes = value;
        }

        [LocalizedDisplayName("DisableListAutoWrap", NameResourceType = typeof(FieldStrings))]
        public bool DisableListAutoWrap
        {
            get => VisualEditFieldParams.DisableListAutoWrap;
            set => VisualEditFieldParams.DisableListAutoWrap = value;
        }

        [LocalizedDisplayName("DisableVersionControl", NameResourceType = typeof(FieldStrings))]
        public bool DisableVersionControl { get; set; }

        [LocalizedDisplayName("BaseImage", NameResourceType = typeof(FieldStrings))]
        public int? BaseImageId { get; set; }

        [LocalizedDisplayName("ReadOnly", NameResourceType = typeof(FieldStrings))]
        public bool ReadOnly { get; set; }

        public string DefaultBlobValue { get; set; }

        public int? LinkId { get; set; }

        [LocalizedDisplayName("UseSiteLibrary", NameResourceType = typeof(FieldStrings))]
        public bool UseSiteLibrary { get; set; }

        [LocalizedDisplayName("SubFolder", NameResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "SubFolderMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        [FormatValidator(RegularExpressions.RelativeWindowsFolderPath, MessageTemplateResourceName = "SubFolderInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
        [Example(@"\subfolder1\subfolder2")]
        public string SubFolder { get; set; }

        public int? PersistentId { get; set; }

        [MaxLengthValidator(300, MessageTemplateResourceName = "FriendlyNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        [LocalizedDisplayName("FriendlyName", NameResourceType = typeof(FieldStrings))]
        public string FriendlyName { get; set; }

        [LocalizedDisplayName("DocType", NameResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "DocTypeMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        public string DocType { get; set; }

        [LocalizedDisplayName("FullPage", NameResourceType = typeof(FieldStrings))]
        public bool FullPage { get; set; }

        [LocalizedDisplayName("OnScreen", NameResourceType = typeof(FieldStrings))]
        public bool OnScreen { get; set; }

        [LocalizedDisplayName("MapAsProperty", NameResourceType = typeof(FieldStrings))]
        public bool MapAsProperty { get; set; }

        [LocalizedDisplayName("LinqPropertyName", NameResourceType = typeof(FieldStrings))]
        [FormatValidator(RegularExpressions.NetName, MessageTemplateResourceName = "LinqPropertyNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "LinqPropertyNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        public string LinqPropertyName { get; set; }

        [LocalizedDisplayName("LinqBackPropertyName", NameResourceType = typeof(FieldStrings))]
        [FormatValidator(RegularExpressions.NetName, MessageTemplateResourceName = "LinqBackPropertyNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "LinqBackPropertyNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        public string LinqBackPropertyName { get; set; }

        [LocalizedDisplayName("IsInteger", NameResourceType = typeof(FieldStrings))]
        public bool IsInteger { get; set; }

        [LocalizedDisplayName("IsLong", NameResourceType = typeof(FieldStrings))]
        public bool IsLong { get; set; }

        [LocalizedDisplayName("IsDecimal", NameResourceType = typeof(FieldStrings))]
        public bool IsDecimal { get; set; }

        public int Size { get; set; }

        public int? JoinId { get; set; }

        [LocalizedDisplayName("AutoExpand", NameResourceType = typeof(FieldStrings))]
        public bool AutoExpand { get; set; }

        [LocalizedDisplayName("AutoLoad", NameResourceType = typeof(FieldStrings))]
        public bool AutoLoad { get; set; }

        [LocalizedDisplayName("RebuildVirtualContents", NameResourceType = typeof(FieldStrings))]
        public bool RebuildVirtualContents { get; set; }

        internal bool HasAnyAggregators => _hasAnyAggregators.Value;

        [LocalizedDisplayName("EnumValues", NameResourceType = typeof(FieldStrings))]
        public IEnumerable<StringEnumItem> StringEnumItems { get; set; }

        [LocalizedDisplayName("ShowAsRadioButtons", NameResourceType = typeof(FieldStrings))]
        public bool ShowAsRadioButtons { get; set; }

        [LocalizedDisplayName("UseForDefaultFiltration", NameResourceType = typeof(FieldStrings))]
        public bool UseForDefaultFiltration { get; set; }

        [LocalizedDisplayName("UseInChildContentFilter", NameResourceType = typeof(FieldStrings))]
        public bool UseInChildContentFilter { get; set; }

        public Content Content { get; set; }

        public Field Relation => _relation.Value;

        public Field BackRelation
        {
            get => _backRelation.Value;
            set => _backRelation.Value = value;
        }

        public string DisplayName => !string.IsNullOrEmpty(FriendlyName) ? FriendlyName : Name;

        public string FormName => Prefix + Id;

        public string FormCheckboxName => FormName + "_checkbox";

        public static int ParseFormName(string formName) => int.TryParse(formName.Replace(Prefix, string.Empty), out var value) ? value : 0;

        public string ParamName => "@" + FormName;

        public string Default => IsBlob ? DefaultBlobValue : DefaultValue;

        public bool IsBlob => TypeId == FieldTypeCodes.Textbox || TypeId == FieldTypeCodes.VisualEdit;

        public bool IsDateTime => TypeId == FieldTypeCodes.DateTime || TypeId == FieldTypeCodes.Date || TypeId == FieldTypeCodes.Time;

        public bool ReplaceUrls => ExactType == FieldExactTypes.String || IsBlob;

        public RelationType RelationType
        {
            get
            {
                switch (ExactType)
                {
                    case FieldExactTypes.M2MRelation:
                        return RelationType.ManyToMany;
                    case FieldExactTypes.M2ORelation:
                        return RelationType.ManyToOne;
                    case FieldExactTypes.O2MRelation:
                        return RelationType.OneToMany;
                    default:
                        return RelationType.None;
                }
            }
        }

        public string RelationFilter => SqlFilterComposer.Compose(UseRelationCondition ? "(" + RelationCondition + ")" : "", DefaultRelationFilter);

        [LocalizedDisplayName("FieldType", NameResourceType = typeof(FieldStrings))]
        public FieldExactTypes ExactType
        {
            get => _exactType.Value;
            set => _exactType.Value = value;
        }

        public Type XamlType
        {
            get
            {
                switch (ExactType)
                {
                    case FieldExactTypes.Undefined:
                        return null;
                    case FieldExactTypes.String:
                    case FieldExactTypes.File:
                    case FieldExactTypes.Image:
                    case FieldExactTypes.Textbox:
                    case FieldExactTypes.VisualEdit:
                    case FieldExactTypes.DynamicImage:
                    case FieldExactTypes.StringEnum:
                        return typeof(string);
                    case FieldExactTypes.Numeric:
                        return IsInteger ? typeof(int) : typeof(double);
                    case FieldExactTypes.Boolean:
                        return typeof(bool);
                    case FieldExactTypes.Date:
                    case FieldExactTypes.Time:
                    case FieldExactTypes.DateTime:
                        return typeof(DateTime);
                    case FieldExactTypes.M2ORelation:
                    case FieldExactTypes.M2MRelation:
                        return typeof(ListOfInt);
                    case FieldExactTypes.O2MRelation:
                        return typeof(int);
                    case FieldExactTypes.Classifier:
                        return typeof(int);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        [LocalizedDisplayName("IsUnique", NameResourceType = typeof(FieldStrings))]
        public bool IsUnique
        {
            get => _isUnique.Value;
            set => _isUnique.Value = value;
        }

        [LocalizedDisplayName("RelateTo", NameResourceType = typeof(FieldStrings))]
        public int? RelateToContentId
        {
            get => _relateToContentId.Value;
            set => _relateToContentId.Value = value;
        }

        public Content RelatedToContent => RelateToContentId.HasValue ? _contentRepository.GetById(RelateToContentId.Value) : null;

        [LocalizedDisplayName("TextBoxRows", NameResourceType = typeof(FieldStrings))]
        public int TextBoxRows { get; set; }

        [LocalizedDisplayName("HighlightType", NameResourceType = typeof(FieldStrings))]
        public string HighlightType { get; set; }

        [LocalizedDisplayName("MaxDataListItemCount", NameResourceType = typeof(FieldStrings))]
        [RangeValueValidator(0, 100, MessageTemplateResourceName = "MaxDataItemCountNotInRange", MessageTemplateResourceType = typeof(FieldStrings))]
        public int MaxDataListItemCount { get; set; }

        [LocalizedDisplayName("VisualEditorHeight", NameResourceType = typeof(FieldStrings))]
        public int VisualEditorHeight { get; set; }

        [LocalizedDisplayName("DecimalPlaces", NameResourceType = typeof(FieldStrings))]
        public int DecimalPlaces { get; set; }

        [LocalizedDisplayName("StringSize", NameResourceType = typeof(FieldStrings))]
        public int StringSize { get; set; }

        [LocalizedDisplayName("UseInputMask", NameResourceType = typeof(FieldStrings))]
        public bool UseInputMask { get; set; }

        public DynamicImage DynamicImage
        {
            get => _dynamicImage.Value;
            set => _dynamicImage.Value = value;
        }

        public VisualEditorConfig VisualEditor => _visualEditor.Value;

        public bool UseVersionControl => !DisableVersionControl && Content.MaxNumOfStoredVersions != 0;

        public Type ClrType
        {
            get
            {
                var clrType = typeof(object);

                if (Name == FieldTypeName.String)
                {
                    clrType = typeof(string);
                }
                else if (Name == FieldTypeName.Numeric)
                {
                    clrType = Required ? typeof(decimal) : typeof(decimal?);
                }
                else if (Name == FieldTypeName.Boolean)
                {
                    clrType = Required ? typeof(bool) : typeof(bool?);
                }
                else if (Name == FieldTypeName.Date)
                {
                    clrType = typeof(DateTime);
                }
                else if (Name == FieldTypeName.Time)
                {
                    clrType = typeof(DateTime);
                }
                else if (Name == FieldTypeName.DateTime)
                {
                    clrType = typeof(DateTime);
                }
                else if (Name == FieldTypeName.File)
                {
                    clrType = typeof(string);
                }
                else if (Name == FieldTypeName.Image)
                {
                    clrType = typeof(string);
                }
                else if (Name == FieldTypeName.Textbox)
                {
                    clrType = typeof(string);
                }
                else if (Name == FieldTypeName.VisualEdit)
                {
                    clrType = typeof(string);
                }
                else if (Name == FieldTypeName.Relation || Name == FieldTypeName.M2ORelation)
                {
                    clrType = Required ? typeof(decimal) : typeof(decimal?);
                }
                else if (Name == FieldTypeName.DynamicImage)
                {
                    clrType = typeof(string);
                }

                return clrType;
            }
        }

        public int LibraryEntityId => UseSiteLibrary ? Content.SiteId : ContentId;

        public int LibraryParentEntityId => UseSiteLibrary ? 0 : Content.SiteId;

        public ContentConstraint Constraint
        {
            get => _constraint.Value;
            set => _constraint.Value = value;
        }

        public ContentLink ContentLink
        {
            get => _contentLink.Value;
            set => _contentLink.Value = value;
        }

        /// <summary>
        /// Обратное поле
        /// </summary>
        public Field M2MBackwardField
        {
            get => _m2MBackwardField.Value;
            set => _m2MBackwardField.Value = value;
        }

        /// <summary>
        /// Обратное поле
        /// </summary>
        public Field O2MBackwardField
        {
            get => _o2MBackwardField.Value;
            set => _o2MBackwardField.Value = value;
        }

        /// <summary>
        /// Существует ли для данного поля обратное поле ?
        /// </summary>
        public bool IsBackwardFieldExists => BackwardField != null && !BackwardField.IsNew;

        public Field BackwardField => M2MBackwardField ?? O2MBackwardField;

        [LocalizedDisplayName("BackwardFieldId", NameResourceType = typeof(FieldStrings))]
        public int? BackwardRelateToFieldId => BackwardField.Id;

        /// <summary>
        /// Поля которые ссылаются на данное поле связью O2M
        /// </summary>
        public IEnumerable<Field> RelatedO2MFields => IsNew ? Enumerable.Empty<Field>() : _fieldRepository.GetRelatedO2MFields(Id);

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public string StringDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public double? NumericDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public int? ClassifierDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public bool BooleanDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public DateTime? DateDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public TimeSpan? TimeDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public DateTime? DateTimeDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public string TextBoxDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public string VisualEditDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public string FileDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public string O2MDefaultValue { get; set; }

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public string M2MDefaultValue { get; set; }

        [LocalizedDisplayName("RootElementClass", NameResourceType = typeof(VisualEditorStrings))]
        public string RootElementClass
        {
            get => VisualEditFieldParams.RootElementClass;
            set => VisualEditFieldParams.RootElementClass = value;
        }

        public string ExternalCss
        {
            get => VisualEditFieldParams.ExternalCss;
            set => VisualEditFieldParams.ExternalCss = value;
        }

        [LocalizedDisplayName("ExternalCss", NameResourceType = typeof(VisualEditorStrings))]
        public IEnumerable<ExternalCss> ExternalCssItems
        {
            get => _externalCssItems.Value;
            set => _externalCssItems.Value = value;
        }

        public string O2MDefaultValueName
        {
            get
            {
                var result = string.Empty;
                if (Relation != null && int.TryParse(O2MDefaultValue, out var parsedArticleId))
                {
                    result = ArticleRepository.GetFieldValue(parsedArticleId, Relation.ContentId, Relation.Name);
                }

                return result;
            }
        }

        public string M2MDefaultValueName
        {
            get
            {
                var result = string.Empty;
                if (Relation != null && int.TryParse(M2MDefaultValue, out var parsedArticleId))
                {
                    result = ArticleRepository.GetFieldValue(parsedArticleId, Relation.ContentId, Relation.Name);
                }

                return result;
            }
        }

        [LocalizedDisplayName("BackwardFieldName", NameResourceType = typeof(FieldStrings))]
        [FormatValidator(RegularExpressions.InvalidEntityName, Negated = true, MessageTemplateResourceName = "BackwardFieldNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "BackwardFieldNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        public string NewM2MBackwardFieldName { get; set; }

        [LocalizedDisplayName("BackwardFieldName", NameResourceType = typeof(FieldStrings))]
        [FormatValidator(RegularExpressions.InvalidEntityName, Negated = true, MessageTemplateResourceName = "BackwardFieldNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
        [MaxLengthValidator(255, MessageTemplateResourceName = "BackwardFieldNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
        public string NewO2MBackwardFieldName { get; set; }

        [LocalizedDisplayName("UseRelationCondition", NameResourceType = typeof(FieldStrings))]
        public bool UseRelationCondition { get; set; }

        [LocalizedDisplayName("UseForTree", NameResourceType = typeof(FieldStrings))]
        public bool UseForTree { get; set; }

        [LocalizedDisplayName("AutoCheckChildren", NameResourceType = typeof(FieldStrings))]
        public bool AutoCheckChildren { get; set; }

        [LocalizedDisplayName("UseRelationSecurity", NameResourceType = typeof(FieldStrings))]
        public bool UseRelationSecurity { get; set; }

        [LocalizedDisplayName("CopyPermissionsToChildren", NameResourceType = typeof(FieldStrings))]
        public bool CopyPermissionsToChildren { get; set; }

        /// <summary>
        /// Имя поля полученное из БД
        /// не меняеться на основе данных формы
        /// </summary>
        public string StoredName { get; set; }

        /// <summary>
        /// Изменил ли пользователь имя поля
        /// </summary>
        public bool IsNameChanged
        {
            get
            {
                if (Name == null && StoredName == null)
                {
                    return true;
                }

                if (Name == null && StoredName != null || Name != null && StoredName == null)
                {
                    return false;
                }

                if (StoredName == null)
                {
                    throw new ArgumentNullException();
                }

                return !StoredName.Equals(Name, StringComparison.InvariantCultureIgnoreCase); // игнорируем регист букв!
            }
        }

        internal VisualEditFieldParams VisualEditFieldParams => _visualEditFieldParams.Value;

        /// <summary>
        /// Могут ли поля O2M ссылаться на это поле
        /// </summary>
        internal bool IsRelateable => ExactType != FieldExactTypes.M2MRelation && ExactType != FieldExactTypes.M2ORelation;

        /// <summary>
        /// Можно ли изменять поле для существующей статьи
        /// </summary>
        [LocalizedDisplayName("Changeable", NameResourceType = typeof(FieldStrings))]
        public bool Changeable { get; set; }

        [LocalizedDisplayName("UseTypeSecurity", NameResourceType = typeof(FieldStrings))]
        public bool UseTypeSecurity { get; set; }

        /// <summary>
        /// Являеться ли поле агреггатором
        /// </summary>
        [LocalizedDisplayName("Aggregated", NameResourceType = typeof(FieldStrings))]
        public bool Aggregated { get; set; }

        /// <summary>
        /// ID cвязанного классификатора если поле является агреггатором
        /// </summary>
        [LocalizedDisplayName("ClassifierId", NameResourceType = typeof(FieldStrings))]
        public int? ClassifierId { get; set; }

        /// <summary>
        /// Являеться ли поле классификатором
        /// </summary>
        public bool IsClassifier { get; set; }

        [LocalizedDisplayName("ParentField", NameResourceType = typeof(FieldStrings))]
        public int? ParentFieldId { get; set; }

        [LocalizedDisplayName("Override", NameResourceType = typeof(FieldStrings))]
        public bool Override { get; set; }

        [LocalizedDisplayName("Hide", NameResourceType = typeof(FieldStrings))]
        public bool Hide { get; set; }

        [LocalizedDisplayName("UseForContext", NameResourceType = typeof(FieldStrings))]
        public bool UseForContext { get; set; }

        [LocalizedDisplayName("OptimizeForHierarchy", NameResourceType = typeof(FieldStrings))]
        public bool OptimizeForHierarchy { get; set; }

        [LocalizedDisplayName("IsLocalization", NameResourceType = typeof(FieldStrings))]
        public bool IsLocalization { get; set; }

        [LocalizedDisplayName("UseSeparateReverseViews", NameResourceType = typeof(FieldStrings))]
        public bool UseSeparateReverseViews { get; set; }

        [LocalizedDisplayName("UseForVariations", NameResourceType = typeof(FieldStrings))]
        public bool UseForVariations { get; set; }

        public IEnumerable<Field> ChildFields => _childFields.Value;

        public Field ParentField
        {
            get => _parentField.Value;

            set => _parentField.Value = value;
        }

        public FieldTypeInfo TypeInfo { get; private set; }

        private ContentLink GetContentLinkForInit()
        {
            if (ExactType != FieldExactTypes.M2MRelation || !LinkId.HasValue)
            {
                return new ContentLink { LContentId = ContentId };
            }

            return _contentRepository.GetContentLinkById(LinkId.Value);
        }

        private ContentConstraint GetConstraintForInit() => IsNew ? null : ContentConstraintRepository.GetConstraintByFieldId(Id);

        private DynamicImage GetDynamicImageForInit() => !IsNew && TypeId == FieldTypeCodes.DynamicImage ? DynamicImage.Load(this) : null;

        private Field GetRelationForInit() => RelationId.HasValue ? _fieldRepository.GetById(RelationId.Value) : null;

        private Field GetBackRelationForInit() => BackRelationId.HasValue ? _fieldRepository.GetById(BackRelationId.Value) : null;

        private bool GetIsUniqueForInit() => UniqueFieldTypes.Contains(ExactType) && Constraint != null;

        private bool GetIsUniqueForSet(bool value) => value && UniqueFieldTypes.Contains(ExactType);

        private int? GetRelateToForInit()
        {
            int? result = null;
            if (!IsNew)
            {
                if (ExactType == FieldExactTypes.O2MRelation)
                {
                    result = Relation?.ContentId;
                }
                else if (ExactType == FieldExactTypes.M2ORelation)
                {
                    result = BackRelation?.ContentId;
                }
                else if (ExactType == FieldExactTypes.M2MRelation && !ContentLink.IsNew)
                {
                    if (IsBackwardFieldExists)
                    {
                        if (ContentLink.LContentId == ContentId && ContentLink.RContentId == ContentId)
                        {
                            throw new ApplicationException("Обратное поле создано для того же контента.");
                        }
                    }

                    var field = this;
                    if (Content.IsVirtual)
                    {
                        var baseFieldId = VirtualFieldRepository.GetRealBaseFieldIds(Id).First();
                        field = _fieldRepository.GetById(baseFieldId);
                    }

                    if (field.ContentLink.LContentId == field.ContentId)
                    {
                        result = field.ContentLink.RContentId;
                    }
                    else if (field.ContentLink.RContentId == field.ContentId)
                    {
                        result = field.ContentLink.LContentId;
                    }
                }
            }

            return result;
        }

        private Field GetM2MBackwardForInit()
        {
            // Обратное поле может существовать только у невиртуальных контентов
            if (Content.VirtualType == VirtualType.None)
            {
                var checkedContentId = 0;

                if (IsNew || ContentLink == null || ContentLink.IsNew || LinkId == null)
                {
                    return null;
                }

                if (ContentLink.LContentId == ContentLink.RContentId && ContentLink.LContentId == ContentId)
                {
                    // поле связано со своим контентом
                    return null;
                }

                if (ContentLink.LContentId == ContentId)
                {
                    checkedContentId = ContentLink.RContentId;
                }
                else if (ContentLink.RContentId == ContentId)
                {
                    checkedContentId = ContentLink.LContentId;
                }

                var rContent = _contentRepository.GetById(checkedContentId);
                return rContent.Fields.SingleOrDefault(f => f.LinkId == LinkId && f.Id != Id);
            }

            return null;
        }

        private Field GetO2MBackwardForInit() => _fieldRepository.GetByBackRelationId(Id);

        private FieldExactTypes GetExactTypeForInit()
        {
            FieldExactTypes result;
            if (TypeId == FieldTypeCodes.Relation)
            {
                result = LinkId.HasValue ? FieldExactTypes.M2MRelation : FieldExactTypes.O2MRelation;
            }
            else if (TypeId == FieldTypeCodes.Numeric && IsClassifier)
            {
                result = FieldExactTypes.Classifier;
            }
            else if (TypeId == FieldTypeCodes.String && StringEnumItems.Any())
            {
                result = FieldExactTypes.StringEnum;
            }
            else
            {
                result = (FieldExactTypes)TypeId;
            }

            TypeInfo = new FieldTypeInfo(result, LinkId ?? BackRelationId);
            return result;
        }

        private FieldExactTypes GetExactTypeForSet(FieldExactTypes value)
        {
            if (value == FieldExactTypes.O2MRelation || value == FieldExactTypes.M2MRelation)
            {
                TypeId = FieldTypeCodes.Relation;
            }
            else if (value == FieldExactTypes.Classifier)
            {
                TypeId = FieldTypeCodes.Numeric;
            }
            else if (value == FieldExactTypes.StringEnum)
            {
                TypeId = FieldTypeCodes.String;
            }
            else
            {
                TypeId = (int)value;
            }

            TypeInfo = new FieldTypeInfo(value, LinkId ?? BackRelationId);
            return value;
        }

        private Field GetParentFieldForInit() => ParentFieldId.HasValue ? _fieldRepository.GetById(ParentFieldId.Value) : null;

        private IEnumerable<Field> GetChildFieldsForInit() => _fieldRepository.GetChildList(Id);

        private bool GetHasAnyAggregatorsForInit() => _fieldRepository.HasAnyAggregators(Id);

        private IEnumerable<ExternalCss> GetExternalCssItemsForInit() => ExternalCssHelper.GenerateExternalCss(ExternalCss);

        private PathInfo GetPathInfoForInit()
        {
            var pathInfo = UseSiteLibrary ? Content.Site.PathInfo : Content.PathInfo;
            if (!string.IsNullOrEmpty(SubFolder))
            {
                pathInfo = pathInfo.GetSubPathInfo(SubFolder);
            }

            return pathInfo;
        }

        private VisualEditFieldParams GetVisualEditFieldParamsForInit() => IsNew ? VisualEditFieldParams.Create(Content.Site) : _fieldRepository.GetVisualEditFieldParams(Id);

        private VisualEditorConfig GetVisualEditorConfigForInit() => new VisualEditorConfig(this);

        public Field Init()
        {
            _exactType = new InitPropertyValue<FieldExactTypes>(GetExactTypeForInit, GetExactTypeForSet);

            _constraint = new InitPropertyValue<ContentConstraint>(GetConstraintForInit);
            _contentLink = new InitPropertyValue<ContentLink>(GetContentLinkForInit);
            _relation = new InitPropertyValue<Field>(GetRelationForInit);
            _backRelation = new InitPropertyValue<Field>(GetBackRelationForInit);

            if (IsNew)
            {
                Indexed = ExactType == FieldExactTypes.O2MRelation;
                OnScreen = ExactType == FieldExactTypes.String || ExactType == FieldExactTypes.Textbox || ExactType == FieldExactTypes.VisualEdit;

                TextBoxRows = TextBoxRowsDefaultValue;
                VisualEditorHeight = VisualEditorHeightDefaultValue;
                StringSize = StringSizeDefaultValue;
                DecimalPlaces = DecimalPlacesDefaultValue;
                UseInputMask = false;
            }

            InitDefaultValues();
            InitOrderBy();
            IsInteger = IsNew || ExactType == FieldExactTypes.Numeric && DecimalPlaces == 0;
            UseRelationCondition = !IsNew && !string.IsNullOrEmpty(RelationCondition);

            _dynamicImage = new InitPropertyValue<DynamicImage>(GetDynamicImageForInit);
            _m2MBackwardField = new InitPropertyValue<Field>(GetM2MBackwardForInit);
            _o2MBackwardField = new InitPropertyValue<Field>(GetO2MBackwardForInit);
            _relateToContentId = new InitPropertyValue<int?>(GetRelateToForInit);
            _isUnique = new InitPropertyValue<bool>(GetIsUniqueForInit, GetIsUniqueForSet);
            _pathInfo = new InitPropertyValue<PathInfo>(GetPathInfoForInit);
            _visualEditor = new InitPropertyValue<VisualEditorConfig>(GetVisualEditorConfigForInit);
            _visualEditFieldParams = new InitPropertyValue<VisualEditFieldParams>(GetVisualEditFieldParamsForInit);
            _externalCssItems = new InitPropertyValue<IEnumerable<ExternalCss>>(GetExternalCssItemsForInit);
            _hasAnyAggregators = new InitPropertyValue<bool>(GetHasAnyAggregatorsForInit);
            _parentField = new InitPropertyValue<Field>(GetParentFieldForInit);
            _childFields = new InitPropertyValue<IEnumerable<Field>>(GetChildFieldsForInit);
            _activeVeCommandIds = new InitPropertyValue<int[]>(GetActiveVeCommandIds);
            _activeVeStyleIds = new InitPropertyValue<int[]>(GetActiveVeStyleIds);
            _defaultArticleIds = new InitPropertyValue<int[]>(GetDefaultArticleIds);

            return this;
        }

        private int[] GetActiveVeStyleIds()
        {
            return VisualEditorRepository.GetResultStyles(Id, Content.SiteId).Where(n => n.On).Select(n => n.Id).ToArray();
        }

        private int[] GetDefaultArticleIds()
        {
            return ExactType == FieldExactTypes.M2MRelation ? _fieldRepository.GetActiveArticlesForM2MField(Id).Select(x => x.Id).ToArray() : new int[] { };
        }

        private int[] GetActiveVeCommandIds()
        {
            return VisualEditorRepository.GetResultCommands(Id, Content.SiteId).Where(n => n.On).Select(n => n.Id).ToArray();
        }

        internal void LoadVeBindings()
        {
        }

        private void InitDefaultValues()
        {
            if (ExactType != FieldExactTypes.M2ORelation && !String.IsNullOrEmpty(DefaultValue))
            {
                StringDefaultValue = DefaultValue;
                NumericDefaultValue = Converter.ToNullableDouble(DefaultValue);
                BooleanDefaultValue = Converter.ToBoolean(DefaultValue, false);

                if (ExactType == FieldExactTypes.String)
                {
                    TextBoxDefaultValue = StringDefaultValue;
                    VisualEditDefaultValue = StringDefaultValue;
                }
                else if (ExactType == FieldExactTypes.Textbox || ExactType == FieldExactTypes.VisualEdit)
                {
                    TextBoxDefaultValue = DefaultBlobValue;
                    VisualEditDefaultValue = DefaultBlobValue;
                }
                else if (ExactType == FieldExactTypes.Image || ExactType == FieldExactTypes.File)
                {
                    FileDefaultValue = DefaultValue;
                }
                else if (ExactType == FieldExactTypes.O2MRelation)
                {
                    O2MDefaultValue = DefaultValue;
                }
                else if (ExactType == FieldExactTypes.Classifier)
                {
                    ClassifierDefaultValue = Converter.ToNullableInt32(DefaultValue);
                }
                else if (ExactType == FieldExactTypes.Date)
                {
                    DateDefaultValue = Converter.ToNullableDateTime(DefaultValue);
                }
                else if (ExactType == FieldExactTypes.DateTime)
                {
                    DateTimeDefaultValue = Converter.ToNullableDateTime(DefaultValue);
                }
                else if (ExactType == FieldExactTypes.Time)
                {
                    var tdt = Converter.ToNullableDateTime(DefaultValue);
                    TimeDefaultValue = tdt?.TimeOfDay;
                }
            }
        }

        private void InitOrderBy()
        {
            ListFieldTitleCount = FieldTitleCount;
            ListO2MFieldTitleCount = FieldTitleCount;
            ListOrderByTitle = OrderByTitle;
            ListOrderFieldId = OrderFieldId;
            ListIncludeRelationsInTitle = IncludeRelationsInTitle;
            ListO2MIncludeRelationsInTitle = IncludeRelationsInTitle;
            TreeFieldTitleCount = FieldTitleCount;
            TreeOrderByTitle = OrderByTitle;
            TreeOrderFieldId = OrderFieldId;
            TreeIncludeRelationsInTitle = IncludeRelationsInTitle;
        }

        public void UpdateModel()
        {
            if (ExactType == FieldExactTypes.Numeric && IsInteger)
            {
                DecimalPlaces = 0;
            }

            if (ExactType == FieldExactTypes.Classifier)
            {
                IsClassifier = true;
                Indexed = true;
                IsInteger = true;
                DecimalPlaces = 0;
            }
            else
            {
                IsClassifier = false;
            }

            if (ExactType == FieldExactTypes.Boolean || ExactType == FieldExactTypes.DynamicImage)
            {
                Required = false;
            }

            if (ExactType == FieldExactTypes.Date
                || ExactType == FieldExactTypes.DateTime
                || ExactType == FieldExactTypes.Time
                || ExactType == FieldExactTypes.DynamicImage
                || ExactType == FieldExactTypes.M2MRelation
                || ExactType == FieldExactTypes.M2ORelation
                || ExactType == FieldExactTypes.Textbox
                || ExactType == FieldExactTypes.VisualEdit)
            {
                IsUnique = false;
            }

            if (ExactType == FieldExactTypes.Boolean
                || ExactType == FieldExactTypes.Textbox
                || ExactType == FieldExactTypes.VisualEdit
                || ExactType == FieldExactTypes.M2MRelation
                || ExactType == FieldExactTypes.M2ORelation)
            {
                Indexed = false;
            }

            if (ExactType == FieldExactTypes.O2MRelation)
            {
                Indexed = true;
                if (!RelateToContentId.HasValue || RelateToContentId == ContentId)
                {
                    Aggregated = false;
                }

                if (!Aggregated)
                {
                    ClassifierId = null;
                }
            }

            if (ExactType == FieldExactTypes.M2MRelation
                || ExactType == FieldExactTypes.M2ORelation
                || ExactType == FieldExactTypes.DynamicImage)
            {
                OnScreen = false;
            }

            if (ExactType != FieldExactTypes.VisualEdit)
            {
                DocType = null;
            }

            UpdateStringEnum();
            UpdateDefaultValueModel();
            UpdateRelationModel();
            UpdateOrderByModel();
            UpdateBindingsModel();

            if (!MapAsProperty)
            {
                LinqPropertyName = null;
                UseSeparateReverseViews = false;
            }

            if (ContentId != RelateToContentId)
            {
                TreeOrderFieldId = null;
            }
        }

        public void UpdateBindingsModel()
        {
        }

        public void UpdateStringEnum()
        {
            if (ExactType == FieldExactTypes.StringEnum)
            {
                var enumDefValue = StringEnumItems.FirstOrDefault(v => v.GetIsDefault());
                StringDefaultValue = enumDefValue?.Value;

                Indexed = true;
                if (ShowAsRadioButtons)
                {
                    Required = true;
                }
            }
            else
            {
                StringEnumItems = Enumerable.Empty<StringEnumItem>();
                ShowAsRadioButtons = false;
            }
        }

        /// <summary>
        /// Установить значение поля по умолчанию
        /// </summary>
        private void UpdateDefaultValueModel()
        {
            switch (ExactType)
            {
                case FieldExactTypes.Boolean:
                    DefaultValue = BooleanDefaultValue ? "1" : "0";
                    break;
                case FieldExactTypes.Date:
                    DefaultValue = Converter.ToDbDateTimeString(DateDefaultValue);
                    break;
                case FieldExactTypes.DateTime:
                    DefaultValue = Converter.ToDbDateTimeString(DateTimeDefaultValue);
                    break;
                case FieldExactTypes.Time:
                    DefaultValue = Converter.ToDbDateTimeString(TimeDefaultValue);
                    break;
                case FieldExactTypes.String:
                case FieldExactTypes.StringEnum:
                    DefaultValue = StringDefaultValue;
                    break;
                case FieldExactTypes.Numeric:
                    DefaultValue = Converter.ToDbNumericString(NumericDefaultValue);
                    break;
                case FieldExactTypes.Image:
                case FieldExactTypes.File:
                    DefaultValue = FileDefaultValue;
                    break;
                case FieldExactTypes.Textbox:
                    DefaultBlobValue = TextBoxDefaultValue;
                    break;
                case FieldExactTypes.VisualEdit:
                    DefaultBlobValue = VisualEditDefaultValue;
                    break;
                case FieldExactTypes.O2MRelation:
                    DefaultValue = O2MDefaultValue;
                    break;
                case FieldExactTypes.Classifier:
                    DefaultValue = Converter.ToDbNumericString(ClassifierDefaultValue);
                    break;
                default:
                    DefaultValue = null;
                    DefaultBlobValue = null;
                    break;
            }
        }

        /// <summary>
        /// Устанавливает свойства поля типа "связь"
        /// </summary>
        private void UpdateRelationModel()
        {
            if (ExactType != FieldExactTypes.M2MRelation && ExactType != FieldExactTypes.O2MRelation)
            {
                UseForDefaultFiltration = false;
            }

            if (ExactType != FieldExactTypes.M2MRelation)
            {
                LinkId = null;
                ContentLink = null;
                OptimizeForHierarchy = false;
                UseSeparateReverseViews = false;
            }

            if (ExactType != FieldExactTypes.O2MRelation)
            {
                RelationId = null;
                LinqBackPropertyName = null;
            }

            if (ExactType != FieldExactTypes.M2ORelation)
            {
                BackRelationId = null;
            }

            if (ExactType != FieldExactTypes.O2MRelation && ExactType != FieldExactTypes.M2MRelation)
            {
                UseRelationCondition = false;
                UseRelationSecurity = false;
            }

            if (!UseRelationCondition)
            {
                RelationCondition = null;
            }

            if (ExactType == FieldExactTypes.M2MRelation)
            {
                if (RelateToContentId.HasValue)
                {
                    var relContent = _contentRepository.GetById(RelateToContentId.Value);
                    if (!relContent.HasTreeField)
                    {
                        OptimizeForHierarchy = false;
                    }

                    if (ContentLink != null && ContentLink.LContentId == ContentId && ContentLink.RContentId != RelateToContentId.Value)
                    {
                        ContentLink.RContentId = RelateToContentId.Value;
                        ContentLink.LinkId = 0;
                    }
                    else if (ContentLink != null && ContentLink.RContentId == ContentId && ContentLink.LContentId != RelateToContentId.Value)
                    {
                        ContentLink.LContentId = RelateToContentId.Value;
                        ContentLink.LinkId = 0;
                    }

                    // Создать обратное поле если это необходимо и возможно
                    if (!string.IsNullOrWhiteSpace(NewM2MBackwardFieldName) && !IsBackwardFieldExists && RelateToContentId != ContentId)
                    {
                        // возможно только если Related контент не виртуальный
                        if (relContent.VirtualType == 0)
                        {
                            var f = Create(relContent, _fieldRepository, _contentRepository);
                            f.Name = NewM2MBackwardFieldName;
                            f.ExactType = FieldExactTypes.M2MRelation;
                            f.ContentId = relContent.Id;
                            f.RelateToContentId = ContentId;
                            M2MBackwardField = f;
                        }
                    }
                }

                if (ContentLink != null && !ContentLink.MapAsClass)
                {
                    ContentLink.NetLinkName = null;
                    ContentLink.NetPluralLinkName = null;
                }
            }

            if (ExactType == FieldExactTypes.O2MRelation)
            {
                // Создать обратное поле если это необходимо и возможно
                if (!string.IsNullOrWhiteSpace(NewO2MBackwardFieldName) && !IsBackwardFieldExists)
                {
                    if (RelateToContentId != null)
                    {
                        var relContent = _contentRepository.GetById(RelateToContentId.Value);

                        // возможно только если Related контент не виртуальный
                        if (relContent.VirtualType == 0)
                        {
                            var f = Create(relContent, _fieldRepository, _contentRepository);
                            f.Name = NewO2MBackwardFieldName;
                            f.ExactType = FieldExactTypes.M2ORelation;
                            f.ContentId = relContent.Id;
                            f.BackRelationId = Id;
                            f.BackRelation = this;
                            if (MapAsProperty)
                            {
                                f.MapAsProperty = true;
                                f.LinqPropertyName = LinqBackPropertyName;
                                f.LinqBackPropertyName = null;
                            }
                            O2MBackwardField = f;
                        }
                    }
                }
            }

            if (ExactType != FieldExactTypes.O2MRelation || ContentId != RelateToContentId)
            {
                UseForTree = false;
                AutoCheckChildren = false;
                CopyPermissionsToChildren = false;
            }
        }

        public void UpdateOrderByModel()
        {
            if (ExactType == FieldExactTypes.M2MRelation || ExactType == FieldExactTypes.M2ORelation)
            {
                FieldTitleCount = ListFieldTitleCount;
                OrderByTitle = ListOrderByTitle;
                OrderFieldId = ListOrderFieldId;
                IncludeRelationsInTitle = ListIncludeRelationsInTitle;
            }
            else if (ExactType == FieldExactTypes.O2MRelation && ContentId == RelateToContentId)
            {
                FieldTitleCount = TreeFieldTitleCount;
                OrderByTitle = TreeOrderByTitle;
                OrderFieldId = TreeOrderFieldId;
                IncludeRelationsInTitle = TreeIncludeRelationsInTitle;
            }
            else if (ExactType == FieldExactTypes.O2MRelation && ContentId != RelateToContentId)
            {
                FieldTitleCount = ListO2MFieldTitleCount;
                OrderByTitle = false;
                OrderFieldId = null;
                IncludeRelationsInTitle = ListO2MIncludeRelationsInTitle;
            }
            else
            {
                FieldTitleCount = 1;
                OrderByTitle = false;
                OrderFieldId = null;
                IncludeRelationsInTitle = false;
            }
        }

        public override void Validate()
        {
            var errors = new RulesException<Field>();
            base.Validate(errors);
            ValidateField(errors);
            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }

        private void ValidateExternalCss(RulesException errors)
        {
            if (ExactType == FieldExactTypes.VisualEdit)
            {
                ExternalCssHelper.ValidateExternalCss(ExternalCssItems, errors);
            }
        }

        private void ValidateField(RulesException<Field> errors)
        {
            // ------------------------------------------------------------------------------
            // ReadOnly и Required нельзя устанавливать вместе
            if (ReadOnly && Required)
            {
                errors.ErrorFor(f => f.ReadOnly, FieldStrings.ReadOnlyAndRequiredBothAreTrue);
            }

            if (ReadOnly && ExactType == FieldExactTypes.Classifier)
            {
                errors.ErrorFor(f => ReadOnly, FieldStrings.ClassiferCannotBeReadonly);
            }

            if (IsUnique)
            {
                if (IsNew && !Constraint.IsComplex)
                {
                    if (_contentRepository.IsAnyArticle(ContentId))
                    {
                        errors.ErrorFor(f => IsUnique, FieldStrings.CannotCreateUniqueFieldBecauseOfExisting);
                    }
                }
                else
                {
                    var count = Constraint.CountDuplicates(0);
                    if (count > 0)
                    {
                        errors.ErrorFor(f => IsUnique, string.Format(FieldStrings.ExistingDataIsNotUnique, count));
                    }
                }
            }

            // Значение по умолчанию и уникальность нельзя устанавливать одновременно
            if (!string.IsNullOrEmpty(Default) && IsUnique)
            {
                errors.ErrorFor(f => f.DefaultValue, FieldStrings.DefaultAndUniqueBothAreSet);
            }

            // Проверить дли значения по умолчанию
            if (!IsUnique
                && ExactType != FieldExactTypes.Textbox
                && ExactType != FieldExactTypes.VisualEdit
                && !string.IsNullOrEmpty(Default)
                && !DefaultValue.Length.IsInRange(0, 255))
            {
                errors.ErrorFor(f => f.DefaultValue, FieldStrings.DefaultMaxLengthExceeded);
            }

            if (UseInputMask && string.IsNullOrWhiteSpace(InputMask))
            {
                errors.ErrorFor(f => f.InputMask, FieldStrings.InputMaskNotEntered);
            }

            ValidateRelations(errors);
            ValidateSize(errors);
            ValidateDynamicImage(errors);
            ValidateTypeChanging(errors);
            ValidateAggregated(errors);
            ValidateStringEnumValues(errors);
            ValidateVariations(errors);
            ValidateExternalCss(errors);

            // Валидация поля на конфликты с полями дочерних контентов
            foreach (var ruleViolation in new FieldsConflictValidator().SubContentsCheck(Content, this))
            {
                errors.ErrorForModel(ruleViolation.Message);
            }
        }

        private void ValidateVariations(RulesException<Field> errors)
        {
            if (UseForVariations && Content.Fields.Any(n => n.ExactType == FieldExactTypes.M2ORelation))
            {
                errors.ErrorFor(f => f.UseForVariations, FieldStrings.VariationAndM2OIncompatible);
            }

            if (ExactType == FieldExactTypes.M2ORelation && Content.Fields.Any(n => n.UseForVariations))
            {
                errors.ErrorFor(f => f.ExactType, FieldStrings.VariationAndM2OIncompatible);
            }
        }

        private void ValidateStringEnumValues(RulesException<Field> errors)
        {
            if (ExactType == FieldExactTypes.StringEnum)
            {
                if (StringEnumItems.Any())
                {
                    foreach (var v in StringEnumItems)
                    {
                        v.Invalid = false;
                    }

                    var emptyEnumValues =
                        StringEnumItems.Where(
                            v => string.IsNullOrWhiteSpace(v.Value) || string.IsNullOrWhiteSpace(v.Alias)).ToArray();
                    var sameValueItems =
                        StringEnumItems.GroupBy(v => v.Value).Where(g => g.Count() > 1).SelectMany(g => g).ToArray();
                    var sameAliasItems =
                        StringEnumItems.GroupBy(v => v.Alias).Where(g => g.Count() > 1).SelectMany(g => g).ToArray();
                    if (emptyEnumValues.Any())
                    {
                        errors.ErrorFor(m => m.StringEnumItems, FieldStrings.EnumValueIsInvalid);
                    }

                    if (sameValueItems.Any())
                    {
                        errors.ErrorFor(m => m.StringEnumItems, FieldStrings.EnumValueValueIsNotUnique);
                    }

                    if (sameAliasItems.Any())
                    {
                        errors.ErrorFor(m => m.StringEnumItems, FieldStrings.EnumValueAliasIsNotUnique);
                    }

                    var invalidateItems = emptyEnumValues;
                    if (!invalidateItems.Any())
                    {
                        invalidateItems = sameValueItems;
                    }

                    if (!invalidateItems.Any())
                    {
                        invalidateItems = sameAliasItems;
                    }

                    foreach (var v in invalidateItems)
                    {
                        v.Invalid = true;
                    }
                }
                else
                {
                    errors.ErrorFor(m => m.StringEnumItems, FieldStrings.EnumValueIsNotDefined);
                }
            }
        }

        private void ValidateAggregated(RulesException<Field> errors)
        {
            var aggregatedChangeType = 3; // (Type: 3 - осталось неагрегированным);
            Field dbFieldVersion = null;

            if (IsNew)
            {
                aggregatedChangeType = Aggregated ? 0 : 3;
            }
            else
            {
                dbFieldVersion = _fieldRepository.GetById(Id);
                if (dbFieldVersion.Aggregated && !Aggregated)
                {
                    aggregatedChangeType = 1;
                }
                else if (!dbFieldVersion.Aggregated && Aggregated)
                {
                    aggregatedChangeType = 0;
                }
                else if (dbFieldVersion.Aggregated && Aggregated)
                {
                    aggregatedChangeType = 2;
                }
                else if (!dbFieldVersion.Aggregated && !Aggregated)
                {
                    aggregatedChangeType = 3;
                }
            }

            if (aggregatedChangeType == 0 || aggregatedChangeType == 2) // поле становиться агрегированным (Type: 0) или осталось агрегированным (Type: 2)
            {
                if (Content.Fields.Any(f => f.Id != Id && f.Aggregated))
                {
                    // Контент не должен содержать других агрегированных связей
                    errors.ErrorFor(f => f.Aggregated, FieldStrings.ThereIsAggregatedInContent);
                }
                else
                {
                    if (!ClassifierId.HasValue)
                    {
                        errors.ErrorFor(f => f.ClassifierId, FieldStrings.ClassifierRefNotSelected);
                    }

                    if (!IsUnique) // агрегированное поле должно быть уникальным
                    {
                        errors.ErrorFor(f => f.IsUnique, FieldStrings.AggregatedIsNotUnique);
                    }
                    else if (Constraint.IsComplex) // но не в комбинации с каким-либо другим полем
                    {
                        errors.ErrorFor(f => f.IsUnique, FieldStrings.AggregatedIsComplexUnique);
                    }

                    if (Content.HasAnyNotification)
                    {
                        errors.ErrorForModel(FieldStrings.CannotSetAggregateBecauseOfNotification);
                    }

                    if (RelateToContentId.HasValue)
                    {
                        if (RelateToContentId == ContentId)
                        {
                            // Нельзя сделать связь агрегированной, если это связь на тот же контент
                            errors.ErrorFor(f => f.RelateToContentId, FieldStrings.AggregatedRelateToCurrentContent);
                        }
                        else if (RelatedToContent.HasAggregatedFields)
                        {
                            // Корневой контент не должен содержать агрегированных связей
                            errors.ErrorFor(f => f.RelateToContentId, FieldStrings.ThereAreAggregatedsInRootContent);
                        }
                    }

                    if (_contentRepository.IsAnyArticle(ContentId)) // Если контент содержит статьи, то
                    {
                        if (aggregatedChangeType == 0) // поле нельзя сделать агрегиованным
                        {
                            errors.ErrorFor(f => Aggregated, FieldStrings.CannotSetAggregateBecauseOfExisting);
                        }
                        else if (aggregatedChangeType == 2)
                        {
                            if (dbFieldVersion != null && dbFieldVersion.ExactType != ExactType) // нельзя сменить тип поля
                            {
                                errors.ErrorFor(f => ExactType, FieldStrings.CannotChangeAggregateFieldTypeBecauseOfExisting);
                            }

                            if (dbFieldVersion != null && dbFieldVersion.ClassifierId != ClassifierId) // нельзя изменить ссылку на классификатор
                            {
                                errors.ErrorFor(f => ClassifierId, FieldStrings.CannotChangeClassifierRefBecauseOfExisting);
                            }
                        }
                    }

                    if (aggregatedChangeType == 2 && ClassifierId.HasValue && _fieldRepository.GetById(ClassifierId.Value).ClassifierDefaultValue == ContentId)
                    {
                        // Если ClassifierDefaultValue классификатора равно контенту текущего поля, то
                        if (dbFieldVersion != null && dbFieldVersion.ExactType != ExactType)
                        {
                            // нельзя сменить тип поля
                            errors.ErrorFor(f => ExactType, FieldStrings.CannotChangeAggregateFieldTypeBecauseOfClassifierDefaultValue);
                        }

                        if (dbFieldVersion != null && dbFieldVersion.ClassifierId != ClassifierId)
                        {
                            // нельзя изменить ссылку на классификатор
                            errors.ErrorFor(f => ClassifierId, FieldStrings.CannotChangeClassifierRefBecauseOfClassifierDefaultValue);
                        }
                    }

                    // Дочерний и родительский контенты должны иметь одинаковые значения свойства контента:
                    if (Content.AutoArchive != RelatedToContent.AutoArchive)
                    {
                        // Архивировать при удалении
                        errors.ErrorFor(f => RelateToContentId, FieldStrings.RelatedContentAutoArchiveIsDifferent);
                    }

                    if (Content.WorkflowBinding.WorkflowId != RelatedToContent.WorkflowBinding.WorkflowId)
                    {
                        // Тип Workflow
                        errors.ErrorFor(f => RelateToContentId, FieldStrings.RelatedContentWorkflowIsDifferent);
                    }

                    if (Content.WorkflowBinding.IsAsync != RelatedToContent.WorkflowBinding.IsAsync)
                    {
                        // Расщеплять ли статьи по Workflow
                        errors.ErrorFor(f => RelateToContentId, FieldStrings.RelatedContentWorkflowSplitIsDifferent);
                    }
                }
            }
            else if (aggregatedChangeType == 1) // поле становиться не агрегированным (Type: 1)
            {
                if (_contentRepository.IsAnyArticle(ContentId))
                {
                    // если контент содержит статьи, то
                    errors.ErrorFor(f => Aggregated, FieldStrings.CannotResetAggregateBecauseOfExisting);
                }

                if (dbFieldVersion == null)
                {
                    throw new ArgumentNullException();
                }

                if (dbFieldVersion.ClassifierId.HasValue && _fieldRepository.GetById(dbFieldVersion.ClassifierId.Value).ClassifierDefaultValue == ContentId)
                {
                    // Если ClassifierDefaultValue классификатора равно контенту текущего поля, то
                    errors.ErrorFor(f => Aggregated, FieldStrings.CannotResetAggregateBecauseOfClassifierDefaultValue);
                }
            }
        }

        private void ValidateTypeChanging(RulesException<Field> errors)
        {
            if (!IsNew)
            {
                var dbFieldVersion = _fieldRepository.GetById(Id);
                if (dbFieldVersion.ExactType != ExactType)
                {
                    // Участвует ли поле в сложном ограничении уникальности
                    if (dbFieldVersion.IsUnique && dbFieldVersion.Constraint.IsComplex && IsUnique && Constraint.IsComplex)
                    {
                        errors.ErrorFor(f => f.ExactType, FieldStrings.MultiplyUniqueConstraint);
                    }

                    // Если на поле есть ссылки по O2M, то нельзя менять тип этого поля на не "ссылочный"
                    if (RelatedO2MFields.Any())
                    {
                        // Если поле уже не ссылочное, то запретитить менять тип
                        if (!dbFieldVersion.IsRelateable)
                        {
                            ExactType = dbFieldVersion.ExactType;
                        }

                        // Если было ссылочное, а меняем на не ссылочное, то оставить тип без изменения
                        else if (!IsRelateable)
                        {
                            errors.ErrorFor(f => f.ExactType, FieldStrings.CannotChangeTypeToNotRelateable);
                        }
                    }

                    // Если поле классификатор и на него ссылаються агрегированные поля, то тип менять нельзя
                    if (dbFieldVersion.ExactType == FieldExactTypes.Classifier && dbFieldVersion.HasAnyAggregators)
                    {
                        errors.ErrorFor(f => f.ExactType, FieldStrings.CannotChangeClassifierType);
                    }
                }

                // String -> File || Image: контент в БД не более 255 символов
                if (dbFieldVersion.ExactType == FieldExactTypes.String && (ExactType == FieldExactTypes.File || ExactType == FieldExactTypes.Image))
                {
                    if (_fieldRepository.GetTextFieldMaxLength(dbFieldVersion) > 255)
                    {
                        errors.ErrorFor(f => f.ExactType, string.Format(FieldStrings.FieldValueLengthExceeded, 255));
                    }
                }

                // String -> StringEnum
                else if (dbFieldVersion.ExactType == FieldExactTypes.String && ExactType == FieldExactTypes.StringEnum)
                {
                    if (_fieldRepository.IsNonEnumFieldValueExist(this))
                    {
                        errors.ErrorFor(f => f.ExactType, FieldStrings.NonEnumFieldValueExist);
                    }
                }

                // Textbox || VisualEdit -> String: контент в БД не более 3500 символов
                else if (dbFieldVersion.IsBlob && ExactType == FieldExactTypes.String)
                {
                    if (_fieldRepository.GetTextFieldMaxLength(dbFieldVersion) > StringSize)
                    {
                        errors.ErrorFor(f => f.ExactType, string.Format(FieldStrings.FieldValueLengthExceeded, StringSize));
                    }
                }
                else if (dbFieldVersion.ExactType == FieldExactTypes.Numeric && ExactType == FieldExactTypes.Boolean)
                {
                    var maxValue = _fieldRepository.GetNumericFieldMaxValue(dbFieldVersion);
                    if (maxValue.HasValue && !maxValue.Value.IsInRange(0, 1))
                    {
                        errors.ErrorFor(f => f.ExactType, string.Format(FieldStrings.FieldValueIsNotInRange, 0, 1));
                    }
                }
                else if (dbFieldVersion.ExactType == FieldExactTypes.Numeric && ExactType == FieldExactTypes.O2MRelation)
                {
                    if (!_fieldRepository.CheckNumericValuesAsKey(this, dbFieldVersion))
                    {
                        errors.ErrorFor(f => f.ExactType, FieldStrings.FieldHasBadValues);
                    }
                }
                else if (dbFieldVersion.ExactType == FieldExactTypes.O2MRelation && ExactType == FieldExactTypes.M2MRelation)
                {
                    if (dbFieldVersion.ContentId == dbFieldVersion.RelateToContentId)
                    {
                        errors.ErrorFor(f => f.ExactType, FieldStrings.CannotChangeRelationTypeForSelfRelation);
                    }
                }
                else if (dbFieldVersion.ExactType == FieldExactTypes.M2MRelation && ExactType == FieldExactTypes.O2MRelation)
                {
                    if (dbFieldVersion.ContentId == dbFieldVersion.RelateToContentId)
                    {
                        errors.ErrorFor(f => f.ExactType, FieldStrings.CannotChangeRelationTypeForSelfRelation);
                    }
                    else if (_fieldRepository.DoPluralLinksExist(dbFieldVersion))
                    {
                        errors.ErrorFor(f => f.ExactType, FieldStrings.FieldHasBadValues);
                    }
                }
            }
        }

        private void ValidateDynamicImage(RulesException<Field> errors)
        {
            if (ExactType == FieldExactTypes.DynamicImage)
            {
                if (BaseImageId == null)
                {
                    errors.ErrorFor(f => f.BaseImageId, FieldStrings.BaseImageNotSelected);
                }

                if (DynamicImage.Type == DynamicImage.JPG_EXTENSION)
                {
                    if (DynamicImage.Quality == null)
                    {
                        errors.ErrorFor(f => f.DynamicImage.Quality, FieldStrings.DynamicImageQualityNotEntered);
                    }
                    else if (!DynamicImage.Quality.Value.IsInRange(DynamicImage.MinQuality, DynamicImage.MaxQuality))
                    {
                        errors.ErrorFor(f => f.DynamicImage.Quality, FieldStrings.DynamicImageQualityNotInRange);
                    }
                }

                if (DynamicImage.ResizeMode != DynamicImage.ImageResizeMode.ByWidth && !DynamicImage.Height.IsInRange(DynamicImage.MinImageSize, DynamicImage.MaxImageSize))
                {
                    errors.ErrorFor(f => f.DynamicImage.Height, FieldStrings.DynamicImageHeightNotInRange);
                }
                if (DynamicImage.ResizeMode != DynamicImage.ImageResizeMode.ByHeight && !DynamicImage.Width.IsInRange(DynamicImage.MinImageSize, DynamicImage.MaxImageSize))
                {
                    errors.ErrorFor(f => f.DynamicImage.Width, FieldStrings.DynamicImageWidthNotInRange);
                }
            }
        }

        private void ValidateSize(RulesException<Field> errors)
        {
            if (Content.VirtualType == VirtualType.None)
            {
                switch (ExactType)
                {
                    case FieldExactTypes.Textbox:
                        if (!TextBoxRows.IsInRange(5, 30))
                        {
                            errors.ErrorFor(f => f.TextBoxRows, FieldStrings.TextboxRowsNotInRange);
                        }
                        break;
                    case FieldExactTypes.VisualEdit:
                        if (!VisualEditorHeight.IsInRange(350, 750))
                        {
                            errors.ErrorFor(f => f.VisualEditorHeight, FieldStrings.VisualEditHeightNotInRange);
                        }
                        break;
                    case FieldExactTypes.Numeric:
                        if (!DecimalPlaces.IsInRange(0, 25))
                        {
                            errors.ErrorFor(f => f.DecimalPlaces, FieldStrings.DecimalPlacesNotInRange);
                        }
                        break;
                    case FieldExactTypes.String:
                    {
                        if (!IsNew)
                        {
                            var dbFieldVersion = _fieldRepository.GetById(Id);
                            if (dbFieldVersion.ExactType == FieldExactTypes.String)
                            {
                                var maxFieldLength = _fieldRepository.GetTextFieldMaxLength(dbFieldVersion);
                                if (maxFieldLength > StringSize)
                                {
                                    errors.ErrorFor(f => f.StringSize, string.Format(FieldStrings.FieldValueLengthExceeded, maxFieldLength));
                                }
                            }
                        }
                        if (!StringSize.IsInRange(1, 3500))
                        {
                            errors.ErrorFor(f => f.StringSize, FieldStrings.StringLengthNotInRange);
                        }
                        break;
                    }
                }
            }
        }

        private static bool LinqPropertyNameExistsInSystemColumns(string linqPropertyName)
        {
            //Проверка на пересечение служебных полей и введённого пользователем LINQ имени
            var systemMembersNames = new SystemColumnMemberNames().GetFields();
            var systemNames = new SystemColumnNames().GetFields();
            return systemNames.Contains(linqPropertyName, StringComparer.CurrentCultureIgnoreCase) || systemMembersNames.Contains(linqPropertyName, StringComparer.CurrentCultureIgnoreCase);
        }

        private void ValidateRelations(RulesException<Field> errors)
        {
            if (MapAsProperty)
            {
                if (string.IsNullOrWhiteSpace(LinqPropertyName))
                {
                    errors.ErrorFor(f => f.LinqPropertyName, FieldStrings.LinqPropertyNameNotEntered);
                }
                else if (_fieldRepository.LinqPropertyNameExists(this))
                {
                    errors.ErrorFor(f => f.LinqPropertyName, FieldStrings.LinqPropertyNameNonUnique);
                }
                else if (LinqPropertyNameExistsInSystemColumns(LinqPropertyName))
                {
                    errors.ErrorFor(f => f.LinqPropertyName, FieldStrings.LinqPropertyNameExistInSystemColumns);
                }

                if (ExactType == FieldExactTypes.O2MRelation && O2MBackwardField == null)
                {
                    if (string.IsNullOrWhiteSpace(LinqBackPropertyName))
                    {
                        errors.ErrorFor(f => f.LinqBackPropertyName, FieldStrings.LinqBackPropertyNameNotEntered);
                    }
                    else if (_fieldRepository.LinqBackPropertyNameExists(this))
                    {
                        errors.ErrorFor(f => f.LinqBackPropertyName, FieldStrings.LinqBackPropertyNameNonUnique);
                    }
                    else if (LinqPropertyNameExistsInSystemColumns(LinqBackPropertyName))
                    {
                        errors.ErrorFor(f => f.LinqBackPropertyName, FieldStrings.LinqBackPropertyNameExistInSystemColumns);
                    }

                    if (!string.IsNullOrWhiteSpace(LinqPropertyName) && LinqPropertyName.Equals(LinqBackPropertyName, StringComparison.InvariantCulture))
                    {
                        errors.ErrorFor(f => f.LinqBackPropertyName, FieldStrings.LinqPropertyNamesAreEqual);
                    }
                }
            }

            if (TypeId == FieldTypeCodes.Relation)
            {
                if (RelateToContentId == null)
                {
                    errors.ErrorFor(f => f.RelateToContentId, FieldStrings.RelateToContentNotSelected);
                }

                if (UseRelationSecurity && !Required)
                {
                    errors.ErrorFor(f => f.Required, FieldStrings.RelationSecurityFieldMustBeRequired);
                }
            }

            if (ExactType == FieldExactTypes.M2MRelation)
            {
                if (ContentLink.MapAsClass)
                {
                    if (string.IsNullOrWhiteSpace(ContentLink.NetLinkName))
                    {
                        errors.ErrorFor(f => f.ContentLink.NetLinkName, FieldStrings.NetLinkNameNotEntered);
                    }
                    else if (_fieldRepository.NetNameExists(ContentLink))
                    {
                        errors.ErrorFor(f => f.ContentLink.NetLinkName, FieldStrings.NetLinkNameNonUnique);
                    }

                    if (string.IsNullOrWhiteSpace(ContentLink.NetPluralLinkName))
                    {
                        errors.ErrorFor(f => f.ContentLink.NetPluralLinkName, FieldStrings.NetPluralLinkNameNotEntered);
                    }
                    else if (_fieldRepository.NetPluralNameExists(ContentLink))
                    {
                        errors.ErrorFor(f => f.ContentLink.NetPluralLinkName, FieldStrings.NetPluralLinkNameNonUnique);
                    }

                    if (ContentLink.NetLinkName.Equals(ContentLink.NetPluralLinkName, StringComparison.InvariantCulture))
                    {
                        errors.ErrorFor(f => f.ContentLink.NetPluralLinkName, FieldStrings.NetLinkNamesAreEqual);
                    }
                }

                if (!string.IsNullOrWhiteSpace(NewM2MBackwardFieldName))
                {
                    if (RelateToContentId == null)
                    {
                        throw new InvalidOperationException("RelateToContentId is null");
                    }

                    var relContent = _contentRepository.GetById(RelateToContentId.Value);

                    // BackwardField возможно создать только если Related контент не виртуальный
                    if (relContent.VirtualType != 0)
                    {
                        errors.ErrorFor(f => f.NewM2MBackwardFieldName, FieldStrings.VirtualBackwardFieldIsNotAllowed);
                    }
                }

                if (M2MBackwardField != null && M2MBackwardField.IsNew && !string.IsNullOrEmpty(M2MBackwardField.Name))
                {
                    if (M2MBackwardField.Name.Equals(Name, StringComparison.InvariantCulture))
                    {
                        errors.ErrorFor(f => f.NewM2MBackwardFieldName, FieldStrings.BackwardFieldNameAndNameAreEqual);
                    }
                    else
                    {
                        try
                        {
                            M2MBackwardField.Validate();
                        }
                        catch (RulesException<Field> ex)
                        {
                            foreach (var error in ex.Errors)
                            {
                                errors.ErrorFor(f => f.NewM2MBackwardFieldName, error.Message);
                            }
                        }
                    }
                }
            }
            else if (ExactType == FieldExactTypes.O2MRelation)
            {
                if (RelationId == null)
                {
                    errors.ErrorFor(f => f.RelationId, FieldStrings.RelationIdNotSelected);
                }

                if (UseForTree)
                {
                    var treeName = _contentRepository.GetTreeFieldName(ContentId, Id);
                    if (!string.IsNullOrEmpty(treeName))
                    {
                        errors.ErrorFor(f => f.UseForTree, string.Format(FieldStrings.UseForTreeAlreadySet, treeName));
                    }
                }

                if (!string.IsNullOrWhiteSpace(NewO2MBackwardFieldName))
                {
                    if (RelateToContentId == null)
                    {
                        throw new InvalidOperationException("RelateToContentId is null");
                    }

                    var relContent = _contentRepository.GetById(RelateToContentId.Value);

                    // BackwardField возможно создать только если Related контент не виртуальный
                    if (relContent.VirtualType != 0)
                    {
                        errors.ErrorFor(f => f.NewO2MBackwardFieldName, FieldStrings.VirtualBackwardFieldIsNotAllowed);
                    }
                }

                if (O2MBackwardField != null && O2MBackwardField.IsNew && !string.IsNullOrEmpty(O2MBackwardField.Name))
                {
                    if (O2MBackwardField.Name.Equals(Name, StringComparison.InvariantCulture))
                    {
                        errors.ErrorFor(f => f.NewO2MBackwardFieldName, FieldStrings.BackwardFieldNameAndNameAreEqual);
                    }
                    else
                    {
                        try
                        {
                            O2MBackwardField.Validate();
                        }
                        catch (RulesException<Field> ex)
                        {
                            foreach (var error in ex.Errors)
                            {
                                var isLinq = false;
                                if (error.Property?.Body is MemberExpression expr && expr.Member.Name == "LinqPropertyName")
                                {
                                    errors.ErrorFor(f => f.LinqBackPropertyName, error.Message);
                                    isLinq = true;
                                }

                                if (!isLinq)
                                {
                                    errors.ErrorFor(f => f.NewO2MBackwardFieldName, error.Message);
                                }
                            }
                        }
                    }
                }
            }

            else if (ExactType == FieldExactTypes.M2ORelation)
            {
                if (BackRelationId == null)
                {
                    errors.ErrorFor(f => f.BackRelationId, FieldStrings.BackRelationIdNotSelected);
                    return;
                }

                if (Content.AssignedAsyncWorkflow && !BackRelation.Content.AssignedAsyncWorkflow)
                {
                    errors.ErrorFor(f => f.BackRelationId, FieldStrings.BackRelationContentNotAssignedAsyncWorkflow);
                }
            }

            // Если Relation-поле используется для построения JOIN-контента и происходит изменения значимого для Relation параметра
            // (Имени, Типа или контента, на который ссылается поле Relation O2M)
            // то обновление запрещается
            if (!IsNew)
            {
                var dbVersion = _fieldRepository.GetById(Id);
                if (dbVersion.ExactType == FieldExactTypes.O2MRelation)
                {
                    var virtualChildrenFieldsExist = VirtualFieldRepository.JoinVirtualChildrenFieldsExist(this);
                    if (virtualChildrenFieldsExist &&
                        (!dbVersion.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase) ||
                            dbVersion.ExactType != ExactType ||
                            dbVersion.RelateToContentId != RelateToContentId)
                    )
                    {
                        errors.ErrorForModel(FieldStrings.VirtualChildrenFieldsExist);
                    }
                }
            }
        }

        public void ValidateBeforeDie(IList<string> violationMessages, bool forceChildDelete)
        {
            if (violationMessages == null)
            {
                throw new ArgumentNullException(nameof(violationMessages));
            }

            // Участвует ли поле в сложном ограничении уникальности
            if (IsUnique && Constraint.IsComplex)
            {
                violationMessages.Add(FieldStrings.MultiplyUniqueConstraint);
            }

            // Есть ли поля которые ссылаются на данное поле связью O2M ?
            if (RelatedO2MFields.Any())
            {
                // проверяем, есть ли в данном контенте поля на которые их можно перенаправить (алгоритм get_display_field)
                // если нет - то удалять нельзя
                if (_contentRepository.GetDisplayFieldIds(ContentId, false, 0).All(id => id == Id))
                {
                    violationMessages.Add(FieldStrings.RelatedFieldsExist);
                }
            }

            // ------------------------------------------------------------
            // Если для поля есть Thumbnails - то удалять поле нельзя
            if (GetDynamicImages().Any())
            {
                violationMessages.Add(FieldStrings.ThumbnailsExist);
            }

            // агрегированную связь нельзя удалять если
            if (ExactType == FieldExactTypes.O2MRelation && Aggregated)
            {
                if (_contentRepository.IsAnyArticle(ContentId)) // в контенте есть статьи
                {
                    violationMessages.Add(FieldStrings.CannotRemoveAggregated);
                }

                if (ClassifierId.HasValue && _fieldRepository.GetById(ClassifierId.Value).ClassifierDefaultValue == ContentId) // Если ClassifierDefaultValue классификатора равно контенту текущего поля
                {
                    violationMessages.Add(FieldStrings.CannotRemoveAggregatedBecauseOfClassifierDefaultValue);
                }
            }

            // классификатор нельзя удалять если есть агрегеруемые поля которые на него ссылаются
            if (ExactType == FieldExactTypes.Classifier && HasAnyAggregators)
            {
                violationMessages.Add(FieldStrings.CannotRemoveClassifier);
            }

            if (ParentField != null && !forceChildDelete)
            {
                violationMessages.Add(FieldStrings.CannotRemoveChildField);
            }
        }

        public static IEnumerable<string> Die(int id, bool removeChildFields = true)
        {
            var field = FieldRepository.GetById(id);
            return field != null ? field.Die(removeChildFields) : new[] { string.Format(FieldStrings.FieldNotFound, id) };
        }

        public IEnumerable<string> Die(bool removeVirtualFields = true, bool forceChildDelete = false)
        {
            var violationMessages = new List<string>();
            ValidateBeforeDie(violationMessages, forceChildDelete);
            if (!violationMessages.Any())
            {
                DieWithoutValidation(removeVirtualFields);
            }

            return violationMessages;
        }

        public void DieWithoutValidation(bool removeVirtualFields)
        {
            RemoveChildren();
            using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(ContentId))
            {
                // Удалить подчиненные виртуальные поля
                var rebuildedSubContentViews = removeVirtualFields ? RemoveVirtualFields() : null;

                // Если есть ссылающиеся поля на это поле, до переключить их на другое подходящее поле
                if (RelatedO2MFields.Any())
                {
                    var newDisplayFieldId = _contentRepository.GetDisplayFieldIds(ContentId, false, 0).First(id => id != Id);
                    _contentRepository.ChangeRelationIdToNewOne(Id, newDisplayFieldId);
                }

                // Удалить директорию динамического изображения
                DynamicImage?.DeleteDirectory();

                // Удалить ограничения уникальности
                if (IsUnique && !Constraint.IsNew)
                {
                    ContentConstraintRepository.Delete(Constraint);
                }

                // обнулить поле E-mail для уведомлений
                new NotificationRepository().ClearEmailField(Id);

                // Удалить версии линков M2M
                _fieldRepository.RemoveLinkVersions(Id);

                // M2O-поле не может существовать без основного
                O2MBackwardField?.Die();

                var helper = new VirtualContentHelper();
                var subContents = rebuildedSubContentViews?.ToArray() ?? new Content.TreeItem[]{ };
                var contents = helper.GetVirtualContentsToRebuild(subContents);

                if (QPContext.DatabaseType == DatabaseType.Postgres)
                {
                    foreach (var content in contents)
                    {
                        helper.DropContentViews(content);
                    }
                }

                // Удалить поле
                _fieldRepository.Delete(Id);



                // Перестроить подчиненные контенты
                if (removeVirtualFields)
                {
                    helper.RebuildSubContentViews(contents);
                }

                // Удалить записи OrderField
                _fieldRepository.ClearTreeOrder(Id);
            }
        }

        /// <summary>
        /// Удалить подчиненные виртуальные поля при удалении поля родительского контента
        /// </summary>
        private IEnumerable<Content.TreeItem> RemoveVirtualFields()
        {
            var helper = new VirtualContentHelper();
            return helper.RemoveSubContentVirtualFields(Content, new[] { Id });
        }

        public Field PersistWithBackward(bool explicitOrder)
        {
            SaveContentLink();
            var result = PersistWithVirtualRebuild(explicitOrder);
            Id = result.Id;
            SaveBackwardFields();
            return result;
        }

        private void SetParentAndOverride(int parentId)
        {
            ParentFieldId = parentId;
            Override = true;
        }

        private void ResetParentAndOverride()
        {
            ParentFieldId = null;
            Override = false;
        }

        private void SaveChildren(bool wasNewBeforeUpdate)
        {
            var fieldIds = ForceChildFieldIds != null ? new Queue<int>(ForceChildFieldIds) : new Queue<int>();
            var linkIds = ForceChildLinkIds != null ? new Queue<int>(ForceChildLinkIds) : new Queue<int>();
            var virtualIds = ForceVirtualFieldIds != null ? new List<int>(ForceVirtualFieldIds) : new List<int>();
            var newVirtualFieldIds = new List<int>();
            var newChildLinkIds = new List<int>();
            var newChildFieldIds = new List<int>();
            if (wasNewBeforeUpdate)
            {
                var contents = Content.ChildContents;
                foreach (var content in contents)
                {
                    var childField = _fieldRepository.GetByName(content.Id, Name);
                    if (childField != null)
                    {
                        childField.SetParentAndOverride(Id);
                        childField.PersistWithVirtualRebuild();
                    }
                    else
                    {
                        childField = CloneForHierarchy(content);
                        if (fieldIds.Any())
                        {
                            childField.ForceId = fieldIds.Dequeue();
                        }

                        childField.ResultChildLinkIds = ResultChildLinkIds;
                        if (childField.ExactType == FieldExactTypes.M2MRelation)
                        {
                            if (linkIds.Any())
                            {
                                childField.ContentLink.ForceLinkId = linkIds.Dequeue();
                            }

                            childField.SaveContentLink();
                            if (childField.LinkId != null)
                            {
                                newChildLinkIds.Add(childField.LinkId.Value);
                            }
                        }

                        var orderField = _fieldRepository.GetByOrder(ContentId, Order + 1);
                        childField.Order = orderField?.GetChild(content.Id).Order - 1 ?? content.GetMaxFieldsOrder();
                        childField.ForceChildFieldIds = fieldIds.ToArray();
                        childField.ForceChildLinkIds = linkIds.ToArray();
                        childField.ForceVirtualFieldIds = virtualIds.ToArray();

                        childField = childField.PersistWithVirtualRebuild(true);

                        virtualIds = childField.ForceVirtualFieldIds?.ToList() ?? new List<int>();
                        newChildFieldIds.Add(childField.Id);

                        if (childField.ResultChildFieldIds != null)
                        {
                            newChildFieldIds.AddRange(childField.ResultChildFieldIds);
                        }

                        if (childField.ResultChildLinkIds != null)
                        {
                            newChildLinkIds.AddRange(childField.ResultChildLinkIds);
                        }

                        if (childField.NewVirtualFieldIds != null)
                        {
                            newVirtualFieldIds.AddRange(childField.NewVirtualFieldIds);
                        }
                    }
                }
            }
            else
            {
                foreach (var childField in ChildFields.Where(n => !n.Override))
                {
                    var newChildField = CopyToChild(childField);
                    if (newChildField.ExactType == FieldExactTypes.M2MRelation)
                    {
                        if (linkIds.Any())
                        {
                            newChildField.ContentLink.ForceLinkId = linkIds.Dequeue();
                        }

                        newChildField.SaveContentLink();
                        if (childField.LinkId != newChildField.LinkId)
                        {
                            if (newChildField.LinkId == null)
                            {
                                throw new ArgumentException(@"Wrong input data");
                            }

                            newChildLinkIds.Add(newChildField.LinkId.Value);
                        }
                    }

                    newChildField.PersistWithVirtualRebuild();
                    if (newChildField.ResultChildLinkIds != null)
                    {
                        newChildLinkIds.AddRange(newChildField.ResultChildLinkIds);
                    }
                }
            }

            ResultChildFieldIds = newChildFieldIds.ToArray();
            ResultChildLinkIds = newChildLinkIds.ToArray();
            NewVirtualFieldIds = newVirtualFieldIds.ToArray();
            ForceVirtualFieldIds = virtualIds.ToArray();
        }

        private void RemoveChildren()
        {
            foreach (var field in ChildFields)
            {
                if (field.Override)
                {
                    field.ResetParentAndOverride();
                    field.PersistWithVirtualRebuild();
                }
                else
                {
                    field.Die(true, true);
                }
            }
        }

        private Field GetChild(int contentId)
        {
            return ChildFields.SingleOrDefault(n => n.ContentId == contentId);
        }

        private Field CopyToChild(Field childField)
        {
            var result = CloneForHierarchy(childField.Content);
            result.Id = childField.Id;
            result.Order = childField.Order;

            if (result.ExactType == FieldExactTypes.M2MRelation)
            {
                if (result.ContentLink.LContentId == childField.ContentLink.LContentId && result.ContentLink.RContentId == childField.ContentLink.RContentId)
                {
                    result.ContentLink = childField.ContentLink;
                }
                else
                {
                    result.ContentLink.NetLinkName = childField.ContentLink.NetLinkName;
                    result.ContentLink.NetPluralLinkName = childField.ContentLink.NetPluralLinkName;
                    result.ContentLink.MapAsClass = childField.ContentLink.MapAsClass;
                }
            }

            if (childField.Constraint != null && result.Constraint != null)
            {
                result.Constraint.Id = childField.Constraint.Id;
            }

            if (result.ExactType == FieldExactTypes.DynamicImage)
            {
                result.DynamicImage.Id = childField.Id;
            }

            return result;
        }

        private Field CloneForHierarchy(Content destContent)
        {
            var result = (Field)MemberwiseClone();
            result.ContentId = destContent.Id;
            result.Content = destContent;
            result.Init();
            result.LoadVeBindings();
            result.Id = 0;
            result.ParentFieldId = Id;
            result.ParentField = this;

            if (ExactType == FieldExactTypes.M2MRelation)
            {
                result.ContentLink = ContentLink.Clone(ContentId, destContent.Id);
                if (result.ContentLink.LContentId == result.ContentLink.RContentId)
                {
                    result.DefaultArticleIds = Enumerable.Empty<int>().ToArray();
                }
            }
            else if (Relation != null && Relation.ContentId == ContentId)
            {
                result.RelationId = Relation.GetChild(destContent.Id).Id;
                result.DefaultValue = null;
            }
            else if (BackRelation != null && BackRelation.ContentId == ContentId)
            {
                result.BackRelationId = BackRelation.GetChild(destContent.Id).Id;
            }
            else if (ExactType == FieldExactTypes.DynamicImage)
            {
                result.DynamicImage = DynamicImage.Clone(result);
                var baseImage = Content.Fields.Single(n => BaseImageId != null && n.Id == BaseImageId.Value);
                result.BaseImageId = baseImage.GetChild(destContent.Id).Id;
            }

            if (Constraint != null)
            {
                var cc = new ContentConstraint { ContentId = destContent.Id };
                foreach (var rule in Constraint.Rules)
                {
                    var childField = rule.Field.GetChild(destContent.Id);
                    var fieldId = childField?.Id ?? 0;
                    cc.Rules.Add(new ContentConstraintRule { FieldId = fieldId });
                }

                result.Constraint = cc;
            }

            return result;
        }

        /// <summary>
        /// Сохранение поля и Rebuild виртуальных контентов
        /// </summary>
        /// <param name="explicitOrder"></param>
        /// <returns></returns>
        public Field PersistWithVirtualRebuild(bool explicitOrder = false)
        {
            using (VirtualFieldRepository.LoadVirtualFieldsRelationsToMemory(ContentId))
            {
                var result = Persist(explicitOrder);
                if (RebuildVirtualContents)
                {
                    var helper = new VirtualContentHelper(result.ForceVirtualFieldIds.ToList());
                    helper.UpdateVirtualFields(result);
                    result.NewVirtualFieldIds = result.NewVirtualFieldIds?.Concat(helper.NewFieldIds).ToArray() ?? helper.NewFieldIds;
                    result.ForceVirtualFieldIds = helper.ForceNewFieldIds;
                }

                return result;
            }
        }

        /// <summary>
        /// Простое сохранение поля в базу
        /// </summary>
        public Field Persist(bool explicitOrder = false)
        {
            if (ExactType != FieldExactTypes.O2MRelation)
            {
                Aggregated = false;
                ClassifierId = null;
            }

            SetRelationDefaultValue();

            var isNew = IsNew;
            Field result;
            if (IsNew)
            {
                result = _fieldRepository.CreateNew(this, explicitOrder);
            }
            else
            {
                result = _fieldRepository.Update(this);
                CorrectEnumInContentData();
            }

            result.ActiveVeStyleIds = ActiveVeStyleIds;
            result.ActiveVeCommandIds = ActiveVeCommandIds;
            result.DefaultArticleIds = DefaultArticleIds;
            result.SaveM2MDefaultArticles();
            result.SaveVisualEditorCommands();
            result.SaveVisualEditorStyles();

            result.ForceChildFieldIds = ForceChildFieldIds;
            result.ForceChildLinkIds = ForceChildLinkIds;
            result.ForceVirtualFieldIds = ForceVirtualFieldIds;
            result.SaveChildren(isNew);

            return result;
        }

        private void SaveBackwardFields()
        {
            if (ExactType == FieldExactTypes.O2MRelation && O2MBackwardField != null && O2MBackwardField.IsNew)
            {
                O2MBackwardField.BackRelationId = Id;
                if (ForceBackwardId != 0)
                {
                    O2MBackwardField.ForceId = ForceBackwardId;
                }
                O2MBackwardField.PersistWithVirtualRebuild();
            }

            if (ExactType == FieldExactTypes.M2MRelation && M2MBackwardField != null && M2MBackwardField.IsNew)
            {
                M2MBackwardField.LinkId = LinkId;
                if (ForceBackwardId != 0)
                {
                    M2MBackwardField.ForceId = ForceBackwardId;
                }
                M2MBackwardField.ContentLink = ContentLink.GetBackwardLink();
                M2MBackwardField.PersistWithVirtualRebuild();
            }
        }

        private void SaveVisualEditorCommands()
        {
            if (ActiveVeCommandIds != null)
            {
                var oldCommands = VisualEditorRepository.GetResultCommands(Id, Content.SiteId)
                    .ToDictionary(s => s.Id, s => s.On);

                var activeCommandIdsSet = new HashSet<int>(ActiveVeCommandIds);

                var newCommands = oldCommands.Keys
                    .Union(ActiveVeCommandIds)
                    .ToDictionary(id => id, id => activeCommandIdsSet.Contains(id));

                var changedCommands = newCommands.Keys
                    .Where(id => !oldCommands.ContainsKey(id) || oldCommands[id] != newCommands[id])
                    .ToDictionary(id => id, id => newCommands[id]);

                var defaultCommands = VisualEditorRepository.GetDefaultCommands()
                    .ToDictionary(s => s.Id, s => s.On);

                var siteCommands = VisualEditorRepository.GetSiteCommands(Content.SiteId)
                    .ToDictionary(s => s.Id, s => s.On);

                VisualEditorRepository.SetFieldCommands(Content.SiteId, Id, changedCommands, defaultCommands, siteCommands);
            }
        }

        public void SaveM2MDefaultArticles()
        {
            if (DefaultArticleIds != null)
            {
                _fieldRepository.SetFieldM2MDefValue(Id, DefaultArticleIds.ToArray());
            }
        }

        internal void SaveVisualEditorStyles()
        {
            if (ActiveVeStyleIds != null)
            {
                var oldStyles = VisualEditorRepository.GetResultStyles(Id, Content.SiteId)
                    .ToDictionary(s => s.Id, s => s.On);

                var activeStyleIdsSet = new HashSet<int>(ActiveVeStyleIds);

                var newStyles = oldStyles.Keys
                    .Union(ActiveVeStyleIds)
                    .ToDictionary(id => id, id => activeStyleIdsSet.Contains(id));

                var changedStyles = newStyles.Keys
                    .Where(id => !oldStyles.ContainsKey(id) || oldStyles[id] != newStyles[id])
                    .ToDictionary(id => id, id => newStyles[id]);

                var defaultStyles = VisualEditorRepository.GetAllStyles()
                    .ToDictionary(s => s.Id, s => s.On);

                var siteStyles = VisualEditorRepository.GetSiteStyles(Content.SiteId)
                    .ToDictionary(s => s.Id, s => s.On);

                VisualEditorRepository.SetFieldStyles(Content.SiteId, Id, changedStyles, defaultStyles, siteStyles);
            }
        }

        internal void SaveContentLink()
        {
            if (ExactType == FieldExactTypes.M2MRelation && ContentLink != null)
            {
                var isNew = ContentLink.IsNew;
                ContentLink = isNew ? _contentRepository.SaveLink(ContentLink) : _contentRepository.UpdateLink(ContentLink);
                LinkId = ContentLink.LinkId;
            }
        }

        /// <summary>
        /// Обновляет значения поля StringEnum в существующих статьях, в соответствии со составом перечисления
        /// </summary>
        private void CorrectEnumInContentData()
        {
            if (!IsNew && ExactType == FieldExactTypes.StringEnum)
            {
                _fieldRepository.CorrectEnumInContentData(this);
            }
        }

        private void SetRelationDefaultValue()
        {
            if (ExactType == FieldExactTypes.M2MRelation)
            {
                DefaultValue = LinkId?.ToString();
            }
            else if (ExactType == FieldExactTypes.M2ORelation)
            {
                DefaultValue = BackRelationId?.ToString();
            }
        }

        public IEnumerable<ListItem> GetRelatedTitles(string value)
        {
            if (!string.IsNullOrEmpty(value) && RelateToContentId != null)
            {
                var ids = value.Split(",".ToCharArray()).Select(int.Parse).ToArray();
                return ArticleRepository.GetSimpleList(RelateToContentId.Value, null, Id, ListSelectionMode.OnlySelectedItems, ids, null, 0);
            }

            return null;
        }

        public Field GetBaseField(int articleId)
        {
            if (!Content.IsVirtual)
            {
                return this;
            }

            int oldId, newId = Id;
            do
            {
                oldId = newId;
                newId = _fieldRepository.GetBaseFieldId(newId, articleId);
            } while (newId != oldId);

            return _fieldRepository.GetById(newId);
        }

        public List<Field> GetDynamicImages() => _fieldRepository.GetDynamicImageFields(ContentId, Id);

        public static IEnumerable<MaskTemplate> GetAllMaskTemplates() => MaskTemplateRepository.GetAllMaskTemplates();

        /// <summary>
        /// Обновляет порядок полей контента
        /// </summary>
        internal void ReorderContentFields(bool forDelete = false)
        {
            if (Content.Fields.Any())
            {
                if (forDelete)
                {
                    var lastOrder = Content.GetMaxFieldsOrder();
                    _fieldRepository.UpdateContentFieldsOrders(ContentId, Order, lastOrder);

                }
                else if (IsNew)
                {
                    var lastOrder = Content.GetMaxFieldsOrder();
                    if (Order != lastOrder)
                    {
                        _fieldRepository.UpdateContentFieldsOrders(ContentId, -1, Order);
                    }

                    Order++;
                }
                else
                {
                    var currentOrder = _fieldRepository.GetById(Id).Order;

                    // Если позиция поля поменялась - то выполняем reorder
                    if (currentOrder != Order)
                    {
                        _fieldRepository.UpdateContentFieldsOrders(ContentId, currentOrder, Order);
                        if (currentOrder > Order)
                        {
                            Order++;
                        }
                    }
                }
            }
        }

        internal void MutateLinqBackPropertyName()
        {
            if (ExactType == FieldExactTypes.O2MRelation)
            {
                if (!string.IsNullOrEmpty(LinqBackPropertyName))
                {
                    var name = LinqBackPropertyName;
                    var index = 0;
                    do
                    {
                        index++;
                        LinqBackPropertyName = MutateHelper.MutateNetName(name, index);
                    } while (_fieldRepository.LinqBackPropertyNameExists(this));
                }
            }
        }

        internal void MutateLinqPropertyName()
        {
            if (!string.IsNullOrEmpty(LinqPropertyName))
            {
                var name = LinqPropertyName;
                var index = 0;
                do
                {
                    index++;
                    LinqPropertyName = MutateHelper.MutateNetName(name, index);
                } while (_fieldRepository.LinqPropertyNameExists(this));
            }
        }

        internal void MutateName()
        {
            var name = Name;
            var index = 0;
            do
            {
                index++;
                Name = MutateHelper.MutateTitle(name, index);
            } while (_fieldRepository.NameExists(this));
        }

        internal void MutateNames()
        {
            MutateName();
            MutateLinqPropertyName();
            MutateLinqBackPropertyName();
        }

        internal Field GetVirtualClone(Content virtualContent)
        {
            var result = Create(virtualContent, _fieldRepository, _contentRepository);

            result.Name = Name;
            result.TypeId = TypeId;
            result.RelationId = RelationId;
            result.BaseImageId = BaseImageId;
            result.UseSiteLibrary = UseSiteLibrary;
            result.LinkId = LinkId;
            result.ViewInList = ViewInList;
            result.BackRelationId = BackRelationId;
            result.IsLong = IsLong;
            result.UseForTree = UseForTree;
            result.AutoCheckChildren = AutoCheckChildren;
            result.IsClassifier = IsClassifier;
            result.ExactType = ExactType;
            result.Size = Size;
            result.TextBoxRows = TextBoxRows;
            result.VisualEditorHeight = VisualEditorHeight;
            result.DecimalPlaces = DecimalPlaces;
            result.StringSize = StringSize;

            return result;
        }

        /// <summary>
        /// Возвращает виртуальное поле на основе данного поля
        /// </summary>
        internal Field GetVirtualCloneForJoin(Content virtualContent, Field parentVirtualField)
        {
            var result = GetVirtualClone(virtualContent);
            result.PersistentId = Id;

            if (parentVirtualField != null)
            {
                result.Name = string.Concat(parentVirtualField.Name, '.', Name);
                result.JoinId = parentVirtualField.Id;
            }
            else
            {
                result.Name = Name;
            }

            return result;
        }

        public string GetRelationFilter(int articleId)
        {
            var databaseType = QPContext.DatabaseType;
            var escapedBackRelationName = ExactType != FieldExactTypes.M2ORelation
                ? null
                : SqlQuerySyntaxHelper.EscapeEntityName(databaseType, BackRelation.Name);
            return ExactType != FieldExactTypes.M2ORelation
                ? RelationFilter
                : $"(c.{escapedBackRelationName} = {articleId} OR c.{escapedBackRelationName} IS NULL) AND c.{SqlQuerySyntaxHelper.EscapeEntityName(databaseType, "ARCHIVE")} = 0";
        }

        public void ParseStringEnumJson(string json)
        {
            if (!string.IsNullOrWhiteSpace(json))
            {
                StringEnumItems = JsonConvert.DeserializeObject<StringEnumItem[]>(json) ?? Enumerable.Empty<StringEnumItem>();
            }
            else
            {
                StringEnumItems = Enumerable.Empty<StringEnumItem>();
            }
        }

        public IEnumerable<Content> GetAggregatedContents() => IsClassifier ? _fieldRepository.GetAggregatableContentsForClassifier(this) : null;

        [LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
        public int[] DefaultArticleIds
        {
            get => _defaultArticleIds.Value;
            set => _defaultArticleIds.Value = value;
        }

        public IEnumerable<ListItem> DefaultArticleListItems => !RelateToContentId.HasValue
            ? new List<ListItem>()
            : ArticleRepository.GetSimpleList(RelateToContentId.Value, 0, Id, ListSelectionMode.OnlySelectedItems, DefaultArticleIds.ToArray(), "", 0);

        public bool HasArticles => _fieldRepository.FieldHasArticles(ContentId);

        public static FieldExactTypes CreateExactType(int typeId, int? linkId, bool isClassifier, bool isStringEnum)
        {
            switch (typeId)
            {
                case FieldTypeCodes.String when isStringEnum:
                    return FieldExactTypes.StringEnum;
                case FieldTypeCodes.Numeric when isClassifier:
                    return FieldExactTypes.Classifier;
                case FieldTypeCodes.Relation when linkId.HasValue:
                    return FieldExactTypes.M2MRelation;
                default:
                    return (FieldExactTypes)typeId;
            }
        }

        public override string ToString() => $"{Id,6}: {Name}";

        public int FieldPriority(bool withRelations)
        {
            var result = 1;

            if (TypeId == FieldTypeCodes.Textbox || TypeId == FieldTypeCodes.VisualEdit)
            {
                result = withRelations ? 1 : 0;
            }

            if (TypeId == FieldTypeCodes.M2ORelation || IsClassifier || TypeId == FieldTypeCodes.Relation && !withRelations)
            {
                result = -1;
            }

            return result;

        }
    }
}
