using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Reflection;

namespace Quantumart.QP8.Utils
{
    public static class AssemblyExtensions
    {
        private static ConcurrentDictionary<Assembly, string> _cache = new ConcurrentDictionary<Assembly, string>();

        /// <summary>
        /// Get 8-letter Git commit hash from an assembly that patched by GitVersion
        /// https://gitversion.readthedocs.io/en/latest/usage/command-line/
        /// </summary>
        public static string GetCommitHash(this Assembly assembly)
        {
            return _cache.GetOrAdd(assembly, a =>
            {
                string procuctVersion = FileVersionInfo.GetVersionInfo(a.Location).ProductVersion;
                if (String.IsNullOrEmpty(procuctVersion))
                {
                    return null;
                }

                int shaIndex = procuctVersion.IndexOf(".Sha.");
                if (shaIndex == -1)
                {
                    return null;
                }

                return procuctVersion.Substring(shaIndex + 5, 8);
            });
        }
    }
}
