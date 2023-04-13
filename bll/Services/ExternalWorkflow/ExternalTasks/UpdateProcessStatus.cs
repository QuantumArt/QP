using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QA.Workflow.Extensions;
using QA.Workflow.Models;
using QA.Workflow.TaskWorker.Interfaces;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.ExternalTasks;

public class UpdateProcessStatus : IExternalTaskHandler
{
    private const string StatusParameter = "NewStatus";

    public Task<Dictionary<string, object>> Handle(string taskKey, ProcessInstanceData processInstance)
    {
        string newStatus = processInstance.GetVariableByName<string>(StatusParameter);

        QpConnectionInfo cnnInfo = QPConfiguration.GetConnectionInfo(processInstance.TenantId);

        if (cnnInfo == null)
        {
            throw new InvalidOperationException($"Unable find connection info fot tenant {processInstance.TenantId}");
        }

        QPContext.CurrentDbConnectionInfo = cnnInfo;

        ExternalWorkflowDAL workflow = QPContext.EFContext.ExternalWorkflowSet.Single(w => w.ProcessId == processInstance.Id);

        ExternalWorkflowStatusDAL status = new()
        {
            ExternalWorkflowId = workflow.Id,
            Created = DateTime.Now,
            Status = newStatus,
            CreatedBy = UserRepository.GetById(SpecialIds.AdminUserId).LogOn
        };

        ExternalWorkflowStatusDAL savedStatus = DefaultRepository.SimpleSave(status);

        if (savedStatus is not { Id: > 0 })
        {
            throw new InvalidOperationException("Unable to save new status to statuses table");
        }

        ExternalWorkflowInProgressDAL progress = QPContext.EFContext.ExternalWorkflowInProgressSet
           .Single(p => p.ProcessId == workflow.Id);

        progress.CurrentStatus = savedStatus.Id;
        progress.LastModifiedBy = SpecialIds.AdminUserId;

        ExternalWorkflowInProgressDAL savedProgress = DefaultRepository.SimpleUpdate(progress);

        if (savedProgress == null)
        {
            throw new InvalidOperationException("Unable to update process current status");
        }

        return Task.FromResult<Dictionary<string, object>>(new());
    }
}
