using Microsoft.AspNetCore.Mvc;

namespace Quantumart.QP8.WebMvc.ViewModels.Library
{
    public class CropViewModel
    {
        [BindProperty(Name = "overwriteFile")]
        public bool OverwriteFile { get; set; }

        [BindProperty(Name = "targetFileUrl")]
        public string TargetFileUrl { get; set; }

        [BindProperty(Name = "sourceFileUrl")]
        public string SourceFileUrl { get; set; }

        [BindProperty(Name = "resize")]
        public double Resize { get; set; }

        [BindProperty(Name = "top")]
        public int? Top { get; set; }

        [BindProperty(Name = "left")]
        public int? Left { get; set; }

        [BindProperty(Name = "width")]
        public int? Width { get; set; }

        [BindProperty(Name = "height")]
        public int? Height { get; set; }
    }
}
