using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Text;
using Telerik.Web.Mvc;
using Telerik.Web.Mvc.UI;
using Telerik.Web.Mvc.UI.Fluent;
using Telerik.Web.Mvc.Extensions;
using Quantumart.QP8;
using Quantumart.QP8.Utils;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.Constants;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.WebMvc.ViewModels.Article;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
	public static class HtmlHelperGridExtensions
	{
		public static MvcHtmlString ArticleGrid(this HtmlHelper html, ArticleListViewModel model)
		{
			string result = html.Telerik().Grid<DataRow>()
				.Name(model.MainComponentId)
				.Columns(c => ConfigureColumns(c, model.DisplayFields, model.ParentEntityId, model.AllowMultipleEntitySelection, !model.IsWindow))
				.DataBinding(db => db.Ajax()
					.Select(model.GetDataActionName, model.ControllerName)
				)
				.EnableCustomBinding(true)
				.Sortable()
				.Pageable(p => 
					p
						.Style(GridPagerStyles.NextPreviousAndNumeric)
						.PageSize(model.PageSize)
				)
				.Selectable(s => s.Enabled(true))
				.ClientEvents(e => 
					e
						.OnDataBinding("$q.preventDefaultFunction")
						.OnRowDataBound("$q.preventDefaultFunction")
				)
				.ToHtmlString()
				;

			return MvcHtmlString.Create(result);
		}
                    
		private static GridDataKeyFactory<DataRow> ConfigureKeys(GridDataKeyFactory<DataRow> keyFactory, IEnumerable<DataRow> articleList)
		{
			keyFactory.Add<decimal>(r => r.Field<decimal>(Constants.FieldName.CONTENT_ITEM_ID));

			return keyFactory;
		}

		private static GridColumnFactory<DataRow> ConfigureColumns(GridColumnFactory<DataRow> columnFactory, IEnumerable<Field> displayFields,
			int contentId, bool allowMultipleEntitySelection, bool hasTitleLink)
		{
			string currentTheme = HttpContext.Current.Session["theme"].ToString();

			if (allowMultipleEntitySelection) {
				columnFactory.Bound("")
					.Title(@"<input type=""checkbox"" name=""SelectHeader"" value="""" />")
					.Width(30)
					.HeaderHtmlAttributes(new { @class = "t-select-header" })
					.ClientTemplate(
						String.Format(
							@"<input type=""checkbox"" name=""SelectedArticlesIDs"" value=""<#= {0} #>"" />", 
							Constants.FieldName.CONTENT_ITEM_ID
						)
					)
					.HtmlAttributes(new { @class = "t-select-cell" })
					.Sortable(false)
					;
			}

			columnFactory.Bound(typeof(decimal), Constants.FieldName.CONTENT_ITEM_ID)
				.Title(ArticleStrings.ID)
				.HtmlAttributes(new { @class = "id" })
				.Width(30)
				;

			columnFactory.Bound(typeof(decimal), Constants.FieldName.LOCKED_BY)
				.Title(String.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
					Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/lock.gif")),
					ArticleStrings.IsLockedHeaderTooltip
				))
				.Width(18)
				.HeaderHtmlAttributes(new { @class = "t-image-header" })
				.ClientTemplate(
					String.Format(
						@"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme), 
						"<#= " + Constants.FieldName.LOCKED_BY_ICON + " #>", 
						"<#= " + Constants.FieldName.LOCKED_BY_TOOLTIP + " #>"
					)
				)
				.HtmlAttributes(new { style = "text-align: center;" });

			columnFactory.Bound(typeof(bool), Constants.FieldName.SCHEDULED)
				.Title(String.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
					Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/scheduled.gif")),
					ArticleStrings.IsScheduledTooltip
				))
				.Width(18)
				.HeaderHtmlAttributes(new { @class = "t-image-header" })
				.ClientTemplate(
					String.Format(
						@"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
						"<#= " + Constants.FieldName.SCHEDULED_ICON + " #>",
						"<#= " + Constants.FieldName.SCHEDULED_TOOLTIP + " #>"
					)
				)
				.HtmlAttributes(new { style = "text-align: center;" });

			columnFactory.Bound(typeof(bool), Constants.FieldName.SPLITTED)
				.Title(String.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
					Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/splited.gif")),
					ArticleStrings.IsSplitedTooltip
				))
				.Width(18)
				.HeaderHtmlAttributes(new { @class = "t-image-header" })
				.ClientTemplate(
					String.Format(
						@"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
						"<#= " + Constants.FieldName.SPLITTED_ICON + " #>",
						"<#= " + Constants.FieldName.SPLITTED_TOOLTIP + " #>"
					)
				)
				.HtmlAttributes(new { style = "text-align: center;" });

			columnFactory.Bound(typeof(bool), Constants.FieldName.VISIBLE)
				.Title(String.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
					Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/invisible.gif")),
					ArticleStrings.IsInvisibleTooltip
				))
				.Width(18)
				.HeaderHtmlAttributes(new { @class = "t-image-header" })
				.ClientTemplate(
					String.Format(
						@"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
						"<#= " + Constants.FieldName.VISIBLE_ICON + " #>",
						"<#= " + Constants.FieldName.VISIBLE_TOOLTIP + " #>"
					)
				)
				.HtmlAttributes(new { style = "text-align: center;" });

			int i = 0;
			Dictionary<int, int> relationCounters = new Dictionary<int, int>();
			foreach (Field displayField in displayFields)
			{
				ConfigureDynamicColumn(columnFactory, displayField, (i == 0), hasTitleLink, relationCounters);
				i++;
			}

			columnFactory.Bound(typeof(string), Constants.FieldName.STATUS_TYPE_NAME)
				.Title(ArticleStrings.Status)
				.ClientTemplate("<#= " + Constants.FieldName.STATUS_TYPE_NAME + " #>");
				;

			columnFactory.Bound(typeof(DateTime), Constants.FieldName.CREATED)
				.Title(ArticleStrings.Created)
				.ClientTemplate("<#= " + Constants.FieldName.CREATED + " #>")
				;

			columnFactory.Bound(typeof(DateTime), Constants.FieldName.MODIFIED)
				.Title(ArticleStrings.Modified)
				.ClientTemplate("<#= " + Constants.FieldName.MODIFIED + " #>")
				;

			columnFactory.Bound(typeof(int), Constants.FieldName.MODIFIER_LOGIN)
				.Title(ArticleStrings.LastModifiedBy)
				.ClientTemplate("<#= " + Constants.FieldName.MODIFIER_LOGIN + " #>")
				;

			return columnFactory;
		}

		private static GridColumnFactory<DataRow> ConfigureDynamicColumn(GridColumnFactory<DataRow> columnFactory, Field field, bool isFirstColumn, bool hasTitleLink, Dictionary<int, int> relationCounts)
		{
			columnFactory
				.Bound(field.ClrType, Article.GetDynamicColumnName(field, relationCounts, true))
				.Title(field.DisplayName)
				.ClientTemplate("<#= " + field.FormName + " #>")
				.Sortable(field.ExactType != FieldExactTypes.M2MRelation && field.ExactType != FieldExactTypes.M2ORelation)
				.HtmlAttributes(GetDynamicColumnHtmlAttributes(field, isFirstColumn, hasTitleLink))
				;

			return columnFactory;
		}

		private static Dictionary<string, object> GetDynamicColumnHtmlAttributes(Field field, bool isFirstColumn, bool hasTitleLink)
		{
			Dictionary<string, object> htmlAttributes = new Dictionary<string, object>();

			if (isFirstColumn && hasTitleLink)
			{
				htmlAttributes.AddCssClass("title");
			}

			if (field.Type.Name == Constants.FieldTypeName.Boolean)
			{
				htmlAttributes.Add("align", "center");
			}
			else if (field.Type.Name == Constants.FieldTypeName.Numeric && !field.IsClassifier)
			{
				htmlAttributes.Add("align", "right");
			}
			return htmlAttributes;
		}
	}
}