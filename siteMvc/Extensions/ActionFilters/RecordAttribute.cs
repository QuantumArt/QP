using System;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
using Quantumart.QP8.WebMvc.Infrastructure.Services.XmlDbUpdate;
using Quantumart.QP8.WebMvc.Infrastructure.Constants.XmlDbUpdate;

namespace Quantumart.QP8.WebMvc.Extensions.ActionFilters
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RecordAttribute : ActionFilterAttribute
    {
        public static readonly int FilterOrder = BackendActionLogAttribute.FilterOrder + 1;

        private readonly string _code;
        private readonly bool _ignoreForm;

        public RecordAttribute()
        {
            Order = FilterOrder;
        }

        public RecordAttribute(string actionCode) : this()
        {
            _code = actionCode;
        }

        public RecordAttribute(string actionCode, bool ignoreForm) : this(actionCode)
        {
            _ignoreForm = ignoreForm;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var db = DbService.ReadSettings();
            if (db.RecordActions && db.SingleUserId != QPContext.CurrentUserId)
            {
                throw new Exception(DBStrings.SingeUserModeMessage);
            }

            base.OnActionExecuting(filterContext);
        }

        public override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            var isValid = filterContext.Exception == null && filterContext.Controller.ViewData.ModelState.IsValid && !QPController.IsError(filterContext.HttpContext);
            if (isValid && DbRepository.Get().RecordActions)
            {
                var actionToSerialize = XmlDbUpdateActionCorrectionService.CreateActionFromHttpContext(filterContext.HttpContext, _code ?? BackendActionContext.Current.ActionCode, _ignoreForm);
                XmlDbUpdateSerializerHelpers
                    .SerializeAction(actionToSerialize, CommonHelpers.GetBackendUrl(filterContext.HttpContext))
                    .Save(XmlDbUpdateXDocumentConstants.XmlFilePath);
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
