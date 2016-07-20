using System;

namespace Quantumart.QP8.WebMvc.Infrastructure.Exceptions
{
    [Serializable]
    public class XmlDbUpdateReplayActionException : Exception
    {
        public XmlDbUpdateReplayActionException()
        {
        }

        public XmlDbUpdateReplayActionException(string message)
            : base(message)
        {
        }

        public XmlDbUpdateReplayActionException(string message, Exception ex)
            : base(message, ex)
        {
        }
    }
}
