using System.Configuration;
using Quantumart.QP8.WebMvc.Infrastructure.Constants;

namespace Quantumart.QP8.WebMvc.Infrastructure.Helpers
{
    public class ConfigHelpers
    {
        public static bool ShouldSet500ForHandledExceptions => bool.Parse(ConfigurationManager.AppSettings[ConfigConstants.Set500ForHandledExceptions]);
    }
}
