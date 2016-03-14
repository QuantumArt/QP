using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Quantumart.QP8.Resources;
using Quantumart.QP8.BLL.Repository;
using System.Text.RegularExpressions;

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
			// remove leading and traling slashes if exist
			if (folderName.IndexOf(@"\") == 0)
				folderName = folderName.Substring(1);
			if (!folderName.Equals(String.Empty) && folderName.LastIndexOf(@"\") == folderName.Length - 1)
				folderName = folderName.Substring(0, folderName.Length - 1);

			if (folderName.Equals(String.Empty))
				return new PathInfo { Path = Path, Url = Url };
			else
				return new PathInfo { Path = String.Format(@"{0}\{1}", Path, folderName), Url = String.Format(@"{0}{1}/", Url, folderName.Replace(@"\", @"/")) };
        }

        public string GetPath(string fileName)
        {
            return System.IO.Path.Combine(Path, ReplaceUp(fileName));
        }

        public string GetUrl(string fileName)
        {
            return String.Format("{0}{1}", Url, ReplaceUp(fileName));
        }

		private string ReplaceUp(string input)
		{
			return input.Replace("..", "");
		}

		internal FolderFile GetFile(string fileName)
		{
			string path = GetPath(fileName);
			if (!File.Exists(path))
				throw new Exception(String.Format(LibraryStrings.FileNotExists, path));
			else
				return new FolderFile(new FileInfo(path));
		}

        public static PathSecurityResult CheckSecurity(string path)
		{
			return PathSecurity.Check(path);
		}

		public static string ConvertToUrl(string path)
		{
			string url = String.Empty;
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
			string path = String.Empty;
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
