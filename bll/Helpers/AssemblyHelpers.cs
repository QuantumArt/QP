using System.Reflection;

namespace Quantumart.QP8.BLL.Helpers
{
    public static class AssemblyHelpers
    {
        public static string GetAssemblyName()
        {
            return Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetCallingAssembly().GetName().Name;
        }
    }
}
