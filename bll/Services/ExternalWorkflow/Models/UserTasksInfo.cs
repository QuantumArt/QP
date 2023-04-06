using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.Models;

public class UserTasksInfo
{
    public int TotalCount { get; set; }
    public List<UserTaskData> Data { get; set; } = new();
}
