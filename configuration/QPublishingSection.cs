using System.Configuration;
using Quantumart.QP8.Configuration.Authentication;
using Quantumart.QP8.Configuration.Globalization;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.Configuration
{
    public class QPublishingSection : ConfigurationSection
    {
        [ConfigurationProperty("globalization", IsRequired = true)]
        public GlobalizationElement Globalization => (GlobalizationElement)base["globalization"];

        [ConfigurationProperty("authentication", IsRequired = true)]
        public AuthenticationElement Authentication => (AuthenticationElement)base["authentication"];

        /// <summary>
        /// Возвращает URL, по которому расположен бэкенд
        /// </summary>
        [ConfigurationProperty("backendUrl", IsRequired = true)]
        public string BackendUrl
        {
#if !NET_STANDARD
            get => Url.ToAbsolute(base["backendUrl"].ToString());
#else
            get => base["backendUrl"].ToString();
#endif
            set => base["backendUrl"] = value;
        }

        /// <summary>
        /// Возвращает название темы по умолчанию
        /// </summary>
        [ConfigurationProperty("defaultTheme", IsRequired = true)]
        public string DefaultTheme
        {
#if !NET_STANDARD
            get => Url.ToAbsolute(base["defaultTheme"].ToString());
#else
            get => base["defaultTheme"].ToString();
#endif
            set => base["defaultTheme"] = value;
        }

        /// <summary>
        /// Возвращает название темы по умолчанию
        /// </summary>
        [ConfigurationProperty("uploaderType", IsRequired = true)]
        public string UploaderTypeName
        {
#if !NET_STANDARD
            get => Url.ToAbsolute(base["uploaderType"].ToString());
#else
            get => base["uploaderType"].ToString();
#endif
            set => base["uploaderType"] = value;
        }

        [ConfigurationProperty("uploadMaxSize", DefaultValue = 100)]
        public int UploadMaxSize => (int)base["uploadMaxSize"];

        [ConfigurationProperty("instanceName", DefaultValue = "")]
        public string InstanceName => (string)base["instanceName"];

        [ConfigurationProperty("qpConfigPath", DefaultValue = "")]
        public string QpConfigPath => (string)base["qpConfigPath"];

        [ConfigurationProperty("qpConfigUrl", DefaultValue = "")]
        public string QpConfigUrl => (string)base["qpConfigUrl"];

        [ConfigurationProperty("qpConfigToken", DefaultValue = "")]
        public string QpConfigToken => (string)base["qpConfigToken"];

        [ConfigurationProperty("relationLimit", DefaultValue = Default.RelationCountLimit)]
        public int RelationCountLimit => (int)base["relationLimit"];

        [ConfigurationProperty("cmdTimeout", DefaultValue = 120)]
        public int CommandTimeout => (int)base["cmdTimeout"];
    }
}
