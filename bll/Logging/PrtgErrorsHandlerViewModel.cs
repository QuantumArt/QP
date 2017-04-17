using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.BLL.Logging
{
    public class PrtgErrorsHandlerViewModel
    {
        private int _customersTasksQueueCount;

        public int CustomersTasksQueueCount => _customersTasksQueueCount;

        public ConcurrentQueue<Exception> CustomersExceptions { get; } = new ConcurrentQueue<Exception>();

        public List<QaConfigCustomer> Customers { get; private set; }

        public PrtgErrorsHandlerViewModel(List<QaConfigCustomer> customers)
        {
            Customers = customers;
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
