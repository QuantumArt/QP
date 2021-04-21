using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Infrastructure.Extensions;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HtmlHelpersExtensions
    {
        public const string TextboxClassName = "textbox simple-text";
        public const string VisualEditorTextboxClassName = "textbox";
        public const string ArticleTextboxClassName = "article textbox simple-text";
        public const string ArticleVisualEditorTextboxClassName = "article textbox";
        public const string NumericTextboxClassName = "t-input textbox";
        public const string CheckboxClassName = "checkbox";
        public const string SimpleCheckboxClassName = "simple-checkbox";
        public const string CheckboxListItemClassName = "chb-list-item";
        public const string MultiplePickerItemCheckboxClassName = "multi-picker-item";
        public const string MultiplePickerOverflowHiddenValue = "overflowHiddenValue";
        public const string NoTrackChangeInputClass = "qp-notChangeTrack";
        public const string VisualEditorClassName = "visualEditor";
        public const string VisualEditorComponentClassName = "visualEditorComponent";
        public const string VisualEditorToolbarClassName = "visualEditorToolbar";
        public const string VersionAreaClassName = "versionArea";
        public const string VersionTextClassName = "versionText";
        public const string DataListClassName = "dataList";
        public const string CheckBoxTreeClassName = "checkboxTree";
        public const string DropDownListClassName = "dropDownList";
        public const string ListboxClassName = "listBox";
        public const string RadioButtonsListClassName = "radioButtonsList";
        public const string CheckboxsListClassName = "checkboxsList";
        public const string SingleItemPickerClassName = "singleItemPicker";
        public const string MultipleItemPickerClassName = "multipleItemPicker";
        public const string FileFieldClassName = "fileField";
        public const string FieldWrapperClassName = "fieldWrapper";
        public const string BrowseButtonClassName = "browseButton";
        public const string DownloadButtonClassName = "downloadButton";
        public const string PreviewButtonClassName = "previewButton";
        public const string LibraryButtonClassName = "libraryButton";
        public const string PickButtonClassName = "pickButton";
        public const string LinkButtonClassName = "linkButton";
        public const string ActionLinkClassName = "actionLink";
        public const string AggregationListClassName = "aggregationList";
        public const string WorkflowControlClassName = "workflow_control";
        public const string AggregationListResultClassName = "aggregationListResult";
        public const string AggregationListContainerClassName = "aggregationListContainer";
        public const string WorkflowResultClassName = "workflowResult";
        public const string WorkflowContainerClassName = "workflowContainer";
        public const string HorizontalDirectionClassName = "horizontalDirection";
        public const string VerticalDirectionClassName = "verticalDirection";
        public const string DisabledClassName = "disabled";
        public const string SelfClearFloatsClassName = "group";
        public const string DataContentFieldName = "data-content_field_name";
        public const string DateTextboxClassName = "date";
        public const string TimeTextboxClassName = "time";
        public const string DateTimeTextboxClassName = "datetime";

        internal static Dictionary<string, object> QpHtmlProperties<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, EditorType type, int index = -1)
        {
            var data = source.GetMetaData(expression);
            var name = ModelExpressionProvider(source).GetExpressionText(expression);
            var maxlength = GetMaxLength(data.ContainerType, data.PropertyName);
            return source.QpHtmlProperties(name, maxlength, type, index);
        }

        internal static ModelExpressionProvider ModelExpressionProvider<TModel>(this IHtmlHelper<TModel> html)
        {
            var expressionProvider = html.ViewContext.HttpContext.RequestServices
                .GetService(typeof(ModelExpressionProvider)) as ModelExpressionProvider;
            return expressionProvider;
        }

        internal static Dictionary<string, object> QpHtmlProperties(this IHtmlHelper source, string name, int maxlength, EditorType type, int index = -1)
        {
            var htmlAttributes = new Dictionary<string, object> { { "id", source.UniqueId(name, index) } };
            switch (type)
            {
                case EditorType.Checkbox:
                    htmlAttributes.Add("class", CheckboxClassName);
                    htmlAttributes.AddCssClass(SimpleCheckboxClassName);
                    break;
                case EditorType.Select:
                    htmlAttributes.Add("class", DropDownListClassName);
                    break;
                case EditorType.ListBox:
                    htmlAttributes.Add("class", ListboxClassName);
                    break;
                case EditorType.TextArea:
                    htmlAttributes.Add("class", TextboxClassName);
                    htmlAttributes.Add("rows", 5);
                    break;
                case EditorType.VisualEditor:
                    htmlAttributes.Add("class", VisualEditorClassName);
                    break;
                case EditorType.Textbox:
                case EditorType.Password:
                    htmlAttributes.Add("class", TextboxClassName);
                    break;
                case EditorType.Numeric:
                    htmlAttributes.Add("class", NumericTextboxClassName);
                    break;
                case EditorType.File:
                    htmlAttributes.Add("class", TextboxClassName);
                    break;
            }

            if (maxlength != 0)
            {
                htmlAttributes.Add("maxlength", maxlength);
            }

            if (source.IsReadOnly())
            {
                htmlAttributes = AddReadOnlyToHtmlAttributes(type, htmlAttributes);
            }

            return htmlAttributes;
        }

        internal static Dictionary<string, object> QpHtmlProperties(this IHtmlHelper source, string id, Field field, int index, bool isReadOnly, string contentFieldName = null)
        {
            var htmlAttributes = new Dictionary<string, object> { { "id", source.UniqueId(id, index) } };
            htmlAttributes.AddData("exact_type", field.ExactType.ToString());
            if (contentFieldName != null)
            {
                htmlAttributes.AddData("content_field_name", contentFieldName);
            }

            switch (field.Type.Name)
            {
                case FieldTypeName.Boolean:
                    htmlAttributes.Add("class", CheckboxClassName);
                    htmlAttributes.AddCssClass(SimpleCheckboxClassName);
                    break;
                case FieldTypeName.Numeric:
                    htmlAttributes.Add("class", NumericTextboxClassName);
                    break;
                default:
                    if (field.ExactType == FieldExactTypes.Textbox)
                    {
                        htmlAttributes.Add("class", ArticleTextboxClassName + HighlightModeSelectHelper.SelectHighlightType(field.HighlightType));
                        htmlAttributes.Add("rows", field.TextBoxRows >= 255 ? 5 : field.TextBoxRows);
                    }
                    else if (field.ExactType == FieldExactTypes.VisualEdit)
                    {
                        htmlAttributes.Add("class", ArticleVisualEditorTextboxClassName);
                        htmlAttributes.Add("style", $"height: {field.VisualEditorHeight}px");
                    }
                    else if (!(field.RelationType == RelationType.OneToMany || field.RelationType == RelationType.ManyToMany || field.RelationType == RelationType.ManyToOne))
                    {
                        if (field.Type.Name == FieldTypeName.String)
                        {
                            htmlAttributes.Add("maxlength", field.StringSize);
                            htmlAttributes.Add("class", ArticleTextboxClassName);
                        }
                        else
                        {
                            htmlAttributes.Add("class", TextboxClassName);
                        }
                    }

                    break;
            }

            if (isReadOnly && (field.Type.Name != FieldTypeName.Relation || field.Type.Name != FieldTypeName.M2ORelation))
            {
                htmlAttributes = AddReadOnlyToHtmlAttributes(field, htmlAttributes);
            }

            return htmlAttributes;
        }

        private static int GetMaxLength(Type sourceType, string propertyName)
        {
            var attr = GetCustomAttribute(sourceType, propertyName, typeof(MaxLengthAttribute));
            return ((MaxLengthAttribute)attr)?.Length ?? 0;
        }

        internal static string GetExampleText(Type sourceType, string propertyName)
        {
            var attr = GetCustomAttribute(sourceType, propertyName, typeof(ExampleAttribute));
            return ((ExampleAttribute)attr)?.Text;
        }

        private static object GetCustomAttribute(Type sourceType, string propertyName, Type type)
        {
            var result = sourceType.GetProperty(propertyName);
            var attrs = result?.GetCustomAttributes(type, true);
            return (attrs ?? throw new InvalidOperationException()).Any() ? attrs[0] : null;
        }

        public static IHtmlContent QpLabel(this IHtmlHelper html, string id, string title, bool withColon = true, string tooltip = null)
        {
            var label = new TagBuilder("label");
            label.MergeAttribute("for", id);
            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                label.MergeAttribute("title", tooltip);
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                label.InnerHtml.AppendHtml(title);
                if (withColon)
                {
                    label.InnerHtml.AppendHtml(":");
                }
            }
            return label;
        }

        public static IHtmlContent FileUpload(this IHtmlHelper source, string id)
        {
            var tb = new TagBuilder("div");
            tb.MergeAttribute("id", source.UniqueId(id + "_upload"));

            tb.AddCssClass("l-pl-uploader-container");
            tb.MergeAttribute("style", "display:inline-block;");

            var pbTb = new TagBuilder("div");
            pbTb.AddCssClass("lop-pbar-container");
            pbTb.MergeAttribute("style", "height: 18px;");
            pbTb.InnerHtml.AppendHtml("<div class=\"lop-pbar\"></div>");

            tb.InnerHtml.AppendHtml($"<div id={source.UniqueId("uploadBtn_") + id} class=\"t-button pl_upload_button\"><span>{LibraryStrings.Upload}</span></div>");
            tb.InnerHtml.AppendHtml(pbTb);

            return tb;
        }

        private static IHtmlContent ImgButton(string id, string title, string cssClassName)
        {
            var img = new TagBuilder("img");
            img.MergeAttribute("src", PathUtility.Combine(SitePathHelper.GetCommonRootImageFolderUrl(), "0.gif"));

            var div = new TagBuilder("div");
            div.MergeAttribute("id", id);
            div.MergeAttribute("title", title);
            div.AddCssClass(cssClassName);
            div.InnerHtml.AppendHtml(img);

            return div;
        }

        internal static IHtmlContent FileDownload(this IHtmlHelper source, string id) => ImgButton(source.UniqueId(id + "_download"), GlobalStrings.ViewDownload, DownloadButtonClassName);

        internal static IHtmlContent ImagePreview(this IHtmlHelper source, string id) => ImgButton(source.UniqueId(id + "_preview"), GlobalStrings.Preview, PreviewButtonClassName);

        public static IHtmlContent ImageLibrary(this IHtmlHelper source, string id) => ImgButton(source.UniqueId(id + "_library"), GlobalStrings.Library, LibraryButtonClassName);

        private static string DateTimePart(object value, string formatString, DateTime? defaultValue)
        {
            if (value != null && System.DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt.ToString(formatString);
            }

            return defaultValue?.ToString(formatString) ?? string.Empty;
        }

        public static IHtmlContent Div(this IHtmlHelper source, string id, string value, Dictionary<string, object> htmlAttributes)
        {
            var tb = new TagBuilder("div");
            tb.MergeAttribute("id", source.UniqueId(id));
            tb.MergeAttributes(htmlAttributes);
            tb.InnerHtml.AppendHtml(value);

            return tb;
        }

        public static IHtmlContent VersionText(this IHtmlHelper source, string id, string value)
        {
            var properties = new Dictionary<string, object> { { "class", VersionTextClassName }, { "name", id } };
            if (string.IsNullOrEmpty(value))
            {
                value = "&nbsp;";
            }

            return source.Div(id, value, properties);
        }

        public static IHtmlContent VersionArea(this IHtmlHelper source, string id, string value)
        {
            var properties = new Dictionary<string, object> { { "class", VersionAreaClassName }, { "name", id }, { "style", "overflow : auto" } };
            return source.Div(id, value, properties);
        }

        public static IHtmlContent Span(this IHtmlHelper source, string id, string value)
        {
            var tb = new TagBuilder("span");
            tb.MergeAttribute("id", id);
            tb.InnerHtml.AppendHtml(value);

            return tb;
        }

        public static IHtmlContent NumericTextBox(this IHtmlHelper source, string name, object value, Dictionary<string, object> htmlAttributes, int decimalDigits = 0, double? minValue = null, double? maxValue = null)
        {
            double? doubleValue = Converter.ToNullableDouble(value ?? (source.ViewData.Any() ? source.ViewData.Eval(name) : null));

            string inputId = htmlAttributes["id"].ToString();
            string inputClass = htmlAttributes["class"].ToString();

            var widget = new TagBuilder("div");
            widget.AddCssClass("t-widget t-numerictextbox");

            var input = new TagBuilder("input");
            input.AddCssClass("t-input");
            input.AddCssClass(inputClass);
            input.MergeAttribute("id", inputId);
            input.MergeAttribute("name", name);
            input.MergeAttribute("type", "text");
            input.MergeAttribute("value", doubleValue?.ToString());

            if (ContainsReadOnly(htmlAttributes))
            {
                input.MergeAttribute("disabled", "disabled");
            }
            if (htmlAttributes.ContainsKey(DataContentFieldName))
            {
                input.MergeAttribute(DataContentFieldName, htmlAttributes[DataContentFieldName].ToString());
            }

            var increment = new TagBuilder("a");
            increment.AddCssClass("t-link t-icon t-arrow-up");
            increment.MergeAttribute("href", "#");
            increment.MergeAttribute("tabindex", "-1");
            increment.MergeAttribute("title", GlobalStrings.IncreaseValue);
            increment.InnerHtml.AppendHtml("Increment");

            var decrement = new TagBuilder("a");
            decrement.AddCssClass("t-link t-icon t-arrow-down");
            decrement.MergeAttribute("href", "#");
            decrement.MergeAttribute("tabindex", "-1");
            decrement.MergeAttribute("title", GlobalStrings.DecreaseValue);
            decrement.InnerHtml.AppendHtml("Decrement");

            widget.InnerHtml.AppendHtml(input);
            widget.InnerHtml.AppendHtml(increment);
            widget.InnerHtml.AppendHtml(decrement);

            var script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml.AppendHtml($@"
              $('#{inputId}').tTextBox({{
                val: '{doubleValue?.ToString() ?? "null"}',
                separator: '{CultureInfo.CurrentCulture.NumberFormat.CurrencyDecimalSeparator}',
                groupSeparator: '{CultureInfo.CurrentCulture.NumberFormat.CurrencyGroupSeparator}',
                step: 1,
                minValue: {minValue?.ToString() ?? "null"},
                maxValue: {maxValue?.ToString() ?? "null"},
                digits: {decimalDigits},
                groupSize: 3,
                negative: 1,
                type: 'numeric',
              }});");

            var html = new HtmlContentBuilder();
            html.AppendHtml(widget);
            html.AppendHtml(script);
            return html;
        }

        public static IHtmlContent Relation(this IHtmlHelper source, string id, IEnumerable<QPSelectListItem> list, ControlOptions options, RelationType relationType, bool isListOverflow, EntityDataListArgs entityDataListArgs)
        {
            if (relationType == RelationType.OneToMany)
            {
                return !isListOverflow ? source.QpDropDownList(id, list, GlobalStrings.NotSelected, options, entityDataListArgs) : source.QpSingleItemPicker(id, list.ToList(), options, entityDataListArgs);
            }

            return !isListOverflow ? source.QpCheckBoxList(id, list, options, entityDataListArgs) : source.QpMultipleItemPicker(id, list.ToList(), options, entityDataListArgs);
        }

        public static IHtmlContent SingleItemPickerFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, QPSelectListItem selected, EntityDataListArgs entityDataListArgs, ControlOptions options)
        {
            var name = source.ModelExpressionProvider().GetExpressionText(expression);
            IEnumerable<QPSelectListItem> list = null;
            if (selected != null)
            {
                list = new[] { selected };
            }

            return source.QpSingleItemPicker(name, list, options, entityDataListArgs);
        }

        public static IHtmlContent QpCheckBox(this IHtmlHelper source, string name, object value, bool isChecked, Dictionary<string, object> htmlAttributes)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name");
            }

            if (value == null)
            {
                value = "true";
            }

            var box = new TagBuilder("input");
            box.MergeAttribute("type", "checkbox");
            box.MergeAttribute("name", name);
            box.MergeAttribute("id", name);
            box.MergeAttribute("value", value.ToString());
            if (isChecked)
            {
                box.MergeAttribute("checked", "checked");
            }
            box.MergeAttributes(htmlAttributes);

            var hidden = new TagBuilder("input");
            hidden.MergeAttribute("type", "hidden");
            hidden.MergeAttribute("name", name);
            hidden.MergeAttribute("value", "false");

            var html = new HtmlContentBuilder();
            html.AppendHtml(box);
            html.AppendHtml(hidden);
            return html;
        }

        public static IHtmlContent Warning(this IHtmlHelper source, params IHtmlContent[] messages)
        {
            var url = new UrlHelper(source.ViewContext);

            var wrapper = new TagBuilder("div");
            wrapper.AddCssClass("warning");

            var img = new TagBuilder("img");
            img.AddCssClass("w-item");
            img.MergeAttribute("src", url.Content("~/Static/QP8/exclamation.png"));

            wrapper.InnerHtml.AppendHtml(img);

            foreach (var message in messages)
            {
                var text = new TagBuilder("span");
                text.AddCssClass("w-item");
                text.InnerHtml.AppendHtml(message);

                wrapper.InnerHtml.AppendHtml(text);
            }

            return wrapper;
        }

        /// <summary>
        /// Генерирует код раскрывающегося списка
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя раскрывающегося списка</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="optionLabel">текст пустого элемента</param>
        /// <param name="options">дополнительные настройки раскрывающегося списка</param>
        /// <returns>код раскрывающегося списка</returns>
        public static IHtmlContent QpDropDownList(this IHtmlHelper source, string name, IEnumerable<QPSelectListItem> list, string optionLabel, ControlOptions options) => source.QpDropDownList(name, list, optionLabel, options, new EntityDataListArgs());

        /// <summary>
        /// Генерирует код раскрывающегося списка
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя раскрывающегося списка</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="optionLabel">текст пустого элемента</param>
        /// <param name="options">дополнительные настройки раскрывающегося списка</param>
        /// <param name="entityDataListArgs">свойства списка сущностей</param>
        /// <returns>код раскрывающегося списка</returns>
        public static IHtmlContent QpDropDownList(this IHtmlHelper source, string name, IEnumerable<QPSelectListItem> list, string optionLabel, ControlOptions options, EntityDataListArgs entityDataListArgs)
        {
            var qpSelectListItems = list.ToList();
            options.SetDropDownOptions(name, source.UniqueId(name), qpSelectListItems, entityDataListArgs);
            return entityDataListArgs.ShowIds
                ? source.DropDownList(name, qpSelectListItems.Select(n => n.CopyWithIdInText()), optionLabel, options.HtmlAttributes)
                : source.DropDownList(name, qpSelectListItems, optionLabel, options.HtmlAttributes);
        }

        /// <summary>
        /// Генерирует код списка радио-кнопок
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя списка радио-кнопок</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="options">дополнительные настройки списка радио-кнопок</param>
        /// <returns>код списка радио-кнопок</returns>
        public static IHtmlContent QpRadioButtonList(this IHtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options) =>
            source.QpRadioButtonList(name, list, options, null);

        /// <summary>
        /// Генерирует код списка радио-кнопок
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя списка радио-кнопок</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="repeatDirection">направление списка</param>
        /// <param name="options">дополнительные настройки списка радио-кнопок</param>
        /// <returns>код списка радио-кнопок</returns>
        public static IHtmlContent QpRadioButtonList(this IHtmlHelper source, string name, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection, ControlOptions options) =>
            source.QpRadioButtonList(name, list, repeatDirection, options, null);

        /// <summary>
        /// Генерирует код списка радио-кнопок
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя списка радио-кнопок</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="options">дополнительные настройки списка радио-кнопок</param>
        /// <param name="entityDataListArgs">свойства списка сущностей</param>
        /// <returns>код списка радио-кнопок</returns>
        public static IHtmlContent QpRadioButtonList(this IHtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs) =>
            source.QpRadioButtonList(name, list, RepeatDirection.Horizontal, options, entityDataListArgs);

        /// <summary>
        /// Генерирует код списка радио-кнопок
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя списка радио-кнопок</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="repeatDirection">направление списка</param>
        /// <param name="options">дополнительные настройки списка радио-кнопок</param>
        /// <param name="entityDataListArgs">свойства списка сущностей</param>
        /// <returns>код списка радио-кнопок</returns>
        public static IHtmlContent QpRadioButtonList(this IHtmlHelper source, string name, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection, ControlOptions options, EntityDataListArgs entityDataListArgs)
        {
            var div = new TagBuilder("div");
            var qpSelectListItems = list.ToList();
            var contentFieldName = (string)options.HtmlAttributes?.GetAndRemove(DataContentFieldName);
            options.SetRadioButtonListOptions(name, source.UniqueId(name), qpSelectListItems, repeatDirection, entityDataListArgs);
            div.MergeAttributes(options.HtmlAttributes);

            var ul = new TagBuilder("ul");

            for (int itemIndex = 0; itemIndex < qpSelectListItems.Count; itemIndex++)
            {
                var item = qpSelectListItems[itemIndex];

                var itemId = source.UniqueId(name, itemIndex);

                var radioButtonHtmlAttributes = new Dictionary<string, object> { { "id", itemId } };

                if (!options.Enabled)
                {
                    radioButtonHtmlAttributes.Add("disabled", "disabled");
                }

                if (contentFieldName != null)
                {
                    radioButtonHtmlAttributes.Add(DataContentFieldName, contentFieldName);
                }

                var li = new TagBuilder("li");
                li.InnerHtml.AppendHtml(source.RadioButton(name, item.Value, item.Selected, radioButtonHtmlAttributes));
                li.InnerHtml.AppendHtml(" ");
                li.InnerHtml.AppendHtml(source.QpLabel(itemId, item.Text, false));

                ul.InnerHtml.AppendHtml(li);
            }

            div.InnerHtml.AppendHtml(ul);

            return div;
        }

        /// <summary>
        /// Генерирует код списка чекбоксов
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя списка чекбоксов</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="repeatDirection">направление списка</param>
        /// <param name="options">дополнительные настройки списка чекбоксов</param>
        /// <param name="entityDataListArgs">свойства списка сущностей</param>
        /// <param name="asArray">asArray</param>
        /// <returns>код списка чекбоксов</returns>
        public static IHtmlContent QpCheckBoxList(this IHtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs, RepeatDirection repeatDirection = RepeatDirection.Vertical, bool asArray = false)
        {
            var qpSelectListItems = list.ToList();
            var div = new TagBuilder("div");
            options.SetCheckBoxListOptions(name, source.UniqueId(name), qpSelectListItems, repeatDirection, entityDataListArgs);

            var contentFieldName = (string)options.HtmlAttributes?.GetAndRemove(DataContentFieldName);

            div.MergeAttributes(options.HtmlAttributes);

            var ul = new TagBuilder("ul");

            for (int itemIndex = 0; itemIndex < qpSelectListItems.Count; itemIndex++)
            {
                var item = qpSelectListItems[itemIndex];

                var htmlAttributes = source.QpHtmlProperties(name, 0, EditorType.Checkbox, itemIndex);
                if (!options.Enabled)
                {
                    AddReadOnlyToHtmlAttributes(EditorType.Checkbox, htmlAttributes);
                }

                if (contentFieldName != null)
                {
                    htmlAttributes.Add(DataContentFieldName, contentFieldName);
                }

                htmlAttributes.RemoveCssClass(SimpleCheckboxClassName);
                htmlAttributes.AddCssClass(CheckboxListItemClassName);
                htmlAttributes.AddCssClass(NoTrackChangeInputClass);

                var li = new TagBuilder("li");
                li.InnerHtml.AppendHtml(source.QpCheckBox(
                    asArray ? string.Concat(name, "[", itemIndex, "]") : name, item.Value, item.Selected, htmlAttributes));
                li.InnerHtml.AppendHtml(" ");
                if (entityDataListArgs != null && entityDataListArgs.ShowIds)
                {
                    li.InnerHtml.AppendHtml(GetIdLink(item.Value));
                }
                                li.InnerHtml.AppendHtml(source.QpLabel(source.UniqueId(name, itemIndex), item.Text, false));

                ul.InnerHtml.AppendHtml(li);
            }

            div.InnerHtml.AppendHtml(ul);

            return div;
        }

        public static IHtmlContent QpSingleItemPicker(this IHtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs, bool ignoreIdSet = false)
        {
            var item = list?.FirstOrDefault(i => i.Selected);
            var itemValue = item?.Value ?? string.Empty;
            var itemText = item?.Text ?? string.Empty;

            string valueId, wrapperId;

            var contentFieldName = (string)options.HtmlAttributes?.GetAndRemove(DataContentFieldName);

            if (options.HtmlAttributes != null && options.HtmlAttributes.ContainsKey("id"))
            {
                wrapperId = options.HtmlAttributes["id"].ToString();
                valueId = wrapperId + "_value";
            }
            else
            {
                valueId = source.UniqueId(name);
                wrapperId = valueId + "_list";
            }

            var wrapper = new TagBuilder("div");
            options.SetSinglePickerOptions(name, wrapperId, entityDataListArgs, ignoreIdSet);
            wrapper.MergeAttributes(options.HtmlAttributes);

            wrapper.InnerHtml.AppendHtml(GetSingleItemDisplayValue(itemText, itemValue, entityDataListArgs != null && entityDataListArgs.ShowIds));

            var hiddenAttributes = new Dictionary<string, object>();

            if (!ignoreIdSet)
            {
                hiddenAttributes.Add("id", valueId);
            }
            else
            {
                hiddenAttributes.Add("data-bind", "value: " + name + "Id" + " , attr :{ id: '" + source.UniqueId(name) + "' + $index(), name: '" + source.UniqueId(name) + "' + $index()}");
            }

            if (contentFieldName != null)
            {
                hiddenAttributes.Add(DataContentFieldName, contentFieldName);
            }

            hiddenAttributes.Add("class", "stateField");

            wrapper.InnerHtml.AppendHtml(source.Hidden(name, itemValue, hiddenAttributes));

            return wrapper;
        }

        private static IHtmlContent GetSingleItemDisplayValue(string text, string value, bool showIds)
        {
            var wrapper = new TagBuilder("span");
            wrapper.AddCssClass("displayField");

            if (showIds)
            {
                wrapper.InnerHtml.AppendHtml(GetIdLink(value));
            }

            var textWrapper = new TagBuilder("span");
            textWrapper.AddCssClass("title");
            textWrapper.InnerHtml.Append(text);

            wrapper.InnerHtml.AppendHtml(textWrapper);

            return wrapper;
        }

        private static IHtmlContent GetIdLink(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return HtmlString.Empty;
            }

            var idLinkBuilder = new TagBuilder("a");
            idLinkBuilder.AddCssClass("js");
            idLinkBuilder.Attributes.Add("href", "javascript:void(0)");
            idLinkBuilder.InnerHtml.Append(value);

            var idBuilder = new TagBuilder("span");
            idBuilder.AddCssClass("idLink");
            idBuilder.InnerHtml.AppendHtml("(");
            idBuilder.InnerHtml.AppendHtml(idLinkBuilder);
            idBuilder.InnerHtml.AppendHtml(")");

            return idBuilder;
        }

        private static IHtmlContent QpMultipleItemPicker(this IHtmlHelper source, string name, IReadOnlyList<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs)
        {
            var wrapper = new TagBuilder("div");
            options.SetMultiplePickerOptions(name, source.UniqueId(name), entityDataListArgs);

            var contentFieldName = (string)options.HtmlAttributes?.GetAndRemove(DataContentFieldName);

            wrapper.MergeAttributes(options.HtmlAttributes);

            if (list != null && list.Count >= QPConfiguration.Options.RelationCountLimit)
            {
                var value = string.Join(",", list.Select(n => n.Value).ToArray());
                wrapper.InnerHtml.AppendHtml(source.Hidden(name, value, new { @class = MultiplePickerOverflowHiddenValue, id = source.UniqueId(name) }));
            }

            var ul = new TagBuilder("ul");
            if (list != null && list.Count < QPConfiguration.Options.RelationCountLimit)
            {
                for (int itemIndex = 0; itemIndex < list.Count; itemIndex++)
                {
                    var item = list[itemIndex];

                    var liAttributes = source.QpHtmlProperties(name, 0, EditorType.Checkbox, itemIndex);
                    if (!options.Enabled)
                    {
                        AddReadOnlyToHtmlAttributes(EditorType.Checkbox, liAttributes);
                    }

                    if (contentFieldName != null)
                    {
                        liAttributes.Add(DataContentFieldName, contentFieldName);
                    }

                    liAttributes.RemoveCssClass(SimpleCheckboxClassName);
                    liAttributes.AddCssClass(MultiplePickerItemCheckboxClassName);
                    liAttributes.AddCssClass(NoTrackChangeInputClass);

                    var li = new TagBuilder("li");
                    li.InnerHtml.AppendHtml(source.QpCheckBox(name, item.Value, item.Selected, liAttributes));
                    li.InnerHtml.AppendHtml(" ");
                    if (entityDataListArgs.ShowIds)
                    {
                        li.InnerHtml.AppendHtml(GetIdLink(item.Value));
                    }
                    li.InnerHtml.AppendHtml(source.QpLabel(source.UniqueId(name, itemIndex), item.Text, false));

                    ul.InnerHtml.AppendHtml(li);
                }
            }

            wrapper.InnerHtml.AppendHtml(ul);

            return wrapper;
        }

        public static IHtmlContent QpTextBox(this IHtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes) => source.TextBox(id, value, htmlAttributes);

        public static IHtmlContent QpTextArea(this IHtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes) => source.TextArea(id, value.ToString(), htmlAttributes);

        public static IHtmlContent VisualEditor(this IHtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, Field field, bool forceReadOnly)
        {
            SetVisualEditorAttributes(htmlAttributes, field, forceReadOnly);
            return VisualEditor(
                source.TextArea(id, value.ToString(), htmlAttributes),
                field != null && field.AutoExpand,
                field != null && !field.AutoExpand && field.AutoLoad
            );
        }

        public static IHtmlContent VisualEditorFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Field field)
        {
            var htmlAttributes = source.QpHtmlProperties(expression, EditorType.VisualEditor);
            SetVisualEditorAttributes(htmlAttributes, field, false);
            return VisualEditor(
                source.TextAreaFor(expression, htmlAttributes),
                field != null && field.AutoExpand,
                field != null && !field.AutoExpand && field.AutoLoad
            );
        }

        // TODO: review generated VisualEditor markup
        private static IHtmlContent VisualEditor(IHtmlContent ve, bool isExpanded, bool useTexteditor)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<div class='visualEditorToolbar'><ul class='linkButtons group'>");

            const string itemTemplate = "<li style='display: {1};' class='{0}'>" +
                "<span class='linkButton actionLink'>"+
                    "<a href='javascript:void(0);'>" +
                        "<span class='icon {0}'><img src='Static/Common/0.gif'></span>" +
                        "<span class='text'>{2}</span>" +
                    "</a>"+
                "</span>" +
            "</li>";

            sb.AppendFormatLine(itemTemplate, "expand", isExpanded || useTexteditor ? "none" : "block", GlobalStrings.ShowVisualEditor);
            sb.AppendFormatLine(itemTemplate, "collapse", "none", GlobalStrings.HideVisualEditor);

            if (useTexteditor)
            {
                sb.AppendFormatLine(itemTemplate, "visualeditor", "block", GlobalStrings.ToVisualEditor);
                sb.AppendFormatLine(itemTemplate, "texteditor", "none", GlobalStrings.ToTextEditor);
            }

            sb.AppendLine("</ul></div>");
            sb.AppendLine("<div class='visualEditorContainer'>");
            ve.WriteToStringBuilder(sb);
            sb.AppendLine("</div>");

            var componentTag = new TagBuilder("div");
            componentTag.AddCssClass(VisualEditorComponentClassName);
            componentTag.InnerHtml.AppendHtml(sb.ToString());
            return componentTag;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void SetVisualEditorAttributes(Dictionary<string, object> htmlAttributes, Field field, bool forceReadOnly)
        {
            htmlAttributes.AddCssClass(VisualEditorClassName);
            htmlAttributes.AddData("field_id", field?.Id ?? 0);
            htmlAttributes.AddData("content_id", field.ContentId);
            htmlAttributes.AddData("use_site_library", Converter.ToJsString(true));
            htmlAttributes.AddData("library_entity_id", field.Content.SiteId.ToString());
            htmlAttributes.AddData("library_parent_entity_id", "0");
            htmlAttributes.AddData("library_url", field.Content.Site.PathInfo.Url);
            htmlAttributes.AddData("is_readonly", forceReadOnly);
            htmlAttributes.AddData("is_expanded", field.AutoExpand);
            htmlAttributes.AddData("is_texteditor", !field.AutoExpand && field.AutoLoad);
            htmlAttributes.Add("rows", 5);
            if (htmlAttributes.ContainsKey("style"))
            {
                htmlAttributes.Remove("style");
            }
        }

        private static bool ContainsReadOnly(IReadOnlyDictionary<string, object> htmlAttributes) => htmlAttributes != null && (htmlAttributes.ContainsKey("readonly") || htmlAttributes.ContainsKey("disabled"));

        public static IHtmlContent DateTime(this IHtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool isNullable = false, bool readOnly = false) => source.DateTimePicker(id, value, htmlAttributes, DateTimePickerMode.DateTime, readOnly);

        public static IHtmlContent Date(this IHtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool isNullable = false, bool readOnly = false) => source.DateTimePicker(id, value, htmlAttributes, DateTimePickerMode.Date, readOnly);

        public static IHtmlContent Time(this IHtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool isNullable = false, bool readOnly = false) => source.DateTimePicker(id, value, htmlAttributes, DateTimePickerMode.Time, readOnly);

        private static IHtmlContent DateTimePicker(this IHtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, int mode, bool isReadOnly)
        {
            DateTime? dateTime = ToDateTime(value ?? (source.ViewData.Any() ? source.ViewData.Eval(id) : null));

            string inputId = htmlAttributes["id"].ToString();

            var widget = new TagBuilder("div");
            widget.AddCssClass("t-widget");

            var wrap = new TagBuilder("div");
            wrap.AddCssClass("t-picker-wrap");
            widget.InnerHtml.AppendHtml(wrap);

            var input = new TagBuilder("input");
            input.AddCssClass("t-input");
            input.MergeAttribute("id", inputId);
            input.MergeAttribute("name", id);
            input.MergeAttribute("type", "text");
            wrap.InnerHtml.AppendHtml(input);

            if (isReadOnly)
            {
                input.MergeAttribute("disabled", "disabled");
            }
            if (htmlAttributes.ContainsKey(DataContentFieldName))
            {
                input.MergeAttribute(DataContentFieldName, htmlAttributes[DataContentFieldName].ToString());
            }

            var select = new TagBuilder("span");
            select.AddCssClass("t-select");
            wrap.InnerHtml.AppendHtml(select);

            var openCalendar = new TagBuilder("span");
            openCalendar.AddCssClass("t-icon t-icon-calendar");
            openCalendar.MergeAttribute("title", "Open the calendar");
            openCalendar.InnerHtml.Append("Open the calendar");

            var openTime = new TagBuilder("span");
            openTime.AddCssClass("t-icon t-icon-clock");
            openTime.MergeAttribute("title", "Open the time view");
            openTime.InnerHtml.Append("Open the time view");

            var script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");

            switch (mode)
            {
                case DateTimePickerMode.DateTime:
                    widget.AddCssClass("t-datetimepicker");
                    input.AddCssClass(DateTimeTextboxClassName);
                    input.MergeAttribute("value", dateTime?.ToString("g"));
                    select.InnerHtml.AppendHtml(openCalendar);
                    select.InnerHtml.AppendHtml(openTime);
                    script.InnerHtml.AppendHtml($@"
                      $('#{inputId}').tDateTimePicker({{
                        format: '{CurrentDateFormat('g')}',
                        minValue: new Date(1899, 11, 31),
                        maxValue: new Date(2100, 0, 1),
                        startTimeValue: {ToJavaScriptDate(System.DateTime.Today)},
                        endTimeValue: {ToJavaScriptDate(System.DateTime.Today)},
                        interval: 30,
                        selectedValue: {ToJavaScriptDate(dateTime)},
                      }});");
                    break;
                case DateTimePickerMode.Date:
                    widget.AddCssClass("t-datepicker");
                    input.AddCssClass(DateTextboxClassName);
                    input.MergeAttribute("value", dateTime?.ToString("d"));
                    select.InnerHtml.AppendHtml(openCalendar);
                    script.InnerHtml.AppendHtml($@"
                      $('#{inputId}').tDatePicker({{
                        format: '{CurrentDateFormat('d')}',
                        minValue: new Date(1899, 11, 31),
                        maxValue: new Date(2100, 0, 1),
                        selectedValue: {ToJavaScriptDate(dateTime?.Date)},
                      }});");
                    break;
                case DateTimePickerMode.Time:
                    widget.AddCssClass("t-timepicker");
                    input.AddCssClass(TimeTextboxClassName);
                    input.MergeAttribute("value", dateTime?.ToString("t"));
                    select.InnerHtml.AppendHtml(openTime);
                    script.InnerHtml.AppendHtml($@"
                      $('#{inputId}').tTimePicker({{
                        format: '{CurrentDateFormat('t')}',
                        minValue: {ToJavaScriptDate(System.DateTime.Today)},
                        maxValue: {ToJavaScriptDate(System.DateTime.Today)},
                        interval: 30,
                        selectedValue: {ToJavaScriptDate(dateTime)},
                      }});");
                    break;
                default:
                    throw new NotSupportedException();
            }

            var html = new HtmlContentBuilder();
            html.AppendHtml(widget);
            html.AppendHtml(script);
            return html;
        }

        private static DateTime? ToDateTime(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime;
            }
            if (value is TimeSpan timeSpan)
            {
                return System.DateTime.Today.Add(timeSpan);
            }
            if (System.DateTime.TryParse(value?.ToString(), out dateTime))
            {
                return dateTime;
            }
            return null;
        }

        private static string ToJavaScriptDate(DateTime? dateTime)
        {
            if (dateTime == null)
            {
                return "null";
            }
            DateTime dt = dateTime.Value;
            return $"new Date({dt.Year}, {dt.Month - 1}, {dt.Day}, {dt.Hour}, {dt.Minute}, {dt.Second})";
        }

        private static string CurrentDateFormat(char format)
        {
            return CultureInfo.CurrentCulture.DateTimeFormat.GetAllDateTimePatterns(format).First();
        }

        public static IHtmlContent File(this IHtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, Field field, int? entityId, ArticleVersion version, bool? isReadOnly = null, bool? allowUpload = null, bool allowPreview = true, bool allowDownload = true)
        {
            var readOnly = isReadOnly ?? field.ExactType == FieldExactTypes.DynamicImage || ContainsReadOnly(htmlAttributes);
            var shouldAllowUpload = allowUpload ?? !readOnly;
            var allowLibrary = !readOnly && version == null;

            string fieldId = null;
            if (htmlAttributes.ContainsKey("id"))
            {
                fieldId = htmlAttributes["id"].ToString();
            }

            var tb = source.FileWrapper(id, fieldId, field, entityId, version, readOnly, shouldAllowUpload);
            tb.InnerHtml.AppendHtml(source.FileContents(id, value, htmlAttributes, allowLibrary, shouldAllowUpload, allowPreview, allowDownload));

            return tb;
        }

        private static IHtmlContent FileContents(this IHtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool allowLibrary, bool allowUpload, bool allowPreview, bool allowDownload)
        {
            var html = new HtmlContentBuilder();
            html.AppendHtml(source.QpTextBox(id, value, htmlAttributes));
            if (allowPreview)
            {
                html.AppendHtml(source.ImagePreview(id));
            }
            if (allowDownload)
            {
                html.AppendHtml(source.FileDownload(id));
            }
            if (allowLibrary)
            {
                html.AppendHtml(source.ImageLibrary(id));
            }
            if (allowUpload)
            {
                html.AppendHtml(source.FileUpload(id));
            }
            return html;
        }

        internal static TagBuilder FileWrapper(this IHtmlHelper source, string id, string fieldId, Field field, int? entityId, ArticleVersion version, bool readOnly, bool allowUpload)
        {
            var isVersion = version != null;
            var tb = new TagBuilder("div");
            tb.MergeAttribute("id", source.UniqueId(id + "_wrapper"));
            tb.AddCssClass(FieldWrapperClassName);
            tb.AddCssClass(FileFieldClassName);
            tb.AddCssClass(SelfClearFloatsClassName);

            tb.MergeDataAttribute("field_name", id);
            tb.MergeDataAttribute("field_id", string.IsNullOrWhiteSpace(fieldId) ? source.UniqueId(id) : fieldId);
            tb.MergeDataAttribute("entity_id", entityId != null ? entityId.ToString() : null);
            tb.MergeDataAttribute("is_version", Converter.ToJsString(isVersion));

            if (!readOnly)
            {
                var useSiteLibrary = !isVersion && field.UseSiteLibrary;
                var repository = FolderFactory.Create(useSiteLibrary ? EntityTypeCode.SiteFolder : EntityTypeCode.ContentFolder).CreateRepository();
                var folder = repository.GetRoot(useSiteLibrary ? field.Content.SiteId : field.Content.Id);
                var subFolder = isVersion ? string.Empty : field.SubFolder;
                var libraryEntityId = isVersion ? string.Empty : field.LibraryEntityId.ToString();
                var libraryParentEntityId = isVersion ? string.Empty : field.LibraryParentEntityId.ToString();
                var libraryPath = isVersion ? version.PathInfo.Path : field.PathInfo.Path;
                var libraryUrl = isVersion ? version.PathInfo.Url : field.PathInfo.Url;
                var renameMatched = !isVersion && field.RenameMatched;
                tb.MergeDataAttribute("use_site_library", Converter.ToJsString(useSiteLibrary));
                tb.MergeDataAttribute("subfolder", subFolder);
                tb.MergeDataAttribute("library_entity_id", libraryEntityId);
                tb.MergeDataAttribute("library_parent_entity_id", libraryParentEntityId);
                tb.MergeDataAttribute("library_path", libraryPath);
                tb.MergeDataAttribute("library_url", libraryUrl);
                tb.MergeDataAttribute("rename_matched", Converter.ToJsString(renameMatched));
                tb.MergeDataAttribute("is_image", Converter.ToJsString(field.ExactType == FieldExactTypes.Image));
                tb.MergeDataAttribute("allow_file_upload", Converter.ToJsString(allowUpload));
                tb.MergeDataAttribute("folder_Id", folder?.Id.ToString() ?? string.Empty);
            }

            return tb;
        }

        public static IHtmlContent UnlockLink(this IHtmlHelper source, LockableEntityViewModel model) => source.BackendActionLink(model.UnlockId, model.UnlockText, model.LockableData.Id, model.LockableData.Name, model.ParentEntityId, ActionTypeCode.ChangeLock, model.CaptureLockActionCode);

        public static IHtmlContent SelectAllLink(this IHtmlHelper source, ListViewModel model) => source.BackendActionLink(model.SelectAllId, GlobalStrings.SelectAll, 0, string.Empty, model.ParentEntityId, ActionTypeCode.SelectAll, string.Empty, ActionTargetType.NewTab, true);

        public static IHtmlContent UnselectLink(this IHtmlHelper source, ListViewModel model) => source.BackendActionLink(model.UnselectId, GlobalStrings.CancelSelection, 0, string.Empty, model.ParentEntityId, ActionTypeCode.DeselectAll, string.Empty, ActionTargetType.NewTab, true);

        public static IHtmlContent ParentPermissionLink(this IHtmlHelper source, ChildEntityPermissionListViewModel model) => source.BackendActionLink(model.UniqueId("chlpActionLink"), EntityPermissionStrings.ParentEntityPermission, 0, string.Empty, model.ParentEntityId, ActionTypeCode.List, model.ParentPermissionsListAction, ActionTargetType.NewTab, true);

        public static IHtmlContent AddNewItemLink(this IHtmlHelper source, ListViewModel model) => source.BackendActionLink(model.AddNewItemLinkId, model.AddNewItemText, 0, string.Empty, model.ParentEntityId, ActionTypeCode.AddNew, model.AddNewItemActionCode, ActionTargetType.NewTab, true);

        public static IHtmlContent SimpleAddActionLink(this IHtmlHelper source, string text) => new HtmlString(@"<span class=""linkButton actionLink""><a href=""javascript:void(0);""><span class=""icon add""> " + $@"<img src=""Static/Common/0.gif""></span><span class=""text"">{text}</span></a></span>");

        public static IHtmlContent AggregationListFor<TValue>(this IHtmlHelper source, string name, IEnumerable<TValue> list, string bindings, Dictionary<string, string> additionalData = null)
        {
            var div = new TagBuilder("div");
            div.MergeAttribute("id", source.UniqueId(name + "_aggregationlist"));
            div.AddCssClass(AggregationListClassName);
            div.InnerHtml.AppendHtml($@"<div class =""{AggregationListContainerClassName}""></div>" + $@"<input type=""hidden"" name=""AggregationListItems{name.Replace(".", string.Empty)}"" class=""{AggregationListResultClassName}""/>");
            div.MergeDataAttribute("aggregation_list_data", JsonConvert.SerializeObject(list));
            div.MergeDataAttribute("aggregation_list_item_fields", bindings);
            if (additionalData != null)
            {
                div.MergeDataAttribute("additional_names", string.Join(",", additionalData.Select(x => x.Key).ToArray()));
                foreach (var item in additionalData)
                {
                    div.MergeDataAttribute("additional_" + item.Key, item.Value);
                }
            }

            div.MergeDataAttribute("field_name", name);
            return div;
        }

        public static IHtmlContent VersionTextFor<TValue>(this IHtmlHelper source, string name, TValue text) => source.VersionText(source.UniqueId(name + "_versionText"), text.ToString());

        public static IHtmlContent VersionAreaFor<TValue>(this IHtmlHelper source, string name, TValue text) => source.VersionArea(source.UniqueId(name + "_versionText"), text.ToString());

        public static IHtmlContent WorkflowFor<TValue>(this IHtmlHelper source, string name, IEnumerable<TValue> list)
        {
            var div = new TagBuilder("div");
            div.MergeAttribute("id", source.UniqueId(name + "_workflow_control"));
            div.AddCssClass(WorkflowControlClassName);
            div.MergeDataAttribute("workflow_list_data", JsonConvert.SerializeObject(list));
            div.InnerHtml.AppendHtml($@"<div class =""{WorkflowContainerClassName}""></div>");
            return div;
        }

        public static IHtmlContent BackendActionLink(this IHtmlHelper source, string id, string text, int entityId, string entityName, int parentEntityId, string actionTypeCode, string actionCode, ActionTargetType actionTargetType = ActionTargetType.NewTab, bool returnListElement = false)
        {
            var span = new TagBuilder("span");
            span.MergeAttribute("id", id);
            span.AddCssClass(LinkButtonClassName);
            span.AddCssClass(ActionLinkClassName);
            span.MergeDataAttribute("entity_id", entityId.ToString());
            span.MergeDataAttribute("entity_name", entityName);
            span.MergeDataAttribute("parent_entity_id", parentEntityId.ToString());
            span.MergeDataAttribute("action_type_code", actionTypeCode);
            span.MergeDataAttribute("action_code", actionCode);
            span.MergeDataAttribute("action_target_type", ((int)actionTargetType).ToString());

            var iconCssClassName = string.Empty;
            if (actionTypeCode == ActionTypeCode.AddNew)
            {
                iconCssClassName = "add";
            }
            else if (actionTypeCode == ActionTypeCode.ChangeLock)
            {
                iconCssClassName = "unlock";
            }
            else if (actionTypeCode == ActionTypeCode.DeselectAll)
            {
                iconCssClassName = "deselectAll";
            }
            else if (actionTypeCode == ActionTypeCode.SelectAll)
            {
                iconCssClassName = "selectAll";
            }
            else if (actionCode == ActionCode.SitePermissions || actionCode == ActionCode.ContentPermissions)
            {
                iconCssClassName = "parentPermissions";
            }

            var sb = new StringBuilder();
            sb.AppendFormat(@"<a href=""javascript:void(0);"">");
            sb.Append("<span");

            if (!string.IsNullOrWhiteSpace(iconCssClassName))
            {
                sb.AppendFormat(@" class=""icon {0}""", iconCssClassName);
            }

            sb.Append(">");
            sb.AppendFormat(@"<img src=""{0}"" />", PathUtility.Combine(SitePathHelper.GetCommonRootImageFolderUrl(), "0.gif"));
            sb.Append("</span>");
            sb.AppendFormat(@"<span class=""text"">{0}</span>", text);
            sb.Append("</a>");

            span.InnerHtml.AppendHtml(sb.ToString());

            if (returnListElement)
            {
                var li = new TagBuilder("li");
                li.AddCssClass("doctab-title__element");
                li.InnerHtml.AppendHtml(span);
                return li;
            }
            return span;
        }

        public static IHtmlContent QpTextBox<TModel>(this IHtmlHelper<TModel> source, string fieldName, string fieldValue, Dictionary<string, object> htmlAttributes = null)
        {
            var htmlProperties = source.QpHtmlProperties(fieldName, 0, EditorType.Textbox);
            htmlProperties.Merge(htmlAttributes, true);
            return source.TextBox(fieldName, fieldValue, htmlProperties);
        }

        public static IHtmlContent QpTextBoxFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            var htmlProperties = source.QpHtmlProperties(expression, EditorType.Textbox);
            htmlProperties.Merge(htmlAttributes, true);
            return source.TextBoxFor(expression, htmlProperties);
        }

        public static IHtmlContent QpTextAreaFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            var htmlProperties = source.QpHtmlProperties(expression, EditorType.TextArea).Merge(htmlAttributes, true);
            return source.TextAreaFor(expression, htmlProperties);
        }

        public static IHtmlContent DateTimeFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression)
        {
            var data = source.GetModelExpression(expression);
            return source.DateTime(ModelExpressionProvider(source).GetExpressionText(expression), data.Model, source.QpHtmlProperties(expression, EditorType.Textbox));
        }

        public static IHtmlContent DateFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression)
        {
            var data = source.GetModelExpression(expression);
            return source.Date(ModelExpressionProvider(source).GetExpressionText(expression), data.Model, source.QpHtmlProperties(expression, EditorType.Textbox));
        }

        public static IHtmlContent TimeFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression)
        {
            var data = source.GetModelExpression(expression);
            return source.Time(ModelExpressionProvider(source).GetExpressionText(expression), data.Model, source.QpHtmlProperties(expression, EditorType.Textbox));
        }

        public static IHtmlContent NumericFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, int decimalDigits = 0, double? minValue = null, double? maxValue = null, Dictionary<string, object> htmlAttributes = null)
        {
            var data = source.GetModelExpression(expression);
            return source.NumericTextBox(ModelExpressionProvider(source).GetExpressionText(expression), data.Model, source.QpHtmlProperties(expression, EditorType.Numeric).Merge(htmlAttributes, true), decimalDigits, minValue, maxValue);
        }

        public static IHtmlContent FileFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Field field, Dictionary<string, object> htmlAttributes)
        {
            var name = ModelExpressionProvider(source).GetExpressionText(expression);
            var htmlProperties = source.QpHtmlProperties(expression, EditorType.File);
            htmlProperties.Merge(htmlAttributes, true);
            return source.File(name, null, htmlProperties, field, null, null, false, true, false, false);
        }

        public static IHtmlContent QpCheckBoxFor<TModel>(this IHtmlHelper<TModel> source, Expression<Func<TModel, bool>> expression, string toggleId = null, bool reverseToggle = false, Dictionary<string, object> htmlAttributes = null, bool forceReadOnly = false)
        {
            var htmlProperties = source.QpHtmlProperties(expression, EditorType.Checkbox);
            if (!string.IsNullOrWhiteSpace(toggleId))
            {
                var toggleIds = toggleId.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                var uniqueToggleIds = new List<string>();
                foreach (var id in toggleIds)
                {
                    uniqueToggleIds.Add(source.UniqueId(id));
                }
                htmlProperties.AddData("toggle_for", string.Join(",", uniqueToggleIds));
                htmlProperties.AddData("reverse", reverseToggle.ToString().ToLowerInvariant());
            }

            if (forceReadOnly)
            {
                htmlProperties.Add("disabled", "disabled");
            }

            htmlProperties.Merge(htmlAttributes, true);
            return source.CheckBoxFor(expression, htmlProperties);
        }

        public static IHtmlContent QpDropDownListFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, Dictionary<string, object> htmlAttributes, SelectOptions dropDownOptions)
        {
            var qpSelectListItems = list.ToList();
            var options = new ControlOptions { Enabled = !source.IsReadOnly() && !dropDownOptions.ReadOnly };
            var name = ModelExpressionProvider(source).GetExpressionText(expression);
            var showedList = string.IsNullOrEmpty(dropDownOptions?.DefaultOption)
                ? qpSelectListItems.ToList()
                : new[] { new QPSelectListItem { Value = string.Empty, Text = dropDownOptions.DefaultOption } }.Concat(qpSelectListItems).ToList();

            options.SetDropDownOptions(name, source.UniqueId(name), qpSelectListItems, dropDownOptions?.EntityDataListArgs);
            options.HtmlAttributes.Merge(htmlAttributes, true);
            return source.DropDownListFor(expression, showedList, options.HtmlAttributes);
        }

        public static IHtmlContent QpRadioButtonListFor<TModel, TValue>(this IHtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection = RepeatDirection.Horizontal, EntityDataListArgs entityDataListArgs = null, ControlOptions options = null)
        {
            var div = new TagBuilder("div");
            var qpSelectListItems = list.ToList();
            var name = ModelExpressionProvider(source).GetExpressionText(expression);
            var id = source.UniqueId(name);
            var localOptions = options ?? new ControlOptions();

            localOptions.Enabled &= !source.IsReadOnly();
            localOptions.SetRadioButtonListOptions(name, id, qpSelectListItems, repeatDirection, entityDataListArgs);
            div.MergeAttributes(localOptions.HtmlAttributes);

            var ul = new TagBuilder("ul");
            for (int itemIndex = 0; itemIndex < qpSelectListItems.Count; itemIndex++)
            {
                var item = qpSelectListItems[itemIndex];

                var htmlAttributes = source.QpHtmlProperties(expression, EditorType.RadioButton, itemIndex);
                if (!localOptions.Enabled)
                {
                    htmlAttributes = AddReadOnlyToHtmlAttributes(EditorType.RadioButton, htmlAttributes);
                }

                var li = new TagBuilder("li");
                li.InnerHtml.AppendHtml(source.RadioButtonFor(expression, item.Value, htmlAttributes));
                li.InnerHtml.AppendHtml(" ");
                li.InnerHtml.AppendHtml(source.QpLabelFor(expression, item.Text, false, itemIndex));
                ul.InnerHtml.AppendHtml(li);
            }
            div.InnerHtml.AppendHtml(ul);
            return div;
        }

        public static IHtmlContent QpCheckBoxListFor<TModel>(this IHtmlHelper<TModel> source, Expression<Func<TModel, IList<QPCheckedItem>>> expression, IEnumerable<QPSelectListItem> list, EntityDataListArgs entityDataListArgs, Dictionary<string, object> htmlAttributes, RepeatDirection repeatDirection = RepeatDirection.Vertical)
        {
            var qpSelectListItems = list.ToList();
            var name = ModelExpressionProvider(source).GetExpressionText(expression);
            var options = new ControlOptions { Enabled = !source.IsReadOnly() };
            if (htmlAttributes != null)
            {
                options.HtmlAttributes = htmlAttributes;
            }

            foreach (var item in qpSelectListItems)
            {
                item.Selected = false;
            }

            if (source.GetModelExpression(expression).Model is IList<QPCheckedItem> propertyValue && propertyValue.Count > 0)
            {
                var checkedValues = propertyValue.Where(n => n != null).Select(i => i.Value).Intersect(qpSelectListItems.Select(b => b.Value)).ToList();
                foreach (var item in qpSelectListItems)
                {
                    item.Selected = checkedValues.Contains(item.Value);
                }
            }

            return source.QpCheckBoxList(name, qpSelectListItems, options, entityDataListArgs, repeatDirection, true);
        }

        public static IHtmlContent CheckBoxTreeFor<TModel>(
            this IHtmlHelper<TModel> source,
            Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression,
            string entityTypeCode,
            int? parentEntityId,
            string actionCode,
            bool allowGlobalSelection = false,
            Dictionary<string, object> htmlAttributes = null)
        {
            var name = ModelExpressionProvider(source).GetExpressionText(expression);
            var options = new Dictionary<string, object> { { "id", source.UniqueId(name) } };
            options.AddData("entity_type_code", entityTypeCode);
            options.AddData("parent_entity_id", parentEntityId);
            options.AddData("read_action_code", actionCode);
            options.AddData("allow_global_selection", allowGlobalSelection.ToString().ToLowerInvariant());
            options.AddData("tree_name", name);
            options.AddData("show_checkbox", bool.TrueString.ToLowerInvariant());

            if (source.GetModelExpression(expression).Model is IList<QPTreeCheckedNode> propertyValue && propertyValue.Count > 0)
            {
                var selectedIDsString = string.Join(";", propertyValue.Select(i => i.Value));
                options.AddData("selected_ids", selectedIDsString);
            }

            options.Merge(htmlAttributes, true);

            var widget = new TagBuilder("div");
            widget.MergeAttributes(options);
            widget.AddCssClass(CheckBoxTreeClassName);
            widget.AddCssClass("t-widget t-treeview t-reset");

            var script = new TagBuilder("script");
            script.MergeAttribute("type", "text/javascript");
            script.InnerHtml.AppendHtml($"$('#{widget.Attributes["id"]}').tTreeView();");

            var html = new HtmlContentBuilder();
            html.AppendHtml(widget);
            html.AppendHtml(script);
            return html;
        }

        public static IHtmlContent VirtualFieldTreeFor<TModel>(this IHtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression, int? parentEntityId, int virtualContentId, Dictionary<string, object> htmlAttributes = null)
        {
            var options = new Dictionary<string, object>();
            options.AddData("virtual_content_id", virtualContentId);
            options.Merge(htmlAttributes, true);
            return source.CheckBoxTreeFor(expression, EntityTypeCode.Field, parentEntityId, ActionCode.Fields, false, options);
        }

        public static IHtmlContent MultipleItemPickerFor<TModel>(this IHtmlHelper<TModel> source, string name, IEnumerable<QPSelectListItem> selectedItemList, EntityDataListArgs entityDataListArgs, Dictionary<string, object> htmlAttributes = null)
        {
            var options = new ControlOptions { Enabled = !source.IsReadOnly() };
            options.HtmlAttributes.Merge(htmlAttributes, true);
            return source.Relation(name, selectedItemList, options, RelationType.ManyToMany, true, entityDataListArgs);
        }

        public static IHtmlContent MultipleItemPickerFor<TModel>(
            this IHtmlHelper<TModel> source,
            Expression<Func<TModel, IEnumerable<int>>> expression,
            IEnumerable<ListItem> selectedItemList,
            EntityDataListArgs entityDataListArgs,
            Dictionary<string, object> htmlAttributes = null
        ) => source.MultipleItemPickerFor(
            ModelExpressionProvider(source).GetExpressionText(expression),
            selectedItemList.Select(c => new QPSelectListItem { Selected = true, Text = c.Text, Value = c.Value }).ToArray(),
            entityDataListArgs,
            htmlAttributes
        );

        public static IHtmlContent UnionContentsFor<TModel>(this IHtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<int>>> expression, IEnumerable<ListItem> selectedItemList, int siteId, Dictionary<string, object> htmlAttributes = null)
        {
            var entityDataListArgs = new EntityDataListArgs
            {
                EntityTypeCode = EntityTypeCode.Content,
                ParentEntityId = siteId,
                SelectActionCode = ActionCode.MultipleSelectContentForUnion,
                ListId = -1 * System.DateTime.Now.Millisecond,
                MaxListHeight = 200,
                MaxListWidth = 350
            };

            return source.MultipleItemPickerFor(expression, selectedItemList, entityDataListArgs, htmlAttributes);
        }

        public static IEnumerable<QPSelectListItem> List(this IHtmlHelper source, IEnumerable<ListItem> list) => list.Select(n => new QPSelectListItem
        {
            Text = n.Text,
            Value = n.Value,
            HasDependentItems = n.HasDependentItems,
            DependentItemIDs = n.DependentItemIDs?.Select(s => source.UniqueId(s)).ToArray(),
            Selected = n.Selected
        });

        public static string FormatAsTime(this IHtmlHelper source, object value, DateTime? defaultValue = null) => DateTimePart(value, "T", defaultValue);

        public static string FormatAsDate(this IHtmlHelper source, object value, DateTime? defaultValue = null) => DateTimePart(value, "d", defaultValue);

        public static string FormatAsDateTime(this IHtmlHelper source, object value, DateTime? defaultValue = null) => DateTimePart(value, "G", defaultValue);

        private static Dictionary<string, object> AddReadOnlyToHtmlAttributes(EditorType type, Dictionary<string, object> htmlAttributes)
        {
            if (type == EditorType.TextArea ||
                type == EditorType.Textbox ||
                type == EditorType.Password ||
                type == EditorType.VisualEditor)
            {
                return htmlAttributes.Merge(new Dictionary<string, object> { { "readonly", "readonly" } });
            }

            return htmlAttributes.Merge(new Dictionary<string, object> { { "disabled", "disabled" } });
        }

        private static Dictionary<string, object> AddReadOnlyToHtmlAttributes(Field field, Dictionary<string, object> htmlAttributes)
        {
            if (field.ExactType == FieldExactTypes.Textbox ||
                field.ExactType == FieldExactTypes.VisualEdit ||
                field.ExactType == FieldExactTypes.String ||
                field.Type.Name == FieldTypeName.Textbox ||
                field.Type.Name == FieldTypeName.VisualEdit ||
                field.Type.Name == FieldTypeName.String)
            {
                return htmlAttributes.Merge(new Dictionary<string, object> { { "readonly", "readonly" } });
            }

            return htmlAttributes.Merge(new Dictionary<string, object> { { "disabled", "disabled" } });
        }
    }
}
