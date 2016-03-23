using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using Quantumart.QPublishing.Info;
using Quantumart.QPublishing.Resizer;

namespace Quantumart.QPublishing.Database
{
    // ReSharper disable once InconsistentNaming
    public partial class DBConnector
    {
        public void AddUpdateArticles(int contentId, IEnumerable<Dictionary<string, string>> values, int lastModifiedBy)
        {
            var content = GetContentObject(contentId);
            if (content == null)
            {
                throw new Exception($"Content not found (ID = {contentId})");
            }

            if (content.VirtualType > 0)
            {
                throw new Exception($"Cannot modify virtual content (ID = {contentId})");
            }

            var arrValues = values as Dictionary<string, string>[] ?? values.ToArray();
            var names =
                new HashSet<string>(arrValues.SelectMany(n => n.Keys).Distinct().Select(n => n.ToLowerInvariant()));
            var fullAttrs = GetContentAttributeObjects(contentId).ToArray();
            var attrs = fullAttrs.Where(n => names.Contains(n.Name.ToLowerInvariant())).ToArray();

            var existingIds = values.Select(n => int.Parse(n[SystemColumnNames.Id])).Where(n => n != 0).ToArray();
            var versionIdsToRemove = GetVersionIdsToRemove(existingIds, content.MaxVersionNumber);

            var doc = GetImportContentItemDocument(arrValues, content);
            int[] newIds;
            MassUpdateContentItem(contentId, arrValues, lastModifiedBy, doc, out newIds);

            CreateDynamicImages(arrValues, fullAttrs);

            ValidateConstraints(arrValues, fullAttrs);
            var dataDoc = GetMassUpdateContentDataDocument(arrValues, attrs, fullAttrs, newIds, content);
            ImportContentData(dataDoc);

            var attrString = string.Join(",", attrs.Select(n => n.Id.ToString()).ToArray());
            ReplicateData(arrValues, attrString);

            var manyToManyAttrs = attrs.Where(n => n.Type == AttributeType.Relation && n.LinkId.HasValue).ToArray();
            if (manyToManyAttrs.Any())
            {
                var linkDoc = GetImportItemLinkDocument(arrValues, manyToManyAttrs);
                ImportItemLink(linkDoc);
            }

            CreateFilesVersions(arrValues, existingIds, contentId);

            foreach (var id in versionIdsToRemove)
            {
                var oldFolder = GetVersionFolderForContent(contentId, id);
                if (Directory.Exists(oldFolder))
                    Directory.Delete(oldFolder, true);
            }
        }

