namespace Quantumart.QPublishing.Info
{
    public class ContentAttribute
    {
        public int SiteId { get; set; }
        public int ContentId { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AttributeType Type { get; set; }
        public string DbTypeName { get; set; }

        public string FullDbTypeName => DbTypeName == "NVARCHAR" ? $"{DbTypeName}({Size})" : DbTypeName;

        public bool IsBlob => DbTypeName == "NTEXT";

        public bool IsDateTime => DbTypeName == "DATETIME";

        public bool IsNumeric => DbTypeName == "NUMERIC";

        public string DbField => IsBlob ? "BLOB_DATA" : "DATA";

        public string InputTypeName { get; set; }
        public string InputMask { get; set; }
        public int Size { get; set; }
        public string DefaultValue { get; set; }
        public bool Indexed { get; set; }
        public int Order { get; set; }
        public bool Required { get; set; }
        public bool ReadOnly { get; set; }
        public int? RelatedImageId { get; set; }
        public int? RelatedContentId { get; set; }
        public int PersistentId { get; set; }
        public int JoinId { get; set; }
        public int? LinkId { get; set; }
        public bool UseSiteLibrary { get; set; }
        public bool DisableVersionControl { get; set; }
        public string SubFolder { get; set; }
        public string LinqName { get; set; }
        public bool IsClassifier { get; set; }
        public bool Aggregated { get; set; }
        public int? ConstraintId { get; set; }
        public BackRelation BackRelation { get; set; }
        public DynamicImageAttribute DynamicImage { get; set; }
        public SourceAttribute SourceAttribute { get; set; }

    }

    public class BackRelation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ContentId { get; set; }
    }

    public class DynamicImageAttribute
    {
        public int Id { get; set; }
        public short Width { get; set; }
        public short Height { get; set; }
        public string Type { get; set; }
        public bool MaxSize { get; set; }
        public short Quality { get; set; }
        public int BaseImageId { get; set; }
    }

    public class SourceAttribute
    {
        public int Id;
        public int ContentId;
        public bool UseSiteLibrary;
    }
}
