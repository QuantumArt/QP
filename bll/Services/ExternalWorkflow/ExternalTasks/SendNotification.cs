using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QA.Workflow.Extensions;
using QA.Workflow.Integration.QP.Models;
using QA.Workflow.Models;
using QA.Workflow.TaskWorker.Interfaces;
using QP.ConfigurationService.Models;
using Quantumart.QP8.Configuration;
using Quantumart.QPublishing.Database;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow.ExternalTasks;

public class SendNotification : IExternalTaskHandler
{
    private const string NotificationIds = "NotificationIds";
    private const string NotificationsSeparator = ",";

    public Task<Dictionary<string, object>> Handle(string taskKey, ProcessInstanceData processInstance)
    {
        int contentItemId = processInstance.GetVariableByName<int>(ExternalWorkflowQpDpcSettings.ContentItemId);
        string notificationIdsString = processInstance.GetVariableByName<string>(NotificationIds);

        int[] notificationIds = notificationIdsString.Split(NotificationsSeparator)
           .Select(
               x => int.TryParse(x, out int id)
               ? id
               : throw new InvalidOperationException($"Unable to parse notification id value {x} as int")
           )
           .ToArray();

        QpConnectionInfo cnnInfo = QPConfiguration.GetConnectionInfo(processInstance.TenantId);

        if (cnnInfo == null)
        {
            throw new InvalidOperationException($"Unable find connection info fot tenant {processInstance.TenantId}");
        }

        DBConnector connector = new(cnnInfo.ConnectionString, (DatabaseType)cnnInfo.DbType);
        QPConfiguration.SetAppSettings(connector.DbConnectorSettings);
        connector.SendNotificationById(contentItemId, notificationIds);

        return Task.FromResult<Dictionary<string, object>>(new());
    }
}
