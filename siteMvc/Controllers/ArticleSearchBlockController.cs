using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.Article;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ArticleSearchBlockController : AuthQpController
    {
        private readonly IArticleSearchService _articleSearchService;

        public ArticleSearchBlockController(IArticleSearchService articleSearchService)
        {
            _articleSearchService = articleSearchService;
        }

        /// <summary>
        /// Возвращает содержимое блока полнотекстового поиска.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> FullTextBlock(int parentEntityId, string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.TextFieldsComboID = $"{elementIdPrefix}_TextFieldsCombo";
            ViewBag.QueryTextBoxID = $"{elementIdPrefix}_QueryTextBox";

            // Получить список "текстовых" полей
            var fieldList = _articleSearchService.GetFullTextSearchableFieldGroups(parentEntityId);
            if (!fieldList.Any())
            {
                return Json(new { success = true, view = string.Empty });
            }

            ViewBag.SearchableFieldList = fieldList;
            return await JsonHtml("FullTextBlock", null);
        }

        /// <summary>
        /// Возвращает содержимое блока поиска по всем полям.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> FieldSearchBlock(int parentEntityId, string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.FieldSearchListElementID = string.Format("{0}_FieldSearchList", ViewBag.ElementIdPrefix);
            ViewBag.FieldSearchSelectorElementID = string.Format("{0}_FieldSearchSelector", ViewBag.ElementIdPrefix);
            ViewBag.FieldsComboID = string.Format("{0}_FieldsCombo", ViewBag.ElementIdPrefix);
            ViewBag.AddFieldSearchElementID = string.Format("{0}_AddFieldSearchButton", ViewBag.ElementIdPrefix);
            ViewBag.SearchableFieldList = _articleSearchService.GetSearchableFieldFieldGroups(parentEntityId);
            return await JsonHtml("FieldSearchBlock", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по идентификатору.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> Identifier(string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.NumberFromElementID = string.Format("{0}_numberFrom", ViewBag.ElementIdPrefix);
            ViewBag.NumberToElementID = string.Format("{0}_numberTo", ViewBag.ElementIdPrefix);
            ViewBag.InverseCheckBoxElementID = string.Format("{0}_inverseCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.DisablingContainerElementID = string.Format("{0}_disablingContainer", ViewBag.ElementIdPrefix);
            ViewBag.ByValueElementID = string.Format("{0}_byValueSelector", ViewBag.ElementIdPrefix);
            ViewBag.TextElementID = $"{elementIdPrefix}_text";
            return await JsonHtml("Identifier", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по полю c датой.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> DateRange(string elementIdPrefix)
        {
            SetDateOrTimeRangeViewBag(elementIdPrefix);
            return await JsonHtml("DateRange", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по полю c временем.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> TimeRange(string elementIdPrefix)
        {
            SetDateOrTimeRangeViewBag(elementIdPrefix);
            return await JsonHtml("TimeRange", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по полю c датой-временем.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> DateTimeRange(string elementIdPrefix)
        {
            SetDateOrTimeRangeViewBag(elementIdPrefix);
            return await JsonHtml("DateTimeRange", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по числовому полю.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> NumericRange(string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.NumberFromElementID = string.Format("{0}_numberFrom", ViewBag.ElementIdPrefix);
            ViewBag.NumberToElementID = string.Format("{0}_numberTo", ViewBag.ElementIdPrefix);
            ViewBag.IsNullCheckBoxElementID = string.Format("{0}_isNullCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.InverseCheckBoxElementID = string.Format("{0}_inverseCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.DisablingContainerElementID = string.Format("{0}_disablingContainer", ViewBag.ElementIdPrefix);
            ViewBag.ByValueElementID = string.Format("{0}_byValueSelector", ViewBag.ElementIdPrefix);
            return await JsonHtml("NumericRange", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по текстовому полю.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> TextSearch(string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.TextBoxElementId = string.Format("{0}_textBox", ViewBag.ElementIdPrefix);
            ViewBag.IsNullCheckBoxElementID = string.Format("{0}_isNullCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.InverseCheckBoxElementID = string.Format("{0}_inverseCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.ExactMatchCheckBoxElementID = string.Format("{0}_exactCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.StartBeginningCheckBoxElementID = string.Format("{0}_beginningCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.ListTextElementID = $"{elementIdPrefix}_listTextBox";
            ViewBag.ByValueElementID = string.Format("{0}_byValueSelector", ViewBag.ElementIdPrefix);
            return await JsonHtml("TextSearch", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по строковому перечислению.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> StringEnum(string elementIdPrefix, [FromQuery(Name = "fieldID")]int fieldId)
        {
            ViewBag.QueryDropDownListID = elementIdPrefix + "_queryDropDownList";
            ViewBag.IsNullCheckBoxID = elementIdPrefix + "_isNullCheckBox";
            var model = FieldService.Read(fieldId).StringEnumItems.Select(o => new QPSelectListItem { Value = o.Value, Text = o.Alias }).ToList();
            return await JsonHtml("StringEnum", model);
        }

        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> ContentsListForClassifier(string elementIdPrefix, [FromQuery(Name = "fieldID")]int fieldId)
        {
            var classifier = FieldService.Read(fieldId);
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.ContentElementID = string.Format("{0}_contentID", ViewBag.ElementIdPrefix);
            ViewBag.IsNullCheckBoxElementID = string.Format("{0}_isNullCheckBox", ViewBag.ElementIdPrefix);
            IEnumerable<QPSelectListItem> model = FieldService.GetAggregetableContentsForClassifier(classifier).Select(o => new QPSelectListItem { Value = o.Value, Text = o.Text }).ToList();
            return await JsonHtml("ContentsListForClassifier", model);
        }

        /// <summary>
        /// Разметка блока поиска по связям.
        /// </summary>
        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> RelationSearch([FromBody] RelationSearchViewModel model)
        {
            ViewBag.IsNullCheckBoxElementID = $"{model.ElementIdPrefix}_isNullCheckBox";
            ViewBag.InverseCheckBoxElementID = $"{model.ElementIdPrefix}_inverseCheckBox";
            ViewBag.UnionAllCheckBoxElementID = $"{model.ElementIdPrefix}_unionAllCheckBox";
            ViewBag.RelationElementID = $"{model.ElementIdPrefix}_relation";
            ViewBag.SelectorElementID = $"{model.ElementIdPrefix}_selector";
            ViewBag.RelationTextAreaElementID = $"{model.ElementIdPrefix}_relationTextArea";

            if (model.FieldId == (int)ServiceFieldType.LastModifiedBy)
            {
                ViewBag.Users = _articleSearchService.GetAllUsersList().Select(u => new QPSelectListItem
                {
                    Text = u.LogOn,
                    Value = u.Id.ToString(),
                    Selected = model.Ids?.Contains(u.Id) ?? false
                }).ToArray();

                return await JsonHtml("UserRelation", null);
            }

            if (model.FieldId == (int)ServiceFieldType.StatusType)
            {
                ViewBag.StatusTypes = _articleSearchService.GetStatusListByContentId(model.ParentEntityId)
                    .Select(s => new QPSelectListItem
                {
                    Text = s.DisplayName,
                    Value = s.Id.ToString(),
                    Selected = model.Ids?.Contains(s.Id) ?? false
                }).ToArray();

                return await JsonHtml("StatusTypeRelation", null);
            }

            var field = _articleSearchService.GetFieldByID(model.FieldId);
            if (field == null)
            {
                return Json(new { success = true, view = string.Empty });
            }

            ViewBag.ItemList = model.Ids?.Length > 0 ? _articleSearchService.GetSimpleList(field, model.Ids) : Enumerable.Empty<ListItem>();
            ViewBag.Field = field;
            ViewBag.HasRelatedHierarchyContent = field.RelatedToContent != null && field.RelatedToContent.Fields.Any(f => f.UseForTree);

            return await JsonHtml("RelationSearch", null);
        }

        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult DefaultFilterStates(string actionCode, int contentId)
        {
            var filterStates = _articleSearchService.GetDefaultFilterStates(actionCode, contentId);
            return Json(new { success = true, filterStates });
        }

        /// <summary>
        /// Возвращает разметку для блока переключения контекста.
        /// </summary>
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public async Task<ActionResult> ContextBlock(int id, string actionCode, string hostId)
        {
            var model = new ContextBlockViewModel(id, actionCode, hostId);
            return await JsonHtml("ContextBlock", model);
        }

        private void SetDateOrTimeRangeViewBag(string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.DateFromElementID = $"{elementIdPrefix}_dateFrom";
            ViewBag.DateToElementID = $"{elementIdPrefix}_dateTo";
            ViewBag.IsNullCheckBoxElementID = $"{elementIdPrefix}_isNullCheckBox";
            ViewBag.DisablingContainerElementID = $"{elementIdPrefix}_disablingContainer";
            ViewBag.ByValueElementID = $"{elementIdPrefix}_byValueSelector";
        }
    }
}
