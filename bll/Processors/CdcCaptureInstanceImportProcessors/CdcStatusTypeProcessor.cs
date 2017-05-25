using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcStatusTypeProcessor : CdcImportProcessor
    {
        public CdcStatusTypeProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row => new CdcTableTypeModel
            {
                Action = row["operation"] as string,
                ChangeType = CdcActionType.Data,
                TransactionDate = (DateTime)row["transactionDate"],
                TransactionLsn = row["transactionLsn"] as string,
                SequenceLsn = row["sequenceLsn"] as string,
                FromLsn = row["fromLsn"] as string,
                ToLsn = row["toLsn"] as string,
                Entity = new CdcEntityModel
                {
                    EntityType = "status_type",
                    InvariantName = "STATUS_TYPE",
                    Columns = new Dictionary<string, object>
                    {
                        { "STATUS_TYPE_ID", (decimal)row["status_type_id"] },
                        { "STATUS_TYPE_NAME", row["status_type_name"] as string }
                    }
                }
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
