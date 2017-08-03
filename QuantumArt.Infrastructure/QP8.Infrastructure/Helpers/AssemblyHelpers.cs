using System.Reflection;

namespace QP8.Infrastructure.Helpers
{
    public static class AssemblyHelpers
    {
        public static string GetAssemblyName() => Assembly.GetEntryAssembly()?.GetName().Name ?? Assembly.GetCallingAssembly().GetName().Name;
    }
}
