using Quantumart.QP8.Configuration;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.Scheduler.API
{
	public class ConnectionStrings : IConnectionStrings
	{
		private const string ExceptCustomerCodesKey = "ExceptCustomerCodes";
		private readonly ServiceDescriptor _descriptor;

		public ConnectionStrings(ServiceDescriptor descriptor)
		{
			_descriptor = descriptor;
		}

		#region IConnectionStrings implementation
		public IEnumerator<string> GetEnumerator()
		{
			var exceptCodes = GetexceptCustomerCodes();
			return QPConfiguration.ConfigConnectionStrings(_descriptor.Name, exceptCodes).GetEnumerator();
		}

		private static string[] GetexceptCustomerCodes()
		{
			string codes = ConfigurationManager.AppSettings[ExceptCustomerCodesKey];

			if (codes == null)
			{
				return new string[0];
			}
			else
			{
				return codes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion
	}
}