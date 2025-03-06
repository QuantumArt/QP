using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QA.Workflow.Interfaces;
using QA.Workflow.TaskWorker.Models;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.ExternalWorkflow.Models;

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

            QPContext.SetConnectionInfo(tenant);

            var processes = ExternalWorkflowRepository.GetProcessIds();

            _logger.LogTrace("For customer code {CustomerCode} was found {ProcessCount} processes", tenant, processes.Count);

            foreach (var process in processes)
            {
                await CheckProcessAndStopIfDead(process, tenant);
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while preparing context for tenant");
        }
    }

    private async Task CheckProcessAndStopIfDead(int process, string tenant)
    {
        try
        {
            _logger.LogTrace("Checking process {Process} for customer code {CustomerCode}", process, tenant);

            var processId = ExternalWorkflowRepository.GetProcessId(process);
            bool isAlive = await _processService.CheckProcessExistence(processId, tenant);

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
            var result = ExternalWorkflowRepository.SaveStatus(process, DeadProcessStatusName);
            if (result <= 0)
            {
                _logger.LogError("Failed to save new status {Status} for process {ProcessId}",
                    DeadProcessStatusName,
                    process);
            }
            ExternalWorkflowRepository.DeleteProcess(process);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while changing process info in db");
        }
    }
}
