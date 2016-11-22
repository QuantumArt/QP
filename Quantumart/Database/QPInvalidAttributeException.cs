using System;

namespace Quantumart.QPublishing.Database
{
    public class QpInvalidAttributeException : ApplicationException
    {
        public QpInvalidAttributeException()
        {
        }

        public QpInvalidAttributeException(string message)
            : base(message)
        {
        }

        public QpInvalidAttributeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public QpInvalidAttributeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
