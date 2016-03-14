using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Quantumart.QP8.BLL.Services.Communication
{
	public interface ICommunicationService
	{
		void Send(string key, object message);
		void SendAll(string key, string message);
	}
}