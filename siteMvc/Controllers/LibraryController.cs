using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using QP8.Infrastructure.Web.Extensions;
using Quantumart.QP8.BLL;
using Quantumart.QP8.BLL.Helpers;
using Quantumart.QP8.BLL.Services;
using Quantumart.QP8.Constants;
using Quantumart.QP8.Constants.Mvc;
using Quantumart.QP8.Resources;
using Quantumart.QP8.Utils;
using Quantumart.QP8.WebMvc.Extensions.Controllers;
using Quantumart.QP8.WebMvc.ViewModels.Library;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using Quantumart.QP8.BLL.Repository;
using Quantumart.QP8.BLL.Services.DbServices;
using Quantumart.QP8.Configuration;

namespace Quantumart.QP8.WebMvc.Controllers
{
    public class LibraryController : AuthQpController
    {
        private const string ContentDispositionTemplate = "attachment; filename=\"{0}\"; filename*=UTF-8''{0}";
        private const int DefaultSvgWidth = 800;
        private const int DefaultSvgHeight = 600;

        private readonly ILibraryService _libraryService;

        private readonly IDbService _dbService;
        private readonly PathHelper _pathHelper;

        public LibraryController(
            ILibraryService libraryService,
            IDbService dbService,
            PathHelper pathHelper
        )
        {
            _libraryService = libraryService;
            _dbService = dbService;
            _pathHelper = pathHelper;
        }

        private class FilePropertiesOptions
        {
            public bool IsVersion { get; set; }

            public string NotSupportedMessage { get; set; } = LibraryStrings.OperationIsNotSupported;
        }

        public JsonResult FullNameFileExists(string name) => Json(new { result = name != null && _pathHelper.FileExists(name) });

        public JsonResult FileExists(string path, string name) => Json(new
        {
            result = name != null && _pathHelper.FileExists(_pathHelper.CombinePath(path, name))
        });

        public JsonResult ResolveFileName(string path, string name)
        {
            var result = GetResolvingFileName(path, name);

            return Json(new { result });
        }

        public JsonResult TestFieldValueDownload(string id, string fileName, bool isVersion, int? entityId)
        {
            var fieldId = Field.ParseFormName(id);
            if (fieldId == 0)
            {
                return Json(new { proceed = false, msg = string.Format(LibraryStrings.InvalidId, id) });
            }

            var info = GetFilePathInfo(fieldId, entityId, isVersion);
            info.PathHelper = _pathHelper;
            return GetTestFileDownloadResult(info, HttpUtility.UrlDecode(fileName), isVersion);
        }

        public JsonResult ExportFileDownload(string fileName)
        {
            var folderForUpload = $@"{PathHelper.GetUploadPath()}{Path.DirectorySeparatorChar}";
            var inf = new PathInfo
            {
                Path = folderForUpload
            };

            return GetTestFileDownloadResult(inf, HttpUtility.UrlDecode(fileName), false);
        }

        public JsonResult TestLibraryFileDownload(int id, string fileName, string entityTypeCode)
        {
            var pathInfo = entityTypeCode == EntityTypeCode.ContentFile ? ContentFolderService.GetPathInfo(id) : SiteFolderService.GetPathInfo(id);
            if (pathInfo == null)
            {
                var formatString = entityTypeCode == EntityTypeCode.ContentFile ? LibraryStrings.ContentFolderNotExists : LibraryStrings.SiteFolderNotExists;
                return Json(new { proceed = false, msg = string.Format(formatString, id) });
            }

            pathInfo.PathHelper = _pathHelper;
            return GetTestFileDownloadResult(pathInfo, HttpUtility.UrlDecode(fileName), false);
        }

        public JsonResult GetImageProperties(string id, string fileName, bool isVersion, int? entityId, int? parentEntityId)
        {
            var fieldId = Field.ParseFormName(id);
            if (fieldId == 0)
            {
                return Json(new { proceed = false, msg = string.Format(LibraryStrings.InvalidId, id) });
            }

            var pathInfo = GetFilePathInfo(fieldId, entityId, isVersion);
            return GetFileProperties(pathInfo, HttpUtility.UrlDecode(fileName), new FilePropertiesOptions { IsVersion = isVersion });
        }

