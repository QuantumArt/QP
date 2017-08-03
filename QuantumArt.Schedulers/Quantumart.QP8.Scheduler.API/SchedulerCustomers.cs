using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.Scheduler.API
{
    public class SchedulerCustomerCollection : ISchedulerCustomerCollection
    {
        private readonly ServiceDescriptor _descriptor;

        public SchedulerCustomerCollection(ServiceDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public IEnumerator<QaConfigCustomer> GetEnumerator()
        {
            return QPConfiguration.GetCustomers(_descriptor.Name).Where(c => !c.ExcludeFromSchedulers).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
