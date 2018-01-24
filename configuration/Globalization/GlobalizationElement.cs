using System.Configuration;

namespace Quantumart.QP8.Configuration.Globalization
{
    public class GlobalizationElement : ConfigurationElement
    {
        [ConfigurationProperty("defaultLanguageId", IsRequired = true)]
        public int DefaultLanguageId
        {
            get => (int)base["defaultLanguageId"];
            set => base["defaultLanguageId"] = value;
        }

        [ConfigurationProperty("defaultCulture", IsRequired = true)]
        public string DefaultCulture
        {
            get => (string)base["defaultCulture"];
            set => base["defaultCulture"] = value;
        }
    }
}
