using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Factories;
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
    // TODO: CLEAN THIS FILE
    public static class HtmlHelpersExtensions
    {
        public const string TEXTBOX_CLASS_NAME = "textbox simple-text";
        public const string VISUAL_EDITOR_TEXTBOX_CLASS_NAME = "textbox";
        public const string ARTICLE_TEXTBOX_CLASS_NAME = "article textbox simple-text";
        public const string ARTICLE_VISUAL_EDITOR_TEXTBOX_CLASS_NAME = "article textbox";
        public const string NUMERIC_TEXTBOX_CLASS_NAME = "t-input textbox";

        public const string CHECKBOX_CLASS_NAME = "checkbox";
        public const string SIMPLE_CHECKBOX_CLASS_NAME = "simple-checkbox";
        public const string CHECKBOX_LIST_ITEM_CLASS_NAME = "chb-list-item";
        public const string MULTIPLE_PICKER_ITEM_CHECKBOX_CLASS_NAME = "multi-picker-item";
        public const string MULTIPLE_PICKER_OVERFLOW_HIDDEN_VALUE = "overflowHiddenValue";

        public const string NO_TRACK_CHANGE_INPUT_CLASS = "qp-notChangeTrack";

        public const string VISUAL_EDITOR_CLASS_NAME = "visualEditor";
        public const string VISUAL_EDITOR_COMPONENT_CLASS_NAME = "visualEditorComponent";
        public const string VISUAL_EDITOR_TOOLBAR_CLASS_NAME = "visualEditorToolbar";
        public const string VERSION_AREA_CLASS_NAME = "versionArea";
        public const string VERSION_TEXT_CLASS_NAME = "versionText";
        public const string DATA_LIST_CLASS_NAME = "dataList";
        public const string CHECK_BOX_TREE_CLASS_NAME = "checkboxTree";
        public const string DROP_DOWN_LIST_CLASS_NAME = "dropDownList";
        public const string LISTBOX_CLASS_NAME = "listBox";
        public const string RADIO_BUTTONS_LIST_CLASS_NAME = "radioButtonsList";
        public const string CHECKBOXS_LIST_CLASS_NAME = "checkboxsList";
        public const string SINGLE_ITEM_PICKER_CLASS_NAME = "singleItemPicker";
        public const string MULTIPLE_ITEM_PICKER_CLASS_NAME = "multipleItemPicker";
        public const string FILE_FIELD_CLASS_NAME = "fileField";
        public const string FIELD_WRAPPER_CLASS_NAME = "fieldWrapper";
        public const string BROWSE_BUTTON_CLASS_NAME = "browseButton";
        public const string DOWNLOAD_BUTTON_CLASS_NAME = "downloadButton";
        public const string PREVIEW_BUTTON_CLASS_NAME = "previewButton";
        public const string LIBRARY_BUTTON_CLASS_NAME = "libraryButton";
        public const string PICK_BUTTON_CLASS_NAME = "pickButton";
        public const string LINK_BUTTON_CLASS_NAME = "linkButton";
        public const string ACTION_LINK_CLASS_NAME = "actionLink";
        public const string AGGREGATION_LIST_CLASS_NAME = "aggregationList";
        public const string WORKFLOW_CONTROL_CLASS_NAME = "workflow_control";
        public const string AGGREGATION_LIST_RESULT_CLASS_NAME = "aggregationListResult";
        public const string AGGREGATION_LIST_CONTAINER_CLASS_NAME = "aggregationListContainer";
        public const string WORKFLOW_RESULT_CLASS_NAME = "workflowResult";
        public const string WORKFLOW_CONTAINER_CLASS_NAME = "workflowContainer";
        public const string HORIZONTAL_DIRECTION_CLASS_NAME = "horizontalDirection";
        public const string VERTICAL_DIRECTION_CLASS_NAME = "verticalDirection";
        public const string DISABLED_CLASS_NAME = "disabled";
        public const string SELF_CLEAR_FLOATS_CLASS_NAME = "group";

        internal static Dictionary<string, object> QPHtmlProperties<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, EditorType type, int index = -1)
        {
            var data = source.GetMetaData(expression);
            var name = ExpressionHelper.GetExpressionText(expression);
            var maxlength = source.GetMaxLength(data.ContainerType, data.PropertyName);

            return source.QPHtmlProperties(name, maxlength, type, index);
        }

        internal static Dictionary<string, object> QPHtmlProperties(this HtmlHelper source, string name, EditorType type, int index = -1)
        {
            return source.QPHtmlProperties(name, 0, type, index);
        }

        internal static Dictionary<string, object> QPHtmlProperties(this HtmlHelper source, string name, int maxlength, EditorType type, int index = -1)
        {
            var htmlProperties = new Dictionary<string, object> { { "id", source.UniqueId(name, index) } };

            switch (type)
            {
                case EditorType.Checkbox:
                    htmlProperties.Add("class", CHECKBOX_CLASS_NAME);
                    htmlProperties.AddCssClass(SIMPLE_CHECKBOX_CLASS_NAME);
                    break;
                case EditorType.Select:
                    htmlProperties.Add("class", DROP_DOWN_LIST_CLASS_NAME);
                    break;
                case EditorType.ListBox:
                    htmlProperties.Add("class", LISTBOX_CLASS_NAME);
                    break;
                case EditorType.TextArea:
                    htmlProperties.Add("class", TEXTBOX_CLASS_NAME);
                    htmlProperties.Add("rows", 5);
                    break;
                case EditorType.VisualEditor:
                    htmlProperties.Add("class", VISUAL_EDITOR_CLASS_NAME);
                    break;
                case EditorType.Textbox:
                case EditorType.Password:
                    htmlProperties.Add("class", TEXTBOX_CLASS_NAME);
                    break;
                case EditorType.Numeric:
                    htmlProperties.Add("class", NUMERIC_TEXTBOX_CLASS_NAME);
                    break;
                case EditorType.File:
                    htmlProperties.Add("class", TEXTBOX_CLASS_NAME);
                    break;
            }

            if (maxlength != 0)
            {
                htmlProperties.Add("maxlength", maxlength);
            }

            if (source.IsReadOnly())
            {
                htmlProperties.Add("disabled", string.Empty);
            }

            return htmlProperties;
        }

        internal static Dictionary<string, object> QPHtmlProperties(this HtmlHelper source, string id, Field field, bool readOnly)
        {
            return source.QPHtmlProperties(id, field, -1, readOnly);
        }

        internal static Dictionary<string, object> QPHtmlProperties(this HtmlHelper source, string id, Field field, int index, bool readOnly)
        {
            var htmlProperties = new Dictionary<string, object> { { "id", source.UniqueId(id, index) } };
            htmlProperties.AddData("exact_type", field.ExactType.ToString());
            switch (field.Type.Name)
            {
                case FieldTypeName.Boolean:
                    htmlProperties.Add("class", CHECKBOX_CLASS_NAME);
                    htmlProperties.AddCssClass(SIMPLE_CHECKBOX_CLASS_NAME);
                    break;
                case FieldTypeName.Numeric:
                    htmlProperties.Add("class", NUMERIC_TEXTBOX_CLASS_NAME);
                    break;
                default:
                    if (field.ExactType == FieldExactTypes.Textbox)
                    {
                        htmlProperties.Add("class", ARTICLE_TEXTBOX_CLASS_NAME);
                        htmlProperties.Add("rows", field.TextBoxRows >= 255 ? 5 : field.TextBoxRows);
                    }
                    else if (field.ExactType == FieldExactTypes.VisualEdit)
                    {
                        htmlProperties.Add("class", ARTICLE_VISUAL_EDITOR_TEXTBOX_CLASS_NAME);
                        htmlProperties.Add("style", $"height: {field.VisualEditorHeight}px");
                    }

                    else if (!(field.RelationType == RelationType.OneToMany || field.RelationType == RelationType.ManyToMany || field.RelationType == RelationType.ManyToOne))
                    {
                        if (field.Type.Name == FieldTypeName.String)
                        {
                            htmlProperties.Add("maxlength", field.StringSize);
                            htmlProperties.Add("class", ARTICLE_TEXTBOX_CLASS_NAME);
                        }
                        else
                        {
                            htmlProperties.Add("class", TEXTBOX_CLASS_NAME);
                        }
                    }
                    break;
            }

            if (readOnly && (field.Type.Name != FieldTypeName.Relation || field.Type.Name != FieldTypeName.M2ORelation))
            {
                htmlProperties.Add("disabled", "");
            }

            return htmlProperties;
        }

        private static int GetMaxLength<TModel>(this HtmlHelper<TModel> source, Type sourceType, string propertyName)
        {
            var attr = source.GetCustomAttribute(sourceType, propertyName, typeof(MaxLengthValidatorAttribute));
            return ((MaxLengthValidatorAttribute)attr)?.UpperBound ?? 0;
        }

        internal static string GetExampleText<TModel>(this HtmlHelper<TModel> source, Type sourceType, string propertyName)
        {
            var attr = source.GetCustomAttribute(sourceType, propertyName, typeof(ExampleAttribute));
            return ((ExampleAttribute)attr)?.Text;
        }

        private static object GetCustomAttribute<TModel>(this HtmlHelper<TModel> source, Type sourceType, string propertyName, Type type)
        {
            var result = sourceType.GetProperty(propertyName);
            var attrs = result.GetCustomAttributes(type, true);
            return attrs.Length > 0 ? attrs[0] : null;
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
                    tb.MergeAttribute("class", BROWSE_BUTTON_CLASS_NAME + " l-sl-uploader");
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

        private static string ImgButton(this HtmlHelper source, string id, string title, string cssClassName)
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

        internal static string FileDownload(this HtmlHelper source, string id)
        {
            return source.ImgButton(source.UniqueId(id + "_download"), GlobalStrings.ViewDownload, DOWNLOAD_BUTTON_CLASS_NAME);
        }

        internal static string ImagePreview(this HtmlHelper source, string id)
        {
            return source.ImgButton(source.UniqueId(id + "_preview"), GlobalStrings.Preview, PREVIEW_BUTTON_CLASS_NAME);
        }

        public static string ImageLibrary(this HtmlHelper source, string id)
        {
            return source.ImgButton(source.UniqueId(id + "_library"), GlobalStrings.Library, LIBRARY_BUTTON_CLASS_NAME);
        }

        private static string DateTimePart(object value, string formatString, DateTime? defaultValue)
        {
            DateTime dt;
            if (value != null && System.DateTime.TryParse(value.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
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
            var properties = new Dictionary<string, object>();
            properties.Add("class", VERSION_TEXT_CLASS_NAME);
            properties.Add("name", id);
            if (string.IsNullOrEmpty(value))
            {
                value = "&nbsp;";
            }

            return source.Div(id, value, properties);
        }

        public static MvcHtmlString VersionArea(this HtmlHelper source, string id, string value)
        {
            var properties = new Dictionary<string, object> { { "class", VERSION_AREA_CLASS_NAME }, { "name", id }, { "style", "overflow : auto" } };
            return source.Div(id, value, properties);
        }

        public static MvcHtmlString Span(this HtmlHelper source, string id, object value)
        {
            var tb = new TagBuilder("span");
            tb.MergeAttribute("id", id);
            tb.InnerHtml = value.ToString();

            return MvcHtmlString.Create(tb.ToString());
        }

        public static MvcHtmlString NumericTextBox(this HtmlHelper source, string name, object value, Dictionary<string, object> htmlAttributes, Field field)
        {
            return source.NumericTextBox(name, value, htmlAttributes, field.DecimalPlaces);
        }

        public static MvcHtmlString NumericTextBox(this HtmlHelper source, string name, object value, Dictionary<string, object> htmlAttributes, int decimalDigits = 0, double? minValue = null, double? maxValue = null)
        {
            object newHtmlAttributes;
            if (CheckReadOnly(htmlAttributes))
            {
                newHtmlAttributes = new { id = htmlAttributes["id"], @class = htmlAttributes["class"], disabled = "" };
            }
            else
            {
                newHtmlAttributes = new { id = htmlAttributes["id"], @class = htmlAttributes["class"] };
            }

            return MvcHtmlString.Create(source.Telerik().NumericTextBox()
                .MinValue(minValue)
                .MaxValue(maxValue)
                .ButtonTitleDown(GlobalStrings.DecreaseValue)
                .ButtonTitleUp(GlobalStrings.IncreaseValue)
                .Name(name)
                .InputHtmlAttributes(newHtmlAttributes)
                .Value(Converter.ToNullableDouble(value))
                .DecimalDigits(decimalDigits)
                .Spinners(true)
                .EmptyMessage(string.Empty)
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

        public static MvcHtmlString Warning(this HtmlHelper source, string message)
        {
            return source.Warning(new[] { new MvcHtmlString(message) });
        }

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

            wrapper.InnerHtml = img.ToString(TagRenderMode.SelfClosing) + string.Join("", messageSpans);
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
        public static MvcHtmlString QpDropDownList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, string optionLabel, ControlOptions options)
        {
            return source.QpDropDownList(name, list, optionLabel, options, new EntityDataListArgs());
        }

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
            options.SetDropDownOptions(name, source.UniqueId(name), list.ToList(), entityDataListArgs);
            if (entityDataListArgs.ShowIds)
            {
                list = list.Select(n => n.CopyWithIdInText());
            }

            return source.DropDownList(name, list, optionLabel, options.HtmlAttributes);
        }

        /// <summary>
        /// Генерирует код списка радио-кнопок
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя списка радио-кнопок</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="options">дополнительные настройки списка радио-кнопок</param>
        /// <returns>код списка радио-кнопок</returns>
        public static MvcHtmlString QpRadioButtonList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options)
        {
            return source.QpRadioButtonList(name, list, options, null);
        }

        /// <summary>
        /// Генерирует код списка радио-кнопок
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя списка радио-кнопок</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="repeatDirection">направление списка</param>
        /// <param name="options">дополнительные настройки списка радио-кнопок</param>
        /// <returns>код списка радио-кнопок</returns>
        public static MvcHtmlString QpRadioButtonList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, RepeatDirection repeatDirection, ControlOptions options)
        {
            return source.QpRadioButtonList(name, list, repeatDirection, options, null);
        }

        /// <summary>
        /// Генерирует код списка радио-кнопок
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="name">имя списка радио-кнопок</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="options">дополнительные настройки списка радио-кнопок</param>
        /// <param name="entityDataListArgs">свойства списка сущностей</param>
        /// <returns>код списка радио-кнопок</returns>
        public static MvcHtmlString QpRadioButtonList(this HtmlHelper source, string name, IEnumerable<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs)
        {
            return source.QpRadioButtonList(name, list, RepeatDirection.Horizontal, options, entityDataListArgs);
        }

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
            options.SetRadioButtonListOptions(name, source.UniqueId(name), list.ToList(), repeatDirection, entityDataListArgs);
            div.MergeAttributes(options.HtmlAttributes);

            var sb = new StringBuilder();
            sb.AppendLine("<ul>");

            var itemIndex = 0;
            foreach (var item in list)
            {
                var itemId = source.UniqueId(name, itemIndex);

                var radioButtonHtmlAttributes = new Dictionary<string, object> { { "id", itemId } };
                if (!options.Enabled)
                {
                    radioButtonHtmlAttributes.Add("disabled", "disabled");
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
            var div = new TagBuilder("div");
            options.SetCheckBoxListOptions(name, source.UniqueId(name), list, repeatDirection, entityDataListArgs);
            div.MergeAttributes(options.HtmlAttributes);

            var sb = new StringBuilder();
            sb.AppendLine("<ul>");

            var itemIndex = 0;
            foreach (var item in list)
            {
                var checkBoxHtmlAttributes = source.QPHtmlProperties(name, EditorType.Checkbox, itemIndex);
                if (!options.Enabled && !checkBoxHtmlAttributes.ContainsKey("disabled"))
                {
                    checkBoxHtmlAttributes.Add("disabled", "disabled");
                }
                checkBoxHtmlAttributes.RemoveCssClass(SIMPLE_CHECKBOX_CLASS_NAME);
                checkBoxHtmlAttributes.AddCssClass(CHECKBOX_LIST_ITEM_CLASS_NAME);
                checkBoxHtmlAttributes.AddCssClass(NO_TRACK_CHANGE_INPUT_CLASS);

                sb.Append("<li>");
                sb.Append(source.QpCheckBox(asArray ? string.Concat(name, "[", itemIndex, "]") : name,
                    item.Value, item.Selected, checkBoxHtmlAttributes));
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

        private static MvcHtmlString QpMultipleItemPicker(this HtmlHelper source, string name, IList<QPSelectListItem> list, ControlOptions options, EntityDataListArgs entityDataListArgs)
        {
            var wrapper = new TagBuilder("div");
            options.SetMultiplePickerOptions(name, source.UniqueId(name), entityDataListArgs);
            wrapper.MergeAttributes(options.HtmlAttributes);

            var sb = new StringBuilder();
            if (list != null && list.Count >= QPConfiguration.WebConfigSection.RelationCountLimit)
            {
                var value = string.Join(",", list.Select(n => n.Value).ToArray());
                sb.AppendLine(source.Hidden(name, value, new { @class = MULTIPLE_PICKER_OVERFLOW_HIDDEN_VALUE, id = source.UniqueId(name) }).ToString());
            }

            sb.AppendLine("<ul>");
            if (list != null && list.Count < QPConfiguration.WebConfigSection.RelationCountLimit)
            {
                var itemIndex = 0;
                foreach (var item in list)
                {
                    var checkBoxHtmlAttributes = source.QPHtmlProperties(name, EditorType.Checkbox, itemIndex);
                    if (!options.Enabled && !checkBoxHtmlAttributes.ContainsKey("disabled"))
                    {
                        checkBoxHtmlAttributes.Add("disabled", "disabled");
                    }

                    checkBoxHtmlAttributes.RemoveCssClass(SIMPLE_CHECKBOX_CLASS_NAME);
                    checkBoxHtmlAttributes.AddCssClass(MULTIPLE_PICKER_ITEM_CHECKBOX_CLASS_NAME);
                    checkBoxHtmlAttributes.AddCssClass(NO_TRACK_CHANGE_INPUT_CLASS);

                    sb.Append("<li>");
                    sb.Append(source.QpCheckBox(name, item.Value, item.Selected, checkBoxHtmlAttributes));
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

        public static MvcHtmlString QpTextBox(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes)
        {
            return source.TextBox(id, value, htmlAttributes);
        }

        public static MvcHtmlString QpTextArea(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes)
        {
            return source.TextArea(id, value.ToString(), htmlAttributes);
        }

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
            var htmlAttributes = source.QPHtmlProperties(expression, EditorType.VisualEditor);
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
            componentTag.AddCssClass(VISUAL_EDITOR_COMPONENT_CLASS_NAME);
            componentTag.InnerHtml = sb.ToString();
            return MvcHtmlString.Create(componentTag.ToString());
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        private static void SetVisualEditorAttributes(Dictionary<string, object> htmlAttributes, Field field, bool forceReadOnly)
        {
            htmlAttributes.AddCssClass(VISUAL_EDITOR_CLASS_NAME);
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

        private static bool CheckReadOnly(IReadOnlyDictionary<string, object> htmlAttributes)
        {
            return htmlAttributes != null && htmlAttributes.ContainsKey("disabled");
        }

        public static MvcHtmlString DateTime(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool isNullable = false, bool readOnly = false)
        {
            return source.DateTimePicker(id, value, htmlAttributes, DateTimePickerMode.DateTime, readOnly);
        }

        public static MvcHtmlString Date(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool isNullable = false, bool readOnly = false)
        {
            return source.DateTimePicker(id, value, htmlAttributes, DateTimePickerMode.Date, readOnly);
        }

        public static MvcHtmlString Time(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, bool isNullable = false, bool readOnly = false)
        {
            return source.DateTimePicker(id, value, htmlAttributes, DateTimePickerMode.Time, readOnly);
        }

        private static MvcHtmlString DateTimePicker(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, int mode, bool readOnly)
        {
            var inputId = htmlAttributes["id"].ToString();
            var stringValue = value?.ToString();

            switch (mode)
            {
                case DateTimePickerMode.DateTime:
                    return MvcHtmlString.Create(source.Telerik().DateTimePicker()
                        .Name(id)
                        .Value(stringValue)
                        .Enable(!readOnly)
                        .InputHtmlAttributes(new { id = inputId, @class = "datetime" })
                        .ToHtmlString());
                case DateTimePickerMode.Date:
                    return MvcHtmlString.Create(source.Telerik().DatePicker()
                        .Name(id)
                        .Value(stringValue)
                        .Enable(!readOnly)
                        .InputHtmlAttributes(new { id = inputId, @class = "date" })
                        .ToHtmlString());
                case DateTimePickerMode.Time:
                    return MvcHtmlString.Create(source.Telerik().TimePicker()
                        .Name(id)
                        .Value(stringValue)
                        .Enable(!readOnly)
                        .InputHtmlAttributes(new { id = inputId, @class = "time" })
                        .ToHtmlString());
                default:
                    throw new NotSupportedException();
            }
        }

        public static MvcHtmlString File(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, Field field, int? entityId, ArticleVersion version, bool? isReadOnly = null, bool? allowUpload = null, bool allowPreview = true, bool allowDownload = true)
        {
            var readOnly = isReadOnly ?? field.ExactType == FieldExactTypes.DynamicImage || CheckReadOnly(htmlAttributes);
            var shouldAllowUpload = allowUpload ?? !readOnly;
            var allowLibrary = !readOnly && version == null;

            string fieldId = null;
            if (htmlAttributes.ContainsKey("id"))
            {
                fieldId = htmlAttributes["id"].ToString();
            }
            TagBuilder tb = source.FileWrapper(id, fieldId, field, entityId, version, readOnly, shouldAllowUpload);
            tb.InnerHtml = source.FileContents(id, value, htmlAttributes, field, allowLibrary, shouldAllowUpload, allowPreview, allowDownload);

            return MvcHtmlString.Create(tb.ToString());
        }

        private static string FileContents(this HtmlHelper source, string id, object value, Dictionary<string, object> htmlAttributes, Field field, bool allowLibrary, bool allowUpload, bool allowPreview, bool allowDownload)
        {
            var sb = new StringBuilder();
            sb.Append(source.QpTextBox(id, value, htmlAttributes));
            if (allowPreview && field.ExactType != FieldExactTypes.File)
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
            tb.AddCssClass(FIELD_WRAPPER_CLASS_NAME);
            tb.AddCssClass(FILE_FIELD_CLASS_NAME);
            tb.AddCssClass(SELF_CLEAR_FLOATS_CLASS_NAME);

            tb.MergeDataAttribute("field_name", id);
            tb.MergeDataAttribute("field_id", string.IsNullOrWhiteSpace(fieldId) ? source.UniqueId(id) : fieldId);
            tb.MergeDataAttribute("entity_id", entityId != null ? entityId.ToString() : null);
            tb.MergeDataAttribute("is_version", Converter.ToJsString(isVersion));

            if (!readOnly)
            {
                var useSiteLibrary = isVersion ? false : field.UseSiteLibrary;
                var repository = FolderFactory.Create(useSiteLibrary ? EntityTypeCode.SiteFolder : EntityTypeCode.ContentFolder).CreateRepository();
                var folder = repository.GetRoot(useSiteLibrary ? field.Content.SiteId : field.Content.Id);
                var subFolder = isVersion ? string.Empty : field.SubFolder;
                var libraryEntityId = isVersion ? string.Empty : field.LibraryEntityId.ToString();
                var libraryParentEntityId = isVersion ? string.Empty : field.LibraryParentEntityId.ToString();
                var libraryPath = isVersion ? version.PathInfo.Path : field.PathInfo.Path;
                var libraryUrl = isVersion ? version.PathInfo.Url : field.PathInfo.Url;
                var renameMatched = isVersion ? false : field.RenameMatched;
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
                tb.MergeDataAttribute("folder_Id", folder == null ? string.Empty : folder.Id.ToString());
            }

            return tb;
        }

        public static MvcHtmlString UnlockLink(this HtmlHelper source, LockableEntityViewModel model)
        {
            return source.BackendActionLink(model.UnlockId, model.UnlockText, model.Data.Id, model.Data.Name, model.ParentEntityId, ActionTypeCode.ChangeLock, model.CaptureLockActionCode);
        }

        public static MvcHtmlString SelectAllLink(this HtmlHelper source, ListViewModel model)
        {
            return source.BackendActionLink(model.SelectAllId, GlobalStrings.SelectAll, 0, string.Empty, model.ParentEntityId, ActionTypeCode.SelectAll, string.Empty, ActionTargetType.NewTab, true);
        }

        public static MvcHtmlString UnselectLink(this HtmlHelper source, ListViewModel model)
        {
            return source.BackendActionLink(model.UnselectId, GlobalStrings.CancelSelection, 0, string.Empty, model.ParentEntityId, ActionTypeCode.DeselectAll, string.Empty, ActionTargetType.NewTab, true);
        }

        public static MvcHtmlString ParentPermissionLink(this HtmlHelper source, ChildEntityPermissionListViewModel model)
        {
            return source.BackendActionLink(model.UniqueId("chlpActionLink"), EntityPermissionStrings.ParentEntityPermission, 0, string.Empty, model.ParentEntityId, ActionTypeCode.List, model.ParentPermissionsListAction, ActionTargetType.NewTab, true);
        }

        public static MvcHtmlString AddNewItemLink(this HtmlHelper source, ListViewModel model)
        {
            return source.BackendActionLink(model.AddNewItemLinkId, model.AddNewItemText, 0, string.Empty, model.ParentEntityId, ActionTypeCode.AddNew, model.AddNewItemActionCode, ActionTargetType.NewTab, true);
        }

        public static MvcHtmlString BackendActionLink(this HtmlHelper source, string id, string text, int entityId, string entityName, int parentEntityId, string actionTypeCode, string actionCode)
        {
            return source.BackendActionLink(id, text, entityId, entityName, parentEntityId, actionTypeCode, actionCode, ActionTargetType.NewTab);
        }

        public static MvcHtmlString SimpleAddActionLink(this HtmlHelper source, string text)
        {
            return MvcHtmlString.Create(@"<span class=""linkButton actionLink""><a href=""javascript:void(0);""><span class=""icon add""> " + $@"<img src=""/Backend/Content/Common/0.gif""></span><span class=""text"">{text}</span></a></span>");
        }

        public static MvcHtmlString AggregationListFor<TValue>(this HtmlHelper source, string name, IEnumerable<TValue> list, string bindings, Dictionary<string, string> additionalData = null)
        {
            var div = new TagBuilder("div");
            div.MergeAttribute("id", source.UniqueId(name + "_aggregationlist"));
            div.AddCssClass(AGGREGATION_LIST_CLASS_NAME);
            div.InnerHtml = $@"<div class =""{AGGREGATION_LIST_CONTAINER_CLASS_NAME}""></div>" + $@"<input type=""hidden"" name=""AggregationListItems{name.Replace(".", string.Empty)}"" class=""{AGGREGATION_LIST_RESULT_CLASS_NAME}"">";
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

        public static MvcHtmlString VersionTextFor<TValue>(this HtmlHelper source, string name, TValue text)
        {
            return source.VersionText(source.UniqueId(name + "_versionText"), text.ToString());
        }

        public static MvcHtmlString VersionAreaFor<TValue>(this HtmlHelper source, string name, TValue text)
        {
            return source.VersionArea(source.UniqueId(name + "_versionText"), text.ToString());
        }

        public static MvcHtmlString WorkflowFor<TValue>(this HtmlHelper source, string name, IEnumerable<TValue> list)
        {
            var div = new TagBuilder("div");
            div.MergeAttribute("id", source.UniqueId(name + "_workflow_control"));
            div.AddCssClass(WORKFLOW_CONTROL_CLASS_NAME);
            div.MergeDataAttribute("workflow_list_data", JsonConvert.SerializeObject(list));
            div.InnerHtml = $@"<div class =""{WORKFLOW_CONTAINER_CLASS_NAME}""></div>";
            return MvcHtmlString.Create(div.ToString());
        }

        public static MvcHtmlString BackendActionLink(this HtmlHelper source, string id, string text, int entityId, string entityName, int parentEntityId, string actionTypeCode, string actionCode, ActionTargetType actionTargetType = ActionTargetType.NewTab, bool returnListElement = false)
        {
            var span = new TagBuilder("span");
            span.MergeAttribute("id", id);
            span.AddCssClass(LINK_BUTTON_CLASS_NAME);
            span.AddCssClass(ACTION_LINK_CLASS_NAME);
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

        public static MvcHtmlString QpTextBoxFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            var options = source.QPHtmlProperties(expression, EditorType.Textbox);
            options.Merge(htmlAttributes, true);
            return source.TextBoxFor(expression, options);
        }

        public static MvcHtmlString QpTextAreaFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Dictionary<string, object> htmlAttributes = null)
        {
            var attrs = source.QPHtmlProperties(expression, EditorType.TextArea).Merge(htmlAttributes, true);
            return source.TextAreaFor(expression, attrs);
        }

        public static MvcHtmlString DateTimeFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression)
        {
            var data = source.GetMetaData(expression);
            return source.DateTime(ExpressionHelper.GetExpressionText(expression), data.Model, source.QPHtmlProperties(expression, EditorType.Textbox));
        }

        public static MvcHtmlString DateFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression)
        {
            var data = source.GetMetaData(expression);
            return source.Date(ExpressionHelper.GetExpressionText(expression), data.Model, source.QPHtmlProperties(expression, EditorType.Textbox));
        }

        public static MvcHtmlString TimeFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression)
        {
            var data = source.GetMetaData(expression);
            return source.Time(ExpressionHelper.GetExpressionText(expression), data.Model, source.QPHtmlProperties(expression, EditorType.Textbox));
        }

        public static MvcHtmlString NumericFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, int decimalDigits = 0, double? minValue = null, double? maxValue = null, Dictionary<string, object> htmlAttributes = null)
        {
            var data = source.GetMetaData(expression);
            return source.NumericTextBox(ExpressionHelper.GetExpressionText(expression), data.Model, source.QPHtmlProperties(expression, EditorType.Numeric).Merge(htmlAttributes, true), decimalDigits, minValue, maxValue);
        }

        public static MvcHtmlString FileFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, Field field, Dictionary<string, object> htmlAttributes)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var options = source.QPHtmlProperties(expression, EditorType.File);
            options.Merge(htmlAttributes, true);

            return source.File(name, null, options, field, null, null, false, true, false, false);
        }

        public static MvcHtmlString QpCheckBoxFor<TModel>(this HtmlHelper<TModel> source, Expression<Func<TModel, bool>> expression, string toggleId = null, bool reverseToggle = false, Dictionary<string, object> htmlAttributes = null)
        {
            var options = source.QPHtmlProperties(expression, EditorType.Checkbox);
            if (!string.IsNullOrWhiteSpace(toggleId))
            {
                options.AddData("toggle_for", source.UniqueId(toggleId));
                options.AddData("reverse", reverseToggle.ToString().ToLowerInvariant());
            }

            options.Merge(htmlAttributes, true);
            return source.CheckBoxFor(expression, options);
        }

        /// <summary>
        /// Генерирует код раскрывающегося списка
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="expression">выражение</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="htmlAttributes"></param>
        /// <param name="dropDownOptions">дополнительные опции</param>
        /// <returns>код раскрывающегося списка</returns>
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public static MvcHtmlString QpDropDownListFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list, Dictionary<string, object> htmlAttributes, SelectOptions dropDownOptions)
        {
            var options = new ControlOptions { Enabled = !source.IsReadOnly() && !dropDownOptions.ReadOnly };
            var name = ExpressionHelper.GetExpressionText(expression);

            var showedList = list;
            if (dropDownOptions != null && !string.IsNullOrEmpty(dropDownOptions.DefaultOption))
            {
                showedList = new[] { new QPSelectListItem { Value = "", Text = dropDownOptions.DefaultOption } }.Concat(list).ToArray();
            }

            options.SetDropDownOptions(name, source.UniqueId(name), list.ToList(), dropDownOptions.EntityDataListArgs);
            options.HtmlAttributes.Merge(htmlAttributes, true);
            return source.DropDownListFor(expression, showedList, options.HtmlAttributes);
        }

        /// <summary>
        /// Генерирует код списка радио-кнопок
        /// </summary>
        /// <param name="source">HTML-хелпер</param>
        /// <param name="expression">выражение</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="repeatDirection">направление списка</param>
        /// <param name="entityDataListArgs">свойства списка сущностей</param>
        /// <returns>код списка радио-кнопок</returns>
        public static MvcHtmlString QpRadioButtonListFor<TModel, TValue>(this HtmlHelper<TModel> source, Expression<Func<TModel, TValue>> expression, IEnumerable<QPSelectListItem> list = null, RepeatDirection repeatDirection = RepeatDirection.Horizontal, EntityDataListArgs entityDataListArgs = null, ControlOptions options = null)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var id = source.UniqueId(name);
            var div = new TagBuilder("div");

            var localOptions = options ?? new ControlOptions();
            localOptions.Enabled &= !source.IsReadOnly();
            localOptions.SetRadioButtonListOptions(name, id, list.ToList(), repeatDirection, entityDataListArgs);
            div.MergeAttributes(localOptions.HtmlAttributes);

            var sb = new StringBuilder();
            sb.AppendLine("<ul>");

            var itemIndex = 0;
            foreach (var item in list)
            {
                var radioButtonHtmlAttributes = source.QPHtmlProperties(expression, EditorType.RadioButton, itemIndex);
                if (!localOptions.Enabled && !radioButtonHtmlAttributes.ContainsKey("disabled"))
                {
                    radioButtonHtmlAttributes.Add("disabled", "disabled");
                }

                sb.Append("<li>");
                sb.Append(source.RadioButtonFor(expression, item.Value, radioButtonHtmlAttributes));
                sb.Append(" ");
                sb.Append(source.QpLabelFor(expression, item.Text, false, itemIndex));
                sb.AppendLine("</li>");

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
        /// <param name="expression">выражение</param>
        /// <param name="list">список элементов списка</param>
        /// <param name="repeatDirection">направление списка</param>
        /// <param name="entityDataListArgs">свойства списка сущностей</param>
        /// <returns>код списка чекбоксов</returns>
        public static MvcHtmlString QpCheckBoxListFor<TModel>(this HtmlHelper<TModel> source, Expression<Func<TModel, IList<QPCheckedItem>>> expression, IEnumerable<QPSelectListItem> list, EntityDataListArgs entityDataListArgs, Dictionary<string, object> htmlAttributes, RepeatDirection repeatDirection = RepeatDirection.Vertical)
        {
            var name = ExpressionHelper.GetExpressionText(expression);
            var options = new ControlOptions { Enabled = !source.IsReadOnly() };
            if (htmlAttributes != null)
            {
                options.HtmlAttributes = htmlAttributes;
            }

            var resultList = list.ToArray();
            foreach (var item in resultList)
            {
                item.Selected = false;
            }
            var propertyValue = source.GetMetaData(expression).Model as IList<QPCheckedItem>;
            if (propertyValue != null && propertyValue.Count > 0)
            {
                var checkedValues = propertyValue.Select(i => i.Value).Intersect(list.Select(b => b.Value));
                foreach (var item in resultList)
                {
                    item.Selected = checkedValues.Contains(item.Value);
                }
            }

            return source.QpCheckBoxList(name, resultList, options, entityDataListArgs, repeatDirection, true);
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
            options.AddCssClass(CHECK_BOX_TREE_CLASS_NAME);

            var propertyValue = source.GetMetaData(expression).Model as IList<QPTreeCheckedNode>;
            if (propertyValue != null && propertyValue.Count > 0)
            {
                var selectedIDsString = string.Join(";", propertyValue.Select(i => i.Value));
                options.AddData("selected_ids", selectedIDsString);
            }

            options.Merge(htmlAttributes, true);
            var htmlString = source.Telerik().TreeView().Name(name).HtmlAttributes(options).ToHtmlString();

            return MvcHtmlString.Create(htmlString);
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

        /// <summary>
        /// Преобразует список ListItem(BLL) в список SelectListItem(Web)
        /// </summary>
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

        /// <summary>
        /// Преобразует список ListItem(BLL) в список SelectListItem(Web)
        /// </summary>
        /// TODO: CHECK UNUSED
        public static IEnumerable<QPSelectListItem> List(this HtmlHelper source, IEnumerable<ListItem> list, string value)
        {
            var values = value.Split(',');
            return list.Select(n => new QPSelectListItem
            {
                Text = n.Text,
                Value = n.Value,
                Selected = values.Contains(n.Value),
                HasDependentItems = n.HasDependentItems
            });
        }

        public static string FormatAsTime(this HtmlHelper source, object value, DateTime? defaultValue = null)
        {
            return DateTimePart(value, "T", defaultValue);
        }

        public static string FormatAsDate(this HtmlHelper source, object value, DateTime? defaultValue = null)
        {
            return DateTimePart(value, "d", defaultValue);
        }

        public static string FormatAsDateTime(this HtmlHelper source, object value, DateTime? defaultValue = null)
        {
            return DateTimePart(value, "G", defaultValue);
        }
    }
}
