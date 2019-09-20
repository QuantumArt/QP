using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class SelectedFilesViewModel
    {
        [BindProperty(Name="IDs")]
        public string[] Names { get; set; }
    }
}
