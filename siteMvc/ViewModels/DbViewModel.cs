using System.Collections.Generic;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class DbViewModel : EntityViewModel
    {
        public DbViewModel()
        {
            OverrideRecordsFile = false;
        }

        public new Db Data
        {
            get => (Db)EntityData;
            set => EntityData = value;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.DbSettings;

        [LocalizedDisplayName("OverrideRecordsFile", NameResourceType = typeof(DBStrings))]
        public bool OverrideRecordsFile { get; set; }

        [LocalizedDisplayName("OverrideRecordsUser", NameResourceType = typeof(DBStrings))]
        public bool OverrideRecordsUser { get; set; }

        public string AggregationListItemsDataAppSettings { get; set; }

        public void DoCustomBinding()
        {
            if (!string.IsNullOrEmpty(AggregationListItemsDataAppSettings))
            {
                Data.AppSettings = JsonConvert.DeserializeObject<List<AppSettingsItem>>(AggregationListItemsDataAppSettings);
            }
        }
    }
}
