using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    public class Db : EntityObject
    {
        public override string Name => QPContext.CurrentCustomerCode;

        public int? SingleUserId { get; set; }

        [Display(Name = "RecordActionsIntoFile", ResourceType = typeof(DBStrings))]
        public bool RecordActions { get; set; }

        [Display(Name = "UseExternalUsersSyncService", ResourceType = typeof(DBStrings))]
        public bool UseAdSyncService { get; set; }

        [Display(Name = "UseDPC", ResourceType = typeof(DBStrings))]
        public bool UseDpc { get; set; }

        [Display(Name = "UseTokens", ResourceType = typeof(DBStrings))]
        public bool UseTokens { get; set; }

        [Display(Name = "UseCDC", ResourceType = typeof(DBStrings))]
        public bool UseCdc { get; set; }

        [Display(Name = "UseS3", ResourceType = typeof(DBStrings))]
        public bool UseS3 { get; set; }

        [Display(Name = "AutoLoadHome", ResourceType = typeof(DBStrings))]
        public bool AutoOpenHome { get; set; }

        [Display(Name = "AppSettings", ResourceType = typeof(DBStrings))]
        public IEnumerable<AppSettingsItem> AppSettings { get; set; }

        public override void Validate()
        {
            var errors = new RulesException<Db>();
            base.Validate(errors);

            var duplicateNames = AppSettings.GroupBy(c => c.Key).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
            var settings = AppSettings.ToArray();
            for (var i = 0; i < settings.Length; i++)
            {
                settings[i].Validate(errors, i + 1, duplicateNames);
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }
    }
}
