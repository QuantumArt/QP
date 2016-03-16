using System;

namespace Quantumart.QPublishing.Database
{
    // ReSharper disable once InconsistentNaming
    public class QPInvalidAttributeException : ApplicationException
    {
        public QPInvalidAttributeException()
        {
        }

        public QPInvalidAttributeException(string message)
            : base(message)
        {
        }

        public QPInvalidAttributeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public QPInvalidAttributeException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}