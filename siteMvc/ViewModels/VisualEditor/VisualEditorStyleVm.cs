using System.Collections.Generic;

namespace Quantumart.QP8.WebMvc.ViewModels.VisualEditor
{
    public class VisualEditorStyleVm
    {
        public string Name { get; set; }

        public string Element { get; set; }

        public string Overrides { get; set; }

        public IDictionary<string, string> Styles { get; set; }

        public IDictionary<string, string> Attributes { get; set; }
    }
}
