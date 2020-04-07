using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class MultiStepActionViewModel
    {
        [BindProperty(Name="step")]
        public int Step { get; set; }

        [BindProperty(Name="stage")]
        public int Stage { get; set; }

    }
}
