using System.Collections.Generic;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
    public class VisualEditorConfigVm
    {
        public string Language { get; set; }

        public string DocType { get; set; }

        public bool FullPage { get; set; }

        public int EnterMode { get; set; }

        public int ShiftEnterMode { get; set; }

        public bool UseEnglishQuotes { get; set; }

        public int Height { get; set; }

        public string BodyClass { get; set; }

        public IEnumerable<string> ContentsCss { get; set; }

        public IEnumerable<VisualEditorStyleVm> StylesSet { get; set; }

        public IEnumerable<VisualEditorStyleVm> FormatsSet { get; set; }

        public IEnumerable<VisualEditorPluginVm> ExtraPlugins { get; set; }

        public IEnumerable<object> Toolbar { get; set; }
    }
}
