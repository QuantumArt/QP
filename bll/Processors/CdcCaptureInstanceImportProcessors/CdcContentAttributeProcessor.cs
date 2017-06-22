using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.DbColumns;
using Quantumart.QP8.Utils;
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
                try
                {
                    var contentId = Convert.ToInt32(row[ContentAttributeColumnName.ContentId]);
                    var attributeId = Convert.ToInt32(row[ContentAttributeColumnName.AttributeId]);
                    var attributeTypeId = Convert.ToInt32(row[ContentAttributeColumnName.AttributeTypeId]);
                    var linkId = row[ContentAttributeColumnName.LinkId] == DBNull.Value ? (int?)null : Convert.ToInt32(row[ContentAttributeColumnName.LinkId]);
                    var relationType = string.Empty;
                    switch (attributeTypeId)
                    {
                        case 11:
                            relationType = linkId == null ? O2M : M2M;
                            break;
                        case 13:
                            relationType = M2O;
                            break;
                    }

                    return new CdcTableTypeModel
                    {
                        ChangeType = CdcActionType.Schema,
                        Action = (CdcOperationType)Enum.Parse(typeof(CdcOperationType), row[TarantoolCommonConstants.Operation] as string),
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
                                { ContentId, Convert.ToInt32(row[ContentAttributeColumnName.ContentId]) },
                                { InvariantName, GetInvariantName(attributeId) },
                                { ContentInvariantName, GetParentInvariantName(contentId) },
                                { Name, row[ContentAttributeColumnName.AttributeName] as string },
                                { IsIndexed, relationType == O2M },
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
                }
                catch (Exception ex)
                {
                    throw new Exception($"There was an exception for parsing \"{CaptureInstanceName}\" row: {row.ToSimpleDataRow().ToJsonLog()}", ex);
                }
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
