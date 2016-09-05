using System;

namespace Quantumart.QP8.WebMvc.Infrastructure.Exceptions
{
    [Serializable]
    public class XmlDbUpdateLoggingException : Exception
    {
        public XmlDbUpdateLoggingException(string message)
            : base(message)
        {
        }
    }
}
