using System;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using RazorGenerator.Mvc;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(Quantumart.QP8.WebMvc.RazorGeneratorMvcStart), "Start")]

namespace Quantumart.QP8.WebMvc
{
    public static class RazorGeneratorMvcStart
    {
        public static void Start()
        {
            try
            {
                var engine = new PrecompiledMvcEngine(typeof(RazorGeneratorMvcStart).Assembly)
                {
                    UsePhysicalViewsIfNewer = HttpContext.Current.Request.IsLocal
                };

                ViewEngines.Engines.Insert(0, engine);
                VirtualPathFactoryManager.RegisterVirtualPathFactory(engine);
            }
            catch (ReflectionTypeLoadException e)
            {
                var exceptions = new StringBuilder("The following DLL load exceptions occurred:");
                foreach (var x in e.LoaderExceptions)
                {
                    exceptions.AppendFormat("{0},\n\n", x.Message);
                }

                throw new Exception($"Error loading Razor Generator Stuff:\n{exceptions}");
            }
        }
    }
}
