namespace QP8.Infrastructure.Logging.PrtgMonitoring.Data
{
    public class PrtgServiceMonitoringMessage
    {
        public string Message { get; set; }

        public PrtgServiceMonitoringEnum State { get; set; }

        public int Queue { get; set; }

        public int Status => (int)State * (Queue + 1);
    }
}
