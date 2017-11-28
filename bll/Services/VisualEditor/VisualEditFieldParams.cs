namespace Quantumart.QP8.BLL.Services.VisualEditor
{
    public class VisualEditFieldParams
    {
        public bool PEnterMode { get; set; }

        public bool UseEnglishQuotes { get; set; }

        public bool DisableListAutoWrap { get; set; }

        public string ExternalCss { get; set; }

        public string RootElementClass { get; set; }

        public VisualEditFieldParams()
        {
        }

        public VisualEditFieldParams(Site site)
        {
            PEnterMode = site.PEnterMode;
            UseEnglishQuotes = site.UseEnglishQuotes;
            DisableListAutoWrap = site.DisableListAutoWrap;
            ExternalCss = site.ExternalCss;
            RootElementClass = site.RootElementClass;
        }
    }
}
