using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Internal;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;

namespace Quantumart.QP8.BLL
{
    public class PathSecurityInfo
    {
        public int Id { get; set; }

        public string Path { get; set; }
    }

    public class PathSecurityResult
    {
        public bool Result { get; set; }

        public bool IsSite { get; set; }

        public int FolderId { get; set; }
    }

    public class PathSecurity
    {
        private static readonly StringComparison CompareOption = StringComparison.InvariantCultureIgnoreCase;

        private static PathSecurityInfo FindFirst(string path, List<PathSecurityInfo> input, char separator)
            => FindMatched(path, input, separator).FirstOrDefault();

        private static PathSecurityInfo FindLongest(string path, List<PathSecurityInfo> input, char separator)
        {
            return FindMatched(path, input, separator).OrderByDescending(n => n.Path.Length).FirstOrDefault();
        }

        private static List<PathSecurityInfo> FindMatched(string path, List<PathSecurityInfo> input, char separator)
        {
            var result = new List<PathSecurityInfo>();
            foreach (var item in input)
            {
                if (path.StartsWith(item.Path.Replace('\\', separator).TrimEnd(separator) + separator, CompareOption))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private static PathSecurityResult CheckContentFolder(string pathToFind, int contentId, string requestedTypeCode, char separator)
        {
            var result = new PathSecurityResult();
            var factory = new ContentFolderFactory();
            var contentFolder = FindLongest(pathToFind, factory.CreateRepository().GetPaths(contentId), separator);
            if (contentFolder != null)
            {
                result.Result = SecurityRepository.IsEntityAccessible(EntityTypeCode.ContentFolder, contentFolder.Id, requestedTypeCode);
                result.FolderId = contentFolder.Id;
            }
            else
            {
                result.Result = SecurityRepository.IsEntityAccessible(EntityTypeCode.Content, contentId, requestedTypeCode);
            }

            return result;
        }

        private static PathSecurityResult CheckSiteFolder(string pathToFind, int siteId, string requestedTypeCode, char separator)
        {
            var result = new PathSecurityResult();
            var factory = new SiteFolderFactory();
            var siteFolder = FindLongest(pathToFind, factory.CreateRepository().GetPaths(siteId), separator);
            if (siteFolder != null)
            {
                result.Result = SecurityRepository.IsEntityAccessible(EntityTypeCode.SiteFolder, siteFolder.Id, requestedTypeCode);
                result.FolderId = siteFolder.Id;
            }
            else
            {
                result.Result = SecurityRepository.IsEntityAccessible(EntityTypeCode.Site, siteId, requestedTypeCode);
            }

            return result;
        }

        public static PathSecurityResult Check(string path, bool forModify, char separator)
        {
            var typeCode = forModify ? ActionTypeCode.Update : ActionTypeCode.Read;
            var result = new PathSecurityResult();
            var pathToFind = path;
            if (pathToFind[^1].ToString() != separator.ToString())
            {
                pathToFind += separator;
            }

            var site = FindFirst(pathToFind, SiteRepository.GetPaths(), separator);
            if (site == null)
            {
                result.Result = pathToFind.StartsWith(QPConfiguration.TempDirectory);
                return result;
            }

            var images = $@"{separator}images";
            pathToFind = pathToFind.Replace(site.Path, string.Empty);
            if (pathToFind.StartsWith(images, CompareOption))
            {
                result.IsSite = true;
                pathToFind = pathToFind.Replace(images, string.Empty);

                var checksiteFolderResult = CheckSiteFolder(pathToFind, site.Id, typeCode, separator);
                result.Result = checksiteFolderResult.Result;
                result.FolderId = checksiteFolderResult.FolderId;
                return result;
            }

            var contents = new Regex($@"^\{separator}contents\{separator}([\d]+)");
            var match = contents.Match(pathToFind);
            if (match.Success)
            {
                var contentId = int.Parse(match.Groups[1].Value);
                pathToFind = pathToFind.Replace(match.Value, string.Empty);
                return CheckContentFolder(pathToFind, contentId, typeCode, separator);
            }

            return result;
        }
    }
}
