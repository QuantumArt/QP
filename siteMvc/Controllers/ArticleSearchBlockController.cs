using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.WebMvc.Extensions.ActionFilters;
using Quantumart.QP8.WebMvc.Extensions.ActionResults;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.Article;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class ArticleSearchBlockController : QPController
    {
        private readonly IArticleSearchService _articleSearchService;

        public ArticleSearchBlockController(IArticleSearchService articleSearchService)
        {
            _articleSearchService = articleSearchService;
        }

        /// <summary>
        /// Возвращает содержимое блока полнотекстового поиска
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult FullTextBlock(int parentEntityId, string elementIdPrefix)
        {

            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.TextFieldsComboID = $"{elementIdPrefix}_TextFieldsCombo";
            ViewBag.QueryTextBoxID = $"{elementIdPrefix}_QueryTextBox";

            // Получить список "текстовых" полей
            var fieldList = _articleSearchService.GetFullTextSearchableFieldGroups(parentEntityId);
            if (!fieldList.Any())
            {
                return new JsonNetResult<object>(new { success = true, view = string.Empty });
            }

            ViewBag.SearchableFieldList = fieldList;
            return JsonHtml("FullTextBlock", null);
        }

        /// <summary>
        /// Возвращает содержимое блока поиска по всем полям
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult FieldSearchBlock(int parentEntityId, string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.FieldSearchListElementID = string.Format("{0}_FieldSearchList", ViewBag.ElementIdPrefix);
            ViewBag.FieldSearchSelectorElementID = string.Format("{0}_FieldSearchSelector", ViewBag.ElementIdPrefix);
            ViewBag.FieldsComboID = string.Format("{0}_FieldsCombo", ViewBag.ElementIdPrefix);
            ViewBag.AddFieldSearchElementID = string.Format("{0}_AddFieldSearchButton", ViewBag.ElementIdPrefix);
            ViewBag.SearchableFieldList = _articleSearchService.GetSearchableFieldFieldGroups(parentEntityId);
            return JsonHtml("FieldSearchBlock", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по идентификатору
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult Identifier(string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.NumberFromElementID = string.Format("{0}_numberFrom", ViewBag.ElementIdPrefix);
            ViewBag.NumberToElementID = string.Format("{0}_numberTo", ViewBag.ElementIdPrefix);
            ViewBag.InverseCheckBoxElementID = string.Format("{0}_inverseCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.DisablingContainerElementID = string.Format("{0}_disablingContainer", ViewBag.ElementIdPrefix);
            ViewBag.ByValueElementID = string.Format("{0}_byValueSelector", ViewBag.ElementIdPrefix);
            ViewBag.TextElementID = $"{elementIdPrefix}_text";
            return JsonHtml("Identifier", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по полю c датой
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult DateRange(string elementIdPrefix)
        {
            SetDateOrTimeRangeViewBag(elementIdPrefix);
            return JsonHtml("DateRange", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по полю c временем
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult TimeRange(string elementIdPrefix)
        {
            SetDateOrTimeRangeViewBag(elementIdPrefix);
            return JsonHtml("TimeRange", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по полю c датой-временем
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult DateTimeRange(string elementIdPrefix)
        {
            SetDateOrTimeRangeViewBag(elementIdPrefix);
            return JsonHtml("DateTimeRange", null);
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

        /// <summary>
        /// Возвращает разметку для блока поиска по числовому полю
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult NumericRange(string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.NumberFromElementID = string.Format("{0}_numberFrom", ViewBag.ElementIdPrefix);
            ViewBag.NumberToElementID = string.Format("{0}_numberTo", ViewBag.ElementIdPrefix);
            ViewBag.IsNullCheckBoxElementID = string.Format("{0}_isNullCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.InverseCheckBoxElementID = string.Format("{0}_inverseCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.DisablingContainerElementID = string.Format("{0}_disablingContainer", ViewBag.ElementIdPrefix);
            ViewBag.ByValueElementID = string.Format("{0}_byValueSelector", ViewBag.ElementIdPrefix);
            return JsonHtml("NumericRange", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по текстовому полю
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult TextSearch(string elementIdPrefix)
        {
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.TextBoxElementId = string.Format("{0}_textBox", ViewBag.ElementIdPrefix);
            ViewBag.IsNullCheckBoxElementID = string.Format("{0}_isNullCheckBox", ViewBag.ElementIdPrefix);
            ViewBag.InverseCheckBoxElementID = string.Format("{0}_inverseCheckBox", ViewBag.ElementIdPrefix);
            return JsonHtml("TextSearch", null);
        }

        /// <summary>
        /// Возвращает разметку для блока поиска по строковому перечислению
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult StringEnum(string elementIdPrefix, int fieldID)
        {
            ViewBag.QueryDropDownListID = elementIdPrefix + "_queryDropDownList";
            ViewBag.IsNullCheckBoxID = elementIdPrefix + "_isNullCheckBox";
            var model = FieldService.Read(fieldID).StringEnumItems.Select(o => new QPSelectListItem { Value = o.Value, Text = o.Alias }).ToList();
            return JsonHtml("StringEnum", model);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult ContentsListForClassifier(string elementIdPrefix, int fieldID)
        {
            var classifier = FieldService.Read(fieldID);
            ViewBag.ElementIdPrefix = elementIdPrefix;
            ViewBag.ContentElementID = string.Format("{0}_contentID", ViewBag.ElementIdPrefix);
            ViewBag.IsNullCheckBoxElementID = string.Format("{0}_isNullCheckBox", ViewBag.ElementIdPrefix);
            IEnumerable<QPSelectListItem> model = FieldService.GetAggregetableContentsForClassifier(classifier).Select(o => new QPSelectListItem { Value = o.Value, Text = o.Text }).ToList();
            return JsonHtml("ContentsListForClassifier", model);
        }

        /// <summary>
        /// Разметка блока поиска по связям
        /// </summary>
        [HttpPost]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public ActionResult RelationSearch(int parentEntityId, int fieldID, int[] IDs, string elementIdPrefix)
        {
            IDs = IDs ?? new int[0];

            ViewBag.IsNullCheckBoxElementID = $"{elementIdPrefix}_isNullCheckBox";
            ViewBag.InverseCheckBoxElementID = $"{elementIdPrefix}_inverseCheckBox";
            ViewBag.RelationElementID = $"{elementIdPrefix}_relation";
            ViewBag.SelectorElementID = $"{elementIdPrefix}_selector";
            ViewBag.RelationTextAreaElementID = $"{elementIdPrefix}_relationTextArea";

            if (fieldID == (int)ServiceFieldType.LastModifiedBy)
            {
                ViewBag.Users = _articleSearchService.GetAllUsersList().Select(u => new QPSelectListItem
                {
                    Text = u.LogOn,
                    Value = u.Id.ToString(),
                    Selected = IDs.Contains(u.Id)
                }).ToArray();

                return JsonHtml("UserRelation", null);
            }

            if (fieldID == (int)ServiceFieldType.StatusType)
            {
                ViewBag.StatusTypes = _articleSearchService.GetStatusListByContentId(parentEntityId).Select(s => new QPSelectListItem
                {
                    Text = s.Name,
                    Value = s.Id.ToString(),
                    Selected = IDs.Contains(s.Id)
                }).ToArray();

                return JsonHtml("StatusTypeRelation", null);
            }

            var field = _articleSearchService.GetFieldByID(fieldID);
            if (field == null)
            {
                return new JsonNetResult<object>(new { success = true, view = string.Empty });
            }

            ViewBag.ItemList = IDs.Length > 0 ? _articleSearchService.GetSimpleList(field, IDs) : Enumerable.Empty<ListItem>();
            ViewBag.Field = field;
            ViewBag.HasRelatedHierarchyContent = field.RelatedToContent != null && field.RelatedToContent.Fields.Any(f => f.UseForTree);

            return JsonHtml("RelationSearch", null);
        }

        [HttpGet]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        public ActionResult DefaultFilterStates(string actionCode, int contentId)
        {
            var filterStates = _articleSearchService.GetDefaultFilterStates(actionCode, contentId);
            return new JsonResult
            {
                Data = new
                {
                    success = true,
                    filterStates
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        /// <summary>
        /// Возвращает разметку для блока переключения контекста
        /// </summary>
        [HttpGet]
        [ExceptionResult(ExceptionResultMode.UiAction)]
        public ActionResult ContextBlock(int id, string actionCode, string hostId)
        {
            var model = new ContextBlockViewModel(id, actionCode, hostId);
            return JsonHtml("ContextBlock", model);
        }
    }
}
