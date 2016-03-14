using System;
using System.Collections.Generic;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;
using System.Linq;
using Quantumart.QP8.Utils;
using System.Collections.ObjectModel;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Validators;
using System.Threading;
using System.Text;
using Quantumart.QP8.BLL.Repository.Articles;
using System.Linq.Expressions;
using QA.Validation.Xaml.ListTypes;
using Quantumart.QP8.BLL.ListItems;
using System.Web.Script.Serialization;

namespace Quantumart.QP8.BLL
{
    public class Field : EntityObject
	{
		#region Static
		/// <summary>
		/// Траслирует SortExpression из Presentation в BLL
		/// </summary>
		/// <param name="sortExpression">SortExpression</param>
		/// <returns>SortExpression</returns>
		public new static string TranslateSortExpression(string sortExpression)
		{
			string result = EntityObject.TranslateSortExpression(sortExpression);
			Dictionary<string, string> replaces = new Dictionary<string, string>() { 
				{"Type", "Type.Name"},
				{"Indexed", "IndexFlag"} 

			};
			return TranslateHelper.TranslateSortExpression(result, replaces);
		}

		#region Comparation		
		public static IEqualityComparer<Field> IdComparer
		{
			get
			{
				return new LambdaEqualityComparer<Field>((f1, f2) => f1.Id == f2.Id, f => f.Id);
			}
		}			

		public static IEqualityComparer<Field> NameComparer
		{
			get
			{
				return new LambdaEqualityComparer<Field>(NameComparerPredicate, f => GetNameHashCode(f.Name));
			}
		}
			

		public static bool NameComparerPredicate(Field f1, Field f2)
		{
			return NameComparerPredicate(f1.Name, f2.Name);
		}

		public static bool NameComparerPredicate(string n1, string n2)
		{
			if (n1 == null && n2 == null)
				return true;
			else if ((n1 == null && n2 != null) || (n1 != null && n2 == null))
				return false;
			else
				return NameStringComparer.Equals(n1, n2);			
		}

		public static IEqualityComparer<string> NameStringComparer
		{
			get
			{
				return StringComparer.InvariantCultureIgnoreCase; // new LambdaComparer<string>(NameComparerPredicate, f => f.ToLowerInvariant().GetHashCode());
			}
		}	

		public static int GetNameHashCode(string name)
		{
			if (name == null)
				return 0;
			else
				return name.ToLowerInvariant().GetHashCode();
		}
		#endregion

		#endregion

		#region Constants & Readonly

		public const int TEXT_BOX_ROWS_DEFAULT_VALUE = 5;
		public const int VISUAL_EDITOR_HEIGHT_DEFAULT_VALUE = 450;
		public const int DECIMAL_PLACES_DEFAULT_VALUE = 2;
		public const int STRING_SIZE_DEFAULT_VALUE = 255;

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
		#endregion

		#region Creation
		public Field()
		{
			RebuildVirtualContents = true;
			FieldTitleCount = 1;
			StringEnumItems = Enumerable.Empty<StringEnumItem>();
		}		

		public Field(Content content)
			: this()
		{
			Content = content;
			ContentId = content.Id;			
		}
		#endregion

		#region Fields and Properties

		#region private fields

		private InitPropertyValue<Content> _Content;

		private InitPropertyValue<Field> _Relation;

		private InitPropertyValue<Field> _BackRelation;

		private InitPropertyValue<VisualEditFieldParams> _VisualEditFieldParams;

		private InitPropertyValue<IEnumerable<ExternalCss>> _ExternalCssItems;

		private InitPropertyValue<Field> _O2MBackwardField = null;

		private InitPropertyValue<Field> _M2MBackwardField = null;

		private InitPropertyValue<ContentLink> _ContentLink = null;

		private InitPropertyValue<ContentConstraint> _Constraint = null;

		private InitPropertyValue<DynamicImage> _DynamicImage;

		private InitPropertyValue<VisualEditorConfig> _VisualEditor;

		private InitPropertyValue<bool> _HasAnyAggregators = null;

		private InitPropertyValue<bool> _IsUnique = null;

		private InitPropertyValue<int?> _RelateToContentId = null;

		private InitPropertyValue<FieldExactTypes> _ExactType;

		private InitPropertyValue<PathInfo> _PathInfo;

		private InitPropertyValue<Field> _ParentField;

		private InitPropertyValue<IEnumerable<Field>> _ChildFields;

		private InitPropertyValue<int[]> _ActiveVeCommandIds;

		private InitPropertyValue<int[]> _ActiveVeStyleIds;

		private InitPropertyValue<int[]> _DefaultArticleIds;


		#endregion

		#region automatic mapped properties

		#region overrides

