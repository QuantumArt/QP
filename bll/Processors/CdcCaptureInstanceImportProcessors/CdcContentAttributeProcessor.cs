using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Quantumart.QP8.BLL.Models.NotificationSender;

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
                var attributeId = (decimal)row["attribute_id"];
                var attributeTypeId = (decimal)row["attr_type_id"];
                var linkId = row["link_id"] as decimal?;
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
                    Action = row["operation"] as string,
                    ChangeType = CdcActionType.Schema,
                    TransactionDate = (DateTime)row["transactionDate"],
                    TransactionLsn = row["transactionLsn"] as string,
                    SequenceLsn = row["sequenceLsn"] as string,
                    FromLsn = row["fromLsn"] as string,
                    ToLsn = row["toLsn"] as string,
                    Entity = new CdcEntityModel
                    {
                        EntityType = "content_attribute",
                        InvariantName = "CONTENT_ATTRIBUTE",
                        Columns = new Dictionary<string, object>
                        {
                            { "id", attributeId },
                            { "contentId", (decimal)row["content_id"] },
                            { "invariantName", $"field_{attributeId}" },
                            { "name", row["attribute_name"] as string },
                            { "isIndexed", relationType == "o2m" },
                            { "linkId", linkId },
                            { "isLocalization", (bool)row["is_localization"] },
                            { "isSystem", false },
                            { "storageType", row["attr_type_db"] as string },
                            { "isRelation", string.IsNullOrWhiteSpace(relationType) },
                            { "isClassifier", (bool)row["is_classifier"] },
                            { "type", row["attr_type_name"] as string },
                            { "isPrimaryKey", false },
                            { "relationType", relationType },
                            { "isAggregated", (bool)row["aggregated"] }
                        }
                    }
                };
            }).OrderBy(cdc => cdc.TransactionLsn).ToList();
        }
    }
}
