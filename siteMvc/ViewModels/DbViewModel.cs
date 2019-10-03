using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.ViewModels.Abstract;

namespace Quantumart.QP8.WebMvc.ViewModels
{
    public class DbViewModel : EntityViewModel
    {
        public DbViewModel()
        {
            OverrideRecordsFile = false;
        }

        public Db Data
        {
            get => (Db)EntityData;
            set => EntityData = value;
        }

        public override string EntityTypeCode => Constants.EntityTypeCode.CustomerCode;

        public override string ActionCode => Constants.ActionCode.DbSettings;

        [Display(Name = "OverrideRecordsFile", ResourceType = typeof(DBStrings))]
        public bool OverrideRecordsFile { get; set; }

        [Display(Name = "OverrideRecordsUser", ResourceType = typeof(DBStrings))]
        public bool OverrideRecordsUser { get; set; }

        public string AggregationListItemsDataAppSettings { get; set; }

        public override void DoCustomBinding()
        {
            base.DoCustomBinding();
            if (!string.IsNullOrEmpty(AggregationListItemsDataAppSettings))
            {
                Data.AppSettings = JsonConvert.DeserializeObject<List<AppSettingsItem>>(AggregationListItemsDataAppSettings);
            }
        }
    }
}
