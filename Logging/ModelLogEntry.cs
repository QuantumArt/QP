using Microsoft.Practices.EnterpriseLibrary.Logging;

namespace Quantumart.QP8.Logging
{
    public class ModelLogEntry : LogEntry
    {
        public object Model { get; set; }

        public object Context { get; set; }
    }
}
