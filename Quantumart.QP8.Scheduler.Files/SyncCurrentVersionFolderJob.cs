using NLog;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services.FileSynchronization;
using Quantumart.QP8.CommonScheduler;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Scheduler.API;
using Quartz;

namespace Quantumart.QP8.Scheduler.Files;

[DisallowConcurrentExecution]
[PersistJobDataAfterExecution]
public class SyncCurrentVersionFolderJob : IJob
{
    private const string MaxNumberOfFilesKey = "MaxNumberOfFiles";

    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly ISchedulerCustomerCollection _schedulerCustomers;
    private readonly ICurrentVersionFolderService _service;

    public SyncCurrentVersionFolderJob(
        ISchedulerCustomerCollection schedulerCustomers,
        ICurrentVersionFolderService service
    )
    {
        _schedulerCustomers = schedulerCustomers;
        _service = service;
    }

    public Task Execute(IJobExecutionContext context)
    {
        Logger.Info($"Start synchronizing current version folder");
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
                if (!context.CancellationToken.IsCancellationRequested)
                {
                    ProcessCustomer(customer, limit, context.CancellationToken);
                }
            }
            catch (Exception ex)
            {
                ex.Data.Clear();
                ex.Data.Add("CustomerCode", customer.CustomerName);
                Logger.Error(ex, $"There was an error on customer code: {customer.CustomerName}", ex);
            }
        }

        Logger.Info($"Finished synchronizing current version folder");
        return Task.CompletedTask;
    }

    private void ProcessCustomer(QaConfigCustomer customer, int limit, CancellationToken token)
    {
        using (new QPConnectionScope(customer.ConnectionString, customer.DbType))
        {
            _service.SyncFolders(customer.CustomerName, limit);
        }
    }
}