        public JsonResult GetLibraryImageProperties(int id, string fileName, string entityTypeCode)
        {
            var pathInfo = entityTypeCode == EntityTypeCode.ContentFile ? ContentFolderService.GetPathInfo(id) : SiteFolderService.GetPathInfo(id);
            pathInfo.PathHelper = _pathHelper;
            return GetFileProperties(pathInfo, HttpUtility.UrlDecode(fileName), new FilePropertiesOptions());
        }

        public ActionResult DownloadFile(string id, string fileName)
        {
            var path = (string)TempData[id];

            if (!string.IsNullOrWhiteSpace(path))
            {
                if (_dbService.UseS3() && !path.StartsWith(QPConfiguration.TempDirectory))
                {
                    var stream = _pathHelper.GetS3Stream(path);
                    return File(stream, MimeTypes.OctetStream, HttpUtility.UrlDecode(fileName));
                }
                else
                {
                    var dir = Path.GetDirectoryName(path);
                    var file = Path.GetFileName(path);
                    var readStream = new PhysicalFileProvider(dir).GetFileInfo(file).CreateReadStream();
                    return File(readStream, MimeTypes.OctetStream, HttpUtility.UrlDecode(fileName));
                }
            }

            return Json(null);
        }

        [HttpPost]
        public JsonResult CheckForCrop([FromBody] CropViewModel model)
        {
            var path = _pathHelper.FixPathSeparator(PathInfo.ConvertToPath(model.TargetFileUrl));
            var ext = Path.GetExtension(path);
            if (_pathHelper.FileExists(path))
            {
                return Json(new { ok = false, message = string.Format(LibraryStrings.FileExistsTryAnother, path) });
            }

            if (string.IsNullOrWhiteSpace(ext) || string.IsNullOrWhiteSpace(GetMimeType(ext)))
            {
                return Json(new { ok = false, message = string.Format(LibraryStrings.ExtensionIsNotAllowed, ext) });
            }

            if (!PathInfo.CheckSecurity(path, true, _pathHelper.Separator).Result)
            {
                return Json(new { ok = false, message = string.Format(LibraryStrings.AccessDenied, path, QPContext.CurrentUserName) });
            }

            return Json(new { ok = true, message = string.Empty });
        }

        [HttpPost]
        public JsonResult Crop([FromBody] CropViewModel model)
        {
            var sourcePath = _pathHelper.FixPathSeparator(PathInfo.ConvertToPath(model.SourceFileUrl));
            var targetPath = model.OverwriteFile ? sourcePath : _pathHelper.FixPathSeparator(PathInfo.ConvertToPath(model.TargetFileUrl));
            if (!PathInfo.CheckSecurity(targetPath, true, _pathHelper.Separator).Result)
            {
                return Json(new { ok = false, message = LibraryStrings.AccessDenied });
            }

            int sourceWidth = 0, sourceHeight = 0, sourceTop = 0, sourceLeft = 0;
            try
            {
                var size = GetImageSize(sourcePath);
                sourceWidth = size.Item1;
                sourceHeight = size.Item2;
            }
            catch (Exception)
            {
                return Json(new { ok = false, message = LibraryStrings.ExtensionIsNotAllowed });
            }

            var targetWidth = model.Width ?? (int)(sourceWidth * model.Resize);
            var targetHeight = model.Height ?? (int)(sourceHeight *  model.Resize);

            sourceWidth = model.Width.HasValue ? (int)(model.Width.Value / model.Resize) : sourceWidth;
            sourceHeight = model.Height.HasValue ? (int)(model.Height.Value / model.Resize) : sourceHeight;
            sourceTop = model.Top.HasValue ? (int)(model.Top.Value / model.Resize) : sourceTop;
            sourceLeft = model.Left.HasValue ? (int)(model.Left.Value / model.Resize) : sourceLeft;

            if (_pathHelper.FileExists(sourcePath))
            {
                using (var img = _pathHelper.LoadImage(sourcePath))
                {
                    img.Mutate(n => n
                        .Crop(new Rectangle(sourceLeft, sourceTop, sourceWidth, sourceHeight))
                        .Resize(targetWidth, targetHeight)
                    );

                    // ReSharper disable once AssignNullToNotNullAttribute
                    if (!_pathHelper.UseS3)
                    {
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                        if (_pathHelper.FileExists(targetPath))
                        {
                            System.IO.File.SetAttributes(targetPath, FileAttributes.Normal);
                            System.IO.File.Delete(targetPath);
                        }
                    }
                    _pathHelper.SaveImage(img, targetPath);
                }
            }

            return Json(new { ok = true, message = string.Empty });
        }

