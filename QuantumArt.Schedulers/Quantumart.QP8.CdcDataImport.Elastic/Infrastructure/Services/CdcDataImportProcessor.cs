using System.Collections.Generic;
using System.Linq;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.BLL.Services.CdcImport;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Cdc;

namespace Quantumart.QP8.CdcDataImport.Elastic.Infrastructure.Services
{
    public class CdcDataImportProcessor : ICdcDataImportProcessor
    {
        private readonly ICdcImportService _cdcImportService;

        public CdcDataImportProcessor(ICdcImportService cdcImportService)
        {
            _cdcImportService = cdcImportService;
        }

        public List<CdcTableTypeModel> GetCdcDataFromTables(string fromLsn = null, string toLsn = null)
        {
            Logger.Log.Trace($"Getting cdc data with fromLsn:{fromLsn} and toLsn:{toLsn}");
            var tablesGroup = new List<List<CdcTableTypeModel>>
            {
                _cdcImportService.ImportData(CdcCaptureConstants.ContentData, fromLsn, toLsn)
            };

            return tablesGroup.SelectMany(tg => tg).OrderBy(t => t.TransactionLsn).ToList();
        }
    }
}
