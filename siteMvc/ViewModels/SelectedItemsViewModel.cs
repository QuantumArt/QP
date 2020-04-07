using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class SelectedItemsViewModel
    {
        [BindProperty(Name="IDs")]
        public int[] Ids { get; set; }
    }
}
