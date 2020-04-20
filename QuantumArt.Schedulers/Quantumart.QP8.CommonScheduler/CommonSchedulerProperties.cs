using System;
using System.Collections.Generic;

namespace Quantumart.QP8.CommonScheduler
{
    public class CommonSchedulerProperties
    {
        public CommonSchedulerProperties()
        {
            DefaultLanguageId = 1;
            DefaultUserId = 1;
            ServiceRepeatInterval = TimeSpan.FromSeconds(30);
            ServiceRepeatOnErrorInterval = TimeSpan.FromSeconds(30);
            Tasks = new CommonSchedulerTaskProperties[] { };
        }

        public TimeSpan ServiceRepeatInterval { get; set; }

        public TimeSpan ServiceRepeatOnErrorInterval { get; set; }

        public CommonSchedulerTaskProperties[] Tasks { get; set; }

        public int DefaultLanguageId { get; set; }

        public int DefaultUserId { get; set; }
    }
}
