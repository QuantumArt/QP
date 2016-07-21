using System;
using System.Collections.Generic;
using System.Transactions;
using System.Xml.Linq;
using Quantumart.QP8.BLL;
using Quantumart.QP8.Configuration;
using System.Data.SqlClient;

namespace Quantumart.QP8.WebMvc.Extensions.Helpers.API
{
    public class ReplayService
    {
        private readonly string _customerCode;
        private readonly string _connectionString;
        private readonly int _userId;
        private readonly HashSet<string> _identityInsertOptions;
        private readonly XDocument _fingerPrintSettings;

        public virtual void OnUpdating(SqlConnection connection) {}

        public virtual void OnUpdated(SqlConnection connection) {}

        public ReplayService(string customerCode, int userId, XDocument fingerPrintSettings, HashSet<string> identityInsertOptions, bool passConnectionString)
        {
            if (string.IsNullOrEmpty(customerCode))
            {
                throw new ArgumentNullException(nameof(customerCode));
            }

            _connectionString = passConnectionString ? customerCode : QPConfiguration.ConfigConnectionString(customerCode);
            if (string.IsNullOrEmpty(_connectionString))
            {
                throw new ArgumentException("Customer code is not found");
            }

            _customerCode = customerCode;
            _userId = userId;
            _fingerPrintSettings = fingerPrintSettings;
            _identityInsertOptions = identityInsertOptions;
        }

        public ReplayService(string customerCode, int userId, XDocument fingerPrintSettings, HashSet<string> identityInsertOptions) : this(customerCode, userId, fingerPrintSettings, identityInsertOptions, false) { }

        public ReplayService(string customerCode, int userId, XDocument fingerPrintSettings) : this(customerCode, userId, fingerPrintSettings, new HashSet<string>(), false) { }

        public ReplayService(string customerCode, int userId, bool passConnectionString) : this(customerCode, userId, null, new HashSet<string>(), passConnectionString) { }

        public ReplayService(string customerCode, int userId) : this(customerCode, userId, null) { }

        public string ComputeFingerPrint()
        {
            QPContext.CurrentCustomerCode = _customerCode;

            string result;
            using (new QPConnectionScope(_connectionString, _identityInsertOptions))
            {
                result = new RecordReplayHelper().GetFingerPrint(_fingerPrintSettings);
            }

            QPContext.CurrentCustomerCode = null;
            return result;
        }

        public void ReplayXml(string xmlText)
        {
            QPContext.CurrentCustomerCode = _customerCode;
            using (var tscope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                using (var scope = new QPConnectionScope(_connectionString, _identityInsertOptions))
                {
                    QPContext.CurrentUserId = _userId;
                    var helper = new RecordReplayHelper();
                    OnUpdating(scope.DbConnection);
                    helper.ReplayActionsFromXml(xmlText, _userId, _fingerPrintSettings);
                    OnUpdated(scope.DbConnection);
                    QPContext.CurrentUserId = 0;
                    tscope.Complete();
                }
            }

            QPContext.CurrentCustomerCode = null;
        }
    }
}
