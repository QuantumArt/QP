using System;
using I = System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Minio.DataModel;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Resources;

namespace Quantumart.QP8.BLL
{
    /// <summary>
    /// Информация о физических и виртуальных путях заданной библиотеки
    /// </summary>
    public class PathInfo
    {
        public string Path { get; set; }

        public string FixedPath => FixPath(Path);

        public string Url { get; set; }

        public string BaseUploadUrl { get; set; }

        public PathHelper PathHelper { get; set; }

        public PathInfo GetSubPathInfo(string folderName)
        {
            if (folderName.IndexOf(@"\", StringComparison.Ordinal) == 0)
            {
                folderName = folderName.Substring(1);
            }

            if (!folderName.Equals(string.Empty) && folderName.LastIndexOf(@"\", StringComparison.Ordinal) == folderName.Length - 1)
            {
                folderName = folderName.Substring(0, folderName.Length - 1);
            }

            if (folderName.Equals(string.Empty))
            {
                return new PathInfo { Path = Path, Url = Url, BaseUploadUrl = BaseUploadUrl};
            }

            var replacedName = folderName.Replace(@"\", @"/");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                folderName = replacedName;
            }
            return new PathInfo
            {
                Path = FixPath($@"{Path}{I.Path.DirectorySeparatorChar}{folderName}"),
                Url = $@"{Url}{replacedName}/",
                BaseUploadUrl = BaseUploadUrl
            };
        }

        private string FixPath(string path) => PathHelper != null ? PathHelper.FixPathSeparator(path) : path;

        private string CombinePath(string path, string name) => PathHelper != null ? PathHelper.CombinePath(path, name) : I.Path.Combine(path, name);

        public string GetPath(string fileName) => CombinePath(Path, ReplaceUp(fileName));

        public string GetUrl(string fileName) => $"{Url}{ReplaceUp(fileName)}";

        private static string ReplaceUp(string input) => input.Replace("..", "");

        internal FolderFile GetFile(string fileName)
        {
            var path = GetPath(fileName);
            if (!PathHelper.FileExists(path))
            {
                throw new Exception(string.Format(LibraryStrings.FileNotExists, path));
            }

            if (PathHelper.UseS3)
            {
                return PathHelper.GetS3File(path);
            }
            return new FolderFile(new I.FileInfo(path));
        }

        public static PathSecurityResult CheckSecurity(string path, bool forModify, char separator)
            => PathSecurity.Check(path, forModify, separator);

        public static string ConvertToUrl(string path)
        {
            var url = string.Empty;
            foreach (var pathInfo in SiteRepository.GetPathInfoList())
            {
                if (path.StartsWith(pathInfo.Path, StringComparison.InvariantCultureIgnoreCase))
                {
                    var pathWithSep = pathInfo.Path + System.IO.Path.DirectorySeparatorChar;
                    url = Regex.Replace(path, Regex.Escape(pathWithSep), pathInfo.Url);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        url = url.Replace(@"\", @"/");
                    }
                    break;
                }
            }

            return url;
        }

        public static string ConvertToPath(string url)
        {
            var path = string.Empty;
            foreach (var pathInfo in SiteRepository.GetPathInfoList())
            {
                if (url.StartsWith(pathInfo.Url, StringComparison.InvariantCultureIgnoreCase))
                {
                    var pathWithSep = pathInfo.Path + System.IO.Path.DirectorySeparatorChar;
                    path = Regex.Replace(url, Regex.Escape(pathInfo.Url), pathWithSep);
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        path = path.Replace(@"/", @"\");
                    }
                    break;
                }
            }

            return path;
        }
    }
}
