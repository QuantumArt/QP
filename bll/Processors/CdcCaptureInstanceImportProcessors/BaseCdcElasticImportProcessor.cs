using System.Collections.Generic;
using System.Data;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.CdcImport;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public abstract class BaseCdcElasticImportProcessor : ICdcImportProcessor
    {
        protected internal readonly string CaptureInstanceName;

        protected BaseCdcElasticImportProcessor(string captureInstanceName)
        {
            CaptureInstanceName = captureInstanceName;
        }

        public abstract List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null);

        public DataTable GetCdcDataTable(string fromLsn = null, string toLsn = null) =>
            CdcElasticImportDal.GetCdcTableData(QPConnectionScope.Current.DbConnection, CaptureInstanceName, fromLsn, toLsn);
    }
}
