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
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.ExternalWorkflow;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

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
    [ActionAuthorize(ActionCode.GetExternalWorkflowUserTask)]
    [BackendActionContext(ActionCode.GetExternalWorkflowUserTask)]
    public async Task<IActionResult> GetUserTask(string tabId, int parentId, [FromQuery(Name = "TaskId")] string taskId)
    {
        AbstractUserTask userTask = await _externalWorkflowService.GetUserTaskHandler(taskId);
        UserTaskBase result = userTask.GetUserTaskForm();
        return await JsonHtml(result.ViewName, GetViewModel(tabId, parentId, result.GetType()));
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

    private static UserTaskBaseViewModel GetViewModel(string tabId, int parentId, Type taskType)
    {
        var data = new ExternalWorkflowTask();
        // Replace it with an automapper or something before it turns into a mess.
        return taskType.Name switch
        {
            nameof(FillArticleDto) => EntityViewModel.Create<FillArticleViewModel>(data, tabId, parentId),
            nameof(ApprovalDto) => EntityViewModel.Create<ApproveViewModel>(data, tabId, parentId),
            _ => throw new ArgumentException($"Unsupported user task type {taskType.Name}")
        };
    }
}
