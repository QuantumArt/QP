using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
namespace Quantumart.QP8.WebMvc.ViewModels.Field
{
	public class FieldViewModel : EntityViewModel
	{
		#region Creation		
		public FieldViewModel()
		{
			
		}		
		
		public static FieldViewModel Create(B.Field field, string tabId, int parentId)
		{
			var viewModel = EntityViewModel.Create<FieldViewModel>(field, tabId, parentId);
			var allVeStylesAndFormats = SiteService.GetAllVeStyles();

			viewModel.ActiveVeCommands = viewModel.Data.ActiveVeCommandIds.Select(c => new QPCheckedItem { Value = c.ToString() }).ToList();

			viewModel.DefaultCommandsListItems = FieldService.GetDefaultVisualEditorCommands()
				.Select(c => new ListItem { Value = c.Id.ToString(), Text = c.Alias }).ToArray();			

			viewModel.AllStylesListItems = allVeStylesAndFormats.Where(s => s.IsFormat == false)
				.Select(x => new ListItem { Value = x.Id.ToString(), Text = x.Name }).ToArray();

			viewModel.AllFormatsListItems = allVeStylesAndFormats.Where(s => s.IsFormat == true)
				.Select(x => new ListItem { Value = x.Id.ToString(), Text = x.Name }).ToArray();

			viewModel.ActiveVeStyles = allVeStylesAndFormats
				.Where(s => s.IsFormat == false && viewModel.Data.ActiveVeStyleIds.Contains(s.Id))
				.Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();
	
			viewModel.ActiveVeFormats = allVeStylesAndFormats
				.Where(s => s.IsFormat == true && viewModel.Data.ActiveVeStyleIds.Contains(s.Id))
				.Select(x => new QPCheckedItem { Value = x.Id.ToString() }).ToList();

			viewModel.DefaultArticleIds = viewModel.Data.DefaultArticleIds;
			viewModel.DefaultArticleListItems = viewModel.Data.DefaultArticleListItems;
			viewModel.Init();
			return viewModel;
		}
		#endregion

		#region Properties

		public new B.Field Data
		{
			get { return (B.Field)EntityData; }
			set { EntityData = value; }
		}

		public bool OrderChanged { get; set; }

		public bool ViewInListAffected { get; set; }

		public string RelatedReadActionCode 
		{ 
			get
			{
				return (Data.IsNew || Data.RelatedToContent == null) ? String.Empty : ((Data.RelatedToContent.IsVirtual) ? C.ActionCode.VirtualContentProperties : C.ActionCode.ContentProperties);
			}
		}

		#region BLL Properties												

		public IList<QPCheckedItem> InCombinationWith { get; set; }		

		[LocalizedDisplayName("InputMaskType", NameResourceType = typeof(FieldStrings))]
		public InputMaskTypes InputMaskType { get; set; }

		[LocalizedDisplayName("InputMask", NameResourceType = typeof(FieldStrings))]
		public int? MaskTemplateId { get; set; }		

		[LocalizedDisplayName("DynamicImageMode", NameResourceType = typeof(FieldStrings))]
		public DynamicImageMode DynamicImageSizeMode { get; set; }		
		#endregion

		string entityTypeCode = C.EntityTypeCode.Field;
		public override string EntityTypeCode
		{
			get
			{
				return entityTypeCode;
			}
		}

		string actionCode = C.ActionCode.FieldProperties;
		public override string ActionCode
		{
			get
			{
				if (this.IsNew)
				{
					return C.ActionCode.AddNewField;
				}
				else
				{
					return actionCode;
				}
			}
		}		

		public override ExpandoObject MainComponentOptions
		{
			get 
			{ 
				dynamic result = base.MainComponentOptions;
				result.orderChanged = OrderChanged;
				result.viewInListAffected = ViewInListAffected;
				return result;
			}
		}

		public string RelateToSelectElementId { get { return UniqueId("RelateToSelect"); } }
		public string DisplayFieldSelectElementId { get { return UniqueId("DisplayFieldSelect"); } }
		public string CurrentFieldIdHiddenElementId { get { return UniqueId("CurrentFieldIdHidden"); } }
		public string InCombinationWithElementId { get { return UniqueId("InCombinationWith"); } }


