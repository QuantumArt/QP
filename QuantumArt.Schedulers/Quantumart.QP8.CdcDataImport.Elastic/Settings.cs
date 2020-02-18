
using System;

namespace Quantumart.QP8.CdcDataImport.Elastic {


    public class Settings {

        public static Settings Default = new Settings();

        public Settings()
        {
            CheckNotificationsQueueRecurrentTimeout = TimeSpan.Parse("00:00:20");
            CdcRecurrentTimeout = TimeSpan.Parse("00:00:01");
            HttpTimeout = TimeSpan.Parse("00:00:10");
        }

        public string HttpEndpoint { get; set; }

        public TimeSpan CheckNotificationsQueueRecurrentTimeout { get; set; }

        public TimeSpan CdcRecurrentTimeout { get; set; }

        public TimeSpan HttpTimeout { get; set; }

    }
}