        [HttpPost]
        public async Task<JsonResult> CheckForAutoResize([FromBody] CheckAutoResizeViewModel resizeParameters)
        {
            var settings = await _libraryService.GetSettingsFromStorage(resizeParameters.BaseUrl);

            if (settings?.ReduceSizes == null || string.IsNullOrWhiteSpace(settings.ResizedImageTemplate) || settings.ExtensionsAllowedToResize.Length == 0)
            {
                return Json(new { ok = false, message = LibraryStrings.AutoResizeSettingsAreIncorrect});
            }

            var sourcePath = resizeParameters.FolderUrl;
            var path = PathInfo.ConvertToPath(sourcePath);

            var ext = Path.GetExtension(resizeParameters.FileName);

            if (string.IsNullOrWhiteSpace(ext) || !settings.ExtensionsAllowedToResize.Contains(ext))
            {
                return Json(new { ok = false, message = string.Format(LibraryStrings.ExtensionIsNotAllowed, ext) });
            }

            if (settings.ReduceSizes.Any(a=> a.ReduceRatio <= 0))
            {
                return Json(new { ok = false, message = string.Format(LibraryStrings.InvalidReduceRatio, ext) });
            }

            if (!PathInfo.CheckSecurity(path, true, _pathHelper.Separator).Result)
            {
                return Json(new { ok = false, message = string.Format(LibraryStrings.AccessDenied, path, QPContext.CurrentUserName) });
            }

            var filename = Path.GetFileNameWithoutExtension(resizeParameters.FileName);
            var basePath = Path.GetDirectoryName(path);
            var filenames = settings.ReduceSizes.Select(s => _pathHelper.CombinePath(basePath, string.Format(settings.ResizedImageTemplate, filename, s.Postfix, ext)));

            foreach (var file in filenames)
            {
                if (_pathHelper.FileExists(file))
                {
                    return Json(new { ok = false, message = string.Format(LibraryStrings.AutoResizedFileExists, file) });
                }
            }

            return Json(new { ok = true, message = string.Empty, reduceSizes = settings.ReduceSizes, resizedImageTemplate = settings.ResizedImageTemplate });
        }

        [HttpPost]
        public JsonResult AutoResize([FromBody] AutoResizeViewModel resizeParameters)
        {
            var sourcePath = resizeParameters.FolderUrl;
            var path = PathInfo.ConvertToPath(sourcePath);

            var ext = Path.GetExtension(resizeParameters.FileName);

            var filename = Path.GetFileNameWithoutExtension(resizeParameters.FileName);
            var basePath = Path.GetDirectoryName(path);

            foreach (var size in resizeParameters.ReduceSizes)
            {
                var newFile = _pathHelper.CombinePath(basePath, string.Format(resizeParameters.ResizedImageTemplate, filename, size.Postfix, ext));
                using var image = _pathHelper.LoadImage(_pathHelper.CombinePath(path, resizeParameters.FileName));
                var width = (int)(image.Width / size.ReduceRatio);
                var height = (int)(image.Height / size.ReduceRatio);
                image.Mutate(x => x.Resize(width, height));
                _pathHelper.SaveImage(image, newFile);
            }

            return Json(new { ok = true, message = string.Empty });
        }