        private int[] GetVersionIdsToRemove(int[] ids, int maxNumber)
        {

            var cmd = new SqlCommand(@"  select content_item_version_id from
                (
                    select content_item_id, content_item_version_id,
                    row_number() over(partition by civ.content_item_id order by civ.content_item_version_id desc) as num
                    from content_item_version civ
                    where content_item_id in (select id from @ids)
                    ) c
                    where c.num >= @maxNumber")
            {
                CommandType = CommandType.Text
            };
            cmd.Parameters.AddWithValue("@maxNumber", maxNumber);
            cmd.Parameters.AddWithValue("@ids", IdsToDataTable(ids));
            return GetRealData(cmd).AsEnumerable().Select(n => (int) n.Field<decimal>("content_item_version_id")).ToArray();
        }

        private void CreateDynamicImages(Dictionary<string, string>[] arrValues, ContentAttribute[] fullAttrs)
        {
            foreach (var dynImageAttr in fullAttrs.Where(n => n.RelatedImageId.HasValue))
            {
                if (dynImageAttr.RelatedImageId == null) continue;
                var imageAttr = fullAttrs.Single(n => n.Id == dynImageAttr.RelatedImageId.Value);
                var attrDir = GetDirectoryForFileAttribute(imageAttr.Id);
                var contentDir = GetContentLibraryDirectory(imageAttr.SiteId, imageAttr.ContentId);
                foreach (var article in arrValues)
                {
                    string image;
                    if (article.TryGetValue(imageAttr.Name, out image))
                    {
                        DynamicImageCreator.CreateDynamicImage(
                            contentDir,
                            attrDir,
                            image,
                            dynImageAttr.Id,
                            dynImageAttr.DynamicImage.Width,
                            dynImageAttr.DynamicImage.Height,
                            dynImageAttr.DynamicImage.Quality,
                            dynImageAttr.DynamicImage.Type,
                            dynImageAttr.DynamicImage.MaxSize
                            );

                    }
                    article[dynImageAttr.Name] = DynamicImage.GetDynamicImageRelUrl(image, dynImageAttr.Id, dynImageAttr.DynamicImage.Type);
                }
            }
        }

        private void CreateFilesVersions(IEnumerable<Dictionary<string, string>> values, int[] ids, int contentId)
        {
            var fileAttrs = GetFilesAttributesForVersionControl(contentId).ToArray();
            if (fileAttrs.Any())
            {
                var newVersionIds = GetLatestVersionIds(ids);
                var fileAttrIds = fileAttrs.Select(n => n.Id).ToArray();
                var fileAttrDirs = fileAttrs.ToDictionary(n => n.Name, m => GetDirectoryForFileAttribute(m.Id));
                var currentVersionFolder = GetCurrentVersionFolderForContent(contentId);
                if (newVersionIds.Any())
                {
                    var files = GetVersionDataValues(newVersionIds, fileAttrIds).AsEnumerable().Select(n => new
                    {
                        FieldId = (int)n.Field<decimal>("attribute_id"),
                        VersionId = (int)n.Field<decimal>("content_item_version_id"),
                        Data = n.Field<string>("data")
                    })
                    .Where(n => !String.IsNullOrEmpty(n.Data))
                    .Select(n => new FileToCopy
                    {
                        Name = Path.GetFileName(n.Data),
                        Folder = currentVersionFolder,
                        ToFolder = GetVersionFolderForContent(contentId, n.VersionId)
                    }).Distinct().ToArray();

                    CopyArticleFiles(files);
                }
                var strIds = new HashSet<string>(ids.Select(n => n.ToString()));
                var newFiles = values
                    .Where(n => strIds.Contains(n[SystemColumnNames.Id]))
                    .SelectMany(n => n)
                    .Where(n => fileAttrDirs.ContainsKey(n.Key) && !String.IsNullOrEmpty(n.Value))
                    .Distinct()
                    .Select(n => new FileToCopy
                    {
                        Name = n.Value,
                        Folder = fileAttrDirs[n.Key],
                        ToFolder = currentVersionFolder
                    }
                    ).ToArray();

                CopyArticleFiles(newFiles);
            }

        }

        private void ValidateConstraints(IEnumerable<Dictionary<string, string>> values, IEnumerable<ContentAttribute> attrs)
        {
            var validatedAttrs = attrs.Where(n => n.ConstraintId.HasValue).ToArray();
            if (validatedAttrs.Any())
            {
                var constraints = validatedAttrs.GroupBy(n => n.ConstraintId).Select(n => new
                {
                    Id = (int) n.Key,
                    Attrs = n.ToArray()
                }).ToArray();

                foreach (var constraint in constraints)
                {
                    XDocument validatedDataDoc = GetValidatedDataDocument(values, constraint.Attrs);
                    SelfValidate(validatedDataDoc, constraint.Attrs);
                    ValidateConstraint(validatedDataDoc, constraint.Attrs);
                }
            }
        }

        private void SelfValidate(XDocument validatedDataDoc, ContentAttribute[] attrs)
        {
            var items = validatedDataDoc.Root.Descendants("items").ToArray();
            var set = new HashSet<string>();
            foreach (var item in items)
            {
                var str = item.ToString();
                if (set.Contains(str))
                {
                    var msg = String.Join(", ", item.Descendants("DATA").Select(n => $"[{n.Attribute("name")}] = '{n.Value}'"));
                    throw new QPInvalidAttributeException($"Unique constraint violation between articles being added/updated: " + msg);
                }
                set.Add(str);
            }
        }

        private XDocument GetValidatedDataDocument(IEnumerable<Dictionary<string, string>> values, IEnumerable<ContentAttribute> attrs)
        {
            var result = new XDocument();
            var root = new XElement("items", values.Select(m =>
            {
                var temp = new XElement("item");
                temp.Add(new XAttribute("id", m[SystemColumnNames.Id]));
                temp.Add(attrs.Select(n =>
                {
                    string value;
                    if (!m.TryGetValue(n.Name, out value)) return null;
                    var elem = new XElement("DATA", value);
                    elem.Add(new XAttribute("name", n.Name));
                    elem.Add(new XAttribute("id", n.Id));
                    return elem;
                }));
                return temp;
            }));
            result.Add(root);
            return result;
        }

        private void ValidateConstraint(XDocument validatedDataDoc, ContentAttribute[] attrs)
        {
            var sb = new StringBuilder();
            int contentId = attrs[0].ContentId;
            var attrNames = string.Join(", ", attrs.Select(n => n.Name));
            sb.AppendLine($"WITH X(CONTENT_ITEM_ID, {attrNames})");
            sb.AppendLine(@"AS (  SELECT doc.col.value('./@id', 'int') CONTENT_ITEM_ID");
            foreach (var attr in attrs)
            {
                sb.AppendLine($",doc.col.value('(DATA)[@id={attr.Id}]', '{attr.FullDbTypeName}') {attr.Name}");
            }
            sb.AppendLine("FROM @xmlParameter.nodes('/ITEMS/ITEM') doc(col))");
            sb.AppendLine($" SELECT c.CONTENT_ITEM_ID FROM dbo.CONTENT_{contentId}_UNITED c INNER JOIN X ON c.CONTENT_ITEM_ID = X.CONTENT_ITEM_ID");
            foreach (var attr in attrs)
            {
                sb.AppendLine($"AND c.[{attr.Name}] = X.[{attr.Name}]");
            }

            var cmd = new SqlCommand(sb.ToString())
            {
                CommandTimeout = 120,
                CommandType = CommandType.Text
            };
            cmd.Parameters.Add(new SqlParameter("@xmlParameter", SqlDbType.Xml) { Value = validatedDataDoc.ToString(SaveOptions.None) });
            var conflictIds = GetRealData(cmd).AsEnumerable().Select(n => (int) n.Field<decimal>("CONTENT_ITEM_ID")).ToArray();
            if (conflictIds.Any())
                throw new QPInvalidAttributeException($"Unique constraint violation. Fields: {attrNames}. Article IDs: {string.Join(", ", conflictIds.ToArray())}");

        }

        private XDocument GetMassUpdateContentDataDocument(IEnumerable<Dictionary<string, string>> values, ContentAttribute[] attrs, ContentAttribute[] fullAttrs, int[] newIds, Content content)
        {
            var longUploadUrl = GetImagesUploadUrl(content.SiteId);
            var longSiteLiveUrl = GetSiteUrl(content.SiteId, true);
            var longSiteStageUrl = GetSiteUrl(content.SiteId, false);
            var dataDoc = new XDocument();
            dataDoc.Add(new XElement("ITEMS"));
            foreach (var value in values)
            {
                var isNewArticle = newIds.Contains(int.Parse(value[SystemColumnNames.Id]));
                var resultAttrs = isNewArticle ? fullAttrs : attrs; 
                foreach (var attr in resultAttrs)
                {
                    var elem = new XElement("ITEM");
                    elem.Add(new XAttribute("id", value[SystemColumnNames.Id]));
                    elem.Add(new XAttribute("attrId", attr.Id));
                    string result;
                    var valueExists = value.TryGetValue(attr.Name, out result);
                    if (attr.LinkId.HasValue)
                    {
                        result = attr.LinkId.Value.ToString();
                        valueExists = true;
                    }
                    else if (attr.BackRelation != null)
                    {
                        result = attr.BackRelation.Id.ToString();
                        valueExists = true;
                    }
                    else if (!string.IsNullOrEmpty(result))
                    {
                        if (attr.DbTypeName == "DATETIME" && Information.IsDate(result))
                        {
                            result = DateTime.Parse(result).ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else if (attr.DbTypeName == "NUMERIC")
                        {
                            result = result.Replace(",", ".");
                        }
                        else if (attr.Type == AttributeType.String || attr.Type == AttributeType.Textbox ||
                                 attr.Type == AttributeType.VisualEdit)
                        {
                            result = (result)
                                .Replace(longUploadUrl, UploadPlaceHolder)
                                .Replace(longSiteStageUrl, SitePlaceHolder)
                                .Replace(longSiteLiveUrl, SitePlaceHolder)
                                ;

                        }
                    }
                    else if (isNewArticle)
                    {
                        result = attr.DefaultValue;
                    }

                    if (isNewArticle || valueExists)
                    {
                        ValidateAttributeValue(attr, result, true);
                        elem.Add(new XElement(attr.DbField, XmlValidChars(result)));
                        dataDoc.Root?.Add(elem);
                    }

                }
            }
            return dataDoc;
        }

        private void MassUpdateContentItem(int contentId, IEnumerable<Dictionary<string, string>> values, int lastModifiedBy, XDocument doc, out int[] newIds)
        {
            var insertInto = @"
                DECLARE @Articles TABLE 
                (
                    CONTENT_ITEM_ID NUMERIC,
                    STATUS_TYPE_ID NUMERIC,
                    VISIBLE NUMERIC,
                    ARCHIVE NUMERIC                
                )

                DECLARE @NewArticles [Ids]
                DECLARE @OldIds [Ids]
                DECLARE @OldNonSplittedIds [Ids]
                DECLARE @NewSplittedIds [Ids]


                INSERT INTO @Articles
                    SELECT
                     doc.col.value('(CONTENT_ITEM_ID)[1]', 'numeric') CONTENT_ITEM_ID
                    ,doc.col.value('(STATUS_TYPE_ID)[1]', 'numeric') STATUS_TYPE_ID
                    ,doc.col.value('(VISIBLE)[1]', 'numeric') VISIBLE
                    ,doc.col.value('(ARCHIVE)[1]', 'numeric') ARCHIVE
                    FROM @xmlParameter.nodes('/ITEMS/ITEM') doc(col)

                INSERT into CONTENT_ITEM (CONTENT_ID, VISIBLE, ARCHIVE, STATUS_TYPE_ID, LAST_MODIFIED_BY, NOT_FOR_REPLICATION)
                OUTPUT inserted.[CONTENT_ITEM_ID] INTO @NewArticles
                SELECT @contentId, VISIBLE, ARCHIVE, STATUS_TYPE_ID, @lastModifiedBy, @notForReplication
                FROM @Articles a WHERE a.CONTENT_ITEM_ID = 0

                INSERT INTO @OldIds    
                SELECT a.CONTENT_ITEM_ID from @Articles a INNER JOIN content_item ci on a.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID

                exec qp_create_content_item_versions @OldIds, @lastModifiedBy    

                INSERT INTO @OldNonSplittedIds
                SELECT a.CONTENT_ITEM_ID from @OldIds i INNER JOIN content_item ci on i.id = ci.CONTENT_ITEM_ID where ci.SPLITTED = 0

                UPDATE CONTENT_ITEM SET 
                    VISIBLE = COALESCE(a.visible, ci.visible), 
                    ARCHIVE = COALESCE(a.archive, ci.archive), 
                    STATUS_TYPE_ID = COALESCE(a.STATUS_TYPE_ID, ci.STATUS_TYPE_ID),
                    LAST_MODIFIED_BY = @lastModifiedBy,
                    MODIFIED = GETDATE()
                FROM @Articles a INNER JOIN content_item ci on a.CONTENT_ITEM_ID = ci.CONTENT_ITEM_ID

                INSERT INTO @NewSplittedIds
                SELECT a.ID from @OldNonSplittedIds i INNER JOIN content_item ci on i.ID = ci.CONTENT_ITEM_ID where ci.SPLITTED = 1

                exec qp_split_articles @NewSplittedIds, @lastModifiedBy    
                   
                SELECT ID FROM @NewArticles
                ";
            var cmd = new SqlCommand(insertInto) { CommandType = CommandType.Text };
            cmd.Parameters.Add(new SqlParameter("@xmlParameter", SqlDbType.Xml) { Value = doc.ToString(SaveOptions.None) });
            cmd.Parameters.AddWithValue("@contentId", contentId);
            cmd.Parameters.AddWithValue("@lastModifiedBy", lastModifiedBy);
            cmd.Parameters.AddWithValue("@notForReplication", 1);


            var ids = new Queue<int>(GetRealData(cmd).AsEnumerable().Select(n => (int)n["ID"]).ToArray());

            newIds = ids.ToArray();

            foreach (var value in values)
            {
                if (value[SystemColumnNames.Id] == "0")
                    value[SystemColumnNames.Id] = ids.Dequeue().ToString();
            }
        }
    }

}
