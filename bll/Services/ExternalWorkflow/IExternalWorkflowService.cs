using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UserTaskInfo =  Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTasksInfo;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public interface IExternalWorkflowService
{
    Task<bool> PublishWorkflow(string customerCode, int contentItemId, int siteId, CancellationToken token);

    Task<bool> StartProcess(string customerCode,
        int contentItemId,
        int contentId,
        CancellationToken token);

    Task<int> GetTaskCount();

    Task<UserTaskInfo> GetUserTasks(int page, int pageSize);

    Task<string> GetUserTaskKey(string taskId);

    Task<IUserTaskHandler> GetUserTaskHandler(string taskId);

    Task CompleteUserTask(string taskId, Dictionary<string, object> variables);

    T GetVariable<T>(Dictionary<string, object> variables, string name);

    Task<Dictionary<string, object>> GetTaskVariables(string taskId);
}
