using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;
using Quantumart.QP8.Constants.Cdc;
using Quantumart.QP8.Constants.Cdc.Enums;
using Quantumart.QP8.Constants.DbColumns;
using static Quantumart.QP8.Constants.Cdc.TarantoolContentArticleModel;

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
                object data = row[ContentDataColumnName.Data] as string;
                object blobData = row[ContentDataColumnName.BlobData] as string;

                var typedb = row[ContentDataColumnName.AttributeTypeDb] as string;
                var typeName = row[ContentDataColumnName.AttributeTypeName] as string;
                if (string.Equals(typedb, "NUMERIC", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.Equals(typeName, "BOOLEAN", StringComparison.OrdinalIgnoreCase))
                    {
                        data = Convert.ToBoolean(Convert.ToInt16(data));
                    }
                    else
                    {
                        data = data == null ? (object)null : Convert.ToDecimal(data, CultureInfo.InvariantCulture);
                    }
                }

                return new CdcTableTypeModel
                {
                    ChangeType = CdcActionType.Data,
                    Action = (CdcOperationType)Enum.Parse(typeof(CdcOperationType), row[TarantoolCommonConstants.Operation] as string),
                    TransactionDate = (DateTime)row[TarantoolCommonConstants.TransactionDate],
                    TransactionLsn = row[TarantoolCommonConstants.TransactionLsn] as string,
                    SequenceLsn = row[TarantoolCommonConstants.SequenceLsn] as string,
                    FromLsn = row[TarantoolCommonConstants.FromLsn] as string,
                    ToLsn = row[TarantoolCommonConstants.ToLsn] as string,
                    Entity = new CdcEntityModel
                    {
                        EntityType = ContentDataColumnName.TableName,
                        InvariantName = ContentDataColumnName.TableName.ToUpper(),
                        Columns = new Dictionary<string, object>
                        {
                            { AttributeId, (int)row[ContentDataColumnName.AttributeId] },
                            { ContentItemId, (int)row[ContentDataColumnName.ContentItemId] },
                            { ContentDataId, (int)row[ContentDataColumnName.ContentDataId] },
                            { Data, blobData ?? data },
                            { Created, (DateTime)row[ContentDataColumnName.Created] },
                            { Modified, (DateTime)row[ContentDataColumnName.Modified] }
                        },
                        MetaData = new Dictionary<string, object>
                        {
                            { IsSplitted, Equals(true, row[ContentDataColumnName.Splitted]) }
                        }
                    }
                };
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
