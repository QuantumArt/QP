using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Quantumart.QP8.CodeGeneration.Services
{
    public class ModelReader
    {
        Dictionary<int, string> _roleHash = new Dictionary<int, string>();
        Dictionary<int, string> _roles = new Dictionary<int, string>();
        Dictionary<int, int> _rolesInContent = new Dictionary<int, int>();

        public SchemaInfo Schema;
        public List<ContentInfo> Contents;
        public List<AttributeInfo> Attributes;
        public List<LinkInfo> Links;

        public ModelReader(XDocument doc, Action<string> write, bool isStage = true)
        {
            Schema = doc.Descendants("schema").Select(x => new SchemaInfo
            {
                ConnectionStringName = RootUtil.GetAttribute<string>(x, "connectionStringName", true),
                ClassName = RootUtil.GetAttribute<string>(x, "class", true),
                NamespaceName = RootUtil.GetAttribute<string>(x, "namespace", true),
                SiteName = RootUtil.GetAttribute<string>(x, "siteName", true),
                UseLongUrls = RootUtil.GetAttribute<bool>(x, "useLongUrls"),
                ReplaceUrls = RootUtil.GetAttribute<bool>(x, "replaceUrls"),
                IsStageMode = isStage,//GetAttribute<bool>(x, "forStage") 
            }).First();

            Contents = doc.Descendants("schema").First().Descendants("content").Select(x => new ContentInfo
            {
                Id = RootUtil.GetAttribute<int>(x, "id", true),
                Name = RootUtil.GetAttribute<string>(x, "name", true),
                MappedName = RootUtil.GetAttribute<string>(x, "mapped_name", true),
                PluralMappedName = RootUtil.GetAttribute<string>(x, "plural_mapped_name", true),
                UseDefaultFiltration = bool.Parse(x.Attribute("use_default_filtration").Value),
                IsVirtual = RootUtil.GetAttribute<bool>(x, "virtual"),
                SplitArticles = RootUtil.GetAttribute<bool>(x, "split_articles", fallbackValue: true),
                IsStageMode = Schema.IsStageMode,
                Attributes = x.Descendants("attribute").Select(a => new AttributeInfo
                {
                    Id = RootUtil.GetAttribute<int>(a, "id", true),
                    Name = RootUtil.GetAttribute<string>(a, "name", true),
                    MappedName = RootUtil.GetAttribute<string>(a, "mapped_name", true),
                    MappedBackName = RootUtil.GetAttribute<string>(a, "mapped_back_name"),
                    Type = RootUtil.GetAttribute<string>(a, "type", true),
                    Size = RootUtil.GetAttribute<int>(a, "size"),
                    IsLong = RootUtil.GetAttribute<bool>(a, "is_long"),
                    RelatedContentId = RootUtil.GetAttribute<int>(a, "related_content_id"),
                    RelatedAttributeId = RootUtil.GetAttribute<int>(a, "related_attribute_id"),
                    LinkId = RootUtil.GetAttribute<int>(a, "link_id"),
                    ContentId = int.Parse(x.Attribute("id").Value),
                    HasM2O = RootUtil.GetAttribute<bool>(a, "has_m2o")
                }).ToList()
            }).ToList();

            Links = doc.Descendants("schema").First().Descendants("link").Select(x => new LinkInfo
            {
                Id = RootUtil.GetAttribute<int>(x, "id"),
                MappedName = RootUtil.GetAttribute<string>(x, "mapped_name"),
                PluralMappedName = RootUtil.GetAttribute<string>(x, "plural_mapped_name"),
                ContentId = RootUtil.GetAttribute<int>(x, "content_id"),
                LinkedContentId = RootUtil.GetAttribute<int>(x, "linked_content_id"),
                IsSelf = RootUtil.GetAttribute<bool>(x, "self"),
            }).ToList();

            Build(write);
        }

        public ModelReader(string path, Action<string> write, bool isStage = true)
            : this(XDocument.Load(path), write, isStage)
        {
        }

        public ModelReader()
        {
            Schema = new SchemaInfo();
            Contents = new List<ContentInfo>();
            Attributes = new List<AttributeInfo>();
            Links = new List<LinkInfo>();
        }

        public void Build()
        {
            Build(_ => { });
        }
        public void Build(Action<string> write)
        {
            Schema.SchemaNamespace = Schema.ConnectionStringName + Schema.ClassName.Replace(".", "");
            Schema.SchemaContainer = Schema.SchemaNamespace + "StoreContainer";

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
                    Description = string.Format("Auto-generated backing property for field (id: {0})/{1} {2}", item.Id, item.Name, item.MappedBackName),
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
            var candidate = string.IsNullOrEmpty(attr.MappedBackName) ? ("BackwardFor" + attr.MappedName) : attr.MappedBackName;
            var count = contentTo.Attributes.Count(x => x.MappedName.StartsWith(candidate));
            var name = candidate + (count > 0 ? count.ToString() : "");/* contentTo.Attributes.Any(x => x.MappedName == contentFrom.PluralMappedName) ?
                (candidate + (count == 0 ? "" : count.ToString()))
                : (attr.MappedBackName ?? contentFrom.PluralMappedName);*/

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
                Description = string.Format("Auto-generated backing property for {0}/{1}", attr.Id, attr.Name, attr.MappedBackName),
                Content = contentTo,
                RelatedContent = contentFrom,
                RelatedContentId = contentFrom.Id
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
    }
}
