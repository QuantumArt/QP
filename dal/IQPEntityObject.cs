using System;

namespace Quantumart.QP8.DAL
{
    public interface IQpEntityObject
    {
        decimal Id { get; set; }

        decimal LastModifiedBy { get; set; }

        DateTime Created { get; set; }

        DateTime Modified { get; set; }
    }
}
