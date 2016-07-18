using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quantumart.QP8.EntityFramework.Services
{
    public class AttributeInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string MappedName { get; set; }
        public string MappedBackName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
        public bool IsLong { get; set; }
        public int RelatedContentId { get; set; }
        public int RelatedAttributeId { get; set; }
        public int LinkId { get; set; }
        public int ContentId { get; set; }
        public bool HasM2O { get; set; }
        public string OriginalMappedName { get { return (Type == "O2M" && MappedName != null) ? MappedName + "_ID" : MappedName; } }
        public bool IsRelation { get { return IsO2M || IsM2M || IsM2O; } }
        public bool IsLocalization { get; set; }
        public bool IsO2M { get { return Type == "O2M"; } }
        public bool IsM2M { get { return Type == "M2M"; } }
        public bool IsM2O { get { return Type == "M2O"; } }
        public ContentInfo Content { get; set; }
        public bool GenerateLibraryUrl { get { return this.Type == "Image" || this.Type == "File" || this.Type == "Dynamic Image"; } }
        public bool GenerateUploadPath { get { return this.Type == "Image" || this.Type == "File"; } }
        public bool CanContainPlaceholders
        {
            get
            {
                return this.Type == "TextBox" || this.Type == "String" || this.Type == "VisualEdit";
            }
        }
        public bool IsNullable
        {
            get
            {
                return this.Type == "Numeric" || this.Type == "Datetime" || this.Type == "Boolean";
            }
        }

        public string NetType
        {
            get
            {
                switch (Type)
                {
                    case "Textbox":
                    case "String":
                    case "Image":
                    case "VisualEdit":
                    case "Dynamic Image":
                    case "File":
                        return "String";
                    case "Boolean": return "Boolean?";
                    case "Numeric":
                        if (IsLong && Size == 0)
                            return "Int64?";
                        if (Size == 0)
                            return "Int32?";
                        if (IsLong)
                            return "Decimal?";
                        return "Double?";
                    case "O2M":
                        return "Int32?";
                    case "DateTime": return "DateTime?";
                    case "Date": return "DateTime?";
                    case "Time": return "TimeSpan?";
                    default: throw new NotSupportedException("Type is not supported yet: " + this.Type);
                }
            }
        }
        public String ExactType
        {
            get
            {
                switch (Type)
                {
                    case "Textbox":
                    case "String":
                    case "Image":
                    case "VisualEdit":
                    case "Dynamic Image":
                    case "File":
                        return "String";
                    case "Boolean": return "Boolean";
                    case "Numeric":
                        if (IsLong && Size == 0)
                            return "Int64";
                        if (Size == 0)
                            return "Int32";
                        if (IsLong)
                            return "Decimal";
                        return "Double";
                    case "O2M":
                        return "Int32";
                    case "DateTime": return "DateTime";
                    case "Date": return "DateTime";
                    case "Time": return "TimeSpan";
                    default: throw new NotSupportedException("Type is not supported yet: " + this.Type);
                }
            }
        }

        public ContentInfo RelatedContent { get; set; }

        public AttributeInfo RelatedAttribute { get; set; }

        public bool? IsSource { get; set; }

        public bool? IsTarget { get; set; }

        public LinkInfo Link { get; set; }

        public bool ShouldMap { get; set; }

        public string ExplicitMapping { get; set; }
    }
}
