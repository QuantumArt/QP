using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Caching;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Services.MultistepActions.CopySite
{
    public class CopySiteFilesHelper
    {
        private const string contentVersionFolder = "_qp7_article_files_versions";
        private Site _source;
        private Site _destination;

        public CopySiteFilesHelper(Site source, Site destination)
        {
            this._source = source;
            this._destination = destination;
        }

        public static int FilesCount(Site source)
        {
            return GetAllFilesPaths(source).Count();
        }

        private CopySiteSettings Settings
        {
            get {
                return (CopySiteSettings)HttpContext.Current.Session["CopySiteService.Settings"];
            }
        }
        public static long GetAllFileSize(Site source)
        {
            List<string> files = GetAllFilesPaths(source);
            long allFileSize = 0;
            foreach (var filePath in files)
            {
                FileInfo file = new FileInfo(filePath);
                allFileSize += file.Length;
            }
            return allFileSize;
        }

        public static int FilesSizeLimitPerTransaction
        {
            get
            {
                return 5000000;
            }
        }

        public void CopyDirectories()
        {

            if (CheckIfFoldersAreEqual())
                return;
            List<string> dirs = new List<string>();

            if (_source.LiveDirectory != _destination.LiveDirectory && Directory.Exists(_source.LiveDirectory))
                dirs.Add(_destination.LiveDirectory);

            if (_source.AssemblyPath != _destination.AssemblyPath && Directory.Exists(_source.AssemblyPath))
                dirs.Add(_destination.AssemblyPath);

            if (_source.UploadDir != _destination.UploadDir && Directory.Exists(_source.UploadDir))
                dirs.AddRange(Directory.GetDirectories(_source.UploadDir, ".", SearchOption.AllDirectories).Where(s => !s.Contains(contentVersionFolder)));


            IEnumerable<DataRow> contentsRelations = ContentRepository.GetRelationsBetweenContents(this._source.Id, this._destination.Id, String.Empty);

            string[] contentIds = Directory.GetDirectories(String.Format("{0}\\{1}", _source.UploadDir, "contents"), ".", SearchOption.TopDirectoryOnly);
            IEnumerable<string> contentIdsInt = contentIds.Select(s => s.Split('\\').Last());

            if (contentIds.Count() == 0)
                contentIds[0] = "0";
            IEnumerable<DataRow> attributesRelations = FieldRepository.GetRelationsBetweenAttributes(_source.Id, _destination.Id, String.Join(", ", contentIdsInt), forVirtualContents: false, byNewContents: false);

            foreach (string dirPath in dirs)
            {
                    string path = ReplaceSourceContentDirsToNew(dirPath, contentsRelations, attributesRelations);
                    Directory.CreateDirectory(path.Replace(_source.UploadDir, _destination.UploadDir));
            }
        }

        private bool CheckIfFoldersAreEqual() {
            return (
                    _source.LiveDirectory == _destination.LiveDirectory || _source.AssemblyPath == _destination.AssemblyPath || _source.UploadDir == _destination.UploadDir
                );
        }

        public int CopyFiles(int step, bool overwriteFilesOnDestination = true)
        {
            if (step == 0)
            {
                WriteAllFilesToBuffer();
            }

            IEnumerable<DataRow> contentsRelations = ContentRepository.GetRelationsBetweenContents(this._source.Id, this._destination.Id, String.Empty);

            string[] contentIds = Directory.GetDirectories(String.Format("{0}\\{1}", _source.UploadDir, "contents"), ".", SearchOption.TopDirectoryOnly);

            if (contentIds.Length == 0)
                contentIds = new string[1]{"0"};

            IEnumerable<string> contentIdsInt = contentIds.Select(s => s.Split('\\').Last());
            IEnumerable<DataRow> attributesRelations = FieldRepository.GetRelationsBetweenAttributes(_source.Id, _destination.Id, String.Join(", ", contentIdsInt), forVirtualContents: false, byNewContents: false);

            List<string> filePaths = GetAllFiles();
            long tempFilesSizeLimit = 0;
            int filesCountPerTransaction = 0;

            while (tempFilesSizeLimit <= FilesSizeLimitPerTransaction && filePaths.Count > filesCountPerTransaction)
            {
                string path = filePaths[filesCountPerTransaction];
                FileInfo file = new FileInfo(path);

                string newPath = ChangeFilePaths(path, contentsRelations, attributesRelations);
                if (!File.Exists(newPath) && !CheckIfFoldersAreEqual()) { 
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
                filePaths.RemoveRange(0, filesCountPerTransaction);
            WriteAllFilesToBuffer(filePaths);

            return filesCountPerTransaction;
        }

        private string ReplaceSourceContentDirsToNew(string dirPath, IEnumerable<DataRow> contentsRelations, IEnumerable<DataRow> attributesRelations)
        {
            MatchCollection matchContentValues = Regex.Matches(dirPath, @"\\contents\\([\d]*)", RegexOptions.IgnoreCase);

            if (matchContentValues.Count > 0)
            {
                string sourceContentId = matchContentValues[0].Groups[1].Value;
                DataRow row = contentsRelations.Where(r => r["source_content_id"].ToString() == sourceContentId).FirstOrDefault();
                if (row != null)
                {
                    string newContentId = row["destination_content_id"].ToString();
                    dirPath = dirPath.Replace("contents\\" + sourceContentId, "contents\\" + newContentId);
                }
            }

            MatchCollection matchAttributeValues = Regex.Matches(dirPath, @"field_([\d]*)", RegexOptions.IgnoreCase);

            if (matchAttributeValues.Count > 0)
            {
                string sourceAttributeId = matchAttributeValues[0].Groups[1].Value;
                DataRow row = attributesRelations.Where(r => r["source_attribute_id"].ToString() == sourceAttributeId).FirstOrDefault();
                if (row != null)
                {
                    string newAttributeId = row["destination_attribute_id"].ToString();
                    dirPath = dirPath.Replace("field_" + sourceAttributeId, "field_" + newAttributeId);
                }
            }

            return dirPath;
        }
        
        private string ChangeFilePaths(string path, IEnumerable<DataRow> contentsRelations, IEnumerable<DataRow> attributesRelations)
        {
            string result = String.Empty;
            if (path.IndexOf(_source.UploadDir) > -1)
                result = path.Replace(_source.UploadDir, _destination.UploadDir);
            if (!String.IsNullOrEmpty(_source.AssemblyPath) && path.IndexOf(_source.AssemblyPath) > -1)
                result = path.Replace(_source.AssemblyPath, _destination.AssemblyPath);

            return ReplaceSourceContentDirsToNew(result, contentsRelations, attributesRelations);
        }

        private static List<string> GetAllFilesPaths(Site source)
        {
            List<string> filePaths = new List<string>();
 
            if (!String.IsNullOrEmpty(source.AssemblyPath) && Directory.Exists(source.AssemblyPath)) {           
                filePaths.AddRange(Directory.EnumerateFiles(source.AssemblyPath, "*.*", SearchOption.TopDirectoryOnly).ToList());
            }
            if (Directory.Exists(source.UploadDir)) {
                filePaths.AddRange(Directory.EnumerateFiles(source.UploadDir, "*.*", SearchOption.AllDirectories).Where(s => !s.Contains(contentVersionFolder)).ToList());            
            }
            return filePaths;
        }

        public void WriteAllFilesToBuffer(List<string> filePaths = null)
        {
            if (filePaths == null) {
                filePaths = GetAllFilesPaths(this._source);
            }
            File.WriteAllText(Settings.PathForFileWithFilesToCopy, String.Empty);
            File.WriteAllLines(Settings.PathForFileWithFilesToCopy, filePaths.ToArray<string>());
        }

        private List<string> GetAllFiles() {
            return File.ReadAllLines(Settings.PathForFileWithFilesToCopy).ToList<string>();
        }
    }
}
