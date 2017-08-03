using System.Collections.Generic;
using System.Data;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public interface ICdcImportProcessor
    {
        List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null);

        DataTable GetCdcDataTable(string fromLsn = null, string toLsn = null);
    }
}
