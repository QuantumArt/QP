using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Workflow.Interfaces;
using QA.Workflow.TaskWorker.Models;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.Entities;

namespace Quantumart.QP8.BLL.Services.ExternalWorkflow;

public class ExternalWorkflowProcessWatcher : IHostedService
{
    private readonly IWorkflowProcessService _processService;
    private readonly WorkflowTenants _tenants;
    private readonly ExternalTaskProcessWatcherConfig _config;
    private readonly ILogger<ExternalWorkflowProcessWatcher> _logger;
    private readonly CancellationTokenSource _cancellationToken;
    private Task _executionTask;

    private const string DeadProcessStatusName = "Процесс завершен";

    public ExternalWorkflowProcessWatcher(ILogger<ExternalWorkflowProcessWatcher> logger,
        IOptions<ExternalTaskProcessWatcherConfig> config,
        WorkflowTenants tenants,
        IWorkflowProcessService processService)
    {
        _cancellationToken = new();
        _logger = logger;
        _config = config.Value;
        _tenants = tenants;
        _processService = processService;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _executionTask = Watch(_cancellationToken.Token);

        _logger.LogInformation("Workflow process alive watcher started");
        return _executionTask.IsCompleted ? _executionTask : Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping workflow process alive watcher");

        if (_executionTask == null)
        {
            return;
        }

        try
        {
            _cancellationToken.Cancel();
        }
        finally
        {
            await Task.WhenAny(_executionTask, Task.Delay(Timeout.Infinite, cancellationToken));
        }
    }

    private async Task Watch(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                foreach (string tenant in _tenants.Tenants)
                {
                    await CheckTenantProcessesAndStopDead(tenant);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while loading customer codes");
            }
            finally
            {
                await Task.Delay(_config.ProcessCheckInterval, cancellationToken);
            }
        }
    }

    private async Task CheckTenantProcessesAndStopDead(string tenant)
    {
        try
        {
            _logger.LogTrace("Checking external workflow processes for customer code {CustomerCode}", tenant);

            QpConnectionInfo cnnInfo = QPConfiguration.GetConnectionInfo(tenant);

            if (cnnInfo == null)
            {
                throw new InvalidOperationException($"Unable find connection info fot tenant {tenant}");
            }

            QPContext.CurrentDbConnectionInfo = cnnInfo;
            List<decimal> processes = QPContext.EFContext.ExternalWorkflowInProgressSet
               .Select(w => w.ProcessId)
               .ToList();

            _logger.LogTrace("For customer code {CustomerCode} was found {ProcessCount} processes", tenant, processes.Count);

            foreach (decimal process in processes)
            {
                await CheckProcessAndStopIfDead(process, tenant);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while preparing context for tenant");
        }
    }

    private async Task CheckProcessAndStopIfDead(decimal process, string tenant)
    {
        try
        {
            _logger.LogTrace("Checking process {Process} for customer code {CustomerCode}", process, tenant);

            ExternalWorkflowDAL processInfo = QPContext.EFContext.ExternalWorkflowSet
               .Single(x => x.Id == process);

            bool isAlive = await _processService.CheckProcessExistence(processInfo.ProcessId, tenant);

            if (isAlive)
            {
                _logger.LogTrace("Process {Process} is alive", process);

                return;
            }

            _logger.LogInformation("Process {Process} for customer code {CustomerCode} is dead. Removing it from active processes",
                process,
                tenant);

            StopProcessInDb(process);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while working on process {Process}", process);
        }
    }

    private void StopProcessInDb(decimal process)
    {
        try
        {
            ExternalWorkflowStatusDAL newStatus = new()
            {
                Created = DateTime.Now,
                Status = DeadProcessStatusName,
                ExternalWorkflowId = process,
                CreatedBy = UserRepository.GetById(SpecialIds.AdminUserId).LogOn
            };

            ExternalWorkflowStatusDAL savedStatus = DefaultRepository.SimpleSave(newStatus);

            if (savedStatus is not { Id: > 0 })
            {
                _logger.LogError("Failed to save new status {Status} for process {ProcessId}",
                    DeadProcessStatusName,
                    process);
            }

            ExternalWorkflowInProgressDAL inProcess = QPContext.EFContext.ExternalWorkflowInProgressSet
               .SingleOrDefault(x => x.ProcessId == process);

            if (inProcess != null)
            {
                DefaultRepository.Delete<ExternalWorkflowInProgressDAL>((int)inProcess.Id);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while changing process info in db");
        }
    }
}
