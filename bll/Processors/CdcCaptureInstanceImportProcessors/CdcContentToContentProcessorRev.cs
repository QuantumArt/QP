using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcContentToContentRevProcessor : CdcImportProcessor
    {
        private readonly object _defaultFields = new object[]
        {
            new
            {
                invariantName = "id",
                isIndexed = true,
                isLocalization = false,
                isSystem = false,
                storageType = "INT",
                isRelation = true,
                isClassifier = false,
                isPrimaryKey = true,
                isAggregated = false
            },
            new
            {
                invariantName = "linked_id",
                isIndexed = true,
                isLocalization = false,
                isSystem = false,
                storageType = "INT",
                isRelation = true,
                isClassifier = false,
                isPrimaryKey = true,
                isAggregated = false
            }
        };

        public CdcContentToContentRevProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                var linkId = (decimal)row["link_id"];
                return new CdcTableTypeModel
                {
                    Action = row["operation"] as string,
                    ChangeType = CdcActionType.Schema,
                    TransactionDate = (DateTime)row["transactionDate"],
                    TransactionLsn = row["transactionLsn"] as string,
                    SequenceLsn = row["sequenceLsn"] as string,
                    FromLsn = row["fromLsn"] as string,
                    ToLsn = row["toLsn"] as string,
                    Entity = new CdcEntityModel
                    {
                        EntityType = "content_to_content",
                        InvariantName = "CONTENT_TO_CONTENT",
                        Columns = new Dictionary<string, object>
                        {
                            { "linkId", linkId },
                            { "invariantName", $"item_link_{linkId}_rev" },
                            { "leftContentId", (decimal)row["r_content_id"] },
                            { "rightContentId", (decimal)row["l_content_id"] },
                            { "isSymmetric", (bool)row["symmetric"] },
                            { "isReverse", true },
                            { "defaultFields", _defaultFields }
                        }
                    }
                };
            }).ToList();
        }
    }
}
