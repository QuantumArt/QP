using System.ComponentModel.DataAnnotations;
using Quantumart.QP8.Merger;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL
{
    public class QpPluginVersion : EntityObject
    {
        [Display(Name = "Contract", ResourceType = typeof(QpPluginStrings))]
        public string Contract { get; set; }

        [Display(Name = "Contract", ResourceType = typeof(QpPluginStrings))]
        public string ContractToMerge { get; set; }

        public int PluginId { get; set; }

        public QpPlugin Plugin { get; set; }

        public override string Name => Id.ToString();

        /// <summary>
        /// Фальшивый идентификатор для текущей версии
        /// </summary>
        public static readonly int CurrentVersionId = 1;

        /// <summary>
        /// Версия, с которой происходит сравнение
        /// </summary>
        public QpPluginVersion VersionToMerge { get; set; }

        /// <summary>
        /// ID версии, с которой происходит сравнение
        /// </summary>
        public int MergedId => VersionToMerge?.Id ?? 0;

        /// <summary>
        /// Осуществляет слияние с указанной версией для реализации сравнения
        /// </summary>
        /// <param name="versionToMerge">версия для слияния</param>
        internal void MergeToVersion(QpPluginVersion versionToMerge)
        {
            ContractToMerge = Merge(Formatter.ProtectHtml(Contract), Formatter.ProtectHtml(versionToMerge.Contract));
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
