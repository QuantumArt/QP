using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Quantumart.QP8.Validators;
using Quantumart.QP8.Resources;
using System.Globalization;

namespace Quantumart.QP8.BLL
{
	public class SessionsLog
	{
		public SessionsLog()
		{
			IsQP7 = false;
		}

		
		public int SessionId { get; set; }

		[LocalizedDisplayName("UserLogin", NameResourceType = typeof(AuditStrings))]
		public string Login { get; set; }

		public int? UserId { get; set; }

		[LocalizedDisplayName("SessionStartTime", NameResourceType = typeof(AuditStrings))]
		public DateTime StartTime { get; set; }			

		[LocalizedDisplayName("SessionEndTime", NameResourceType = typeof(AuditStrings))]
		public DateTime? EndTime { get; set; }

		[LocalizedDisplayName("IP", NameResourceType = typeof(AuditStrings))]
		public string IP { get; set; }

		[LocalizedDisplayName("Browser", NameResourceType = typeof(AuditStrings))]
		public string Browser { get; set; }

		[LocalizedDisplayName("ServerName", NameResourceType = typeof(AuditStrings))]
		public string ServerName { get; set; }

		[LocalizedDisplayName("AutoLogged", NameResourceType = typeof(AuditStrings))]
		public int AutoLogged { get; set; }

		public string Sid { get; set; }

		[LocalizedDisplayName("IsQP7", NameResourceType = typeof(AuditStrings))]
		public bool IsQP7 { get; set; }


		#region Caulculated
		[LocalizedDisplayName("FailedTime", NameResourceType = typeof(AuditStrings))]
		public DateTime FailedTime { get { return StartTime; } }

		[LocalizedDisplayName("Duration", NameResourceType = typeof(AuditStrings))]
		public string Duration
		{
			get
			{
				if (EndTime.HasValue)
				{					
					return (EndTime.Value - StartTime).Duration().ToString(@"hh\:mm\:ss");
				}
				else
					return null;
			}
		}
		
		public string AutoLoggedChecked { get { return AutoLogged > 0 ? "checked=\"checked\"" : null; } }

		public string IsQP7Checked { get { return IsQP7 ? "checked=\"checked\"" : null; } }				
		#endregion	

	}
}
