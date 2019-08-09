using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QP8.Infrastructure.Logging;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.Configuration;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using FileIO = System.IO.File;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class UploadController : QPController
    {
        private readonly IBackendActionLogRepository _logger;

        public UploadController(IBackendActionLogRepository logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> UploadChunk(int? chunk, int? chunks, string name, string destinationUrl)
        {
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                var errorMsg = $"File to upload: \"{name}\" has invalid characters";
                Logger.Log.Warn(errorMsg);
                return Json(new { message = errorMsg, isError = true });
            }

            destinationUrl = WebUtility.UrlDecode(destinationUrl);
            if (string.IsNullOrEmpty(destinationUrl))
            {
                throw new ArgumentException("Folder Path is empty");
            }

            if (!Directory.Exists(destinationUrl))
            {
                Directory.CreateDirectory(destinationUrl);
            }

            chunk = chunk ?? 0;
            chunks = chunks ?? 1;
            PathSecurityResult securityResult;

            var fileUpload = Request.Form.Files[0];
            var tempPath = Path.Combine(QPConfiguration.TempDirectory, name);
            var destPath = Path.Combine(destinationUrl, name);

            if (chunk == 0 && chunks == 1)
            {
                securityResult = PathInfo.CheckSecurity(destinationUrl);
                if (!securityResult.Result)
                {
                    var errorMsg = string.Format(PlUploadStrings.ServerError, name, destinationUrl, $"Access to the folder (ID = {securityResult.FolderId}) denied");
                    Logger.Log.Warn(errorMsg);
                    return Json(new { message = errorMsg, isError = true });
                }

                try
                {
                    using (var fileStream = new FileStream(destPath, FileMode.Create))
                    {
                        await fileUpload.CopyToAsync(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = string.Format(PlUploadStrings.ServerError, name, destinationUrl, ex.Message);
                    Logger.Log.Error(errorMsg, ex);
                    return Json(new { message = errorMsg, isError = true });
                }
            }
            else
            {
                try
                {
                    using (var fileStream = new FileStream(tempPath, chunk == 0 ? FileMode.Create : FileMode.Append))
                    {
                       await fileUpload.CopyToAsync(fileStream);
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = string.Format(PlUploadStrings.ServerError, name, tempPath, ex.Message);
                    Logger.Log.Error(errorMsg, ex);
                    return Json(new { message = errorMsg, isError = true });
                }

                try
                {
                    var isTheLastChunk = chunk.Value == chunks.Value - 1;
                    if (isTheLastChunk)
                    {
                        securityResult = PathInfo.CheckSecurity(destinationUrl);
                        var actionCode = securityResult.IsSite ? ActionCode.UploadSiteFile : ActionCode.UploadContentFile;

                        if (!securityResult.Result)
                        {
                            var errorMsg = string.Format(PlUploadStrings.ServerError, name, destinationUrl, $"Access to the folder (ID = {securityResult.FolderId}) denied");
                            Logger.Log.Warn(errorMsg);
                            return Json(new { message = errorMsg, isError = true });
                        }

                        if (FileIO.Exists(destPath))
                        {
                            FileIO.SetAttributes(destPath, FileAttributes.Normal);
                            FileIO.Delete(destPath);
                        }

                        FileIO.Move(tempPath, destPath);
                        BackendActionContext.SetCurrent(actionCode, new[] { name }, securityResult.FolderId);

                        var logs = BackendActionLog.CreateLogs(BackendActionContext.Current, _logger);
                        _logger.Save(logs);

                        BackendActionContext.ResetCurrent();
                    }
                }
                catch (Exception ex)
                {
                    var errorMsg = string.Format(PlUploadStrings.ServerError, name, destinationUrl, ex.Message);
                    Logger.Log.Error(errorMsg, ex);
                    return Json(new { message = errorMsg, isError = true });
                }

                return Json(new { message = $"chunk#{chunk.Value}, of file{name} uploaded", isError = false });
            }

            return Json(new { message = $"file{name} uploaded", isError = false });
        }
    }
}
