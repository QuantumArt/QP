using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using NLog;
using Quantumart.QP8.BLL.Factories.FolderFactory;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Repository.ContentRepositories;
using Quantumart.QP8.BLL.Services.API;

namespace Quantumart.QP8.BLL.Services.FileSynchronization;

public class CleanSystemFoldersService : ICleanSystemFoldersService
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
    private readonly CommonSchedulerProperties _options;
    private PathHelper _pathHelper;

    public CleanSystemFoldersService(IOptions<CommonSchedulerProperties> options, PathHelper pathHelper)
    {
        _options = options.Value;
        _pathHelper = pathHelper;
    }

    public int CleanSystemFolders(string customerName, int numFiles)
    {
        QPContext.CurrentUserId = _options.DefaultUserId;
        Logger.Info($"Start processing customer code {customerName}");
        var contentIds = SiteRepository.GetAll().SelectMany(n => ContentRepository.GetContentIds(n.Id)).ToArray();
        foreach (var contentId in contentIds)
        {
            if (numFiles == 0)
            {
                return 0;
            }

            var dirInfo = GetContentRootDirectoryInfo(contentId);
            numFiles = SyncCurrentVersionFolder(numFiles, dirInfo, contentId);
            numFiles = ClearContentTempFolder(numFiles, dirInfo, contentId);
            numFiles = RemoveEmptyVersionFolders(numFiles, dirInfo, contentId);
        }

        var siteIds = SiteRepository.GetAll().Select(n => n.Id).ToArray();
        foreach (var siteId in siteIds)
        {
            var dirInfo = GetSiteRootDirectoryInfo(siteId);
            numFiles = ClearSiteTempFolder(numFiles, dirInfo, siteId);
        }
        return numFiles;
    }

    private int ClearSiteTempFolder(int numFiles, DirectoryInfo dirInfo, int siteId)
    {
        if (!_pathHelper.UseS3 && dirInfo is { Exists: true } && numFiles > 0)
        {
            var tempDirInfo = dirInfo.Parent?.GetDirectories("temp").FirstOrDefault();
            if (tempDirInfo is { Exists: true })
            {
                var filesToDelete = tempDirInfo.EnumerateFiles("*", SearchOption.AllDirectories).ToArray();
                if (filesToDelete.Any())
                {
                    Logger.Info($"Found {filesToDelete.Length} files in temp folder for site {siteId}," +
                        $" but the number is limited to {numFiles}");
                    numFiles = ProcessFiles(numFiles, filesToDelete);
                }
            }
        }
        return numFiles;
    }

    private int ClearContentTempFolder(int numFiles, DirectoryInfo dirInfo, int contentId)
    {
        if (!_pathHelper.UseS3 && dirInfo is { Exists: true } && numFiles > 0)
        {
            var tempDirInfo = dirInfo.GetDirectories("_temp").FirstOrDefault();
            if (tempDirInfo is { Exists: true })
            {
                var filesToDelete = tempDirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).ToArray();
                if (filesToDelete.Any())
                {
                    Logger.Info($"Found {filesToDelete.Length} files in _temp folder for content {contentId}," +
                        $" but the number is limited to {numFiles}");
                    numFiles = ProcessFiles(numFiles, filesToDelete);
                }
            }
        }
        return numFiles;
    }

    private int RemoveEmptyVersionFolders(int numFiles, DirectoryInfo dirInfo, int contentId)
    {
        if (!_pathHelper.UseS3 && dirInfo is { Exists: true } && numFiles > 0)
        {
            var allVersionsDir = dirInfo.GetDirectories(ArticleVersion.RootFolder).FirstOrDefault();
            if (allVersionsDir is { Exists: true })
            {
                var versionDirs = allVersionsDir.GetDirectories("*");
                foreach (var versionDir in versionDirs)
                {
                    if (versionDir.Name != "current" && !versionDir.EnumerateFileSystemInfos().Any() && numFiles > 0)
                    {
                       Logger.Info($"Removing empty folder {versionDir.Name} for content {contentId}");
                       Directory.Delete(versionDir.FullName);
                       numFiles--;
                    }
                }

            }
        }
        return numFiles;
    }

    private int SyncCurrentVersionFolder(int numFiles, DirectoryInfo dirInfo, int contentId)
    {
        if (numFiles > 0)
        {
            if (_pathHelper.UseS3)
            {
                var contentFiles = new HashSet<string>(GetContentFiles(dirInfo));
                var path = dirInfo.ToString();
                var prefix = _pathHelper.CombinePath(path, $"{ArticleVersion.RootFolder}/current");
                var filesToDelete = _pathHelper.ListS3Files(prefix)
                    .Where(n => !contentFiles.Contains(n.Name)).Take(numFiles).ToArray();

                if (filesToDelete.Any())
                {
                    Logger.Info($"Found {filesToDelete.Length} files in current version folder " +
                        $"for content {contentId}, but the number is limited to {numFiles}");
                    _pathHelper.RemoveS3Files(filesToDelete);
                    numFiles -= filesToDelete.Length;
                }
            }
            else
            {
                if (dirInfo is { Exists: true })
                {
                    var versionsDir = dirInfo.GetDirectories(ArticleVersion.RootFolder).FirstOrDefault();
                    if (versionsDir is { Exists: true })
                    {
                        var currentVersionDirInfo = versionsDir.GetDirectories("current").FirstOrDefault();
                        if (currentVersionDirInfo is { Exists: true })
                        {
                            var contentFiles = GetContentFiles(dirInfo);
                            var filesToDelete = GetFilesToDelete(currentVersionDirInfo, contentFiles);
                            if (filesToDelete.Any())
                            {
                                Logger.Info($"Found {filesToDelete.Length} files in current version folder " +
                                    $"for content {contentId}, but the number is limited to {numFiles}");
                                numFiles = ProcessFiles(numFiles, filesToDelete);
                            }
                        }
                    }
                }
            }
        }
        return numFiles;
    }

    private static int ProcessFiles(int numFiles, FileInfo[] filesToDelete)
    {
        var processed = 0;
        foreach (var info in filesToDelete.Take(numFiles))
        {
            Logger.Info($"Deleting file {info.FullName}");
            info.Delete();
            processed++;
        }
        return numFiles - processed;
    }

    private string[] GetContentFiles(DirectoryInfo rootDirInfo)
    {
        var result = new List<string>();
        if (_pathHelper.UseS3)
        {
            var path = rootDirInfo.ToString();
            var dirs = _pathHelper.ListS3Files(path, onlyDirs: true)
                .Where(n => n.Name != ArticleVersion.RootFolder && n.Name != "_temp").ToArray();
            foreach (var dir in dirs)
            {
                result.AddRange(_pathHelper.ListS3Files(dir.FullName, recursive: true).Select(n => n.Name).ToArray());
            }
            result.AddRange(_pathHelper.ListS3Files(path).Select(n => n.Name).ToArray());
        }
        else
        {
            var dirs = rootDirInfo.GetDirectories("*", SearchOption.TopDirectoryOnly)
                .Where(n => n.Name != ArticleVersion.RootFolder && n.Name != "_temp").ToArray();
            foreach (var dir in dirs)
            {
                result.AddRange(dir.EnumerateFiles("*", SearchOption.AllDirectories).Select(n => n.Name));
            }
            result.AddRange(rootDirInfo.EnumerateFiles("*", SearchOption.TopDirectoryOnly).Select(n => n.Name));
        }
        return result.ToArray();
    }

    private DirectoryInfo GetContentRootDirectoryInfo(int contentId)
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

    private DirectoryInfo GetSiteRootDirectoryInfo(int siteId)
    {
        var factory = new SiteFolderFactory();
        var repository = factory.CreateRepository();
        var root = repository.GetRoot(siteId);
        if (root == null)
        {
            return null;
        }
        var pathInfo = root.PathInfo;
        return new DirectoryInfo(pathInfo.Path);
    }

    private FileInfo[] GetFilesToDelete(DirectoryInfo currentVersionDir, string[] contentFiles)
    {
        var hash = new HashSet<string>(contentFiles);
        return currentVersionDir.EnumerateFiles("*", SearchOption.TopDirectoryOnly)
            .Where(n => !hash.Contains(n.Name)).ToArray();
    }

}
