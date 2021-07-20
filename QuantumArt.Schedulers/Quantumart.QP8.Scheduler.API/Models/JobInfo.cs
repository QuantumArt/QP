using System.Collections.Generic;

namespace Quantumart.QP8.Scheduler.API.Models
{
    public class JobInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<TriggerInfo> Trigger { get; set; }
    }

    public class TriggerInfo
    {
        public string Name { get; set; }
        public string LastStartTime { get; set; }
        public string NextStartTime { get; set; }
        public string Schedule { get; set; }
    }
}
