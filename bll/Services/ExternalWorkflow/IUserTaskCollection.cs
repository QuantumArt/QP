using System;
using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public interface IUserTaskCollection
{
    List<Type> UserTasks { get; }

    void Register<TUserTask>()
        where TUserTask : AbstractUserTask;
}
