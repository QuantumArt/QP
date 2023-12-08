using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Article;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using System.Net;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using QP8.Plugins.Contract;
using Newtonsoft.Json;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperFieldExtensions
    {
        private const string LabelClassName = "label";
        private const string FieldClassName = "field";
        private const string ValidatorsClassName = "validators";
        private const string RowClassName = "row";
        private const string DescriptionClassName = "description";
        private static readonly Regex WindowIdRegExp = new Regex(@"^win[0-9]+$");

        private static IHtmlContent FieldTemplate(
            this IHtmlHelper html,
            IHtmlContent content,
            string id,
            string title,
            bool forCheckbox = false,
            string example = null,
            bool required = false,
            string description = null,
            string fieldName = null)
        {
            var label = new HtmlContentBuilder();

            if (required && !forCheckbox)
            {
                var star = new TagBuilder("span");
                star.MergeAttribute("class", "star");
                star.InnerHtml.AppendHtml("*");
                label.AppendHtml(star);
                label.AppendHtml(" ");
            }

            label.AppendHtml(html.QpLabel(html.UniqueId(id), title, !forCheckbox));

            if (!string.IsNullOrWhiteSpace(description))
            {
                label.AppendHtml(
                $" <span class='linkButton fieldDescription' data-field_description_text='{description}'>" +
                    "<a href='javascript:void(0);'>" +
                        "<span class='text'>(?)</span>" +
                    "</a>" +
                "</span>");
            }

            var dt = new TagBuilder("dt");
            dt.AddCssClass(LabelClassName);
            if (!forCheckbox)
            {
                dt.InnerHtml.AppendHtml(label);
            }

            var em = new TagBuilder("em");
            em.AddCssClass(ValidatorsClassName);

            var validator = html.ValidationMessage(id, new { id = html.UniqueId(id) + "_validator" });
            if (validator != null)
            {
                em.InnerHtml.AppendHtml(validator.ToHtmlEncodedString().ProtectCurlyBrackets());
            }

            string exampleCode = string.IsNullOrEmpty(example)
                ? string.Empty
                : $"<em class=\"{DescriptionClassName}\">{example}</em>";

            var dd = new TagBuilder("dd");
            dd.AddCssClass(FieldClassName);
            dd.InnerHtml.AppendHtml(content);
            if (forCheckbox)
            {
                dd.InnerHtml.AppendHtml(" ");
                dd.InnerHtml.AppendHtml(label);
            }
            else
            {
                dd.InnerHtml.AppendHtml(exampleCode);
            }
            dd.InnerHtml.AppendHtml(em);

            var dl = new TagBuilder("dl");
            dl.AddCssClass(RowClassName);
            dl.MergeAttribute("data-field_form_name", id);

            fieldName = fieldName?.Replace("\"", "") ?? "";
            if (!string.IsNullOrEmpty(fieldName))
            {
                dl.MergeAttribute("data-field_name", fieldName);
            }

            dl.InnerHtml.AppendHtml(dt);
            dl.InnerHtml.AppendHtml(dd);

            return dl;
        }

        private static IHtmlContent Editor(this IHtmlHelper html, FieldValue pair, bool articleIsAgregated = false, bool forceReadonly = false)
        {
            var field = pair.Field;
            var id = pair.Field.FormName;
            var value = pair.Value;
            var isVersionView = pair.Article.ViewType == ArticleViewType.CompareVersions;
            if (isVersionView)
            {
                switch (field.Type.Name)
                {
                    case FieldTypeName.Textbox:
                    case FieldTypeName.VisualEdit:
                        return html.VersionArea(id, value);
                    case FieldTypeName.Relation:
                    case FieldTypeName.M2ORelation:
                        return html.VersionRelation(id, value, pair.ValueToMerge, field, pair.Article.Id, pair.Article.ViewType);
                    case FieldTypeName.File:
                    case FieldTypeName.Image:
                        return html.VersionFile(id, value, field, pair.Version, pair.Article.ViewType);
                    case FieldTypeName.Time:
                        return html.VersionTime(id, value, pair.ValueToMerge, pair.Article.ViewType);
                    case FieldTypeName.Date:
                        return html.VersionDate(id, value, pair.ValueToMerge, pair.Article.ViewType);
                    case FieldTypeName.DateTime:
                        return html.VersionDateTime(id, value, pair.ValueToMerge, pair.Article.ViewType);
                    case FieldTypeName.Numeric:
                        return field.IsClassifier
                            ? html.VersionClassifierField(id, value, field, pair.Article, pair.Version, true, pair.ValueToMerge)
                            : html.VersionText(id, value);
                    default:
                        return html.VersionText(id, value);
                }
            }

            var readOnly = forceReadonly || pair.Article.IsReadOnly || field.IsReadOnly || !articleIsAgregated && pair.Article.Content.HasAggregatedFields;
            var htmlAttributes = html.QpHtmlProperties(id, field, -1, readOnly, pair.Field.Name);
            switch (field.ExactType)
            {
                case FieldExactTypes.Textbox:
                    return html.QpTextArea(id, value, htmlAttributes);
                case FieldExactTypes.VisualEdit:
                    return html.VisualEditor(id, value, htmlAttributes, field, readOnly);
                case FieldExactTypes.File:
                case FieldExactTypes.Image:
                case FieldExactTypes.DynamicImage:
                    return html.File(id, value, htmlAttributes, field, pair.Version?.Id ?? pair.Article.Id, pair.Version);
                case FieldExactTypes.DateTime:
                    return html.DateTime(id, value, htmlAttributes, !field.Required, readOnly);
                case FieldExactTypes.Date:
                    return html.Date(id, value, htmlAttributes, !field.Required, readOnly);
                case FieldExactTypes.Time:
                    return html.Time(id, value, htmlAttributes, !field.Required, readOnly);
                case FieldExactTypes.Boolean:
                    return html.QpCheckBox(id, null, Converter.ToBoolean(value), htmlAttributes);
                case FieldExactTypes.Numeric:
                    return html.NumericTextBox(id, value, htmlAttributes, field.DecimalPlaces);
                case FieldExactTypes.Classifier:
                    if (pair.Version != null)
                    {
                        return html.VersionClassifierField(id, value, field, pair.Article, pair.Version, readOnly);
                    }

                    return html.ClassifierField(id, value, field, pair.Article, readOnly);
                case FieldExactTypes.O2MRelation:
                case FieldExactTypes.M2MRelation:
                case FieldExactTypes.M2ORelation:
                    var result = ArticleViewModel.GetListForRelation(field, value, pair.Article.Id);
                    return html.Relation(id, html.List(result.Items), new ControlOptions
                    {
                        Enabled = !readOnly,
                        HtmlAttributes = htmlAttributes
                    }, field, pair.Article.Id, result.IsListOverflow);
                case FieldExactTypes.StringEnum:
                    return html.StringEnumEditor(id, value, field, readOnly, pair.Article.IsNew);
                default:
                    return html.QpTextBox(id, value, htmlAttributes);
            }
        }

        private static IHtmlContent Editor(this IHtmlHelper html, QpPluginFieldValue pair, bool forceReadOnly)
        {
            var field = pair.Field;
            var id = pair.FormName;
            var value = pair.Value;
            Dictionary<string, object> htmlAttributes;
            switch (field.ValueType)
            {
                case QpPluginValueType.String:
                     htmlAttributes = html.QpHtmlProperties(id, 0, EditorType.Textbox);
                    return html.QpTextBox(id, value, htmlAttributes);
                case QpPluginValueType.DateTime:
                    htmlAttributes = html.QpHtmlProperties(id, 0, EditorType.Textbox);
                    return html.DateTime(id, value, htmlAttributes, true);
                case QpPluginValueType.Bool:
                    htmlAttributes = html.QpHtmlProperties(id, 0, EditorType.Checkbox);
                    return html.QpCheckBox(id, null, Converter.ToBoolean(value), htmlAttributes);
                case QpPluginValueType.Numeric:
                    htmlAttributes = html.QpHtmlProperties(id, 0, EditorType.Numeric);
                    return html.NumericTextBox(id, value, htmlAttributes);
                default:
                    throw new ArgumentException("Unsupported Field Type");
            }
        }

        internal static IHtmlContent VersionRelation(this IHtmlHelper source, string id, string value, string valueToMerge, Field field, int articleId, ArticleViewType viewType)
        {
            if (viewType == ArticleViewType.CompareVersions)
            {
                var titles1 = field.GetRelatedTitles(value);
                var titles2 = field.GetRelatedTitles(valueToMerge);
                return source.VersionText(id, ArticleVersion.MergeRelation(titles1, titles2));
            }

            var titles = string.Join("<br />", field.GetRelatedTitles(value).Select(i => $"(#{i.Value}) - {i.Text}"));
            return source.VersionText(id, titles);
        }

        internal static IHtmlContent Relation(this IHtmlHelper source, string id, IEnumerable<QPSelectListItem> list, ControlOptions options, Field field, int articleId, bool isListOverflow)
        {
            const string entityTypeCode = EntityTypeCode.Article;
            var baseField = field.GetBaseField(articleId);
            var contentId = baseField.RelateToContentId ?? 0;
            var fieldId = baseField.Id;
            var filter = baseField.GetExternalRelationFilters(articleId);
            var relatedToContent = baseField.RelatedToContent;
            var addNewActionCode = ActionCode.None;
            var readActionCode = ActionCode.None;

            if (relatedToContent != null)
            {
                if (!relatedToContent.IsVirtual)
                {
                    readActionCode = ActionCode.EditArticle;
                    if (relatedToContent.IsAccessible(ActionTypeCode.Update))
                    {
                        addNewActionCode = ActionCode.AddNewArticle;
                    }
                }
                else
                {
                    readActionCode = ActionCode.ViewVirtualArticle;
                }
            }

            var listArgs = new EntityDataListArgs
            {
                EntityTypeCode = entityTypeCode,
                ParentEntityId = contentId,
                EntityId = articleId,
                ListId = fieldId,
                AddNewActionCode = addNewActionCode,
                ReadActionCode = readActionCode,
                SelectActionCode = baseField.RelationType == RelationType.OneToMany ? ActionCode.SelectArticle : ActionCode.MultipleSelectArticle,
                EnableCopy = field.ExactType != FieldExactTypes.M2ORelation,
                ReadDataOnInsert = field.OrderByTitle || field.OrderFieldId.HasValue || field.FieldTitleCount > 1,
                MaxListHeight = 200,
                MaxListWidth = 350,
                ShowIds = true,
                Filter = filter == null ? null : JsonConvert.SerializeObject(filter)
            };

            return source.Relation(id, list, options, baseField.RelationType, isListOverflow, listArgs);
        }

        internal static IHtmlContent QpLabelFor<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string displayName = "", bool withColon = true, int index = -1)
        {
            var data = html.GetMetaData(expression);
            return html.QpLabel(html.UniqueId(
                html.ModelExpressionProvider().GetExpressionText(expression), index), string.IsNullOrEmpty(displayName) ? data.DisplayName : displayName, withColon);
        }

        internal static IHtmlContent VersionFile(this IHtmlHelper source, string id, string value, Field field, ArticleVersion version, ArticleViewType viewType)
        {
            if (viewType == ArticleViewType.CompareVersions)
            {
                return source.VersionText(id, value);
            }

            var tb = source.FileWrapper(id, null, field, version.Id, version, true, false);

            tb.InnerHtml.AppendHtml(source.VersionText(id, value));

            if (field.TypeId == FieldTypeCodes.Image)
            {
                tb.InnerHtml.AppendHtml(source.ImagePreview(id));
            }

            tb.InnerHtml.AppendHtml(source.FileDownload(id));

            return tb;
        }

        internal static IHtmlContent VersionDate(this IHtmlHelper source, string id, string value, string valueToMerge, ArticleViewType viewType)
        {
            var resultValue = viewType == ArticleViewType.PreviewVersion ? source.FormatAsDate(value) : ArticleVersion.Merge(source.FormatAsDate(value), source.FormatAsDate(valueToMerge));
            return source.VersionText(id, resultValue);
        }

        internal static IHtmlContent VersionTime(this IHtmlHelper source, string id, string value, string valueToMerge, ArticleViewType viewType)
        {
            var resultValue = viewType == ArticleViewType.PreviewVersion ? source.FormatAsTime(value) : ArticleVersion.Merge(source.FormatAsTime(value), source.FormatAsTime(valueToMerge));
            return source.VersionText(id, resultValue);
        }

        internal static IHtmlContent VersionDateTime(
            this IHtmlHelper source,
            string id,
            string value,
            string valueToMerge,
            ArticleViewType viewType
        ) => source.VersionText(
            id,
            viewType == ArticleViewType.PreviewVersion
                ? source.FormatAsDateTime(value)
                : ArticleVersion.Merge(source.FormatAsDateTime(value), source.FormatAsDateTime(valueToMerge))
        );

        public static string UniqueId(this IHtmlHelper html, string id, int index = -1) =>
            UniqueId(id, html.ViewContext.RouteData.Values[SpecialKeys.TabId], index);

        public static string UniqueId(string id, object tabId, int index = -1)
        {
            var resultId = id.Replace(".", "_");
            if (index == -1)
            {
                return tabId == null ? resultId : $"{tabId}_{resultId}";
            }

            return tabId == null ? $"{resultId}_{index}" : $"{tabId}_{resultId}_{index}";
        }

        public static string TabId(this IHtmlHelper html) => html.ViewContext.RouteData.Values[SpecialKeys.TabId].ToString();

        public static bool IsReadOnly(this IHtmlHelper html)
        {
            var obj = html.ViewData[SpecialKeys.IsEntityReadOnly];
            return obj != null && (bool)obj;
        }

        public static bool IsWindow(string tabId)
        {
            var isWindow = false;
            if (!string.IsNullOrWhiteSpace(tabId))
            {
                isWindow = WindowIdRegExp.IsMatch(tabId);
            }

            return isWindow;
        }

        public static IHtmlContent Field(this IHtmlHelper html, FieldValue pair, bool articleIsAgregated = false, bool forceReadonly = false)
        {
            // Если статья уже отображается как агрегированная, то у нее не отображаются поля-классификаторы и агрегирующие поля
            if (articleIsAgregated && (pair.Field.IsClassifier || pair.Field.Aggregated))
            {
                return HtmlString.Empty;
            }

            // Если M2O поле обычного (не виртуального) контента имеет aggregated backfield - то такое поле не показывать
            if (pair.Field.ExactType == FieldExactTypes.M2ORelation && !pair.Field.Content.IsVirtual && pair.Field.BackRelation != null && pair.Field.BackRelation.Aggregated)
            {
                return HtmlString.Empty;
            }

            var required = pair.Field.Required && !pair.Article.IsReadOnly;
            var fieldDescription = string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(pair.Field.Description ?? string.Empty).Replace("\u00A0", string.Empty)) ? null : pair.Field.Description;
            if (!string.IsNullOrEmpty(fieldDescription))
            {
                fieldDescription = fieldDescription.Replace("{", "{{").Replace("}", "}}");
            }

            var htmlContent = html.Editor(pair, articleIsAgregated, forceReadonly);
            var fieldHtmlString = html.FieldTemplate(htmlContent, pair.Field.FormName, pair.Field.DisplayName, required: required, description: fieldDescription, fieldName: pair.Field.Name);

            if (pair.Field.IsClassifier && pair.Article.ViewType != ArticleViewType.Virtual)
            {
                var result = new HtmlContentBuilder();
                result.AppendHtml(fieldHtmlString);
                result.AppendHtml(GetExtensionArticleFields(html, pair));
                return result;
            }

            return fieldHtmlString;
        }

        public static IHtmlContent Field(this IHtmlHelper html, QpPluginFieldValue pair, bool forceReadonly = false)
        {
            var fieldDescription = string.IsNullOrWhiteSpace(WebUtility.HtmlDecode(pair.Field.Description ?? string.Empty).Replace("\u00A0", string.Empty)) ? null : pair.Field.Description;
            if (!string.IsNullOrEmpty(fieldDescription))
            {
                fieldDescription = fieldDescription.Replace("{", "{{").Replace("}", "}}");
            }

            var htmlContent = html.Editor(pair, forceReadonly);
            var fieldHtmlString = html.FieldTemplate(htmlContent, pair.FormName, pair.Field.Name, required: false, fieldName: pair.Field.Name);

            return fieldHtmlString;
        }

        private static IHtmlContent GetExtensionArticleFields(IHtmlHelper html, FieldValue pair)
        {
            var extensionContentId = Converter.ToInt32(pair.Value, 0);
            if (extensionContentId == 0)
            {
                return HtmlString.Empty;
            }

            Article aggregatedArticle = null;
            if (pair.Article.ViewType == ArticleViewType.CompareVersions || pair.Article.ViewType == ArticleViewType.PreviewVersion)
            {
                if (pair.Article.ViewType == ArticleViewType.CompareVersions)
                {
                    if (pair.Version == null || !StringComparer.InvariantCultureIgnoreCase.Equals(pair.Value, pair.ValueToMerge))
                    {
                        return HtmlString.Empty;
                    }

                    aggregatedArticle = pair.Version.AggregatedArticles.SingleOrDefault(n => n.ContentId == extensionContentId);
                }

                if (pair.Article.ViewType == ArticleViewType.PreviewVersion)
                {
                    if (pair.Version == null)
                    {
                        return HtmlString.Empty;
                    }

                    aggregatedArticle = pair.Version.AggregatedArticles.SingleOrDefault(n => n.ContentId == extensionContentId);
                }
            }
            else
            {
                aggregatedArticle = pair.Article.GetAggregatedArticleByClassifier(extensionContentId);
            }

            var div = new TagBuilder("div");
            div.AddCssClass($"articleWrapper_{extensionContentId}");

            div.InnerHtml.AppendHtml(
                html.AggregatedFieldValues(
                    aggregatedArticle?.FieldValues.Where(n => !n.Field.Aggregated)));

            return div;
        }

        public static IHtmlContent DisplayField(this IHtmlHelper html, string id, string title, string value)
        {
            return html.FieldTemplate(html.Span(html.UniqueId(id), value), id, title);
        }

        public static ModelExpression GetModelExpression<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return html.ModelExpressionProvider().CreateModelExpression(html.ViewData, expression);
        }

        public static ModelMetadata GetMetaData<TModel, TValue>(this IHtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return html.GetModelExpression(expression).Metadata;
        }

        public static IHtmlContent HtmlFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            Func<TModel, IHtmlContent> content)
        {
            return html.FieldTemplate(
                content(html.ViewData.Model),
                html.ModelExpressionProvider().GetExpressionText(expression), html.GetMetaData(expression).DisplayName);
        }

        public static IHtmlContent TextBoxField<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            string defaultValue,
            Dictionary<string, object> htmlAttributes = null)
        {
            var fieldName = html.ModelExpressionProvider().GetExpressionText(expression);
            var metaData = html.GetMetaData(expression);
            return html.FieldTemplate(
                html.QpTextBox(fieldName, defaultValue, htmlAttributes),
                fieldName,
                metaData.DisplayName,
                false,
                HtmlHelpersExtensions.GetExampleText(metaData.ContainerType, metaData.PropertyName));
        }

        public static IHtmlContent TextBoxField<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            string defaultValue,
            bool forceReadonly)
        {
            var htmlAttributes = forceReadonly ? new Dictionary<string, object> { { "readonly", "readonly" } } : null;
            return html.TextBoxField(expression, defaultValue, htmlAttributes);
        }

        public static IHtmlContent TextBoxFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            Dictionary<string, object> htmlAttributes = null)
        {
            var metaData = html.GetMetaData(expression);
            return html.FieldTemplate(
                html.QpTextBoxFor(expression, htmlAttributes),
                html.ModelExpressionProvider().GetExpressionText(expression),
                metaData.DisplayName,
                false,
                HtmlHelpersExtensions.GetExampleText(metaData.ContainerType, metaData.PropertyName));
        }

        public static IHtmlContent TextAreaFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            Dictionary<string, object> htmlAttributes = null)
        {
            return html.FieldTemplate(
                html.QpTextAreaFor(expression, htmlAttributes),
                html.ModelExpressionProvider().GetExpressionText(expression),
                html.GetMetaData(expression).DisplayName);
        }

        public static IHtmlContent VisualEditorFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            Field field)
        {
            return html.FieldTemplate(
                html.VisualEditorFor(expression, field),
                html.ModelExpressionProvider().GetExpressionText(expression),
                html.GetMetaData(expression).DisplayName);
        }

        public static IHtmlContent DateTimeFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            return html.FieldTemplate(
                html.DateTimeFor(expression),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent DateFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            return html.FieldTemplate(
                html.DateFor(expression),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent TimeFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            return html.FieldTemplate(
                html.TimeFor(expression),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent DisplayFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            return html.FieldTemplate(
                html.DisplayFor(expression),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent DisplayFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            string templateName)
        {
            return html.FieldTemplate(
                html.DisplayFor(expression, templateName),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent SelectFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            IEnumerable<QPSelectListItem> list,
            SelectOptions options)
        {
            return html.SelectFieldFor(expression, list, null, options);
        }

        public static IHtmlContent SelectFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            IEnumerable<QPSelectListItem> list,
            Dictionary<string, object> htmlAttributes = null,
            SelectOptions options = null,
            bool required = false)
        {
            return html.FieldTemplate(
                html.QpDropDownListFor(expression, list, htmlAttributes, options ?? new SelectOptions()),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName,
                required: required);
        }

        public static IHtmlContent CheckBoxFieldFor<TModel>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, bool>> expression,
            string toggleId = null,
            bool reverseToggle = false,
            Dictionary<string, object> htmlAttributes = null,
            bool forceReadonly = false)
        {
            return html.FieldTemplate(
                html.QpCheckBoxFor(expression, toggleId, reverseToggle, htmlAttributes, forceReadOnly: forceReadonly),
                html.ModelExpressionProvider().GetExpressionText(expression),
                html.GetMetaData(expression).DisplayName,
                true);
        }

        public static IHtmlContent RadioFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            IEnumerable<QPSelectListItem> list,
            RepeatDirection repeatDirection = RepeatDirection.Horizontal,
            EntityDataListArgs entityDataListArgs = null,
            ControlOptions options = null)
        {
            return html.FieldTemplate(
                 html.QpRadioButtonListFor(expression, list, repeatDirection, entityDataListArgs, options),
                 html.ModelExpressionProvider().GetExpressionText(expression),
                 GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent PasswordFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression)
        {
            return html.FieldTemplate(
                html.PasswordFor(expression, html.QpHtmlProperties(expression, EditorType.Password)),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent NumericFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            double? minValue = null,
            double? maxValue = null,
            int decimalDigits = 0,
            Dictionary<string, object> htmlAttributes = null)
        {
            return html.FieldTemplate(
                html.NumericFor(expression, decimalDigits, minValue, maxValue, htmlAttributes),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent FileForFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, TValue>> expression,
            Field field,
            Dictionary<string, object> htmlAttributes = null)
        {
            return html.FieldTemplate(
                html.FileFor(expression, field, htmlAttributes),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent SingleItemPickerFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> source,
            Expression<Func<TModel, TValue>> expression,
            QPSelectListItem selected,
            EntityDataListArgs entityDataListArgs,
            ControlOptions options)
        {
            return source.FieldTemplate(
                source.SingleItemPickerFor(expression, selected, entityDataListArgs, options),
                source.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(source, expression).DisplayName);
        }

        public static IHtmlContent AggregationListFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> source,
            Expression<Func<TModel, IEnumerable<TValue>>> expression,
            IEnumerable<TValue> list,
            string bindings,
            Dictionary<string, string> additionalData = null)
        {
            return source.FieldTemplate(
                source.AggregationListFor(source.ModelExpressionProvider().GetExpressionText(expression), list, bindings, additionalData),
                source.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(source, expression).DisplayName);
        }

        public static IHtmlContent VersionTextFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> source,
            Expression<Func<TModel, TValue>> expression,
            TValue text)
        {
            return source.FieldTemplate(
                source.VersionTextFor(source.ModelExpressionProvider().GetExpressionText(expression), text.ToString()),
                source.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(source, expression).DisplayName);
        }

        public static IHtmlContent VersionAreaFieldFor<TModel, TValue>(
            this IHtmlHelper<TModel> source,
            Expression<Func<TModel, TValue>> expression,
            TValue text)
        {
            return source.FieldTemplate(
                source.VersionAreaFor(source.ModelExpressionProvider().GetExpressionText(expression), text.ToString()),
                source.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(source, expression).DisplayName);
        }

        public static IHtmlContent WorkflowFor<TModel, TValue>(
            this IHtmlHelper<TModel> source,
            Expression<Func<TModel, IEnumerable<TValue>>> expression,
            IEnumerable<TValue> list)
        {
            return source.WorkflowFor(source.ModelExpressionProvider().GetExpressionText(expression), list);
        }

        public static IHtmlContent CheckboxListFieldFor<TModel>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, IList<QPCheckedItem>>> expression,
            IEnumerable<QPSelectListItem> list,
            EntityDataListArgs entityDataListArgs,
            Dictionary<string, object> htmlAttributes,
            RepeatDirection repeatDirection = RepeatDirection.Vertical)
        {
            return html.FieldTemplate(
                html.QpCheckBoxListFor(expression, list, entityDataListArgs, htmlAttributes, repeatDirection),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent CheckBoxTreeFieldFor<TModel>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression,
            string entityTypeCode,
            int? parentEntityId,
            string actionCode,
            bool allowGlobalSelection = false,
            Dictionary<string, object> htmlAttributes = null)
        {
            return html.FieldTemplate(
                html.CheckBoxTreeFor(
                    expression, entityTypeCode, parentEntityId,
                    actionCode, allowGlobalSelection, htmlAttributes),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent VirtualFieldTreeFieldFor<TModel>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression,
            int? parentEntityId,
            int virtualContentId,
            Dictionary<string, object> htmlAttributes = null)
        {
            return html.FieldTemplate(
                html.VirtualFieldTreeFor(expression, parentEntityId, virtualContentId, htmlAttributes),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent UnionContentsFieldFor<TModel>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, IEnumerable<int>>> expression,
            IEnumerable<ListItem> selectedItemList,
            int siteId,
            Dictionary<string, object> htmlAttributes = null)
        {
            return html.FieldTemplate(
                html.UnionContentsFor(expression, selectedItemList, siteId, htmlAttributes),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        public static IHtmlContent MultipleItemPickerFieldFor<TModel>(
            this IHtmlHelper<TModel> html,
            Expression<Func<TModel, IEnumerable<int>>> expression,
            IEnumerable<ListItem> selectedItemList,
            EntityDataListArgs entityDataListArgs,
            Dictionary<string, object> htmlAttributes = null)
        {
            return html.FieldTemplate(
                html.MultipleItemPickerFor(expression, selectedItemList, entityDataListArgs, htmlAttributes),
                html.ModelExpressionProvider().GetExpressionText(expression),
                GetMetaData(html, expression).DisplayName);
        }

        private static IHtmlContent ClassifierField(
            this IHtmlHelper source,
            string name,
            string value,
            Field field,
            Article article,
            bool forceReadOnly)
        {
            Article aggregatedArticle = null;
            var classifierValue = Converter.ToInt32(value, 0);
            if (article.ViewType != ArticleViewType.Virtual)
            {
                aggregatedArticle = article.GetAggregatedArticleByClassifier(classifierValue);
            }

            var classifierTag = source.ClassifierFieldComponent(name, value, field, article, aggregatedArticle);

            if (forceReadOnly)
            {
                var classifierContent = ArticleViewModel.GetContentById(Converter.ToNullableInt32(value));
                var classifierContentName = classifierContent?.Name;
                var htmlAttributes = new Dictionary<string, object>
                {
                    { "class", HtmlHelpersExtensions.ArticleTextboxClassName },
                    { "disabled", "disabled" },
                    { HtmlHelpersExtensions.DataContentFieldName, field.Name }
                };
                classifierTag.InnerHtml.AppendHtml(source.QpTextBox(name, classifierContentName, htmlAttributes));
            }
            else
            {
                var contentListHtmlAttrs = new Dictionary<string, object>
                {
                    { "class", "dropDownList classifierContentList" },
                    { HtmlHelpersExtensions.DataContentFieldName, field.Name }
                };
                var dropDownList = source.DropDownList(
                    name,
                    source.List(ArticleViewModel.GetAggregatableContentsForClassifier(field, value)),
                    FieldStrings.SelectContent,
                    contentListHtmlAttrs);
                classifierTag.InnerHtml.AppendHtml(dropDownList);
            }
            return classifierTag;
        }

        private static IHtmlContent VersionClassifierField(this IHtmlHelper source, string name, string value, Field field, Article article, ArticleVersion version = null, bool forceReadOnly = true, string valueToMerge = null)
        {
            var name1 = version?.GetAggregatedContent(value)?.Name;
            if (!StringComparer.InvariantCultureIgnoreCase.Equals(value, valueToMerge) && article.ViewType == ArticleViewType.CompareVersions)
            {
                var name2 = version?.VersionToMerge.GetAggregatedContent(valueToMerge)?.Name;
                var mergedValue = ArticleVersion.Merge(Formatter.ProtectHtml(name1), Formatter.ProtectHtml(name2));
                return source.VersionText(name, mergedValue);
            }

            Article aggregatedArticle = null;
            var classifierValue = Converter.ToInt32(value, 0);
            if (article.ViewType != ArticleViewType.Virtual)
            {
                aggregatedArticle = version?.AggregatedArticles.SingleOrDefault(n => n.ContentId == classifierValue);
            }

            var classifierTag = source.ClassifierFieldComponent(name, value, field, article, aggregatedArticle);

            if (forceReadOnly)
            {
                classifierTag.InnerHtml.AppendHtml(source.VersionText(name, name1));
            }
            else
            {
                var contentListHtmlAttrs = new Dictionary<string, object> { { "class", "dropDownList classifierContentList" } };
                var dropDownList = source.DropDownList(name, source.List(ArticleViewModel.GetAggregatableContentsForClassifier(field, value)), FieldStrings.SelectContent, contentListHtmlAttrs);
                classifierTag.InnerHtml.AppendHtml(dropDownList);
            }

            return classifierTag;
        }

        private static TagBuilder ClassifierFieldComponent(
            this IHtmlHelper html,
            string name,
            string value,
            Field field,
            Article article,
            EntityObject aggregatedArticle)
        {
            var componentElemId = html.UniqueId(name);
            var acticleHtmlElemId = componentElemId + "_articleHtml";
            var aggregatedArticleId = aggregatedArticle?.Id.ToString() ?? string.Empty;

            var div = new TagBuilder("div");
            div.MergeAttribute("id", componentElemId);
            div.MergeAttribute("data-host_id", html.TabId());
            div.MergeAttribute("data-field_name", name);
            div.MergeAttribute("data-aggregated_content_id", value);
            div.MergeAttribute("data-aggregated_article_id", aggregatedArticleId);
            div.MergeAttribute("data-root_content_id", article.ContentId.ToString());
            div.MergeAttribute("data-classifier_id", field.Id.ToString());
            div.MergeAttribute("data-root_article_id", article.Id.ToString());
            div.MergeAttribute("data-acticle_html_id", acticleHtmlElemId);
            div.MergeAttribute("data-is_not_changeable", $"{!article.IsNew && !field.Changeable}");
            div.AddCssClass("classifierComponent");
            return div;
        }

        private static string EndClassifierFieldComponent() => "</div>";

        public static IHtmlContent AggregatedFieldValues(this IHtmlHelper html, IEnumerable<FieldValue> values)
        {
            var content = new HtmlContentBuilder();
            foreach (FieldValue pair in values)
            {
                content.AppendHtml(html.Field(pair, true));
            }
            return content;
        }

        private static IHtmlContent StringEnumEditor(
            this IHtmlHelper html, string name, string value, Field field, bool forceReadOnly, bool isNew)
        {
            const string specClass = "qp-stringEnumEditor";

            var items = field.StringEnumItems
                .Select(i => new QPSelectListItem
                {
                    Text = i.Alias,
                    Value = i.Value,
                    Selected = i.Value.Equals(value, StringComparison.InvariantCulture)
                });

            if (field.ShowAsRadioButtons)
            {
                var htmlAttributes = new Dictionary<string, object>();
                htmlAttributes.AddCssClass(specClass);
                htmlAttributes.Add(HtmlHelpersExtensions.DataContentFieldName, field.Name);
                return html.QpRadioButtonList(
                    name, items, RepeatDirection.Horizontal,
                    new ControlOptions
                    {
                        HtmlAttributes = htmlAttributes,
                        Enabled = !forceReadOnly
                    });
            }

            return html.QpDropDownList(
                name, items, !field.Required || !isNew ? GlobalStrings.NotSelected : null,
                new ControlOptions
                {
                    Enabled = !forceReadOnly,
                    HtmlAttributes = new Dictionary<string, object>
                    {
                        { "class", specClass }
                    }
                });
        }
    }
}
