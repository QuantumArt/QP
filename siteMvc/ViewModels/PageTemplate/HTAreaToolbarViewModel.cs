using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels.PageTemplate
{
    public class HtAreaToolbarViewModel
    {
        public HtAreaToolbarViewModel(bool presentationOrCodeBehind, int? formatId, int? templateId)
        {
            PresentationOrCodeBehind = presentationOrCodeBehind;
            FormatId = formatId;
            TemplateId = templateId;
        }

        [BindProperty(Name = "presentationOrCodeBehind")]
        public bool PresentationOrCodeBehind { get; set; }

        [BindProperty(Name = "formatId")]
        public int? FormatId { get; set; }

        [BindProperty(Name = "templateId")]
        public int? TemplateId { get; set; }
    }
}
