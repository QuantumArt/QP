using System;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Security;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Extensions.Helpers;

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
                var action = new RecordedAction();
                var form = filterContext.HttpContext.Request.Form;
                if (form != null && !_ignoreForm)
                {
                    action.Form = form;
                }

                action.Code = _code ?? BackendActionContext.Current.ActionCode;
                action.ParentId = BackendActionContext.Current.ParentEntityId.HasValue ? BackendActionContext.Current.ParentEntityId.Value : 0;
                action.Lcid = CultureInfo.CurrentCulture.LCID;
                action.Executed = DateTime.Now;
                action.ExecutedBy = (filterContext.HttpContext.User.Identity as QPIdentity)?.Name;

                var fromId = filterContext.HttpContext.Items.Contains("FROM_ID") ? (int)filterContext.HttpContext.Items["FROM_ID"] : 0;
                action.Ids = fromId != 0 ? new[] { fromId.ToString() } : BackendActionContext.Current.Entities.Select(n => n.StringId).ToArray();

                var helper = DependencyResolver.Current.GetService<IRecordHelper>();
                helper.PersistAction(action, filterContext.HttpContext);
            }

            base.OnActionExecuted(filterContext);
        }
    }
}
