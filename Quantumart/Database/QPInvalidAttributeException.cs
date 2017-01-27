using System;
using System.Runtime.Serialization;

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

        public QpInvalidAttributeException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
