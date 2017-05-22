using System;
using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure.Extensions;
using QP8.Infrastructure.Logging;
using QP8.Infrastructure.Logging.Extensions;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants;
using Quantumart.QP8.DAL.NotificationSender;

namespace Quantumart.QP8.BLL.Services.NotificationSender
{
    public class CdcImportService
    {
        public string GetMaxLsn() =>
            CdcImportDal.GetMaxLsn(QPConnectionScope.Current.DbConnection);

        public string GetLastExecutedLsn() =>
            CdcImportDal.GetLastExecutedLsn(QPConnectionScope.Current.DbConnection);

        public void PostLastExecutedLsn(string providerUrl, string lastPushedLsn, string lastExecutedLsn)
        {
            CdcImportDal.PostLastExecutedLsn(QPConnectionScope.Current.DbConnection, providerUrl, lastPushedLsn, lastExecutedLsn);
        }

        public List<CdcTableTypeModel> ImportData(string captureInstance, string fromLsn = null, string toLsn = null) =>
            CdcCaptureInstanceImportFactory.Create(captureInstance).ImportCdcData(fromLsn, toLsn).LogTraceFormat($"Imported cdc table ({captureInstance}) data object: {{0}}");

        public List<CdcTableTypeModel> GetCdcDataFromTables(string fromLsn = null, string toLsn = null)
        {
            Logger.Log.Trace($"Getting cdc data for with fromLsn:{fromLsn} and toLsn:{toLsn}");
            var tablesGroup = new List<List<CdcTableTypeModel>>
            {
                ImportData(CdcCaptureConstants.Content, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.ContentAttribute, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.ContentToContent, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.ContentToContentRev, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.StatusType, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.ItemToItem, fromLsn, toLsn),
                ImportData(CdcCaptureConstants.ItemLinkAsync, fromLsn, toLsn),
                GetContentArticleDto(fromLsn, toLsn).ToList()
            };

            return tablesGroup.SelectMany(tg => tg).OrderBy(t => t.TransactionLsn).ToList();
        }

        private IEnumerable<CdcTableTypeModel> GetContentArticleDto(string fromLsn = null, string toLsn = null)
        {
            var contentItemTable = ImportData(CdcCaptureConstants.ContentItem, fromLsn, toLsn);
            var contentDataTable = ImportData(CdcCaptureConstants.ContentData, fromLsn, toLsn);
            for (var i = 0; i < contentItemTable.Count; i++)
            {
                var item = contentItemTable[i];
                var nextItem = contentItemTable.Count - 1 == i ? null : contentItemTable[i + 1];
                var fieldColumns = contentDataTable
                    .Where(cdt => cdt.Action == item.Action)
                    .Where(cdt => cdt.TransactionLsn == item.TransactionLsn)
                    .Where(cdt => string.Compare(cdt.SequenceLsn, item.SequenceLsn, StringComparison.Ordinal) == 1)
                    .Where(cdt => nextItem == null || string.Compare(cdt.SequenceLsn, nextItem.SequenceLsn, StringComparison.Ordinal) == -1)
                    .OrderBy(cdt => cdt.SequenceLsn)
                    .Select(cdt => new KeyValuePair<string, object>($"field_{cdt.Entity.Columns["ATTRIBUTE_ID"]}", cdt.Entity.Columns["DATA"]));

                item.Entity.Columns.AddRange(fieldColumns);
                yield return item;
            }
        }
    }
}
