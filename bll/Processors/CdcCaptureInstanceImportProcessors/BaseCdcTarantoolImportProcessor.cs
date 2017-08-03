using System.Collections.Generic;
using System.Data;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.CdcImport;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public abstract class BaseCdcTarantoolImportProcessor : ICdcImportProcessor
    {
        protected internal readonly string CaptureInstanceName;

        protected BaseCdcTarantoolImportProcessor(string captureInstanceName)
        {
            CaptureInstanceName = captureInstanceName;
        }

        public abstract List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null);

        public DataTable GetCdcDataTable(string fromLsn = null, string toLsn = null) =>
            CdcTarantoolImportDal.GetCdcTableData(QPConnectionScope.Current.DbConnection, CaptureInstanceName, fromLsn, toLsn);
    }
}
