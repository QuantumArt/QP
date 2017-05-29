using Quantumart.QP8.Constants.DbColumns;

namespace Quantumart.QP8.Constants.Cdc
{
    public class TarantoolContentArticleModel
    {
        public const string EntityType = "article";
        public const string IsSplitted = "isSplitted";

        public static readonly string ContentItemId = ContentItemColumnName.ContentItemId.ToUpper();
        public static readonly string StatusTypeId = ContentItemColumnName.StatusTypeId.ToUpper();
        public static readonly string Visible = ContentItemColumnName.Visible.ToUpper();
        public static readonly string Archive = ContentItemColumnName.Archive.ToUpper();
        public static readonly string Created = ContentItemColumnName.Created.ToUpper();
        public static readonly string Modified = ContentItemColumnName.Modified.ToUpper();
        public static readonly string LastModifiedBy = ContentItemColumnName.LastModifiedBy.ToUpper();

        public static readonly string AttributeId = ContentDataColumnName.AttributeId.ToUpper();
        public static readonly string ContentDataId = ContentDataColumnName.ContentDataId.ToUpper();
        public static readonly string Data = ContentDataColumnName.Data.ToUpper();

        public static string GetFieldName(object attributeId) => $"field_{attributeId}";

        public static string GetInvariantName(decimal contentId) => $"content_{contentId}";
    }
}
