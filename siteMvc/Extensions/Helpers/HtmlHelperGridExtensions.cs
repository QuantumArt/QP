using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Article;
using Telerik.Web.Mvc.UI;
using Telerik.Web.Mvc.UI.Fluent;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperGridExtensions
    {
        public static MvcHtmlString ArticleGrid(this HtmlHelper html, ArticleListViewModel model)
        {
            var result = html.Telerik().Grid<DataRow>()
                .Name(model.MainComponentId)
                .Columns(c => ConfigureColumns(c, model.DisplayFields, model.AllowMultipleEntitySelection, !model.IsWindow))
                .DataBinding(db => db.Ajax().Select(model.GetDataActionName, model.ControllerName))
                .EnableCustomBinding(true)
                .Sortable()
                .Pageable(p => p.Style(GridPagerStyles.NextPreviousAndNumeric).PageSize(model.PageSize))
                .Selectable(s => s.Enabled(true))
                .ClientEvents(e => e.OnDataBinding("$q.preventDefaultFunction").OnRowDataBound("$q.preventDefaultFunction"))
                .ToHtmlString();

            return MvcHtmlString.Create(result);
        }

        private static void ConfigureColumns(GridColumnFactory<DataRow> columnFactory, IEnumerable<Field> displayFields, bool allowMultipleEntitySelection, bool hasTitleLink)
        {
            var currentTheme = HttpContext.Current.Session["theme"].ToString();
            if (allowMultipleEntitySelection)
            {
                columnFactory
                    .Bound(string.Empty)
                    .Title(@"<input type=""checkbox"" name=""SelectHeader"" value="""" />")
                    .Width(30)
                    .HeaderHtmlAttributes(new { @class = "t-select-header" })
                    .ClientTemplate($@"<input type=""checkbox"" name=""SelectedArticlesIDs"" value=""<#= {FieldName.CONTENT_ITEM_ID} #>"" />")
                    .HtmlAttributes(new { @class = "t-select-cell" })
                    .Sortable(false);
            }

            columnFactory.Bound(typeof(decimal), FieldName.CONTENT_ITEM_ID)
                .Title(ArticleStrings.ID)
                .HtmlAttributes(new { @class = "id" })
                .Width(30);

            columnFactory.Bound(typeof(decimal), FieldName.LOCKED_BY)
                .Title(string.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
                    Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/lock.gif")),
                    ArticleStrings.IsLockedHeaderTooltip
                )).Width(18)
                .HeaderHtmlAttributes(new { @class = "t-image-header" })
                .ClientTemplate(string.Format(
                    @"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
                    "<#= " + FieldName.LOCKED_BY_ICON + " #>",
                    "<#= " + FieldName.LOCKED_BY_TOOLTIP + " #>"
                )).HtmlAttributes(new { style = "text-align: center;" });

            columnFactory.Bound(typeof(bool), FieldName.SCHEDULED)
                .Title(string.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
                    Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/scheduled.gif")),
                    ArticleStrings.IsScheduledTooltip
                )).Width(18)
                .HeaderHtmlAttributes(new { @class = "t-image-header" })
                .ClientTemplate(string.Format(
                    @"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
                    "<#= " + FieldName.SCHEDULED_ICON + " #>",
                    "<#= " + FieldName.SCHEDULED_TOOLTIP + " #>"
                )).HtmlAttributes(new { style = "text-align: center;" });

            columnFactory.Bound(typeof(bool), FieldName.SPLITTED)
                .Title(string.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
                    Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/splited.gif")),
                    ArticleStrings.IsSplitedTooltip
                )).Width(18)
                .HeaderHtmlAttributes(new { @class = "t-image-header" })
                .ClientTemplate(string.Format(
                    @"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
                    "<#= " + FieldName.SPLITTED_ICON + " #>",
                    "<#= " + FieldName.SPLITTED_TOOLTIP + " #>"
                )).HtmlAttributes(new { style = "text-align: center;" });

            columnFactory.Bound(typeof(bool), FieldName.VISIBLE)
                .Title(string.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
                    Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/invisible.gif")),
                    ArticleStrings.IsInvisibleTooltip
                )).Width(18)
                .HeaderHtmlAttributes(new { @class = "t-image-header" })
                .ClientTemplate(string.Format(
                    @"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
                    "<#= " + FieldName.VISIBLE_ICON + " #>",
                    "<#= " + FieldName.VISIBLE_TOOLTIP + " #>"
                )).HtmlAttributes(new { style = "text-align: center;" });

            var i = 0;
            var relationCounters = new Dictionary<int, int>();
            foreach (var displayField in displayFields)
            {
                ConfigureDynamicColumn(columnFactory, displayField, i == 0, hasTitleLink, relationCounters);
                i++;
            }

            columnFactory.Bound(typeof(string), FieldName.STATUS_TYPE_NAME).Title(ArticleStrings.Status).ClientTemplate("<#= " + FieldName.STATUS_TYPE_NAME + " #>");
            columnFactory.Bound(typeof(DateTime), FieldName.CREATED).Title(ArticleStrings.Created).ClientTemplate("<#= " + FieldName.CREATED + " #>");
            columnFactory.Bound(typeof(DateTime), FieldName.MODIFIED).Title(ArticleStrings.Modified).ClientTemplate("<#= " + FieldName.MODIFIED + " #>");
            columnFactory.Bound(typeof(int), FieldName.MODIFIER_LOGIN).Title(ArticleStrings.LastModifiedBy).ClientTemplate("<#= " + FieldName.MODIFIER_LOGIN + " #>");
        }

        private static void ConfigureDynamicColumn(GridColumnFactory<DataRow> columnFactory, Field field, bool isFirstColumn, bool hasTitleLink, Dictionary<int, int> relationCounts)
        {
            columnFactory
                .Bound(field.ClrType, Article.GetDynamicColumnName(field, relationCounts, true))
                .Title(field.DisplayName)
                .ClientTemplate("<#= " + field.FormName + " #>")
                .Sortable(field.ExactType != FieldExactTypes.M2MRelation && field.ExactType != FieldExactTypes.M2ORelation)
                .HtmlAttributes(GetDynamicColumnHtmlAttributes(field, isFirstColumn, hasTitleLink));
        }

        private static Dictionary<string, object> GetDynamicColumnHtmlAttributes(Field field, bool isFirstColumn, bool hasTitleLink)
        {
            var htmlAttributes = new Dictionary<string, object>();
            if (isFirstColumn && hasTitleLink)
            {
                htmlAttributes.AddCssClass("title");
            }

            if (field.Type.Name == FieldTypeName.Boolean)
            {
                htmlAttributes.Add("align", "center");
            }
            else if (field.Type.Name == FieldTypeName.Numeric && !field.IsClassifier)
            {
                htmlAttributes.Add("align", "right");
            }

            return htmlAttributes;
        }
    }
}
