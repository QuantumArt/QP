using System.Threading.Tasks;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models.UserTaskModels;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public interface IUserTaskHandler
{
    UserTaskBase GetUserTaskForm();
    Task CompleteUserTask(string taskId, string taskResult);
}
