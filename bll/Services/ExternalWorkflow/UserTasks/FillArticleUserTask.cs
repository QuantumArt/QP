using System;
using System.Threading.Tasks;
using QA.Workflow.Interfaces;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.UserTasks;

public class FillArticleUserTask : IUserTaskHandler
{
    private readonly IExternalWorkflowService _externalWorkflow;

    public FillArticleUserTask(IExternalWorkflowService externalWorkflow)
    {
        _externalWorkflow = externalWorkflow;
    }

    public UserTaskBase GetUserTaskForm() =>
        new FillArticleDto
        {
            ViewName = "../ExternalWorkflow/FillArticle"
        };

    public async Task CompleteUserTask(string taskId, string taskResult)
    {
        await _externalWorkflow.CompleteUserTask(taskId, null);
    }
}
