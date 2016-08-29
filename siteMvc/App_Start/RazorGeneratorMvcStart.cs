using System;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using Quantumart.QP8.WebMvc;
using RazorGenerator.Mvc;
using WebActivatorEx;

[assembly: PostApplicationStartMethod(typeof(RazorGeneratorMvcStart), "Start")]
namespace Quantumart.QP8.WebMvc
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
            catch (ReflectionTypeLoadException e)
            {
                var exceptions = new StringBuilder("The following DLL load exceptions occurred:");
                foreach (var exc in e.LoaderExceptions)
                {
                    exceptions.AppendFormat("{0},\n\n", exc.Message);
                }

                throw new Exception($"Error loading Razor Generator Stuff:\n{exceptions}");
            }

            ViewEngines.Engines.Insert(0, engine);
            VirtualPathFactoryManager.RegisterVirtualPathFactory(engine);
        }
    }
}
