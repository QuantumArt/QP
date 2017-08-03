using System.Collections.Concurrent;
using System.Collections.Generic;
using Quantumart.QP8.Configuration.Comparers;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.CdcDataImport.Common.Infrastructure
{
    public static class CdcSynchronizationContext
    {
        public static volatile ConcurrentDictionary<QaConfigCustomer, bool> CustomersDictionary = new ConcurrentDictionary<QaConfigCustomer, bool>();

        public static void ReplaceData(IDictionary<QaConfigCustomer, bool> newData)
        {
            CustomersDictionary = new ConcurrentDictionary<QaConfigCustomer, bool>(newData, new QaConfigCustomerComparer());
        }

        public static void SetCustomerNotificationQueueNotEmpty(QaConfigCustomer customer)
        {
            SetIsCustomerNotificationQueueEmpty(customer, false);
        }

        private static void SetIsCustomerNotificationQueueEmpty(QaConfigCustomer customer, bool isNotificationQueueEmpty)
        {
            CustomersDictionary.TryUpdate(customer, isNotificationQueueEmpty, !isNotificationQueueEmpty);
        }
    }
}
