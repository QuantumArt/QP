using System;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.ArticleScheduler
{
    public class ArticleSchedulerProperties
    {
        public ArticleSchedulerProperties()
        {

        }

        public ArticleSchedulerProperties(QPublishingOptions options)
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

        public string MailHost { get; set; }

    }
}
