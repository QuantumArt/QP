﻿using System.Collections.Generic;
using System.Linq;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL
{
    public class Db : EntityObject
    {
        public override string Name => QPContext.CurrentCustomerCode;

        public int? SingleUserId { get; set; }

        [LocalizedDisplayName("RecordActions", NameResourceType = typeof(DBStrings))]
        public bool RecordActions { get; set; }

        [LocalizedDisplayName("UseAdSyncService", NameResourceType = typeof(DBStrings))]
        public bool UseAdSyncService { get; set; }

        [LocalizedDisplayName("AutoLoadHome", NameResourceType = typeof(DBStrings))]
        public bool AutoOpenHome { get; set; }

        [LocalizedDisplayName("AppSettings", NameResourceType = typeof(DBStrings))]
        public IEnumerable<AppSettingsItem> AppSettings { get; set; }

        public override void Validate()
        {
            var errors = new RulesException<Db>();
            base.Validate(errors);

            var duplicateNames = AppSettings.GroupBy(c => c.Key).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
            var setts = AppSettings.ToArray();
            for (var i = 0; i < setts.Length; i++)
            {
                setts[i].Validate(errors, i + 1, duplicateNames);
            }

            if (!errors.IsEmpty)
            {
                throw errors;
            }
        }
    }
}
