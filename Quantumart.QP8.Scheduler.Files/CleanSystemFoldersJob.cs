﻿using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.FileSynchronization;
using Quantumart.QP8.CommonScheduler;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quartz;

namespace Quantumart.QP8.Scheduler.Files;

[DisallowConcurrentExecution]
[PersistJobDataAfterExecution]
public class CleanSystemFoldersJob : IJob
{
    private const string MaxNumberOfFilesKey = "MaxNumberOfFilesPerRun";

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly ISchedulerCustomerCollection _schedulerCustomers;
    private readonly ICleanSystemFoldersService _service;

    public CleanSystemFoldersJob(
        ISchedulerCustomerCollection schedulerCustomers,
        ICleanSystemFoldersService service
    )
    {
        _schedulerCustomers = schedulerCustomers;
        _service = service;
    }

    public Task Execute(IJobExecutionContext context)
    {
        Logger.Info($"Start cleaning system folders");
        var customers = _schedulerCustomers.GetItems();

        var dataMap = context.MergedJobDataMap;
        customers = JobHelpers.FilterCustomers(customers, dataMap);
        var limit = dataMap.GetIntValue(MaxNumberOfFilesKey);

        if (limit <= 0)
        {
            Logger.Warn($"Task is cancelled because maximimum number of files to process is {limit}");
            return Task.CompletedTask;
        }

        foreach (var customer in customers)
        {
            try
            {
                if (!context.CancellationToken.IsCancellationRequested && limit > 0)
                {
                    limit = ProcessCustomer(customer, limit);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("CustomerCode", customer.CustomerName);
                Logger.Error(ex, $"There was an error on customer code: {customer.CustomerName}", ex);
            }
        }

        Logger.Info($"Finished cleaning system folders");
        return Task.CompletedTask;
    }

    private int ProcessCustomer(QaConfigCustomer customer, int limit)
    {
        using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
        {
            limit = _service.CleanSystemFolders(customer.CustomerName, limit);
        }

        return limit;
    }
}
