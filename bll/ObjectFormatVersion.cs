using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.Merger;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class ObjectFormatVersion : EntityObject
    {
        [Display(Name = "CodeBehind", ResourceType = typeof(ObjectFormatStrings))]
        public string CodeBehind { get; set; }

        [Display(Name = "Presentation", ResourceType = typeof(ObjectFormatStrings))]
        public string FormatBody { get; set; }

        [Display(Name = "NetClassName", ResourceType = typeof(TemplateStrings))]
        public string NetFormatName { get; set; }

        public NetLanguage NetLanguage { get; set; }

        [Display(Name = "NetLanguage", ResourceType = typeof(TemplateStrings))]
        public string NetLanguageName => NetLanguage.Name;

        public ObjectFormat ObjectFormat { get; set; }

        [Display(Name = "Name", ResourceType = typeof(EntityObjectStrings))]
        public string NameToMerge { get; set; }

        [Display(Name = "NetClassName", ResourceType = typeof(TemplateStrings))]
        public string NetFormatNameToMerge { get; set; }

        [Display(Name = "Description", ResourceType = typeof(EntityObjectStrings))]
        public string DescriptionToMerge { get; set; }

        [Display(Name = "NetLanguage", ResourceType = typeof(TemplateStrings))]
        public string NetLanguageNameToMerge { get; set; }

        [Display(Name = "Presentation", ResourceType = typeof(ObjectFormatStrings))]
        public string PresentationToMerge { get; set; }

        [Display(Name = "CodeBehind", ResourceType = typeof(ObjectFormatStrings))]
        public string CodeBehindToMerge { get; set; }

        public int? NetLanguageId { get; set; }

        /// <summary>
        /// Фальшивый идентификатор для текущей версии
        /// </summary>
        public static readonly int CurrentVersionId = 1;

        /// <summary>
        /// Версия, с которой происходит сравнение
        /// </summary>
        public ObjectFormatVersion VersionToMerge { get; set; }

        /// <summary>
        /// ID версии, с которой происходит сравнение
        /// </summary>
        public int MergedId => VersionToMerge?.Id ?? 0;

        /// <summary>
        /// Осуществляет слияние с указанной версией для реализации сравнения
        /// </summary>
        /// <param name="versionToMerge">версия для слияния</param>
        internal void MergeToVersion(ObjectFormatVersion versionToMerge)
        {
            NameToMerge = Merge(Formatter.ProtectHtml(Name), Formatter.ProtectHtml(versionToMerge.Name));
            NetFormatNameToMerge = Merge(Formatter.ProtectHtml(NetFormatName), Formatter.ProtectHtml(versionToMerge.NetFormatName));
            DescriptionToMerge = Merge(Formatter.ProtectHtml(Description), Formatter.ProtectHtml(versionToMerge.Description));
            NetLanguageNameToMerge = Merge(Formatter.ProtectHtml(NetLanguage.Name), Formatter.ProtectHtml(versionToMerge.NetLanguage.Name));
            PresentationToMerge = Merge(Formatter.ProtectHtml(FormatBody), Formatter.ProtectHtml(versionToMerge.FormatBody));
            CodeBehindToMerge = Merge(Formatter.ProtectHtml(CodeBehind), Formatter.ProtectHtml(versionToMerge.CodeBehind));
            VersionToMerge = versionToMerge;
        }

        /// <summary>
        /// Осуществляет слияние 2 строк с помощью QA_Merger
        /// </summary>
        /// <param name="s1">строка 1</param>
        /// <param name="s2">строка 2</param>
        /// <returns>результат слияния</returns>
        public static string Merge(string s1, string s2)
        {
            var prefix = "<html><body>";
            var suffix = "</body></html>";
            var mergeFormat = "{0}{1}{2}";
            var merger = new MergeProcessor(string.Format(mergeFormat, prefix, s1, suffix), string.Format(mergeFormat, prefix, s2, suffix));
            var result = merger.Merge();
            return result.Replace(prefix, "").Replace(suffix, "");
        }

        [Display(Name = "Name", ResourceType = typeof(EntityObjectStrings))]
        public string ExpandedName => Id == CurrentVersionId || Id == 0 ? EntityObjectStrings.CurrentVersion : string.Format(EntityObjectStrings.VersionN, Id);
    }
}
