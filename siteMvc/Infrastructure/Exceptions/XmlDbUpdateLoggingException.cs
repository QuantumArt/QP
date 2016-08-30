using System;

namespace Quantumart.QP8.WebMvc.Infrastructure.Exceptions
{
    public class XmlDbUpdateLoggingException : Exception
    {
        public XmlDbUpdateLoggingException(string message)
            : base(message)
        {
        }
    }
}
