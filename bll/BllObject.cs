using System;
using Quantumart.QP8.Resources;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Validators;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Utils;
using System.Text.RegularExpressions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL
{
	public class BllObject : LockableEntityObject
	{		
		private int _ContainerObjectTypeId = ObjectType.GetContainer().Id;

		private int _PublishingFormObjectTypeId = ObjectType.GetForm().Id;

		private int _JavaScriptObjectTypeId = ObjectType.GetJavaScript().Id;

		private int _CssObjectTypeId = ObjectType.GetCss().Id;

		private int _GenericTypeId = ObjectType.GetGeneric().Id;

		[LocalizedDisplayName("ParentTemplateObject", NameResourceType = typeof(TemplateStrings))]
		public int? ParentObjectId { get; set; }

		[LocalizedDisplayName("Type", NameResourceType = typeof(TemplateStrings))]
		public int TypeId { get; set; }

		[LocalizedDisplayName("Global", NameResourceType = typeof(TemplateStrings))]
		public bool Global { get; set; }

		[LocalizedDisplayName("DefaultFormat", NameResourceType = typeof(TemplateStrings))]
		public int? DefaultFormatId { get; set; }

		public IEnumerable<ObjectFormat> ChildObjectFormats { get; set; }

		[LocalizedDisplayName("EnableViewState", NameResourceType = typeof(TemplateStrings))]
		public bool EnableViewState { get; set; }

		[LocalizedDisplayName("DisableAutoDataBinding", NameResourceType = typeof(TemplateStrings))]
		public bool DisableDatabind { get; set; }

		[LocalizedDisplayName("CustomClass", NameResourceType = typeof(TemplateStrings))]
		public string ControlCustomClass { get; set; }

		[LocalizedDisplayName("UseDefaultValues", NameResourceType = typeof(TemplateStrings))]
		public bool UseDefaultValues { get; set; }

		[LocalizedDisplayName("NetClassName", NameResourceType = typeof(TemplateStrings))]
		public string NetName { get; set; }

		[LocalizedDisplayName("EnableOnScreen", NameResourceType = typeof(TemplateStrings))]
		public bool AllowStageEdit { get; set; }

		public ObjectFormat DefaultFormat { get; set; }

		public BllObject()
		{
			_contentForm = new InitPropertyValue<ContentForm>(() => InitContentForm());
			_container = new InitPropertyValue<Container>(() => InitContainer());
			InitDefaultValues();			
		}

		public void InitDefaultValues()
		{
			_DefaultValues = ObjectRepository.GetDefaultValuesByObjectId(Id).Select(x => new DefaultValue { VariableName = x.VariableName, VariableValue = x.VariableValue }).ToList();
		}

		private ContentForm InitContentForm()
		{
			if (IsObjectFormType)
				return PageTemplateRepository.GetContentFormByObjectId(this.Id) ?? new ContentForm() { ObjectId = Id, ContentId = null, GenerateUpdateScript = true };
			else
				return new ContentForm();
		}

		private Container InitContainer()
		{
			if (IsObjectContainerType)
			{
				var container = PageTemplateRepository.GetContainerByObjectId(this.Id) ?? new Container() { ObjectId = Id, ContentId = null};
				if (container.ContentId != null)
				{
					container.AdditionalDataForAggregationList = new Dictionary<string, string>()
					{
						{"fields", String.Join(",", ServiceField.CreateAll().Select(f => f.ColumnName)
							.Concat(container.Content.Fields.Select(x => x.Name)))},
						{"orders", TemplateStrings.Ascending + "," + TemplateStrings.Descending}
					};
				}
				container.AllowDynamicContentChanging = !string.IsNullOrWhiteSpace(container.DynamicContentVariable);
				if (!container.ApplySecurity)
				{
					container.UseLevelFiltration = false;
					container.StartLevel = EntityPermissionLevel.GetList().Id;
					container.EndLevel = EntityPermissionLevel.GetFullAccess().Id;					
				}
				return container;
			}
			else
				return new Container { UseLevelFiltration = false, AdditionalDataForAggregationList =				
					new Dictionary<string, string>()
					{
						{"fields", String.Join(",", ServiceField.CreateAll().Select(f => f.ColumnName))},
						{"orders", TemplateStrings.Ascending + "," + TemplateStrings.Descending}
					}
				};
		}

		internal static BllObject Create(int parentId, bool pageOrTemplate)//true-page false-template
		{			
			var obj = new BllObject();
			if (pageOrTemplate)
			{
				obj.PageId = parentId;
				obj.PageTemplateId = obj.page.TemplateId;
				obj.PageTemplate = PageTemplateRepository.GetPageTemplatePropertiesById(obj.page.TemplateId);
			}
			else
			{
				obj.PageTemplateId = parentId;
				obj.PageTemplate = PageTemplateRepository.GetPageTemplatePropertiesById(parentId);
			}
			obj.TypeId = ObjectType.GetGeneric().Id;
			return obj;
		}

		public int? PageId { get; set; }

		public IEnumerable<BllObject> InheritedObjects { get; set; }

		public BllObject ObjectInheritedFrom { get; set; }

		public int PageTemplateId { get; set; }

		public PageTemplate PageTemplate { get; set; }

		public ObjectType ObjectType { get; set; }

		/// <summary>
		/// TRUE- it`s page object, false -it`s template object
		/// </summary>
		public bool PageOrTemplate { get { return PageId.HasValue; } }

		public override string LockedByAnyoneElseMessage
		{
			get { return string.Format(TemplateStrings.ObjectLockedByAnyoneElse, LockedBy); }
		}

		public override string EntityTypeCode
		{
			get
			{
				return PageOrTemplate ? Constants.EntityTypeCode.PageObject : Constants.EntityTypeCode.TemplateObject;
			}
		}

		public override int ParentEntityId
		{
			get
			{
				return PageOrTemplate ? PageId.Value : PageTemplateId;
			}
		}

		public bool IsSiteDotNet { get { return PageTemplate.SiteIsDotNet; } }

		private Page _page;

		public Page page 
		{ 
			get 
			{
				if(_page == null && PageId.HasValue)
					_page = PageRepository.GetPagePropertiesById(PageId.Value);
				return _page;
			}
		}

		private List<DefaultValue> _DefaultValues;

		public IEnumerable<DefaultValue> DefaultValues
		{
			get { return _DefaultValues; }
			set { _DefaultValues = value.ToList(); }
		}

		public override void Validate()
		{
			RulesException<BllObject> errors = new RulesException<BllObject>();
			base.Validate(errors);

			if (OverrideTemplateObject)
			{
				if(ParentObjectId == null)
					errors.ErrorFor(x => x.ParentObjectId, TemplateStrings.ParentObjectIdRequired);
			}

			if (PageTemplate.SiteIsDotNet)
			{

				if (!string.IsNullOrWhiteSpace(NetName))
				{
					if (!Regex.IsMatch(NetName, RegularExpressions.NetName))
						errors.ErrorFor(x => x.NetName, TemplateStrings.NetNameInvalidFormat);
					if (!ObjectRepository.ObjectNetNameUnique(NetName, PageId, PageTemplateId, PageOrTemplate, Id))
						errors.ErrorFor(x => x.NetName, TemplateStrings.NetNameNotUnique);
				}

				if (!string.IsNullOrWhiteSpace(ControlCustomClass))
				{
					if (!Regex.IsMatch(ControlCustomClass, RegularExpressions.NetName))
						errors.ErrorFor(x => x.ControlCustomClass, TemplateStrings.CustomClassInvalidFormat);
					if(ControlCustomClass.Length > 255)
						errors.ErrorFor(x => x.ControlCustomClass, TemplateStrings.CustomClassMaxLengthExceeded);
				}				
			}

			if (IsObjectContainerType)
			{
				if(Container.ContentId == null)
					errors.ErrorFor(x => x.Container.ContentId, TemplateStrings.ContentRequired);
				if (!string.IsNullOrWhiteSpace(Container.FilterValue) && Container.FilterValue.Length > 255)
					errors.ErrorFor(x => x.Container.FilterValue, TemplateStrings.FilterMaxLengthExceeded);
				if (Container.AllowOrderDynamic)
				{
					if (string.IsNullOrWhiteSpace(Container.OrderDynamic))
						errors.ErrorFor(x => x.Container.OrderDynamic, TemplateStrings.DynamicSortingExpressionRequired);
					else if (Container.OrderDynamic.Length > 512)
						errors.ErrorFor(x => x.Container.OrderDynamic, TemplateStrings.DynamicSortingExpressionMaxLengthExceeded);
				}
				if (Container.AllowDynamicContentChanging)
				{
					if (string.IsNullOrWhiteSpace(Container.DynamicContentVariable))
						errors.ErrorFor(x => x.Container.DynamicContentVariable, TemplateStrings.DynamicContentVariableRequired);
					else if (Container.DynamicContentVariable.Length > 255)
						errors.ErrorFor(x => x.Container.DynamicContentVariable, TemplateStrings.DynamicContentVariableMaxLengthExceeded);
				}
				if (Container.AllowOrderDynamic)
				{
					if (string.IsNullOrWhiteSpace(Container.OrderDynamic))
						errors.ErrorFor(x => x.Container.OrderDynamic, TemplateStrings.OrderDynamicRequired);
					else if (Container.OrderDynamic.Length > 255)
						errors.ErrorFor(x => x.Container.OrderDynamic, TemplateStrings.OrderDynamicMaxLengthExceeded);
				}

				if (!Container.StartsFromFirstArticle)
				{
					if (string.IsNullOrWhiteSpace(Container.SelectStart))
						errors.ErrorFor(x => x.Container.SelectStart, TemplateStrings.SelectStartRequired);
					else if (Container.SelectStart.Length > 255)
						errors.ErrorFor(x => x.Container.SelectStart, TemplateStrings.SelectStartMaxLengthExceeded);
				}

				if (!Container.IncludesAllArticles)
				{
					if (string.IsNullOrWhiteSpace(Container.SelectTotal))
						errors.ErrorFor(x => x.Container.SelectTotal, TemplateStrings.SelectTotalRequired);
					else if (Container.SelectTotal.Length > 255)
						errors.ErrorFor(x => x.Container.SelectTotal, TemplateStrings.SelectTotalMaxLengthExceeded);
				}
			}

			else if (IsObjectFormType)
			{
				if (ContentForm.ContentId == null)
					errors.ErrorFor(x => x.ContentForm.ContentId, TemplateStrings.ContentRequired);
			}

			if (UseDefaultValues)
			{
				var duplicateNames = DefaultValues.GroupBy(c => c.VariableName).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
				var defaultArray = DefaultValues.ToArray();
				for (int i = 0; i < defaultArray.Length; i++)
					ValidateDefaultValue(defaultArray[i], errors, i + 1, duplicateNames);
			}

			if (!errors.IsEmpty)
				throw errors;
		}
		
		private void ValidateDefaultValue(DefaultValue dValue, RulesException<BllObject> errors, int index, string[] dupNames)
		{
			if (dupNames.Contains(dValue.VariableName))
			{
				errors.ErrorForModel(String.Format(TemplateStrings.DefaultValueNameNotUnique, index));
				dValue.Invalid = true;
				return;
			}

			if (string.IsNullOrWhiteSpace(dValue.VariableName))
			{
				errors.ErrorForModel(String.Format(TemplateStrings.DefaultValueNameRequired, index));
				dValue.Invalid = true;
				return;
			}

			if (string.IsNullOrWhiteSpace(dValue.VariableValue))
			{
				errors.ErrorForModel(String.Format(TemplateStrings.DefaultValueValueRequired, index));
				dValue.Invalid = true;
				return;
			}

			if (dValue.VariableName.Length > 255)
			{
				errors.ErrorForModel(String.Format(TemplateStrings.DefaultValueNameMaxLengthExceeded, index));
				dValue.Invalid = true;
				return;
			}

			if (dValue.VariableValue.Length > 255)
			{
				errors.ErrorForModel(String.Format(TemplateStrings.DefaultValueValueMaxLengthExceeded, index));
				dValue.Invalid = true;
				return;
			}
		}

		[LocalizedDisplayName("OverrideTemplateObject", NameResourceType = typeof(TemplateStrings))]
		public bool OverrideTemplateObject { get; set; }

		public bool IsObjectContainerType { get { return TypeId == 2;/*_ContainerObjectTypeId;*/ } }

		public bool IsObjectFormType { get { return TypeId == _PublishingFormObjectTypeId; } }

		public bool IsJavaScriptType { get { return TypeId == _JavaScriptObjectTypeId; } }

		public bool IsCssType { get { return TypeId == _CssObjectTypeId; } }

		public bool IsGenericType { get { return TypeId == _GenericTypeId; } }

		private InitPropertyValue<ContentForm> _contentForm;	

		public ContentForm ContentForm
		{
			get { return _contentForm.Value; }
			set { _contentForm.Value = value; }
		}

		private InitPropertyValue<Container> _container;	

		public Container Container
		{
			get { return _container.Value; }
			set { _container.Value = value; }
		}

		internal void BindWithStatuses(IEnumerable<int> activeStatuses, bool isContainer)
		{
			List<int> stats = activeStatuses.ToList();			
			ObjectRepository.SetObjectActiveStatuses(this.Id, stats, isContainer);
		}

		public void GenerateNetName()
		{
			if (!string.IsNullOrEmpty(this.Name))
				this.NetName = this.Name.Replace(" ", "_");
		}
	}
}
