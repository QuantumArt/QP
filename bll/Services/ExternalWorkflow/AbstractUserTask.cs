using System.Collections.Generic;
using System.Threading.Tasks;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public abstract class AbstractUserTask
{
    protected readonly IExternalWorkflowService ExternalWorkflow;

    private const string ExecutedUser = "ExecutedUserInfo";

    protected AbstractUserTask(IExternalWorkflowService externalWorkflow)
    {
        ExternalWorkflow = externalWorkflow;
    }

    public abstract UserTaskBase GetUserTaskForm();
    public abstract Task CompleteUserTask(string taskId, string taskResult);

    protected Dictionary<string, object> GetDictionaryWithExecutedUser(Dictionary<string, object> variables)
    {
        string userParameter = ExternalWorkflow.GetVariable<string>(variables, ExecutedUser);

        return new() { { userParameter, QPContext.CurrentUserName } };
    }
}
