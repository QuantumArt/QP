using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using QA.Workflow.Extensions;
using QA.Workflow.Models;
using QA.Workflow.TaskWorker.Interfaces;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.ExternalTasks;

public class UpdateProcessStatus : IExternalTaskHandler
{
    private const string StatusParameter = "NewStatus";

    public Task<Dictionary<string, object>> Handle(string taskKey, ProcessInstanceData processInstance)
    {
        string newStatus = processInstance.GetVariableByName<string>(StatusParameter);

        QPContext.SetConnectionInfo(processInstance.TenantId);

        ExternalWorkflowRepository.UpdateStatus(processInstance.Id, newStatus);

        return Task.FromResult<Dictionary<string, object>>(new());
    }
}
