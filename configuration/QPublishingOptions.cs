using System;
using Microsoft.AspNetCore.HttpOverrides;
using Quantumart.QP8.Configuration.Enums;
using Quantumart.QP8.Configuration.Models;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.Configuration
{
    public class QPublishingOptions
    {
        public QPublishingOptions()
        {
            UploadMaxSize = 100;
            RelationCountLimit = Default.RelationCountLimit;
            CommandTimeout = 120;
            Globalization = new Globalization();
            CookieTimeout = 1440;
            SessionTimeout = 60;
            QpConfigPollingInterval = TimeSpan.FromMinutes(2);
            Version = "8.0.0";
            BuildVersion = "8.0.0.0-00000000";
            UseClientExceptions = true;
        }

        public bool AllowReplayWithRecording { get; set; }

        public bool UseClientExceptions { get; set; }

        public string Version { get; set; }

        public string BuildVersion { get; set; }

        public string BackendUrl { get; set; }

        public string DefaultTheme  { get; set; }

        public int UploadMaxSize  { get; set; }

        public string InstanceName { get; set; }

        public string QpConfigPath { get; set; }

        public string QpConfigUrl { get; set; }

        public string QpConfigToken { get; set; }

        public TimeSpan QpConfigPollingInterval { get; set; }

        public string TempDirectory { get; set; }

        public bool AllowSelectCustomerCode { get; set; }

        public int RelationCountLimit { get; set; }

        public int CommandTimeout { get; set; }

        public int CookieTimeout { get; set; }

        public int SessionTimeout { get; set; }

        public bool Set500ForHandledExceptions { get; set; }

        public Globalization Globalization { get; set; }

        public bool EnableArticleScheduler { get; set; }

        public bool EnableCommonScheduler { get; set; }

        public ExternalAuthentication ExternalAuthentication { get; set; } = new();

        public bool ForceHttpForImageResizing { get; set; }

        public string SessionEncryptionKeysPath { get; set; }

        public string XForwardedForHeaderName { get; set; } = ForwardedHeadersDefaults.XForwardedForHeaderName;
    }
}
