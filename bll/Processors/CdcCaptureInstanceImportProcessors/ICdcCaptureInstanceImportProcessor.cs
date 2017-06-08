using System.Collections.Generic;
using System.Data;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.DAL.NotificationSender;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public abstract class CdcImportProcessor
    {
        protected internal readonly string CaptureInstanceName;

        protected CdcImportProcessor(string captureInstanceName)
        {
            CaptureInstanceName = captureInstanceName;
        }

        public abstract List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null);

        public virtual DataTable GetCdcDataTable(string fromLsn = null, string toLsn = null) =>
            CdcImportDal.GetCdcTableData(QPConnectionScope.Current.DbConnection, CaptureInstanceName, fromLsn, toLsn);
    }
}
