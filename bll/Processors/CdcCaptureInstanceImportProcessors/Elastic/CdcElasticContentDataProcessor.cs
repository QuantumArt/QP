using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using QP8.Infrastructure.Extensions;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Elastic;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.DbColumns;
using Quantumart.QP8.Utils;

namespace Quantumart.QP8.BLL.Processors.CdcCaptureInstanceImportProcessors.Elastic
{
    public class CdcElasticContentDataProcessor : BaseCdcElasticImportProcessor
    {
        public CdcElasticContentDataProcessor(string captureInstanseName)
            : base(captureInstanseName)
        {
        }

        public override List<CdcTableTypeModel> ImportCdcData(string fromLsn = null, string toLsn = null)
        {
            return GetCdcDataTable(fromLsn, toLsn).AsEnumerable().Select(row =>
            {
                try
                {
                    object data = row[ContentDataColumnName.Data] as string;
                    object blobData = row[ContentDataColumnName.BlobData] as string;

                    return new CdcTableTypeModel
                    {
                        ChangeType = CdcActionType.Data,
                        Action = (CdcOperationType)Enum.Parse(typeof(CdcOperationType), Convert.ToString(row[CdcCommonConstants.Operation])),
                        TransactionDate = (DateTime)row[CdcCommonConstants.TransactionDate],
                        TransactionLsn = row[CdcCommonConstants.TransactionLsn] as string,
                        SequenceLsn = row[CdcCommonConstants.SequenceLsn] as string,
                        FromLsn = row[CdcCommonConstants.FromLsn] as string,
                        ToLsn = row[CdcCommonConstants.ToLsn] as string,
                        Entity = new CdcEntityModel
                        {
                            EntityType = ContentDataColumnName.TableName,
                            InvariantName = ContentDataColumnName.TableName.ToUpper(),
                            Columns = new Dictionary<string, object>
                            {
                                { ElasticContentArticleModel.Id, Convert.ToInt32(row[ContentDataColumnName.ContentItemId]) },
                                { ElasticContentArticleModel.FieldId, Convert.ToInt32(row[ContentDataColumnName.AttributeId]) },
                                { ElasticContentArticleModel.ContentId, Convert.ToInt32(row[ContentDataColumnName.ContentId]) },
                                { ElasticContentArticleModel.Data, blobData ?? data }
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
