using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.ViewModels.Article;

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

        public static string FieldTemplate(this HtmlHelper html, string id, string title, bool forCheckbox = false, string example = null, bool required = false, string description = null, string fieldName = null)
        {
            var label = html.QpLabel(html.UniqueId(id), title, !forCheckbox).ToString();
            if (!string.IsNullOrWhiteSpace(description))
            {
                label = $"{label} <span class='linkButton fieldDescription' data-field_description_text='{description}'>" + "<a href='javascript:void(0);'>" + "<span class='text'>(?)</span>" + "</a>" + "</span>";
            }

            if (required && !forCheckbox)
            {
                var star = new TagBuilder("span");
                star.MergeAttribute("class", "star");
                star.InnerHtml = "*";
                label = $"{star} {label}";
            }

            var labelCell = new TagBuilder("dt");
            labelCell.AddCssClass(LabelClassName);
            labelCell.InnerHtml = forCheckbox ? string.Empty : label;

            var validatorWrapper = new TagBuilder("em");
            validatorWrapper.AddCssClass(ValidatorsClassName);

            var validator = html.ValidationMessage(id, new { id = html.UniqueId(id) + "_validator" });
            if (validator != null)
            {
                validatorWrapper.InnerHtml = validator.ToString().ProtectCurlyBrackets();
            }

            var exampleCode = string.IsNullOrEmpty(example)
                ? string.Empty
                : $"<em class=\"{DescriptionClassName}\">{example}</em>";

            var fieldCell = new TagBuilder("dd");
            fieldCell.AddCssClass(FieldClassName);
            fieldCell.InnerHtml = "{0}" + (forCheckbox ? " " + label : exampleCode) + validatorWrapper;
            fieldName = fieldName?.Replace("\"", "") ?? "";
            if (!string.IsNullOrEmpty(fieldName))
            {
                fieldName = $"data-field_name=\"{fieldName}\"";
            }

            return $"<dl class=\"{RowClassName}\" data-field_form_name=\"{id}\" {fieldName}>{labelCell}{fieldCell}</dl>";
        }

        private static MvcHtmlString Editor(this HtmlHelper html, FieldValue pair, bool articleIsAgregated = false, bool forceReadonly = false)
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

        internal static MvcHtmlString VersionRelation(this HtmlHelper source, string id, string value, string valueToMerge, Field field, int articleId, ArticleViewType viewType)
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

        internal static MvcHtmlString Relation(this HtmlHelper source, string id, IEnumerable<QPSelectListItem> list, ControlOptions options, Field field, int articleId, bool isListOverflow)
        {
            const string entityTypeCode = EntityTypeCode.Article;
            var baseField = field.GetBaseField(articleId);
            var contentId = baseField.RelateToContentId ?? 0;
            var fieldId = baseField.Id;
            var filter = baseField.GetRelationFilter(articleId);
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
                Filter = filter
            };

            return source.Relation(id, list, options, baseField.RelationType, isListOverflow, listArgs);
        }

        internal static MvcHtmlString QpLabelFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string displayName = "", bool withColon = true, int index = -1)
        {
            var data = html.GetMetaData(expression);
            return html.QpLabel(html.UniqueId(ExpressionHelper.GetExpressionText(expression), index), string.IsNullOrEmpty(displayName) ? data.DisplayName : displayName, withColon);
        }

        internal static MvcHtmlString VersionFile(this HtmlHelper source, string id, string value, Field field, ArticleVersion version, ArticleViewType viewType)
        {
            if (viewType == ArticleViewType.CompareVersions)
            {
                return source.VersionText(id, value);
            }

            var sb = new StringBuilder();
            sb.Append(source.VersionText(id, value));
            if (field.TypeId == FieldTypeCodes.Image)
            {
                sb.Append(source.ImagePreview(id));
            }

            sb.Append(source.FileDownload(id));
            var tb = source.FileWrapper(id, null, field, version.Id, version, true, false);
            tb.InnerHtml = sb.ToString();

            return MvcHtmlString.Create(tb.ToString());
        }

        internal static MvcHtmlString VersionDate(this HtmlHelper source, string id, string value, string valueToMerge, ArticleViewType viewType)
        {
            var resultValue = viewType == ArticleViewType.PreviewVersion ? source.FormatAsDate(value) : ArticleVersion.Merge(source.FormatAsDate(value), source.FormatAsDate(valueToMerge));
            return source.VersionText(id, resultValue);
        }

        internal static MvcHtmlString VersionTime(this HtmlHelper source, string id, string value, string valueToMerge, ArticleViewType viewType)
        {
            var resultValue = viewType == ArticleViewType.PreviewVersion ? source.FormatAsTime(value) : ArticleVersion.Merge(source.FormatAsTime(value), source.FormatAsTime(valueToMerge));
            return source.VersionText(id, resultValue);
        }

        internal static MvcHtmlString VersionDateTime(this HtmlHelper source, string id, string value, string valueToMerge, ArticleViewType viewType)
        {
            var resultValue = viewType == ArticleViewType.PreviewVersion ? source.FormatAsDateTime(value) : ArticleVersion.Merge(source.FormatAsDateTime(value), source.FormatAsDateTime(valueToMerge));
            return source.VersionText(id, resultValue);
        }

        public static string UniqueId(this HtmlHelper html, string id, int index = -1) => UniqueId(id, html.ViewContext.RouteData.Values[SpecialKeys.TabId], index);

        public static string UniqueId(string id, object tabId, int index = -1)
        {
            var resultId = id.Replace(".", "_");
            if (index == -1)
            {
                return tabId == null ? resultId : $"{tabId}_{resultId}";
            }

            return tabId == null ? $"{resultId}_{index}" : $"{tabId}_{resultId}_{index}";
        }

        public static string TabId(this HtmlHelper html) => html.ViewContext.RouteData.Values[SpecialKeys.TabId].ToString();

        public static bool IsReadOnly(this HtmlHelper html)
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

        public static MvcHtmlString Field(this HtmlHelper html, FieldValue pair, bool articleIsAgregated = false, bool forceReadonly = false)
        {
            // Если статья уже отображается как агрегированная, то у нее не отображаются поля-классификаторы и агрегирующие поля
            if (articleIsAgregated && (pair.Field.IsClassifier || pair.Field.Aggregated))
            {
                return MvcHtmlString.Empty;
            }

            // Если M2O поле обычного (не виртуального) контента имеет aggregated backfield - то такое поле не показывать
            if (pair.Field.ExactType == FieldExactTypes.M2ORelation && !pair.Field.Content.IsVirtual && pair.Field.BackRelation != null && pair.Field.BackRelation.Aggregated)
            {
                return MvcHtmlString.Empty;
            }

            var required = pair.Field.Required && !pair.Article.IsReadOnly;
            var fieldDescription = string.IsNullOrWhiteSpace(HttpUtility.HtmlDecode(pair.Field.Description ?? string.Empty).Replace("\u00A0", string.Empty)) ? null : pair.Field.Description;
            if (!string.IsNullOrEmpty(fieldDescription))
            {
                fieldDescription = fieldDescription.Replace("{", "{{").Replace("}", "}}");
            }

            var fieldTemplate = html.FieldTemplate(pair.Field.FormName, pair.Field.DisplayName, required: required, description: fieldDescription, fieldName: pair.Field.Name);
            var fieldHtmlString = string.Format(fieldTemplate, html.Editor(pair, articleIsAgregated, forceReadonly));
            if (pair.Field.IsClassifier && pair.Article.ViewType != ArticleViewType.Virtual)
            {
                fieldHtmlString = fieldHtmlString + GetExtensionArticleFields(html, pair);
            }

            return MvcHtmlString.Create(fieldHtmlString);
        }

        private static string GetExtensionArticleFields(HtmlHelper html, FieldValue pair)
        {
            var extensionContentId = Converter.ToInt32(pair.Value, 0);
            if (extensionContentId == 0)
            {
                return string.Empty;
            }

            Article aggregatedArticle = null;
            if (pair.Article.ViewType == ArticleViewType.CompareVersions || pair.Article.ViewType == ArticleViewType.PreviewVersion)
            {
                if (pair.Article.ViewType == ArticleViewType.CompareVersions)
                {
                    if (pair.Version == null || !StringComparer.InvariantCultureIgnoreCase.Equals(pair.Value, pair.ValueToMerge))
                    {
                        return string.Empty;
                    }

                    aggregatedArticle = pair.Version.AggregatedArticles.SingleOrDefault(n => n.ContentId == extensionContentId);
                }

                if (pair.Article.ViewType == ArticleViewType.PreviewVersion)
                {
                    if (pair.Version == null)
                    {
                        return string.Empty;
                    }

                    aggregatedArticle = pair.Version.AggregatedArticles.SingleOrDefault(n => n.ContentId == extensionContentId);
                }
            }
            else
            {
                aggregatedArticle = pair.Article.GetAggregatedArticleByClassifier(extensionContentId);
            }

            return $"<div class=\"articleWrapper_{extensionContentId}\">{html.AggregatedFieldValues(aggregatedArticle?.FieldValues.Where(n => !n.Field.Aggregated))}</div>"; // check;
        }

        public static MvcHtmlString DisplayField(this HtmlHelper html, string id, string title, object value) => MvcHtmlString.Create(string.Format(html.FieldTemplate(id, title), html.Span(html.UniqueId(id), value)));

        public static ModelMetadata GetMetaData<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression) => ModelMetadata.FromLambdaExpression(expression, html.ViewData);

        public static MvcHtmlString HtmlFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Func<TModel, object> content)
        {
            var data = html.GetMetaData(expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    content(html.ViewData.Model)
                )
            );
        }

        public static MvcHtmlString TextBoxField<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string defaultValue, Dictionary<string, object> htmlAttributes = null)
        {
            var fieldName = ExpressionHelper.GetExpressionText(expression);
            var data = html.GetMetaData(expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(fieldName, data.DisplayName, false, HtmlHelpersExtensions.GetExampleText(data.ContainerType, data.PropertyName)),
                    html.QpTextBox(fieldName, defaultValue, htmlAttributes).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString TextBoxField<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string defaultValue, bool forceReadonly)
        {
            var htmlAttributes = forceReadonly ? new Dictionary<string, object> { { "readonly", "readonly" } } : null;
            return html.TextBoxField(expression, defaultValue, htmlAttributes);
        }

        public static MvcHtmlString TextBoxFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            var data = html.GetMetaData(expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName, false, HtmlHelpersExtensions.GetExampleText(data.ContainerType, data.PropertyName)),
                    html.QpTextBoxFor(expression, htmlAttributes).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString TextAreaFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            var data = html.GetMetaData(expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.QpTextAreaFor(expression, htmlAttributes).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString VisualEditorFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Field field)
        {
            var data = html.GetMetaData(expression);
            var result = MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.VisualEditorFor(expression, field).ToHtmlString()
                )
            );

            return result;
        }

        public static MvcHtmlString DateTimeFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.DateTimeFor(expression).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString DateFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.DateFor(expression).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString TimeFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.TimeFor(expression).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString DisplayFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.DisplayFor(expression).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString DisplayFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.DisplayFor(expression, templateName).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString SelectFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, SelectOptions options) => html.SelectFieldFor(expression, list, null, options);

        public static MvcHtmlString SelectFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, Dictionary<string, object> htmlAttributes = null, SelectOptions options = null, bool required = false)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName, required: required),
                    html.QpDropDownListFor(
                        expression, list,
                        htmlAttributes,
                        options ?? new SelectOptions()
                    ).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString CheckBoxFieldFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression, string toggleId = null, bool reverseToggle = false, Dictionary<string, object> htmlAttributes = null)
        {
            var data = html.GetMetaData(expression);

            var result = new StringBuilder(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName, true),
                    html.QpCheckBoxFor(expression, toggleId, reverseToggle, htmlAttributes).ToHtmlString()
                )
            );

            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString RadioFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection = RepeatDirection.Horizontal, EntityDataListArgs entityDataListArgs = null, ControlOptions options = null)
        {
            var data = GetMetaData(html, expression);

            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.QpRadioButtonListFor(expression, list, repeatDirection, entityDataListArgs, options)
                )
            );
        }

        public static MvcHtmlString PasswordFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.PasswordFor(expression, html.QpHtmlProperties(expression, EditorType.Password))
                )
            );
        }

        public static MvcHtmlString NumericFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, double? minValue = null, double? maxValue = null, int decimalDigits = 0, Dictionary<string, object> htmlAttributes = null)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.NumericFor(expression, decimalDigits, minValue, maxValue, htmlAttributes)
                )
            );
        }

        public static MvcHtmlString FileForFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Field field, Dictionary<string, object> htmlAttributes = null)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(
                string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.FileFor(expression, field, htmlAttributes)
                )
            );
        }

        public static MvcHtmlString SingleItemPickerFieldFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, QPSelectListItem selected, EntityDataListArgs entityDataListArgs, ControlOptions options)
        {
            var data = GetMetaData(source, expression);
            return MvcHtmlString.Create(
                string.Format(
                    source.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    source.SingleItemPickerFor(expression, selected, entityDataListArgs, options)
                )
            );
        }

        public static MvcHtmlString AggregationListFieldFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<TValue>>> expression, IEnumerable<TValue> list, string bindings, Dictionary<string, string> additionalData = null)
        {
            var data = GetMetaData(source, expression);
            return MvcHtmlString.Create(
                string.Format(
                    source.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    source.AggregationListFor(ExpressionHelper.GetExpressionText(expression), list, bindings, additionalData)
                )
            );
        }

        public static MvcHtmlString VersionTextFieldFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, TValue text)
        {
            var data = GetMetaData(source, expression);
            return MvcHtmlString.Create(
                string.Format(
                    source.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    source.VersionTextFor(ExpressionHelper.GetExpressionText(expression), text.ToString())
                )
            );
        }

        public static MvcHtmlString VersionAreaFieldFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, TValue text)
        {
            var data = GetMetaData(source, expression);
            return MvcHtmlString.Create(string.Format(
                source.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                source.VersionAreaFor(ExpressionHelper.GetExpressionText(expression), text.ToString())
            ));
        }

        public static MvcHtmlString WorkflowFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<TValue>>> expression, IEnumerable<TValue> list) => source.WorkflowFor(ExpressionHelper.GetExpressionText(expression), list);

        public static MvcHtmlString CheckboxListFieldFor<TModel>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, IList<QPCheckedItem>>> expression,
            IEnumerable<QPSelectListItem> list,
            EntityDataListArgs entityDataListArgs,
            Dictionary<string, object> htmlAttributes,
            RepeatDirection repeatDirection = RepeatDirection.Vertical)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(string.Format(
                html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                html.QpCheckBoxListFor(expression, list, entityDataListArgs, htmlAttributes, repeatDirection)
            ));
        }

        public static MvcHtmlString CheckBoxTreeFieldFor<TModel>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression,
            string entityTypeCode,
            int? parentEntityId,
            string actionCode,
            bool allowGlobalSelection = false,
            Dictionary<string, object> htmlAttributes = null)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(string.Format(
                html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                html.CheckBoxTreeFor(expression, entityTypeCode, parentEntityId, actionCode, allowGlobalSelection, htmlAttributes).ToHtmlString()
            ));
        }

        public static MvcHtmlString VirtualFieldTreeFieldFor<TModel>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression,
            int? parentEntityId,
            int virtualContentId,
            Dictionary<string, object> htmlAttributes = null)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(string.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.VirtualFieldTreeFor(expression, parentEntityId, virtualContentId, htmlAttributes).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString UnionContentsFieldFor<TModel>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, IEnumerable<int>>> expression,
            IEnumerable<ListItem> selectedItemList,
            int siteId,
            Dictionary<string, object> htmlAttributes = null)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(string.Format(
                html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                html.UnionContentsFor(expression, selectedItemList, siteId, htmlAttributes).ToHtmlString()
            ));
        }

        public static MvcHtmlString MultipleItemPickerFieldFor<TModel>(
            this HtmlHelper<TModel> html,
            Expression<Func<TModel, IEnumerable<int>>> expression,
            IEnumerable<ListItem> selectedItemList,
            EntityDataListArgs entityDataListArgs,
            Dictionary<string, object> htmlAttributes = null)
        {
            var data = GetMetaData(html, expression);
            return MvcHtmlString.Create(string.Format(
                html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                html.MultipleItemPickerFor(expression, selectedItemList, entityDataListArgs, htmlAttributes).ToHtmlString()
            ));
        }

        private static MvcHtmlString ClassifierField(this HtmlHelper source, string name, string value, Field field, Article article, bool forceReadOnly)
        {
            Article aggregatedArticle = null;
            var classifierValue = Converter.ToInt32(value, 0);
            if (article.ViewType != ArticleViewType.Virtual)
            {
                aggregatedArticle = article.GetAggregatedArticleByClassifier(classifierValue);
            }

            var sb = new StringBuilder(source.BeginClassifierFieldComponent(name, value, field, article, aggregatedArticle, out var acticleHtmlElemId));

            if (forceReadOnly)
            {
                var classifierContent = ArticleViewModel.GetContentById(Converter.ToNullableInt32(value));
                var classifierContentName = classifierContent?.Name;
                var htmlAttributes = new Dictionary<string, object> { { "class", HtmlHelpersExtensions.ArticleTextboxClassName }, { "disabled", "disabled" }, { HtmlHelpersExtensions.DataContentFieldName, field.Name } };
                sb.Append(source.QpTextBox(name, classifierContentName, htmlAttributes));
            }
            else
            {
                var contentListHtmlAttrs = new Dictionary<string, object> { { "class", "dropDownList classifierContentList" }, { HtmlHelpersExtensions.DataContentFieldName, field.Name } };
                sb.Append(source.DropDownList(name, source.List(ArticleViewModel.GetAggregatableContentsForClassifier(field, value)), FieldStrings.SelectContent, contentListHtmlAttrs).ToHtmlString());
            }

            sb.Append(EndClassifierFieldComponent());
            return MvcHtmlString.Create(sb.ToString());
        }

        private static MvcHtmlString VersionClassifierField(this HtmlHelper source, string name, string value, Field field, Article article, ArticleVersion version = null, bool forceReadOnly = true, string valueToMerge = null)
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

            var sb = new StringBuilder(source.BeginClassifierFieldComponent(name, value, field, article, aggregatedArticle, out string acticleHtmlElemId));

            if (forceReadOnly)
            {
                sb.Append(source.VersionText(name, name1));
            }
            else
            {
                var contentListHtmlAttrs = new Dictionary<string, object> { { "class", "dropDownList classifierContentList" } };
                sb.Append(source.DropDownList(name, source.List(ArticleViewModel.GetAggregatableContentsForClassifier(field, value)), FieldStrings.SelectContent, contentListHtmlAttrs).ToHtmlString());
            }

            sb.Append(EndClassifierFieldComponent());
            return MvcHtmlString.Create(sb.ToString());
        }

        private static string BeginClassifierFieldComponent(this HtmlHelper html, string name, string value, Field field, Article article, EntityObject aggregatedArticle, out string acticleHtmlElemId)
        {
            var componentElemId = html.UniqueId(name);
            acticleHtmlElemId = componentElemId + "_articleHtml";
            var aggregatedArticleId = aggregatedArticle?.Id.ToString() ?? string.Empty;
            return $"<div id={componentElemId} data-host_id=\"{html.TabId()}\" data-field_name=\"{name}\" data-aggregated_content_id=\"{value}\" data-aggregated_article_id=\"{aggregatedArticleId}\" data-root_content_id=\"{article.ContentId}\" data-classifier_id=\"{field.Id}\" data-root_article_id=\"{article.Id}\" data-acticle_html_id=\"{acticleHtmlElemId}\" data-is_not_changeable=\"{!article.IsNew && !field.Changeable}\" class=\"classifierComponent\">";
        }

        private static string EndClassifierFieldComponent() => "</div>";

        public static MvcHtmlString AggregatedFieldValues(this HtmlHelper html, IEnumerable<FieldValue> values)
        {
            var sb = new StringBuilder();
            values?.Aggregate(sb, (b, pair) =>
            {
                b.Append(html.Field(pair, true).ToHtmlString());
                return b;
            });

            return MvcHtmlString.Create(sb.ToString());
        }

        private static MvcHtmlString StringEnumEditor(this HtmlHelper html, string name, string value, Field field, bool forceReadOnly, bool isNew)
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
                return html.QpRadioButtonList(name, items, RepeatDirection.Horizontal, new ControlOptions { HtmlAttributes = htmlAttributes, Enabled = !forceReadOnly });
            }

            return html.QpDropDownList(name, items, !field.Required || !isNew ? GlobalStrings.NotSelected : null, new ControlOptions
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
