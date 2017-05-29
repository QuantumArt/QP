using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.DbColumns;
using static Quantumart.QP8.Constants.Cdc.TarantoolContentAttributeModel;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors
{
    public class CdcContentAttributeProcessor : CdcImportProcessor
    {
        public CdcContentAttributeProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                var attributeId = (decimal)row[ContentAttributeColumnName.AttributeId];
                var attributeTypeId = (decimal)row[ContentAttributeColumnName.AttributeTypeId];
                var linkId = row[ContentAttributeColumnName.LinkId] as decimal?;
                var relationType = string.Empty;
                switch (attributeTypeId)
                {
                    case 11:
                        relationType = linkId == null ? "o2m" : "m2m";
                        break;
                    case 13:
                        relationType = "m2o";
                        break;
                }

                return new CdcTableTypeModel
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
                        EntityType = ContentAttributeColumnName.TableName,
                        InvariantName = ContentAttributeColumnName.TableName.ToUpper(),
                        Columns = new Dictionary<string, object>
                        {
                            { Id, attributeId },
                            { ContentId, (decimal)row[ContentAttributeColumnName.ContentId] },
                            { InvariantName, $"field_{attributeId}" },
                            { Name, row[ContentAttributeColumnName.AttributeName] as string },
                            { IsIndexed, relationType == "o2m" },
                            { LinkId, linkId },
                            { IsLocalization, (bool)row[ContentAttributeColumnName.IsLocalization] },
                            { IsSystem, false },
                            { StorageType, row[ContentAttributeColumnName.AttributeTypeDb] as string },
                            { IsRelation, string.IsNullOrWhiteSpace(relationType) },
                            { IsClassifier, (bool)row[ContentAttributeColumnName.IsClassifier] },
                            { AttributeType, row[ContentAttributeColumnName.AttributeTypeName] as string },
                            { IsPrimaryKey, false },
                            { AttributeRelationType, relationType },
                            { IsAggregated, (bool)row[ContentAttributeColumnName.IsAggregated] }
                        }
                    }
                };
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
