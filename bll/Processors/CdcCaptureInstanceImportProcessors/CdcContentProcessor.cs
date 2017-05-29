using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using static Quantumart.QP8.Constants.DbColumns.ContentColumnName;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcContentProcessor : CdcImportProcessor
    {
        private readonly object _defaultFields = new object[]
        {
            new { order = 1, isIndexed = true, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "INT", invariantName = "CONTENT_ITEM_ID", name = "CONTENT_ITEM_ID", netAttributeName = "Id" },
            new { order = 2, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "INT", invariantName = "STATUS_TYPE_ID", name = "STATUS_TYPE_ID", netAttributeName = "StatusTypeId" },
            new { order = 3, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "BIT", invariantName = "VISIBLE", name = "VISIBLE", netAttributeName = "Visible" },
            new { order = 4, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "BIT", invariantName = "ARCHIVE", name = "ARCHIVE", netAttributeName = "Archive" },
            new { order = 5, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "DATETIME", invariantName = "CREATED", name = "CREATED", netAttributeName = "Created" },
            new { order = 6, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "DATETIME", invariantName = "MODIFIED", name = "MODIFIED", netAttributeName = "Modified" },
            new { order = 7, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "INT", invariantName = "LAST_MODIFIED_BY", name = "LAST_MODIFIED_BY", netAttributeName = "LastModifiedBy" }
        };

        public CdcContentProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row => new CdcTableTypeModel
            {
                ChangeType = CdcActionType.Schema,
                Action = row[TarantoolCommonConstants.Operation] as string,
                TransactionDate = (DateTime)row[TarantoolCommonConstants.TransactionDate],
                TransactionLsn = row[TarantoolCommonConstants.TransactionLsn] as string,
                SequenceLsn = row[TarantoolCommonConstants.SequenceLsn] as string,
                FromLsn = row[TarantoolCommonConstants.FromLsn] as string,
                ToLsn = row[TarantoolCommonConstants.ToLsn] as string,
                Entity = new CdcEntityModel
                {
                    EntityType = TableName,
                    InvariantName = TableName.ToUpper(),
                    Columns = new Dictionary<string, object>
                    {
                        { ContentId.ToUpper(), (decimal)row[ContentId] },
                        { ContentName.ToUpper(), row[ContentName] as string },
                        { NetContentName.ToUpper(), row[NetContentName] as string },
                        { TarantoolContentModel.IsForReplication, true },
                        { TarantoolContentModel.DefaultFields, _defaultFields }
                    }
                }
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
