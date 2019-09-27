using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class ReplayViewModel
    {
        [BindProperty(Name="xmlString")]
        public string XmlString { get; set; }

        [BindProperty(Name="generateNewFieldIds")]
        public bool GenerateNewFieldIds { get; set; }

        [BindProperty(Name="generateNewContentIds")]
        public bool GenerateNewContentIds { get; set; }

        [BindProperty(Name="useGuidSubstitution")]
        public bool UseGuidSubstitution { get; set; }
    }
}
