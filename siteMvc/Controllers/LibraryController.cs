using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Resources;
using Quantumart.QP8.WebMvc.Extensions.Controllers;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class LibraryController : QPController
    {
        private const string ContentDispositionKey = "Content-Disposition";
        private const string ContentDispositionTemplate = "attachment; filename=\"{0}\"; filename*=UTF-8''{0}";

        public class FilePropertiesOptions
        {
            public bool IsVersion { get; set; }

            public string NotSupportedMessage { get; set; }

            public FilePropertiesOptions()
            {
                IsVersion = false;
                NotSupportedMessage = LibraryStrings.OperationIsNotSupported;
            }
        }


        private JsonResult GetTestFileDownloadResult(PathInfo info, string fileName, bool isVersion)
        {
            var normilizedFileName = isVersion ? Path.GetFileName(fileName) : fileName;
            var path = info.GetPath(normilizedFileName);
            return System.IO.File.Exists(path)
                ? Json(new { proceed = true, key = SavePath(path) }, JsonRequestBehavior.AllowGet)
                : Json(new { proceed = false, msg = string.Format(LibraryStrings.NotExists, normilizedFileName) }, JsonRequestBehavior.AllowGet);
        }

        private string SavePath(string path)
        {
            var rnd = new Random();
            var key = rnd.Next().ToString();
            TempData[key] = path;
            return key;
        }

        private static PathInfo GetFilePathInfo(int fieldId, int? entityId, bool isVersion)
        {
            // ReSharper disable once PossibleInvalidOperationException
            return !isVersion ?
                FieldService.GetPathInfo(fieldId, entityId)
                : ArticleVersionService.GetPathInfo(fieldId, (int)entityId);
        }

        private JsonResult GetFileProperties(PathInfo pathInfo, string fileName, FilePropertiesOptions options)
        {
            var normalizedFileName = options.IsVersion ? Path.GetFileName(fileName) : fileName;
            var path = pathInfo.GetPath(normalizedFileName);
            var url = string.Empty;
            var message = string.Empty;
            int width = 0, height = 0;

            if (System.IO.File.Exists(path))
            {
                url = pathInfo.GetUrl(normalizedFileName);
                if (!GetImageSize(path, ref width, ref height))
                {
                    message = string.Format(options.NotSupportedMessage, normalizedFileName, Path.GetExtension(normalizedFileName));
                }
            }
            else
            {
                message = string.Format(LibraryStrings.NotExists, normalizedFileName);
            }

            var result = !string.IsNullOrEmpty(message)
                ? (object)new { proceed = false, msg = message }
                : new { proceed = true, url, folderUrl = pathInfo.Url, width, height };
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        private static bool GetImageSize(string path, ref int width, ref int height)
        {
            try
            {
                using (var img = Image.FromFile(path))
                {
                    width = img.Width;
                    height = img.Height;
                    return true;
                }
            }
            catch (OutOfMemoryException)
            {
            }

            return false;
        }

        /// <summary>
        /// Формирует имя файла для ресолвинга
        /// </summary>
        /// <param name="path"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static string GetResolvingFileName(string path, string name)
        {
            string result;
            var index = 0;
            do
            {
                index++;
                result = MutateHelper.MutateFileName(name, index);
            }

            while (System.IO.File.Exists(Path.Combine(path, result)));
            return result;
        }

        [HttpGet]
        public JsonResult FullNameFileExists(string name)
        {
            return Json(new { result = name != null && System.IO.File.Exists(name) }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult FileExists(string path, string name)
        {
            return new JsonResult { Data = new { result = name != null && System.IO.File.Exists(Path.Combine(path, name)) }, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpGet]
        public JsonResult ResolveFileName(string path, string name)
        {
            var result = GetResolvingFileName(path, name);

            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }

        [NonAction]
        public static string UploadSecuritySessionKey(string path)
        {
            return "upload_" + path;
        }

        [HttpGet]
        public JsonResult CheckSecurity(string path)
        {
            var result = PathInfo.CheckSecurity(path).Result && CheckFolderExistence(path);
            Session[UploadSecuritySessionKey(path)] = result;
            return Json(new { result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult TestFieldValueDownload(string id, string fileName, bool isVersion, int? entityId)
        {
            var fieldId = Field.ParseFormName(id);
            if (fieldId == 0)
            {
                return Json(new { proceed = false, msg = string.Format(LibraryStrings.InvalidId, id) }, JsonRequestBehavior.AllowGet);
            }

            var info = GetFilePathInfo(fieldId, entityId, isVersion);
            return GetTestFileDownloadResult(info, fileName, isVersion);
        }

        [HttpGet]
        public JsonResult ExportFileDownload(int id, string fileName)
        {
            var currentSite = SiteService.Read(id);
            var folderForUpload = $"{currentSite.LiveDirectory}\\temp\\";
            var inf = new PathInfo
            {
                Path = folderForUpload
            };

            return GetTestFileDownloadResult(inf, fileName, false);
        }


        [HttpGet]
        public JsonResult TestLibraryFileDownload(int id, string fileName, string entityTypeCode)
        {
            var pathInfo = entityTypeCode == EntityTypeCode.ContentFile ? ContentFolderService.GetPathInfo(id) : SiteFolderService.GetPathInfo(id);
            if (pathInfo == null)
            {
                var formatString = entityTypeCode == EntityTypeCode.ContentFile ? LibraryStrings.ContentFolderNotExists : LibraryStrings.SiteFolderNotExists;
                return Json(new { proceed = false, msg = string.Format(formatString, id) }, JsonRequestBehavior.AllowGet);
            }

            return GetTestFileDownloadResult(pathInfo, fileName, false);
        }

        [HttpGet]
        public JsonResult GetImageProperties(string id, string fileName, bool isVersion, int? entityId, int? parentEntityId)
        {
            var fieldId = Field.ParseFormName(id);
            if (fieldId == 0)
            {
                return Json(new { proceed = false, msg = string.Format(LibraryStrings.InvalidId, id) }, JsonRequestBehavior.AllowGet);
            }

            var pathInfo = GetFilePathInfo(fieldId, entityId, isVersion);
            return GetFileProperties(pathInfo, fileName, new FilePropertiesOptions { IsVersion = isVersion });
        }

        [HttpGet]
        public JsonResult GetLibraryImageProperties(int id, string fileName, string entityTypeCode)
        {
            var pathInfo = entityTypeCode == EntityTypeCode.ContentFile ? ContentFolderService.GetPathInfo(id) : SiteFolderService.GetPathInfo(id);
            return GetFileProperties(pathInfo, fileName, new FilePropertiesOptions());
        }

        [HttpPost]
        public JsonResult CheckForCrop(string targetFileUrl)
        {
            var path = PathInfo.ConvertToPath(targetFileUrl);
            var ext = Path.GetExtension(path);
            if (System.IO.File.Exists(path))
            {
                return Json(new { ok = false, message = string.Format(LibraryStrings.FileExistsTryAnother, path) });
            }

            if (string.IsNullOrEmpty(ext) || GetImageCodecInfo(ext) == null)
            {
                return Json(new { ok = false, message = string.Format(LibraryStrings.ExtensionIsNotAllowed, ext) });
            }

            if (!PathInfo.CheckSecurity(path).Result)
            {
                return Json(new { ok = false, message = string.Format(LibraryStrings.AccessDenied, path, QPContext.CurrentUserName) });
            }

            return Json(new { ok = true, message = string.Empty });
        }

        [HttpPost]
        public JsonResult Crop(bool overwriteFile, string targetFileUrl, string sourceFileUrl, double resize, int? top, int? left, int? width, int? height)
        {
            var sourcePath = PathInfo.ConvertToPath(sourceFileUrl);
            var targetPath = overwriteFile ? sourcePath : PathInfo.ConvertToPath(targetFileUrl);
            if (!PathInfo.CheckSecurity(targetPath).Result)
                return Json(new { ok = false, message = LibraryStrings.AccessDenied });

            int sourceWidth = 0, sourceHeight = 0, sourceTop = 0, sourceLeft = 0;
            if (!GetImageSize(sourcePath, ref sourceWidth, ref sourceHeight))
                return Json(new { ok = false, message = LibraryStrings.ExtensionIsNotAllowed });

            var targetWidth = width ?? (int)(sourceWidth * resize);
            var targetHeight = height ?? (int)(sourceHeight * resize);
            var targetTop = 0;
            var targetLeft = 0;

            sourceWidth = width.HasValue ? (int)(width.Value / resize) : sourceWidth;
            sourceHeight = height.HasValue ? (int)(height.Value / resize) : sourceHeight;
            sourceTop = top.HasValue ? (int)(top.Value / resize) : sourceTop;
            sourceLeft = left.HasValue ? (int)(left.Value / resize) : sourceLeft;

            if (System.IO.File.Exists(sourcePath))
            {
                using (var output = new Bitmap(targetWidth, targetHeight))
                {
                    var resizer = Graphics.FromImage(output);
                    resizer.InterpolationMode = resize < 1 ? InterpolationMode.HighQualityBicubic : InterpolationMode.HighQualityBilinear;

                    using (var input = new Bitmap(sourcePath))
                    {
                        resizer.DrawImage(input, new Rectangle(targetLeft, targetTop, targetWidth, targetHeight), sourceLeft, sourceTop, sourceWidth, sourceHeight, GraphicsUnit.Pixel);
                    }

                    // ReSharper disable once AssignNullToNotNullAttribute
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    if (System.IO.File.Exists(targetPath))
                    {
                        System.IO.File.SetAttributes(targetPath, FileAttributes.Normal);
                        System.IO.File.Delete(targetPath);
                    }

                    using (var fs = System.IO.File.OpenWrite(targetPath))
                    {
                        var ext = Path.GetExtension(targetPath);
                        output.Save(fs, GetImageCodecInfo(ext), GetEncoderParameters(ext));
                    }

                }
            }

            return Json(new { ok = true, message = string.Empty });
        }

        private static EncoderParameters GetEncoderParameters(string extension)
        {
            if (extension.Equals(".JPG", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".JPEG", StringComparison.InvariantCultureIgnoreCase))
            {
                var parameters = new EncoderParameters(1)
                {
                    Param = { [0] = new EncoderParameter(Encoder.Quality, 90L) }
                };

                return parameters;
            }

            return null;
        }

        private static string GetMimeType(string extension)
        {
            if (extension.Equals(".JPG", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".JPEG", StringComparison.InvariantCultureIgnoreCase))
            {
                return "image/jpeg";
            }

            if (extension.Equals(".GIF", StringComparison.InvariantCultureIgnoreCase))
            {
                return "image/gif";
            }

            if (extension.Equals(".PNG", StringComparison.InvariantCultureIgnoreCase))
            {
                return "image/png";
            }

            return string.Empty;
        }

        private static ImageCodecInfo GetImageCodecInfo(string extension)
        {
            return ImageCodecInfo.GetImageEncoders().SingleOrDefault(n => n.MimeType == GetMimeType(extension));
        }

        [HttpGet]
        public ActionResult DownloadFile(string id, string fileName)
        {
            var path = (string)TempData[id];
            if (!string.IsNullOrEmpty(path))
            {
                Response.AppendHeader(ContentDispositionKey, string.Format(ContentDispositionTemplate, Server.UrlPathEncode(fileName)));
                return File(path, "application/octet-stream");
            }

            return null;
        }

        [HttpPost]
        public ActionResult UploadFile(string folderPath, bool resolveFileName)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                throw new ArgumentException("Folder Path is empty");
            }

            if (!PathInfo.CheckSecurity(folderPath).Result || !CheckFolderExistence(folderPath))
            {
                return Json(new { proceed = false, msg = string.Format(LibraryStrings.UploadIsNotAllowed, folderPath) });
            }

            var allFileNames = new List<string>(HttpContext.Request.Files.Count);
            foreach (var fileKey in HttpContext.Request.Files.AllKeys)
            {
                var file = HttpContext.Request.Files[fileKey];
                var path = Path.Combine(folderPath, file.FileName);
                if (System.IO.File.Exists(path))
                {
                    // если файл существуем то ресовим его, иначе удаляем
                    if (resolveFileName)
                    {
                        path = Path.Combine(folderPath, GetResolvingFileName(folderPath, file.FileName));
                    }
                    else
                    {
                        System.IO.File.SetAttributes(path, FileAttributes.Normal);
                        System.IO.File.Delete(path);
                    }
                }

                file.SaveAs(path);
                allFileNames.Add(Path.GetFileName(path));
            }

            return Json(new { fileNames = allFileNames.ToArray(), proceed = true }, "text/plain");
        }

        private static bool CheckFolderExistence(string path)
        {
            var result = true;
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch
                {
                    result = false;
                }
            }

            return result;
        }
    }
}
