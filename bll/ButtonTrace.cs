using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Validators;

namespace Quantumart.QP8.BLL
{
	public class ButtonTrace
	{
		[LocalizedDisplayName("ButtonName", NameResourceType = typeof(AuditStrings))]
		public string ButtonName { get; set; }

        [LocalizedDisplayName("TabName", NameResourceType = typeof(AuditStrings))]
		public string TabName { get; set; }

		[LocalizedDisplayName("ExecutionTime", NameResourceType = typeof(AuditStrings))]
		public DateTime ActivatedTime { get; set; }

		public int UserId { get; set; }

		[LocalizedDisplayName("UserLogin", NameResourceType = typeof(AuditStrings))]
		public string UserLogin { get; set; }

 	}
}
