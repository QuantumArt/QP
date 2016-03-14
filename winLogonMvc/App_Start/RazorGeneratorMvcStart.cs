using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using RazorGenerator.Mvc;
using System.Text;
using System;

[assembly: WebActivatorEx.PostApplicationStartMethod(typeof(Quantumart.QP8.WebMvc.WinLogOn.App_Start.RazorGeneratorMvcStart), "Start")]

namespace Quantumart.QP8.WebMvc.WinLogOn.App_Start {
    public static class RazorGeneratorMvcStart {
        public static void Start() {
			PrecompiledMvcEngine engine = null;
			try
			{
				engine = new PrecompiledMvcEngine(typeof(RazorGeneratorMvcStart).Assembly)
				{
					UsePhysicalViewsIfNewer = HttpContext.Current.Request.IsLocal
				};
			}
			catch (System.Reflection.ReflectionTypeLoadException e)
			{
				StringBuilder exceptions = new StringBuilder("The following DLL load exceptions occurred:");
				foreach (var x in e.LoaderExceptions)
					exceptions.AppendFormat("{0},\n\n", x.Message);
				throw new Exception(string.Format("Error loading Razor Generator Stuff:\n{0}", exceptions));
			}

            ViewEngines.Engines.Insert(0, engine);

            // StartPage lookups are done by WebPages. 
            VirtualPathFactoryManager.RegisterVirtualPathFactory(engine);
        }
    }
}
