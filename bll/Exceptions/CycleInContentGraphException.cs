using System;

namespace Quantumart.QP8.BLL.Exceptions
{
    [Serializable]
    public class CycleInContentGraphException : ApplicationException
    {
        public CycleInContentGraphException()
        {
        }

        public CycleInContentGraphException(string message)
            : base(message)
        {
        }

        public CycleInContentGraphException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
