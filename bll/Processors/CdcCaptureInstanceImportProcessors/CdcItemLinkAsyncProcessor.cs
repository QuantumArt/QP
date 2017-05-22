using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcItemLinkAsyncProcessor : CdcImportProcessor
    {
        public CdcItemLinkAsyncProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                // ReSharper disable ConditionIsAlwaysTrueOrFalse
                var isRev = false;
                var linkId = (decimal)row["link_id"];
                var leftId = (decimal)row["item_id"];
                var rightId = (decimal)row["linked_item_id"];
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
                        EntityType = "item_link",
                        InvariantName = $"item_link_{linkId}" + (isRev ? "_rev" : string.Empty),
                        Columns = new Dictionary<string, object>
                        {
                            { "id", isRev ? rightId : leftId },
                            { "linked_id", isRev ? leftId : rightId }
                        }
                    }
                };

                // ReSharper restore ConditionIsAlwaysTrueOrFalse
            }).ToList();
        }
    }
}
