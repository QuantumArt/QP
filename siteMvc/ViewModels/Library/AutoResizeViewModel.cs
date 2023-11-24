using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
    public class AutoResizeViewModel
    {
        [BindProperty(Name = "resizedImageTemplate")]
        public string ResizedImageTemplate { get; set; }

        [BindProperty(Name = "reduceSizes")]
        public ReduceSize[] ReduceSizes { get; set; }

        [BindProperty(Name = "folderUrl")]
        public string FolderUrl { get; set; }

        [BindProperty(Name = "fileName")]
        public string FileName { get; set; }
    }

    public class ReduceSize
    {
        [BindProperty(Name = "postfix")]
        public string Postfix { get; set; }
        [BindProperty(Name = "reduceRatio")]
        public decimal ReduceRatio { get; set; }
    }

    public class CheckAutoResizeViewModel
    {
        [BindProperty(Name = "folderUrl")]
        public string FolderUrl { get; set; }

        [BindProperty(Name = "fileName")]
        public string FileName { get; set; }

        [BindProperty(Name = "baseUrl")]
        public string BaseUrl { get; set; }

    }
}
