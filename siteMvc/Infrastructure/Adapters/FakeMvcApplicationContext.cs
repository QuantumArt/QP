using System;
using System.Reflection;
using AutoMapper;

namespace Quantumart.QP8.WebMvc.Infrastructure.Adapters
{
    // TODO: init AspNetCore in FakeMvcApplicationContext
    public class FakeMvcApplicationContext : IDisposable
    {
        internal FakeMvcApplicationContext()
        {
            //MvcApplication.RegisterModelBinders();
            //MvcApplication.RegisterModelValidatorProviders();
            CheatBuildManager();

            //MvcApplication.RegisterUnity();
            //MvcApplication.RegisterRoutes(new RouteCollection());
        }

        private static void CheatBuildManager()
        {
            // var memberInfo = typeof(BuildManager).GetField("_theBuildManager", BindingFlags.NonPublic | BindingFlags.Static);
            // if (memberInfo != null)
            // {
            //     var manager = memberInfo.GetValue(null);
            //     typeof(BuildManager).GetProperty("PreStartInitStage", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, 2, null);
            //
            //     var fieldInfo = typeof(BuildManager).GetField("_topLevelFilesCompiledStarted", BindingFlags.NonPublic | BindingFlags.Instance);
            //     fieldInfo?.SetValue(manager, true);
            //
            //     var field = typeof(BuildManager).GetField("_topLevelReferencedAssemblies", BindingFlags.NonPublic | BindingFlags.Instance);
                //field?.SetValue(manager, new List<Assembly> { typeof(MvcApplication).Assembly });
            //}
        }

        public void Dispose()
        {
            //MvcApplication.UnregisterModelBinders();
            //MvcApplication.UnregisterModelValidatorProviders();
            //MvcApplication.UnregisterRoutes();
        }
    }
}

