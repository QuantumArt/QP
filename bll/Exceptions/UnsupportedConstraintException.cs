using System;

namespace Quantumart.QP8.BLL.Exceptions
{
    [Serializable]
    public class UnsupportedConstraintException : ApplicationException
    {
        public UnsupportedConstraintException() { }
        public UnsupportedConstraintException(string message) : base(message) { }
        public UnsupportedConstraintException(string message, Exception innerException) : base(message, innerException) { }
    }
}
