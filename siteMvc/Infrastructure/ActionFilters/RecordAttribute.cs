using System;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.Helpers;
//using Quantumart.QP8.WebMvc.Infrastructure.Helpers.XmlDbUpdate;

namespace Quantumart.QP8.WebMvc.Infrastructure.ActionFilters
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

        public RecordAttribute(string actionCode)
            : this()
        {
            _code = actionCode;
        }

        public RecordAttribute(string actionCode, bool ignoreForm)
            : this(actionCode)
        {
            _ignoreForm = ignoreForm;
        }

        // TODO: uncomment RecordAttribute code and port it to AspNetCore
        //public override void OnActionExecuting(ActionExecutingContext filterContext)
        //{
        //    var db = DbService.ReadSettings();
        //    if (db.RecordActions && db.SingleUserId != QPContext.CurrentUserId)
        //    {
        //        throw new Exception(DBStrings.SingeUserModeMessage);
        //    }

        //    base.OnActionExecuting(filterContext);
        //}

        //public override void OnActionExecuted(ActionExecutedContext filterContext)
        //{
        //    try
        //    {
        //        var controller = (Controller)filterContext.Controller;
        //        var isValid = filterContext.Exception == null && controller.ViewData.ModelState.IsValid && !QPController.IsError(filterContext.HttpContext);
        //        if (isValid && DbRepository.Get().RecordActions)
        //        {
        //            var currentDbVersion = new ApplicationInfoRepository().GetCurrentDbVersion();
        //            var actionToSerialize = XmlDbUpdateHttpContextHelpers.CreateXmlDbUpdateActionFromHttpContext(filterContext.HttpContext, _code ?? BackendActionContext.Current.ActionCode, _ignoreForm);
        //            XmlDbUpdateSerializerHelpers
        //                .SerializeAction(actionToSerialize, currentDbVersion, CommonHelpers.GetBackendUrl(filterContext.HttpContext))
        //                .Save(QPContext.GetRecordXmlFilePath(), SaveOptions.None);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("There was an error while recording xml actions", ex);
        //    }

        //    base.OnActionExecuted(filterContext);
        //}
    }
}
