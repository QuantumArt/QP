using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Minio;
using NLog;
using NLog.Fluent;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using FileIO = System.IO.File;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class UploadController : AuthQpController
    {
        private readonly IBackendActionLogRepository _logger;
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly S3Options _options;
        private readonly IDbService _dbService;
        private readonly PathHelper _pathHelper;

        public UploadController(
            IBackendActionLogRepository logger,
            S3Options options,
            IDbService dbService,
            PathHelper pathHelper
        )
        {
            _logger = logger;
            _options = options;
            _dbService = dbService;
            _pathHelper = pathHelper;
            EnsureS3ClientCreated();
        }

        private void EnsureS3ClientCreated()
        {
            if (_dbService.UseS3())
            {
                new MinioClient()
                    .WithEndpoint(_options.Endpoint)
                    .WithCredentials(_options.AccessKey, _options.SecretKey)
                    .Build();
            }
        }

        private void LogError(string msg, string fileName, Exception ex = null)
        {
            var msgBuilder = Logger.Error().Message(msg).Property("fileName", fileName);
            if (ex != null)
            {
                msgBuilder.Exception(ex);
            }
            msgBuilder.Write();
        }

        [HttpPost]
        public async Task<ActionResult> UploadChunk(
            IFormFile file, int? chunk, int? chunks, string name, string destinationUrl
        )
        {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                var errorMsg = $"File to upload: \"{name}\" has invalid characters";
                LogError(errorMsg, name);
                return Json(new { message = errorMsg, isError = true });
            }

            destinationUrl = WebUtility.UrlDecode(destinationUrl);
            if (string.IsNullOrEmpty(destinationUrl))
            {
                throw new ArgumentException("Folder Path is empty");
            }

            if (_pathHelper.UseS3)
            {
                destinationUrl = _pathHelper.FixPathSeparator(destinationUrl);
            }
            else
            {
                if (!Directory.Exists(destinationUrl))
                {
                    Directory.CreateDirectory(destinationUrl);
                }
            }

            chunk ??= 0;
            chunks ??= 1;
            PathSecurityResult securityResult;

            var tempPath = Path.Combine(QPConfiguration.TempDirectory, name);
            var destPath = _pathHelper.CombinePath(destinationUrl, name);

            if (chunk == 0 && chunks == 1)
            {
                securityResult = PathInfo.CheckSecurity(destinationUrl, true, _pathHelper.Separator);
                if (!securityResult.Result)
                {
                    var errorMsg = string.Format(PlUploadStrings.ServerError, name, destinationUrl, $"Access to the folder (ID = {securityResult.FolderId}) denied");

                    LogError(errorMsg, name);

                    return Json(new { message = errorMsg, isError = true });
                }

                try
                {
                    if (_dbService.UseS3())
                    {
                        var stream = new MemoryStream((int)file.Length);
                        await file.CopyToAsync(stream);
                        stream.Position = 0;
                        _pathHelper.SetS3File(stream, destPath);
                    }
                    else
                    {
                        await using var fileStream = new FileStream(destPath, FileMode.Create);
                        await file.CopyToAsync(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = string.Format(PlUploadStrings.ServerError, name, destinationUrl, ex.Message);

                    LogError(errorMsg, name, ex);

                    return Json(new { message = errorMsg, isError = true });
                }
            }
            else
            {
                try
                {
                    await using var fileStream = new FileStream(tempPath, chunk == 0 ? FileMode.Create : FileMode.Append);
                    await file.CopyToAsync(fileStream);
                }
                catch (Exception ex)
                {
                    var errorMsg = string.Format(PlUploadStrings.ServerError, name, tempPath, ex.Message);

                    LogError(errorMsg, name, ex);

                    return Json(new { message = errorMsg, isError = true });
                }

                try
                {
                    var isTheLastChunk = chunk.Value == chunks.Value - 1;
                    if (isTheLastChunk)
                    {
                        securityResult = PathInfo.CheckSecurity(destinationUrl, true, _pathHelper.Separator);
                        var actionCode = securityResult.IsSite ? ActionCode.UploadSiteFile : ActionCode.UploadContentFile;

                        if (!securityResult.Result)
                        {
                            var errorMsg = string.Format(PlUploadStrings.ServerError, name, destinationUrl, $"Access to the folder (ID = {securityResult.FolderId}) denied");

                            LogError(errorMsg, name);

                            return Json(new { message = errorMsg, isError = true });
                        }

                        if (_dbService.UseS3())
                        {
                            await using var tempStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read);
                            _pathHelper.SetS3File(tempStream, destPath);
                        }
                        else
                        {
                            if (FileIO.Exists(destPath))
                            {
                                FileIO.SetAttributes(destPath, FileAttributes.Normal);
                                FileIO.Delete(destPath);
                            }

                            FileIO.Move(tempPath, destPath!);
                        }

                        BackendActionContext.SetCurrent(actionCode, new[] { name }, securityResult.FolderId);

                        var logs = BackendActionLog.CreateLogs(BackendActionContext.Current, _logger);
                        _logger.Save(logs);

                        BackendActionContext.ResetCurrent();
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = string.Format(PlUploadStrings.ServerError, name, destinationUrl, ex.Message);

                    LogError(errorMsg, name, ex);

                    return Json(new { message = errorMsg, isError = true });
                }

                return Json(new { message = $"chunk#{chunk.Value}, of file{name} uploaded", isError = false });
            }

            return Json(new { message = $"file{name} uploaded", isError = false });
        }
    }
}