		public string FileDefaultElementId { get { return UniqueId("FileDefaultFileField"); } }
		public string ExactSelectElementId { get { return UniqueId("ExactSelect"); } }
		public string O2MDefaultElementId { get { return UniqueId("O2MSinglePicker"); } }
		public string M2MDefaultElementId { get { return UniqueId("M2MPicker"); } }
		public string IndexedElementId { get { return UniqueId("FieldIndexedCheckbox"); } }
		public string OnScreenElementId { get { return UniqueId("FieldOnScreenCheckbox"); } }

		public string RelationFieldPanelElementId { get { return UniqueId("RelationFieldPanel"); } }
		public string O2MDefaultPanelElementId { get { return UniqueId("O2MDefaultPanel"); } }
		public string M2MBackwardFieldNamePanelElementId { get { return UniqueId("M2MBackwardFieldNamePanel"); } }
		public string O2MBackwardFieldNamePanelElementId { get { return UniqueId("O2MBackwardFieldNamePanel"); } }
		public string O2MTreePanelElementId { get { return UniqueId("O2MTreePanel"); } }
		public string ClassifierFieldPanelElementId { get { return UniqueId("ClassifierFieldPanel"); } }
		public string M2MDefaultPanelElementId { get { return UniqueId("M2MDefaultPanel"); } }
		public string ListTitleOptionsPanelElementId { get { return UniqueId("ListTitleOptionsPanel"); } }
        

		public string AggregatedElementId { get { return UniqueId("AggregatedCheckbox"); } }
		public string ClassifierSelectElementId { get { return UniqueId("ClassifierSelect"); } }
        public string ListOrderSelectElementId { get { return UniqueId("ListOrderSelect"); } }


		public string RelatedSettingsPanelsSelector
		{
			get
			{
				return String.Format("#{0},#{1},#{2},#{3},#{4},#{5},#{6},#{7}",
					RelationFieldPanelElementId,
					O2MDefaultPanelElementId,
					M2MBackwardFieldNamePanelElementId,
					O2MBackwardFieldNamePanelElementId,
					O2MTreePanelElementId,
					ClassifierFieldPanelElementId,
					M2MDefaultPanelElementId,
					ListTitleOptionsPanelElementId
					);
			}
		}

		public EntityDataListArgs InCombinationWithEventArgs
		{
			get { return new EntityDataListArgs() { MaxListHeight = 200 };}
		}

		[LocalizedDisplayName("ParentField", NameResourceType = typeof(FieldStrings))]
		public string ParentFieldName
		{
			get
			{
				return (Data.ParentField == null) ? String.Empty : Data.ParentField.Name;
			}
		}

