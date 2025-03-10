using System;
using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public class UserTaskCollection : IUserTaskCollection
{
    public List<Type> UserTasks { get; } = new();

    public void Register<TUserTask>()
        where TUserTask : AbstractUserTask
    {
        UserTasks.Add(typeof(TUserTask));
    }
}
