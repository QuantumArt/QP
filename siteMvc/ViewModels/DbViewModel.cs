using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;
using B = Quantumart.QP8.BLL;
using C = Quantumart.QP8.Constants;
using System.Web.Script.Serialization;
using Quantumart.QP8.BLL.Helpers;

namespace Quantumart.QP8.WebMvc.ViewModels
{
	public class DbViewModel : EntityViewModel
	{
		public DbViewModel()
		{
			OverrideRecordsFile = false;
		}

		public new B.Db Data
		{
			get
			{
				return (B.Db)EntityData;
			}

			set
			{
				EntityData = value;
			}
		}
		public override string EntityTypeCode
		{
			get
			{
				return C.EntityTypeCode.CustomerCode;
			}
		}

		public override string ActionCode
		{
			get
			{
					return C.ActionCode.DbSettings;
			}
		}

		[LocalizedDisplayName("OverrideRecordsFile", NameResourceType = typeof(DBStrings))]
		public bool OverrideRecordsFile { get; set; }

		[LocalizedDisplayName("OverrideRecordsUser", NameResourceType = typeof(DBStrings))]
		public bool OverrideRecordsUser { get; set; }

		public string AggregationListItems_Data_AppSettings { get; set; }

		public void DoCustomBinding()
		{
			if (!String.IsNullOrEmpty(AggregationListItems_Data_AppSettings))
			{
				Data.AppSettings = new JavaScriptSerializer().Deserialize<List<AppSettingsItem>>(AggregationListItems_Data_AppSettings);
			}
		}

	}
}