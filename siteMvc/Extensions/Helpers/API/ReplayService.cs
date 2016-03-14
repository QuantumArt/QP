using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Configuration;
using System.Data.SqlClient;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers.API
{
	public class ReplayService
	{
		private string customerCode;
		private string connectionString;
		private int userId;
		private HashSet<string> identityInsertOptions = new HashSet<string>();
		private XDocument fingerPrintSettings;

		public virtual void OnUpdating(SqlConnection connection) {}
		public virtual void OnUpdated(SqlConnection connection) {}
		
		public ReplayService(string customerCode, int userId, XDocument fingerPrintSettings, HashSet<string> identityInsertOptions)
		{
			if (String.IsNullOrEmpty(customerCode))
				throw new ArgumentNullException("connectionString");
			this.connectionString = QPConfiguration.ConfigConnectionString(customerCode);
			if (String.IsNullOrEmpty(this.connectionString))
				throw new ArgumentException("Customer code is not found");

			this.customerCode = customerCode;
			this.userId = userId;
			this.fingerPrintSettings = fingerPrintSettings;
			this.identityInsertOptions = identityInsertOptions;
		}

		public ReplayService(string customerCode, int userId, XDocument fingerPrintSettings) : this(customerCode, userId, fingerPrintSettings, new HashSet<string>()) { }

		public ReplayService(string customerCode, int userId) : this(customerCode, userId, null) { }

		public string ComputeFingerPrint()
		{
			QPContext.CurrentCustomerCode = customerCode;
			string result = String.Empty;
			using (QPConnectionScope scope = new QPConnectionScope(connectionString, identityInsertOptions))
			{
				result = new RecordReplayHelper().GetFingerPrint(fingerPrintSettings);
			}
			QPContext.CurrentCustomerCode = null;
			return result;
		}

		public void ReplayXml(string xmlText)
		{
			QPContext.CurrentCustomerCode = customerCode;
			using (TransactionScope tscope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
			{
				using (QPConnectionScope scope = new QPConnectionScope(connectionString, identityInsertOptions))
				{
					QPContext.CurrentUserId = userId;
					var helper = new RecordReplayHelper();
					OnUpdating(scope.DbConnection);
					helper.ReplayActionsFromXml(xmlText, userId, fingerPrintSettings);
					OnUpdated(scope.DbConnection);
					QPContext.CurrentUserId = 0;
					tscope.Complete();
				}
			}
			QPContext.CurrentCustomerCode = null;
		}
		
	}
}
