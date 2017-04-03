using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.Scheduler.API
{
    public class ConnectionStrings : IConnectionStrings
    {
        private readonly ServiceDescriptor _descriptor;

        public ConnectionStrings(ServiceDescriptor descriptor)
        {
            _descriptor = descriptor;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return QPConfiguration.GetCustomers(_descriptor.Name, true).Select(c => c.ConnectionString).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
