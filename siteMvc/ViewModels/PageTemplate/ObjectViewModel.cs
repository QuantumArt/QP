using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class ObjectViewModel : LockableEntityViewModel
    {
        private IObjectService _service;

        public string AggregationListItemsSortingItems { get; set; }

        public string AggregationListItemsDataDefaultValues { get; set; }

        private int _parentId;

        public override string EntityTypeCode => Data.PageOrTemplate ? Constants.EntityTypeCode.PageObject : Constants.EntityTypeCode.TemplateObject;

        internal static ObjectViewModel Create(BllObject obj, string tabId, int parentId, IObjectService objectService)
        {
            var model = Create<ObjectViewModel>(obj, tabId, parentId);
            model._service = objectService;
            model._parentId = parentId;

            var activeStatuses = objectService.GetActiveStatusesByObjectId(model.Data.Id).OrderBy(x => x.Weight).ToList();
            if (!activeStatuses.Any())
            {
                activeStatuses.Add(BLL.StatusType.GetPublished(obj.PageTemplate.SiteId));
            }

            model.ActiveStatusTypeIds = activeStatuses.Select(x => x.Id).ToList();
            model.ActiveStatusTypeListItems = activeStatuses.Select(x => new ListItem { Value = x.Id.ToString(), Selected = true, Text = x.Name }).ToList();
            model._parentTemplateObjects = new InitPropertyValue<List<BllObject>>(() => objectService.GetFreeTemplateObjectsByPageId(model._parentId).ToList());
            model._sortingItems = new InitPropertyValue<IEnumerable<SortingItem>>(() => string.IsNullOrEmpty(model.Data.Container?.OrderStatic) ? Enumerable.Empty<SortingItem>() : SortingItemHelper.Deserialize(model.Data.Container.OrderStatic));
            model._netLanguages = new InitPropertyValue<List<ListItem>>(() => objectService.GetNetLanguagesAsListItems().ToList());
            model.EntityDataListArgs = new EntityDataListArgs
            {
                EntityTypeCode = Constants.EntityTypeCode.StatusType,
                ParentEntityId = model.Data.Container?.Content?.WorkflowBinding?.WorkflowId ?? 0,
                SelectActionCode = Constants.ActionCode.MultipleSelectStatusesForWorkflow,
                ListId = -1 * DateTime.Now.Millisecond,
                MaxListHeight = 200,
                MaxListWidth = 350
            };

            model.PublishedId = objectService.GetPublishedStatusIdBySiteId(model.Data.PageTemplate.SiteId);
            model.SelectionIsStarting = string.IsNullOrEmpty(model.Data.Container.SelectStart) ? ArticleSelectionMode.FromTheFirstArticle : ArticleSelectionMode.FromTheSpecifiedArticle;
            model.SelectionIncludes = string.IsNullOrEmpty(model.Data.Container.SelectTotal) ? ArticleSelectionIncludeMode.AllArticles : ArticleSelectionIncludeMode.SpecifiedNumberOfArticles;

            if (string.IsNullOrEmpty(model.Data.Container.SelectStart))
            {
                model.Data.Container.SelectStart = "1";
            }

            if (string.IsNullOrEmpty(model.Data.Container.SelectTotal))
            {
                model.Data.Container.SelectTotal = "10";
            }

            if (!model.Data.Container.Duration.HasValue)
            {
                model.Data.Container.Duration = 10;
            }

            return model;
        }

        public override string ActionCode
        {
            get
            {
                if (IsNew) { return Data.PageOrTemplate ? Constants.ActionCode.AddNewPageObject : Constants.ActionCode.AddNewTemplateObject; }

                return Data.PageOrTemplate ? Constants.ActionCode.PageObjectProperties : Constants.ActionCode.TemplateObjectProperties;
            }
        }

        public new BllObject Data
        {
            get
            {
                return (BllObject)EntityData;
            }
            set
            {
                EntityData = value;
            }
        }

        private InitPropertyValue<List<ListItem>> _netLanguages;
        public List<ListItem> NetLanguages => _netLanguages.Value;

        private InitPropertyValue<List<BllObject>> _parentTemplateObjects;
        public List<BllObject> ParentTemplateObjects => _parentTemplateObjects.Value;

        public List<ListItem> ParentTemplateObjectsAsListItems
        {
            get
            {
                return ParentTemplateObjects.Select(x => new ListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();
            }
        }

        private List<ListItem> _types;

        public List<ListItem> Types => _types ?? (_types = _service.GetTypes().ToList());

        internal void DoCustomBinding()
        {
            if (Data.IsSiteDotNet && string.IsNullOrWhiteSpace(Data.NetName))
            {
                Data.GenerateNetName();
            }

            if (Data.OverrideTemplateObject)
            {
                Data.ObjectInheritedFrom = _service.ReadObjectProperties(Data.ParentObjectId.Value, false);
                Data.Name = Data.ObjectInheritedFrom.Name;
                Data.NetName = Data.ObjectInheritedFrom.NetName;
            }

            if (Data.IsObjectContainerType)
            {
                Data.Container.DoCustomBinding(SelectionIsStarting, SelectionIncludes);
                SortingItems = JsonConvert.DeserializeObject<List<SortingItem>>(AggregationListItemsSortingItems);
                Data.Container.OrderStatic = SortingItemHelper.Serialize(SortingItems);
            }

            if (Data.UseDefaultValues)
            {
                Data.DefaultValues = JsonConvert.DeserializeObject<List<DefaultValue>>(AggregationListItemsDataDefaultValues);
            }
        }

        public List<ListItem> DefaultFormats
        {
            get
            {
                return Data.ChildObjectFormats.Select(x => new ListItem { Text = x.Name, Value = x.Id.ToString(), Selected = x.Id == Data.DefaultFormatId }).ToList();
            }
        }

        public SelectOptions SelectDefaultFormatOptions
        {
            get
            {
                var options = new SelectOptions
                {
                    EntityDataListArgs = new EntityDataListArgs
                    {
                        EntityTypeCode = Data.PageOrTemplate ? Constants.EntityTypeCode.PageObjectFormat : Constants.EntityTypeCode.TemplateObjectFormat,
                        ParentEntityId = Data.Id,
                        EntityId = Data.DefaultFormatId,
                        AddNewActionCode = Data.PageOrTemplate ? Constants.ActionCode.AddNewPageObjectFormat : Constants.ActionCode.AddNewTemplateObjectFormat,
                        ReadActionCode = Data.PageOrTemplate ? Constants.ActionCode.PageObjectFormatProperties : Constants.ActionCode.TemplateObjectFormatProperties
                    }
                };

                return options;
            }
        }

        public QPSelectListItem ContainerContentListItem
        {
            get
            {
                if (Data.Container.ContentId == null)
                {
                    return null;
                }

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
                {
                    return null;
                }

                var content = _service.GetContentById(Data.ContentForm.ContentId.Value);
                return
                    new QPSelectListItem { Value = content.Id.ToString(), Text = content.Name, Selected = true };
            }
        }

        public QPSelectListItem SitePageListItem
        {
            get
            {
                if (Data.ContentForm?.ThankYouPageId == null)
                {
                    return null;
                }

                var page = _service.ReadPageProperties(Data.ContentForm.ThankYouPageId.Value);
                return new QPSelectListItem { Value = page.Id.ToString(), Text = page.Name, Selected = true };
            }
        }

        public List<ListItem> FiltrationLevels => new List<ListItem>
        {
            new ListItem(ObjectSecurityMode.OnlyMatching.ToString(), TemplateStrings.ReturnOnlyMatchingArticles),
            new ListItem(ObjectSecurityMode.All.ToString(), TemplateStrings.ReturnAllArticles)
        };

        public List<ListItem> ReturnArticlesModes => new List<ListItem>
        {
            new ListItem(ArticleReturnMode.Sequentially.ToString(), TemplateStrings.Sequentially),
            new ListItem(ArticleReturnMode.Randomly.ToString(), TemplateStrings.Randomly)
        };

        private List<ListItem> _permissionLevels;

        public List<ListItem> PermissionLevels => _permissionLevels ?? (_permissionLevels = _service.GetPermissionLevels().ToList());

        private InitPropertyValue<IEnumerable<SortingItem>> _sortingItems;

        [LocalizedDisplayName("DefaultSorting", NameResourceType = typeof(TemplateStrings))]
        public IEnumerable<SortingItem> SortingItems
        {
            get { return _sortingItems.Value; }
            set { _sortingItems.Value = value; }
        }

        public IEnumerable<ListItem> SelectionStartingModes => new List<ListItem>
        {
            new ListItem(ArticleSelectionMode.FromTheFirstArticle.ToString(), TemplateStrings.FromTheFirstArticle){HasDependentItems = true, DependentItemIDs = new[]{"FromTheFirstPanel"}},
            new ListItem(ArticleSelectionMode.FromTheSpecifiedArticle.ToString(), TemplateStrings.FromTheSpecifiedArticle){HasDependentItems = true, DependentItemIDs = new[]{"FromTheSpecialPanel"}}
        };

        public IEnumerable<ListItem> SelectionIncludeModes => new List<ListItem>
        {
            new ListItem(ArticleSelectionIncludeMode.AllArticles.ToString(), TemplateStrings.AllArticles){HasDependentItems = true, DependentItemIDs = new[]{"AllArticlesPanel"}},
            new ListItem(ArticleSelectionIncludeMode.SpecifiedNumberOfArticles.ToString(), TemplateStrings.SpecifiedNumberOfArticles){HasDependentItems = true, DependentItemIDs = new[]{"SpecialArticlesPanel"}}
        };

        public override void Validate(ModelStateDictionary modelState)
        {
            base.Validate(modelState);
            var duplicateNames = SortingItems.GroupBy(c => c.Field).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
            var sortingArray = SortingItems.ToArray();
            for (var i = 0; i < sortingArray.Length; i++)
            {
                ValidateSortingItem(sortingArray[i], i + 1, duplicateNames, modelState);
            }
        }

        private static void ValidateSortingItem(SortingItem item, int index, string[] dupNames, ModelStateDictionary modelState)
        {
            if (dupNames.Contains(item.Field))
            {
                modelState.AddModelError(string.Empty, string.Format(TemplateStrings.SortExperessionFieldNotUnique, index));
                item.Invalid = true;
            }
        }

        public string ParentTemplateObjectsData
        {
            get
            {
                return JsonConvert.SerializeObject(ParentTemplateObjects.Select(x => new { x.Name, x.NetName, x.Id }));
            }
        }

        public string ShowGlobalTypeIds => ObjectType.GetCss().Id + "," + ObjectType.GetJavaScript().Id;

        [LocalizedDisplayName("Statuses", NameResourceType = typeof(WorkflowStrings))]
        public IEnumerable<int> ActiveStatusTypeIds { get; set; }

        [LocalizedDisplayName("Statuses", NameResourceType = typeof(WorkflowStrings))]
        public IEnumerable<ListItem> ActiveStatusTypeListItems { get; set; }

        public EntityDataListArgs EntityDataListArgs { get; set; }

        public int PublishedId { get; private set; }

        public bool HasWorkflow => Data.Container?.Content?.WorkflowBinding != null && Data.Container.Content.WorkflowBinding.WorkflowId != 0;

        [LocalizedDisplayName("SelectionIsStarting", NameResourceType = typeof(TemplateStrings))]
        public int SelectionIsStarting { get; set; }

        [LocalizedDisplayName("SelectionIncludes", NameResourceType = typeof(TemplateStrings))]
        public int SelectionIncludes { get; set; }

        public override string CaptureLockActionCode => Data.PageOrTemplate ? Constants.ActionCode.CaptureLockPageObject : Constants.ActionCode.CaptureLockTemplateObject;

        public string FilterHtaClass
        {
            get
            {
                if (!Data.IsSiteDotNet)
                {
                    return "hta-VBSTextArea";
                }

                if (Data.DefaultFormat != null)
                {
                    return HighlightModeSelectHelper.SelectMode(Data.DefaultFormat.NetLanguageId);
                }

                return "hta-cSharpTextArea";
            }
        }
    }
}
