using Quantumart.QP8.BLL;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HighlightModeSelectHelper
    {
        private const string CsharpMode = "hta-cSharpTextArea";
        private const string VbsMode = "hta-VBSTextArea";
        private const string VbMode = "hta-VBTextArea";

        public static string SelectMode(int? languageId)
        {
            if (NetLanguage.GetcSharp().Id == languageId)
            {
                return CsharpMode;
            }

            if (NetLanguage.GetVbNet().Id == languageId)
            {
                return VbMode;
            }

            return VbsMode;
        }
    }
}
