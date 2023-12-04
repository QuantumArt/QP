namespace Quantumart.QP8.DAL.DTO
{
    public class CustomFilter
    {
        public const string ArchiveFilter = "Archive";
        public const string VirtualTypeFilter = "VirtualType";
        public const string M2MFilter = "M2M";
        public const string RelationFilter = "Relation";
        public const string BackwardFilter = "Backward";
        public const string FieldFilter = "Field";

        public string Filter { get; set; }
        public string Field { get; set; }
        public object Value { get; set; }

        public static CustomFilter GetArchiveFilter(int value) => new CustomFilter
        {
            Filter = ArchiveFilter,
            Value = value
        };

        public static CustomFilter GetVirtualType(int value) => new CustomFilter
        {
            Filter = VirtualTypeFilter,
            Value = value
        };

        public static CustomFilter GetRelationFilter(int fieldId) => new CustomFilter
        {
            Filter = RelationFilter,
            Value = fieldId
        };

        public static CustomFilter GetBackwardFilter(int articleId, int fieldId) => new CustomFilter
        {
            Filter = BackwardFilter,
            Value = new int[] { articleId, fieldId }
        };

        public static CustomFilter GetFieldFilter(string field, object value) => new CustomFilter
        {
            Filter = FieldFilter,
            Field = field,
            Value = value
        };

        public static CustomFilter GetMtMFilter(object value) => new CustomFilter
        {
            Filter = M2MFilter,
            Value = value
        };

        public override bool Equals(object obj)
        {
            if (obj is CustomFilter filter)
            {
                return filter.Filter == Filter && filter.Field == Field && filter.Value == Value;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }    
}
