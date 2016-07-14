using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace EFCodeFirstV6Tests
{
    /// <summary>
    /// Класс, который используется в кодогенераторе.
    /// </summary>
    public class ModelReader
    {
        Dictionary<int, string> _roleHash = new Dictionary<int, string>();
        Dictionary<int, string> _roles = new Dictionary<int, string>();
        Dictionary<int, int> _rolesInContent = new Dictionary<int, int>();

        public SchemaInfo Schema;
        public List<ContentInfo> Contents;
        public List<AttributeInfo> Attributes;
        public List<LinkInfo> Links;


        public ModelReader(string path, Action<string> write, bool isStage = true)
        {
            var doc = System.Xml.Linq.XDocument.Load(path);
            Schema = doc.Descendants("schema").Select(x => new SchemaInfo
            {
                ConnectionStringName = GetAttribute<string>(x, "connectionStringName", true),
                ClassName = GetAttribute<string>(x, "class", true),
                NamespaceName = GetAttribute<string>(x, "namespace", true),
                SiteName = GetAttribute<string>(x, "siteName", true),
                UseLongUrls = GetAttribute<bool>(x, "useLongUrls"),
                ReplaceUrls = GetAttribute<bool>(x, "replaceUrls"),
                IsStageMode = isStage,//GetAttribute<bool>(x, "forStage") 
            }).First();

            Schema.SchemaNamespace = Schema.ConnectionStringName + Schema.ClassName.Replace(".", "");
            Schema.SchemaContainer = Schema.SchemaNamespace + "StoreContainer";

            Contents = doc.Descendants("schema").First().Descendants("content").Select(x => new ContentInfo
            {
                Id = GetAttribute<int>(x, "id", true),
                Name = GetAttribute<string>(x, "name", true),
                MappedName = GetAttribute<string>(x, "mapped_name", true),
                PluralMappedName = GetAttribute<string>(x, "plural_mapped_name", true),
                UseDefaultFiltration = bool.Parse(x.Attribute("use_default_filtration").Value),
                IsVirtual = GetAttribute<bool>(x, "virtual"),
                IsStageMode = Schema.IsStageMode,
                Attributes = x.Descendants("attribute").Select(a => new AttributeInfo
                {
                    Id = GetAttribute<int>(a, "id", true),
                    Name = GetAttribute<string>(a, "name", true),
                    MappedName = GetAttribute<string>(a, "mapped_name", true),
                    MappedBackName = GetAttribute<string>(a, "mapped_back_name"),
                    Type = GetAttribute<string>(a, "type", true),
                    Size = GetAttribute<int>(a, "size"),
                    IsLong = GetAttribute<bool>(a, "is_long"),
                    RelatedContentId = GetAttribute<int>(a, "related_content_id"),
                    RelatedAttributeId = GetAttribute<int>(a, "related_attribute_id"),
                    LinkId = GetAttribute<int>(a, "link_id"),
                    ContentId = int.Parse(x.Attribute("id").Value),
                    HasM2O = GetAttribute<bool>(a, "has_m2o")
                }).ToList()
            }).ToList();

            Links = doc.Descendants("schema").First().Descendants("link").Select(x => new LinkInfo
            {
                Id = GetAttribute<int>(x, "id"),
                MappedName = GetAttribute<string>(x, "mapped_name"),
                PluralMappedName = GetAttribute<string>(x, "plural_mapped_name"),
                ContentId = GetAttribute<int>(x, "content_id"),
                LinkedContentId = GetAttribute<int>(x, "linked_content_id"),
                IsSelf = GetAttribute<bool>(x, "self"),
            }).ToList();

            Attributes = Contents.SelectMany(x => x.Attributes).ToList();

            var attributesToDelete = new List<AttributeInfo>();



            foreach (var item in Attributes.Where(x => !string.IsNullOrEmpty(x.MappedBackName) && !Attributes.Any(y => y.RelatedAttributeId == x.Id)).ToList())
            {
                if (item.RelatedContentId != 0 && !Contents.Any(x => x.Id == item.RelatedContentId))
                {
                    if (item.IsO2M)
                    {
                        item.Type = "Numeric";
                        item.ExplicitMapping = item.MappedName;
                        item.MappedName += "_ID";
                        item.ShouldMap = true;
                        item.Size = 0;
                    }
                    else
                    {
                        Attributes.RemoveAll(x => x == item);
                        var content = Contents.FirstOrDefault(x => x.Id == item.ContentId);
                        if (content == null) throw new Exception("Content not found: " + item.ContentId + " for attribute: " + item.Id);
                        content.Attributes.RemoveAll(x => x == item);
                    }

                    continue;
                }

                var newId = Attributes.Max(x => x.Id) + 1;

                var attr = new AttributeInfo
                {
                    Id = newId,
                    Name = item.MappedBackName,
                    MappedName = item.MappedBackName,
                    ContentId = item.RelatedContentId,
                    Description = "Auto-generated backing property for " + item.MappedName,
                    RelatedAttribute = item,
                    Type = (item.Type == "O2M" ? "M2O" : "O2M")
                };


                var relatedContent = Contents.FirstOrDefault(x => x.Id == attr.ContentId);
                var c = Contents.FirstOrDefault(x => x.Id == attr.ContentId);

                if (relatedContent == null) throw new Exception("related Content not found: " + attr.ContentId + " for attribute: " + item.Id + " " + item.MappedBackName);
                if (c == null) throw new Exception("Content not found: " + attr.ContentId + " for attribute: " + item.Id + " " + item.MappedBackName);

                attr.Content = relatedContent;
                attr.RelatedContent = c;

                write(string.Format("added realted attribute  {3}/{4} ({0}) (relative to {2}) to content {1}", attr.Id, relatedContent.Id, item.Id, attr.Name, attr.MappedName));

                Attributes.Add(attr);
                relatedContent.Attributes.Add(attr);

                item.RelatedAttributeId = newId;
            }

            foreach (var item in Attributes)
            {
                item.Content = Contents.First(x => x.Id == item.ContentId);
            }

            foreach (var item in Contents.SelectMany(x => x.Attributes))
            {
                item.Content = Contents.First(x => x.Id == item.ContentId);
            }

            foreach (var item in Attributes.Where(x => x.RelatedAttributeId != 0))
            {
                write(string.Format("process {0} {1}", item.Id, item.Name));
                item.RelatedAttribute = Attributes.FirstOrDefault(x => x.Id == item.RelatedAttributeId);
                if (item.RelatedAttribute != null)
                {
                    item.RelatedAttribute.RelatedAttribute = item;

                    item.RelatedContent = item.RelatedAttribute.Content;

                    item.RelatedAttribute.RelatedContent = item.Content;
                }
            }

            var linksToRemove = new List<LinkInfo>();
            foreach (var link in Links.ToList())
            {
                var contentFrom = Contents.FirstOrDefault(x => x.Id == link.ContentId);

                if (contentFrom == null)
                {
                    linksToRemove.Add(link);
                    write(string.Format("removed link {0} with name {1}", link.Id, link.MappedName));
                    continue;
                    //throw new Exception(string.Format("Link {0} is incorrect, content {1} is not found.", link.Id, link.ContentId));
                }
                var contentTo = Contents.FirstOrDefault(x => x.Id == link.LinkedContentId);


                if (contentTo == null)
                {
                    write(string.Format("removed link {0} with name {1}", link.Id, link.MappedName));
                    linksToRemove.Add(link);
                    continue;
                }
                var attributeFrom = Attributes.FirstOrDefault(x => x.LinkId == link.Id && link.ContentId == x.ContentId);
                var attributeTo = Attributes.FirstOrDefault(x => x.LinkId == link.Id && link.LinkedContentId == x.ContentId && (attributeFrom == null || attributeFrom.Id != x.Id));


                if (attributeFrom == null)
                {
                    attributeFrom = GenM2M(contentTo, contentFrom, link, attributeTo);
                    Attributes.Add(attributeFrom);
                    contentFrom.Attributes.Add(attributeFrom);
                }

                if (attributeTo == null)
                {
                    attributeTo = GenM2M(contentFrom, contentTo, link, attributeFrom);
                    Attributes.Add(attributeTo);
                    contentTo.Attributes.Add(attributeTo);
                }

                attributeTo.RelatedAttributeId = 0;
                attributeFrom.RelatedAttributeId = attributeTo.Id;
                attributeFrom.RelatedAttribute = attributeTo;
                attributeTo.RelatedAttribute = attributeFrom;
                attributeFrom.IsSource = true;
                attributeFrom.Link = link;
                attributeFrom.Content = contentFrom;
                attributeFrom.RelatedContent = contentTo;
                attributeTo.Link = link;
                attributeTo.IsTarget = true;
                attributeTo.Content = contentTo;
                attributeTo.RelatedContent = contentFrom;

            }

            Links.RemoveAll(z => linksToRemove.Contains(z));

            // validation
            foreach (var validatingContent in Contents.ToList())
            {
                foreach (var vatr in validatingContent.Attributes.ToList())
                {
                    if ((vatr.IsM2O || vatr.IsM2M) && vatr.RelatedContent == null)
                    {
                        // удалим это поле
                        validatingContent.Attributes.Remove(vatr);
                        Attributes.Remove(vatr);
                        write(string.Format("removed attribute {0}/{1} from content {2} of type {3}", vatr.Id, vatr.Name, validatingContent.Name, vatr.Type));
                    }
                }
            }
        }

        private AttributeInfo GenM2M(ContentInfo contentFrom, ContentInfo contentTo, LinkInfo link, AttributeInfo attr)
        {
            var candidate = attr.MappedBackName ?? ( "BackwardFor" + attr.MappedName);
            var count = contentTo.Attributes.Count(x => x.MappedName.StartsWith(candidate));
            var name = contentTo.Attributes.Any(x => x.MappedName == contentFrom.PluralMappedName) ?
                (candidate + (count == 0 ? "" : count.ToString()))
                : contentFrom.PluralMappedName;

            var mappedName = name;

            return new AttributeInfo
            {
                Id = Attributes.Max(x => x.Id) + 1,
                Name = name,
                Type = "M2M",
                MappedName = mappedName,
                Link = link,
                LinkId = link.Id,
                ContentId = contentTo.Id,
                Description = "Auto-generated m2m backing property",
                Content = contentTo,
                RelatedContent=contentFrom,
                RelatedContentId=contentFrom.Id
            };

        }

        public bool ValidateAttribute(AttributeInfo attribute, bool throwOnError = false)
        {
            if (attribute.IsM2M || attribute.IsM2O)
            {
                var r = Attributes.FirstOrDefault(x => attribute.Id == x.RelatedAttributeId);
                if (r == null && throwOnError)
                    throw new ArgumentException(string.Format("validation: x => attribute.Id ({0}) == x.RelatedAttributeId", attribute.Id));

                return r != null;
            }

            return true;
        }

        public AttributeInfo GetSource(AttributeInfo attribute)
        {
            if (attribute == null)
                throw new ArgumentException("attribute");

            if (attribute.RelatedAttributeId != 0)
            {
                return attribute;
            }

            var r = Attributes.FirstOrDefault(x => attribute.Id == x.RelatedAttributeId);
            if (r == null)
                throw new ArgumentException(string.Format("x => attribute.Id ({0}) == x.RelatedAttributeId", attribute.Id));

            return r;
        }


        public AttributeInfo GetAnother(AttributeInfo attribute)
        {
            if (attribute == null)
                throw new ArgumentException("attribute");

            var r = Attributes.FirstOrDefault(x => attribute.Id == x.RelatedAttributeId) ?? Attributes.First(x => x.Id == attribute.RelatedAttributeId);
            return r;
        }

        public AttributeInfo GetTarget(AttributeInfo attribute)
        {
            if (attribute.RelatedAttributeId == 0)
            {
                return attribute;
            }

            var r = Attributes.FirstOrDefault(x => x.Id == attribute.RelatedAttributeId);

            if (r == null)
                throw new ArgumentException(string.Format("x => x.Id == attribute.RelatedAttributeId({0}", attribute.RelatedAttributeId));

            return r;
        }

        public string StartCast(AttributeInfo attribute)
        {
            switch (attribute.NetType)
            {
                case "Boolean": return "CAST(";
                case "Double": return "CAST(";
                default: return "     ";
            }
        }

        public string EndCast(AttributeInfo attribute)
        {
            switch (attribute.NetType)
            {
                case "Boolean": return " AS BIT)";
                case "Double": return " AS FLOAT)";
                default: return "";
            }
        }


        public static T GetAttribute<T>(XElement e, string name, bool required = false)
        {
            return Util.GetAttribute<T>(e, name, required);
        }

        public static T GetElementValue<T>(XElement e, string name, bool required = false)
        {
            return Util.GetElementValue<T>(e, name, required);
        }

        public string ToPascal(string input)
        {
            if (input == null) return null;

            switch (input)
            {
                case "STUB_FIELD": return "StubField";
            }

            return string.Join("", input.Split('_').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x))
                .Select(x => x.Length > 1 ? (x[0] + x.Substring(1).ToLower()) : x));
        }

        public static class Util
        {
            public static T GetAttribute<T>(XElement e, string name, bool required = false)
            {
                try
                {
                    var a = e.Attribute(name);
                    if (a == null || a.Value == null || a.Value == "")
                    {
                        if (required) throw new Exception("value should not be null or empty");
                        return default(T);
                    }
                    if (typeof(T) == typeof(bool) && a.Value.Length == 1)
                        return (T)(object)Convert.ToBoolean(Convert.ToInt16(a.Value));

                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    return (T)converter.ConvertFromInvariantString(a.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception("problem with element " + name, ex);
                }
            }

            public static T GetElementValue<T>(XElement e, string name, bool required = false)
            {
                try
                {
                    var a = e.Elements().FirstOrDefault(x => x.Name == name);
                    if (a == null || a.Value == null || a.Value == "")
                    {
                        if (required) throw new Exception("value should not be null or empty");
                        return default(T);
                    }
                    if (typeof(T) == typeof(bool) && a.Value.Length == 1)
                        return (T)(object)Convert.ToBoolean(Convert.ToInt16(a.Value));

                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    return (T)converter.ConvertFromInvariantString(a.Value);
                }
                catch (Exception ex)
                {
                    throw new Exception("problem with element " + name, ex);
                }
            }
        }
    }
    public class SchemaInfo
    {
        public string ConnectionStringName { get; set; }
        public string ClassName { get; set; }
        public string NamespaceName { get; set; }
        public bool UseLongUrls { get; set; }
        public bool ReplaceUrls { get; set; }
        public bool SendNotifications { get; set; }
        public bool DbIndependent { get; set; }
        public string SiteName { get; set; }
        public bool IsPartial { get; set; }
        public string SchemaNamespace { get; set; }
        public string SchemaContainer { get; set; }
        public bool IsStageMode { get; set; }
        public bool LazyLoadingEnabled { get; set; }
    }

    [DebuggerDisplay("{Id}: {MappedName}")]
    public class ContentInfo
    {
        public int Id { get; set; }
        public bool IsStageMode { get; set; }
        public string Name { get; set; }
        public string MappedName { get; set; }
        public string PluralMappedName { get; set; }
        public string Description { get; set; }
        public bool UseDefaultFiltration { get; set; }
        public bool IsVirtual { get; set; }
        public List<AttributeInfo> Attributes { get; set; }
        public IEnumerable<AttributeInfo> Columns { get { return Attributes.Where(x => !x.IsM2M && !x.IsM2O); } }
        public bool DoPostProcessing { get { return !SkipPostProcessing && Attributes.Any(x => x.CanContainPlaceholders); } }
        public bool SkipPostProcessing { get; set; }

        public string TableName
        {
            get
            {
                var name = "content_" + Id;
                if (UseDefaultFiltration)
                    name += (IsStageMode ? "_stage_new" : "_live_new");
                else
                    name += "_united_new";
                return name;
            }
        }
    }

    [DebuggerDisplay("{Id}: {MappedName}")]
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
                        return "Double";
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
                        if (IsLong && Size==0)
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

    public class LinkInfo
    {
        public int Id { get; set; }
        public string MappedName { get; set; }
        public string PluralMappedName { get; set; }
        public int ContentId { get; set; }
        public int LinkedContentId { get; set; }
        public bool IsSelf { get; set; }
    }
}
