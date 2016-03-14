using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Net;
using Quantumart.QP8.Security;
using System.Web.Security;
using System.Web;

namespace Quantumart.QP8.BLL.WCF.Notification
{		
	internal partial class NotificationWCFServiceClient : ClientBase<INotificationService>, INotificationService
	{

		public NotificationWCFServiceClient()
		{
		}

		public NotificationWCFServiceClient(string endpointConfigurationName) :
			base(endpointConfigurationName)
		{
		}

		public NotificationWCFServiceClient(string endpointConfigurationName, string remoteAddress) :
			base(endpointConfigurationName, remoteAddress)
		{
		}

		public NotificationWCFServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) :
			base(endpointConfigurationName, remoteAddress)
		{
		}

		public NotificationWCFServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) :
			base(binding, remoteAddress)
		{
		}

		public void SendNotificationOneWay(string connectionString, int id, string code)
		{
			AuthenticatedInvoke(() => base.Channel.SendNotificationOneWay(connectionString, id, code));			
		}

		public void SendNotification(string connectionString, int id, string code)
		{
			AuthenticatedInvoke(() => base.Channel.SendNotification(connectionString, id, code));
		}

		/// <summary>
		/// Добавить аутентификационный cookie в запрос
		/// </summary>
		/// <param name="operation"></param>
		private void AuthenticatedInvoke(Action operation)
		{
			if (HttpContext.Current.Request.IsAuthenticated)
			{
				HttpRequestMessageProperty httpRequestProperty = new HttpRequestMessageProperty();
				httpRequestProperty.Headers.Add(HttpRequestHeader.Cookie, String.Concat(FormsAuthentication.FormsCookieName, "=", HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName].Value));

				using (OperationContextScope scope = new OperationContextScope(this.InnerChannel))
				{
					OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = httpRequestProperty;
					operation();
				}
			}
			else
				operation();
		}
	}

}
