using Quantumart.QP8.BLL.Services.Communication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.Communication
{
	public abstract class SyncServiceBase<T> : ISyncService<T>
	{	
		private readonly ICommunicationService _communicationService;

		#region Abstract members
		protected abstract string Key { get; }

		protected abstract string GetMessage();
		protected abstract string GetMessage(T data);
		#endregion

		public SyncServiceBase(ICommunicationService communicationService)
		{
			_communicationService = communicationService;
		}

		#region ISyncService implementation
		public void Sync()
		{
			_communicationService.Send(Key, GetMessage());
		}

		public void Sync(T data)
		{
			_communicationService.SendAll(Key, GetMessage(data));
		}
		#endregion
	}
}
