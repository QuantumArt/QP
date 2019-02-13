using System;

namespace Quantumart.QP8.ArticleScheduler
{
    public class ArticleSchedulerProperties
    {
        public TimeSpan RecurrentTimeout { get; set; }

        public TimeSpan PrtgLoggerTasksQueueCheckShiftTime { get; set; }

        public string ConfigServiceUrl { get; set; }

        public string ConfigServiceToken { get; set; }

        public string XmlConfigPath { get; set; }

        public string MailHost { get; set; }

    }
}
