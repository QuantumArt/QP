using System;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.ArticleScheduler
{
    public class ArticleSchedulerProperties
    {
        private const int DefaultRecurrentTimeout = 1;
        private const int DefaultPrtgLoggerTasksQueueCheckShiftTime = 3;

        public ArticleSchedulerProperties()
        {
            RecurrentTimeout = TimeSpan.FromMinutes(DefaultRecurrentTimeout);
            PrtgLoggerTasksQueueCheckShiftTime = TimeSpan.FromMinutes(DefaultPrtgLoggerTasksQueueCheckShiftTime);
        }

        public ArticleSchedulerProperties(QPublishingOptions options) : this()
        {
            XmlConfigPath = options.QpConfigPath;
            ConfigServiceUrl = options.QpConfigUrl;
            ConfigServiceToken = options.QpConfigToken;
        }

        public TimeSpan RecurrentTimeout { get; set; }

        public TimeSpan PrtgLoggerTasksQueueCheckShiftTime { get; set; }

        public string ConfigServiceUrl { get; set; }

        public string ConfigServiceToken { get; set; }

        public string XmlConfigPath { get; set; }
    }
}
