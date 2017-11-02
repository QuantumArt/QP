using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.Cdc.Tarantool;
using Quantumart.QP8.Constants.DbColumns;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors.Tarantool
{
    public sealed class CdcTarantoolContentAttributeAsyncProcessor : BaseCdcTarantoolImportProcessor
    {
        public CdcTarantoolContentAttributeAsyncProcessor(string captureInstanseName)
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
                            relationType = linkId == null ? TarantoolContentAttributeModel.O2M : TarantoolContentAttributeModel.M2M;
                            break;
                        case 13:
                            relationType = TarantoolContentAttributeModel.M2O;
                            break;
                    }

                    return new CdcTableTypeModel
                    {
                        ChangeType = CdcActionType.Schema,
                        Action = (CdcOperationType)Enum.Parse(typeof(CdcOperationType), Convert.ToString(row[CdcCommonConstants.Operation])),
                        TransactionDate = (DateTime)row[CdcCommonConstants.TransactionDate],
                        TransactionLsn = row[CdcCommonConstants.TransactionLsn] as string,
                        SequenceLsn = row[CdcCommonConstants.SequenceLsn] as string,
                        FromLsn = row[CdcCommonConstants.FromLsn] as string,
                        ToLsn = row[CdcCommonConstants.ToLsn] as string,
                        Entity = new CdcEntityModel
                        {
                            EntityType = ContentAttributeColumnName.TableName,
                            InvariantName = ContentAttributeColumnName.TableName.ToUpper(),
                            Columns = new Dictionary<string, object>
                            {
                                { TarantoolContentAttributeModel.Id, attributeId },
                                { TarantoolContentAttributeModel.ContentId, Convert.ToInt32(row[ContentAttributeColumnName.ContentId]) },
                                { TarantoolContentAttributeModel.InvariantName, TarantoolContentAttributeModel.GetInvariantName(attributeId) },
                                { TarantoolContentAttributeModel.ContentInvariantName, TarantoolContentAttributeModel.GetContentInvariantName(contentId, true) },
                                { TarantoolContentAttributeModel.Name, row[ContentAttributeColumnName.AttributeName] as string },
                                { TarantoolContentAttributeModel.IsIndexed, relationType == TarantoolContentAttributeModel.O2M },
                                { TarantoolContentAttributeModel.LinkId, linkId },
                                { TarantoolContentAttributeModel.IsLocalization, (bool)row[ContentAttributeColumnName.IsLocalization] },
                                { TarantoolContentAttributeModel.IsSystem, false },
                                { TarantoolContentAttributeModel.StorageType, row[ContentAttributeColumnName.AttributeTypeDb] as string },
                                { TarantoolContentAttributeModel.IsRelation, string.IsNullOrWhiteSpace(relationType) },
                                { TarantoolContentAttributeModel.IsClassifier, (bool)row[ContentAttributeColumnName.IsClassifier] },
                                { TarantoolContentAttributeModel.AttributeType, row[ContentAttributeColumnName.AttributeTypeName] as string },
                                { TarantoolContentAttributeModel.IsPrimaryKey, false },
                                { TarantoolContentAttributeModel.AttributeRelationType, relationType },
                                { TarantoolContentAttributeModel.IsAggregated, (bool)row[ContentAttributeColumnName.IsAggregated] }
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
