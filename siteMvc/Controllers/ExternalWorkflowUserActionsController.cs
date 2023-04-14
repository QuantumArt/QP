using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QP8.Infrastructure.Web.Enums;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.ExternalWorkflowUserTask;
using QP8.Infrastructure.Web.Responses;

namespace Quantumart.QP8.WebMvc.Controllers;

public class ExternalWorkflowUserActionsController : AuthQpController
{
    [HttpGet]
    [ExceptionResult(ExceptionResultMode.UiAction)]
    [ActionAuthorize(ActionCode.GetExternalWorkflowUserTasks)]
    [BackendActionContext(ActionCode.GetExternalWorkflowUserTasks)]
    public async Task<IActionResult> GetUserTask([FromQuery(Name = "TaskId")] string taskId)
    {
        return await JsonHtml("../ExternalWorkflow/FillArticle", new FillArticleViewModel
        {
            Message = "Вы уверены что заполнили все данные?"
        });
    }

    [HttpPost]
    [ExceptionResult(ExceptionResultMode.OperationAction)]
    [ActionAuthorize(ActionCode.CompleteExternalWorkflowUserTask)]
    [BackendActionContext(ActionCode.CompleteExternalWorkflowUserTask)]
    public async Task<IActionResult> CompleteUserTask()
    {
        string query = Request.QueryString.ToString();

        return Json(new JSendResponse { Status = JSendStatus.Success, Message = "Задача успешно выполнена."});
    }
}
