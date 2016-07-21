using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels;
using System.Web;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelperFieldExtensions
    {
        private const string LabelClassName = "label";
        private const string FieldClassName = "field";
        private const string ValidatorsClassName = "validators";
        private const string RowClassName = "row";
        private const string DescriptionClassName = "description";

        private static readonly Regex TabIdRegExp = new Regex(@"^tab[0-9]+$");
        private static readonly Regex WindowIdRegExp = new Regex(@"^win[0-9]+$");

        #region Private And Internal Members

        public static string FieldTemplate(this HtmlHelper html, string id, string title, bool forCheckbox = false, string example = null, bool required = false, string description = null)
        {
            string label = html.QpLabel(html.UniqueId(id), title, !forCheckbox).ToString();

            if (!String.IsNullOrWhiteSpace(description))
            {
                label = String.Format(
                       "{0} <span class='linkButton fieldDescription' data-field_description_text='{1}'>" +
                           "<a href='javascript:void(0);'>" +
                               "<span class='text'>(?)</span>" +
                           "</a>" +
                       "</span>",
                       label, description);
            }

            if (required && !forCheckbox)
            {
                TagBuilder star = new TagBuilder("span");
                star.MergeAttribute("class", "star");
                star.InnerHtml = "*";
                label = String.Format("{0} {1}", star, label);
            }

            TagBuilder labelCell = new TagBuilder("dt");
            labelCell.AddCssClass(LabelClassName);
            labelCell.InnerHtml = (forCheckbox) ? String.Empty : label;

            TagBuilder validatorWrapper = new TagBuilder("em");
            validatorWrapper.AddCssClass(ValidatorsClassName);
            MvcHtmlString validator = html.ValidationMessage(id, new { id = html.UniqueId(id) + "_validator" });
            if (validator != null)
                validatorWrapper.InnerHtml = validator.ToString().ProtectCurlyBrackets();

            string exampleCode = (String.IsNullOrEmpty(example)) ? String.Empty : RenderDescription(example);

            TagBuilder fieldCell = new TagBuilder("dd");
            fieldCell.AddCssClass(FieldClassName);
            string cellHtml = "{0}" + (forCheckbox ? " " + label : exampleCode);
            fieldCell.InnerHtml = cellHtml + validatorWrapper.ToString();

            TagBuilder row = new TagBuilder("dl");
            row.AddCssClass(RowClassName);
            row.MergeDataAttribute("field_form_name", id);
            row.InnerHtml = labelCell.ToString() + fieldCell.ToString();
            return row.ToString();

        }

        private static string RenderDescription(string example)
        {
            TagBuilder description = new TagBuilder("em");
            description.AddCssClass(DescriptionClassName);
            description.InnerHtml = example;
            return description.ToString();
        }

        private static MvcHtmlString Editor(this HtmlHelper html, FieldValue pair, bool articleIsAgregated = false, bool forceReadonly = false)
        {
            Field field = pair.Field;
            string id = pair.Field.FormName;
            string value = pair.Value;
            bool isVersionView = pair.Article.ViewType == ArticleViewType.CompareVersions;
            if (isVersionView)
            {
                switch (field.Type.Name)
                {
                    case FieldTypeName.Textbox:
                    case FieldTypeName.VisualEdit:
                        return html.VersionArea(id, value.ToString());
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
            else
            {
                bool readOnly = forceReadonly || pair.Article.IsReadOnly || field.IsReadOnly || (!articleIsAgregated && pair.Article.Content.HasAggregatedFields);
                Dictionary<string, object> htmlAttributes = html.QPHtmlProperties(id, field, readOnly);

                switch (field.ExactType)
                {
                    case FieldExactTypes.Textbox:
                        return html.QpTextArea(id, value, htmlAttributes);
                    case FieldExactTypes.VisualEdit:
                        return html.VisualEditor(id, value, htmlAttributes, field, readOnly);
                    case FieldExactTypes.File:
                    case FieldExactTypes.Image:
                    case FieldExactTypes.DynamicImage:
                        return html.File(id, value, htmlAttributes, field, (pair.Version != null) ? pair.Version.Id : pair.Article.Id, pair.Version);
                    case FieldExactTypes.DateTime:
                        return html.DateTime(id, value, htmlAttributes, !field.Required, readOnly);
                    case FieldExactTypes.Date:
                        return html.Date(id, value, htmlAttributes, !field.Required, readOnly);
                    case FieldExactTypes.Time:
                        return html.Time(id, value, htmlAttributes, !field.Required, readOnly);
                    case FieldExactTypes.Boolean:
                        return html.QpCheckBox(id, null, Converter.ToBoolean(value), htmlAttributes);
                    case FieldExactTypes.Numeric:
                        return html.NumericTextBox(id, value, htmlAttributes, field);
                    case FieldExactTypes.Classifier:
                        {
                            if (pair.Version != null)
                                return html.VersionClassifierField(id, value, field, pair.Article, pair.Version, readOnly);
                            else
                                return html.ClassifierField(id, value, field, pair.Article, readOnly);
                        }
                    case FieldExactTypes.O2MRelation:
                    case FieldExactTypes.M2MRelation:
                    case FieldExactTypes.M2ORelation:
                        {
                            var result = ArticleViewModel.GetListForRelation(field, value, pair.Article.Id);
                            return html.Relation(id, html.List(result.Items), new ControlOptions() { Enabled = !readOnly, HtmlAttributes = htmlAttributes },
                                field, pair.Article.Id, result.IsListOverflow);
                        }
                    case FieldExactTypes.StringEnum:
                        return html.StringEnumEditor(id, value, field, readOnly, pair.Article.IsNew);
                    default:
                        return html.QpTextBox(id, value, htmlAttributes);
                }
            }
        }



        internal static MvcHtmlString VersionRelation(this HtmlHelper source, string id, string value, string valueToMerge, Field field, int articleId, ArticleViewType viewType)
        {

            if (viewType == ArticleViewType.CompareVersions)
            {
                IEnumerable<ListItem> titles1 = field.GetRelatedTitles(value);
                IEnumerable<ListItem> titles2 = field.GetRelatedTitles(valueToMerge);
                return source.VersionText(id, ArticleVersion.MergeRelation(titles1, titles2));
            }
            else
            {
                string titles = String.Join("<br />", field.GetRelatedTitles(value).Select(i => String.Format("(#{0}) - {1}", i.Value, i.Text)));
                return source.VersionText(id, titles);
            }
        }

        internal static MvcHtmlString Relation(this HtmlHelper source, string id, IEnumerable<QPSelectListItem> list, ControlOptions options, Field field, int articleId, bool isListOverflow)
        {
            string entityTypeCode = Constants.EntityTypeCode.Article;
            Field baseField = field.GetBaseField(articleId);
            int contentId = baseField.RelateToContentId ?? 0;
            int fieldId = baseField.Id;
            string filter = baseField.GetRelationFilter(articleId);

            Content relatedToContent = baseField.RelatedToContent;
            string addNewActionCode = Constants.ActionCode.None;
            string readActionCode = Constants.ActionCode.None;
            if (relatedToContent != null)
            {
                if (!relatedToContent.IsVirtual)
                {
                    readActionCode = Constants.ActionCode.EditArticle;
                    if (relatedToContent.IsAccessible(Constants.ActionTypeCode.Update))
                        addNewActionCode = Constants.ActionCode.AddNewArticle;
                }
                else
                    readActionCode = Constants.ActionCode.ViewVirtualArticle;
            }

            EntityDataListArgs listArgs = new EntityDataListArgs()
            {
                EntityTypeCode = entityTypeCode,
                ParentEntityId = contentId,
                EntityId = articleId,
                ListId = fieldId,
                AddNewActionCode = addNewActionCode,
                ReadActionCode = readActionCode,
                SelectActionCode = (baseField.RelationType == RelationType.OneToMany) ? Constants.ActionCode.SelectArticle : Constants.ActionCode.MultipleSelectArticle,
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
            return html.QpLabel(html.UniqueId(ExpressionHelper.GetExpressionText(expression), index), String.IsNullOrEmpty(displayName) ? data.DisplayName : displayName, withColon);
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
            string resultValue = (viewType == ArticleViewType.PreviewVersion) ? source.FormatAsDate(value) : ArticleVersion.Merge(source.FormatAsDate(value), source.FormatAsDate(valueToMerge));
            return source.VersionText(id, resultValue);
        }

        internal static MvcHtmlString VersionTime(this HtmlHelper source, string id, string value, string valueToMerge, ArticleViewType viewType)
        {
            string resultValue = (viewType == ArticleViewType.PreviewVersion) ? source.FormatAsTime(value) : ArticleVersion.Merge(source.FormatAsTime(value), source.FormatAsTime(valueToMerge));
            return source.VersionText(id, resultValue);
        }

        internal static MvcHtmlString VersionDateTime(this HtmlHelper source, string id, string value, string valueToMerge, ArticleViewType viewType)
        {
            string resultValue = (viewType == ArticleViewType.PreviewVersion) ? source.FormatAsDateTime(value) : ArticleVersion.Merge(source.FormatAsDateTime(value), source.FormatAsDateTime(valueToMerge));
            return source.VersionText(id, resultValue);
        }

        #endregion

        /// <summary>
        /// Генерирует ID, уникальный для текущего таба (id таба берется из RouteData)
        /// </summary>
        public static string UniqueId(this HtmlHelper html, string id, int index = -1)
        {
            return UniqueId(id, html.ViewContext.RouteData.Values[SpecialKeys.TabId], index);
        }

        // TODO: VERIFY!!!
        public static string UniqueId(string id, object tabId, int index = -1)
        {
            var resultId = id.Replace(".", "_");
            if (index == -1)
            {
                if (tabId == null)
                {
                    return resultId;
                }

                return $"{tabId}_{resultId}";
            }

            if (tabId == null)
            {
                return $"{resultId}_{index}";
            }

            return $"{tabId}_{resultId}_{index}";
        }

        public static string TabId(this HtmlHelper html)
        {
            return html.ViewContext.RouteData.Values[SpecialKeys.TabId].ToString();
        }

        /// <summary>
        /// Проверяет, является ли текущий контекст ReadOnly (по ViewData)
        /// </summary>
        public static bool IsReadOnly(this HtmlHelper html)
        {
            var obj = html.ViewData[SpecialKeys.IsEntityReadOnly];
            return obj != null && (bool)obj;
        }

        /// <summary>
        /// Определяет на основе идентификатора таба является ли контейнер табом
        /// </summary>
        /// <param name="tabId">идентификатор таба</param>
        /// <returns>результат проверки (true - таб; false - не таб)</returns>
        public static bool IsTab(string tabId)
        {
            var isTab = false;
            if (!string.IsNullOrWhiteSpace(tabId))
            {
                isTab = TabIdRegExp.IsMatch(tabId);
            }

            return isTab;
        }

        /// <summary>
        /// Определяет на основе идентификатора таба является ли контейнер всплывающим окном
        /// </summary>
        /// <param name="tabId">идентификатор таба</param>
        /// <returns>результат проверки (true - окно; false - не окно)</returns>
        public static bool IsWindow(string tabId)
        {
            bool isWindow = false;

            if (!String.IsNullOrWhiteSpace(tabId))
            {
                isWindow = WindowIdRegExp.IsMatch(tabId);
            }

            return isWindow;
        }

        #region Untyped helpers

        public static MvcHtmlString Field(this HtmlHelper html, FieldValue pair, bool articleIsAgregated = false, bool forceReadonly = false)
        {
            // Если статья уже отображается как агрегированная, то у нее не отображаются поля-классификаторы и агрегирующие поля
            if (articleIsAgregated && (pair.Field.IsClassifier || pair.Field.Aggregated))
                return MvcHtmlString.Empty;

            // Если M2O поле обычного (не виртуального) контента имеет aggregated backfield - то такое поле не показывать
            if (pair.Field.ExactType == FieldExactTypes.M2ORelation && !pair.Field.Content.IsVirtual && pair.Field.BackRelation != null && pair.Field.BackRelation.Aggregated)
                return MvcHtmlString.Empty;

            bool required = pair.Field.Required && !pair.Article.IsReadOnly;
            string fieldDescription = String.IsNullOrWhiteSpace(HttpUtility.HtmlDecode(pair.Field.Description ?? "").Replace("\u00A0", "")) ? null : pair.Field.Description;

            if (!String.IsNullOrEmpty(fieldDescription))
                fieldDescription = fieldDescription.Replace("{", "{{").Replace("}", "}}");

            return MvcHtmlString.Create(
                String.Format(html.FieldTemplate(pair.Field.FormName, pair.Field.DisplayName, required: required, description: fieldDescription), html.Editor(pair, articleIsAgregated, forceReadonly).ToString())
            );
        }

        public static MvcHtmlString DisplayField(this HtmlHelper html, string id, string title, object value)
        {
            return MvcHtmlString.Create(
                String.Format(html.FieldTemplate(id, title), html.Span(html.UniqueId(id), value).ToString())
            );
        }


        #endregion

        #region Typed helpers

        public static ModelMetadata GetMetaData<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            return ModelMetadata.FromLambdaExpression<TModel, TValue>(expression, html.ViewData);
        }

        public static MvcHtmlString HtmlFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Func<TModel, object> content)
        {
            ModelMetadata data = html.GetMetaData<TModel, TValue>(expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    content(html.ViewData.Model)
                )
            );
        }

        public static MvcHtmlString TextBoxFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = html.GetMetaData<TModel, TValue>(expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression),
                    data.DisplayName,
                    false,
                    html.GetExampleText(data.ContainerType, data.PropertyName)),
                    html.QpTextBoxFor(expression, htmlAttributes).ToHtmlString()
                )
            );
        }


        public static MvcHtmlString TextAreaFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = html.GetMetaData<TModel, TValue>(expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.QpTextAreaFor(expression, htmlAttributes).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString VisualEditorFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Field field)
        {
            ModelMetadata data = html.GetMetaData<TModel, TValue>(expression);
            var result = MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.VisualEditorFor(expression, field).ToHtmlString()
                )
            );

            return result;
        }

        public static MvcHtmlString DateTimeFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.DateTimeFor(expression).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString DateFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.DateFor(expression).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString TimeFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.TimeFor(expression).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString DisplayFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.DisplayFor(expression).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString DisplayFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string templateName)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.DisplayFor(expression, templateName).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString SelectFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, SelectOptions options)
        {
            return html.SelectFieldFor(expression, list, null, options);
        }


        public static MvcHtmlString SelectFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, Dictionary<string, object> htmlAttributes = null, SelectOptions options = null, bool required = false)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);

            return MvcHtmlString.Create(
                String.Format(
                html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName, required: required),
                    html.QpDropDownListFor(
                                            expression, list,
                                            htmlAttributes,
                                            options == null ? new SelectOptions() : options
                                          ).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString CheckBoxFieldFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, bool>> expression, string toggleId = null, bool reverseToggle = false, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = html.GetMetaData(expression);

            StringBuilder result = new StringBuilder(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName, true),
                    html.QpCheckBoxFor(expression, toggleId, reverseToggle, htmlAttributes).ToHtmlString()
                )
            );

            return MvcHtmlString.Create(result.ToString());
        }

        public static MvcHtmlString RadioFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection = RepeatDirection.Horizontal, EntityDataListArgs entityDataListArgs = null, ControlOptions options = null)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);

            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.QpRadioButtonListFor(expression, list, repeatDirection, entityDataListArgs, options)
                )
            );
        }

        public static MvcHtmlString PasswordFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.PasswordFor(expression,
                    html.QPHtmlProperties(expression,
                    EditorType.Password))
                )
            );
        }

        public static MvcHtmlString NumericFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, double? minValue = null, double? maxValue = null, int decimalDigits = 0, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.NumericFor(expression, decimalDigits, minValue, maxValue, htmlAttributes)
                )
            );
        }

        public static MvcHtmlString PlUploadFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, string val, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.PlUploadFor(expression, val, htmlAttributes)
                )
            );
        }

        public static MvcHtmlString FileForFieldFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, Field field, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.FileFor(expression, field, htmlAttributes)
                )
            );
        }

        /// <summary>
        /// Single Item Picker для выбора Entity
        /// </summary>
        public static MvcHtmlString SingleItemPickerFieldFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, QPSelectListItem selected, EntityDataListArgs entityDataListArgs, ControlOptions options)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(source, expression);
            return MvcHtmlString.Create(
                String.Format(
                    source.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    source.SingleItemPickerFor(expression, selected, entityDataListArgs, options)
                )
            );
        }

        /// <param name="AdditionalData">Каждая строка словаря попадет в корень модели, под соотв именем, предназначен для значений различных DDL</param>
        /// <returns></returns>
        public static MvcHtmlString AggregationListFieldFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<TValue>>> expression, IEnumerable<TValue> list, string bindings, Dictionary<string, string> additionalData = null)
        {
            ModelMetadata data = GetMetaData<TModel, IEnumerable<TValue>>(source, expression);
            return MvcHtmlString.Create(
                String.Format(
                    source.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    source.AggregationListFor(ExpressionHelper.GetExpressionText(expression), list, bindings, additionalData)
                )
            );
        }

        public static MvcHtmlString VersionTextFieldFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, TValue text)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(source, expression);
            return MvcHtmlString.Create(
                String.Format(
                    source.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    source.VersionTextFor(ExpressionHelper.GetExpressionText(expression), text.ToString())
                )
            );
        }

        public static MvcHtmlString VersionAreaFieldFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, TValue text)
        {
            ModelMetadata data = GetMetaData<TModel, TValue>(source, expression);
            return MvcHtmlString.Create(
                String.Format(
                    source.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    source.VersionAreaFor(ExpressionHelper.GetExpressionText(expression), text.ToString())
                )
            );
        }

        public static MvcHtmlString WorkflowFieldFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<TValue>>> expression, IEnumerable<TValue> list)
        {
            ModelMetadata data = GetMetaData<TModel, IEnumerable<TValue>>(source, expression);
            return MvcHtmlString.Create(
                String.Format(
                    source.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
            source.WorkflowFor(ExpressionHelper.GetExpressionText(expression), list)
                )
            );
        }

        public static MvcHtmlString WorkflowFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<TValue>>> expression, IEnumerable<TValue> list)
        {
            ModelMetadata data = GetMetaData<TModel, IEnumerable<TValue>>(source, expression);
            return source.WorkflowFor(ExpressionHelper.GetExpressionText(expression), list);
        }

        public static MvcHtmlString CheckboxListFieldFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, IList<QPCheckedItem>>> expression,
            IEnumerable<QPSelectListItem> list, EntityDataListArgs entityDataListArgs, Dictionary<string, object> htmlAttributes, RepeatDirection repeatDirection = RepeatDirection.Vertical)
        {
            ModelMetadata data = GetMetaData<TModel, IList<QPCheckedItem>>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.QpCheckBoxListFor(expression, list, entityDataListArgs, htmlAttributes, repeatDirection)
                )
            );
        }

        public static MvcHtmlString CheckBoxTreeFieldFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression,
                            string entityTypeCode, int? parentEntityId, string actionCode, bool allowGlobalSelection = false, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = GetMetaData<TModel, IEnumerable<QPTreeCheckedNode>>(html, expression);

            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.CheckBoxTreeFor(expression,
                                         entityTypeCode,
                                         parentEntityId,
                                         actionCode,
                                         allowGlobalSelection,
                                         htmlAttributes)
                        .ToHtmlString()
                )
            );
        }

        public static MvcHtmlString VirtualFieldTreeFieldFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression,
            int? parentEntityId, int virtualContentId, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = GetMetaData<TModel, IEnumerable<QPTreeCheckedNode>>(html, expression);

            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.VirtualFieldTreeFor(expression,
                                         parentEntityId,
                                         virtualContentId,
                                         htmlAttributes)
                        .ToHtmlString()
                )
            );
        }

        public static MvcHtmlString UnionContentsFieldFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, IEnumerable<int>>> expression, IEnumerable<ListItem> selectedItemList, int siteId, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = GetMetaData<TModel, IEnumerable<int>>(html, expression);
            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.UnionContentsFor(expression, selectedItemList, siteId, htmlAttributes).ToHtmlString()
                )
            );
        }

        public static MvcHtmlString MultipleItemPickerFieldFor<TModel>(this HtmlHelper<TModel> html, Expression<Func<TModel, IEnumerable<int>>> expression, IEnumerable<ListItem> selectedItemList,
            EntityDataListArgs entityDataListArgs, Dictionary<string, object> htmlAttributes = null)
        {
            ModelMetadata data = GetMetaData<TModel, IEnumerable<int>>(html, expression);

            return MvcHtmlString.Create(
                String.Format(
                    html.FieldTemplate(ExpressionHelper.GetExpressionText(expression), data.DisplayName),
                    html.MultipleItemPickerFor(expression, selectedItemList, entityDataListArgs, htmlAttributes).ToHtmlString()
                )
            );
        }

        /// <summary>
        /// Поле-классификатор
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="htmlAttributes"></param>
        /// <param name="field"></param>
        /// <returns></returns>
        private static MvcHtmlString ClassifierField(this HtmlHelper source, string name, string value, Field field, Article article, bool forceReadOnly)
        {
            // Получить агрегированную статью
            Article aggregatedArticle = null;
            int classifierValue = Converter.ToInt32(value, 0);
            if (article.ViewType != ArticleViewType.Virtual)
                aggregatedArticle = article.GetAggregatedArticleByClassifier(classifierValue);

            string acticleHtmlElemId;
            StringBuilder sb = new StringBuilder(source.BeginClassifierFieldComponent(name, value, field, article, aggregatedArticle, out acticleHtmlElemId));

            // Агрегированный контент
            if (forceReadOnly)
            {
                Content classifierContent = ArticleViewModel.GetContentById(Converter.ToNullableInt32(value));
                string classifierContentName = classifierContent != null ? classifierContent.Name : null;
                sb.Append(
                    source.QpTextBox(name, classifierContentName,
                        new Dictionary<string, object> { { "class", HtmlHelpersExtensions.ARTICLE_TEXTBOX_CLASS_NAME }, { "disabled", "disabled" } }
                    )
                );
            }
            else
            {
                Dictionary<string, object> contentListHtmlAttrs = new Dictionary<string, object>() { { "class", "dropDownList classifierContentList" } };
                sb.Append(
                    source.DropDownList(name,
                        source.List(ArticleViewModel.GetAggregetableContentsForClassifier(field, value)),
                        FieldStrings.SelectContent,
                        contentListHtmlAttrs
                    ).ToHtmlString()
                );
            }

            // Содержимое агрегированной статьи (если она есть)
            sb.Append(source.BeginAggregatedArticleData(acticleHtmlElemId));
            if (aggregatedArticle != null)
                sb.Append(HttpUtility.HtmlEncode(source.AggregatedArticle(aggregatedArticle).ToHtmlString()));
            sb.Append(source.EndAggregatedArticleData());

            sb.Append(source.EndClassifierFieldComponent());

            return MvcHtmlString.Create(sb.ToString());
        }

        /// <summary>
        /// Поле-классификатор в версии статьи
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <param name="article"></param>
        /// <param name="version"></param>
        /// <param name="valueToMerge"></param>
        /// <returns></returns>
        private static MvcHtmlString VersionClassifierField(this HtmlHelper source, string name, string value, Field field, Article article, ArticleVersion version = null, bool forceReadOnly = true, string valueToMerge = null)
        {
            Content classifierContent = ArticleViewModel.GetContentById(Converter.ToNullableInt32(value));
            string classifierContentName = classifierContent != null ? classifierContent.Name : null;

            if (!StringComparer.InvariantCultureIgnoreCase.Equals(value, valueToMerge) && article.ViewType == ArticleViewType.CompareVersions) // Если значение классификатора изменилось, то не загружать агрегированную статью
            {
                Content classifierContent2 = ArticleViewModel.GetContentById(Converter.ToNullableInt32(valueToMerge));
                string classifierContentName2 = classifierContent2 != null ? classifierContent2.Name : null;

                string mergedValue = ArticleVersion.Merge(Formatter.ProtectHtml(classifierContentName), Formatter.ProtectHtml(classifierContentName2));
                return source.VersionText(name, mergedValue);
            }
            else
            {
                // Получить агрегированную статью
                Article aggregatedArticle = null;
                int classifierValue = Converter.ToInt32(value, 0);
                if (article.ViewType != ArticleViewType.Virtual)
                    aggregatedArticle = article.GetAggregatedArticleByClassifier(classifierValue);

                string acticleHtmlElemId;
                StringBuilder sb = new StringBuilder(source.BeginClassifierFieldComponent(name, value, field, article, aggregatedArticle, out acticleHtmlElemId));

                if (forceReadOnly)
                {
                    // Агрегированный контент
                    sb.Append(source.VersionText(name, classifierContentName));
                }
                else
                {
                    Dictionary<string, object> contentListHtmlAttrs = new Dictionary<string, object>() { { "class", "dropDownList classifierContentList" } };
                    sb.Append(
                        source.DropDownList(name,
                            source.List(ArticleViewModel.GetAggregetableContentsForClassifier(field, value)),
                            FieldStrings.SelectContent,
                            contentListHtmlAttrs
                        ).ToHtmlString()
                    );
                }

                // Содержимое агрегированной статьи (если она есть)
                sb.Append(source.BeginAggregatedArticleData(acticleHtmlElemId));
                if (aggregatedArticle != null)
                {
                    IEnumerable<FieldValue> aggFVs = version.GetAggregatedFieldValues(aggregatedArticle);
                    sb.Append(HttpUtility.HtmlEncode(source.AggregatedFieldValues(aggFVs).ToHtmlString()));
                }
                sb.Append(source.EndAggregatedArticleData());

                sb.Append(source.EndClassifierFieldComponent());

                return MvcHtmlString.Create(sb.ToString());
            }
        }

        private static string BeginClassifierFieldComponent(this HtmlHelper source, string name, string value, Field field, Article article, Article aggregatedArticle, out string acticleHtmlElemId)
        {
            string componentElemId = source.UniqueId(name);
            acticleHtmlElemId = componentElemId + "_articleHtml";
            string aggregatedArticleId = aggregatedArticle != null ? aggregatedArticle.Id.ToString() : String.Empty;
            return String.Format("<div id={0} data-host_id=\"{8}\" data-field_name=\"{9}\" data-aggregated_content_id=\"{1}\" data-aggregated_article_id=\"{5}\" data-root_content_id=\"{7}\" data-classifier_id=\"{2}\" data-root_article_id=\"{3}\" data-acticle_html_id=\"{4}\" data-is_not_changeable=\"{6}\" class=\"classifierComponent\">",
                componentElemId, value, field.Id, article.Id, acticleHtmlElemId, aggregatedArticleId, (!article.IsNew && !field.Changeable), article.ContentId, source.TabId(), name);
        }
        private static string EndClassifierFieldComponent(this HtmlHelper source)
        {
            return "</div>";
        }

        private static string BeginAggregatedArticleData(this HtmlHelper source, string acticleHtmlElemId)
        {
            return string.Format("<script type=\"text/plain\" id=\"{0}\">", acticleHtmlElemId);
        }
        private static string EndAggregatedArticleData(this HtmlHelper source)
        {
            return "</script>";
        }


        /// <summary>
        /// Возвращает разметку формы агрегируемой статьи
        /// </summary>
        /// <param name="source"></param>
        /// <param name="article"></param>
        /// <returns></returns>
        public static MvcHtmlString AggregatedArticle(this HtmlHelper source, Article article)
        {
            return source.AggregatedFieldValues(article.FieldValues.Where(n => !n.Field.Aggregated));
        }

        private static MvcHtmlString AggregatedFieldValues(this HtmlHelper source, IEnumerable<FieldValue> values)
        {
            StringBuilder result = new StringBuilder();
            values.Aggregate(result, (sb, pair) =>
            {
                sb.Append(source.Field(pair, articleIsAgregated: true).ToHtmlString());
                return sb;
            });

            return MvcHtmlString.Create(result.ToString());
        }

        /// <summary>
        /// Редактор строкового перечисления
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="field"></param>
        /// <param name="htmlAttributes"></param>
        /// <returns></returns>
        private static MvcHtmlString StringEnumEditor(this HtmlHelper html, string name, string value, Field field, bool forceReadOnly, bool isNew)
        {
            const string specClass = "qp-stringEnumEditor";

            IEnumerable<QPSelectListItem> items = field.StringEnumItems
                .Select(i => new QPSelectListItem
                {
                    Text = i.Alias,
                    Value = i.Value,
                    Selected = i.Value.Equals(value, StringComparison.InvariantCulture)
                });

            if (field.ShowAsRadioButtons)
            {
                Dictionary<string, object> htmlAttributes = new Dictionary<string, object>();
                htmlAttributes.AddCssClass(specClass);
                return html.QpRadioButtonList(name, items, RepeatDirection.Horizontal, new ControlOptions { HtmlAttributes = htmlAttributes, Enabled = !forceReadOnly });
            }
            else
                return html.QpDropDownList(name, items, !field.Required || !isNew ? GlobalStrings.NotSelected : null, new ControlOptions
                {
                    Enabled = !forceReadOnly,
                    HtmlAttributes = new Dictionary<string, object>
                    {
                        {"class", specClass},
                    }
                });
        }
        #endregion
    }
}
