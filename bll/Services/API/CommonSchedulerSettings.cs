using System.Collections.Generic;

namespace Quantumart.QP8.BLL.Services.API
{
    public class CommonSchedulerProperties
    {
        public CommonSchedulerProperties()
        {
            DefaultLanguageId = 1;
            DefaultUserId = 1;
            Tasks = new CommonSchedulerTaskProperties[] { };
        }

        public CommonSchedulerTaskProperties[] Tasks { get; set; }

        public int DefaultLanguageId { get; set; }

        public int DefaultUserId { get; set; }
        public string Name { get; set; }
    }

    public class CommonSchedulerTaskProperties
    {
        public string Name { get; set; }
        public string Schedule { get; set; }
        public string Description { get; set; }

        public Dictionary<string, string> SpecifiedConditions { get; set; }
    }
}
