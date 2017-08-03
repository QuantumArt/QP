namespace Quantumart.QP8.Constants.Cdc.Tarantool
{
    public class TarantoolContentAttributeModel
    {
        public const string Id = "id";
        public const string ContentId = "contentId";
        public const string InvariantName = "invariantName";
        public const string ContentInvariantName = "contentInvariantName";
        public const string Name = "name";
        public const string IsIndexed = "isIndexed";
        public const string LinkId = "linkId";
        public const string IsLocalization = "isLocalization";
        public const string IsSystem = "isSystem";
        public const string StorageType = "storageType";
        public const string IsRelation = "isRelation";
        public const string IsClassifier = "isClassifier";
        public const string AttributeType = "type";
        public const string IsPrimaryKey = "isPrimaryKey";
        public const string AttributeRelationType = "relationType";
        public const string IsAggregated = "isAggregated";

        public const string O2M = "O2M";
        public const string M2O = "M2O";
        public const string M2M = "M2M";

        public static string GetInvariantName(decimal attributeId) => $"field_{attributeId}";

        public static string GetContentInvariantName(decimal contentId, bool isAsync) => isAsync ? $"content_{contentId}_async" : $"content_{contentId}";
    }
}
