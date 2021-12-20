using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Configuration.Models;
using Quartz;

namespace Quantumart.QP8.CommonScheduler
{
    public static class JobHelpers
    {
        public const string CustomerCodesKey = "CustomerCodes";

        public static QaConfigCustomer[] FilterCustomers(QaConfigCustomer[] customers, JobDataMap dataMap)
        {
            var customersHash = new HashSet<string>(dataMap.GetString(CustomerCodesKey)?.Split(',') ?? new string[] { });
            return customersHash.Count > 0 ? customers.Where(n => customersHash.Contains(n.CustomerName)).ToArray() : customers;
        }
    }
}
