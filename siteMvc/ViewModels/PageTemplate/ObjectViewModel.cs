using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;


namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
	public class ObjectViewModel : LockableEntityViewModel
	{
		private IObjectService _service;

		public string AggregationListItems_SortingItems { get; set; }
		
		public string AggregationListItems_Data_DefaultValues { get; set; }

		private int _parentId;		
		
		public override string EntityTypeCode
		{
			get { return Data.PageOrTemplate ? C.EntityTypeCode.PageObject : C.EntityTypeCode.TemplateObject; }
		}

		internal static ObjectViewModel Create(B.BllObject obj, string tabId, int parentId, IObjectService _objectService)
		{
			var model = EntityViewModel.Create<ObjectViewModel>(obj, tabId, parentId);
			model._service = _objectService;
			model._parentId = parentId;

			var activeStatuses = _objectService.GetActiveStatusesByObjectId(model.Data.Id).OrderBy(x => x.Weight).ToList();
			if (activeStatuses.Count() == 0)
				activeStatuses.Add(B.StatusType.GetPublished(obj.PageTemplate.SiteId));
			model.ActiveStatusTypeIds = activeStatuses.Select(x => x.Id).ToList();
			model.ActiveStatusTypeListItems = activeStatuses.Select(x => new ListItem { Value = x.Id.ToString(), Selected = true, Text = x.Name }).ToList();
			model._parentTemplateObjects = new InitPropertyValue<List<BllObject>>(() => _objectService.GetFreeTemplateObjectsByPageId(model._parentId).ToList());
			model._SortingItems = new InitPropertyValue<IEnumerable<SortingItem>>(() => (model.Data.Container == null || string.IsNullOrEmpty(model.Data.Container.OrderStatic)) ? Enumerable.Empty<SortingItem>() :
					SortingItemHelper.Deserialize(model.Data.Container.OrderStatic));
			model._netLanguages = new InitPropertyValue<List<B.ListItem>>(() => _objectService.GetNetLanguagesAsListItems().ToList());

			model.EntityDataListArgs = new EntityDataListArgs
			{
				EntityTypeCode = C.EntityTypeCode.StatusType,
				ParentEntityId = (model.Data.Container!=null && model.Data.Container.Content!=null && model.Data.Container.Content.WorkflowBinding !=null) ? 
				model.Data.Container.Content.WorkflowBinding.WorkflowId : 0,
				SelectActionCode = C.ActionCode.MultipleSelectStatusesForWorkflow,
				ListId = -1 * System.DateTime.Now.Millisecond,
				MaxListHeight = 200,
				MaxListWidth = 350
			};
			/*
			if (model.Data.IsObjectContainerType)
			{
				if (model.Data.Container.Content != null)
				{
					model.AdditionalDataForAggregationList = new Dictionary<string, string>()
					{
						{"fields", String.Join(",", ServiceField.CreateAll().Select(f => f.ColumnName)
							.Concat(model.Data.Container.Content.Fields.Select(x => x.Name)))},
						{"orders", TemplateStrings.Ascending + "," + TemplateStrings.Descending}
					};
				}
			}
			*/
			model.PublishedId = _objectService.GetPublishedStatusIdBySiteId(model.Data.PageTemplate.SiteId);

			model.SelectionIsStarting = string.IsNullOrEmpty(model.Data.Container.SelectStart) ? ArticleSelectionMode.FromTheFirstArticle : ArticleSelectionMode.FromTheSpecifiedArticle;
			model.SelectionIncludes = string.IsNullOrEmpty(model.Data.Container.SelectTotal) ? ArticleSelectionIncludeMode.AllArticles : ArticleSelectionIncludeMode.SpecifiedNumberOfArticles;

			if (string.IsNullOrEmpty(model.Data.Container.SelectStart))
				model.Data.Container.SelectStart = "1";
			if (string.IsNullOrEmpty(model.Data.Container.SelectTotal))
				model.Data.Container.SelectTotal = "10";
			if (!model.Data.Container.Duration.HasValue)
				model.Data.Container.Duration = 10;
			return model;
		}

		public override string ActionCode
		{
			get 
			{
				if (IsNew) { return Data.PageOrTemplate ? C.ActionCode.AddNewPageObject : C.ActionCode.AddNewTemplateObject; }
				else { return Data.PageOrTemplate ? C.ActionCode.PageObjectProperties : C.ActionCode.TemplateObjectProperties; }
			}
		}

		public new B.BllObject Data
		{
			get
			{
				return (B.BllObject)EntityData;
			}
			set
			{
				EntityData = value;
			}
		}

		private InitPropertyValue<List<B.ListItem>> _netLanguages;
		public List<B.ListItem> NetLanguages
		{
			get
			{
				return _netLanguages.Value;
			}
		}

		private InitPropertyValue<List<BllObject>> _parentTemplateObjects;
		public List<BllObject> ParentTemplateObjects
		{
			get
			{
				return _parentTemplateObjects.Value;
			}
		}

		public List<B.ListItem> ParentTemplateObjectsAsListItems
		{
			get
			{
				return ParentTemplateObjects.Select(x => new B.ListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
			}
		}

		private List<B.ListItem> _types;

		public List<B.ListItem> Types 
		{
			get
			{
				if (_types == null)
				{
					_types = _service.GetTypes().ToList();
				}
				return _types;
			}
		}

		internal void DoCustomBinding()
		{
			if (Data.IsSiteDotNet && string.IsNullOrWhiteSpace(Data.NetName))
				Data.GenerateNetName();

			if (this.Data.OverrideTemplateObject)
			{
				Data.ObjectInheritedFrom = _service.ReadObjectProperties(Data.ParentObjectId.Value, false);
				Data.Name = Data.ObjectInheritedFrom.Name;
				Data.NetName = Data.ObjectInheritedFrom.NetName;
			}

			if (Data.IsObjectContainerType)
			{
				Data.Container.DoCustomBinding(SelectionIsStarting, SelectionIncludes);				
				SortingItems = new JavaScriptSerializer().Deserialize<List<SortingItem>>(AggregationListItems_SortingItems);
				Data.Container.OrderStatic = SortingItemHelper.Serialize(SortingItems);				
			}

			if(Data.UseDefaultValues)
				Data.DefaultValues = new JavaScriptSerializer().Deserialize<List<DefaultValue>>(AggregationListItems_Data_DefaultValues);
		}

		public List<B.ListItem> DefaultFormats 
		{			
			get
			{
				return Data.ChildObjectFormats.Select(x => new B.ListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == Data.DefaultFormatId }).ToList();				
			}
		}

		public SelectOptions SelectDefaultFormatOptions
		{
			get
			{
				SelectOptions options = new SelectOptions();
				options.EntityDataListArgs = new EntityDataListArgs();
				options.EntityDataListArgs.EntityTypeCode = Data.PageOrTemplate ? Constants.EntityTypeCode.PageObjectFormat : Constants.EntityTypeCode.TemplateObjectFormat;
				options.EntityDataListArgs.ParentEntityId = Data.Id;
				options.EntityDataListArgs.EntityId = Data.DefaultFormatId;
				options.EntityDataListArgs.AddNewActionCode = Data.PageOrTemplate ? Constants.ActionCode.AddNewPageObjectFormat : Constants.ActionCode.AddNewTemplateObjectFormat;
				options.EntityDataListArgs.ReadActionCode = Data.PageOrTemplate ? Constants.ActionCode.PageObjectFormatProperties : Constants.ActionCode.TemplateObjectFormatProperties;
				return options;
			}
		}

		public QPSelectListItem ContainerContentListItem
		{
            get
            {
				if (Data.Container.ContentId == null)
					return null;
				var content = _service.GetContentById(Data.Container.ContentId.Value);
				return
					new QPSelectListItem { Value = content.Id.ToString(), Text = content.Name, Selected = true };
            }
        }

		public QPSelectListItem FormContentListItem
		{
			get
			{
				if (Data.ContentForm.ContentId == null)
					return null;
				var content = _service.GetContentById(Data.ContentForm.ContentId.Value);
				return
					new QPSelectListItem { Value = content.Id.ToString(), Text = content.Name, Selected = true };
			}
		}

		public QPSelectListItem SitePageListItem 
		{ 
			get
			{
				if (Data.ContentForm == null || Data.ContentForm.ThankYouPageId == null)
					return null;
				var page = _service.ReadPageProperties(Data.ContentForm.ThankYouPageId.Value);
				return
					new QPSelectListItem { Value = page.Id.ToString(), Text = page.Name, Selected = true };
			}
		}			

		public List<B.ListItem>FiltrationLevels
		{
			get
			{
				return new List<B.ListItem>() {                    
                    new B.ListItem(C.ObjectSecurityMode.OnlyMatching.ToString(), TemplateStrings.ReturnOnlyMatchingArticles),
					new B.ListItem(C.ObjectSecurityMode.All.ToString(), TemplateStrings.ReturnAllArticles)
                };
			}
		}

		public List<B.ListItem> ReturnArticlesModes
		{
			get
			{
				return new List<B.ListItem>() {
                    new B.ListItem(C.ArticleReturnMode.Sequentially.ToString(), TemplateStrings.Sequentially),
					new B.ListItem(C.ArticleReturnMode.Randomly.ToString(), TemplateStrings.Randomly)
                };
			}
		}

		private List<B.ListItem> _permissionLevels;

		public List<B.ListItem> PermissionLevels
		{
			get
			{
				if (_permissionLevels == null)
				{
					_permissionLevels = _service.GetPermissionLevels().ToList();
				}
				return _permissionLevels;
			}
		}

		private InitPropertyValue<IEnumerable<SortingItem>> _SortingItems;

		[LocalizedDisplayName("DefaultSorting", NameResourceType = typeof(TemplateStrings))]
		public IEnumerable<SortingItem> SortingItems
		{
			get { return _SortingItems.Value;  }
			set { _SortingItems.Value = value; }
		}				

		public IEnumerable<B.ListItem> SelectionStartingModes 
		{
			get 
			{
				return new List<B.ListItem>() {
                    new B.ListItem(C.ArticleSelectionMode.FromTheFirstArticle.ToString(), TemplateStrings.FromTheFirstArticle){HasDependentItems = true, DependentItemIDs = new[]{"FromTheFirstPanel"}},
					new B.ListItem(C.ArticleSelectionMode.FromTheSpecifiedArticle.ToString(), TemplateStrings.FromTheSpecifiedArticle){HasDependentItems = true, DependentItemIDs = new[]{"FromTheSpecialPanel"}}
                };
			}
		}		

		public IEnumerable<B.ListItem> SelectionIncludeModes
		{
			get
			{
				return new List<B.ListItem>() {					
                    new B.ListItem(C.ArticleSelectionIncludeMode.AllArticles.ToString(), TemplateStrings.AllArticles){HasDependentItems = true, DependentItemIDs = new[]{"AllArticlesPanel"}},
					new B.ListItem(C.ArticleSelectionIncludeMode.SpecifiedNumberOfArticles.ToString(), TemplateStrings.SpecifiedNumberOfArticles){HasDependentItems = true, DependentItemIDs = new[]{"SpecialArticlesPanel"}}
                };
			}
		}		

		public override void Validate(ModelStateDictionary modelState)
		{
			base.Validate(modelState);		
			var duplicateNames = SortingItems.GroupBy(c => c.Field).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
			var sortingArray = SortingItems.ToArray();
			for (int i = 0; i < sortingArray.Length; i++)
				ValidateSortingItem(sortingArray[i], i + 1, duplicateNames, modelState);
		}

		private void ValidateSortingItem(SortingItem item, int index, string[] dupNames, ModelStateDictionary modelState)
		{
			if (dupNames.Contains(item.Field))
			{
				modelState.AddModelError(string.Empty, String.Format(TemplateStrings.SortExperessionFieldNotUnique, index));
				item.Invalid = true;
				return;
			}
		}

		public string ParentTemplateObjectsData 
		{
			get
			{
				return new JavaScriptSerializer().Serialize(ParentTemplateObjects.Select(x => new { x.Name, x.NetName, x.Id }));
			}
		}

		public string ShowGlobalTypeIds
		{
			get
			{
				return ObjectType.GetCss().Id.ToString() + "," + ObjectType.GetJavaScript().Id.ToString();
			}
		}

		[LocalizedDisplayName("Statuses", NameResourceType = typeof(WorkflowStrings))]
		public IEnumerable<int> ActiveStatusTypeIds { get; set; }

		[LocalizedDisplayName("Statuses", NameResourceType = typeof(WorkflowStrings))]
		public IEnumerable<ListItem> ActiveStatusTypeListItems { get; set; }

		public EntityDataListArgs EntityDataListArgs { get; set; }

		public int PublishedId { get; private set; }

		public bool HasWorkflow 
		{ 
			get 
			{
				return (Data.Container != null && Data.Container.Content != null &&
					Data.Container.Content.WorkflowBinding != null && Data.Container.Content.WorkflowBinding.WorkflowId != 0);
			} 
		}

		[LocalizedDisplayName("SelectionIsStarting", NameResourceType = typeof(TemplateStrings))]
		public int SelectionIsStarting { get; set; }

		[LocalizedDisplayName("SelectionIncludes", NameResourceType = typeof(TemplateStrings))]
		public int SelectionIncludes { get; set; }

		public override string CaptureLockActionCode
		{
			get
			{
				return Data.PageOrTemplate ? C.ActionCode.CaptureLockPageObject : C.ActionCode.CaptureLockTemplateObject;
			}
		}

		public string FilterHtaClass
		{
			get
			{
				if (!Data.IsSiteDotNet)
					return "hta-VBSTextArea";
				else
				{
					if (Data.DefaultFormat != null)
					{
						return HighlightModeSelectHelper.SelectMode(Data.DefaultFormat.NetLanguageId);
					}

					else return "hta-cSharpTextArea";
				}
			}
		}
	}
}