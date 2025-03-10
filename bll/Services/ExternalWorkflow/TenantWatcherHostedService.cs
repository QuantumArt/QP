using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Workflow.Integration.QP;
using QA.Workflow.Integration.QP.Models;
using QA.Workflow.TaskWorker.Models;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public class TenantWatcherHostedService : WorkflowTenantWatcher
{
    public TenantWatcherHostedService(ILogger<WorkflowTenantWatcher> logger,
        IOptions<ExtendedCamundaSettings> settings,
        WorkflowTenants tenants)
        : base(logger, settings.Value, tenants)
    {
    }

    public override Task<List<string>> LoadCustomerCodes()
    {
        List<string> customers =  QPConfiguration.GetCustomerCodes();

        return Task.FromResult(customers);
    }

    public override Task<bool> IsExternalWorkflowEnabled(string customerCode)
    {
        QpConnectionInfo cnnInfo = QPConfiguration.GetConnectionInfo(customerCode);
        QPContext.CurrentDbConnectionInfo = cnnInfo;

        return Task.FromResult(ExternalWorkflowService.IsExternalWorkflowEnabled());
    }
}
