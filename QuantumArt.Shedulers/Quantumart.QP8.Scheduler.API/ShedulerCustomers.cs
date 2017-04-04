using System.Collections;
using System.Collections.Generic;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.Scheduler.API
{
    public class ShedulerCustomers : IShedulerCustomers
    {
        private readonly ServiceDescriptor _descriptor;

        public ShedulerCustomers(ServiceDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public IEnumerator<QaConfigCustomer> GetEnumerator()
        {
            return QPConfiguration.GetCustomers(_descriptor.Name, true).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
