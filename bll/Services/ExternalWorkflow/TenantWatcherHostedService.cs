using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QA.Workflow.QP_DPC;
using QA.Workflow.QP_DPC.Models;
using QA.Workflow.TaskWorker.Models;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public class TenantWatcherHostedService : WorkflowTenantWatcher
{
    private readonly IServiceProvider _serviceProvider;

    public TenantWatcherHostedService(ILogger<WorkflowTenantWatcher> logger,
        ExtendedCamundaSettings settings,
        WorkflowTenants tenants,
        IServiceProvider serviceProvider)
        : base(logger, settings, tenants)
    {
        _serviceProvider = serviceProvider;
    }

    public override Task<List<string>> LoadCustomerCodes()
    {
        List<string> customers =  QPConfiguration.GetCustomerCodes();

        return Task.FromResult(customers);
    }

    public override Task<bool> IsExternalWorkflowEnabled(string customerCode)
    {
        QpConnectionInfo cnnInfo = QPConfiguration.GetConnectionInfo(customerCode);
        IServiceScope scope = _serviceProvider.CreateScope();
        IHttpContextAccessor contextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
        contextAccessor.HttpContext?.Items.Add(HttpContextItems.CurrentDbConnectionStringKey, cnnInfo);

        return Task.FromResult(ExternalWorkflowService.IsExternalWorkflowEnabled());
    }
}
