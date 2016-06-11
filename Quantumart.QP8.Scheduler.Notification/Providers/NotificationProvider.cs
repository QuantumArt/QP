using Quantumart.QP8.BLL;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.Notification.Providers
{
	public class NotificationProvider : INotificationProvider
	{
		private const string XmlTemplate = "<?xml version=\"1.0\" encoding=\"utf-8\"?><articles>{0}</articles>";

		public async Task<HttpStatusCode> Notify(NotificationModel notification)
		{
			using (var client = new HttpClient())
			{
				var values = new List<KeyValuePair<string, string>>();
				var newXml = GetXml(notification.NewXmlNodes);
				var oldXml = GetXml(notification.OldXmlNodes);
				values.Add(new KeyValuePair<string, string>("NEW_XML", newXml));
				values.Add(new KeyValuePair<string, string>("OLD_XML", oldXml));

				var content = new FormUrlEncodedContent(values);
				var response = await client.PostAsync(notification.Url, content);
				return response.StatusCode;
			}
		}

		private static string GetXml(IEnumerable<string> nodes)
		{
			return string.Format(XmlTemplate, string.Join(string.Empty, nodes));
		}
	}
}