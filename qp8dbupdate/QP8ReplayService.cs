using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml.Linq;
using qp8dbupdate.Versioning;
using Quantumart.QP8.WebMvc.Extensions.Helpers.API;

namespace qp8dbupdate
{
    public class QP8ReplayService : ReplayService, IDisposable
    {
        private DBUpdateLogEntry _currentData;
        private int _userId;
        private bool _createTable;

        public QP8ReplayService(string customerCode, int userId)
            : base(customerCode, userId) { _userId = userId; }
        public QP8ReplayService(string customerCode, int userId, XDocument fingerPrintSettings)
            : base(customerCode, userId, fingerPrintSettings) { _userId = userId; }
        public QP8ReplayService(string customerCode, int userId, XDocument fingerPrintSettings, HashSet<string> identityInsertOptions)
            : base(customerCode, userId, fingerPrintSettings, identityInsertOptions) { _userId = userId; }


        public void ReplayWithLogging(DBUpdateLogEntry data, bool createTable)
        {
            _currentData = data;
            _currentData.UserId = _userId;
            _createTable = createTable;
            base.ReplayXml(data.Body);
        }


        public override void OnUpdating(SqlConnection connection)
        {
            var manager = new UpdateManager(connection);
            
            if (_createTable)
            {
                manager.EnsureTableExists();
            }

            var similar = manager.FindSimilarEntry(_currentData);

            if (similar != null)
            {
                throw new DbUpdateException("Данное обновление уже было произведено. ", similar);
            }

            base.OnUpdating(connection);
        }

        public override void OnUpdated(SqlConnection connection)
        {
            var manager = new UpdateManager(connection);
            
            manager.SaveLogEntry(_currentData);

            _currentData = null;

            base.OnUpdated(connection);
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_currentData != null)
                _currentData = null;
        }

        #endregion
    }
}
