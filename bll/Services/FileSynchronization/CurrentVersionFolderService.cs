using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.API;

namespace Quantumart.QP8.BLL.Services.FileSynchronization;

public class CurrentVersionFolderService : ICurrentVersionFolderService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private CommonSchedulerProperties _options;

    public CurrentVersionFolderService(IOptions<CommonSchedulerProperties> options)
    {
        _options = options.Value;
    }

    public void SyncFolders(string customerName, int numFiles)
    {
        QPContext.CurrentUserId = _options.DefaultUserId;
        _logger.Info($"Start processing files on customer code {customerName}");
        var contentIds = SiteRepository.GetAll().SelectMany(n => ContentRepository.GetContentIdsBySiteId(n.Id)).ToArray();
        foreach (var contentId in contentIds)
        {
            var dirInfo = GetRootDirectoryInfo(contentId);
            var currentVersionDir = GetCurrentVersionDirectoryInfo(dirInfo);
            if (currentVersionDir == null)
            {
                continue;
            }

            var foundFiles = GetFilesToDelete(currentVersionDir, GetContentFiles(dirInfo));
            _logger.Info($"Found {foundFiles.Length} files for content {contentId}, but the number is limited to {numFiles}");

            foreach (var info in foundFiles.Take(numFiles))
            {
                _logger.Info($"Deleting file {info.FullName}");
                info.Delete();
            }
        }
    }

    private string[] GetContentFiles(DirectoryInfo rootDirInfo)
    {
        var dirs = rootDirInfo.GetDirectories("*", SearchOption.AllDirectories)
            .Where(n => n.Name != ArticleVersion.RootFolder && n.Name != "_temp").ToArray();
        var result = new List<string>();
        foreach (var dir in dirs)
        {
            result.AddRange(dir.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Select(n => n.Name));
        }
        result.AddRange(rootDirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Select(n => n.Name));
        return result.ToArray();
    }

    private DirectoryInfo GetRootDirectoryInfo(int contentId)
    {
        var factory = new ContentFolderFactory();
        var repository = factory.CreateRepository();
        var pathInfo = repository.GetRoot(contentId).PathInfo;
        return new DirectoryInfo(pathInfo.Path);
    }

    private DirectoryInfo GetCurrentVersionDirectoryInfo(DirectoryInfo rootDirInfo)
    {
        var versionsDir = rootDirInfo.GetDirectories(ArticleVersion.RootFolder).FirstOrDefault();
        return versionsDir?.GetDirectories("current").FirstOrDefault();
    }

    private FileInfo[] GetFilesToDelete(DirectoryInfo currentVersionDir, string[] contentFiles)
    {
        var hash = new HashSet<string>(contentFiles);
        return currentVersionDir.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
            .Where(n => !hash.Contains(n.Name)).ToArray();
    }

}
