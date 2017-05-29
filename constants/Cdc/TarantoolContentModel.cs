using Quantumart.QP8.Constants.DbColumns;

namespace Quantumart.QP8.Constants.Cdc
{
    public class TarantoolContentModel
    {
        public const string IsForReplication = "isForReplication";
        public const string DefaultFieldsName = "defaultFields";
        public static readonly string ContentId = ContentColumnName.ContentId.ToUpper();
        public static readonly string ContentName = ContentColumnName.ContentName.ToUpper();
        public static readonly string NetContentName = ContentColumnName.NetContentName.ToUpper();

        public static readonly object DefaultFields = new object[]
        {
            new { order = 1, isIndexed = true, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "INT", invariantName = "CONTENT_ITEM_ID", name = "CONTENT_ITEM_ID", netAttributeName = "Id" },
            new { order = 2, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "INT", invariantName = "STATUS_TYPE_ID", name = "STATUS_TYPE_ID", netAttributeName = "StatusTypeId" },
            new { order = 3, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "BIT", invariantName = "VISIBLE", name = "VISIBLE", netAttributeName = "Visible" },
            new { order = 4, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "BIT", invariantName = "ARCHIVE", name = "ARCHIVE", netAttributeName = "Archive" },
            new { order = 5, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "DATETIME", invariantName = "CREATED", name = "CREATED", netAttributeName = "Created" },
            new { order = 6, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "DATETIME", invariantName = "MODIFIED", name = "MODIFIED", netAttributeName = "Modified" },
            new { order = 7, isIndexed = false, isLocalization = false, isSystem = true, isRelation = false, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "INT", invariantName = "LAST_MODIFIED_BY", name = "LAST_MODIFIED_BY", netAttributeName = "LastModifiedBy" }
        };
    }
}
