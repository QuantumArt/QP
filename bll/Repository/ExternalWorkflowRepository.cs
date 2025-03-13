using System;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.BLL.Exceptions;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Repository;

public class ExternalWorkflowRepository
{
    private const string NewWorkflowStatusName = "Процесс запущен";

    public static void UpdateStatus(string processId, string newStatus)
    {
        ExternalWorkflowDAL workflow = QPContext.EFContext.ExternalWorkflowSet.SingleOrDefault(w => w.ProcessId == processId);

        if (workflow == null)
        {
            throw new ExternalWorkflowNotFoundInDbException($"Workflow with process id {processId} not found in database");
        }

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
            .Single(p => p.ExternalWorkflowId == workflow.Id);

        progress.CurrentStatusId = savedStatus.Id;

        ExternalWorkflowInProgressDAL savedProgress = DefaultRepository.SimpleUpdate(progress);

        if (savedProgress == null)
        {
            throw new InvalidOperationException("Unable to update process current status");
        }
    }

    public static void SaveStartedWorkflowInfoToDb(string processId,
        string workflowName,
        string articleName,
        int workflowId,
        int contentItemId
    )
    {
        DateTime now = DateTime.Now;
        string createdBy = UserRepository.GetById(QPContext.CurrentUserId).LogOn;

        ExternalWorkflowDAL workflowEntity = new()
        {
            Created = now,
            CreatedBy = createdBy,
            ProcessId = processId,
            WorkflowName = workflowName,
            ArticleName = articleName,
            ArticleId = contentItemId,
            WorkflowId = workflowId,
        };

        ExternalWorkflowDAL createdWorkflow = DefaultRepository.SimpleSave(workflowEntity);

        if (createdWorkflow is not { Id: > 0 })
        {
            throw new InvalidOperationException("Unable to save process info to DB.");
        }

        ExternalWorkflowStatusDAL workflowStatus = new()
        {
            Created = now,
            CreatedBy = createdBy,
            Status = NewWorkflowStatusName,
            ExternalWorkflowId = createdWorkflow.Id
        };

        ExternalWorkflowStatusDAL createWorkflowStatus = DefaultRepository.SimpleSave(workflowStatus);

        if (createWorkflowStatus is not { Id: > 0 })
        {
            throw new InvalidOperationException("Unable to save process status to DB.");
        }

        ExternalWorkflowInProgressDAL workflowProgress = new()
        {
            ExternalWorkflowId = createdWorkflow.Id,
            CurrentStatusId = createWorkflowStatus.Id
        };

        ExternalWorkflowInProgressDAL createdWorkflowProgress = DefaultRepository.SimpleSave(workflowProgress);

        if (createdWorkflowProgress is not { Id: > 0 })
        {
            throw new InvalidOperationException("Unable to create process progress in DB.");
        }
    }


    public static void DeleteProcess(decimal process)
    {
        ExternalWorkflowInProgressDAL inProcess = QPContext.EFContext.ExternalWorkflowInProgressSet
            .SingleOrDefault(x => x.ExternalWorkflowId == process);

        if (inProcess != null)
        {
            DefaultRepository.Delete<ExternalWorkflowInProgressDAL>((int)inProcess.Id);
        }
    }

    public static int SaveStatus(decimal process, string status)
    {
        ExternalWorkflowStatusDAL newStatus = new()
        {
            Created = DateTime.Now,
            Status = status,
            ExternalWorkflowId = process,
            CreatedBy = UserRepository.GetById(SpecialIds.AdminUserId).LogOn
        };

        ExternalWorkflowStatusDAL savedStatus = DefaultRepository.SimpleSave(newStatus);
        return savedStatus.Id;
    }

    public static string GetProcessId(int id)
    {
        return QPContext.EFContext.ExternalWorkflowSet.Single(x => x.Id == id).ProcessId;
    }

    public static int GetId(string processId)
    {
        return (int)QPContext.EFContext.ExternalWorkflowSet.Single(x => x.ProcessId == processId).Id;
    }

    public static List<int> GetProcessIds()
    {
        return QPContext.EFContext.ExternalWorkflowInProgressSet.Select(x => (int)x.ExternalWorkflowId).ToList();
    }

    public static Dictionary<string, int> GetExistingWorkflowIdsByProcessIds(string[] processesIds)
    {
        return QPContext.EFContext.ExternalWorkflowSet
            .Where(x => processesIds.Contains(x.ProcessId))
            .ToDictionary(x => x.ProcessId, x => (int)x.Id);
    }
}
