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

        private static PathSecurityInfo FindFirst(string path, List<PathSecurityInfo> input) => FindMatched(path, input).FirstOrDefault();

        private static PathSecurityInfo FindLongest(string path, List<PathSecurityInfo> input)
        {
            return FindMatched(path, input).OrderByDescending(n => n.Path.Length).FirstOrDefault();
        }

        private static List<PathSecurityInfo> FindMatched(string path, List<PathSecurityInfo> input)
        {
            var result = new List<PathSecurityInfo>();
            var sep = System.IO.Path.DirectorySeparatorChar;
            foreach (var item in input)
            {
                if (path.StartsWith(item.Path.TrimEnd(sep) + sep, CompareOption))
                {
                    result.Add(item);
                }
            }

            return result;
        }

        private static PathSecurityResult CheckContentFolder(string pathToFind, int contentId, string requestedTypeCode)
        {
            var result = new PathSecurityResult();
            var factory = new ContentFolderFactory();
            var contentFolder = FindLongest(pathToFind, factory.CreateRepository().GetPaths(contentId));
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

        private static PathSecurityResult CheckSiteFolder(string pathToFind, int siteId, string requestedTypeCode)
        {
            var result = new PathSecurityResult();
            var factory = new SiteFolderFactory();
            var siteFolder = FindLongest(pathToFind, factory.CreateRepository().GetPaths(siteId));
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

        public static PathSecurityResult Check(string path, bool forModify)
        {
            var sep = System.IO.Path.DirectorySeparatorChar;
            var typeCode = forModify ? ActionTypeCode.Update : ActionTypeCode.Read;
            var result = new PathSecurityResult();
            var pathToFind = path;
            if (pathToFind[pathToFind.Length - 1].ToString() != sep.ToString())
            {
                pathToFind = pathToFind + sep;
            }

            var site = FindFirst(pathToFind, SiteRepository.GetPaths());
            if (site == null)
            {
                result.Result = pathToFind.StartsWith(QPConfiguration.TempDirectory);
                return result;
            }

            var images = $@"{sep}images";
            pathToFind = pathToFind.Replace(site.Path, string.Empty);
            if (pathToFind.StartsWith(images, CompareOption))
            {
                result.IsSite = true;
                pathToFind = pathToFind.Replace(images, string.Empty);

                var checksiteFolderResult = CheckSiteFolder(pathToFind, site.Id, typeCode);
                result.Result = checksiteFolderResult.Result;
                result.FolderId = checksiteFolderResult.FolderId;
                return result;
            }

            var contents = new Regex($@"^\{sep}contents\{sep}([\d]+)");
            var match = contents.Match(pathToFind);
            if (match.Success)
            {
                var contentId = int.Parse(match.Groups[1].Value);
                pathToFind = pathToFind.Replace(match.Value, string.Empty);
                return CheckContentFolder(pathToFind, contentId, typeCode);
            }

            return result;
        }
    }
}
