using System.Collections.Generic;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
    public class ExportSettingTemplate : EntityObject
    {
        [LocalizedDisplayName("ImportCulture", NameResourceType = typeof(MultistepActionStrings))]
        public int Culture { get; set; }

        [LocalizedDisplayName("ImportEncoding", NameResourceType = typeof(MultistepActionStrings))]
        public int Encoding { get; set; }

        private readonly int _delimiter = 1;

        [LocalizedDisplayName("SelectDelimiter", NameResourceType = typeof(MultistepActionStrings))]
        public int Delimiter
        {
            get => _delimiter;
            set => value = _delimiter;
        }

        [LocalizedDisplayName("OrderByField", NameResourceType = typeof(MultistepActionStrings))]
        public string OrderByField { get; set; }
    }

    public class ImportSettingTemplate : EntityObject
    {
        public string FileName { get; set; }

        [LocalizedDisplayName("ImportCulture", NameResourceType = typeof(MultistepActionStrings))]
        public int Culture { get; set; }

        [LocalizedDisplayName("ImportEncoding", NameResourceType = typeof(MultistepActionStrings))]
        public int Encoding { get; set; }

        [LocalizedDisplayName("SelectDelimiter", NameResourceType = typeof(MultistepActionStrings))]
        public int Delimiter { get; set; }

        public Dictionary<int, string> FieldsList { get; set; }

        [LocalizedDisplayName("ImportNoHeaders", NameResourceType = typeof(MultistepActionStrings))]
        public bool NoHeaders { get; set; }

        private readonly bool _updateAndInsert = false;

        [LocalizedDisplayName("UpdateAndInsert", NameResourceType = typeof(MultistepActionStrings))]
        public bool UpdateAndInsert
        {
            get => _updateAndInsert;
            set => value = _updateAndInsert;
        }

        [LocalizedDisplayName("UniqueFieldToUpdate", NameResourceType = typeof(MultistepActionStrings))]
        public string UniqueFieldToUpdate { get; set; }

        [LocalizedDisplayName("FlagForChangedStatus", NameResourceType = typeof(MultistepActionStrings))]
        public bool FlagForChangedStatus { get; set; }
    }
}
