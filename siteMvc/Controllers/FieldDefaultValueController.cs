using System.Web.Mvc;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class FieldDefaultValueController : QPController
    {
        private readonly IFieldDefaultValueService _service;

        public FieldDefaultValueController(IFieldDefaultValueService service)
        {
            _service = service;
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ApplyFieldDefaultValue)]
        [BackendActionContext(ActionCode.ApplyFieldDefaultValue)]
        [BackendActionLog]
        public ActionResult PreAction(int parentId, int id) => Json(_service.PreAction(id));

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ApplyFieldDefaultValue)]
        public ActionResult Setup(int parentId, int id)
        {
            var settings = _service.SetupAction(parentId, id);
            return Json(settings);
        }

        [HttpPost]
        [ExceptionResult(ExceptionResultMode.OperationAction)]
        [ActionAuthorize(ActionCode.ApplyFieldDefaultValue)]
        [ConnectionScope]
        public ActionResult Step(int step)
        {
            var stepResult = _service.Step(step);
            return Json(stepResult);
        }

        [HttpPost]
        public ActionResult TearDown(bool isError)
        {
            _service.TearDown();
            return null;
        }
    }
}
