using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Minio;
using Minio.DataModel.Args;
using NLog;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Utils;
using SixLabors.ImageSharp;

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

    public string FixPath(string path)
    {
        var result = path;
        if (_dbService.UseS3())
        {
            result = path.Replace(@"\", @"/");
        }
        return result;
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
                    .WithObject(path);
                _ = Task.Run(async () => await _client.StatObjectAsync(statObjectArgs)).Result;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        return System.IO.File.Exists(path);
    }

    public IEnumerable<FolderFile> GetFiles(string path)
    {
        var result = new List<FolderFile>();
        if (_dbService.UseS3())
        {
            path = path.StartsWith('/') ? path.Substring(1) : path;
            path = path.EndsWith('/') ? path : path + '/';

            var listObjectArgs = new ListObjectsArgs()
                .WithBucket(_options.Bucket)
                .WithPrefix(path)
                .WithRecursive(false)
                .WithVersions(true);
            var observable = _client.ListObjectsAsync(listObjectArgs);
            _ = Task.Run(async () =>
                await observable.Do(item => result.Add(new FolderFile(item, path))).LastOrDefaultAsync()
            ).Result;
        }
        return result;
    }

    public Stream GetFile(string path)
    {
        MemoryStream memoryStream = new MemoryStream();
        GetObjectArgs getObjectArgs = new GetObjectArgs()
            .WithBucket(_options.Bucket)
            .WithObject(FixPath(path))
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

    public Image LoadImage(string path)
    {
        if (_dbService.UseS3())
        {
            using var stream = GetFile(path);
            return Image.Load(stream);
        }
        return Image.Load(path);
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
