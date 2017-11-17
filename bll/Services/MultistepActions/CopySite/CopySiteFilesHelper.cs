using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Repository.FieldRepositories;
using Quantumart.QP8.Constants.Mvc;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteFilesHelper
    {
        private const string ContentVersionFolder = "_qp7_article_files_versions";
        private readonly Site _source;
        private readonly Site _destination;

        public CopySiteFilesHelper(Site source, Site destination)
        {
            _source = source;
            _destination = destination;
        }

        public static int FilesCount(Site source) => GetAllFilesPaths(source).Count;

        private static CopySiteSettings Settings => (CopySiteSettings)HttpContext.Current.Session[HttpContextSession.CopySiteServiceSettings];

        public static long GetAllFileSize(Site source)
        {
            var files = GetAllFilesPaths(source);
            return files.Select(filePath => new FileInfo(filePath)).Select(file => file.Length).Sum();
        }

        public static int FilesSizeLimitPerTransaction => 5000000;

        public void CopyDirectories()
        {
            if (CheckIfFoldersAreEqual())
            {
                return;
            }

            var dirs = new List<string>();
            if (_source.LiveDirectory != _destination.LiveDirectory && Directory.Exists(_source.LiveDirectory))
            {
                dirs.Add(_destination.LiveDirectory);
            }

            if (_source.AssemblyPath != _destination.AssemblyPath && Directory.Exists(_source.AssemblyPath))
            {
                dirs.Add(_destination.AssemblyPath);
            }

            if (_source.UploadDir != _destination.UploadDir && Directory.Exists(_source.UploadDir))
            {
                dirs.AddRange(Directory.GetDirectories(_source.UploadDir, ".", SearchOption.AllDirectories).Where(s => !s.Contains(ContentVersionFolder)));
            }

            var contentsRelations = ContentRepository.GetRelationsBetweenContents(_source.Id, _destination.Id, string.Empty).ToList();
            var contentIds = Directory.GetDirectories($"{_source.UploadDir}\\contents", ".", SearchOption.TopDirectoryOnly);
            var contentIdsInt = contentIds.Select(s => s.Split('\\').Last());
            if (!contentIds.Any())
            {
                contentIds[0] = "0";
            }

            var attributesRelations = FieldRepository.GetRelationsBetweenAttributes(_source.Id, _destination.Id, string.Join(", ", contentIdsInt), false, false).ToList();
            foreach (var dirPath in dirs)
            {
                var path = ReplaceSourceContentDirsToNew(dirPath, contentsRelations, attributesRelations);
                Directory.CreateDirectory(path.Replace(_source.UploadDir, _destination.UploadDir));
            }
        }

        private bool CheckIfFoldersAreEqual() => _source.LiveDirectory == _destination.LiveDirectory || _source.AssemblyPath == _destination.AssemblyPath || _source.UploadDir == _destination.UploadDir;

        public int CopyFiles(int step, bool overwriteFilesOnDestination = true)
        {
            if (step == 0)
            {
                WriteAllFilesToBuffer();
            }

            var contentsRelations = ContentRepository.GetRelationsBetweenContents(_source.Id, _destination.Id, string.Empty).ToList();
            var contentIds = Directory.GetDirectories($"{_source.UploadDir}\\contents", ".", SearchOption.TopDirectoryOnly);
            if (contentIds.Length == 0)
            {
                contentIds = new[] { "0" };
            }

            var contentIdsInt = contentIds.Select(s => s.Split('\\').Last());
            var attributesRelations = FieldRepository.GetRelationsBetweenAttributes(_source.Id, _destination.Id, string.Join(", ", contentIdsInt), false, false).ToList();

            var filePaths = GetAllFiles();
            long tempFilesSizeLimit = 0;

            var filesCountPerTransaction = 0;
            while (tempFilesSizeLimit <= FilesSizeLimitPerTransaction && filePaths.Count > filesCountPerTransaction)
            {
                var path = filePaths[filesCountPerTransaction];
                var file = new FileInfo(path);
                var newPath = ChangeFilePaths(path, contentsRelations, attributesRelations);
                if (!File.Exists(newPath) && !CheckIfFoldersAreEqual())
                {
                    File.Copy(path, newPath);
                }

                filesCountPerTransaction++;
                tempFilesSizeLimit += file.Length;
            }

            if (filePaths.Count == 0)
            {
                File.Delete(Settings.PathForFileWithFilesToCopy);
            }

            if (filePaths.Count > filesCountPerTransaction)
            {
                filePaths.RemoveRange(0, filesCountPerTransaction);
            }

            WriteAllFilesToBuffer(filePaths);
            return filesCountPerTransaction;
        }

        private static string ReplaceSourceContentDirsToNew(string dirPath, IEnumerable<DataRow> contentsRelations, IEnumerable<DataRow> attributesRelations)
        {
            var matchContentValues = Regex.Matches(dirPath, @"\\contents\\([\d]*)", RegexOptions.IgnoreCase);
            if (matchContentValues.Count > 0)
            {
                var sourceContentId = matchContentValues[0].Groups[1].Value;
                var row = contentsRelations.FirstOrDefault(r => r["source_content_id"].ToString() == sourceContentId);
                if (row != null)
                {
                    var newContentId = row["destination_content_id"].ToString();
                    dirPath = dirPath.Replace("contents\\" + sourceContentId, "contents\\" + newContentId);
                }
            }

            var matchAttributeValues = Regex.Matches(dirPath, @"field_([\d]*)", RegexOptions.IgnoreCase);
            if (matchAttributeValues.Count > 0)
            {
                var sourceAttributeId = matchAttributeValues[0].Groups[1].Value;
                var row = attributesRelations.FirstOrDefault(r => r["source_attribute_id"].ToString() == sourceAttributeId);
                if (row != null)
                {
                    var newAttributeId = row["destination_attribute_id"].ToString();
                    dirPath = dirPath.Replace("field_" + sourceAttributeId, "field_" + newAttributeId);
                }
            }

            return dirPath;
        }

        private string ChangeFilePaths(string path, IEnumerable<DataRow> contentsRelations, IEnumerable<DataRow> attributesRelations)
        {
            var result = string.Empty;
            if (path.IndexOf(_source.UploadDir, StringComparison.Ordinal) > -1)
            {
                result = path.Replace(_source.UploadDir, _destination.UploadDir);
            }

            if (!string.IsNullOrEmpty(_source.AssemblyPath) && path.IndexOf(_source.AssemblyPath, StringComparison.Ordinal) > -1)
            {
                result = path.Replace(_source.AssemblyPath, _destination.AssemblyPath);
            }

            return ReplaceSourceContentDirsToNew(result, contentsRelations, attributesRelations);
        }

        private static List<string> GetAllFilesPaths(Site source)
        {
            var filePaths = new List<string>();
            if (!string.IsNullOrEmpty(source.AssemblyPath) && Directory.Exists(source.AssemblyPath))
            {
                filePaths.AddRange(Directory.EnumerateFiles(source.AssemblyPath, "*.*", SearchOption.TopDirectoryOnly).ToList());
            }

            if (Directory.Exists(source.UploadDir))
            {
                filePaths.AddRange(Directory.EnumerateFiles(source.UploadDir, "*.*", SearchOption.AllDirectories).Where(s => !s.Contains(ContentVersionFolder)).ToList());
            }

            return filePaths;
        }

        public void WriteAllFilesToBuffer(List<string> filePaths = null)
        {
            if (filePaths == null)
            {
                filePaths = GetAllFilesPaths(_source);
            }

            File.WriteAllText(Settings.PathForFileWithFilesToCopy, string.Empty);
            File.WriteAllLines(Settings.PathForFileWithFilesToCopy, filePaths.ToArray<string>());
        }

        private static List<string> GetAllFiles() => File.ReadAllLines(Settings.PathForFileWithFilesToCopy).ToList();
    }
}
