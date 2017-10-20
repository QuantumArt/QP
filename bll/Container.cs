using System.Collections.Generic;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

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

		[LocalizedDisplayName("UseDynamicSorting", NameResourceType = typeof(TemplateStrings))]
		public bool AllowOrderDynamic { get; set; }

		[LocalizedDisplayName("Content", NameResourceType = typeof(TemplateStrings))]
		public int? ContentId { get; set; }

		public string CursorLocation { get; set; }

		public string CursorType { get; set; }

		[LocalizedDisplayName("Duration", NameResourceType = typeof(TemplateStrings))]
		public int? Duration { get; set; }

		[LocalizedDisplayName("DynamicContentVariable", NameResourceType = typeof(TemplateStrings))]
		[Example("news_content_name")]
		public string DynamicContentVariable { get; set; }

		[LocalizedDisplayName("AllowDynamicContentChanging", NameResourceType = typeof(TemplateStrings))]
		public bool AllowDynamicContentChanging { get; set; }

		public bool EnableCacheInvalidation { get; set; }

		[LocalizedDisplayName("EndLevel", NameResourceType = typeof(TemplateStrings))]
		public int EndLevel { get; set; }

		[LocalizedDisplayName("FilterValue", NameResourceType = typeof(TemplateStrings))]
		[Example("\"[content_item_id] = \" + NumValue(\"id\") \"[Name] = \'\" + StrValue(\"name\") + \"\'\".")]		
		public string FilterValue { get; set; }

		[LocalizedDisplayName("DynamicSortingExpression", NameResourceType = typeof(TemplateStrings))]
		public string OrderDynamic { get; set; }

		public string OrderStatic { get; set; }

		[LocalizedDisplayName("ReturnLastModifiedDate", NameResourceType = typeof(TemplateStrings))]
		public bool ReturnLastModified { get; set; }

		[LocalizedDisplayName("ReturnArticles", NameResourceType = typeof(TemplateStrings))]
		public int RotateContent { get; set; }

		[LocalizedDisplayName("UseSchedule", NameResourceType = typeof(TemplateStrings))]
		public bool ScheduleDependence { get; set; }

		[LocalizedDisplayName("FirstArticleNumberInTheSelection", NameResourceType = typeof(TemplateStrings))]
		public string SelectStart { get; set; }

		[LocalizedDisplayName("NumberOfArticlesInTheSelection", NameResourceType = typeof(TemplateStrings))]
		public string SelectTotal { get; set; }

		[LocalizedDisplayName("ShowArchived", NameResourceType = typeof(TemplateStrings))]
		public bool ShowArchived { get; set; }

		[LocalizedDisplayName("StartLevel", NameResourceType = typeof(TemplateStrings))]
		public int StartLevel { get; set; }

		[LocalizedDisplayName("SecurityMode", NameResourceType = typeof(TemplateStrings))]
		public bool? UseLevelFiltration { get; set; }

		[LocalizedDisplayName("UseSecurity", NameResourceType = typeof(TemplateStrings))]
		public bool ApplySecurity { get; set; }

		public override string LockedByAnyoneElseMessage => "Container is locked by user{0}";

	    public string LockType { get; set; }

		public BllObject Object { get; set; }		

		[LocalizedDisplayName("EnableDataCaching", NameResourceType = typeof(TemplateStrings))]
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

		public Dictionary<string, string> AdditionalDataForAggregationList
		{
			get;
			set;
		}
	}
}
