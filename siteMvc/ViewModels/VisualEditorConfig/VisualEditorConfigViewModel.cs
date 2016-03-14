using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditorConfig
{
	public class VisualEditorConfigViewModel
	{
		public MvcHtmlString Toolbar { get; set; }

		public MvcHtmlString DocType { get; set; }

		public MvcHtmlString FullPage { get; set; }

		public MvcHtmlString Language { get; set; }

		public MvcHtmlString UseEnglishQuotes { get; set; }

		public MvcHtmlString ContentsCss { get; set; }

		public int EnterMode { get; set; }

		public int ShiftEnterMode { get; set; }

		public int Height { get; set; }

		private IEnumerable<VisualEditorCommand> VeCommands { get; set; }

		private IEnumerable<VisualEditorStyle> VeStyles { get; set; }

		public IEnumerable<VisualEditorStyle> Formats { get { return VeStyles.Where(x => x.On && x.IsFormat).OrderBy(x => x.Order); } }

		public MvcHtmlString FormatTags { get { return MvcHtmlString.Create(String.Join(";", Formats.Select(x => x.Tag).ToArray())); } }

		public IEnumerable<VisualEditorStyle> Styles { get { return VeStyles.Where(x => x.On && !x.IsFormat).OrderBy(x => x.Order); } }

		public IEnumerable<VisualEditorPlugin> Plugins { get; set; }

		public string BodyClass { get; set; }

		public VisualEditorConfigViewModel(int fieldId, int siteId)
		{
			CommonInit(fieldId);
			InitExtensions(fieldId, siteId);
		}

		private void InitExtensions(int fieldId, int siteId)
		{
			VeStyles = FieldService.GetResultStyles(fieldId, siteId);
			VeCommands = FieldService.GetResultVisualEditorCommands(fieldId, siteId).Where(n => n.On);
			Plugins = VisualEditorService.GetVisualEditorPlugins(VeCommands.Select(c => c.PluginId).Where(c => c != null).Distinct().ToList());
			Toolbar = MvcHtmlString.Create(VeAggregationListItemsHelper.GenerateVeToolbar(VeCommands.GroupBy(c => c.RowOrder).ToArray()));
		}

		private void CommonInit(int fieldId)
		{
			if (fieldId != 0)
			{
				BLL.Field field = FieldService.ReadForVisualEditor(fieldId);
				ContentsCss = MvcHtmlString.Create(ExternalCssHelper.CreateConfigString(field.ExternalCssItems));
				DocType = MvcHtmlString.Create(field.VisualEditor.DocType);
				EnterMode = field.VisualEditor.EnterMode;
				ShiftEnterMode = field.VisualEditor.ShiftEnterMode;
				FullPage = MvcHtmlString.Create(field.VisualEditor.FullPage.ToString().ToLowerInvariant());
				Height = field.VisualEditor.Height;
				Language = MvcHtmlString.Create(field.VisualEditor.Language);
				UseEnglishQuotes = MvcHtmlString.Create(field.VisualEditor.UseEnglishQuotes.ToString().ToLowerInvariant());
				BodyClass = field.RootElementClass;
			}
			else
			{
				FullPage = MvcHtmlString.Create("true".ToLowerInvariant());
				UseEnglishQuotes = MvcHtmlString.Create("false".ToLowerInvariant());
				Height = 340;
			}
		}			
	}	
}