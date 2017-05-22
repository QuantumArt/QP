namespace Quantumart.QP8.Scheduler.API
{
    public class ProcessorDescriptor
    {
        public string Processor { get; }

        public string Service { get; }

        public string Schedule { get; }

        public ProcessorDescriptor(string processor, string service, string schedule)
        {
            Processor = processor;
            Service = service;
            Schedule = schedule;
        }
    }
}
