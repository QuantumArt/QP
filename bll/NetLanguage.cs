using Quantumart.QP8.BLL.Repository;

namespace Quantumart.QP8.BLL
{
    public class NetLanguage : EntityObject
    {
        private const string CSharp = "C#";
        private const string VbNet = "VB.NET";

        public static NetLanguage GetcSharp() => PageTemplateRepository.GetNetLanguageByName(CSharp);

        public static NetLanguage GetVbNet() => PageTemplateRepository.GetNetLanguageByName(VbNet);
    }
}
