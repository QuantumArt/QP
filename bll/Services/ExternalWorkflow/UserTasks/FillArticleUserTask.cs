using System.Collections.Generic;
using System.Threading.Tasks;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.UserTasks;

public class FillArticleUserTask : AbstractUserTask
{
    public FillArticleUserTask(IExternalWorkflowService externalWorkflow)
        : base(externalWorkflow)
    {
    }

    public override UserTaskBase GetUserTaskForm() =>
        new FillArticleDto
        {
            ViewName = "../ExternalWorkflow/FillArticle"
        };

    public override async Task CompleteUserTask(string taskId, string taskResult)
    {
        Dictionary<string, object> variables = await ExternalWorkflow.GetTaskVariables(taskId);
        Dictionary<string, object> completionVariables = GetDictionaryWithExecutedUser(variables);
        await ExternalWorkflow.CompleteUserTask(taskId, completionVariables);
    }
}
