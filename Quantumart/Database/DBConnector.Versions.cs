using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using Quantumart.QPublishing.Helpers;
using Quantumart.QPublishing.Info;

namespace Quantumart.QPublishing.Database
{

    // ReSharper disable once InconsistentNaming
    public partial class DBConnector
    {
        #region Versions

        public DataTable GetArticleVersions(int id)
        {
            return GetRealData("exec qp_get_versions " + id);
        }

        private string GetVersionFolderFormat()
        {
            return @"{0}\_qp7_article_files_versions\{1}";
        }

        public string GetContentLibraryFolder(int articleId)
        {
            var contentId = GetContentIdForItem(articleId);
            var siteId = GetSiteIdByContentId(contentId);
            return GetContentLibraryDirectory(siteId, contentId);
        }

        private string GetVersionFolder(int articleId, int versionId)
        {
            if (versionId == 0)
                return String.Empty;
            else
                return String.Format(GetVersionFolderFormat(), GetContentLibraryFolder(articleId), versionId);
        }

        private string GetVersionFolderForContent(int contentId, int versionId)
        {
            if (versionId == 0)
                return String.Empty;
            else
                return String.Format(GetVersionFolderFormat(), GetContentLibraryDirectory(contentId), versionId);
        }

        private string GetCurrentVersionFolder(int articleId)
        {
            return String.Format(GetVersionFolderFormat(), GetContentLibraryFolder(articleId), "current");
        }

        private string GetCurrentVersionFolderForContent(int contentId)
        {
            return String.Format(GetVersionFolderFormat(), GetContentLibraryDirectory(contentId), "current");
        }

        private IEnumerable<ContentAttribute> GetFilesAttributesForVersionControl(int contentId)
        {
            return GetContentAttributeObjects(contentId).Where(n => (n.Type == AttributeType.Image || n.Type == AttributeType.File) && !n.DisableVersionControl);
        }


        private void CreateFilesVersions(int articleId)
        {
            var contentId = GetContentIdForItem(articleId);
            var newVersionId = GetLatestVersionId(articleId);
            var currentVersionFolder = GetCurrentVersionFolder(articleId);
            if (newVersionId != 0)
            {
                var files = GetFilesAttributesForVersionControl(contentId)
                    .Select(n => GetVersionDbDataValue(newVersionId, n))
                    .Where(n => !String.IsNullOrEmpty(n))
                    .Select(n => new FileToCopy
                    {
                        Name = Path.GetFileName(n),
                        Folder = currentVersionFolder,
                        ToFolder = GetVersionFolder(articleId, newVersionId)
                    });

                CopyArticleFiles(files);
            }

            var newFiles = GetFilesAttributesForVersionControl(contentId)
                .Select(n => new FileToCopy
                {
                    Name = GetDbDataValue(articleId, n),
                    Folder = GetDirectoryForFileAttribute(n.Id),
                    ToFolder = currentVersionFolder
                })
                .Where(n => !String.IsNullOrEmpty(n.Name));

            CopyArticleFiles(newFiles);
        }

        private string GetDbDataValue(int articleId, ContentAttribute field)
        {
            var cmd = new SqlCommand {CommandType = CommandType.Text};
            var dbFieldName = field.Type == AttributeType.Textbox || field.Type == AttributeType.VisualEdit ? "BLOB_DATA" : "DATA";
            cmd.CommandText =
                $"select {dbFieldName} from content_data where content_item_id = @itemId and attribute_id = @fieldId";
            cmd.Parameters.AddWithValue("@itemId", articleId);
            cmd.Parameters.AddWithValue("@fieldId", field.Id);
            var result = GetRealScalarData(cmd);
            return result?.ToString() ?? String.Empty;

        }

        private string GetVersionDbDataValue(int versionId, ContentAttribute field)
        {
            var cmd = new SqlCommand {CommandType = CommandType.Text};
            var dbFieldName = field.Type == AttributeType.Textbox || field.Type == AttributeType.VisualEdit ? "BLOB_DATA" : "DATA";
            cmd.CommandText =
                $"select {dbFieldName} from version_content_data where content_item_version_id = @versionId and attribute_id = @fieldId";
            cmd.Parameters.AddWithValue("@versionId", versionId);
            cmd.Parameters.AddWithValue("@fieldId", field.Id);
            var result = GetRealScalarData(cmd);
            return result?.ToString() ?? String.Empty;

        }

        private DataTable GetVersionDataValues(int[] versionIds, int[] attrIds)
        {
            var cmd = new SqlCommand
            {
                CommandType = CommandType.Text,
                CommandText =
                    $"select attribute_id, version_id, data from version_content_data where content_item_version_id in (select id from @versionIds) and attribute_id in (select id from @attrIds)"
            };
            cmd.Parameters.AddWithValue("@versionIds", IdsToDataTable(versionIds));
            cmd.Parameters.AddWithValue("@attrIds", IdsToDataTable(attrIds));
            var result = GetRealData(cmd);
            return result;

        }

        private class FileToCopy
        {
            public string Name { get; set; }
            public string Folder { get; set; }

            public string ToFolder { get; set; }
        }

        private void CopyArticleFiles(IEnumerable<FileToCopy> files)
        {
            var fileToCopies = files as FileToCopy[] ?? files.ToArray();

            foreach (var field in fileToCopies)
            {
                if (!Directory.Exists(field.ToFolder))
                    Directory.CreateDirectory(field.ToFolder);

                if (!Directory.Exists(field.Folder))
                    Directory.CreateDirectory(field.Folder);

                var sourceName = $@"{field.Folder}\{field.Name.Replace("/", "\\")}";
                var destName = $@"{field.ToFolder}\{Path.GetFileName(field.Name)}";

                if (File.Exists(sourceName))
                {
                    if (File.Exists(destName))
                    {
                        File.Delete(destName);
                    }

                    File.Copy(sourceName, destName);
                }
            }
        }

        private int GetEarliestVersionId(int articleId)
        {
            return GetAggregateVersionFunction("MIN", articleId);
        }

        private int GetLatestVersionId(int articleId)
        {
            return GetAggregateVersionFunction("MAX", articleId);
        }

        private int[] GetLatestVersionIds(int[] ids)
        {
            return GetAggregateVersionFunction("MAX", ids);
        }

        private int GetVersionsCount(int articleId)
        {
            return GetAggregateVersionFunction("COUNT", articleId);
        }

        private int GetAggregateVersionFunction(string function, int articleId)
        {
            if (articleId == 0)
                return 0;
            using (var cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText =
                    $"select cast({function}(content_item_version_id) as int) from content_item_version where content_item_id = @id";
                cmd.Parameters.AddWithValue("@id", articleId);
                return CastDbNull.To<int>(GetRealScalarData(cmd));
            }
        }

        private int[] GetAggregateVersionFunction(string function, int[] ids)
        {
            if (!ids.Any())
                return new int[0];
            using (var cmd = new SqlCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText =
                    $"select cast({function}(content_item_version_id) as int) as data from content_item_version group by content_item_id where content_item_id in (select id from @ids)";
                cmd.Parameters.AddWithValue("@ids", IdsToDataTable(ids));

                return GetRealData(cmd).AsEnumerable().Select(n => n.Field<int>("data")).ToArray();
            }
        }

        #endregion
    }
}
