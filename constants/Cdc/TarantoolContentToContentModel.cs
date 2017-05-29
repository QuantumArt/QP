namespace Quantumart.QP8.Constants.Cdc
{
    public class TarantoolContentToContentModel
    {
        public const string LinkId = "linkId";
        public const string InvariantName = "invariantName";
        public const string LeftContentId = "leftContentId";
        public const string RightContentId = "rightContentId";
        public const string IsSymmetric = "isSymmetric";
        public const string IsReverse = "isReverse";
        public const string DefaultFieldsName = "defaultFields";

        public static readonly object DefaultFields = new object[]
        {
            new { isIndexed = true, isLocalization = false, isSystem = false, isRelation = true, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "INT", invariantName = "id" },
            new { isIndexed = true, isLocalization = false, isSystem = false, isRelation = true, isClassifier = false, isPrimaryKey = true, isAggregated = false, storageType = "INT", invariantName = "linked_id" }
        };

        public static string GetInvariantName(decimal linkId, bool isRev) => isRev ? $"item_link_{linkId}_rev" : $"item_link_{linkId}";

        public static string GetInvariantAsyncName(decimal linkId, bool isRev) => isRev ? $"item_link_{linkId}_async_rev" : $"item_link_{linkId}_async";
    }
}