		[LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
		public IEnumerable<ListItem> DefaultCommandsListItems
		{
			get;
			private set;
		}

		[LocalizedDisplayName("Commands", NameResourceType = typeof(VisualEditorStrings))]
		public IList<QPCheckedItem> ActiveVeCommands { get; set; }

		public int[] ActiveVeCommandsIds { get { return ActiveVeCommands.Select(c => int.Parse(c.Value)).ToArray(); } }

		public int[] ActiveVeStyleIds { get { return ActiveVeStyles.Union(ActiveVeFormats).Select(c => int.Parse(c.Value)).ToArray(); } }

		public IEnumerable<ListItem> AllStylesListItems
		{
			get;
			private set;
		}


		public IEnumerable<ListItem> AllFormatsListItems
		{
			get;
			private set;
		}

		[LocalizedDisplayName("Styles", NameResourceType = typeof(VisualEditorStrings))]
		public IList<QPCheckedItem> ActiveVeStyles { get; set; }

		[LocalizedDisplayName("Formats", NameResourceType = typeof(VisualEditorStrings))]
		public IList<QPCheckedItem> ActiveVeFormats { get; set; }

		public string AggregationListItems_Data_ExternalCssItems { get; set; }

		public string AggregationListItems_Data_StringEnumItems { get; set; }

		#endregion

		#region Methods		 

		#region Init Model

		private void Init()
		{									
			this.InitInputMask();
			this.InitDynamicImage();			
			this.InitOrder();

			this.InCombinationWith = Data.IsUnique ? this.GetCombinedFields().Select(f => new QPCheckedItem { Value = f.Id.ToString() }).ToList() : new List<QPCheckedItem>();
			entityTypeCode = Data.Content.VirtualType == VirtualType.None ? C.EntityTypeCode.Field : C.EntityTypeCode.VirtualField;
			actionCode = Data.Content.VirtualType == VirtualType.None ? C.ActionCode.FieldProperties : C.ActionCode.VirtualFieldProperties;			
		}

		private void InitOrder()
		{
			if (IsNew && Data.Order == 0 && Data.Content.Fields.Any())
			{
				Data.Order = Data.Content.Fields.Select(f => f.Order).Max();
			}			
		}
				
		private void InitInputMask()
		{

			InputMaskType = InputMaskTypes.Basic;
			MaskTemplateId = null;

			if (!IsNew && !String.IsNullOrEmpty(Data.InputMask))
			{
				var basicTemplate = B.Field.GetAllMaskTemplates().Where(t => t.Mask.Equals(Data.InputMask, StringComparison.InvariantCulture)).FirstOrDefault();
				if (basicTemplate != null)
				{
					MaskTemplateId = basicTemplate.Id;
					InputMaskType = InputMaskTypes.Basic;
				}
				else
				{
					InputMaskType = InputMaskTypes.Custom;
				}
			}
		}

		private void InitDynamicImage()
		{
			if (Data.DynamicImage == null)
			{
				Data.DynamicImage = DynamicImage.Create(Data);
				DynamicImageSizeMode = DynamicImageMode.Size;
			}
			else
			{
				if ((Data.DynamicImage.Height > 0 && Data.DynamicImage.Width > 0) || (Data.DynamicImage.Height == 0 && Data.DynamicImage.Width == 0))
					DynamicImageSizeMode = DynamicImageMode.Size;
				else if (Data.DynamicImage.Height > 0 && Data.DynamicImage.Width <= 0)
					DynamicImageSizeMode = DynamicImageMode.Height;
				else if (Data.DynamicImage.Height <= 0 && Data.DynamicImage.Width > 0)
					DynamicImageSizeMode = DynamicImageMode.Width;
			}
		}		

		#endregion

		#region Update Model

		public void Update()
		{
			UpdateConstraint();
						
			Data.UpdateModel();
			
			UpdateInputMask();

			UpdateDynamicImage();			
						

			if (Data.ExactType != C.FieldExactTypes.VisualEdit)
				Data.FullPage = false;

			if (Data.ExactType != C.FieldExactTypes.File && Data.ExactType != C.FieldExactTypes.Image)
			{
				Data.SubFolder = null;
				Data.UseSiteLibrary = false;
				Data.RenameMatched = false;
				Data.DisableVersionControl = false;
			}
			else
			{
				Data.SubFolder = PathUtility.CorrectSlashes(Data.SubFolder, CorrectSlashMode.ReplaceDoubleSlashes | CorrectSlashMode.ConvertSlashesToBackSlashes | CorrectSlashMode.WrapToSlashes | CorrectSlashMode.RemoveLastSlash);
			}

		}
		
		private void UpdateDynamicImage()
		{
			if (this.Data.ExactType != Constants.FieldExactTypes.DynamicImage)
				this.Data.DynamicImage = null;
			else
			{
				this.Data.DynamicImage.MaxSize = false;
				
                if (this.DynamicImageSizeMode == DynamicImageMode.Size)
					this.Data.DynamicImage.MaxSize = true;
				else if (this.DynamicImageSizeMode == DynamicImageMode.Height)
					this.Data.DynamicImage.Width = 0;
				else if (this.DynamicImageSizeMode == DynamicImageMode.Width)
					this.Data.DynamicImage.Height = 0;

                if (this.Data.DynamicImage.Type != DynamicImage.JPG_EXTENSION)
                    this.Data.DynamicImage.Quality = 0;
			}
		}

		private void UpdateInputMask()
		{
			if (Data.ExactType != C.FieldExactTypes.String)
				Data.UseInputMask = false;

			if (Data.UseInputMask)
			{
				if (this.InputMaskType == InputMaskTypes.Basic)
				{
					if (this.MaskTemplateId.HasValue)
						this.Data.InputMask = B.Field.GetAllMaskTemplates().Single(t => t.Id == this.MaskTemplateId).Mask;					
				}
			}
			else
				this.Data.InputMask = null;
		}		

		/// <summary>
		/// Устанавливает ограничения по уникальности
		/// </summary>
		/// <param name="model"></param>
		private void UpdateConstraint()
		{
			// Если поле уникальное, то добавить Constraint Rule для него если такого еще нет
			// Если не уникальное, то удалить правило для него если такое есть
			if (Data.IsUnique)
			{
				if (this.Data.Constraint == null)
					this.Data.Constraint = new ContentConstraint() { ContentId = this.Data.ContentId };

				this.Data.Constraint.Rules = this.InCombinationWith
					.Distinct(QPCheckedItem.GetComparer<QPCheckedItem>())
					.Select(c =>
						new ContentConstraintRule
						{
							ConstraintId = this.Data.Constraint.Id,
							FieldId = Converter.ToInt32(c.Value)
						})
					.ToList();
				this.Data.Constraint.Rules.Add(new ContentConstraintRule
				{
					ConstraintId = this.Data.Constraint.Id,
					FieldId = this.Data.Id
				});

			}
			else
			{
				// Удалаяем правило для текущего поля если оно есть
				if (this.Data.Constraint != null)
				{
					var currentFieldRule = this.Data.Constraint.Rules.FirstOrDefault(r => r.FieldId == this.Data.Id);
					if (currentFieldRule != null)
						this.Data.Constraint.Rules.Remove(currentFieldRule);
				}
			}
		}
		
		

		#endregion				

		#region List<ListItem> Providers
		
		#region GetAcceptableExactFieldTypes
					
		public bool IsViewInListShown 
		{
			get
			{
				return Data.ExactType != C.FieldExactTypes.Textbox &&
					   Data.ExactType != C.FieldExactTypes.VisualEdit &&
					   Data.ExactType != C.FieldExactTypes.M2MRelation && 
                       Data.ExactType != C.FieldExactTypes.M2ORelation;
			}
		}

		public bool IsNameReadOnly
		{
			get
			{

				return !(
							Data.Content.VirtualType == VirtualType.None ||
							(Data.Content.VirtualType == VirtualType.Join && Data.JoinId.HasValue)
						);
			}
		}

		/// <summary>
		/// Возвращает список допустимых типов для данного поля
		/// </summary>
		/// <returns></returns>
		public List<ListItem> GetAcceptableExactFieldTypes()
		{			
			var acceptableFieldTypes = FieldService.GetAcceptableExactFieldTypes(IsNew ? C.FieldExactTypes.Undefined : Data.ExactType);
			Func<ListItem, bool> acceptableFilter = i => acceptableFieldTypes.Contains((C.FieldExactTypes)Enum.Parse(typeof(C.FieldExactTypes), i.Value));

			Func<int, string> getValue = (id => Translator.Translate(FieldType.AllFieldTypes.Single(f => f.Id == id).Name));
			return new List<B.ListItem>
					{
						new B.ListItem() { Value = C.FieldExactTypes.String.ToString(), Text = getValue(C.FieldTypeCodes.String), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen"} },
						new B.ListItem() { Value = C.FieldExactTypes.Numeric.ToString(), Text = getValue(C.FieldTypeCodes.Numeric), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "IsNumericLong", "NumericIntegerProps"} },
						new B.ListItem() { Value = C.FieldExactTypes.Boolean.ToString(), Text = getValue(C.FieldTypeCodes.Boolean), HasDependentItems = true, DependentItemIDs = new[]{"Unique", "OnScreen", "ViewInList"} },
						new B.ListItem() { Value = C.FieldExactTypes.Date.ToString(), Text = getValue(C.FieldTypeCodes.Date), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Indexed", "ViewInList", "OnScreen"} },
						new B.ListItem() { Value = C.FieldExactTypes.Time.ToString(), Text = getValue(C.FieldTypeCodes.Time), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Indexed", "ViewInList", "OnScreen"} },
						new B.ListItem() { Value = C.FieldExactTypes.DateTime.ToString(), Text = getValue(C.FieldTypeCodes.DateTime), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Indexed", "ViewInList", "OnScreen"} },
						new B.ListItem() { Value = C.FieldExactTypes.File.ToString(), Text = getValue(C.FieldTypeCodes.File), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "FileOrImage"} },
						new B.ListItem() { Value = C.FieldExactTypes.Image.ToString(), Text = getValue(C.FieldTypeCodes.Image), HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "FileOrImage"} },
						new B.ListItem() { Value = C.FieldExactTypes.Textbox.ToString(), Text = getValue(C.FieldTypeCodes.Textbox), HasDependentItems = true, DependentItemIDs = new[]{"Required", "OnScreen", "ViewInList"} },
						new B.ListItem() { Value = C.FieldExactTypes.VisualEdit.ToString(), Text = getValue(C.FieldTypeCodes.VisualEdit), HasDependentItems = true, DependentItemIDs = new[]{"Required", "OnScreen", "ViewInList"} },
						new B.ListItem() { Value = C.FieldExactTypes.O2MRelation.ToString(), Text = FieldStrings.O2MRelation, HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "RelateTo", "LinqBackProperty", "O2MDefault", "O2MBackwardFieldName", "O2MMainProps", "RelationSecurity", "ListTitleOptions"} },
						new B.ListItem() { Value = C.FieldExactTypes.M2MRelation.ToString(), Text = FieldStrings.M2MRelation, HasDependentItems = true, DependentItemIDs = new[]{"Required", "RelateTo", "M2MMaplink", "M2MBackwardFieldName", "RelationSecurity", "ViewInList", "ListOrderOptions"} },
						new B.ListItem() { Value = C.FieldExactTypes.M2ORelation.ToString(), Text = getValue(C.FieldTypeCodes.M2ORelation), HasDependentItems = true, DependentItemIDs = new[]{"Required", "BaseRelation", "ViewInList", "ListOrderOptions" } },
						new B.ListItem() { Value = C.FieldExactTypes.DynamicImage.ToString(), Text = getValue(C.FieldTypeCodes.DynamicImage), HasDependentItems = true },
						new B.ListItem() { Value = C.FieldExactTypes.Classifier.ToString(), Text = FieldStrings.ClassifierId, HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen", "UseTypeSecurity" } },
						new B.ListItem() { Value = C.FieldExactTypes.StringEnum.ToString(), Text = FieldStrings.StringEnum, HasDependentItems = true, DependentItemIDs = new[]{"Required", "Unique", "Indexed", "ViewInList", "OnScreen"} },
					}
					.Where(acceptableFilter)
					.ToList();			
		}
		#endregion

		#region GetAcceptableCombinationFields		
				

		/// <summary>
		/// Получить список полей соответствующего контетна которые могут быть в комбинации с текущим полем
		/// т.е. не входят не в один контент и иммеют разрешенный тип
		/// </summary>
		/// <returns></returns>
		private IEnumerable<B.Field> GetNonUniqueContentFields()
		{
			return Data.Content.Fields
				.Where(f => f.Constraint == null)
				.Where(f =>  B.Field.UniqueFieldTypes.Contains(f.ExactType))
				.ToArray();
		}

		/// <summary>
		/// Скомбинированный с текущим полем
		/// </summary>
		/// <returns></returns>
		private IEnumerable<B.Field> GetCombinedFields()
		{
			if (Data.Constraint != null)
				return Data.Constraint.Rules.Where(r => r.FieldId != Data.Id).Select(r => r.Field)
					.Distinct(B.Field.IdComparer) 
					.ToArray();
			else
				return Enumerable.Empty<B.Field>();
		}
		/// <summary>
		/// Получить список полей в комбинации с которыми находится данное поле
		/// а также те, которые могут быть в комбинации с текущим полем
		/// 
		/// </summary>
		/// <returns></returns>
		public List<B.ListItem> GetAcceptableCombinationFields()
		{			
			var result = GetNonUniqueContentFields();
			if(Data.IsUnique)
				result = result.Concat(GetCombinedFields());
						
			return result
					.Where(f => f.Id != Data.Id)
					.OrderBy(f => f.Id)
					.Distinct(B.Field.IdComparer) 
					.Select(f => new B.ListItem { Text = f.Name, Value = f.Id.ToString() })						
					.ToList();			
		}

		/// <summary>
		/// Возвращает список классификаторов из связанного контента
		/// </summary>
		/// <returns></returns>
		public IEnumerable<B.ListItem> GetAcceptableClassifierFields()
		{
			if (Data.RelatedToContent != null)
			{
				return Data.RelatedToContent.Fields
					.Where(f => f.IsClassifier)
					.OrderBy(f => f.Id)
					.Select(f => new B.ListItem { Text = f.Name, Value = f.Id.ToString() })
					.ToArray();			
			}
			else
				return new[] { new ListItem("", FieldStrings.NoFields) };						
		}
		#endregion

		/// <summary>
		/// Возвращает список контентов доступных для связи с текущим контентом
		/// </summary>
		public IEnumerable<B.ListItem> GetAcceptableContentForRelation()
		{
			var currentContentId = this.Data.ContentId;
			var contentForRelation = ContentService.GetAcceptableContentForRelation(currentContentId).ToArray();			
			return new[] {new ListItem("", FieldStrings.SelectContent)}.Concat(contentForRelation);
		}

		/// <summary>
		/// Возвращает список полей для связи O2M по выбранному контенту
		/// </summary>
		/// <returns></returns>
		public IEnumerable<B.ListItem> GetAcceptableRelatedFields()
		{
			if (Data.RelateToContentId != null)
			{
				return ContentService.GetRelateableFields(Data.RelateToContentId.Value, Data.Id);
			}
			else
				return new[] { new ListItem("", FieldStrings.NoFields) };
		}

        /// <summary>
        /// Возвращает список базовых полей для поля M2O
        /// </summary>
        /// <returns></returns>
        public IEnumerable<B.ListItem> GetBaseFieldsForM2O()
        {
            return new[] { new ListItem("", FieldStrings.SelectField) }.Concat(FieldService.GetBaseFieldsForM2O(Data.ContentId, Data.Id));
        }

		/// <summary>
		/// Возвращает список типов маски ввода
		/// </summary>
		/// <returns></returns>
		public IEnumerable<B.ListItem> GetInputMaskTypes()
		{
			return new[]
			{
				new ListItem(InputMaskTypes.Basic.ToString(), FieldStrings.BasicInputMask, true),
				new ListItem(InputMaskTypes.Custom.ToString(), FieldStrings.CustomInputMask, true)
			};
		}

		/// <summary>
		/// Возвращает список типов маски ввода
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ListItem> GetMaskTemplates()
		{
			return new[] { new ListItem("", FieldStrings.SelectMaskTemplate) }
				.Concat(B.Field.GetAllMaskTemplates().Select(t => new ListItem(t.Id.ToString(), t.Name)));
			
		}

		/// <summary>
		/// Возвращает список полей типа Image текущего контента
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ListItem> GetContentImageFields()
		{						
			return new[] { new ListItem("", FieldStrings.SelectField) }
				.Concat(Data.Content.Fields.Where(f => f.ExactType == C.FieldExactTypes.Image && f.Id != Data.Id).Select(f => new ListItem(f.Id.ToString(), f.Name)));
		}

		/// <summary>
		/// Возвращает список режимов ресайза Dynamic Image
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ListItem> GetDynamicImageModes()
		{
			return new[]
			{
				new ListItem(DynamicImageMode.Size.ToString(), FieldStrings.DynamicImageModeSize, true){DependentItemIDs = new[]{"HeightModePanel", "WidthModePanel"}},
				new ListItem(DynamicImageMode.Height.ToString(), FieldStrings.DynamicImageModeHeight, true){DependentItemIDs = new[]{"HeightModePanel"}},
				new ListItem(DynamicImageMode.Width.ToString(), FieldStrings.DynamicImageModeWidth, true){DependentItemIDs = new[]{"WidthModePanel"}}
			};
		}

		/// <summary>
		/// Возвращает список типов файлов для Dynamic Image
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ListItem> GetDynamicImageFileTypes()
		{
			return new[]
			{
				new ListItem(DynamicImage.JPG_EXTENSION, DynamicImage.JPG_EXTENSION, true),
				new ListItem(DynamicImage.PNG_EXTENSION, DynamicImage.PNG_EXTENSION),
				new ListItem(DynamicImage.GIF_EXTENSION, DynamicImage.GIF_EXTENSION)
			};
		}

		/// <summary>
		/// Возвращает варианты размещения поля относительно других полей
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ListItem> GetOrderOptions()
		{
			List<ListItem> result = new List<ListItem>();
			var orderedField = Data.Content.Fields								
								.OrderBy(f => f.Order)
								.ToArray();
			if(orderedField.Length > 0)
			{
				if (orderedField[0].Id != Data.Id)
				    result.Add(new ListItem("0", String.Format(FieldStrings.OrderOptionTemplate, orderedField[0].Name)));
				if (orderedField.Length > 1)
				{
					for (int i = 0; i < orderedField.Length - 1; i++)
					{
						if (orderedField[i + 1].Id != Data.Id)
							result.Add(new ListItem(orderedField[i].Order.ToString(), String.Format(FieldStrings.OrderOptionTemplate, orderedField[i + 1].Name)));
					}
				}
				result.Add(new ListItem(orderedField[orderedField.Length - 1].Order.ToString(), FieldStrings.OrderOptionAfterAll));
			}
			else
				result.Add(new ListItem("0", FieldStrings.OrderOptionAfterAll));
			
			return result;
		}

		public QPSelectListItem O2MDefaultValueListItem
		{
			get
			{				
				if (Data.ExactType == FieldExactTypes.O2MRelation)
				{
					return Data.O2MDefaultValue != null ?
						new QPSelectListItem { Value = Data.O2MDefaultValue, Text = Data.O2MDefaultValueName, Selected = true } :
						null;
				}

				return null;
			}
		}

		public QPSelectListItem M2MDefaultValueListItem
		{
			get
			{
				return Data.M2MDefaultValue != null ?
					new QPSelectListItem { Value = Data.M2MDefaultValue, Text = Data.O2MDefaultValueName, Selected = true } :
					null;
			}
		}

		/// <summary>
		/// Возвращает список контентов для классификатора
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ListItem> GetAggregetableContentsForClassifier()
		{
			return FieldService.GetAggregetableContentsForClassifier(this.Data);
		}

		public IEnumerable<B.ListItem> GetFieldsForTreeOrder()
		{
			int contentId = (Data.RelateToContentId.HasValue) ? Data.RelateToContentId.Value: Data.ContentId;
            return FieldService.GetFieldsForTreeOrder(contentId, Data.Id);
		}

		public QPSelectListItem RelateToListItem
		{
			get
			{
				return Data.RelateToContentId.HasValue ?
					new QPSelectListItem { Value = Data.RelateToContentId.Value.ToString(), Text = ContentService.GetNameById(Data.RelateToContentId.Value), Selected = true } :
					null;
			}
		}

		[LocalizedDisplayName("DefaultValue", NameResourceType = typeof(FieldStrings))]
		public IEnumerable<int> DefaultArticleIds { get; set; }

		public IEnumerable<ListItem> DefaultArticleListItems { get; set; }
		#endregion

		#region Binding

		internal void DoCustomBinding()
		{
			if (!String.IsNullOrWhiteSpace(AggregationListItems_Data_ExternalCssItems))
			{
				Data.ExternalCssItems = new JavaScriptSerializer().Deserialize<List<ExternalCss>>(AggregationListItems_Data_ExternalCssItems);
				Data.ExternalCss = ExternalCssHelper.ConvertToString(Data.ExternalCssItems);
			}

			Data.ParseStringEnumJson(AggregationListItems_Data_StringEnumItems);
			Data.DefaultArticleIds = DefaultArticleIds.ToArray();
			Data.ActiveVeStyleIds = ActiveVeStyleIds;
			Data.ActiveVeCommandIds = ActiveVeCommandsIds;
		} 
		#endregion
		
		#endregion

		public EntityDataListArgs EntityDataListArgs 
		{ 
			get
			{
				return new EntityDataListArgs
				{
					EntityTypeCode = C.EntityTypeCode.Article,
					ParentEntityId = Data.RelateToContentId,
					SelectActionCode = C.ActionCode.MultipleSelectArticle,
					Filter = Data.RelationFilter,
					ListId = Data.Id,
					MaxListHeight = 200,
					MaxListWidth = 350,
					ShowIds = true
				};
			}
		}
	}
}
