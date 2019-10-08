using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class SelectedStringItemsViewModel
    {
        [BindProperty(Name="IDs")]
        public string[] Ids { get; set; }
    }
}
