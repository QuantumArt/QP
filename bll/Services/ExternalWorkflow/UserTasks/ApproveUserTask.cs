using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.UserTasks;

public class ApproveUserTask : AbstractUserTask
{
    // Required variables
    private const string ApproveResultVariable = "ApproveResult";
    private const string ApproveMessageVariable = "ApproveMessage";

    public ApproveUserTask(IExternalWorkflowService externalWorkflow)
        : base(externalWorkflow)
    {
    }

    public override UserTaskBase GetUserTaskForm() =>
        new ApprovalDto
        {
            ViewName = "../ExternalWorkflow/Approve"
        };

    public override async Task CompleteUserTask(string taskId, string taskResult)
    {
        ApprovalDto result = JsonConvert.DeserializeObject<ApprovalDto>(taskResult);
        Dictionary<string, object> variables = await ExternalWorkflow.GetTaskVariables(taskId);
        string resultVariable = ExternalWorkflow.GetVariable<string>(variables, ApproveResultVariable);
        string messageVariable = ExternalWorkflow.GetVariable<string>(variables, ApproveMessageVariable);

        Dictionary<string, object> completionVariables = GetDictionaryWithExecutedUser(variables);
        completionVariables.Add(resultVariable, result.Approved);
        completionVariables.Add(messageVariable, result.Message);

        await ExternalWorkflow.CompleteUserTask(taskId, completionVariables);
    }
}
