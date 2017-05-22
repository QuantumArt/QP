using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcContentDataProcessor : CdcImportProcessor
    {
        public CdcContentDataProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                var data = row["data"] as string;
                var blobData = row["blob_data"] as string;
                return new CdcTableTypeModel
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
                        EntityType = "content_data",
                        InvariantName = "CONTENT_DATA",
                        Columns = new Dictionary<string, object>
                        {
                            { "ATTRIBUTE_ID", (decimal)row["attribute_id"] },
                            { "CONTENT_ITEM_ID", (decimal)row["content_item_id"] },
                            { "CONTENT_DATA_ID", (decimal)row["content_data_id"] },
                            { "DATA", blobData ?? data },
                            { "CREATED", (DateTime)row["created"] },
                            { "MODIFIED", (DateTime)row["modified"] }
                        }
                    }
                };
            }).ToList();
        }
    }
}
