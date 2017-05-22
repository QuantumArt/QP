using System.Collections.Generic;
using Quantumart.QP8.Configuration.Models;

namespace Quantumart.QP8.Configuration.Comparers
{
    public class QaConfigCustomerComparer : IEqualityComparer<QaConfigCustomer>
    {
        public bool Equals(QaConfigCustomer obj1, QaConfigCustomer obj2)
        {
            return obj1.CustomerName == obj2.CustomerName;
        }

        public int GetHashCode(QaConfigCustomer obj)
        {
            return obj.CustomerName.GetHashCode();
        }
    }
}
