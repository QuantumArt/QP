using Quantumart.QP8.BLL;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers
{
    public static class HighlightModeSelectHelper
    {
        private const string CsharpMode = "hta-cSharpTextArea";
        private const string VbsMode = "hta-VBSTextArea";
        private const string VbMode = "hta-VBTextArea";
        private const string XmlMode = " highlightedTextarea hta-XmlTextArea";
        private const string JsonMode = " highlightedTextarea hta-JsonTextArea";
        private const string HtmlMode = " highlightedTextarea hta-htmlTextArea";

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

        public static string SelectHighlightType(string type)
        {
            switch (type)
            {
                case TextAreaHighlightTypes.XmlHighlightType:
                    return XmlMode;
                case TextAreaHighlightTypes.JsonHighlightType:
                    return JsonMode;
                case TextAreaHighlightTypes.HtmlHighlightType:
                    return HtmlMode;
                default:
                    return string.Empty;
            }
        }
    }
}