        private static string GetMimeType(string extension)
        {
            if (extension.Equals(".jpg", StringComparison.InvariantCultureIgnoreCase) || extension.Equals(".jpeg", StringComparison.InvariantCultureIgnoreCase))
            {
                return MimeTypes.Jpeg;
            }

            if (extension.Equals(".gif", StringComparison.InvariantCultureIgnoreCase))
            {
                return MimeTypes.Gif;
            }

            if (extension.Equals(".png", StringComparison.InvariantCultureIgnoreCase))
            {
                return MimeTypes.Png;
            }

            if (extension.Equals(".webp", StringComparison.InvariantCultureIgnoreCase))
            {
                return MimeTypes.Webp;
            }

            return string.Empty;
        }

        private string GetResolvingFileName(string path, string name)
        {
            string result;
            var index = 0;
            do
            {
                index++;
                result = MutateHelper.MutateFileName(name, index);
            } while (_pathHelper.FileExists(_pathHelper.CombinePath(path, result)));

            return result;
        }

        private JsonResult GetTestFileDownloadResult(PathInfo info, string fileName, bool isVersion)
        {
            if (!PathInfo.CheckSecurity(info.FixedPath, false, _pathHelper.Separator).Result)
            {
                return Json(new { proceed = false, msg = string.Format(LibraryStrings.AccessDenied, info.Path, QPContext.CurrentUserName) });
            }
            var normalizedFileName = isVersion ? Path.GetFileName(HttpUtility.UrlDecode(fileName)) : HttpUtility.UrlDecode(fileName);
            var path = info.GetPath(normalizedFileName);
            return _pathHelper.FileExists(path)
                ? Json(new { proceed = true, key = SavePath(path) })
                : Json(new { proceed = false, msg = string.Format(LibraryStrings.NotExists, normalizedFileName) });
        }

        private string SavePath(string path)
        {
            using (var rnd = RandomNumberGenerator.Create())
            {
                var key = rnd.Next().ToString();
                TempData[key] = path;
                return key;
            }
        }

        private static PathInfo GetFilePathInfo(int fieldId, int? entityId, bool isVersion) => !isVersion
            ? FieldService.GetPathInfo(fieldId, entityId)
            : ArticleVersionService.GetPathInfo(fieldId, (int)entityId);

        private JsonResult GetFileProperties(PathInfo pathInfo, string fileName, FilePropertiesOptions options)
        {
            var normalizedFileName = options.IsVersion ? Path.GetFileName(HttpUtility.UrlDecode(fileName)) : HttpUtility.UrlDecode(fileName);
            var path = pathInfo.GetPath(normalizedFileName);
            var url = string.Empty;
            var message = string.Empty;
            int width = 0, height = 0;

            if (_pathHelper.FileExists(path))
            {
                url = pathInfo.GetUrl(normalizedFileName);
                try
                {
                    var size = GetImageSize(path);
                    width = size.Item1;
                    height = size.Item2;
                }
                catch (Exception)
                {
                    message = string.Format(options.NotSupportedMessage, normalizedFileName);
                }
            }
            else
            {
                message = string.Format(LibraryStrings.NotExists, normalizedFileName);
            }

            var result = !string.IsNullOrWhiteSpace(message)
                ? (object)new { proceed = false, msg = message }
                : new { proceed = true, url, folderUrl = pathInfo.Url, width, height, baseUrl = pathInfo.BaseUploadUrl };

            return Json(result);
        }

        private Tuple<int, int> GetImageSize(string path)
        {
            if (path.EndsWith(".svg"))
            {
                return new Tuple<int, int>(DefaultSvgWidth, DefaultSvgHeight);
            }

            var img = _pathHelper.IdentifyImage(path);
            return new Tuple<int, int>(img.Width, img.Height);
        }
    }
}
