using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.Communication
{
	public class SingleUserModeSyncService : SyncServiceBase<Db>
	{
		private IUserService _userService;

		public SingleUserModeSyncService(IUserService userService, ICommunicationService communicationService)
			: base(communicationService)
		{
			_userService = userService;
		}

		#region Overrides
		protected override string Key
		{
			get	{ return "SingleUserMode"; }
		}

		protected override string GetMessage()
		{
			var data = DbService.ReadSettings();
			return GetMessage(data);
		}

		protected override string GetMessage(Db data)
		{
			if (data.RecordActions)
			{
				if (data.SingleUserId == QPContext.CurrentUserId)
				{
					return "Single mode for you";
				}
				else
				{
					string userName = _userService.ReadProfile(data.SingleUserId.Value).Name;
					return "Single mode for " + userName;
				}
			}
			else
			{
				return "*****";
			}
		}
		#endregion
	}
}
