using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Xml.Linq;
using qp8dbupdate.Infrastructure.Exceptions;
using qp8dbupdate.Infrastructure.Versioning;
using Quantumart.QP8.WebMvc.Extensions.Helpers.API;

namespace qp8dbupdate
{
    public class QpReplayService : ReplayService, IDisposable
    {
        private DbUpdateLogEntry _currentData;
        private readonly int _userId;
        private bool _createTable;

        public QpReplayService(string customerCode, int userId)
            : base(customerCode, userId)
        {
            _userId = userId;
        }

        public QpReplayService(string customerCode, int userId, XDocument fingerPrintSettings)
            : base(customerCode, userId, fingerPrintSettings)
        {
            _userId = userId;
        }

        public QpReplayService(string customerCode, int userId, XDocument fingerPrintSettings, HashSet<string> identityInsertOptions)
            : base(customerCode, userId, fingerPrintSettings, identityInsertOptions)
        {
            _userId = userId;
        }

        public void ReplayWithLogging(DbUpdateLogEntry data, bool createTable)
        {
            _currentData = data;
            _currentData.UserId = _userId;
            _createTable = createTable;
            ReplayXml(data.Body);
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
            new UpdateManager(connection).SaveLogEntry(_currentData);
            _currentData = null;
            base.OnUpdated(connection);
        }

        public void Dispose()
        {
            _currentData = null;
        }
    }
}