		/// <summary>
		/// название сущности
		/// </summary>			
		[LocalizedDisplayName("Name", NameResourceType = typeof(EntityObjectStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "NameMaxLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
		[RequiredValidator(MessageTemplateResourceName = "NameNotEntered", MessageTemplateResourceType = typeof(EntityObjectStrings))]
		[FormatValidator(Constants.RegularExpressions.InvalidFieldName, Negated=true, MessageTemplateResourceName = "NameInvalidFormat", MessageTemplateResourceType = typeof(EntityObjectStrings))]
		public override string Name
		{
			get;
			set;
		}

		/// <summary>
		/// описание сущности
		/// </summary>
		[LocalizedDisplayName("Description", NameResourceType = typeof(EntityObjectStrings))]
		[MaxLengthValidator(512, MessageTemplateResourceName = "DescriptionMaxLengthExceeded", MessageTemplateResourceType = typeof(EntityObjectStrings))]
		public override string Description
		{
			get;
			set;
		}

		public override string EntityTypeCode
		{
			get
			{
				return Constants.EntityTypeCode.Field;
			}
		}

		public override bool IsReadOnly
		{
			get
			{
				return ReadOnly;
			}
		}

		public override PathInfo PathInfo
		{
			get
			{
                return _PathInfo.Value;
			}
		}

		public override int ParentEntityId
		{
			get
			{
				return ContentId;
			}
		}
		#endregion

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
			get
			{
				return _ActiveVeCommandIds.Value;
			}
			set
			{
				_ActiveVeCommandIds.Value = value;
			}
		}

		public int[] ActiveVeStyleIds
		{
			get
			{
				return _ActiveVeStyleIds.Value;
			}
			set
			{
				_ActiveVeStyleIds.Value = value;
			}
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
			get
			{
				return VisualEditFieldParams.PEnterMode;
			}
			set
			{
				VisualEditFieldParams.PEnterMode = value;
			}
		}

		[LocalizedDisplayName("UseEnglishQuotes", NameResourceType = typeof(FieldStrings))]
		public bool UseEnglishQuotes
		{
			get
			{
				return VisualEditFieldParams.UseEnglishQuotes;
			}
			set
			{
				VisualEditFieldParams.UseEnglishQuotes = value;
			}
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
		[FormatValidator(Constants.RegularExpressions.RelativeWindowsFolderPath, MessageTemplateResourceName = "SubFolderInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
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
		[FormatValidator(Constants.RegularExpressions.NetName, MessageTemplateResourceName = "LinqPropertyNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "LinqPropertyNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
		public string LinqPropertyName { get; set; }

		[LocalizedDisplayName("LinqBackPropertyName", NameResourceType = typeof(FieldStrings))]
		[FormatValidator(Constants.RegularExpressions.NetName, MessageTemplateResourceName = "LinqBackPropertyNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
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

		internal bool HasAnyAggregators { get { return _HasAnyAggregators.Value; } }

		/// <summary>
		/// Значения строкового перечисления
		/// </summary>
		[LocalizedDisplayName("EnumValues", NameResourceType = typeof(FieldStrings))]
		public IEnumerable<StringEnumItem> StringEnumItems { get; set; }

		[LocalizedDisplayName("ShowAsRadioButtons", NameResourceType = typeof(FieldStrings))]
		public bool ShowAsRadioButtons { get; set; }

		[LocalizedDisplayName("UseForDefaultFiltration", NameResourceType = typeof(FieldStrings))]
		public bool UseForDefaultFiltration { get; set; }

		[LocalizedDisplayName("UseInChildContentFilter", NameResourceType = typeof(FieldStrings))]
		public bool UseInChildContentFilter { get; set; }		
		#endregion

        public Content Content { get; set; }

        public Field Relation
		{
			get
			{
                return _Relation.Value;
			}
		}

        public Field BackRelation
        {
            get
            {
                return _BackRelation.Value;
            }
			set
			{
				_BackRelation.Value = value;
			}
        }

		public string DisplayName
		{
			get
			{
				return (!String.IsNullOrEmpty(FriendlyName)) ? FriendlyName : Name;
			}

		}

		public string FormName
		{
			get
			{
				return Field.Prefix + Id;
			}
		}

		public string FormCheckboxName
		{
			get
			{
				return FormName + "_checkbox";
			}
		}

		public static int ParseFormName(string formName)
		{
			int value;
			return (Int32.TryParse(formName.Replace(Field.Prefix, ""), out value)) ? value : 0;
		}

		public string ParamName
		{
			get
			{
				return "@" + FormName;
			}
		}

		public string Default
		{
			get
			{
				return (IsBlob) ? DefaultBlobValue : DefaultValue;
			}

		}

		public bool IsBlob
		{
			get
			{
				return (TypeId == FieldTypeCodes.Textbox || TypeId == FieldTypeCodes.VisualEdit);
			}
		}


		public bool IsDateTime
		{
			get
			{
				return (TypeId == FieldTypeCodes.DateTime || TypeId == FieldTypeCodes.Date || TypeId == FieldTypeCodes.Time);
			}
		}

		public bool ReplaceUrls
		{
			get
			{
				return (ExactType == FieldExactTypes.String || IsBlob);
			}
		}

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
        
		public string RelationFilter
		{
			get
			{
				return SqlFilterComposer.Compose(UseRelationCondition ? "(" + RelationCondition + ")" : "", DefaultRelationFilter);
			}
		}

        [LocalizedDisplayName("FieldType", NameResourceType = typeof(FieldStrings))]
		public FieldExactTypes ExactType
		{
			get
			{
				return _ExactType.Value;
			}
			set
			{
				_ExactType.Value = value;
			}
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
				};
			}
		}
		

		[LocalizedDisplayName("IsUnique", NameResourceType = typeof(FieldStrings))]
		public bool IsUnique
		{
			get
			{
				return _IsUnique.Value;
			}
			set
			{
				_IsUnique.Value = value;
			}
		}

		[LocalizedDisplayName("RelateTo", NameResourceType = typeof(FieldStrings))]
		public int? RelateToContentId
		{
			get
			{
				return _RelateToContentId.Value;
			}
			set
			{
				_RelateToContentId.Value = value;
			}
		}
		
		public Content RelatedToContent
		{
			get
			{
				if (RelateToContentId.HasValue)
					return ContentRepository.GetById(RelateToContentId.Value);
				else
					return null;
			}
		}

		[LocalizedDisplayName("TextBoxRows", NameResourceType = typeof(FieldStrings))]
		public int TextBoxRows { get; set; }

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
			get
			{
				return _DynamicImage.Value;
			}
			set
			{
				_DynamicImage.Value = value;
			}
		}
        
        public VisualEditorConfig VisualEditor
		{
			get
			{
                return _VisualEditor.Value;
			}
		}

		public bool UseVersionControl
		{
			get
			{
				return !DisableVersionControl && Content.MaxNumOfStoredVersions != 0;
			}
		}

		public Type CLRType
		{
			get
			{
				Type clrType = typeof(object);

				if (Name == Constants.FieldTypeName.String)
				{
					clrType = typeof(string);
				}
				else if (Name == Constants.FieldTypeName.Numeric)
				{
					if (this.Required)
					{
						clrType = typeof(decimal);
					}
					else
					{
						clrType = typeof(decimal?);
					}
				}
				else if (Name == Constants.FieldTypeName.Boolean)
				{
					if (this.Required)
					{
						clrType = typeof(bool);
					}
					else
					{
						clrType = typeof(bool?);
					}
				}
				else if (Name == Constants.FieldTypeName.Date)
				{
					clrType = typeof(DateTime);
				}
				else if (Name == Constants.FieldTypeName.Time)
				{
					clrType = typeof(DateTime);
				}
				else if (Name == Constants.FieldTypeName.DateTime)
				{
					clrType = typeof(DateTime);
				}
				else if (Name == Constants.FieldTypeName.File)
				{
					clrType = typeof(string);
				}
				else if (Name == Constants.FieldTypeName.Image)
				{
					clrType = typeof(string);
				}
				else if (Name == Constants.FieldTypeName.Textbox)
				{
					clrType = typeof(string);
				}
				else if (Name == Constants.FieldTypeName.VisualEdit)
				{
					clrType = typeof(string);
				}
                else if (Name == Constants.FieldTypeName.Relation || Name == Constants.FieldTypeName.M2ORelation)
				{
					if (this.Required)
					{
						clrType = typeof(decimal);
					}
					else
					{
						clrType = typeof(decimal?);
					}
				}
				else if (Name == Constants.FieldTypeName.DynamicImage)
				{
					clrType = typeof(string);
				}


				return clrType;
			}
		}

		public int LibraryEntityId
		{
			get
			{
				if (UseSiteLibrary)
					return Content.SiteId;
				else
					return ContentId;
			}
		}

		public int LibraryParentEntityId
		{
			get
			{
				if (UseSiteLibrary)
					return 0;
				else
					return Content.SiteId;
			}
		}

		public ContentConstraint Constraint
		{
			get
			{
                return _Constraint.Value;
			}
			set
			{
                _Constraint.Value = value;
			}
		}

		public ContentLink ContentLink
		{
			get
			{
                return _ContentLink.Value;
			}
			set
			{
                _ContentLink.Value = value;
			}
		}

		/// <summary>
		/// Обратное поле
		/// </summary>
		public Field M2MBackwardField
		{
			get { return _M2MBackwardField.Value; }
			set { _M2MBackwardField.Value = value; }
		}

        /// <summary>
        /// Обратное поле
        /// </summary>
        public Field O2MBackwardField
        {
            get { return _O2MBackwardField.Value; }
            set { _O2MBackwardField.Value = value; }
        }

		/// <summary>
		/// Существует ли для данного поля обратное поле ?
		/// </summary>
		public bool IsBackwardFieldExists
		{
			get { return BackwardField != null && !BackwardField.IsNew; }
		}

		public Field BackwardField
		{
			get { return (M2MBackwardField != null) ? M2MBackwardField : O2MBackwardField; }
		}

		/// <summary>
		/// Поля которые ссылаются на данное поле связью O2M
		/// </summary>
		public IEnumerable<Field> RelatedO2MFields
		{
			get
			{
				if (IsNew)
					return Enumerable.Empty<Field>();

				return FieldRepository.GetRelatedO2MFields(Id);
			}
		}

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
			get
			{
				return VisualEditFieldParams.RootElementClass;
			}
			set
			{
				VisualEditFieldParams.RootElementClass = value;
			}
		}

		public string ExternalCss
		{
			get
			{
				return VisualEditFieldParams.ExternalCss;
			}
			set
			{
				VisualEditFieldParams.ExternalCss = value;
			}
		}

		[LocalizedDisplayName("ExternalCss", NameResourceType = typeof(VisualEditorStrings))]
		public IEnumerable<ExternalCss> ExternalCssItems
		{
			get { return _ExternalCssItems.Value; }
			set { _ExternalCssItems.Value = value; }
		}

		public string O2MDefaultValueName 
		{ 
			get
			{
				string result = String.Empty;
				int parsedArticleId = 0;
				if (Relation != null && Int32.TryParse(O2MDefaultValue, out parsedArticleId))
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
				string result = String.Empty;
				int parsedArticleId = 0;
				if (Relation != null && Int32.TryParse(M2MDefaultValue, out parsedArticleId))
				{
					result = ArticleRepository.GetFieldValue(parsedArticleId, Relation.ContentId, Relation.Name);
				}
				return result;
			}
		}

		[LocalizedDisplayName("BackwardFieldName", NameResourceType = typeof(FieldStrings))]
		[FormatValidator(Constants.RegularExpressions.InvalidEntityName, Negated = true, MessageTemplateResourceName = "BackwardFieldNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
		[MaxLengthValidator(255, MessageTemplateResourceName = "BackwardFieldNameMaxLengthExceeded", MessageTemplateResourceType = typeof(FieldStrings))]
		public string NewM2MBackwardFieldName { get; set; }

        [LocalizedDisplayName("BackwardFieldName", NameResourceType = typeof(FieldStrings))]
        [FormatValidator(Constants.RegularExpressions.InvalidEntityName, Negated = true, MessageTemplateResourceName = "BackwardFieldNameInvalidFormat", MessageTemplateResourceType = typeof(FieldStrings))]
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
					return true;
				else if ((Name == null && StoredName != null) || (Name != null && StoredName == null))
					return false;
				else
					return !StoredName.Equals(Name, StringComparison.InvariantCultureIgnoreCase); // игнорируем регист букв!
			} 
		}

		internal VisualEditFieldParams VisualEditFieldParams
		{
			get
			{
                return _VisualEditFieldParams.Value;
			}
		}

		/// <summary>
		/// Могут ли поля O2M ссылаться на это поле 
		/// </summary>
		internal bool IsRelateable
		{
			get
			{
				return (this.ExactType != FieldExactTypes.M2MRelation && this.ExactType != FieldExactTypes.M2ORelation);
			}
		}

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

		[LocalizedDisplayName("UseForVariations", NameResourceType = typeof(FieldStrings))]
		public bool UseForVariations { get; set; }

		public IEnumerable<Field> ChildFields { get { return _ChildFields.Value; } }

		public Field ParentField
		{
			get
			{
				return _ParentField.Value;
			}

			set
			{
				_ParentField.Value = value;
			}
		}

	    public FieldTypeInfo TypeInfo { get; private set; }

		#endregion

		#region  Methods

		#region Initialize Model

		private ContentLink GetContentLinkForInit()
		{
			if (ExactType != FieldExactTypes.M2MRelation || !LinkId.HasValue)
				return new ContentLink { LContentId = ContentId };
			else
				return ContentRepository.GetContentLinkById(LinkId.Value);
		}

		private ContentConstraint GetConstraintForInit()
		{
			return (IsNew) ? null : ContentConstraintRepository.GetConstraintByFieldId(Id);
		}

		private Content GetContentForInit()
		{
			return ContentRepository.GetById(ContentId);
		}

		private DynamicImage GetDynamicImageForInit()
		{
			return (!IsNew && TypeId == FieldTypeCodes.DynamicImage) ? DynamicImage.Load(this) : null;
		}

		private Field GetRelationForInit()
		{
			return (RelationId.HasValue) ? FieldRepository.GetById(RelationId.Value) : null;
		}

		private Field GetBackRelationForInit()
		{
			return (BackRelationId.HasValue) ? FieldRepository.GetById(BackRelationId.Value) : null;
		}

		private bool GetIsUniqueForInit()
		{
			return UniqueFieldTypes.Contains(ExactType) && Constraint != null;
		}

		private bool GetIsUniqueForSet(bool value)
		{
			return value && UniqueFieldTypes.Contains(ExactType);
		}

		private int? GetRelateToForInit()
		{
			int? result = null;
			if (!IsNew)
			{
				if (ExactType == FieldExactTypes.O2MRelation)
				{
					result = (Relation != null ? Relation.ContentId : (int?)null);
				}
				else if (ExactType == FieldExactTypes.M2ORelation)
				{
					result = (BackRelation != null ? BackRelation.ContentId : (int?)null);
				}
				else if (ExactType == FieldExactTypes.M2MRelation && !ContentLink.IsNew)
				{
					if (IsBackwardFieldExists)
					{
						if (ContentLink.LContentId == ContentId && ContentLink.RContentId == ContentId)
							throw new ApplicationException("Обратное поле создано для того же контента.");
					}

					Field field = this;
					if (this.Content.IsVirtual)
					{
						var baseFieldId = VirtualFieldRepository.GetRealBaseFieldIds(this.Id).First();
						field = FieldRepository.GetById(baseFieldId);
					}
					if (field.ContentLink.LContentId == field.ContentId)
						result = field.ContentLink.RContentId;
					else if (field.ContentLink.RContentId == field.ContentId)
						result = field.ContentLink.LContentId;

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

				if (this.IsNew || ContentLink == null || ContentLink.IsNew || LinkId == null)
					return null;

				if (ContentLink.LContentId == ContentLink.RContentId && ContentLink.LContentId == ContentId) // поле связано со своим контентом
					return null;
				else if (ContentLink.LContentId == ContentId)
					checkedContentId = ContentLink.RContentId;
				else if (ContentLink.RContentId == ContentId)
					checkedContentId = ContentLink.LContentId;

				var rContent = ContentRepository.GetById(checkedContentId);
				return rContent.Fields.Where(f => f.LinkId == this.LinkId && f.Id != this.Id).SingleOrDefault();
			}
			else
				return null;
		}

		private Field GetO2MBackwardForInit()
		{
			return FieldRepository.GetByBackRelationId(Id);
		}

		private FieldExactTypes GetExactTypeForInit()
		{
			FieldExactTypes result;
			if (TypeId == FieldTypeCodes.Relation)
			{
				if (LinkId.HasValue)
					result = FieldExactTypes.M2MRelation;
				else
					result = FieldExactTypes.O2MRelation;
			}
			else if (TypeId == FieldTypeCodes.Numeric && IsClassifier)
				result = FieldExactTypes.Classifier;
			else if (TypeId == FieldTypeCodes.String && StringEnumItems.Any())
				result = FieldExactTypes.StringEnum;
			else
				result = (FieldExactTypes)TypeId;

			TypeInfo = new FieldTypeInfo(result, LinkId ?? BackRelationId);

			return result;
		}

		private FieldExactTypes GetExactTypeForSet(FieldExactTypes value)
		{
			if (value == FieldExactTypes.O2MRelation || value == FieldExactTypes.M2MRelation)
				TypeId = FieldTypeCodes.Relation;
			else if (value == FieldExactTypes.Classifier)
				TypeId = FieldTypeCodes.Numeric;
			else if (value == FieldExactTypes.StringEnum)
				TypeId = FieldTypeCodes.String;
			else
				TypeId = (int)value;

			TypeInfo = new FieldTypeInfo(value, LinkId ?? BackRelationId);

			return value;
		}

		private Field GetParentFieldForInit()
		{
			return (ParentFieldId.HasValue) ? FieldRepository.GetById(ParentFieldId.Value) : null;
		}

		private IEnumerable<Field> GetChildFieldsForInit()
		{
			return FieldRepository.GetChildList(Id);
		}

		private bool GetHasAnyAggregatorsForInit()
		{
			return FieldRepository.HasAnyAggregators(Id);
		}

		private IEnumerable<ExternalCss> GetExternalCssItemsForInit()
		{
			return ExternalCssHelper.GenerateExternalCss(ExternalCss);
		}

		private PathInfo GetPathInfoForInit()
		{
			var pathInfo = (UseSiteLibrary) ? Content.Site.PathInfo : Content.PathInfo;
			if (!String.IsNullOrEmpty(SubFolder))
			{
				pathInfo = pathInfo.GetSubPathInfo(SubFolder);
			}
			return pathInfo;
		}

		private VisualEditFieldParams GetVisualEditFieldParamsForInit()
        {
            return (IsNew) ? new VisualEditFieldParams(Content.Site) : FieldRepository.GetVisualEditFieldParams(Id);
        }

		private VisualEditorConfig GetVisualEditorConfigForInit()
		{
			return new VisualEditorConfig(this);
		}

		public Field Init()
		{
 			_ExactType = new InitPropertyValue<FieldExactTypes>(() => GetExactTypeForInit(), (value) => GetExactTypeForSet(value));


			_Content = new InitPropertyValue<Content>(() => GetContentForInit());
            _Constraint = new InitPropertyValue<ContentConstraint>(() => GetConstraintForInit());
            _ContentLink = new InitPropertyValue<ContentLink>(() => GetContentLinkForInit());
            _Relation = new InitPropertyValue<Field>(() => GetRelationForInit());
            _BackRelation = new InitPropertyValue<Field>(() => GetBackRelationForInit());

            if (IsNew)
			{
				Indexed = (ExactType == FieldExactTypes.O2MRelation);
				OnScreen = (ExactType == FieldExactTypes.String || ExactType == FieldExactTypes.Textbox || ExactType == FieldExactTypes.VisualEdit);

				TextBoxRows = TEXT_BOX_ROWS_DEFAULT_VALUE;
				VisualEditorHeight = VISUAL_EDITOR_HEIGHT_DEFAULT_VALUE;
				StringSize = STRING_SIZE_DEFAULT_VALUE;
				DecimalPlaces = DECIMAL_PLACES_DEFAULT_VALUE;
				UseInputMask = false;
			}

			InitDefaultValues();
			InitOrderBy();
			IsInteger = IsNew || (ExactType == Constants.FieldExactTypes.Numeric && DecimalPlaces == 0);
			UseRelationCondition = IsNew ? false : !String.IsNullOrEmpty(RelationCondition);

			_DynamicImage = new InitPropertyValue<BLL.DynamicImage>(() => GetDynamicImageForInit());
			_M2MBackwardField = new InitPropertyValue<Field>(() => GetM2MBackwardForInit());
			_O2MBackwardField = new InitPropertyValue<Field>(() => GetO2MBackwardForInit());
			_RelateToContentId = new InitPropertyValue<int?>(() => GetRelateToForInit());
			_IsUnique = new InitPropertyValue<bool>(() => GetIsUniqueForInit(), (value) => GetIsUniqueForSet(value));
            _PathInfo = new InitPropertyValue<PathInfo>(() => GetPathInfoForInit());
			_VisualEditor = new InitPropertyValue<VisualEditorConfig>(() => GetVisualEditorConfigForInit());
			_VisualEditFieldParams = new InitPropertyValue<VisualEditFieldParams>(() => GetVisualEditFieldParamsForInit());
			_ExternalCssItems = new InitPropertyValue<IEnumerable<ExternalCss>>(() => GetExternalCssItemsForInit());
			_HasAnyAggregators = new InitPropertyValue<bool>(() => GetHasAnyAggregatorsForInit());
			_ParentField = new InitPropertyValue<Field>(() => GetParentFieldForInit());
			_ChildFields = new InitPropertyValue<IEnumerable<Field>>(() => GetChildFieldsForInit());
			_ActiveVeCommandIds = new InitPropertyValue<int[]>(() => GetActiveVeCommandIds());
			_ActiveVeStyleIds = new InitPropertyValue<int[]>(() => GetActiveVeStyleIds());
			_DefaultArticleIds = new InitPropertyValue<int[]>(() => GetDefaultArticleIds());
			return this;
		}

		private int[] GetActiveVeStyleIds()
		{
			return VisualEditorRepository.GetResultStyles(Id, Content.SiteId).Where(n => n.On).Select(n => n.Id).ToArray();
		}

		private int[] GetDefaultArticleIds()
		{
			if (ExactType == FieldExactTypes.M2MRelation)
			{
				return FieldRepository.GetActiveArticlesForM2mField(Id).Select(x => x.Id).ToArray();
			}
			else
			{
				return new int[] { };
			}
		}

		private int[] GetActiveVeCommandIds()
		{
			return VisualEditorRepository.GetResultCommands(Id, Content.SiteId).Where(n => n.On).Select(n => n.Id).ToArray();
		}

		internal void LoadVeBindings()
		{
			var ids = ActiveVeCommandIds;
			var ids2 = ActiveVeStyleIds;
		}

		private void InitDefaultValues()
		{
            if (ExactType != FieldExactTypes.M2ORelation)
			{
				StringDefaultValue = DefaultValue;
				NumericDefaultValue = Converter.ToNullableDouble(DefaultValue);
				BooleanDefaultValue = Converter.ToBoolean(DefaultValue, false);
				DateDefaultValue = Converter.ToNullableDateTime(DefaultValue);
				var tdt = Converter.ToNullableDateTime(DefaultValue);
				TimeDefaultValue = tdt.HasValue ? tdt.Value.TimeOfDay : (TimeSpan?)null;
				DateTimeDefaultValue = Converter.ToNullableDateTime(DefaultValue);
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
					FileDefaultValue = DefaultValue;
				else if (ExactType == FieldExactTypes.O2MRelation)
					O2MDefaultValue = DefaultValue;
				else if (ExactType == FieldExactTypes.Classifier)
					ClassifierDefaultValue = Converter.ToNullableInt32(DefaultValue);

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
		#endregion

		#region Update Model
		public void UpdateModel()
		{			
			if (ExactType == FieldExactTypes.Numeric && IsInteger)
				DecimalPlaces = 0;

			if (ExactType == FieldExactTypes.Classifier)
			{
				IsClassifier = true;
				Indexed = true;
				IsInteger = true;
				DecimalPlaces = 0;
			}
			else
				IsClassifier = false;

			if (ExactType == FieldExactTypes.Boolean || ExactType == FieldExactTypes.DynamicImage)
				Required = false;

			if (ExactType == FieldExactTypes.Date || ExactType == FieldExactTypes.DateTime || ExactType == FieldExactTypes.Time
                || ExactType == FieldExactTypes.DynamicImage || ExactType == FieldExactTypes.M2MRelation || ExactType == FieldExactTypes.M2ORelation
				|| ExactType == FieldExactTypes.Textbox || ExactType == FieldExactTypes.VisualEdit)
			{
				IsUnique = false;
			}

			if (ExactType == FieldExactTypes.Boolean || ExactType == FieldExactTypes.Textbox
                || ExactType == FieldExactTypes.VisualEdit || ExactType == FieldExactTypes.M2MRelation || ExactType == FieldExactTypes.M2ORelation)
			{
				Indexed = false;
			}

			if (ExactType == FieldExactTypes.O2MRelation)
			{
				Indexed = true;
				if (!RelateToContentId.HasValue || RelateToContentId == ContentId)
					Aggregated = false;
				if (!Aggregated)
					ClassifierId = null;
			}

            if (ExactType == FieldExactTypes.M2MRelation || ExactType == FieldExactTypes.M2ORelation || ExactType == FieldExactTypes.DynamicImage)
				OnScreen = false;

			if (ExactType != FieldExactTypes.VisualEdit)
				DocType = null;

			UpdateStringEnum();

			UpdateDefaultValueModel();

			UpdateRelationModel();

            UpdateOrderByModel();

			UpdateBindingsModel();

            if (!this.MapAsProperty)
                this.LinqPropertyName = null;

            if (ContentId != RelateToContentId)
                TreeOrderFieldId = null;
		}

		public void UpdateBindingsModel()
		{

		}

		public void UpdateStringEnum()
		{
			if (ExactType == FieldExactTypes.StringEnum)
			{												
				StringEnumItem enumDefValue = StringEnumItems.FirstOrDefault(v => v.GetIsDefault());
				if (enumDefValue != null)
					StringDefaultValue = enumDefValue.Value;
				else
					StringDefaultValue = null;

				Indexed = true;
				if (ShowAsRadioButtons)
					Required = true;
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
		/// <param name="model"></param>
		private void UpdateDefaultValueModel()
		{
			switch (this.ExactType)
			{
				case Constants.FieldExactTypes.Boolean:
					this.DefaultValue = this.BooleanDefaultValue ? "1" : "0";
					break;
				case Constants.FieldExactTypes.Date:
					this.DefaultValue = Converter.ToDbDateTimeString(this.DateDefaultValue);
					break;
				case Constants.FieldExactTypes.DateTime:
					this.DefaultValue = Converter.ToDbDateTimeString(this.DateTimeDefaultValue);
					break;
				case Constants.FieldExactTypes.Time:
					this.DefaultValue = Converter.ToDbDateTimeString(this.TimeDefaultValue);
					break;
				case Constants.FieldExactTypes.String:
				case Constants.FieldExactTypes.StringEnum:
					this.DefaultValue = this.StringDefaultValue;
					break;
				case Constants.FieldExactTypes.Numeric:
					this.DefaultValue = Converter.ToDbNumericString(this.NumericDefaultValue);
					break;
				case Constants.FieldExactTypes.Image:
				case Constants.FieldExactTypes.File:
					this.DefaultValue = this.FileDefaultValue;
					break;
				case Constants.FieldExactTypes.Textbox:
					this.DefaultBlobValue = this.TextBoxDefaultValue;
					break;
				case Constants.FieldExactTypes.VisualEdit:
					this.DefaultBlobValue = this.VisualEditDefaultValue;
					break;
				case Constants.FieldExactTypes.O2MRelation:
					this.DefaultValue = this.O2MDefaultValue;
					break;
				case Constants.FieldExactTypes.Classifier:
					this.DefaultValue = Converter.ToDbNumericString(this.ClassifierDefaultValue);
					break;								
				default:
					this.DefaultValue = null;
					this.DefaultBlobValue = null;
					break;
			}
		}

		/// <summary>
		/// Устанавливает свойства поля типа "связь"
		/// </summary>
		/// <param name="model"></param>
		private void UpdateRelationModel()
		{
			if (this.ExactType != Constants.FieldExactTypes.M2MRelation && this.ExactType != Constants.FieldExactTypes.O2MRelation)
				this.UseForDefaultFiltration = false;			

            if (this.ExactType != Constants.FieldExactTypes.M2MRelation)
            {
                this.LinkId = null;
                this.ContentLink = null;
            }

            if (this.ExactType != Constants.FieldExactTypes.O2MRelation)
            {
                this.RelationId = null;
                this.LinqBackPropertyName = null;
            }

            if (this.ExactType != Constants.FieldExactTypes.M2ORelation)
            {
                this.BackRelationId = null;
            }

            if (this.ExactType != Constants.FieldExactTypes.O2MRelation && this.ExactType != Constants.FieldExactTypes.M2MRelation)
            {
                this.UseRelationCondition = false;
				this.UseRelationSecurity = false;
            }
			if (!this.UseRelationCondition)
				this.RelationCondition = null;
            
			if (this.ExactType == Constants.FieldExactTypes.M2MRelation)
			{
                if (this.RelateToContentId.HasValue)
				{
                    if (this.ContentLink.LContentId == ContentId && this.ContentLink.RContentId != this.RelateToContentId.Value)
                    {
                        this.ContentLink.RContentId = this.RelateToContentId.Value;
                        this.ContentLink.LinkId = 0;
                    }
                    else if (this.ContentLink.RContentId == ContentId && this.ContentLink.LContentId != this.RelateToContentId.Value)
                    {
                        this.ContentLink.LContentId = this.RelateToContentId.Value;
                        this.ContentLink.LinkId = 0;
                    }

					// Создать обратное поле если это необходимо и возможно
					if (!String.IsNullOrWhiteSpace(NewM2MBackwardFieldName) && !IsBackwardFieldExists && RelateToContentId != this.ContentId)
					{
						Content relContent = ContentRepository.GetById(RelateToContentId.Value);
						// возможно только если Related контент не виртуальный
						if (relContent.VirtualType == 0)
						{
							var f = new Field(relContent).Init();
							f.Name = NewM2MBackwardFieldName;
							f.ExactType = FieldExactTypes.M2MRelation;
							f.ContentId = relContent.Id;
							f.RelateToContentId = ContentId;
							M2MBackwardField = f;
						}
					}
				}
                
                if (!this.ContentLink.MapAsClass)
                {
                    this.ContentLink.NetLinkName = null;
                    this.ContentLink.NetPluralLinkName = null;
                }
			}

            if (this.ExactType == Constants.FieldExactTypes.O2MRelation)
            {
                // Создать обратное поле если это необходимо и возможно
                if (!String.IsNullOrWhiteSpace(NewO2MBackwardFieldName) && !IsBackwardFieldExists)
                {
                    Content relContent = ContentRepository.GetById(RelateToContentId.Value);
					// возможно только если Related контент не виртуальный
					if (relContent.VirtualType == 0)
					{
						var f = new Field(relContent).Init();
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

			if (this.ExactType != Constants.FieldExactTypes.O2MRelation || this.ContentId != this.RelateToContentId)
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
		#endregion

		#region Save/Update Validation
		public override void Validate()
		{
			var errors = new RulesException<Field>();

			base.Validate(errors);

			ValidateField(errors);

			if (!errors.IsEmpty)
				throw errors;
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
				errors.ErrorFor(f => f.ReadOnly, FieldStrings.ReadOnlyAndRequiredBothAreTrue);
			
			if(ReadOnly && ExactType == FieldExactTypes.Classifier)
				errors.ErrorFor(f => ReadOnly, FieldStrings.ClassiferCannotBeReadonly);
            
            if (IsUnique)
            {
				if (IsNew && !Constraint.IsComplex) {
                    if (ContentRepository.IsAnyArticle(ContentId))
                        errors.ErrorFor(f => IsUnique, FieldStrings.CannotCreateUniqueFieldBecauseOfExisting);
                }				
				else
				{
					int count = Constraint.CountDuplicates(0);
					if (count > 0)
						errors.ErrorFor(f => IsUnique, String.Format(FieldStrings.ExistingDataIsNotUnique, count));
				}
            }
            
            // Значение по умолчанию и уникальность нельзя устанавливать одновременно
			if (!String.IsNullOrEmpty(Default) && IsUnique)
				errors.ErrorFor(f => f.DefaultValue, FieldStrings.DefaultAndUniqueBothAreSet);
			// Проверить дли значения по умолчанию
			if (!IsUnique && ExactType != FieldExactTypes.Textbox && ExactType != FieldExactTypes.VisualEdit &&
				!String.IsNullOrEmpty(Default) && !DefaultValue.Length.IsInRange(0, 255))
			{
				errors.ErrorFor(f => f.DefaultValue, FieldStrings.DefaultMaxLengthExceeded);
			}

			if (UseInputMask && String.IsNullOrWhiteSpace(InputMask))
				errors.ErrorFor(f => f.InputMask, FieldStrings.InputMaskNotEntered);

			ValidateRelations(errors);
			ValidateSize(errors);
			ValidateDynamicImage(errors);
			ValidateTypeChanging(errors);
			ValidateAggregated(errors);
			ValidateStringEnumValues(errors);
			ValidateVariations(errors);
			ValidateExternalCss(errors);

			// Валидация поля на конфликты с полями дочерних контентов
			new FieldsConflictValidator().SubContentsCheck(this.Content, this)
			    .Select(v => { errors.ErrorForModel(v.Message); return (object)null; })
			    .ToArray();
		}

		private void ValidateVariations(RulesException<Field> errors)
		{
			if (UseForVariations && Content.Fields.Any(n => n.ExactType == FieldExactTypes.M2ORelation))
				errors.ErrorFor(f => f.UseForVariations, FieldStrings.VariationAndM2OIncompatible);

			if (ExactType == FieldExactTypes.M2ORelation && Content.Fields.Any(n => n.UseForVariations))
				errors.ErrorFor(f => f.ExactType, FieldStrings.VariationAndM2OIncompatible);
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
					IEnumerable<StringEnumItem> emptyEnumValues = StringEnumItems.Where(v =>String.IsNullOrWhiteSpace(v.Value) || String.IsNullOrWhiteSpace(v.Alias));
					IEnumerable<StringEnumItem> sameValueItems = StringEnumItems.GroupBy(v => v.Value).Where(g => g.Count() > 1).SelectMany(g => g);
					IEnumerable<StringEnumItem> sameAliasItems = StringEnumItems.GroupBy(v => v.Alias).Where(g => g.Count() > 1).SelectMany(g => g);
					if (emptyEnumValues.Any())
						errors.ErrorFor(m => m.StringEnumItems, FieldStrings.EnumValueIsInvalid);
					if(sameValueItems.Any())
						errors.ErrorFor(m => m.StringEnumItems, FieldStrings.EnumValueValueIsNotUnique);
					if (sameAliasItems.Any())
						errors.ErrorFor(m => m.StringEnumItems, FieldStrings.EnumValueAliasIsNotUnique);

					IEnumerable<StringEnumItem> invalidateItems = emptyEnumValues;
					if(!invalidateItems.Any())
						invalidateItems = sameValueItems;
					if (!invalidateItems.Any())
						invalidateItems = sameAliasItems;
					foreach (var v in invalidateItems)
					{
						v.Invalid = true;
					}
				}
				else
					errors.ErrorFor(m => m.StringEnumItems, FieldStrings.EnumValueIsNotDefined);
			}
		}

		private void ValidateAggregated(RulesException<Field> errors)
		{
			int aggregatedChangeType = 3; // (Type: 3 - осталось неагрегированным);
			Field dbFieldVersion = null;

			if (IsNew)
				aggregatedChangeType = Aggregated ? 0 : 3;
			else
			{
				dbFieldVersion = FieldRepository.GetById(Id);
				if (dbFieldVersion.Aggregated && !Aggregated)
					aggregatedChangeType = 1;
				else if (!dbFieldVersion.Aggregated && Aggregated)
					aggregatedChangeType = 0;
				else if (dbFieldVersion.Aggregated && Aggregated)
					aggregatedChangeType = 2;
				else if (!dbFieldVersion.Aggregated && !Aggregated)
					aggregatedChangeType = 3;
			}

			if (aggregatedChangeType == 0 || aggregatedChangeType == 2) // поле становиться агрегированным (Type: 0) или осталось агрегированным (Type: 2)
			{
				if (Content.Fields.Where(f => f.Id != Id && f.Aggregated).Any()) // Контент не должен содержать других агрегированных связей					
					errors.ErrorFor(f => f.Aggregated, FieldStrings.ThereIsAggregatedInContent);
				else
				{
					if (!ClassifierId.HasValue)
						errors.ErrorFor(f => f.ClassifierId, FieldStrings.ClassifierRefNotSelected);

					if (!IsUnique) // агрегированное поле должно быть уникальным
						errors.ErrorFor(f => f.IsUnique, FieldStrings.AggregatedIsNotUnique);
					else if (Constraint.IsComplex) // но не в комбинации с каким-либо другим полем
						errors.ErrorFor(f => f.IsUnique, FieldStrings.AggregatedIsComplexUnique);

					if (Content.HasAnyNotification)
						errors.ErrorForModel(FieldStrings.CannotSetAggregateBecauseOfNotification);
					if (RelateToContentId.HasValue)
					{
						if (RelateToContentId == ContentId) // Нельзя сделать связь агрегированной, если это связь на тот же контент
							errors.ErrorFor(f => f.RelateToContentId, FieldStrings.AggregatedRelateToCurrentContent);
						else if (RelatedToContent.HasAggregatedFields) // Корневой контент не должен содержать агрегированных связей
							errors.ErrorFor(f => f.RelateToContentId, FieldStrings.ThereAreAggregatedsInRootContent);
					}

					if (ContentRepository.IsAnyArticle(ContentId))// Если контент содержит статьи, то 
					{
						if(aggregatedChangeType == 0) // поле нельзя сделать агрегиованным
							errors.ErrorFor(f => Aggregated, FieldStrings.CannotSetAggregateBecauseOfExisting);
						else if (aggregatedChangeType == 2) 
						{
							if (dbFieldVersion.ExactType != ExactType) // нельзя сменить тип поля
								errors.ErrorFor(f => ExactType, FieldStrings.CannotChangeAggregateFieldTypeBecauseOfExisting);
							if(dbFieldVersion.ClassifierId != ClassifierId) // нельзя изменить ссылку на классификатор
								errors.ErrorFor(f => ClassifierId, FieldStrings.CannotChangeClassifierRefBecauseOfExisting);
						}
					}

					if (aggregatedChangeType == 2 && ClassifierId.HasValue && FieldRepository.GetById(ClassifierId.Value).ClassifierDefaultValue == ContentId) // Если ClassifierDefaultValue классификатора равно контенту текущего поля, то 
					{
						if (dbFieldVersion.ExactType != ExactType) // нельзя сменить тип поля
							errors.ErrorFor(f => ExactType, FieldStrings.CannotChangeAggregateFieldTypeBecauseOfClassifierDefaultValue);
						if (dbFieldVersion.ClassifierId != ClassifierId) // нельзя изменить ссылку на классификатор
							errors.ErrorFor(f => ClassifierId, FieldStrings.CannotChangeClassifierRefBecauseOfClassifierDefaultValue);
					}
					
					// Дочерний и родительский контенты должны иметь одинаковые значения свойства контента:
					if (Content.AutoArchive != RelatedToContent.AutoArchive) // Архивировать при удалении
						errors.ErrorFor(f => RelateToContentId, FieldStrings.RelatedContentAutoArchiveIsDifferent);
					if (Content.WorkflowBinding.WorkflowId != RelatedToContent.WorkflowBinding.WorkflowId) // Тип Workflow
						errors.ErrorFor(f => RelateToContentId, FieldStrings.RelatedContentWorkflowIsDifferent);
					if (Content.WorkflowBinding.IsAsync != RelatedToContent.WorkflowBinding.IsAsync) // Расщеплять ли статьи по Workflow
						errors.ErrorFor(f => RelateToContentId, FieldStrings.RelatedContentWorkflowSplitIsDifferent);
						
				}
			}
			else if (aggregatedChangeType == 1) // поле становиться не агрегированным (Type: 1)
			{
				if (ContentRepository.IsAnyArticle(ContentId)) // если контент содержит статьи, то
					errors.ErrorFor(f => Aggregated, FieldStrings.CannotResetAggregateBecauseOfExisting); //Невозможно сделать поле неагрегированным
				if (dbFieldVersion.ClassifierId.HasValue && FieldRepository.GetById(dbFieldVersion.ClassifierId.Value).ClassifierDefaultValue == ContentId) // Если ClassifierDefaultValue классификатора равно контенту текущего поля, то 
					errors.ErrorFor(f => Aggregated, FieldStrings.CannotResetAggregateBecauseOfClassifierDefaultValue); //Невозможно сделать поле неагрегированным
			}
		}

		private void ValidateTypeChanging(RulesException<Field> errors)
		{
			if (!IsNew)
			{
				Field dbFieldVersion = FieldRepository.GetById(Id);

				if (dbFieldVersion.ExactType != ExactType)
				{
					// Участвует ли поле в сложном ограничении уникальности
					if (dbFieldVersion.IsUnique && dbFieldVersion.Constraint.IsComplex && IsUnique && Constraint.IsComplex)
						errors.ErrorFor(f => f.ExactType, FieldStrings.MultiplyUniqueConstraint);

					// Если на поле есть ссылки по O2M, то нельзя менять тип этого поля на не "ссылочный"
					if (RelatedO2MFields.Any())
					{
						// Если поле уже не ссылочное, то запретитить менять тип
						if (!dbFieldVersion.IsRelateable)
							ExactType = dbFieldVersion.ExactType;
						// Если было ссылочное, а меняем на не ссылочное, то оставить тип без изменения
						else if(!IsRelateable)
							errors.ErrorFor(f => f.ExactType, FieldStrings.CannotChangeTypeToNotRelateable);						
					}

					// Если поле классификатор и на него ссылаються агрегированные поля, то тип менять нельзя
					if(dbFieldVersion.ExactType == FieldExactTypes.Classifier && dbFieldVersion.HasAnyAggregators)
						errors.ErrorFor(f => f.ExactType, FieldStrings.CannotChangeClassifierType);
				}
				// String -> File || Image: контент в БД не более 255 символов
				if (dbFieldVersion.ExactType == FieldExactTypes.String && (ExactType == FieldExactTypes.File || ExactType == FieldExactTypes.Image))
				{
					if (FieldRepository.GetTextFieldMaxLength(dbFieldVersion) > 255)
						errors.ErrorFor(f => f.ExactType, String.Format(FieldStrings.FieldValueLengthExceeded, 255));
				}
				// String -> StringEnum
				else if (dbFieldVersion.ExactType == FieldExactTypes.String && ExactType == FieldExactTypes.StringEnum)
				{
					if (FieldRepository.IsNonEnumFieldValueExist(this))
						errors.ErrorFor(f => f.ExactType, FieldStrings.NonEnumFieldValueExist);
				}
				// Textbox || VisualEdit -> String: контент в БД не более 3500 символов
				else if (dbFieldVersion.IsBlob && ExactType == FieldExactTypes.String)
				{
					if (FieldRepository.GetTextFieldMaxLength(dbFieldVersion) > StringSize)
						errors.ErrorFor(f => f.ExactType, String.Format(FieldStrings.FieldValueLengthExceeded, StringSize));
				}
				else if (dbFieldVersion.ExactType == FieldExactTypes.Numeric && ExactType == FieldExactTypes.Boolean)
				{
					int? maxValue = FieldRepository.GetNumericFieldMaxValue(dbFieldVersion);
					if (maxValue.HasValue && !maxValue.Value.IsInRange(0, 1))
						errors.ErrorFor(f => f.ExactType, String.Format(FieldStrings.FieldValueIsNotInRange, 0, 1));
				}
				else if (dbFieldVersion.ExactType == FieldExactTypes.Numeric && ExactType == FieldExactTypes.O2MRelation)
				{
					if (!FieldRepository.CheckNumericValuesAsKey(this, dbFieldVersion))
						errors.ErrorFor(f => f.ExactType, FieldStrings.FieldHasBadValues);
				}
                else if (dbFieldVersion.ExactType == FieldExactTypes.O2MRelation && ExactType == FieldExactTypes.M2MRelation)
                {
                    if (dbFieldVersion.ContentId == dbFieldVersion.RelateToContentId)
                        errors.ErrorFor(f => f.ExactType, FieldStrings.CannotChangeRelationTypeForSelfRelation);
                }
                else if (dbFieldVersion.ExactType == FieldExactTypes.M2MRelation && ExactType == FieldExactTypes.O2MRelation)
                {
                    if (dbFieldVersion.ContentId == dbFieldVersion.RelateToContentId)
                        errors.ErrorFor(f => f.ExactType, FieldStrings.CannotChangeRelationTypeForSelfRelation);
                    else if (FieldRepository.DoPluralLinksExist(dbFieldVersion))
                        errors.ErrorFor(f => f.ExactType, FieldStrings.FieldHasBadValues);
                }
			}
		}

		private void ValidateDynamicImage(RulesException<Field> errors)
		{
			if (ExactType == FieldExactTypes.DynamicImage)
			{
				if (BaseImageId == null)
					errors.ErrorFor(f => f.BaseImageId, FieldStrings.BaseImageNotSelected);

                if (DynamicImage.Type == DynamicImage.JPG_EXTENSION)
                {
                    if (DynamicImage.Quality == null)
                        errors.ErrorFor(f => f.DynamicImage.Quality, FieldStrings.DynamicImageQualityNotEntered);
                    else if (!DynamicImage.Quality.Value.IsInRange(DynamicImage.MinQuality, DynamicImage.MaxQuality))
                        errors.ErrorFor(f => f.DynamicImage.Quality, FieldStrings.DynamicImageQualityNotInRange);
                }

                if (DynamicImage.ResizeMode != DynamicImage.ImageResizeMode.ByWidth && !DynamicImage.Height.IsInRange(DynamicImage.MinImageSize, DynamicImage.MaxImageSize))
					errors.ErrorFor(f => f.DynamicImage.Height, FieldStrings.DynamicImageHeightNotInRange);
                if (DynamicImage.ResizeMode != DynamicImage.ImageResizeMode.ByHeight && !DynamicImage.Width.IsInRange(DynamicImage.MinImageSize, DynamicImage.MaxImageSize))
					errors.ErrorFor(f => f.DynamicImage.Width, FieldStrings.DynamicImageWidthNotInRange);
			}
		}

		private void ValidateSize(RulesException<Field> errors)
		{
			if (this.Content.VirtualType == VirtualType.None)
			{
				switch (ExactType)
				{
					case Constants.FieldExactTypes.Textbox:
						if (!this.TextBoxRows.IsInRange(5, 30))
							errors.ErrorFor(f => f.TextBoxRows, FieldStrings.TextboxRowsNotInRange);
						break;
					case Constants.FieldExactTypes.VisualEdit:
						if (!this.VisualEditorHeight.IsInRange(350, 750))
							errors.ErrorFor(f => f.VisualEditorHeight, FieldStrings.VisualEditHeightNotInRange);
						break;
					case Constants.FieldExactTypes.Numeric:
						if (!this.DecimalPlaces.IsInRange(0, 25))
							errors.ErrorFor(f => f.DecimalPlaces, FieldStrings.DecimalPlacesNotInRange);
						break;
					case Constants.FieldExactTypes.String:
						{
							if (!IsNew)
							{
                                Field dbFieldVersion = FieldRepository.GetById(Id);
								if (dbFieldVersion.ExactType == FieldExactTypes.String)
								{
									int maxFieldLength = FieldRepository.GetTextFieldMaxLength(dbFieldVersion);
									if (maxFieldLength > this.StringSize)
										errors.ErrorFor(f => f.StringSize, String.Format(FieldStrings.FieldValueLengthExceeded, maxFieldLength));
								}
							}
							if (!this.StringSize.IsInRange(1, 3500))
								errors.ErrorFor(f => f.StringSize, FieldStrings.StringLengthNotInRange);
							break;
						}
					default:
						break;
				}
			}
		}

		private void ValidateRelations(RulesException<Field> errors)
		{
			if (MapAsProperty)
			{
				if (String.IsNullOrWhiteSpace(LinqPropertyName))
					errors.ErrorFor(f => f.LinqPropertyName, FieldStrings.LinqPropertyNameNotEntered);
				else if (FieldRepository.LinqPropertyNameExists(this))
					errors.ErrorFor(f => f.LinqPropertyName, FieldStrings.LinqPropertyNameNonUnique);

                if (ExactType == FieldExactTypes.O2MRelation && O2MBackwardField == null)
				{
					if (String.IsNullOrWhiteSpace(LinqBackPropertyName))
						errors.ErrorFor(f => f.LinqBackPropertyName, FieldStrings.LinqBackPropertyNameNotEntered);
					else if (FieldRepository.LinqBackPropertyNameExists(this))
						errors.ErrorFor(f => f.LinqBackPropertyName, FieldStrings.LinqBackPropertyNameNonUnique);

					if (!String.IsNullOrWhiteSpace(LinqPropertyName) && LinqPropertyName.Equals(LinqBackPropertyName, StringComparison.InvariantCulture))
						errors.ErrorFor(f => f.LinqBackPropertyName, FieldStrings.LinqPropertyNamesAreEqual);
				}
			}

			if (this.TypeId == FieldTypeCodes.Relation)
			{
				if (RelateToContentId == null)
					errors.ErrorFor(f => f.RelateToContentId, FieldStrings.RelateToContentNotSelected);

				if (UseRelationSecurity && !Required)
				{
					errors.ErrorFor(f => f.Required, FieldStrings.RelationSecurityFieldMustBeRequired);
				}
			}
			
			if (ExactType == FieldExactTypes.M2MRelation)
			{
				if (ContentLink.MapAsClass)
				{
					if (String.IsNullOrWhiteSpace(ContentLink.NetLinkName))
						errors.ErrorFor(f => f.ContentLink.NetLinkName, FieldStrings.NetLinkNameNotEntered);
					else if (FieldRepository.NetNameExists(ContentLink))
						errors.ErrorFor(f => f.ContentLink.NetLinkName, FieldStrings.NetLinkNameNonUnique);

					if (String.IsNullOrWhiteSpace(ContentLink.NetPluralLinkName))
						errors.ErrorFor(f => f.ContentLink.NetPluralLinkName, FieldStrings.NetPluralLinkNameNotEntered);
					else if (FieldRepository.NetPluralNameExists(ContentLink))
						errors.ErrorFor(f => f.ContentLink.NetPluralLinkName, FieldStrings.NetPluralLinkNameNonUnique);

					if (ContentLink.NetLinkName.Equals(ContentLink.NetPluralLinkName, StringComparison.InvariantCulture))
						errors.ErrorFor(f => f.ContentLink.NetPluralLinkName, FieldStrings.NetLinkNamesAreEqual);
				}

				if (!String.IsNullOrWhiteSpace(NewM2MBackwardFieldName))
				{
					Content relContent = ContentRepository.GetById(RelateToContentId.Value);
					// BackwardField возможно создать только если Related контент не виртуальный
					if (relContent.VirtualType != 0)
						errors.ErrorFor(f => f.NewM2MBackwardFieldName, FieldStrings.VirtualBackwardFieldIsNotAllowed);
				}

				if (M2MBackwardField != null && M2MBackwardField.IsNew && !String.IsNullOrEmpty(M2MBackwardField.Name))
				{
                    if (M2MBackwardField.Name.Equals(Name, StringComparison.InvariantCulture))
                        errors.ErrorFor(f => f.NewM2MBackwardFieldName, FieldStrings.BackwardFieldNameAndNameAreEqual);
                    else
                    {
                        try
                        {
                            M2MBackwardField.Validate();
                        }
                        catch(RulesException<Field> ex) 
                        {
                            foreach (var error in ex.Errors) {
                                errors.ErrorFor(f => f.NewM2MBackwardFieldName, error.Message);
                            }
                        }                    
                    }
				}
			}
			else if (ExactType == FieldExactTypes.O2MRelation)
			{
				if (RelationId == null)
					errors.ErrorFor(f => f.RelationId, FieldStrings.RelationIdNotSelected);

				if (UseForTree)
				{
					string treeName = ContentRepository.GetTreeFieldName(ContentId, Id);
					if (!String.IsNullOrEmpty(treeName))
						errors.ErrorFor(f => f.UseForTree, String.Format(FieldStrings.UseForTreeAlreadySet, treeName));
				}

				if (!String.IsNullOrWhiteSpace(NewO2MBackwardFieldName))
				{
					Content relContent = ContentRepository.GetById(RelateToContentId.Value);
					// BackwardField возможно создать только если Related контент не виртуальный
					if (relContent.VirtualType != 0)
						errors.ErrorFor(f => f.NewO2MBackwardFieldName, FieldStrings.VirtualBackwardFieldIsNotAllowed);
				}

                if (O2MBackwardField != null && O2MBackwardField.IsNew && !String.IsNullOrEmpty(O2MBackwardField.Name))
                {
                    if (O2MBackwardField.Name.Equals(Name, StringComparison.InvariantCulture))
                        errors.ErrorFor(f => f.NewO2MBackwardFieldName, FieldStrings.BackwardFieldNameAndNameAreEqual);
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
								bool isLinq = false;
								if (error.Property != null) 
								{
									MemberExpression expr = error.Property.Body as MemberExpression;
									if (expr != null && expr.Member.Name == "LinqPropertyName") 
									{
										errors.ErrorFor(f => f.LinqBackPropertyName, error.Message);
										isLinq = true;
									}
								}

								if (!isLinq)
									errors.ErrorFor(f => f.NewO2MBackwardFieldName, error.Message);
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
                    errors.ErrorFor(f => f.BackRelationId, FieldStrings.BackRelationContentNotAssignedAsyncWorkflow);
            }

			// Если Relation-поле используется для построения JOIN-контента и происходит изменения значимого для Relation параметра
			// (Имени, Типа или контента, на который ссылается поле Relation O2M)
			// то обновление запрещается							
			if (!IsNew)
			{				
				Field dbVersion = FieldRepository.GetById(this.Id);
				if (dbVersion.ExactType == FieldExactTypes.O2MRelation)
				{
					var virtualChildrenFieldsExist = VirtualFieldRepository.JoinVirtualChildrenFieldsExist(this);
					if (virtualChildrenFieldsExist &&
						(!dbVersion.Name.Equals(this.Name, StringComparison.InvariantCultureIgnoreCase) ||
						 dbVersion.ExactType != ExactType ||
						 dbVersion.RelateToContentId != RelateToContentId)
						)
					{												
							errors.ErrorForModel(FieldStrings.VirtualChildrenFieldsExist);
					}
				}
			}
		}
		#endregion

		#region Remove Validation
		public void ValidateBeforeDie(IList<string> violationMessages, bool forceChildDelete)
		{
			if (violationMessages == null)
				throw new ArgumentNullException("violations");

			// ------------------------------------------------------------
			// Участвует ли поле в сложном ограничении уникальности
			if (IsUnique && Constraint.IsComplex)
				violationMessages.Add(FieldStrings.MultiplyUniqueConstraint);
			// ------------------------------------------------------------

			// ------------------------------------------------------------
			// Есть ли поля которые ссылаются на данное поле связью O2M ?
			if (RelatedO2MFields.Any())
			{
				// проверяем, есть ли в данном контенте поля на которые их можно перенаправить (алгоритм get_display_field)
				// если нет - то удалять нельзя
				if (!ContentRepository.GetDisplayFieldIds(ContentId).Any(_id => _id != Id))
					violationMessages.Add(FieldStrings.RelatedFieldsExist);
			}
			// ------------------------------------------------------------

			// ------------------------------------------------------------
			// Если для поля есть Thumbnails - то удалять поле нельзя
			if (GetDynamicImages().Any())
				violationMessages.Add(FieldStrings.ThumbnailsExist);
			// ------------------------------------------------------------

			// агрегированную связь нельзя удалять если 
			if (ExactType == FieldExactTypes.O2MRelation && Aggregated)
			{
				if (ContentRepository.IsAnyArticle(ContentId)) // в контенте есть статьи
					violationMessages.Add(FieldStrings.CannotRemoveAggregated);
				if (ClassifierId.HasValue && FieldRepository.GetById(ClassifierId.Value).ClassifierDefaultValue == ContentId) // Если ClassifierDefaultValue классификатора равно контенту текущего поля
					violationMessages.Add(FieldStrings.CannotRemoveAggregatedBecauseOfClassifierDefaultValue);
			}
			// классификатор нельзя удалять если есть агрегеруемые поля которые на него ссылаются
			if (ExactType == FieldExactTypes.Classifier && HasAnyAggregators)
				violationMessages.Add(FieldStrings.CannotRemoveClassifier);

			if (ParentField != null && !forceChildDelete)
			{
				violationMessages.Add(FieldStrings.CannotRemoveChildField);
			}
		}
		#endregion

		public static IEnumerable<string> Die(int id, bool removeChildFields = true)
        {
            Field field = FieldRepository.GetById(id);
            if (field != null)
            {
                return field.Die(removeChildFields);
            }
            else
            {
				return new string[] { String.Format(FieldStrings.FieldNotFound, id) };
            }
        }

        public IEnumerable<string> Die(bool removeVirtualFields = true, bool forceChildDelete = false)
        {
            List<string> violationMessages = new List<string>();
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
                IEnumerable<Content.TreeItem> rebuildedSubContentViews = (removeVirtualFields) ? RemoveVirtualFields() : null;

                // Если есть ссылающиеся поля на это поле, до переключить их на другое подходящее поле
                if (RelatedO2MFields.Any())
                {
                    int newDisplayFieldId = ContentRepository.GetDisplayFieldIds(ContentId).First(_id => _id != Id);
                    ContentRepository.ChangeRelationIdToNewOne(Id, newDisplayFieldId);
                }

                // Удалить директорию динамического изображения
                if (DynamicImage != null)
                    DynamicImage.DeleteDirectory();

                // Удалить ограничения уникальности
                if (IsUnique && !Constraint.IsNew)
                    ContentConstraintRepository.Delete(Constraint);

                // обнулить поле E-mail для уведомлений 
                new NotificationRepository().ClearEmailField(Id);

                // Удалить версии линков M2M
                FieldRepository.RemoveLinkVersions(Id);

                
                // M2O-поле не может существовать без основного
                if (O2MBackwardField != null)
                {
                    O2MBackwardField.Die();
                }

                // Удалить поле
                FieldRepository.Delete(Id);

				var helper = new VirtualContentHelper();
				// Перестроить подчиненные контенты
                if (removeVirtualFields)
					helper.RebuildSubContentViews(rebuildedSubContentViews);

				// Удалить записи OrderField
				FieldRepository.ClearTreeOrder(Id);
            }
        }

        /// <summary>
		/// Удалить подчиненные виртуальные поля при удалении поля родительского контента
		/// </summary>
		/// <param name="field"></param>
        private IEnumerable<Content.TreeItem> RemoveVirtualFields()
		{
			var helper = new VirtualContentHelper();
			return helper.RemoveSubContentVirtualFields(Content, new int[] { Id });
		}

		public Field PersistWithBackward(bool explicitOrder)
		{
			SaveContentLink();			
			Field result = PersistWithVirtualRebuild(explicitOrder);
			Id = result.Id;
			SaveBackwardFields();
			return result;
		}

		//private void CorrectParentField()
		//{
		//	if (Content.ParentContentId != null)
		//	{
		//		Field parentField = FieldRepository.GetByName(Content.ParentContentId.Value, Name);
		//		if (parentField != null)
		//			SetParentAndOverride(parentField.Id);
		//		else
		//			ResetParentAndOverride();
		//	}
		//}

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
			Queue<int> fieldIds = (ForceChildFieldIds != null) ? new Queue<int>(ForceChildFieldIds) : new Queue<int>();
			Queue<int> linkIds = (ForceChildLinkIds != null) ? new Queue<int>(ForceChildLinkIds) : new Queue<int>();
			List<int> virtualIds = (ForceVirtualFieldIds != null) ? new List<int>(ForceVirtualFieldIds) : new List<int>();
			List<int> newVirtualFieldIds = new List<int>();
			List<int> newChildLinkIds = new List<int>();
			List<int> newChildFieldIds = new List<int>();
			if (wasNewBeforeUpdate)
			{
				IEnumerable<Content> contents = Content.ChildContents;
				foreach (var content in contents)
				{
					Field childField = FieldRepository.GetByName(content.Id, Name);
					if (childField != null)
					{
						childField.SetParentAndOverride(Id);
						childField.PersistWithVirtualRebuild(false);
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
							newChildLinkIds.Add(childField.LinkId.Value);
						}

						Field orderField = FieldRepository.GetByOrder(ContentId, Order + 1);
						childField.Order = (orderField == null) ? content.GetMaxFieldsOrder() : orderField.GetChild(content.Id).Order - 1;
						childField.ForceChildFieldIds = fieldIds.ToArray();
						childField.ForceChildLinkIds = linkIds.ToArray();
						childField.ForceVirtualFieldIds = virtualIds.ToArray();

						childField = childField.PersistWithVirtualRebuild(true);

						virtualIds = (childField.ForceVirtualFieldIds == null) ? new List<int>() : childField.ForceVirtualFieldIds.ToList();
						newChildFieldIds.Add(childField.Id);
						
						if (childField.ResultChildFieldIds != null)
							newChildFieldIds.AddRange(childField.ResultChildFieldIds);
						if (childField.ResultChildLinkIds != null)
							newChildLinkIds.AddRange(childField.ResultChildLinkIds);
						if (childField.NewVirtualFieldIds != null)
							newVirtualFieldIds.AddRange(childField.NewVirtualFieldIds);
					}
				}
			}
			else
			{
				foreach (var childField in ChildFields.Where(n => !n.Override))
				{
					Field newChildField = CopyToChild(childField);
					if (newChildField.ExactType == FieldExactTypes.M2MRelation)
					{
						if (linkIds.Any())
						{
							newChildField.ContentLink.ForceLinkId = linkIds.Dequeue();
						}

						newChildField.SaveContentLink();

						if (childField.LinkId != newChildField.LinkId)
							newChildLinkIds.Add(newChildField.LinkId.Value);
					}
					newChildField.PersistWithVirtualRebuild();
					if (newChildField.ResultChildLinkIds != null)
						newChildLinkIds.AddRange(newChildField.ResultChildLinkIds);
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
					//TODO: корректная обработка валидации удаления
					field.Die(true, true);
				}
			}
		}

		private Field GetChild(int contentId)
		{
			return ChildFields
				.Where(n => n.ContentId == contentId)
				.SingleOrDefault();
		}

		private Field CopyToChild(Field childField)
		{
			Field result = CloneForHierarchy(childField.Content);
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
			Field result = (Field)this.MemberwiseClone();
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
					result.DefaultArticleIds = Enumerable.Empty<int>().ToArray();
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
				Field BaseImage = Content.Fields.Single(n => n.Id == BaseImageId.Value);
				result.BaseImageId = BaseImage.GetChild(destContent.Id).Id;
			}

			if (Constraint != null)
			{
				ContentConstraint cc = new ContentConstraint() { ContentId = destContent.Id };
				foreach (var rule in Constraint.Rules)
				{
					Field childField = rule.Field.GetChild(destContent.Id);
					int fieldId = (childField == null) ? 0 : childField.Id;
					cc.Rules.Add(new ContentConstraintRule() { FieldId = fieldId });
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
				Field result = Persist(explicitOrder);
				if (RebuildVirtualContents)
				{
					var helper = new VirtualContentHelper(result.ForceVirtualFieldIds);
					helper.UpdateVirtualFields(result);

					if (result.NewVirtualFieldIds != null)
						result.NewVirtualFieldIds = result.NewVirtualFieldIds.Concat(helper.NewFieldIds).ToArray();
					else
						result.NewVirtualFieldIds = helper.NewFieldIds;

					result.ForceVirtualFieldIds = helper.ForceNewFieldIds;

				}
                return result;
            }            
        }

		/// <summary>
		/// Простое сохранение поля в базу
		/// </summary>
		/// <param name="explicitOrder"></param>
		/// <returns></returns>
        public Field Persist(bool explicitOrder = false)
        {
			if (ExactType != FieldExactTypes.O2MRelation)
			{
				Aggregated = false;
				ClassifierId = null;
			}

			SetRelationDefaultValue();

			bool isNew = IsNew;
			Field result;
			if (IsNew)
			{
				result = FieldRepository.Save(this, explicitOrder);
			}
			else
			{
				result = FieldRepository.Update(this);
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
					O2MBackwardField.ForceId = ForceBackwardId;
                O2MBackwardField.PersistWithVirtualRebuild();
            }
            
            if (ExactType == FieldExactTypes.M2MRelation && M2MBackwardField != null && M2MBackwardField.IsNew)
            {
                M2MBackwardField.LinkId = LinkId;
				if (ForceBackwardId != 0)
					M2MBackwardField.ForceId = ForceBackwardId;
				M2MBackwardField.ContentLink = ContentLink.GetBackwardLink();
                M2MBackwardField.PersistWithVirtualRebuild();
            }
        }

		private void SaveVisualEditorCommands()
		{
			if (ActiveVeCommandIds != null)
			{
				var defaultCommands = VisualEditorRepository.GetDefaultCommands();//все возможные команды
				var offVeCommands = VeAggregationListItemsHelper.Subtract(defaultCommands, ActiveVeCommandIds).Select(c => c.Id).ToArray();//opposite to activeVecommands
				var oldFieldCommands = VisualEditorRepository.GetResultCommands(Id, Content.SiteId);// с этим нужно сравнивать на предмет измененеий
				var siteCommands = VisualEditorRepository.GetSiteCommands(Content.SiteId);

				var changedCommandsDictionary = new Dictionary<int, bool>();
				var defaultCommandsDictionary = new Dictionary<int, bool>();
				var siteCommandsDictionary = new Dictionary<int, bool>();

				foreach (var c in defaultCommands)
				{
					defaultCommandsDictionary.Add(c.Id, c.On);
				}

				foreach (var c in siteCommands)
				{
					siteCommandsDictionary.Add(c.Id, c.On);
				}

				foreach (var cId in ActiveVeCommandIds)
				{
					if (!oldFieldCommands.SingleOrDefault(c => c.Id == cId).On)
						changedCommandsDictionary.Add(cId, true);
				}

				foreach (var cId in offVeCommands)
				{
					if (oldFieldCommands.SingleOrDefault(c => c.Id == cId).On)
						changedCommandsDictionary.Add(cId, false);
				}

				VisualEditorRepository.SetFieldCommands(Content.SiteId, Id, changedCommandsDictionary, defaultCommandsDictionary, siteCommandsDictionary);
			}
		}

		public void SaveM2MDefaultArticles()
		{
			if (DefaultArticleIds != null)
			{
				FieldRepository.SetFieldM2MDefValue(Id, DefaultArticleIds.ToArray());
			}

		}

		internal void SaveVisualEditorStyles()
		{
			if (ActiveVeStyleIds != null)
			{
				var defaultStyles = VisualEditorRepository.GetAllStyles();//все возможные стили
				var offVeStyles = VeAggregationListItemsHelper.Subtract(defaultStyles, ActiveVeStyleIds).Select(c => c.Id).ToArray();//opposite to activeVecommands
				var oldFieldStyles = VisualEditorRepository.GetResultStyles(Id, Content.SiteId);// с этим нужно сравнивать на предмет измененеий
				var siteStyles = VisualEditorRepository.GetSiteStyles(Content.SiteId);

				var changedStylesDictionary = new Dictionary<int, bool>();
				var defaultStylesDictionary = new Dictionary<int, bool>();
				var siteStylesDictionary = new Dictionary<int, bool>();

				foreach (var c in defaultStyles)
				{
					defaultStylesDictionary.Add(c.Id, c.On);
				}

				foreach (var c in siteStyles)
				{
					siteStylesDictionary.Add(c.Id, c.On);
				}

				foreach (var cId in ActiveVeStyleIds)
				{
					if (!oldFieldStyles.SingleOrDefault(c => c.Id == cId).On)
						changedStylesDictionary.Add(cId, true);
				}

				foreach (var cId in offVeStyles)
				{
					if (oldFieldStyles.SingleOrDefault(c => c.Id == cId).On)
						changedStylesDictionary.Add(cId, false);
				}

				VisualEditorRepository.SetFieldStyles(Content.SiteId, Id, changedStylesDictionary, defaultStylesDictionary, siteStylesDictionary);
			}
		}

		internal void SaveContentLink()
        {
            if (ExactType == FieldExactTypes.M2MRelation && ContentLink != null)
            {
                bool isNew = ContentLink.IsNew;
				ContentLink = (isNew) ? ContentRepository.SaveLink(ContentLink) : ContentRepository.UpdateLink(ContentLink);
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
				FieldRepository.CorrectEnumInContentData(this);
			}
		}

        private void SetRelationDefaultValue()
        {
            if (ExactType == FieldExactTypes.M2MRelation)
                DefaultValue = (LinkId.HasValue) ? LinkId.ToString() : null;
            else if (ExactType == FieldExactTypes.M2ORelation)
                DefaultValue = (BackRelationId.HasValue) ? BackRelationId.ToString() : null;
        }

		public IEnumerable<ListItem> GetRelatedTitles(string value)
        {
				return ContentRepository.GetArticleTitleList(
					RelateToContentId.Value,
					(ExactType == FieldExactTypes.O2MRelation) ? Relation.Name : null,
					value
				);
        }
		

        public Field GetBaseField(int articleId)
        {
            if (!Content.IsVirtual)
                return this;
            else
            {
                int oldId, newId = Id;
                do
                {
                    oldId = newId;
                    newId = FieldRepository.GetBaseFieldId(newId, articleId);
                } while (newId != oldId);
                return FieldRepository.GetById(newId);
            }
        }		

        public List<Field> GetDynamicImages()
        {
            return FieldRepository.GetDynamicImageFields(ContentId, Id);
        }		

		public static IEnumerable<MaskTemplate> GetAllMaskTemplates()
		{
			return MaskTemplateRepository.GetAllMaskTemplates();
		}		

		/// <summary>
		/// Обновляет порядок полей контента
		/// </summary>
		/// <param name="item"></param>
		internal void ReorderContentFields()
		{
			if (Content.Fields.Any())
			{
				if (IsNew)
				{
					var lastOrder = Content.GetMaxFieldsOrder();
					if (Order != lastOrder)
					{
						FieldRepository.UpdateContentFieldsOrders(ContentId, -1, Order);
					}
                	Order++;
				}
				else
				{
					var currentOrder = FieldRepository.GetById(Id).Order;
					// Если позиция поля поменялась - то выполняем reorder 
					if (currentOrder != Order)
					{
						FieldRepository.UpdateContentFieldsOrders(ContentId, currentOrder, Order);
						if (currentOrder > Order)
							Order++;
					}
				}
			}
		}
		
		internal void MutateLinqBackPropertyName()
		{
			if (ExactType == Constants.FieldExactTypes.O2MRelation)
			{
				if (!String.IsNullOrEmpty(LinqBackPropertyName))
				{
					string name = LinqBackPropertyName;
					int index = 0;
					do
					{
						index++;
						LinqBackPropertyName = MutateHelper.MutateNetName(name, index);
					}
					while (FieldRepository.LinqBackPropertyNameExists(this));
				}
			}
		}

        internal void MutateLinqPropertyName()
        {
            if (!String.IsNullOrEmpty(LinqPropertyName))
            {
                string name = LinqPropertyName;
                int index = 0;
                do
                {
                    index++;
                    LinqPropertyName = MutateHelper.MutateNetName(name, index);
                }
                while (FieldRepository.LinqPropertyNameExists(this));
            }
        }

        internal void MutateName()
        {
            string name = Name;
            int index = 0;
            do
            {
                index++;
                Name = MutateHelper.MutateTitle(name, index);
            }
            while (FieldRepository.NameExists(this));
        }
		
		internal void MutateNames()
		{
            MutateName();
            MutateLinqPropertyName();
            MutateLinqBackPropertyName();
		}

		internal Field GetVirtualClone(Content virtualContent)
		{
			var result = new Field(virtualContent)
			{				
				Name = this.Name,				
				TypeId = this.TypeId,				
				RelationId = this.RelationId,
				BaseImageId = this.BaseImageId,
				UseSiteLibrary = this.UseSiteLibrary,
				LinkId = this.LinkId,
				ViewInList = this.ViewInList,
				BackRelationId = this.BackRelationId,
				IsLong = this.IsLong,
				UseForTree = this.UseForTree,
				AutoCheckChildren = this.AutoCheckChildren,
				IsClassifier = this.IsClassifier
				
			};
			result.Init();

			result.ExactType = this.ExactType;

			result.Size = this.Size;
			result.TextBoxRows = this.TextBoxRows;
			result.VisualEditorHeight = this.VisualEditorHeight;
			result.DecimalPlaces = this.DecimalPlaces;
			result.StringSize = this.StringSize;

			return result;
		}
		/// <summary>
		/// Возвращает виртуальное поле на основе данного поля
		/// </summary>
		/// <param name="virtualContent"></param>
		/// <returns></returns>
		internal Field GetVirtualCloneForJoin(Content virtualContent, Field parentVirtualField)
		{
			var result = GetVirtualClone(virtualContent);
			result.PersistentId = this.Id;
			
			if (parentVirtualField != null)
			{
				result.Name = String.Concat(parentVirtualField.Name, '.', this.Name);
				result.JoinId = parentVirtualField.Id;
			}
			else
				result.Name = this.Name;

			return result;
		}

        public string GetRelationFilter(int articleId)
        {
            if (ExactType != FieldExactTypes.M2ORelation)
                return RelationFilter;
            else
            {
                return String.Format("(c.[{0}] = {1} OR c.[{0}] IS NULL) AND c.[ARCHIVE] = 0", BackRelation.Name, articleId);
            }
        }

		public void ParseStringEnumJson(string json)
		{
			if (!String.IsNullOrWhiteSpace(json))
				StringEnumItems = new JavaScriptSerializer().Deserialize<StringEnumItem[]>(json) ?? Enumerable.Empty<StringEnumItem>();
			else
				StringEnumItems = Enumerable.Empty<StringEnumItem>();
		}

		public IEnumerable<Content> GetAggregatedContents()
		{
			if (IsClassifier)
				return FieldRepository.GetAggregatableContentsForClassifier(this);
			else
				return null;

		}

		#endregion		   							
		
		[LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
		public int[] DefaultArticleIds
		{
			get
			{
				return _DefaultArticleIds.Value;
			}

			set
			{
				_DefaultArticleIds.Value = value;
			}
		}

		public IEnumerable<ListItem> DefaultArticleListItems 
		{ 
			get
			{
				return (!RelateToContentId.HasValue) ? 
					new List<ListItem>() : 
					ArticleRepository.GetSimpleList(RelateToContentId.Value, 0, Id, ListSelectionMode.OnlySelectedItems, DefaultArticleIds.ToArray(), "", 0);
			}
		}

		public bool HasArticles
		{
			get { return FieldRepository.FieldHasArticles(ContentId); }
		}

		public static FieldExactTypes CreateExactType(int typeId, int? linkId, bool isClassifier, bool isStringEnum)
		{
			FieldExactTypes result = (FieldExactTypes)typeId;
			if (result == FieldExactTypes.String && isStringEnum)
				result = FieldExactTypes.StringEnum;
			if (result == FieldExactTypes.Numeric && isClassifier)
				result = FieldExactTypes.Classifier;
			if (result == FieldExactTypes.O2MRelation && linkId.HasValue)
				result = FieldExactTypes.M2MRelation;
			return result;
		}
	}
}
