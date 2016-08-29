using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Quantumart.QP8.WebMvc.WinLogOn;
using RazorGenerator.Mvc;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(RazorGeneratorMvcStart), "Start")]
namespace Quantumart.QP8.WebMvc.WinLogOn
{
    public static class RazorGeneratorMvcStart
    {
        public static void Start()
        {
            PrecompiledMvcEngine engine;
            try
            {
                engine = new PrecompiledMvcEngine(typeof(RazorGeneratorMvcStart).Assembly)
                {
                    UsePhysicalViewsIfNewer = HttpContext.Current.Request.IsLocal
                };
            }
            catch (System.Reflection.ReflectionTypeLoadException e)
            {
                var exceptions = new StringBuilder("The following DLL load exceptions occurred:");
                foreach (var x in e.LoaderExceptions)
                {
                    exceptions.AppendFormat("{0},\n\n", x.Message);
                }

                throw new Exception($"Error loading Razor Generator Stuff:\n{exceptions}");
            }

            ViewEngines.Engines.Insert(0, engine);
            VirtualPathFactoryManager.RegisterVirtualPathFactory(engine);
        }
    }
}
