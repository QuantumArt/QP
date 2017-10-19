using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.ViewModels;
using Quantumart.QP8.WebMvc.ViewModels.EntityPermissions;
using Telerik.Web.Mvc.UI;

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

        internal static Dictionary<string, object> QpHtmlProperties<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, EditorType type, int index = -1)
        {
            var data = source.GetMetaData(expression);
            var name = ExpressionHelper.GetExpressionText(expression);
            var maxlength = GetMaxLength(data.ContainerType, data.PropertyName);
            return source.QpHtmlProperties(name, maxlength, type, index);
        }

        internal static Dictionary<string, object> QpHtmlProperties(this HtmlHelper source, string name, int maxlength, EditorType type, int index = -1)
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

        internal static Dictionary<string, object> QpHtmlProperties(this HtmlHelper source, string id, Field field, int index, bool isReadOnly, string contentFieldName = null)
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
            var attr = GetCustomAttribute(sourceType, propertyName, typeof(MaxLengthValidatorAttribute));
            return ((MaxLengthValidatorAttribute)attr)?.UpperBound ?? 0;
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

        public static MvcHtmlString QpLabel(this HtmlHelper html, string id, string title, bool withColon = true, string tooltip = null)
        {
            var label = new TagBuilder("label");
            label.MergeAttribute("for", id);
            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                label.MergeAttribute("title", tooltip);
            }

            label.InnerHtml = !string.IsNullOrWhiteSpace(title) ? title + (withColon ? ":" : string.Empty) : string.Empty;
            return MvcHtmlString.Create(label.ToString());
        }

        public static string FileUpload(this HtmlHelper source, string id)
        {
            var tb = new TagBuilder("div");
            tb.MergeAttribute("id", source.UniqueId(id + "_upload"));

            var uploaderType = UploaderTypeHelper.UploaderType;
            switch (uploaderType)
            {
                case UploaderType.Silverlight:
                    tb.MergeAttribute("class", BrowseButtonClassName + " l-sl-uploader");
                    break;
                case UploaderType.Html:
                    tb.MergeAttribute("class", "l-html-uploader");
                    tb.InnerHtml = source.Telerik().Upload()
                        .Name(source.UniqueId(id + "mvcUploader"))
                        .Async(async => async.Save("UploadFile", "Library"))
                        .Multiple(false)
                        .ShowFileList(false)
                        .ToHtmlString();
                    break;
                case UploaderType.PlUpload:
                    tb.MergeAttribute("class", "l-pl-uploader-container");
                    tb.MergeAttribute("style", "display:inline-block;");
                    var pbTb = new TagBuilder("div");
                    pbTb.AddCssClass("lop-pbar-container");
                    pbTb.MergeAttribute("style", "height: 18px;");
                    pbTb.InnerHtml = "<div class=\"lop-pbar\"></div>";
                    tb.InnerHtml = $"<div id={source.UniqueId("uploadBtn_") + id} class=\"t-button pl_upload_button\"><span>{LibraryStrings.Upload}</span></div>{pbTb}";
                    break;
            }

            return tb.ToString();
        }

        private static string ImgButton(string id, string title, string cssClassName)
        {
            var img = new TagBuilder("img");
            img.MergeAttribute("src", PathUtility.Combine(SitePathHelper.GetCommonRootImageFolderUrl(), "0.gif"));

            var div = new TagBuilder("div");
            div.MergeAttribute("id", id);
            div.MergeAttribute("title", title);
            div.MergeAttribute("class", cssClassName);
            div.InnerHtml = img.ToString();

            return div.ToString();
        }

        internal static string FileDownload(this HtmlHelper source, string id) => ImgButton(source.UniqueId(id + "_download"), GlobalStrings.ViewDownload, DownloadButtonClassName);

        internal static string ImagePreview(this HtmlHelper source, string id) => ImgButton(source.UniqueId(id + "_preview"), GlobalStrings.Preview, PreviewButtonClassName);

        public static string ImageLibrary(this HtmlHelper source, string id) => ImgButton(source.UniqueId(id + "_library"), GlobalStrings.Library, LibraryButtonClassName);

        private static string DateTimePart(object value, string formatString, DateTime? defaultValue)
        {
            if (value != null && System.DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
            {
                return dt.ToString(formatString);
            }

            return defaultValue?.ToString(formatString) ?? string.Empty;
        }

        public static MvcHtmlString Div(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes)
        {
            var tb = new TagBuilder("div");
            tb.MergeAttribute("id", source.UniqueId(id));
            tb.MergeAttributes(htmlAttributes);
            tb.InnerHtml = value.ToString();

            return MvcHtmlString.Create(tb.ToString());
        }

        public static MvcHtmlString VersionText(this HtmlHelper source, string id, string value)
        {
            var properties = new Dictionary<string, object> { { "class", VersionTextClassName }, { "name", id } };
            if (string.IsNullOrEmpty(value))
            {
                value = "&nbsp;";
            }

            return source.Div(id, value, properties);
        }

        public static MvcHtmlString VersionArea(this HtmlHelper source, string id, string value)
        {
            var properties = new Dictionary<string, object> { { "class", VersionAreaClassName }, { "name", id }, { "style", "overflow : auto" } };
            return source.Div(id, value, properties);
        }

        public static MvcHtmlString Span(this HtmlHelper source, string id, object value)
        {
            var tb = new TagBuilder("span");
            tb.MergeAttribute("id", id);
            tb.InnerHtml = value.ToString();

            return MvcHtmlString.Create(tb.ToString());
        }

        public static MvcHtmlString NumericTextBox(this HtmlHelper source, string name, object value, Dictionary<string, object> htmlAttributes, int decimalDigits = 0, double? minValue = null, double? maxValue = null)
        {
            var newHtmlAttributes = new Dictionary<string, object> { { "id", htmlAttributes["id"] }, { "class", htmlAttributes["class"] } };
            newHtmlAttributes.CopyValueIfExists(htmlAttributes, DataContentFieldName);

            return MvcHtmlString.Create(source.Telerik().NumericTextBox()
                .MinValue(minValue)
                .MaxValue(maxValue)
                .ButtonTitleDown(GlobalStrings.DecreaseValue)
                .ButtonTitleUp(GlobalStrings.IncreaseValue)
                .Name(name)
                .InputHtmlAttributes(new { id = htmlAttributes["id"], @class = htmlAttributes["class"] })
                .Value(Converter.ToNullableDouble(value))
                .DecimalDigits(decimalDigits)
                .Spinners(true)
                .EmptyMessage(string.Empty)
                .Enable(!ContainsReadOnly(htmlAttributes))
                .ToHtmlString());
        }

        public static MvcHtmlString Relation(this HtmlHelper source, string id, IEnumerable<QPSelectListItem> list, ControlOptions options, RelationType relationType, bool isListOverflow, EntityDataListArgs entityDataListArgs)
        {
            if (relationType == RelationType.OneToMany)
            {
                return !isListOverflow ? source.QpDropDownList(id, list, GlobalStrings.NotSelected, options, entityDataListArgs) : source.QpSingleItemPicker(id, list.ToList(), options, entityDataListArgs);
            }

            return !isListOverflow ? source.QpCheckBoxList(id, list, options, entityDataListArgs) : source.QpMultipleItemPicker(id, list.ToList(), options, entityDataListArgs);
        }

        public static MvcHtmlString SingleItemPickerFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, QPSelectListItem selected, EntityDataListArgs entityDataListArgs, ControlOptions options)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            IEnumerable<QPSelectListItem> list = null;
            if (selected != null)
            {
                list = new[] { selected };
            }

            return source.QpSingleItemPicker(name, list, options, entityDataListArgs);
        }

        public static MvcHtmlString QpCheckBox(this HtmlHelper source, string name, object value, bool isChecked, Dictionary<string, object> htmlAttributes)
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

            return MvcHtmlString.Create(box + hidden.ToString());
        }

        public static MvcHtmlString Warning(this HtmlHelper source, string message) => source.Warning(new[] { new MvcHtmlString(message) });

        public static MvcHtmlString Warning(this HtmlHelper source, IList<MvcHtmlString> messages)
        {
            var url = new UrlHelper(source.ViewContext.RequestContext);
            var wrapper = new TagBuilder("div");
            wrapper.AddCssClass("warning");

            var img = new TagBuilder("img");
            img.AddCssClass("w-item");
            img.MergeAttribute("src", url.Content("~/Content/QP8/exclamation.png"));

            var messageSpans = new List<TagBuilder>(messages.Count);
            foreach (var message in messages)
            {
                var text = new TagBuilder("span");
                text.AddCssClass("w-item");
                text.InnerHtml = message.ToHtmlString();
                messageSpans.Add(text);
            }

            wrapper.InnerHtml = img.ToString(TagRenderMode.SelfClosing) + string.Join(string.Empty, messageSpans);
            return MvcHtmlString.Create(wrapper.ToString());
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
        public static MvcHtmlString QpDropDownList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, string optionLabel, ControlOptions options) => source.QpDropDownList(name, list, optionLabel, options, new EntityDataListArgs());

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
        public static MvcHtmlString QpDropDownList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, string optionLabel, ControlOptions options, EntityDataListArgs entityDataListArgs)
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
        public static MvcHtmlString QpRadioButtonList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options) =>
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
        public static MvcHtmlString QpRadioButtonList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection, ControlOptions options) =>
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
        public static MvcHtmlString QpRadioButtonList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs) =>
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
        public static MvcHtmlString QpRadioButtonList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection, ControlOptions options, EntityDataListArgs entityDataListArgs)
        {
            var div = new TagBuilder("div");
            var qpSelectListItems = list.ToList();
            var contentFieldName = (string)options.HtmlAttributes?.GetAndRemove(DataContentFieldName);
            options.SetRadioButtonListOptions(name, source.UniqueId(name), qpSelectListItems, repeatDirection, entityDataListArgs);
            div.MergeAttributes(options.HtmlAttributes);

            var sb = new StringBuilder();
            sb.AppendLine("<ul>");

            var itemIndex = 0;
            foreach (var item in qpSelectListItems)
            {
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

                sb.Append("<li>");
                sb.Append(source.RadioButton(name, item.Value, item.Selected, radioButtonHtmlAttributes));
                sb.Append(" ");
                sb.Append(source.QpLabel(itemId, item.Text, false));
                sb.Append("</li>");
                sb.AppendLine();

                itemIndex++;
            }

            sb.AppendLine("</ul>");
            div.InnerHtml = sb.ToString();
            return MvcHtmlString.Create(div.ToString());
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
        public static MvcHtmlString QpCheckBoxList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs, RepeatDirection repeatDirection = RepeatDirection.Vertical, bool asArray = false)
        {
            var qpSelectListItems = list.ToList();
            var div = new TagBuilder("div");
            options.SetCheckBoxListOptions(name, source.UniqueId(name), qpSelectListItems, repeatDirection, entityDataListArgs);

            var contentFieldName = (string)options.HtmlAttributes?.GetAndRemove(DataContentFieldName);

            div.MergeAttributes(options.HtmlAttributes);

            var sb = new StringBuilder();
            sb.AppendLine("<ul>");

            var itemIndex = 0;
            foreach (var item in qpSelectListItems)
            {
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

                sb.Append("<li>");
                sb.Append(source.QpCheckBox(asArray ? string.Concat(name, "[", itemIndex, "]") : name, item.Value, item.Selected, htmlAttributes));
                sb.Append(" ");
                if (entityDataListArgs != null && entityDataListArgs.ShowIds)
                {
                    sb.Append(GetIdLink(item.Value));
                }

                sb.Append(source.QpLabel(source.UniqueId(name, itemIndex), item.Text, false));
                sb.AppendLine("</li>");
                itemIndex++;
            }

            sb.AppendLine("</ul>");
            div.InnerHtml = sb.ToString();
            return MvcHtmlString.Create(div.ToString());
        }

        public static MvcHtmlString QpSingleItemPicker(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs, bool ignoreIdSet = false)
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

            var htmlBuilder = new StringBuilder();
            htmlBuilder.Append(GetSingleItemDisplayValue(itemText, itemValue, entityDataListArgs != null && entityDataListArgs.ShowIds));
            var htmlAttributes = new Dictionary<string, object>();
            if (!ignoreIdSet)
            {
                htmlAttributes.Add("id", valueId);
            }
            else
            {
                htmlAttributes.Add("data-bind", "value: " + name + "Id" + " , attr :{ id: '" + source.UniqueId(name) + "' + $index(), name: '" + source.UniqueId(name) + "' + $index()}");
            }

            if (contentFieldName != null)
            {
                htmlAttributes.Add(DataContentFieldName, contentFieldName);
            }

            htmlAttributes.Add("class", "stateField");
            htmlBuilder.Append(source.Hidden(name, itemValue, htmlAttributes));
            wrapper.InnerHtml = htmlBuilder.ToString();

            return MvcHtmlString.Create(wrapper.ToString());
        }

        private static string GetSingleItemDisplayValue(string text, string value, bool showIds)
        {
            var wrapper = new TagBuilder("span");
            wrapper.AddCssClass("displayField");

            var htmlBuilder = new StringBuilder();
            if (showIds)
            {
                htmlBuilder.Append(GetIdLink(value));
            }

            var textWrapper = new TagBuilder("span");
            textWrapper.AddCssClass("title");
            textWrapper.SetInnerText(text);
            htmlBuilder.Append(textWrapper);
            wrapper.InnerHtml = htmlBuilder.ToString();

            return wrapper.ToString();
        }

        private static string GetIdLink(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var idLinkBuilder = new TagBuilder("a");
            idLinkBuilder.AddCssClass("js");
            idLinkBuilder.Attributes.Add("href", "javascript:void(0)");
            idLinkBuilder.SetInnerText(value);

            var idBuilder = new TagBuilder("span");
            idBuilder.AddCssClass("idLink");
            idBuilder.InnerHtml = $"({idLinkBuilder})";

            return idBuilder.ToString();
        }

        private static MvcHtmlString QpMultipleItemPicker(this HtmlHelper source, string name, ICollection<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs)
        {
            var wrapper = new TagBuilder("div");
            options.SetMultiplePickerOptions(name, source.UniqueId(name), entityDataListArgs);

            var contentFieldName = (string)options.HtmlAttributes?.GetAndRemove(DataContentFieldName);

            wrapper.MergeAttributes(options.HtmlAttributes);

            var sb = new StringBuilder();
            if (list != null && list.Count >= QPConfiguration.WebConfigSection.RelationCountLimit)
            {
                var value = string.Join(",", list.Select(n => n.Value).ToArray());
                sb.AppendLine(source.Hidden(name, value, new { @class = MultiplePickerOverflowHiddenValue, id = source.UniqueId(name) }).ToString());
            }

            sb.AppendLine("<ul>");
            if (list != null && list.Count < QPConfiguration.WebConfigSection.RelationCountLimit)
            {
                var itemIndex = 0;
                foreach (var item in list)
                {
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
                    htmlAttributes.AddCssClass(MultiplePickerItemCheckboxClassName);
                    htmlAttributes.AddCssClass(NoTrackChangeInputClass);

                    sb.Append("<li>");
                    sb.Append(source.QpCheckBox(name, item.Value, item.Selected, htmlAttributes));
                    sb.Append(" ");
                    if (entityDataListArgs.ShowIds)
                    {
                        sb.Append(GetIdLink(item.Value));
                    }

                    sb.Append(source.QpLabel(source.UniqueId(name, itemIndex), item.Text, false));
                    sb.AppendLine("</li>");
                    itemIndex++;
                }
            }

            sb.AppendLine("</ul>");
            wrapper.InnerHtml = sb.ToString();
            return MvcHtmlString.Create(wrapper.ToString());
        }

        public static MvcHtmlString QpTextBox(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes) => source.TextBox(id, value, htmlAttributes);

        public static MvcHtmlString QpTextArea(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes) => source.TextArea(id, value.ToString(), htmlAttributes);

        public static MvcHtmlString VisualEditor(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, Field field, bool forceReadOnly)
        {
            SetVisualEditorAttributes(htmlAttributes, field, forceReadOnly);
            return VisualEditor(
                source.TextArea(id, value.ToString(), htmlAttributes),
                field != null && field.AutoExpand,
                field != null && !field.AutoExpand && field.AutoLoad
            );
        }

        public static MvcHtmlString VisualEditorFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Field field)
        {
            var htmlAttributes = source.QpHtmlProperties(expression, EditorType.VisualEditor);
            SetVisualEditorAttributes(htmlAttributes, field, false);
            return VisualEditor(
                source.TextAreaFor(expression, htmlAttributes),
                field != null && field.AutoExpand,
                field != null && !field.AutoExpand && field.AutoLoad
            );
        }

        private static MvcHtmlString VisualEditor(MvcHtmlString ve, bool isExpanded, bool useTexteditor)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"<div class='visualEditorToolbar'><ul class='linkButtons group'>");

            const string itemTemplate = @"<li style='display: {1};' class='{0}'>
                    <span class='linkButton actionLink'>
                        <a href='javascript:void(0);'>
                            <span class='icon {0}'><img src='/Backend/Content/Common/0.gif'></span>
                            <span class='text'>{2}</span>
                        </a>
                    </span>
                </li>";

            sb.AppendFormatLine(itemTemplate, "expand", isExpanded || useTexteditor ? "none" : "block", GlobalStrings.ShowVisualEditor);
            sb.AppendFormatLine(itemTemplate, "collapse", "none", GlobalStrings.HideVisualEditor);

            if (useTexteditor)
            {
                sb.AppendFormatLine(itemTemplate, "visualeditor", "block", GlobalStrings.ToVisualEditor);
                sb.AppendFormatLine(itemTemplate, "texteditor", "none", GlobalStrings.ToTextEditor);
            }

            sb.AppendLine("</ul></div>");
            sb.AppendLine("<div class='visualEditorContainer'>");
            sb.AppendLine(ve.ToString());
            sb.AppendLine("</div>");

            var componentTag = new TagBuilder("div");
            componentTag.AddCssClass(VisualEditorComponentClassName);
            componentTag.InnerHtml = sb.ToString();
            return MvcHtmlString.Create(componentTag.ToString());
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

        public static MvcHtmlString DateTime(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool isNullable = false, bool readOnly = false) => source.DateTimePicker(id, value, htmlAttributes, DateTimePickerMode.DateTime, readOnly);

        public static MvcHtmlString Date(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool isNullable = false, bool readOnly = false) => source.DateTimePicker(id, value, htmlAttributes, DateTimePickerMode.Date, readOnly);

        public static MvcHtmlString Time(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool isNullable = false, bool readOnly = false) => source.DateTimePicker(id, value, htmlAttributes, DateTimePickerMode.Time, readOnly);

        private static MvcHtmlString DateTimePicker(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, int mode, bool isReadOnly)
        {
            var inputId = htmlAttributes["id"].ToString();
            var stringValue = value?.ToString();
            var newHtmlAttributes = new Dictionary<string, object> { { "id", inputId } };
            string htmlString;

            var className = mode == DateTimePickerMode.Date
                ? DateTextboxClassName
                : mode == DateTimePickerMode.Time
                    ? TimeTextboxClassName
                    : DateTimeTextboxClassName;

            newHtmlAttributes.Add("class", className);
            newHtmlAttributes.CopyValueIfExists(htmlAttributes, DataContentFieldName);

            switch (mode)
            {
                case DateTimePickerMode.DateTime:
                    htmlString = source.Telerik().DateTimePicker()
                        .Name(id)
                        .Value(stringValue)
                        .Enable(!isReadOnly)
                        .InputHtmlAttributes(newHtmlAttributes)
                        .ToHtmlString();
                    break;
                case DateTimePickerMode.Date:
                    htmlString = source.Telerik().DatePicker()
                        .Name(id)
                        .Value(stringValue)
                        .Enable(!isReadOnly)
                        .InputHtmlAttributes(newHtmlAttributes)
                        .ToHtmlString();
                    break;
                case DateTimePickerMode.Time:
                    htmlString = source.Telerik().TimePicker()
                        .Name(id)
                        .Value(stringValue)
                        .Enable(!isReadOnly)
                        .InputHtmlAttributes(newHtmlAttributes)
                        .ToHtmlString();
                    break;
                default:
                    throw new NotSupportedException();
            }

            return MvcHtmlString.Create(htmlString);
        }

        public static MvcHtmlString File(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, Field field, int? entityId, ArticleVersion version, bool? isReadOnly = null, bool? allowUpload = null, bool allowPreview = true, bool allowDownload = true)
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
            tb.InnerHtml = source.FileContents(id, value, htmlAttributes, allowLibrary, shouldAllowUpload, allowPreview, allowDownload);

            return MvcHtmlString.Create(tb.ToString());
        }

        private static string FileContents(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool allowLibrary, bool allowUpload, bool allowPreview, bool allowDownload)
        {
            var sb = new StringBuilder();
            sb.Append(source.QpTextBox(id, value, htmlAttributes));
            if (allowPreview)
            {
                sb.Append(source.ImagePreview(id));
            }

            if (allowDownload)
            {
                sb.Append(source.FileDownload(id));
            }

            if (allowLibrary)
            {
                sb.Append(source.ImageLibrary(id));
            }

            if (allowUpload)
            {
                sb.Append(source.FileUpload(id));
            }

            return sb.ToString();
        }

        internal static TagBuilder FileWrapper(this HtmlHelper source, string id, string fieldId, Field field, int? entityId, ArticleVersion version, bool readOnly, bool allowUpload)
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
                tb.MergeDataAttribute("uploader_type", ((int)UploaderTypeHelper.UploaderType).ToString());
                tb.MergeDataAttribute("folder_Id", folder?.Id.ToString() ?? string.Empty);
            }

            return tb;
        }

        public static MvcHtmlString UnlockLink(this HtmlHelper source, LockableEntityViewModel model) => source.BackendActionLink(model.UnlockId, model.UnlockText, model.Data.Id, model.Data.Name, model.ParentEntityId, ActionTypeCode.ChangeLock, model.CaptureLockActionCode);

        public static MvcHtmlString SelectAllLink(this HtmlHelper source, ListViewModel model) => source.BackendActionLink(model.SelectAllId, GlobalStrings.SelectAll, 0, string.Empty, model.ParentEntityId, ActionTypeCode.SelectAll, string.Empty, ActionTargetType.NewTab, true);

        public static MvcHtmlString UnselectLink(this HtmlHelper source, ListViewModel model) => source.BackendActionLink(model.UnselectId, GlobalStrings.CancelSelection, 0, string.Empty, model.ParentEntityId, ActionTypeCode.DeselectAll, string.Empty, ActionTargetType.NewTab, true);

        public static MvcHtmlString ParentPermissionLink(this HtmlHelper source, ChildEntityPermissionListViewModel model) => source.BackendActionLink(model.UniqueId("chlpActionLink"), EntityPermissionStrings.ParentEntityPermission, 0, string.Empty, model.ParentEntityId, ActionTypeCode.List, model.ParentPermissionsListAction, ActionTargetType.NewTab, true);

        public static MvcHtmlString AddNewItemLink(this HtmlHelper source, ListViewModel model) => source.BackendActionLink(model.AddNewItemLinkId, model.AddNewItemText, 0, string.Empty, model.ParentEntityId, ActionTypeCode.AddNew, model.AddNewItemActionCode, ActionTargetType.NewTab, true);

        public static MvcHtmlString SimpleAddActionLink(this HtmlHelper source, string text) => MvcHtmlString.Create(@"<span class=""linkButton actionLink""><a href=""javascript:void(0);""><span class=""icon add""> " + $@"<img src=""/Backend/Content/Common/0.gif""></span><span class=""text"">{text}</span></a></span>");

        public static MvcHtmlString AggregationListFor<TValue>(this HtmlHelper source, string name, IEnumerable<TValue> list, string bindings, Dictionary<string, string> additionalData = null)
        {
            var div = new TagBuilder("div");
            div.MergeAttribute("id", source.UniqueId(name + "_aggregationlist"));
            div.AddCssClass(AggregationListClassName);
            div.InnerHtml = $@"<div class =""{AggregationListContainerClassName}""></div>" + $@"<input type=""hidden"" name=""AggregationListItems{name.Replace(".", string.Empty)}"" class=""{AggregationListResultClassName}"">";
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
            return MvcHtmlString.Create(div.ToString());
        }

        public static MvcHtmlString VersionTextFor<TValue>(this HtmlHelper source, string name, TValue text) => source.VersionText(source.UniqueId(name + "_versionText"), text.ToString());

        public static MvcHtmlString VersionAreaFor<TValue>(this HtmlHelper source, string name, TValue text) => source.VersionArea(source.UniqueId(name + "_versionText"), text.ToString());

        public static MvcHtmlString WorkflowFor<TValue>(this HtmlHelper source, string name, IEnumerable<TValue> list)
        {
            var div = new TagBuilder("div");
            div.MergeAttribute("id", source.UniqueId(name + "_workflow_control"));
            div.AddCssClass(WorkflowControlClassName);
            div.MergeDataAttribute("workflow_list_data", JsonConvert.SerializeObject(list));
            div.InnerHtml = $@"<div class =""{WorkflowContainerClassName}""></div>";
            return MvcHtmlString.Create(div.ToString());
        }

        public static MvcHtmlString BackendActionLink(this HtmlHelper source, string id, string text, int entityId, string entityName, int parentEntityId, string actionTypeCode, string actionCode, ActionTargetType actionTargetType = ActionTargetType.NewTab, bool returnListElement = false)
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
            span.InnerHtml = sb.ToString();

            var result = span.ToString();
            if (returnListElement)
            {
                result = @"<li class=""doctab-title__element"">" + result + "</li>";
            }

            return MvcHtmlString.Create(result);
        }

        public static MvcHtmlString QpTextBox<TModel>(this HtmlHelper<TModel> source, string fieldName, string fieldValue, Dictionary<string, object> htmlAttributes = null)
        {
            var htmlProperties = source.QpHtmlProperties(fieldName, 0, EditorType.Textbox);
            htmlProperties.Merge(htmlAttributes, true);
            return source.TextBox(fieldName, fieldValue, htmlProperties);
        }

        public static MvcHtmlString QpTextBoxFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            var htmlProperties = source.QpHtmlProperties(expression, EditorType.Textbox);
            htmlProperties.Merge(htmlAttributes, true);
            return source.TextBoxFor(expression, htmlProperties);
        }

        public static MvcHtmlString QpTextAreaFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            var htmlProperties = source.QpHtmlProperties(expression, EditorType.TextArea).Merge(htmlAttributes, true);
            return source.TextAreaFor(expression, htmlProperties);
        }

        public static MvcHtmlString DateTimeFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression)
        {
            var data = source.GetMetaData(expression);
            return source.DateTime(ExpressionHelper.GetExpressionText(expression), data.Model, source.QpHtmlProperties(expression, EditorType.Textbox));
        }

        public static MvcHtmlString DateFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression)
        {
            var data = source.GetMetaData(expression);
            return source.Date(ExpressionHelper.GetExpressionText(expression), data.Model, source.QpHtmlProperties(expression, EditorType.Textbox));
        }

        public static MvcHtmlString TimeFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression)
        {
            var data = source.GetMetaData(expression);
            return source.Time(ExpressionHelper.GetExpressionText(expression), data.Model, source.QpHtmlProperties(expression, EditorType.Textbox));
        }

        public static MvcHtmlString NumericFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, int decimalDigits = 0, double? minValue = null, double? maxValue = null, Dictionary<string, object> htmlAttributes = null)
        {
            var data = source.GetMetaData(expression);
            return source.NumericTextBox(ExpressionHelper.GetExpressionText(expression), data.Model, source.QpHtmlProperties(expression, EditorType.Numeric).Merge(htmlAttributes, true), decimalDigits, minValue, maxValue);
        }

        public static MvcHtmlString FileFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Field field, Dictionary<string, object> htmlAttributes)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var htmlProperties = source.QpHtmlProperties(expression, EditorType.File);
            htmlProperties.Merge(htmlAttributes, true);
            return source.File(name, null, htmlProperties, field, null, null, false, true, false, false);
        }

        public static MvcHtmlString QpCheckBoxFor<TModel>(this HtmlHelper<TModel> source, Expression<Func<TModel, bool>> expression, string toggleId = null, bool reverseToggle = false, Dictionary<string, object> htmlAttributes = null)
        {
            var htmlProperties = source.QpHtmlProperties(expression, EditorType.Checkbox);
            if (!string.IsNullOrWhiteSpace(toggleId))
            {
                htmlProperties.AddData("toggle_for", source.UniqueId(toggleId));
                htmlProperties.AddData("reverse", reverseToggle.ToString().ToLowerInvariant());
            }

            htmlProperties.Merge(htmlAttributes, true);
            return source.CheckBoxFor(expression, htmlProperties);
        }

        public static MvcHtmlString QpDropDownListFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, Dictionary<string, object> htmlAttributes, SelectOptions dropDownOptions)
        {
            var qpSelectListItems = list.ToList();
            var options = new ControlOptions { Enabled = !source.IsReadOnly() && !dropDownOptions.ReadOnly };
            var name = ExpressionHelper.GetExpressionText(expression);
            var showedList = string.IsNullOrEmpty(dropDownOptions?.DefaultOption)
                ? qpSelectListItems.ToList()
                : new[] { new QPSelectListItem { Value = string.Empty, Text = dropDownOptions.DefaultOption } }.Concat(qpSelectListItems).ToList();

            options.SetDropDownOptions(name, source.UniqueId(name), qpSelectListItems, dropDownOptions?.EntityDataListArgs);
            options.HtmlAttributes.Merge(htmlAttributes, true);
            return source.DropDownListFor(expression, showedList, options.HtmlAttributes);
        }

        public static MvcHtmlString QpRadioButtonListFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection = RepeatDirection.Horizontal, EntityDataListArgs entityDataListArgs = null, ControlOptions options = null)
        {
            var div = new TagBuilder("div");
            var qpSelectListItems = list.ToList();
            var name = ExpressionHelper.GetExpressionText(expression);
            var id = source.UniqueId(name);
            var localOptions = options ?? new ControlOptions();

            localOptions.Enabled &= !source.IsReadOnly();
            localOptions.SetRadioButtonListOptions(name, id, qpSelectListItems, repeatDirection, entityDataListArgs);
            div.MergeAttributes(localOptions.HtmlAttributes);

            var sb = new StringBuilder();
            sb.AppendLine("<ul>");

            var itemIndex = 0;
            foreach (var item in qpSelectListItems)
            {
                var htmlAttributes = source.QpHtmlProperties(expression, EditorType.RadioButton, itemIndex);
                if (!localOptions.Enabled)
                {
                    htmlAttributes = AddReadOnlyToHtmlAttributes(EditorType.RadioButton, htmlAttributes);
                }

                sb.Append("<li>");
                sb.Append(source.RadioButtonFor(expression, item.Value, htmlAttributes));
                sb.Append(" ");
                sb.Append(source.QpLabelFor(expression, item.Text, false, itemIndex));
                sb.AppendLine("</li>");
                itemIndex++;
            }

            sb.AppendLine("</ul>");
            div.InnerHtml = sb.ToString();
            return MvcHtmlString.Create(div.ToString());
        }

        public static MvcHtmlString QpCheckBoxListFor<TModel>(this HtmlHelper<TModel> source, Expression<Func<TModel, IList<QPCheckedItem>>> expression, IEnumerable<QPSelectListItem> list, EntityDataListArgs entityDataListArgs, Dictionary<string, object> htmlAttributes, RepeatDirection repeatDirection = RepeatDirection.Vertical)
        {
            var qpSelectListItems = list.ToList();
            var name = ExpressionHelper.GetExpressionText(expression);
            var options = new ControlOptions { Enabled = !source.IsReadOnly() };
            if (htmlAttributes != null)
            {
                options.HtmlAttributes = htmlAttributes;
            }

            foreach (var item in qpSelectListItems)
            {
                item.Selected = false;
            }

            if (source.GetMetaData(expression).Model is IList<QPCheckedItem> propertyValue && propertyValue.Count > 0)
            {
                var checkedValues = propertyValue.Select(i => i.Value).Intersect(qpSelectListItems.Select(b => b.Value)).ToList();
                foreach (var item in qpSelectListItems)
                {
                    item.Selected = checkedValues.Contains(item.Value);
                }
            }

            return source.QpCheckBoxList(name, qpSelectListItems, options, entityDataListArgs, repeatDirection, true);
        }

        public static MvcHtmlString CheckBoxTreeFor<TModel>(this HtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression, string entityTypeCode, int? parentEntityId, string actionCode, bool allowGlobalSelection = false, Dictionary<string, object> htmlAttributes = null)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var options = new Dictionary<string, object> { { "id", source.UniqueId(name) } };
            options.AddData("entity_type_code", entityTypeCode);
            options.AddData("parent_entity_id", parentEntityId);
            options.AddData("read_action_code", actionCode);
            options.AddData("allow_global_selection", allowGlobalSelection.ToString().ToLowerInvariant());
            options.AddData("tree_name", name);
            options.AddData("show_checkbox", bool.TrueString.ToLowerInvariant());
            options.AddCssClass(CheckBoxTreeClassName);

            if (source.GetMetaData(expression).Model is IList<QPTreeCheckedNode> propertyValue && propertyValue.Count > 0)
            {
                var selectedIDsString = string.Join(";", propertyValue.Select(i => i.Value));
                options.AddData("selected_ids", selectedIDsString);
            }

            options.Merge(htmlAttributes, true);
            return MvcHtmlString.Create(source.Telerik().TreeView()
                .Name(name)
                .HtmlAttributes(options)
                .ToHtmlString()
            );
        }

        public static MvcHtmlString VirtualFieldTreeFor<TModel>(this HtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<QPTreeCheckedNode>>> expression, int? parentEntityId, int virtualContentId, Dictionary<string, object> htmlAttributes = null)
        {
            var options = new Dictionary<string, object>();
            options.AddData("virtual_content_id", virtualContentId);
            options.Merge(htmlAttributes, true);
            return source.CheckBoxTreeFor(expression, EntityTypeCode.Field, parentEntityId, ActionCode.Fields, false, options);
        }

        public static MvcHtmlString MultipleItemPickerFor<TModel>(this HtmlHelper<TModel> source, string name, IEnumerable<QPSelectListItem> selectedItemList, EntityDataListArgs entityDataListArgs, Dictionary<string, object> htmlAttributes = null)
        {
            var options = new ControlOptions { Enabled = !source.IsReadOnly() };
            options.HtmlAttributes.Merge(htmlAttributes, true);
            return source.Relation(name, selectedItemList, options, RelationType.ManyToMany, true, entityDataListArgs);
        }

        public static MvcHtmlString MultipleItemPickerFor<TModel>(this HtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<int>>> expression, IEnumerable<ListItem> selectedItemList, EntityDataListArgs entityDataListArgs, Dictionary<string, object> htmlAttributes = null)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            return source.MultipleItemPickerFor(name, selectedItemList.Select(c => new QPSelectListItem { Selected = true, Text = c.Text, Value = c.Value }).ToArray(), entityDataListArgs, htmlAttributes);
        }

        public static MvcHtmlString UnionContentsFor<TModel>(this HtmlHelper<TModel> source, Expression<Func<TModel, IEnumerable<int>>> expression, IEnumerable<ListItem> selectedItemList, int siteId, Dictionary<string, object> htmlAttributes = null)
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

        public static IEnumerable<QPSelectListItem> List(this HtmlHelper source, IEnumerable<ListItem> list)
        {
            var src = source;
            return list.Select(n => new QPSelectListItem
            {
                Text = n.Text,
                Value = n.Value,
                HasDependentItems = n.HasDependentItems,
                DependentItemIDs = n.DependentItemIDs?.Select(s => src.UniqueId(s)).ToArray(),
                Selected = n.Selected
            });
        }

        public static string FormatAsTime(this HtmlHelper source, object value, DateTime? defaultValue = null) => DateTimePart(value, "T", defaultValue);

        public static string FormatAsDate(this HtmlHelper source, object value, DateTime? defaultValue = null) => DateTimePart(value, "d", defaultValue);

        public static string FormatAsDateTime(this HtmlHelper source, object value, DateTime? defaultValue = null) => DateTimePart(value, "G", defaultValue);

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
