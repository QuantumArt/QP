namespace Quantumart.QP8.Constants.Cdc.Tarantool
{
    public class TarantoolItemLinkAsyncModel
    {
        public const string EntityType = "item_link";
        public const string Id = "id";
        public const string LinkedId = "linked_id";
        public const string LinkId = "linkId";
        public const string IsRev = "isRev";
        public const string IsSelf = "isSelf";

        public static string GetInvariantName(decimal linkId, bool isRev) => isRev ? $"item_link_{linkId}_async_rev" : $"item_link_{linkId}_async";
    }
}
