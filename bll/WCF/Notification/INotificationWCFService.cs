using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Quantumart.QP8.BLL.WCF.Notification
{
	/// <summary>
	/// Контракт WCF-сервиса нотификации
	/// </summary>
	[ServiceContract(Namespace = "http://Quantumart.QP8.WCF.Notification")]
	public interface INotificationService : IDisposable
	{
		/// <summary>
		/// Отправить уведомление
		/// </summary>
		[OperationContract(IsOneWay = true)]
		void SendNotificationOneWay(string connectionString, int id, string code);
		/// <summary>
		/// Отправить уведомление
		/// </summary>
		[OperationContract]
		void SendNotification(string connectionString, int id, string code);
	}
}
