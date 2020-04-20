using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.Scheduler.API
{
    public class SchedulerCustomerCollection : ISchedulerCustomerCollection
    {
        private readonly string AppName = "QP8.CommonScheduler";

        // private readonly QPublishingOptions _options;
        // public SchedulerCustomerCollection(QPublishingOptions options)
        // {
        //     _options = options;
        // }

        public QaConfigCustomer[] GetItems()
        {
            // QPConfiguration.ConfigServiceUrl = _options.QpConfigUrl;
            // QPConfiguration.ConfigServiceToken = _options.QpConfigToken;
            // QPConfiguration.XmlConfigPath = _options.QpConfigPath;

            return QPConfiguration.GetCustomers(AppName).Where(c => !c.ExcludeFromSchedulers).ToArray();
        }
    }
}
