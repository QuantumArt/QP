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

public class CleanSystemFoldersService : ICleanSystemFoldersService
{
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private CommonSchedulerProperties _options;

    public CleanSystemFoldersService(IOptions<CommonSchedulerProperties> options)
    {
        _options = options.Value;
    }

    public void CleanSystemFolders(string customerName, int numFiles)
    {
        QPContext.CurrentUserId = _options.DefaultUserId;
        _logger.Info($"Start processing files on customer code {customerName}");
        var contentIds = SiteRepository.GetAll().SelectMany(n => ContentRepository.GetContentIds(n.Id)).ToArray();
        foreach (var contentId in contentIds)
        {
            var dirInfo = GetRootDirectoryInfo(contentId);
            if (dirInfo is not { Exists: true })
            {
                continue;
            }

            var currentVersionDirInfo = GetCurrentVersionDirectoryInfo(dirInfo);
            if (currentVersionDirInfo is { Exists: true } && numFiles > 0)
            {
                numFiles = ProcessCurrentVersionFiles(numFiles, dirInfo, currentVersionDirInfo, contentId);
            }

            var tempDirInfo = dirInfo.GetDirectories("_temp").FirstOrDefault();
            if (tempDirInfo is { Exists: true } && numFiles > 0)
            {
                numFiles = ProcessTempFiles(numFiles, tempDirInfo, contentId);
            }
        }
    }

    private int ProcessCurrentVersionFiles(int numFiles, DirectoryInfo dirInfo, DirectoryInfo currentVersionDirInfo, int contentId)
    {
        var contentFiles = GetContentFiles(dirInfo);
        var filesToDelete = GetFilesToDelete(currentVersionDirInfo, contentFiles);
        _logger.Info($"Found {filesToDelete.Length} files in current version folder for content {contentId}, but the number is limited to {numFiles}");
        return ProcessFiles(numFiles, filesToDelete);
    }

    private int ProcessTempFiles(int numFiles, DirectoryInfo tempDirInfo, int contentId)
    {
        var filesToDelete = tempDirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).ToArray();
        _logger.Info($"Found {filesToDelete.Length} files in _temp folder for content {contentId}, but the number is limited to {numFiles}");
        return ProcessFiles(numFiles, filesToDelete);
    }

    private static int ProcessFiles(int numFiles, FileInfo[] filesToDelete)
    {
        var processed = 0;
        foreach (var info in filesToDelete.Take(numFiles))
        {
            _logger.Info($"Deleting file {info.FullName}");
            info.Delete();
            processed++;
        }
        return numFiles - processed;
    }

    private string[] GetContentFiles(DirectoryInfo rootDirInfo)
    {
        var dirs = rootDirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly)
            .Where(n => n.Name != ArticleVersion.RootFolder && n.Name != "_temp").ToArray();
        var result = new List<string>();
        foreach (var dir in dirs)
        {
            result.AddRange(dir.EnumerateFiles("*", SearchOption.AllDirectories).Select(n => n.Name));
        }
        result.AddRange(rootDirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Select(n => n.Name));
        return result.ToArray();
    }

    private DirectoryInfo GetRootDirectoryInfo(int contentId)
    {
        var factory = new ContentFolderFactory();
        var repository = factory.CreateRepository();
        var root = repository.GetRoot(contentId);
        if (root == null)
        {
            return null;
        }
        var pathInfo = root.PathInfo;
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
