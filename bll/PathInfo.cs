using System;
using System.IO;
using System.Text.RegularExpressions;
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

        public string Url { get; set; }

        public PathInfo GetSubPathInfo(string folderName)
        {
            if (folderName.IndexOf(@"\") == 0)
            {
                folderName = folderName.Substring(1);
            }

            if (!folderName.Equals(string.Empty) && folderName.LastIndexOf(@"\") == folderName.Length - 1)
            {
                folderName = folderName.Substring(0, folderName.Length - 1);
            }

            if (folderName.Equals(string.Empty))
            {
                return new PathInfo { Path = Path, Url = Url };
            }

            return new PathInfo { Path = $@"{Path}\{folderName}", Url = $@"{Url}{folderName.Replace(@"\", @"/")}/"};
        }

        public string GetPath(string fileName) => System.IO.Path.Combine(Path, ReplaceUp(fileName));

        public string GetUrl(string fileName) => $"{Url}{ReplaceUp(fileName)}";

        private static string ReplaceUp(string input) => input.Replace("..", "");

        internal FolderFile GetFile(string fileName)
        {
            var path = GetPath(fileName);
            if (!File.Exists(path))
            {
                throw new Exception(string.Format(LibraryStrings.FileNotExists, path));
            }

            return new FolderFile(new FileInfo(path));
        }

        public static PathSecurityResult CheckSecurity(string path) => PathSecurity.Check(path);

        public static string ConvertToUrl(string path)
        {
            var url = string.Empty;
            foreach (var pathInfo in SiteRepository.GetPathInfoList())
            {
                if (path.StartsWith(pathInfo.Path, StringComparison.InvariantCultureIgnoreCase))
                {
                    url = Regex.Replace(path, Regex.Escape(pathInfo.Path + @"\"), pathInfo.Url);
                    url = url.Replace(@"\", @"/");
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
                    path = Regex.Replace(url, Regex.Escape(pathInfo.Url), pathInfo.Path + @"\");
                    path = path.Replace(@"/", @"\");
                    break;
                }
            }

            return path;
        }
    }
}
