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

        public QaConfigCustomer[] GetItems()
        {
            return QPConfiguration.GetCustomers(AppName).Where(c => !c.ExcludeFromSchedulers).ToArray();
        }
    }
}
