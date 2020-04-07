using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class Container : LockableEntityObject
    {
        public int ObjectId { get; set; }

        public Content Content { get; set; }

        public Container()
        {
            CursorType = "adOpenForwardOnly";
            CursorLocation = "adUseClient";
            LockType = "adLockReadOnly";
            StartLevel = EntityPermissionLevel.GetList().Id;
            EndLevel = EntityPermissionLevel.GetFullAccess().Id;
        }

        [Display(Name = "UseDynamicSorting", ResourceType = typeof(TemplateStrings))]
        public bool AllowOrderDynamic { get; set; }

        [Display(Name = "Content", ResourceType = typeof(TemplateStrings))]
        public int? ContentId { get; set; }

        public string CursorLocation { get; set; }

        public string CursorType { get; set; }

        [Display(Name = "Duration", ResourceType = typeof(TemplateStrings))]
        public int? Duration { get; set; }

        [Display(Name = "DynamicContentVariable", ResourceType = typeof(TemplateStrings))]
        [Example("news_content_name")]
        public string DynamicContentVariable { get; set; }

        [Display(Name = "AllowDynamicContentChanging", ResourceType = typeof(TemplateStrings))]
        public bool AllowDynamicContentChanging { get; set; }

        public bool EnableCacheInvalidation { get; set; }

        [Display(Name = "EndLevel", ResourceType = typeof(TemplateStrings))]
        public int EndLevel { get; set; }

        [Display(Name = "Filter", ResourceType = typeof(TemplateStrings))]
        [Example("\"[content_item_id] = \" + NumValue(\"id\") \"[Name] = \'\" + StrValue(\"name\") + \"\'\".")]
        public string FilterValue { get; set; }

        [Display(Name = "DynamicSortingExpression", ResourceType = typeof(TemplateStrings))]
        public string OrderDynamic { get; set; }

        public string OrderStatic { get; set; }

        [Display(Name = "ReturnLastModifiedDate", ResourceType = typeof(TemplateStrings))]
        public bool ReturnLastModified { get; set; }

        [Display(Name = "ReturnArticles", ResourceType = typeof(TemplateStrings))]
        public int RotateContent { get; set; }

        [Display(Name = "UseSchedule", ResourceType = typeof(TemplateStrings))]
        public bool ScheduleDependence { get; set; }

        [Display(Name = "FirstArticleNumberInTheSelection", ResourceType = typeof(TemplateStrings))]
        public string SelectStart { get; set; }

        [Display(Name = "NumberOfArticlesInTheSelection", ResourceType = typeof(TemplateStrings))]
        public string SelectTotal { get; set; }

        [Display(Name = "ShowArchived", ResourceType = typeof(TemplateStrings))]
        public bool ShowArchived { get; set; }

        [Display(Name = "StartLevel", ResourceType = typeof(TemplateStrings))]
        public int StartLevel { get; set; }

        [Display(Name = "SecurityMode", ResourceType = typeof(TemplateStrings))]
        public bool? UseLevelFiltration { get; set; }

        [Display(Name = "UseSecurity", ResourceType = typeof(TemplateStrings))]
        public bool ApplySecurity { get; set; }

        public override string LockedByAnyoneElseMessage => "Container is locked by user{0}";

        public string LockType { get; set; }

        public BllObject Object { get; set; }

        [Display(Name = "EnableDataCaching", ResourceType = typeof(TemplateStrings))]
        public bool EnableDataCaching { get; set; }

        public void DoCustomBinding(int selectionIsStarting, int selectionIncludes)
        {
            if (selectionIsStarting == ArticleSelectionMode.FromTheFirstArticle)
            {
                SelectStart = null;
                StartsFromFirstArticle = true;
            }
            else
            {
                StartsFromFirstArticle = false;
            }

            if (selectionIncludes == ArticleSelectionIncludeMode.AllArticles)
            {
                SelectTotal = null;
                IncludesAllArticles = true;
            }
            else
            {
                IncludesAllArticles = false;
            }
        }

        public bool StartsFromFirstArticle { get; private set; }
        public bool IncludesAllArticles { get; private set; }

        public Dictionary<string, string> AdditionalDataForAggregationList { get; set; }
    }
}
