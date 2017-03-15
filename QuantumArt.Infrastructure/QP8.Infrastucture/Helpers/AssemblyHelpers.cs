using System.Reflection;

namespace QP8.Infrastucture.Helpers
{
    public static class AssemblyHelpers
    {
        public static string GetAssemblyName()
        {
            return Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetCallingAssembly().GetName().Name;
        }
    }
}
