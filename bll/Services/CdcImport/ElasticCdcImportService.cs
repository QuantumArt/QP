using System.Collections.Generic;
using System.Linq;
using NLog;
using Quantumart.QP8.BLL.Factories;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;

namespace Quantumart.QP8.BLL.Services.CdcImport
{
    public class ElasticCdcImportService : BaseCdcImportService
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public ElasticCdcImportService()
            : base(new ElasticCdcImportFactory())
        {
        }

        public override List<CdcTableTypeModel> GetCdcDataFromTables(string fromLsn = null, string toLsn = null)
        {
            Logger.Trace($"Getting cdc data with fromLsn:{fromLsn} and toLsn:{toLsn}");
            var tablesGroup = new List<List<CdcTableTypeModel>>
            {
                ImportData(CdcCaptureConstants.ContentData, fromLsn, toLsn)
            };

            return tablesGroup.SelectMany(tg => tg).OrderBy(t => t.TransactionLsn).ToList();
        }
    }
}
