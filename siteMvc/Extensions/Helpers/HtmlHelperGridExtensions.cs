using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Article;
using Telerik.Web.Mvc.UI;
using Telerik.Web.Mvc.UI.Fluent;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    

    public static class HtmlHelperGridExtensions
    {
        public static List<GridRow> GetDynamicColumns(this HtmlHelper html, ArticleListViewModel model)
        {
            var relationCounters = new Dictionary<int, int>();
            var rowList = new List<GridRow>();
            foreach (var field in model.DisplayFields)
            {
                var row = new GridRow
                {
                    Field = Article.GetDynamicColumnName(field, relationCounters, true),
                    Title = field.DisplayName,
                    Sortable = field.ExactType != FieldExactTypes.M2MRelation && field.ExactType != FieldExactTypes.M2ORelation
                };
                rowList.Add(row);
            }
            return rowList;
        }

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
            var currentTheme = HttpContext.Current.Session[HttpContextSession.CurrentCssTheme].ToString();
            if (allowMultipleEntitySelection)
            {
                columnFactory
                    .Bound(string.Empty)
                    .Title(@"<input type=""checkbox"" name=""SelectHeader"" value="""" />")
                    .Width(30)
                    .HeaderHtmlAttributes(new { @class = "t-select-header" })
                    .ClientTemplate($@"<input type=""checkbox"" name=""SelectedArticlesIDs"" value=""<#= {FieldName.ContentItemId} #>"" />")
                    .HtmlAttributes(new { @class = "t-select-cell" })
                    .Sortable(false);
            }

            columnFactory.Bound(typeof(decimal), FieldName.ContentItemId)
                .Title(FieldName.Id)
                .HtmlAttributes(new { @class = "id" })
                .Width(30);

            columnFactory.Bound(typeof(decimal), FieldName.LockedBy)
                .Title(string.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
                    Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/lock.gif")),
                    ArticleStrings.IsLockedHeaderTooltip
                )).Width(18)
                .HeaderHtmlAttributes(new { @class = "t-image-header" })
                .ClientTemplate(string.Format(
                    @"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
                    "<#= " + FieldName.LockedByIcon + " #>",
                    "<#= " + FieldName.LockedByTooltip + " #>"
                )).HtmlAttributes(new { style = "text-align: center;" });

            columnFactory.Bound(typeof(bool), FieldName.Scheduled)
                .Title(string.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
                    Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/scheduled.gif")),
                    ArticleStrings.IsScheduledTooltip
                )).Width(18)
                .HeaderHtmlAttributes(new { @class = "t-image-header" })
                .ClientTemplate(string.Format(
                    @"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
                    "<#= " + FieldName.ScheduledIcon + " #>",
                    "<#= " + FieldName.ScheduledTooltip + " #>"
                )).HtmlAttributes(new { style = "text-align: center;" });

            columnFactory.Bound(typeof(bool), FieldName.Splitted)
                .Title(string.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
                    Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/splited.gif")),
                    ArticleStrings.IsSplitedTooltip
                )).Width(18)
                .HeaderHtmlAttributes(new { @class = "t-image-header" })
                .ClientTemplate(string.Format(
                    @"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
                    "<#= " + FieldName.SplittedIcon + " #>",
                    "<#= " + FieldName.SplittedTooltip + " #>"
                )).HtmlAttributes(new { style = "text-align: center;" });

            columnFactory.Bound(typeof(bool), FieldName.Visible)
                .Title(string.Format("<img src=\"{0}\" style=\"width: 16px; height: 16px;\" class=\"t-image\" title=\"{1}\" alt=\"{1}\" />",
                    Url.ToAbsolute(PathUtility.Combine(SitePathHelper.GetThemeRootImageFolderUrl(currentTheme), "/grid/header_icons/invisible.gif")),
                    ArticleStrings.IsInvisibleTooltip
                )).Width(18)
                .HeaderHtmlAttributes(new { @class = "t-image-header" })
                .ClientTemplate(string.Format(
                    @"<img src=""{0}/{1}"" title=""{2}"" alt=""{2}"" class=""smallIcon"" />", SitePathHelper.GetThemeSmallIconsImageFolderUrl(currentTheme),
                    "<#= " + FieldName.VisibleIcon + " #>",
                    "<#= " + FieldName.VisibleTooltip + " #>"
                )).HtmlAttributes(new { style = "text-align: center;" });

            var i = 0;
            var relationCounters = new Dictionary<int, int>();
            foreach (var displayField in displayFields)
            {
                ConfigureDynamicColumn(columnFactory, displayField, i == 0, hasTitleLink, relationCounters);
                i++;
            }

            columnFactory.Bound(typeof(string), FieldName.StatusTypeName).Title(ArticleStrings.Status).ClientTemplate("<#= " + FieldName.StatusTypeName + " #>");
            columnFactory.Bound(typeof(DateTime), FieldName.Created).Title(ArticleStrings.Created).ClientTemplate("<#= " + FieldName.Created + " #>");
            columnFactory.Bound(typeof(DateTime), FieldName.Modified).Title(ArticleStrings.Modified).ClientTemplate("<#= " + FieldName.Modified + " #>");
            columnFactory.Bound(typeof(int), FieldName.ModifierLogin).Title(ArticleStrings.LastModifiedBy).ClientTemplate("<#= " + FieldName.ModifierLogin + " #>");
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
