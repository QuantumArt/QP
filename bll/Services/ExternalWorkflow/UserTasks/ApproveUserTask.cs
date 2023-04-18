using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.UserTasks;

public class ApproveUserTask : IUserTaskHandler
{
    private readonly IExternalWorkflowService _externalWorkflow;

    // Required variables
    private const string ApproveResultVariable = "ApproveResult";
    private const string ApproveMessageVariable = "ApproveMessage";

    public ApproveUserTask(IExternalWorkflowService externalWorkflow)
    {
        _externalWorkflow = externalWorkflow;
    }

    public UserTaskBase GetUserTaskForm() =>
        new ApprovalDto
        {
            ViewName = "../ExternalWorkflow/Approve"
        };

    public async Task CompleteUserTask(string taskId, string taskResult)
    {
        ApprovalDto result = JsonConvert.DeserializeObject<ApprovalDto>(taskResult);
        Dictionary<string, object> variables = await _externalWorkflow.GetTaskVariables(taskId);
        string resultVariable = _externalWorkflow.GetVariable<string>(variables, ApproveResultVariable);
        string messageVariable = _externalWorkflow.GetVariable<string>(variables, ApproveMessageVariable);

        Dictionary<string, object> completionVariables = new()
        {
            { resultVariable, result.Approved },
            { messageVariable, result.Message }
        };

        await _externalWorkflow.CompleteUserTask(taskId, completionVariables);
    }
}
