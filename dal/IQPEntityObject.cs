using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.DAL
{
    public interface IQPEntityObject
    {
        decimal Id { get; set; }
        decimal LastModifiedBy { get; set; }
        DateTime Created { get; set; }
        DateTime Modified { get; set; }
    }
}
