using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcContentItemProcessor : CdcImportProcessor
    {
        public CdcContentItemProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                var contentId = (decimal)row["content_id"];
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
                        EntityType = "article",
                        InvariantName = $"content_{contentId}",
                        Columns = new Dictionary<string, object>
                        {
                            { "CONTENT_ITEM_ID", (decimal)row["CONTENT_ITEM_ID"] },
                            { "STATUS_TYPE_ID", (decimal)row["STATUS_TYPE_ID"] },
                            { "VISIBLE", (decimal)row["visible"] == 1 },
                            { "ARCHIVE", (decimal)row["archive"] == 1 },
                            { "CREATED", (DateTime)row["created"] },
                            { "MODIFIED", (DateTime)row["modified"] },
                            { "LAST_MODIFIED_BY", (decimal)row["last_modified_by"] }
                        }
                    }
                };
            }).ToList();
        }
    }
}
