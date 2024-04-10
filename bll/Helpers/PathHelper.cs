using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Minio;
using Minio.DataModel.Args;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.BLL.Helpers;

public class PathHelper
{
    private readonly IDbService _dbService;
    private readonly IMinioClient _client;
    private readonly S3Options _options;

    public PathHelper(IDbService dbService, S3Options options)
    {
        _dbService = dbService;
        _options = options;
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
            result = path.Replace(@"\", @"/");
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

    public bool FileExists(string path)
    {
        if (_dbService.UseS3())
        {
            try
            {
                var statObjectArgs = new StatObjectArgs()
                    .WithBucket(_options.Bucket)
                    .WithObject(path);
                Task.Run(() => _client.StatObjectAsync(statObjectArgs)).GetAwaiter().GetResult();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        return System.IO.File.Exists(path);
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
