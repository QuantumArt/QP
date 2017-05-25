using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcContentProcessor : CdcImportProcessor
    {
        private readonly object _defaultFields = new object[]
        {
            new
            {
                order = 1,
                invariantName = "CONTENT_ITEM_ID",
                name = "CONTENT_ITEM_ID",
                isIndexed = true,
                netAttributeName = "Id",
                isLocalization = false,
                isSystem = true,
                storageType = "INT",
                isRelation = false,
                isClassifier = false,
                isPrimaryKey = true,
                isAggregated = false
            },
            new
            {
                order = 2,
                invariantName = "STATUS_TYPE_ID",
                name = "STATUS_TYPE_ID",
                isIndexed = false,
                netAttributeName = "StatusTypeId",
                isLocalization = false,
                isSystem = true,
                storageType = "INT",
                isRelation = false,
                isClassifier = false,
                isPrimaryKey = true,
                isAggregated = false
            },
            new
            {
                order = 3,
                invariantName = "VISIBLE",
                name = "VISIBLE",
                isIndexed = false,
                netAttributeName = "Visible",
                isLocalization = false,
                isSystem = true,
                storageType = "BIT",
                isRelation = false,
                isClassifier = false,
                isPrimaryKey = true,
                isAggregated = false
            },
            new
            {
                order = 4,
                invariantName = "ARCHIVE",
                name = "ARCHIVE",
                isIndexed = false,
                netAttributeName = "Archive",
                isLocalization = false,
                isSystem = true,
                storageType = "BIT",
                isRelation = false,
                isClassifier = false,
                isPrimaryKey = true,
                isAggregated = false
            },
            new
            {
                order = 5,
                invariantName = "CREATED",
                name = "CREATED",
                isIndexed = false,
                netAttributeName = "Created",
                isLocalization = false,
                isSystem = true,
                storageType = "DATETIME",
                isRelation = false,
                isClassifier = false,
                isPrimaryKey = true,
                isAggregated = false
            },
            new
            {
                order = 6,
                invariantName = "MODIFIED",
                name = "MODIFIED",
                isIndexed = false,
                netAttributeName = "Modified",
                isLocalization = false,
                isSystem = true,
                storageType = "DATETIME",
                isRelation = false,
                isClassifier = false,
                isPrimaryKey = true,
                isAggregated = false
            },
            new
            {
                order = 7,
                invariantName = "LAST_MODIFIED_BY",
                name = "LAST_MODIFIED_BY",
                isIndexed = false,
                netAttributeName = "LastModifiedBy",
                isLocalization = false,
                isSystem = true,
                storageType = "INT",
                isRelation = false,
                isClassifier = false,
                isPrimaryKey = true,
                isAggregated = false
            }
        };

        public CdcContentProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row => new CdcTableTypeModel
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
                    EntityType = "content",
                    InvariantName = "CONTENT",
                    Columns = new Dictionary<string, object>
                    {
                        { "CONTENT_ID", (decimal)row["content_id"] },
                        { "CONTENT_NAME", row["content_name"] as string },
                        { "NET_CONTENT_NAME", row["net_content_name"] as string },
                        { "isForReplication", true },
                        { "defaultFields", _defaultFields }
                    }
                }
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
