using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QP8.Infrastructure.Web.Enums;
using Quantumart.QP8.Constants;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.Infrastructure.ActionFilters;
using Quantumart.QP8.WebMvc.Infrastructure.Enums;
using Quantumart.QP8.WebMvc.ViewModels.ExternalWorkflowUserTask;
using QP8.Infrastructure.Web.Responses;
using Quantumart.QP8.BLL.Services.ExternalWorkflow;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;

namespace Quantumart.QP8.WebMvc.Controllers;

public class ExternalWorkflowUserActionsController : AuthQpController
{
    private readonly IExternalWorkflowService _externalWorkflowService;

    public ExternalWorkflowUserActionsController(IExternalWorkflowService externalWorkflowService)
    {
        _externalWorkflowService = externalWorkflowService;
    }

    [HttpGet]
    [ExceptionResult(ExceptionResultMode.UiAction)]
    [ActionAuthorize(ActionCode.GetExternalWorkflowUserTasks)]
    [BackendActionContext(ActionCode.GetExternalWorkflowUserTasks)]
    public async Task<IActionResult> GetUserTask([FromQuery(Name = "TaskId")] string taskId)
    {
        AbstractUserTask userTask = await _externalWorkflowService.GetUserTaskHandler(taskId);
        UserTaskBase result = userTask.GetUserTaskForm();

        return await JsonHtml(result.ViewName, GetViewModel(result.GetType()));
    }

    [HttpPost]
    [ExceptionResult(ExceptionResultMode.OperationAction)]
    [ActionAuthorize(ActionCode.CompleteExternalWorkflowUserTask)]
    [BackendActionContext(ActionCode.CompleteExternalWorkflowUserTask)]
    public async Task<IActionResult> CompleteUserTask([FromQuery(Name = "TaskId")] string taskId)
    {
        AbstractUserTask userTask = await _externalWorkflowService.GetUserTaskHandler(taskId);

        using StreamReader reader = new(Request.Body);
        string body = await reader.ReadToEndAsync();

        await userTask.CompleteUserTask(taskId, body);

        return Json(new JSendResponse { Status = JSendStatus.Success });
    }

    private static UserTaskBaseViewModel GetViewModel(Type taskType)
    {
        // Replace it with an automapper or something before it turns into a mess.
        return taskType.Name switch
        {
            nameof(FillArticleDto) => new FillArticleViewModel(),
            nameof(ApprovalDto) => new ApproveViewModel(),
            _ => throw new ArgumentException($"Unsupported user task type {taskType.Name}")
        };
    }
}
