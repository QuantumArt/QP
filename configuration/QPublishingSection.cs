using System.Configuration;
using Quantumart.QP8.Configuration.Authentication;
using Quantumart.QP8.Configuration.Globalization;

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
            get { return Utils.Url.ToAbsolute(base["backendUrl"].ToString()); }
            set { base["backendUrl"] = value; }
        }

        /// <summary>
        /// Возвращает название темы по умолчанию
        /// </summary>
        [ConfigurationProperty("defaultTheme", IsRequired = true)]
        public string DefaultTheme
        {
            get { return Utils.Url.ToAbsolute(base["defaultTheme"].ToString()); }
            set { base["defaultTheme"] = value; }
        }

        /// <summary>
        /// Возвращает название темы по умолчанию
        /// </summary>
        [ConfigurationProperty("uploaderType", IsRequired = true)]
        public string UploaderTypeName
        {
            get { return Utils.Url.ToAbsolute(base["uploaderType"].ToString()); }
            set { base["uploaderType"] = value; }
        }

        [ConfigurationProperty("uploadMaxSize", DefaultValue = 100)]
        public int UploadMaxSize => (int)base["uploadMaxSize"];

        [ConfigurationProperty("instanceName", DefaultValue = "")]
        public string InstanceName => (string)base["instanceName"];

        [ConfigurationProperty("qpConfigPath", DefaultValue = "")]
        public string QpConfigPath => (string)base["qpConfigPath"];

        [ConfigurationProperty("relationLimit", DefaultValue = 500)]
        public int RelationCountLimit => (int)base["relationLimit"];

        [ConfigurationProperty("cmdTimeout", DefaultValue = 120)]
        public int CommandTimeout => (int)base["cmdTimeout"];
    }
}
