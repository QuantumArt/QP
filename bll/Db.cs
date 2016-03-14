using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.BLL
{
	public class Db : EntityObject
	{
		public override string Name
		{
			get
			{
				return QPContext.CurrentCustomerCode;
			}
		}

		public int? SingleUserId { get; set; }

		[LocalizedDisplayName("RecordActions", NameResourceType = typeof(DBStrings))]
		public bool RecordActions { get; set; }

		[LocalizedDisplayName("FpSettings", NameResourceType = typeof(DBStrings))]
		public string FpSettings { get; set; }

		[LocalizedDisplayName("UseADSyncService", NameResourceType = typeof(DBStrings))]
		public bool UseADSyncService { get; set; }

		[LocalizedDisplayName("AutoLoadHome", NameResourceType = typeof(DBStrings))]
		public bool AutoOpenHome { get; set; }

		public XDocument FpSettingsXml
		{
			get
			{
				if (FpSettings == null)
					return null;
				else
				{
					return XDocument.Load(new StringReader(FpSettings));
				}
			}
		}

		[LocalizedDisplayName("AppSettings", NameResourceType = typeof(DBStrings))]
		public IEnumerable<AppSettingsItem> AppSettings { get; set; }

		public override void Validate()
		{
			RulesException<Db> errors = new RulesException<Db>();
			base.Validate(errors);

			var duplicateNames = AppSettings.GroupBy(c => c.Key).Where(g => g.Count() > 1).Select(x => x.Key).ToArray();
			var setts = AppSettings.ToArray();
			for (int i = 0; i < setts.Length; i++)
				setts[i].Validate(errors, i + 1, duplicateNames);

			if (!errors.IsEmpty)
				throw errors;
		}

	}
}
