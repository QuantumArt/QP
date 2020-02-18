using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.BLL.Logging
{
    public class PrtgErrorsHandlerViewModel
    {
        private int _customersTasksQueueCount;

        public int CustomersTasksQueueCount => _customersTasksQueueCount;

        public ConcurrentQueue<Exception> CustomersExceptions { get; } = new ConcurrentQueue<Exception>();

        public QaConfigCustomer[] Customers { get; }

        public PrtgErrorsHandlerViewModel(IEnumerable<QaConfigCustomer> customers)
        {
            Customers = customers.ToArray();
        }

        public void IncrementTasksQueueCount(int tasksCount)
        {
            Interlocked.Add(ref _customersTasksQueueCount, tasksCount);
        }

        public void EnqueueNewException(Exception exception)
        {
            CustomersExceptions.Enqueue(exception);
        }
    }
}
