using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.StaticFiles;
using Minio;
using Minio.DataModel.Args;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Formats;
using System.Reflection;
using Minio.DataModel.Response;

namespace Quantumart.QP8.BLL.Helpers;

public class PathHelper
{
    private readonly IDbService _dbService;
    private readonly IMinioClient _client;
    private readonly S3Options _options;
    private static readonly ILogger _logger = LogManager.GetCurrentClassLogger();
    private readonly PropertyInfo _responseField = typeof(GenericResponse)
        .GetProperty("ResponseContent", BindingFlags.Instance | BindingFlags.NonPublic);
    private readonly PropertyInfo _codeField = typeof(GenericResponse)
        .GetProperty("ResponseStatusCode", BindingFlags.Instance | BindingFlags.NonPublic);


    public PathHelper(IDbService dbService)
    {
        _dbService = dbService;
        _options = dbService.S3Options;
        if (UseS3)
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
        if (UseS3 && !path.StartsWith(QPConfiguration.TempDirectory))
        {
            result = result.Replace(@"\", @"/");
        }
        return result;
    }

    public string FixPathSeparator(string path)
    {
        var result = path;
        if (UseS3)
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

    public char Separator => UseS3 ? '/' : Path.DirectorySeparatorChar;

    public bool UseS3 => _dbService.UseS3();

    public S3Options S3Options => UseS3 ? _options : new S3Options();

    public bool FileExists(string path)
    {
        if (UseS3 && !path.StartsWith(QPConfiguration.TempDirectory))
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_options.Bucket)
                    .WithObject(FixPathSeparator(path));
                var statObject = Task.Run(async () => await _client.StatObjectAsync(statObjectArgs)).Result;
                return statObject.ETag != null && statObject.Size != 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
        return File.Exists(path);
    }

    public FolderFile GetS3File(string path)
    {
        VerifyS3BucketExists();
        var args = new StatObjectArgs().WithBucket(_options.Bucket).WithObject(FixPathSeparator(path));
        var stat = Task.Run(async () => await _client.StatObjectAsync(args)).Result;
        return new FolderFile(stat);
    }

    public void VerifyS3BucketExists()
    {
        var args = new BucketExistsArgs().WithBucket(_options.Bucket);
        var result = Task.Run(async () => await _client.BucketExistsAsync(args)).Result;
        if (!result)
        {
            throw new ApplicationException($"Bucket {_options.Bucket} does not exist");
        }
    }


    public IEnumerable<FolderFile> ListS3Files(string path, bool recursive = false, bool onlyDirs = false, string pattern = null)
    {
        VerifyS3BucketExists();
        var result = new List<FolderFile>();
        if (UseS3)
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
            try
            {
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
            catch (Exception e)
            {
                _logger.Error(e);
                throw new ApplicationException($"Cannot receive file list for path {path} in bucket {_options.Bucket}");
            }
        }
        return result;
    }

    public MemoryStream GetS3Stream(string path)
    {
        VerifyS3BucketExists();
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
        VerifyS3BucketExists();
        new FileExtensionContentTypeProvider().TryGetContentType(path, out var contentType);

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(FixPathSeparator(path))
            .WithContentType(contentType ?? "application/octet-stream")
            .WithStreamData(stream)
            .WithObjectSize(stream.Length);

        var result = Task.Run(async () => await _client.PutObjectAsync(putObjectArgs)).Result;
        if (result.Etag == null)
        {
            _logger.ForErrorEvent()
                .Message($"Error while saving file {path} to S3")
                .Property("code", _codeField?.GetValue(result))
                .Property("response", _responseField?.GetValue(result))
                .Log();

            throw new ApplicationException($"Error while saving file to S3");
        }
        _logger.Info($"File {path} saved to S3");
    }

    public void RemoveS3File(string path)
    {
        VerifyS3BucketExists();
        var fixedPath = FixPathSeparator(path);
        var objectArgs = new RemoveObjectArgs().WithBucket(_options.Bucket).WithObject(fixedPath);
        Task.Run(async () => await _client.RemoveObjectAsync(objectArgs)).Wait();
        _logger.Info($"File {fixedPath} removed from S3");
    }

    public void CopyS3File(string path, string newPath)
    {
        VerifyS3BucketExists();
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
        VerifyS3BucketExists();
        var fileNames = files.Select(n => n.FullName).ToArray();
        var objectArgs = new RemoveObjectsArgs().WithBucket(_options.Bucket)
            .WithObjects(fileNames);
        Task.Run(async () => await _client.RemoveObjectsAsync(objectArgs)).Wait();
        _logger.Info($"Files {string.Join(", ", fileNames)} removed from S3");
    }

    public void RemoveS3Folder(string path)
    {
        VerifyS3BucketExists();
        var files = ListS3Files(path, true);
        if (files.Any())
        {
            RemoveS3Files(files);
        }
    }

    public void Rename(string path, string newPath)
    {
        VerifyS3BucketExists();
        if (!FileExists(path))
        {
            _logger.ForWarnEvent()
                .Message("Cannot rename non-existing file")
                .Property(nameof(path), path)
                .Log();
            return;
        }
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

    public void Copy(string path, string newPath)
    {
        if (!FileExists(path))
        {
            _logger.ForWarnEvent()
                .Message("Cannot copy non-existing file")
                .Property(nameof(path), path)
                .Log();
            return;
        }
        if (UseS3)
        {
            CopyS3File(path, newPath);
        }
        else
        {
            File.SetAttributes(path, FileAttributes.Normal);
            File.Copy(path, newPath, true);
            File.SetAttributes(newPath, FileAttributes.Normal);
        }
    }

    public void Remove(string path)
    {
        if (!FileExists(path))
        {
            _logger.ForWarnEvent()
                .Message("Cannot remove non-existing file")
                .Property(nameof(path), path)
                .Log();
            return;
        }
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
        if (UseS3)
        {
            using var stream = GetS3Stream(path);
            return Image.Identify(stream);
        }
        return Image.Identify(path);
    }

    public Image LoadImage(string path)
    {
        if (UseS3)
        {
            using var stream = GetS3Stream(path);
            return Image.Load(stream);
        }
        return Image.Load(path);
    }

    public void SaveImage(Image image, string path, IImageEncoder encoder = null)
    {
        if (UseS3)
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
