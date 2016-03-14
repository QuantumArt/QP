using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.IO;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Services;
using System.Text;
using Quantumart.QP8.BLL.Helpers;
using System.Collections;

namespace Quantumart.QP8.WebMvc.ckeditor
{
	/// <summary>
	/// Summary description for CkeditorConfig
	/// </summary>
	public class CkeditorConfig : IHttpHandler
	{
		private static string LoadPlugins(IEnumerable<VisualEditorPlugin> plugins)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(@"(function () {");
			foreach (var plugin in plugins.Where(n => !String.IsNullOrWhiteSpace(n.Url)))
			{
				sb.AppendFormat(@"CKEDITOR.plugins.addExternal('{0}', '{1}', 'plugin.js');", plugin.Name, plugin.Url);
			}
			sb.AppendLine("})();");

			return sb.ToString();
		}

		private static string AddPlugins(IEnumerable<VisualEditorPlugin> plugins)
		{

			StringBuilder sb = new StringBuilder();
			foreach (var plugin in plugins)
			{
				sb.AppendFormat(@"config.extraPlugins += ',{0}';", plugin.Name);
				sb.AppendLine();
			}

			return sb.ToString();
		}
		
		private static string AddCss(Field field)
		{
			StringBuilder sb = new StringBuilder();
			if (field.ExternalCssItems.Any())
			{
				sb.AppendFormat(@"config.contentsCss = [{0}];", ExternalCssHelper.CreateConfigString(field.ExternalCssItems));
				sb.AppendLine();
			}

			if (!String.IsNullOrEmpty(field.RootElementClass))
			{
				sb.AppendFormat(@"config.bodyClass = '{0}'", field.RootElementClass);
				sb.AppendLine();
			}
			return sb.ToString();
		}

		public void ProcessRequest(HttpContext context)
		{

			int fieldId = int.Parse(context.Request.QueryString.Get("fieldId"));
			int siteId = int.Parse(context.Request.QueryString.Get("siteId"));
			List<int?> pluginIds = new List<int?>();
			string toolbar = GenerateVeToolbar(siteId, fieldId, ref pluginIds);
			string scriptTemplatePath = Path.Combine(context.Server.MapPath("~"), "Content/QP8/ckeditor/thesaurus_config_template.js.txt");
			string script = "";
			using (StreamReader reader = new StreamReader(scriptTemplatePath))
			{
				IEnumerable<VisualEditorPlugin> plugins = VisualEditorService.GetVisualEditorPlugins(pluginIds);
				Field field = FieldService.ReadForVisualEditor(fieldId);
				script = String.Format(
					reader.ReadToEnd(), 
					LoadPlugins(plugins), 
					AddPlugins(plugins) + AddCss(field), 
					toolbar, 
					"",
					field.VisualEditor.DocType,
					field.VisualEditor.EnterMode,
					field.VisualEditor.ShiftEnterMode,
					field.VisualEditor.FullPage.ToString().ToLowerInvariant(),
					field.VisualEditor.Height,
					field.VisualEditor.Language,
					field.VisualEditor.UseEnglishQuotes.ToString().ToLowerInvariant()
				);
			}

			context.Response.ContentType = "text/javascript";
			context.Response.Write(script);
		}

		private string GenerateVeToolbar(int siteId, int fieldId, ref List<int?> pluginIds)
		{
			var commands = FieldService.GetResultVisualEditorCommands(fieldId, siteId).Where(c => c.On == true);
			pluginIds = commands.Select(c => c.PluginId).Where(c => c != null).Distinct().ToList();
			var rows = commands.GroupBy(c => c.RowOrder).ToArray();			
			return VeAggregationListItemsHelper.GenerateVeToolbar(rows);			
		}				

		public bool IsReusable
		{
			get
			{
				return false;
			}
		}
	}
}