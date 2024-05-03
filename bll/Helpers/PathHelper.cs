using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.VisualBasic;
using Minio;
using Minio.DataModel.Args;
using NLog;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;

namespace Quantumart.QP8.BLL.Helpers;

public class PathHelper
{
    private readonly IDbService _dbService;
    private readonly IMinioClient _client;
    private readonly S3Options _options;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();

    public PathHelper(IDbService dbService)
    {
        _dbService = dbService;
        _options = dbService.S3Options;
        if (_dbService.UseS3())
        {
            _client = new MinioClient()
                .WithEndpoint(_options.Endpoint)
                .WithCredentials(_options.AccessKey, _options.SecretKey)
                .Build();
        }
    }

    public string CombinePath(string path, string name)
    {
        var result = Path.Combine(path, name);
        if (_dbService.UseS3())
        {
            result = result.Replace(@"\", @"/");
        }
        return result;
    }

    public string FixPathSeparator(string path)
    {
        var result = path;
        if (_dbService.UseS3())
        {
            result = result.Replace(@"\", @"/");
        }
        return result;
    }

    public string RemoveLeadingSeparator(string path)
    {
        return path.StartsWith('/') ? path.Substring(1) : path;
    }

    public string AddTrailingSeparator(string path)
    {
        return path.EndsWith('/') ? path : path + '/';
    }

    public char Separator => _dbService.UseS3() ? '/' : Path.DirectorySeparatorChar;

    public bool UseS3 => _dbService.UseS3();

    public bool FileExists(string path)
    {
        if (_dbService.UseS3())
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_options.Bucket)
                    .WithObject(FixPathSeparator(path));
                var statObject = Task.Run(async () => await _client.StatObjectAsync(statObjectArgs)).Result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        return System.IO.File.Exists(path);
    }

    public FolderFile GetS3File(string path)
    {
        var args = new StatObjectArgs().WithBucket(_options.Bucket).WithObject(FixPathSeparator(path));
        var stat = Task.Run(async () => await _client.StatObjectAsync(args)).Result;
        return new FolderFile(stat);
    }


    public IEnumerable<FolderFile> ListS3Files(string path, bool recursive = false, bool onlyDirs = false, string pattern = null)
    {
        var result = new List<FolderFile>();
        if (_dbService.UseS3())
        {
            path = FixPathSeparator(path);
            path = RemoveLeadingSeparator(path);
            path = AddTrailingSeparator(path);

            var listObjectArgs = new ListObjectsArgs()
                .WithBucket(_options.Bucket)
                .WithPrefix(path)
                .WithRecursive(recursive)
                .WithVersions(true);
            var observable = _client.ListObjectsAsync(listObjectArgs);
            Task.Run(async () =>
                await observable.Do(item =>
                {
                    if (!item.IsDir && !onlyDirs || item.IsDir && onlyDirs)
                    {
                        var file = new FolderFile(item, path);
                        if (string.IsNullOrWhiteSpace(pattern) || file.Name.Contains(pattern))
                        {
                            result.Add(file);
                        }
                    }
                }).LastOrDefaultAsync()
            ).Wait();
        }
        return result;
    }

    public MemoryStream GetS3Stream(string path)
    {
        MemoryStream memoryStream = new MemoryStream();
        GetObjectArgs getObjectArgs = new GetObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(FixPathSeparator(path))
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(memoryStream);
            });
        _ = Task.Run(async () =>
            await _client.GetObjectAsync(getObjectArgs)
        ).Result;
        memoryStream.Position = 0;
        return memoryStream;
    }

    public void SetS3File(Stream stream, string path)
    {
        new FileExtensionContentTypeProvider().TryGetContentType(path, out var contentType);

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(FixPathSeparator(path))
            .WithContentType(contentType ?? "application/octet-stream")
            .WithStreamData(stream)
            .WithObjectSize(stream.Length);

        Task.Run(async () => await _client.PutObjectAsync(putObjectArgs)).Wait();
        _logger.Info($"File {path} saved to S3");
    }

    public void RemoveS3File(string path)
    {
        var fixedPath = FixPathSeparator(path);
        var objectArgs = new RemoveObjectArgs().WithBucket(_options.Bucket).WithObject(fixedPath);
        Task.Run(async () => await _client.RemoveObjectAsync(objectArgs)).Wait();
        _logger.Info($"File {fixedPath} removed from S3");
    }

    public void CopyS3File(string path, string newPath)
    {
        var fixedPath = FixPathSeparator(path);
        var fixedNewPath = FixPathSeparator(newPath);
        var sourceArgs = new CopySourceObjectArgs().WithBucket(_options.Bucket).WithObject(fixedPath);
        var destArgs = new CopyObjectArgs().WithBucket(_options.Bucket).WithObject(fixedNewPath)
            .WithCopyObjectSource(sourceArgs);

        Task.Run(async () => await _client.CopyObjectAsync(destArgs)).Wait();
        _logger.Info($"File {fixedNewPath} copied from file {fixedPath} in S3");
    }

    public void RemoveS3Files(IEnumerable<FolderFile> files)
    {
        var fileNames = files.Select(n => n.FullName).ToArray();
        var objectArgs = new RemoveObjectsArgs().WithBucket(_options.Bucket)
            .WithObjects(fileNames);
        Task.Run(async () => await _client.RemoveObjectsAsync(objectArgs)).Wait();
        _logger.Info($"Files {string.Join(", ", fileNames)} removed from S3");
    }

    public void RemoveS3Folder(string path)
    {
        var files = ListS3Files(path);
        if (files.Any())
        {
            RemoveS3Files(files);
        }
    }

    public void Rename(string path, string newPath)
    {
        if (UseS3)
        {
            CopyS3File(path, newPath);
            RemoveS3File(path);
        }
        else
        {
            File.SetAttributes(path, FileAttributes.Normal);
            File.Move(path, newPath);
        }
    }

    public void Remove(string path)
    {
        if (UseS3)
        {
            RemoveS3File(path);
        }
        else
        {
            File.SetAttributes(path, FileAttributes.Normal);
            File.Delete(path);
        }
    }

    public void RemoveFolder(string path)
    {
        if (UseS3)
        {
            RemoveS3Folder(path);
        }
        else
        {
            if (Directory.Exists(path))
            {
                var directory = new DirectoryInfo(path) { Attributes = FileAttributes.Normal };

                foreach (var info in directory.EnumerateFileSystemInfos("*", SearchOption.AllDirectories))
                {
                    info.Attributes = FileAttributes.Normal;
                }

                directory.Delete(true);
            }
        }

    }

    public ImageInfo IdentifyImage(string path)
    {
        if (_dbService.UseS3())
        {
            using var stream = GetS3Stream(path);
            return Image.Identify(stream);
        }
        return Image.Identify(path);
    }

    public Image LoadImage(string path)
    {
        if (_dbService.UseS3())
        {
            using var stream = GetS3Stream(path);
            return Image.Load(stream);
        }
        return Image.Load(path);
    }

    public void SaveImage(Image image, string path, IImageEncoder encoder = null)
    {
        if (_dbService.UseS3())
        {
            using var stream = new MemoryStream();
            image.Save(stream, encoder ?? image.DetectEncoder(path));
            stream.Position = 0;
            SetS3File(stream, path);
        }
        else
        {
            image.Save(path, encoder ?? image.DetectEncoder(path));
        }
    }

    public static string GetUploadPath()
    {
        var sb = new StringBuilder();
        sb.Append(QPConfiguration.TempDirectory);
        sb.Append(Path.DirectorySeparatorChar);
        sb.Append(QPContext.CurrentCustomerCode);
        sb.Append(Path.DirectorySeparatorChar);
        sb.Append(QPContext.CurrentUserId);
        sb.Append(Path.DirectorySeparatorChar);
        return sb.ToString();
    }

    public static void EnsureUploadPathCreated()
    {
        var path = GetUploadPath();
        if (path != null && !Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }
}
